using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Blade.MG.UI.Controls
{
    public class PropertyEditor : Panel
    {
        private StackPanel stackPanel;
        private Grid grid;
        private TextBox searchBox;
        private object targetObject;
        private List<PropertyInfo> properties = new();

        // TextBox has no change-notification event to hook (its Text is a plain Binding<string>
        // with no subscribe mechanism), so the search filter is applied by diffing text once per
        // frame in Arrange, the same frame-to-frame diffing technique ComboBoxTemplate.Arrange
        // already uses to detect its own EditBox's focus transitions. Property value editors
        // (CreateTextEditor) commit on focus-lost instead of live per-keystroke, so they don't
        // need this polling - see CreateTextEditor's own comment.
        private string lastFilterText = "";

        public object TargetObject
        {
            get => targetObject;
            set
            {
                targetObject = value;

                if (grid != null)
                {
                    RefreshProperties();
                }
            }
        }

        // When set, the inspector also shows the target's Grid-attached placement (Column/Row/
        // ColumnSpan/RowSpan) - state that lives on the parent Grid, not as a property on the
        // target itself (see Grid.GetColumn/SetColumn etc.), so it can't be reached by the
        // reflected-properties walk below. Left null for a target editor with no Grid parent.
        public Grid GridParent { get; set; }

        public PropertyEditor()
        {
        }

        protected override void InitTemplate()
        {
            base.InitTemplate();


            RemoveAllChildren();


            stackPanel = new StackPanel
            {
                Name = "PropertyEditor_StackPanel",
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,

                // Hidden meant a long property list (more rows than fit the panel's own
                // height) had no way to scroll down to the rest - Auto shows a scrollbar
                // only when the content actually overflows.
                HorizontalScrollBarVisible = ScrollBarVisibility.Hidden,
                VerticalScrollBarVisible = ScrollBarVisibility.Auto,


                //Width = 600,+-*
                //Height = 400,
                StretchLastChild = false
            };

            AddChild(stackPanel);

            // Search box at the top
            searchBox = new TextBox
            {
                HelperText = "Search properties...",
                Margin = new Thickness(0, 0, 0, 8),
                Height = 30,
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
            };
            stackPanel.AddChild(searchBox);


            grid = new Grid
            {
                Name = "PropertyEditor_Grid",
                Margin = new Thickness(4),
                Padding = new Thickness(4),
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Top,

                //Background = Color.HotPink,

            };

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1f) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1f) });

            // Testing
            //grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Pixel, 30) });
            //grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Pixel, 30) });
            //grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Pixel, 30) });

            stackPanel.AddChild(grid);


            //grid.AddChild(new Panel
            //{
            //    Background = Color.AliceBlue,
            //}, 0, 0);

            //grid.AddChild(new Panel
            //{
            //    Background = Color.CornflowerBlue,
            //}, 1, 0);

            //grid.AddChild(new Panel
            //{
            //    Background = Color.YellowGreen,
            //}, 0, 1);

            //grid.AddChild(new Panel
            //{
            //    Background = Color.Orange,
            //}, 1, 1);



            // Initial refresh
            RefreshProperties();

        }

        public override void Arrange(UIContext context, Rectangle layoutBounds, Rectangle parentLayoutBounds)
        {
            base.Arrange(context, layoutBounds, parentLayoutBounds);

            string currentFilterText = searchBox?.Text?.Value ?? "";
            if (currentFilterText != lastFilterText)
            {
                lastFilterText = currentFilterText;
                RefreshProperties();
            }
        }

        private void RefreshProperties()
        {
            //grid.Children.Clear();
            grid.RemoveAllChildren();

            if (targetObject == null)
            {
                return;
            }

            string filter = searchBox.Text?.Value.ToLowerInvariant() ?? "";

            properties = targetObject.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite)
                .Where(p => string.IsNullOrEmpty(filter) || p.Name.ToLowerInvariant().Contains(filter))
                .ToList();

            int row = 0;
            grid.RowDefinitions.Clear();

            if (GridParent != null && targetObject is UIComponent gridChild && string.IsNullOrEmpty(filter))
            {
                AddGridPlacementRows(gridChild, ref row);
            }

            foreach (var prop in properties)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Auto) });

                var label = new Label
                {
                    Text = prop.Name,
                    Margin = new Thickness(0, 2)
                };
                grid.AddChild(label, 0, row);

                UIComponent editor = CreateEditor(prop, targetObject);
                grid.AddChild(editor, 1, row);

                row++;
            }
        }

        // Grid.GetColumn/SetColumn etc. are attached state keyed by child instance, not a
        // property on the child - so these 4 rows are built by hand instead of going through
        // the reflected-properties loop above.
        private void AddGridPlacementRows(UIComponent gridChild, ref int row)
        {
            AddGridPlacementRow("Grid Column", () => GridParent.GetColumn(gridChild), v => GridParent.SetColumn(gridChild, v), ref row);
            AddGridPlacementRow("Grid Row", () => GridParent.GetRow(gridChild), v => GridParent.SetRow(gridChild, v), ref row);
            AddGridPlacementRow("Grid Column Span", () => GridParent.GetColumnSpan(gridChild), v => GridParent.SetColumnSpan(gridChild, v), ref row);
            AddGridPlacementRow("Grid Row Span", () => GridParent.GetRowSpan(gridChild), v => GridParent.SetRowSpan(gridChild, v), ref row);
        }

        private void AddGridPlacementRow(string label, Func<int> getValue, Action<int> setValue, ref int row)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Auto) });

            grid.AddChild(new Label { Text = label, Margin = new Thickness(0, 2) }, 0, row);

            UIComponent editor = CreateTextEditor(getValue().ToString(), newText =>
            {
                if (!int.TryParse(newText, out int parsed)) return false;
                setValue(parsed);
                return true;
            });
            grid.AddChild(editor, 1, row);

            row++;
        }

        private UIComponent CreateEditor(PropertyInfo prop, object obj)
        {
            Type propertyType = prop.PropertyType;
            bool isBinding = propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Binding<>);
            Type valueType = isBinding ? propertyType.GetGenericArguments()[0] : propertyType;

            object CurrentValue()
            {
                object propertyValue = prop.GetValue(obj);
                if (!isBinding)
                {
                    return propertyValue;
                }

                return propertyValue == null ? null : GetBindingValueProperty(propertyValue.GetType()).GetValue(propertyValue);
            }

            void ApplyValue(object newValue)
            {
                if (!isBinding)
                {
                    prop.SetValue(obj, newValue);
                    return;
                }

                object bindingInstance = prop.GetValue(obj);
                if (bindingInstance == null)
                {
                    bindingInstance = Activator.CreateInstance(propertyType);
                    prop.SetValue(obj, bindingInstance);
                }

                GetBindingValueProperty(bindingInstance.GetType()).SetValue(bindingInstance, newValue);
            }

            object value = CurrentValue();

            if (valueType == typeof(bool))
            {
                var checkBox = new CheckBox
                {
                    IsChecked = value as bool? ?? false
                };

                checkBox.OnValueChanged = isChecked => ApplyValue(isChecked ?? false);

                return checkBox;
            }
            else if (valueType.IsEnum)
            {
                var comboBox = new ComboBox
                {
                    ItemsSource = Enum.GetNames(valueType).ToList(),
                    SelectedItem = value?.ToString()
                };

                comboBox.OnSelectionChanged = (selectedItem) =>
                {
                    if (selectedItem != null)
                    {
                        ApplyValue(Enum.Parse(valueType, selectedItem.ToString()));
                    }
                };

                return comboBox;
            }
            else if (valueType == typeof(string))
            {
                return CreateTextEditor(value as string ?? "", newText => { ApplyValue(newText); return true; });
            }
            else if (valueType == typeof(int))
            {
                return CreateTextEditor(value?.ToString() ?? "0", newText =>
                {
                    if (!int.TryParse(newText, out int parsed)) return false;
                    ApplyValue(parsed);
                    return true;
                });
            }
            else if (valueType == typeof(float))
            {
                return CreateTextEditor(value?.ToString() ?? "0", newText =>
                {
                    if (!float.TryParse(newText, out float parsed)) return false;
                    ApplyValue(parsed);
                    return true;
                });
            }
            else if (valueType == typeof(double))
            {
                return CreateTextEditor(value?.ToString() ?? "0", newText =>
                {
                    if (!double.TryParse(newText, out double parsed)) return false;
                    ApplyValue(parsed);
                    return true;
                });
            }
            else if (valueType == typeof(Length))
            {
                // Caught broadly (not just FormatException) because this runs once per frame
                // while the user is mid-edit (e.g. typing "12" then "12,") - an uncaught
                // exception here would crash the whole render loop on a single bad keystroke.
                return CreateTextEditor(value?.ToString() ?? "", newText =>
                {
                    try { ApplyValue(Length.FromString(newText)); return true; } catch { return false; }
                });
            }
            else if (valueType == typeof(Thickness))
            {
                return CreateTextEditor(value?.ToString() ?? "", newText =>
                {
                    try { ApplyValue(Thickness.FromString(newText)); return true; } catch { return false; }
                });
            }
            else if (valueType == typeof(Color))
            {
                string hex = value is Color color ? ColorHelper.ToHexColor(color) : "#000000FF";
                return CreateTextEditor(hex, newText =>
                {
                    try { ApplyValue(ColorHelper.FromString(newText)); return true; } catch { return false; }
                });
            }
            else
            {
                var label = new Label
                {
                    Text = value?.ToString() ?? "(not editable)"
                };
                return label;
            }
        }

        // Commits the field's text only when focus leaves it, not on every keystroke -
        // TextEntryControl.HandleKeyAsync already makes a single-line TextBox give up focus on
        // Enter (see its own comment: "Enter commits the edit the same way clicking away does"),
        // so OnFocusChanged(Focused: false) alone covers both Enter and tabbing/clicking away;
        // no separate key handling is needed here. tryApply returns false for text that doesn't
        // parse (e.g. a typo like "#ZZZZZZ"), in which case the field snaps back to the last
        // value that did apply, so an invalid edit doesn't just sit there with no feedback that
        // it was never accepted.
        private UIComponent CreateTextEditor(string initialText, Func<string, bool> tryApply)
        {
            var textBox = new TextBox { Text = initialText };
            string lastValidText = initialText;

            textBox.OnFocusChanged = (sender, uiEvent) =>
            {
                if (uiEvent.Focused)
                {
                    return;
                }

                string currentText = textBox.Text?.Value;
                if (currentText == lastValidText)
                {
                    return;
                }

                if (tryApply(currentText))
                {
                    lastValidText = currentText;
                }
                else
                {
                    textBox.Text.Value = lastValidText;
                }
            };

            return textBox;
        }

        private static PropertyInfo GetBindingValueProperty(Type bindingInstanceType)
        {
            return bindingInstanceType.GetProperty("Value");
        }
    }
}
