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

        // Text shown in the editable textbox (or selected item text when not editable)
        private Binding<string> text = new Binding<string>("");
        public Binding<string> Text
        {
            get => text;
            set => SetField(ref text, value);
        }

        // Whether dropdown is open
        public bool IsDropDownOpen { get; set; } = false;

        // Callback when selection changes
        public Action<object> OnSelectionChanged { get; set; }

        // Internal filtered items used when IsEditable = true
        private List<object> filteredItems = new List<object>();

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
            Content = new ComboBoxTemplate();

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

        // When clicked, toggle dropdown open/close OR if clicking a list item propagate selection (ListView already sets its own SelectedItem)
        public override async Task HandleMouseClickEventAsync(UIWindow uiWindow, UIClickEvent uiEvent)
        {
            // Acquire template
            var tmpl = Content as ComboBoxTemplate;

            // If we clicked on the drop button / header, toggle dropdown
            if (tmpl != null)
            {
                if (tmpl.HeaderRect.Contains(uiEvent.X, uiEvent.Y))
                {
                    IsDropDownOpen = !IsDropDownOpen;
                    StateHasChanged();
                    uiEvent.Handled = true;
                    return;
                }
            }

            // Let base propagate. If dropdown open and a list item was clicked, ListView will set its SelectedItem.
            await base.HandleMouseClickEventAsync(uiWindow, uiEvent);

            // After base propagation, if template exists pick up ListView selected item and update SelectedItem
            if (tmpl != null && IsDropDownOpen)
            {
                var lv = tmpl.ListView;
                if (lv != null && lv.SelectedItem != null)
                {
                    SelectedItem.Value = lv.SelectedItem;
                    Text.Value = ItemToString(SelectedItem.Value);
                    IsDropDownOpen = false;

                    OnSelectionChanged?.Invoke(SelectedItem.Value);
                    StateHasChanged();
                    uiEvent.Handled = true;
                    return;
                }
            }
        }

        // When focus lost and editable: enforce strict mode if requested
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

                IsDropDownOpen = false;
                StateHasChanged();
            }
        }

        // Allow typing when embedded TextBox has focus - template's TextBox is bound to combo.Text
        public override async Task HandleKeyPressAsync(UIWindow uiWindow, UIKeyEvent uiEvent)
        {
            await base.HandleKeyPressAsync(uiWindow, uiEvent);

            // If user presses Escape close dropdown
            if (uiEvent.Key == Keys.Escape && IsDropDownOpen)
            {
                IsDropDownOpen = false;
                StateHasChanged();
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

                IsDropDownOpen = false;
                StateHasChanged();
                uiEvent.Handled = true;
            }
        }
    }
}