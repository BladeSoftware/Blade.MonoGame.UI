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

                HorizontalScrollBarVisible = ScrollBarVisibility.Hidden,
                VerticalScrollBarVisible = ScrollBarVisibility.Hidden,


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
            //searchBox.OnTextChanged = (sender, args) => RefreshProperties(); // TODO: OnTextChanged event
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

        private UIComponent CreateEditor(PropertyInfo prop, object obj)
        {
            var value = prop.GetValue(obj);
            Type type = prop.PropertyType;

            if (type == typeof(bool))
            {
                var checkBox = new CheckBox
                {
                    IsChecked = (bool)value
                };
                //checkBox.OnCheckedChanged = (sender, args) =>
                //{
                //    prop.SetValue(obj, checkBox.IsChecked);
                //};
                return checkBox;
            }
            else if (type.IsEnum)
            {
                var comboBox = new ComboBox
                {
                    ItemsSource = Enum.GetNames(type).ToList(),
                    SelectedItem = value?.ToString()
                };
                //comboBox.OnSelectionChanged = (sender, args) =>
                //{
                //    prop.SetValue(obj, Enum.Parse(type, comboBox.SelectedItem));
                //};
                return comboBox;
            }
            else if (type == typeof(int) || type == typeof(float) || type == typeof(double) || type == typeof(string))
            {
                var textBox = new TextBox
                {
                    Text = value?.ToString() ?? ""
                };
                //textBox.OnTextChanged = (sender, args) =>
                //{
                //    object newValue = Convert.ChangeType(textBox.Text, type); 
                //    prop.SetValue(obj, newValue);
                //};
                return textBox;
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
    }
}
