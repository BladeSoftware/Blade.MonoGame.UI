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
    public class ComboBox : TemplatedControl
    {
        public Type ItemTemplateType { get; set; } = typeof(ListViewItemTemplate);

        // Items source (any IEnumerable of objects)
        public IEnumerable ItemsSource { get; set; }

        // Converts an item to display string. Default uses ToString().
        public Func<object, string> ItemToString { get; set; } = (o) => o?.ToString() ?? "";

        // When true the control allows text entry to filter items
        public Binding<bool> IsEditable { get; set; } = new Binding<bool>(false);

        // When true the control enforces selection of an existing item
        public Binding<bool> StrictMode { get; set; } = new Binding<bool>(true);

        // Currently selected item
        public Binding<object> SelectedItem { get; set; } = new Binding<object>(null);

        public Binding<Length> DropDownHeight { get; set; } = new Binding<Length>("250px");

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
            int y = controlRect.Bottom; // open below control
            int width = controlRect.Width;

            // Compute pixel height from Length
            int height;
            try
            {
                height = (int)DropDownHeight.Value.ToPixelsHeight(this, pw.FinalRect);
                if (height <= 0) height = 200;
            }
            catch
            {
                height = 200;
            }

            // Compute margins so that UIWindow.PerformLayout places the window at (x,y) with given width/height
            int rightMargin = Math.Max(0, parentBounds.Width - x - width);
            int bottomMargin = Math.Max(0, parentBounds.Height - y - height);

            // Create popup window
            var popup = new DropdownWindow(this)
            {
                Name = $"{Name}_Dropdown",
                DefaultZIndex = UIManager.MaxZIndex,
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
                Background = new Color(Color.LightGray, 1f),
            };

            listView.DataContext = GetFilteredItems().Cast<object>().ToList();

            Border border = new Border()
            {
                BorderColor = Color.DarkGray,
                BorderThickness = 2,
                CornerRadius = 4,
                Background = new Color(Color.LightGray, 1f),
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

        // Update dropdown contents while open
        private void RefreshPopupItems()
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

            // If we clicked on the header, toggle dropdown
            if (tmpl != null)
            {
                if (tmpl.HeaderRect.Contains(uiEvent.X, uiEvent.Y))
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
            }

            // Otherwise let base handle (keyboard focus, etc.)
            await base.HandleMouseClickEventAsync(uiWindow, uiEvent);
        }

        // When focus lost and editable: enforce strict mode if requested and close any popup
        public override async Task HandleFocusChangedEventAsync(UIWindow uiWindow, UIFocusChangedEvent uiEvent)
        {
            await base.HandleFocusChangedEventAsync(uiWindow, uiEvent);

            if (!uiEvent.Focused)
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
        }

        // Allow typing when embedded TextBox has focus - template's TextBox is bound to combo.Text
        public override async Task HandleKeyPressAsync(UIWindow uiWindow, UIKeyEvent uiEvent)
        {
            await base.HandleKeyPressAsync(uiWindow, uiEvent);

            // If user presses Escape close dropdown
            if (uiEvent.Key == Keys.Escape && dropdownWindow != null)
            {
                CloseDropDown();
                uiEvent.Handled = true;
                return;
            }

            // If Enter pressed and editable, try to resolve to an item
            if (uiEvent.Key == Keys.Enter && IsEditable.Value)
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

                // If click wasn't handled by children (click outside list), close dropdown
                if (!uiEvent.Handled)
                {
                    parentCombo.CloseDropDown();
                    uiEvent.Handled = true;
                }
            }
        }
    }

}