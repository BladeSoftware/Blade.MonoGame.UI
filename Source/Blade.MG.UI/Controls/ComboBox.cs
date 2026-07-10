using Blade.MG.UI.Components;
using Blade.MG.UI.Controls.Templates;
using Blade.MG.UI.Events;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blade.MG.UI.Controls
{
    // Controls when the dropdown opens while IsEditable - either only via the header's
    // DropDownButton, or automatically as soon as the user starts typing (so the filtered
    // items are visible live). Only relevant when IsEditable is true; in simple (non-editable)
    // mode any header click - button or elsewhere - always opens the dropdown.
    public enum ComboBoxOpenTrigger
    {
        ButtonOnly,
        AutoOpenOnType
    }

    public class ComboBox : TemplatedControl
    {
        public Type ItemTemplateType { get; set; } = typeof(ListViewItemTemplate);

        // Items source (any IEnumerable of objects)
        public IEnumerable ItemsSource { get; set; }

        // Converts an item to display string. Default uses ToString().
        public Func<object, string> ItemToString { get; set; } = (o) => o?.ToString() ?? "";

        // When true the control allows text entry to filter items
        public Binding<bool> IsEditable { get; set; } = new Binding<bool>(false);

        // See ComboBoxOpenTrigger. Defaults to auto-opening on the first typed character.
        public Binding<ComboBoxOpenTrigger> OpenTrigger { get; set; } = new Binding<ComboBoxOpenTrigger>(ComboBoxOpenTrigger.AutoOpenOnType);

        // When true the control enforces selection of an existing item
        public Binding<bool> StrictMode { get; set; } = new Binding<bool>(true);

        // Currently selected item
        public Binding<object> SelectedItem { get; set; } = new Binding<object>(null);

        public Binding<Length> DropDownHeight { get; set; } = new Binding<Length>("250px");

        private Binding<Color> textColor = new Binding<Color>();
        public Binding<Color> TextColor { get => textColor; set => SetField(ref textColor, value); }

        private Binding<Color> borderColor = new Binding<Color>();
        public Binding<Color> BorderColor { get => borderColor; set => SetField(ref borderColor, value); }

        // Text shown in the editable textbox (or selected item text when not editable)
        private Binding<string> text = new Binding<string>("");
        public Binding<string> Text
        {
            get => text;
            set => SetField(ref text, value);
        }

        // Whether dropdown is open (kept for compatibility, main state is the popup window)
        public bool IsDropDownOpen { get => dropdownWindow != null; set { if (value) OpenDropDown(); else CloseDropDown(); } }

        // Callback when selection changes
        public Action<object> OnSelectionChanged { get; set; }

        // Internal filtered items used when IsEditable = true
        private List<object> filteredItems = new List<object>();

        // Popup window / list used for dropdown
        private UIWindow dropdownWindow = null;
        private ListView dropdownListView = null;

        // The same physical click that opens the dropdown (header click, or a synthesized
        // activation - see UIManager.GamePad.cs/UIManager.Touch.cs, which each dispatch their
        // own independent HandleMouseClickEventAsync broadcast alongside the mouse one for the
        // same input) can end up reaching DropdownWindow.HandleMouseClickEventAsync's
        // click-outside-closes check a second time in the very same frame, immediately closing
        // what just opened. Recorded here and checked there to ignore that immediate re-close.
        private DateTime dropdownOpenedAt = DateTime.MinValue;

        public ComboBox()
        {
            TemplateType = typeof(ComboBoxTemplate);
            IsTabStop = true;
            IsHitTestVisible = true;
        }

        protected override void InitTemplate()
        {
            base.InitTemplate();

            // Ensure template is created
            Content = Activator.CreateInstance(TemplateType) as UIComponent;

            // Initialize text from selected item if present
            if (SelectedItem.Value != null)
            {
                Text.Value = ItemToString(SelectedItem.Value);
            }
        }

        // Recompute the filtered items based on Text and ItemsSource
        public IEnumerable<object> GetFilteredItems()
        {
            filteredItems.Clear();

            if (ItemsSource == null)
            {
                return filteredItems;
            }

            // If not editable, simply return ItemsSource cast
            if (!IsEditable.Value)
            {
                foreach (var obj in ItemsSource)
                {
                    filteredItems.Add(obj);
                }
                return filteredItems;
            }

            string q = Text.Value?.Trim() ?? "";
            if (string.IsNullOrEmpty(q))
            {
                foreach (var obj in ItemsSource)
                {
                    filteredItems.Add(obj);
                }
                return filteredItems;
            }

            string qLower = q.ToLowerInvariant();

            foreach (var obj in ItemsSource)
            {
                var s = ItemToString(obj) ?? "";
                if (s.ToLowerInvariant().Contains(qLower))
                {
                    filteredItems.Add(obj);
                }
            }

            return filteredItems;
        }

        // Helper to find first item that exactly matches text (case-insensitive)
        private object FindItemByText(string t)
        {
            if (ItemsSource == null) return null;
            string tt = (t ?? "").Trim();
            foreach (var obj in ItemsSource)
            {
                if (string.Equals(ItemToString(obj) ?? "", tt, StringComparison.OrdinalIgnoreCase))
                {
                    return obj;
                }
            }
            return null;
        }

        // --- Popup management ---

        private void OpenDropDown()
        {
            if (dropdownWindow != null) return;
            if (ParentWindow == null) return;

            var pw = ParentWindow;
            var uiManager = pw.Game.Services.GetService<UIManager>();
            if (uiManager == null) return;

            // Compute desired popup rectangle in parent window (screen) coordinates
            Rectangle controlRect = this.FinalRect; // coordinates relative to parent window layout
            Rectangle parentBounds = pw.FinalContentRect; // full backbuffer

            int x = controlRect.Left;
            int width = controlRect.Width;

            // Compute pixel height from Length
            int desiredHeight;
            try
            {
                desiredHeight = (int)DropDownHeight.Value.ToPixelsHeight(this, pw.FinalRect);
                if (desiredHeight <= 0) desiredHeight = 200;
            }
            catch
            {
                desiredHeight = 200;
            }

            // Prefer opening below the control, but flip above it when there isn't enough
            // room below and more room is available above - and either way clamp the popup's
            // height to whichever side it ends up on so it never runs past the edge of the
            // window (e.g. a combo box near the bottom of a short/resized window).
            int spaceBelow = Math.Max(0, parentBounds.Bottom - controlRect.Bottom);
            int spaceAbove = Math.Max(0, controlRect.Top - parentBounds.Top);

            int y;
            int height;
            if (desiredHeight <= spaceBelow || spaceBelow >= spaceAbove)
            {
                y = controlRect.Bottom;
                height = Math.Min(desiredHeight, spaceBelow);
            }
            else
            {
                height = Math.Min(desiredHeight, spaceAbove);
                y = controlRect.Top - height;
            }

            // Compute margins so that UIWindow.PerformLayout places the window at (x,y) with given width/height
            int rightMargin = Math.Max(0, parentBounds.Width - x - width);
            int bottomMargin = Math.Max(0, parentBounds.Height - y - height);

            // Create popup window
            var popup = new DropdownWindow(this)
            {
                Name = $"{Name}_Dropdown",
                DefaultZIndex = UIManager.MaxZIndex,
                IsModal = true,
                Visible = { Value = Visibility.Visible }
            };

            // Position the popup by setting Margin and fixed size on window
            popup.Margin = new Binding<Thickness>(new Thickness(x, y, rightMargin, bottomMargin));
            popup.Width = width;
            popup.Height = height;

            // Create the ListView for dropdown and initialize
            var listView = new ListView()
            {
                ItemTemplateType = ItemTemplateType,
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                Background = Theme.Surface,

                // Arrow-key browsing should move a highlight without closing the popup on
                // every keypress (OnSelectionChanged below closes it) - only Enter/Space or a
                // mouse click should commit and close. See ListView.CommitSelectionImmediately.
                CommitSelectionImmediately = false,

                // ListView.HandleKeyPressAsync gates its own Up/Down/Enter/Home/End handling on
                // HasFocus.Value, since key events otherwise propagate unconditionally to every
                // component in the tree (see its own comment). Real UI focus stays on the combo
                // box/EditBox on the main window the whole time the dropdown is open - this
                // popup ListView is never the UIWindow-tracked focused component (see
                // DropdownWindow.HandleKeyPressAsync, which forwards unhandled keys back to
                // ComboBox.HandleKeyPressAsync, which in turn calls this ListView's
                // HandleKeyPressAsync directly rather than through the normal focus-dispatch
                // path). Without this, that gate would silently block every Up/Down/Enter press
                // from ever navigating the list.
                HasFocus = true
            };

            listView.DataContext = GetFilteredItems().Cast<object>().ToList();

            // Wire up selection changed callback
            listView.OnSelectionChanged = (selected) =>
            {
                if (selected != null)
                {
                    SelectedItem.Value = selected;
                    Text.Value = ItemToString(SelectedItem.Value);
                    OnSelectionChanged?.Invoke(SelectedItem.Value);
                    CloseDropDown();
                }
            };

            Border border = new Border()
            {
                BorderColor = Theme.Outline,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Elevation = 4,
                Background = Theme.Surface,
                Padding = new Thickness(2),
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                Content = listView
            };

            // Add list view to the popup (as its only child)
            popup.RemoveAllChildren();
            popup.AddChild(border);

            // Keep references
            dropdownWindow = popup;
            dropdownListView = listView;
            dropdownOpenedAt = DateTime.Now;

            // Add to UIManager so it renders above everything
            uiManager.Add(popup, UIManager.MaxZIndex);
        }

        private void CloseDropDown()
        {
            if (dropdownWindow == null) return;
            var pw = ParentWindow;
            var uiManager = pw?.Game.Services.GetService<UIManager>();
            if (uiManager != null)
            {
                uiManager.Remove((UIWindow)dropdownWindow);
            }
            dropdownWindow = null;
            dropdownListView = null;

            // Ensure main control re-renders to reflect closed state
            StateHasChanged();
        }

        // Update dropdown contents while open. Internal so ComboBoxTemplate.Arrange can call it
        // during the layout phase (see there for why) as well as RenderControl below.
        internal void RefreshPopupItems()
        {
            if (dropdownListView != null)
            {
                dropdownListView.DataContext = GetFilteredItems().Cast<object>().ToList();
                dropdownListView.StateHasChanged();
            }
        }

        // When clicked, toggle dropdown open/close (header) OR if clicking a list item propagate selection (handled by popup)
        public override async Task HandleMouseClickEventAsync(UIWindow uiWindow, UIClickEvent uiEvent)
        {
            // Acquire template
            var tmpl = Content as ComboBoxTemplate;

            if (tmpl != null)
            {
                bool onButton = tmpl.DropDownButton != null && tmpl.DropDownButton.FinalRect.Contains(uiEvent.X, uiEvent.Y);

                // Toggle on any header click, but only in simple (non-editable) mode - clicking
                // the header while editable should only focus the text entry, not toggle.
                // Excludes DropDownButton itself, which handles its own toggle via OnPrimaryClick
                // (see ComboBoxTemplate.InitTemplate) so it keeps working the same way via
                // keyboard/gamepad/touch activation (Part 1) regardless of IsEditable.
                if (!onButton && !IsEditable.Value && tmpl.HeaderRect.Contains(uiEvent.X, uiEvent.Y))
                {
                    if (dropdownWindow == null)
                    {
                        OpenDropDown();
                    }
                    else
                    {
                        CloseDropDown();
                    }

                    StateHasChanged();

                    uiEvent.Handled = true;

                    return;
                }

                // Editable mode: explicitly focus the embedded EditBox on any header click
                // (including directly on the box itself), rather than relying on the ambient
                // mouse-down focus search in UIManager.Mouse.cs to land on it for free. EditBox
                // is several levels deep in the template's visual tree, and TextBox's own
                // mouse-down handling (drag-select) locks input to itself via
                // LockEventsToControl - between the two, the automatic hit-test based focus
                // search does not reliably resolve to EditBox when clicked directly. Doing it
                // here, after the click has already resolved, guarantees the right control ends
                // up focused regardless of what the automatic search picked.
                if (!onButton && IsEditable.Value && tmpl.EditBox != null && tmpl.HeaderRect.Contains(uiEvent.X, uiEvent.Y))
                {
                    await uiWindow.SetFocusAsync(tmpl.EditBox);

                    // Re-open on a click while EditBox is already focused (e.g. the dropdown was
                    // just closed by a previous click - see DropdownWindow.HandleMouseClickEventAsync).
                    // The dropdown's own open-on-focus-gained logic (ComboBoxTemplate.Arrange)
                    // only fires on the transition to focused, which already happened, so it
                    // won't fire again here on its own.
                    if (dropdownWindow == null && OpenTrigger.Value == ComboBoxOpenTrigger.AutoOpenOnType)
                    {
                        OpenDropDown();
                    }
                }
            }

            // Otherwise let base handle it - propagates to children (e.g. DropDownButton's own
            // click) and grants keyboard focus, etc.
            await base.HandleMouseClickEventAsync(uiWindow, uiEvent);
        }

        // When focus lost and editable: enforce strict mode if requested and close any popup
        public override async Task HandleFocusChangedEventAsync(UIWindow uiWindow, UIFocusChangedEvent uiEvent)
        {
            await base.HandleFocusChangedEventAsync(uiWindow, uiEvent);

            if (!uiEvent.Focused)
            {
                HandleFocusLost();
            }
        }

        // Shared "lost focus" logic (strict-mode text enforcement + closing the popup). In
        // simple mode the ComboBox itself is the focused component, so
        // HandleFocusChangedEventAsync above fires normally when focus moves elsewhere. In
        // editable mode the embedded EditBox holds focus directly instead (see
        // IsEditBoxFocused) - UIWindow.RaiseFocusChangedEventAsync only calls
        // HandleFocusChangedEventAsync on the exact component that lost focus, so this
        // ComboBox's own override above never runs when EditBox is what loses focus. Called
        // from there instead, via ComboBoxTemplate.Arrange's per-frame EditBox.HasFocus
        // tracking (which already watches the opposite edge to auto-open the dropdown).
        internal void HandleFocusLost()
        {
            // losing focus: if editable and strict mode, ensure text corresponds to an item
            if (IsEditable.Value)
            {
                var match = FindItemByText(Text.Value);
                if (match != null)
                {
                    SelectedItem.Value = match;
                }
                else
                {
                    if (StrictMode.Value)
                    {
                        // revert to selected item text or clear
                        Text.Value = SelectedItem.Value != null ? ItemToString(SelectedItem.Value) : "";
                    }
                    else
                    {
                        SelectedItem.Value = null;
                    }
                }
            }

            CloseDropDown();
        }

        // In editable mode the embedded EditBox, not the ComboBox itself, holds focus - need
        // both checked when deciding whether keyboard nav below applies to this combo.
        private bool IsEditBoxFocused => (Content as ComboBoxTemplate)?.EditBox?.HasFocus.Value ?? false;

        // Allow typing when embedded TextBox has focus - template's TextBox is bound to combo.Text
        public override async Task HandleKeyPressAsync(UIWindow uiWindow, UIKeyEvent uiEvent)
        {
            // If user presses Escape close dropdown
            if (uiEvent.Key == Keys.Escape && dropdownWindow != null)
            {
                CloseDropDown();
                uiEvent.Handled = true;
                return;
            }

            // Space must stay literal typed input while editable - only treat it as an
            // activation/navigation key in simple (non-editable) mode.
            bool spaceIsLiteralText = IsEditable.Value;

            // Open the dropdown from the keyboard when it's closed: Down/Enter always; Space
            // only in simple mode (no text entry for it to type into there).
            if (dropdownWindow == null && (HasFocus.Value || IsEditBoxFocused) &&
                (uiEvent.Key == Keys.Down || uiEvent.Key == Keys.Enter || (uiEvent.Key == Keys.Space && !spaceIsLiteralText)))
            {
                OpenDropDown();
                uiEvent.Handled = true;
            }

            // Forward Up/Down/Enter/(Space outside editable mode) to the open popup list -
            // reuses ListView's own navigation and the OnSelectionChanged wiring already set
            // up in OpenDropDown to commit the highlighted item and close. Home/End are
            // deliberately excluded so they keep moving the text caret inside EditBox instead.
            if (!uiEvent.Handled && dropdownWindow != null && dropdownListView != null &&
                (uiEvent.Key == Keys.Up || uiEvent.Key == Keys.Down || uiEvent.Key == Keys.Enter || (uiEvent.Key == Keys.Space && !spaceIsLiteralText)))
            {
                await dropdownListView.HandleKeyPressAsync(uiWindow, uiEvent);
            }

            // If Enter pressed and editable, and the popup didn't already resolve a
            // highlighted item above, fall back to matching the typed text against the items
            if (!uiEvent.Handled && uiEvent.Key == Keys.Enter && IsEditable.Value)
            {
                var match = FindItemByText(Text.Value);
                if (match != null)
                {
                    SelectedItem.Value = match;
                    OnSelectionChanged?.Invoke(SelectedItem.Value);
                }
                else
                {
                    if (StrictMode.Value)
                    {
                        Text.Value = SelectedItem.Value != null ? ItemToString(SelectedItem.Value) : "";
                    }
                    else
                    {
                        SelectedItem.Value = null;
                    }
                }

                CloseDropDown();
                uiEvent.Handled = true;
            }

            // If dropdown is open and editable, typing should update the displayed items
            if (dropdownWindow != null)
            {
                // Refresh the popup list to reflect new Text filter
                RefreshPopupItems();
                StateHasChanged();
            }


            // Handle keyboard input if this control has focus
            await base.HandleKeyPressAsync(uiWindow, uiEvent);
        }

        override public void RenderControl(UIContext context, Rectangle layoutBounds, Transform parentTransform)
        {
            // If popup open, keep its list updated each frame
            if (dropdownWindow != null)
            {
                RefreshPopupItems();
            }

            base.RenderControl(context, layoutBounds, parentTransform);
        }

        // --- Dropdown window subclass that notifies parent combobox of selection and closes itself ---
        private class DropdownWindow : UIWindow
        {
            private readonly ComboBox parentCombo;
            private readonly ListView dropdownListViewRef;

            public DropdownWindow(ComboBox parent)
            {
                parentCombo = parent;

                // create placeholder listview reference; actual instance set by ComboBox after construction
                dropdownListViewRef = null;
            }

            // Override AddChild so we can capture reference to the ListView when added
            public override void LoadContent()
            {
                base.LoadContent();
            }

            public override void Dispose()
            {
                base.Dispose();
            }

            // While the dropdown is open, this modal popup is the *only* window that receives
            // keyboard input at all (see UIManager.cs's shared DispatchEventAsync - a modal
            // window "owns all input exclusively"). Left alone, that cuts the still-focused
            // EditBox on the main window off from every keystroke - including the literal
            // characters the user is trying to type to filter the list - the moment the
            // dropdown opens. Let the popup's own tree (the ListView's Up/Down/Home/End
            // navigation) handle it first, then forward anything left unhandled back to the
            // combo box on the main window, where it reaches EditBox the normal way.
            public override async Task HandleKeyPressAsync(UIWindow uiWindow, UIKeyEvent uiEvent)
            {
                await base.HandleKeyPressAsync(this, uiEvent);

                if (!uiEvent.Handled && parentCombo.ParentWindow != null)
                {
                    await parentCombo.HandleKeyPressAsync(parentCombo.ParentWindow, uiEvent);
                }
            }

            public override async Task HandleMouseClickEventAsync(UIWindow uiWindow, UIClickEvent uiEvent)
            {
                // Let normal handling run first (ListView should set its SelectedItem)
                await base.HandleMouseClickEventAsync(this, uiEvent);

                // Find listview child if we don't have it
                var lvChild = Children.OfType<ListView>().FirstOrDefault();
                if (lvChild != null)
                {
                    // If a selection was made inside the listview, capture it
                    if (lvChild.SelectedItem != null)
                    {
                        parentCombo.SelectedItem.Value = lvChild.SelectedItem;
                        parentCombo.Text.Value = parentCombo.ItemToString(parentCombo.SelectedItem.Value);
                        parentCombo.OnSelectionChanged?.Invoke(parentCombo.SelectedItem.Value);

                        // Close popup
                        parentCombo.CloseDropDown();

                        uiEvent.Handled = true;
                        return;
                    }
                }

                // If click wasn't handled by children (click outside list), close dropdown -
                // unless this is the same physical click that just opened it, arriving here a
                // second time via an independent input dispatch (see dropdownOpenedAt).
                if (!uiEvent.Handled && (DateTime.Now - parentCombo.dropdownOpenedAt).TotalMilliseconds >= 150)
                {
                    parentCombo.CloseDropDown();
                    uiEvent.Handled = true;
                }
            }
        }
    }

}