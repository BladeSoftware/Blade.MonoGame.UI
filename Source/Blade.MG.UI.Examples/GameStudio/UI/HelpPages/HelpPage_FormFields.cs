using Blade.MG.UI;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Blade.MG.UI.Models;
using Examples.UI.Components;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Examples.UI.HelpPages
{
    public class HelpPage_FormFields : Panel
    {
        protected override void InitTemplate()
        {
            base.InitTemplate();

            var layoutPanel = new StackPanel()
            {
                Orientation = Orientation.Vertical,

                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,

            };


            base.AddChild(layoutPanel);



            layoutPanel.AddChild(
                new PageHeader()
                {
                    Padding = new Thickness(30, 0, 0, 0),
                    Title = "Form Fields"
                });


            // Vertical Alignment = Top
            var alignTopGrid = new Grid()
            {
                Name = "TEST",
                Width = "80%",
                HorizontalAlignment = HorizontalAlignmentType.Center
            };

            alignTopGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1) });
            alignTopGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1) });
            alignTopGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1) });

            alignTopGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Pixel, 150) });
            alignTopGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Pixel, 150) });
            alignTopGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Pixel, 150) });

            layoutPanel.AddChild(alignTopGrid);


            //// ---------------------
            //Dictionary<string, string> appResourceDict = new Dictionary<string, string>();
            //appResourceDict.Add("Background", Color.Green.ToString());
            //appResourceDict.Add("TextColor", Color.SkyBlue.ToString());
            //appResourceDict.Add("[CheckBox]TextColor", Color.Blue.ToString());

            //Dictionary<string, string> localResourceDict = new Dictionary<string, string>();
            //localResourceDict.Add("Background", Color.Pink.ToString());
            //localResourceDict.Add("TextColor", Color.LightSalmon.ToString());

            //CheckBox checkBox = new CheckBox();
            //string resourceProperty = "TextColor";
            //string resourceKey1 = $"[{checkBox.ResourceKey}]{resourceProperty}";
            //string resourceKey2 = $"{resourceProperty}";


            //string col = checkBox.GetResourceValue(resourceProperty);

            //string value = "";
            //if (!localResourceDict.TryGetValue("TextColor", out value))
            //{
            //    if (!appResourceDict.TryGetValue("TextColor", out value))
            //    {
            //        value = Color.Black.ToString();
            //    }
            //}

            //UIColor testColor = new UIColor(value);
            ////testColor = "#FFFF00";
            ////testColor = "TextColor";

            ////testColor = Color.Green.ToString();

            //Color x = testColor;
            //Binding<UIColor> XBackground = testColor;
            //Background = testColor.ToColor();

            //// ---------------------



            // Vertical Align = Top
            alignTopGrid.AddChild(
                new Section()
                {
                    Content = new TextBox()
                    {
                        Name = "TEST",
                        Text = "This is a Text Box",
                        Label = "Standard",
                        HelperText = "This is an Example Text Box",
                        Variant = Variant.Standard,
                        //TextColor = Color.Black,
                        //Width = "75%",
                        //Height = "40%",
                        Background = Color.White,
                        HorizontalAlignment = HorizontalAlignmentType.Stretch,
                        VerticalAlignment = VerticalAlignmentType.Center,
                        Margin = new Thickness(5, 10)

                    }
                },
                0, 0);

            alignTopGrid.AddChild(
                new Section()
                {
                    Content = new TextBox()
                    {
                        Name = "TEST",
                        Text = "This is a Text Box",
                        Label = "Filled",
                        HelperText = "This is an Example Text Box",
                        Variant = Variant.Filled,
                        //TextColor = Color.Black,
                        //Width = "75%",
                        //Height = "40%",
                        Background = Color.White,
                        HorizontalAlignment = HorizontalAlignmentType.Stretch,
                        VerticalAlignment = VerticalAlignmentType.Center,
                        Margin = new Thickness(5, 10)

                    }
                },
                1, 0);

            alignTopGrid.AddChild(
                new Section()
                {
                    Content = new TextBox()
                    {
                        Name = "TEST",
                        Text = "This is a Text Box",
                        Label = "Outlined",
                        HelperText = "This is an Example Text Box",
                        Variant = Variant.Outlined,
                        //TextColor = Color.Black,
                        //Width = "75%",
                        //Height = "40%",
                        Background = Color.White,
                        HorizontalAlignment = HorizontalAlignmentType.Stretch,
                        VerticalAlignment = VerticalAlignmentType.Center,
                        Margin = new Thickness(5, 10)

                    }
                },
                2, 0);


            // Vertical Align = Center
            alignTopGrid.AddChild(
                new Section()
                {
                    Content = new CheckBox()
                    {
                        Text = "CheckBox : Two-State",
                        //TextColor = Color.Black,
                        //Height = "40%",
                        //Height = 35,
                        Tristate = false,
                        HorizontalAlignment = HorizontalAlignmentType.Left,
                        VerticalAlignment = VerticalAlignmentType.Center,
                        Margin = new Thickness(0),
                        Padding = new Thickness(4),

                        IsEnabled = true,
                        IsChecked = true
                    }
                },
                0, 1);

            alignTopGrid.AddChild(
                new Section()
                {
                    Content = new CheckBox()
                    {
                        Text = "CheckBox : Tri-State",
                        //TextColor = Color.Black,
                        //Height = "40%",
                        //Height = 35,
                        Tristate = true,
                        HorizontalAlignment = HorizontalAlignmentType.Left,
                        VerticalAlignment = VerticalAlignmentType.Center,
                        Margin = new Thickness(0),
                        Padding = new Thickness(4),

                        IsEnabled = true,
                        IsChecked = null
                    }
                },
                1, 1);

            alignTopGrid.AddChild(
                new Section()
                {
                    Content = new CheckBox()
                    {
                        Text = "CheckBox : Disabled",
                        //TextColor = Color.Black,
                        //Height = "40%",
                        //Height = 35,
                        Tristate = false,
                        HorizontalAlignment = HorizontalAlignmentType.Left,
                        VerticalAlignment = VerticalAlignmentType.Center,
                        Margin = new Thickness(0),
                        Padding = new Thickness(4),

                        IsEnabled = false,
                        IsChecked = true
                    }
                },
                2, 1);


            // Vertical Align = Bottom
            alignTopGrid.AddChild(
                new Section()
                {
                    Content = new CheckBox()
                    {
                        Text = "CheckBox : Two-State",
                        //TextColor = Color.Black,
                        //Height = "40%",
                        //Height = 35,
                        Tristate = false,
                        HorizontalAlignment = HorizontalAlignmentType.Left,
                        VerticalAlignment = VerticalAlignmentType.Center,
                        Margin = new Thickness(0),
                        Padding = new Thickness(4),

                        CheckedIcon = "M 11.9688 52.2930 C 12.9298 53.0195 14.1485 52.7617 15.6016 51.7071 L 28.0001 42.6133 L 40.4220 51.7071 C 41.8751 52.7617 43.0704 53.0195 44.0548 52.2930 C 45.0157 51.5664 45.2267 50.3711 44.6407 48.6602 L 39.7422 34.0820 L 52.2578 25.0820 C 53.7112 24.0508 54.2968 22.9727 53.9219 21.8008 C 53.5470 20.6758 52.4454 20.1133 50.6406 20.1367 L 35.2891 20.2305 L 30.6251 5.5820 C 30.0626 3.8476 29.2188 2.9805 28.0001 2.9805 C 26.8048 2.9805 25.9610 3.8476 25.3985 5.5820 L 20.7344 20.2305 L 5.3829 20.1367 C 3.5782 20.1133 2.4766 20.6758 2.1016 21.8008 C 1.7032 22.9727 2.3126 24.0508 3.7657 25.0820 L 16.2813 34.0820 L 11.3829 48.6602 C 10.7969 50.3711 11.0079 51.5664 11.9688 52.2930 Z M 15.3438 47.6524 C 15.2969 47.6055 15.3204 47.5820 15.3438 47.4414 L 20.0079 34.0352 C 20.3126 33.1211 20.1485 32.3945 19.3282 31.8320 L 7.6563 23.7695 C 7.5391 23.6992 7.5157 23.6524 7.5391 23.5820 C 7.5626 23.5117 7.6095 23.5117 7.7501 23.5117 L 21.9298 23.7695 C 22.8907 23.7930 23.5001 23.3945 23.8048 22.4336 L 27.8595 8.8633 C 27.9063 8.7227 27.9532 8.6758 28.0001 8.6758 C 28.0704 8.6758 28.1173 8.7227 28.1407 8.8633 L 32.2188 22.4336 C 32.5001 23.3945 33.1329 23.7930 34.0938 23.7695 L 48.2733 23.5117 C 48.4139 23.5117 48.4610 23.5117 48.4845 23.5820 C 48.508 23.6524 48.4610 23.6992 48.3674 23.7695 L 36.6954 31.8320 C 35.8751 32.3945 35.6876 33.1211 36.0157 34.0352 L 40.6798 47.4414 C 40.7032 47.5820 40.7266 47.6055 40.6798 47.6524 C 40.6329 47.7227 40.5626 47.6758 40.4688 47.6055 L 29.1954 39.0039 C 28.4454 38.4180 27.5782 38.4180 26.8282 39.0039 L 15.5548 47.6055 C 15.4610 47.6758 15.3907 47.7227 15.3438 47.6524 Z",
                        UncheckedIcon = "M 11.9688 52.2930 C 12.9298 53.0195 14.1485 52.7617 15.6016 51.7071 L 28.0001 42.6133 L 40.4220 51.7071 C 41.8751 52.7617 43.0704 53.0195 44.0548 52.2930 C 45.0157 51.5664 45.2267 50.3711 44.6407 48.6602 L 39.7422 34.0820 L 52.2578 25.0820 C 53.7112 24.0508 54.2968 22.9727 53.9219 21.8008 C 53.5470 20.6758 52.4454 20.1133 50.6406 20.1367 L 35.2891 20.2305 L 30.6251 5.5820 C 30.0626 3.8476 29.2188 2.9805 28.0001 2.9805 C 26.8048 2.9805 25.9610 3.8476 25.3985 5.5820 L 20.7344 20.2305 L 5.3829 20.1367 C 3.5782 20.1133 2.4766 20.6758 2.1016 21.8008 C 1.7032 22.9727 2.3126 24.0508 3.7657 25.0820 L 16.2813 34.0820 L 11.3829 48.6602 C 10.7969 50.3711 11.0079 51.5664 11.9688 52.2930 Z M 15.3438 47.6524 C 15.2969 47.6055 15.3204 47.5820 15.3438 47.4414 L 20.0079 34.0352 C 20.3126 33.1211 20.1485 32.3945 19.3282 31.8320 L 7.6563 23.7695 C 7.5391 23.6992 7.5157 23.6524 7.5391 23.5820 C 7.5626 23.5117 7.6095 23.5117 7.7501 23.5117 L 21.9298 23.7695 C 22.8907 23.7930 23.5001 23.3945 23.8048 22.4336 L 27.8595 8.8633 C 27.9063 8.7227 27.9532 8.6758 28.0001 8.6758 C 28.0704 8.6758 28.1173 8.7227 28.1407 8.8633 L 32.2188 22.4336 C 32.5001 23.3945 33.1329 23.7930 34.0938 23.7695 L 48.2733 23.5117 C 48.4139 23.5117 48.4610 23.5117 48.4845 23.5820 C 48.508 23.6524 48.4610 23.6992 48.3674 23.7695 L 36.6954 31.8320 C 35.8751 32.3945 35.6876 33.1211 36.0157 34.0352 L 40.6798 47.4414 C 40.7032 47.5820 40.7266 47.6055 40.6798 47.6524 C 40.6329 47.7227 40.5626 47.6758 40.4688 47.6055 L 29.1954 39.0039 C 28.4454 38.4180 27.5782 38.4180 26.8282 39.0039 L 15.5548 47.6055 C 15.4610 47.6758 15.3907 47.7227 15.3438 47.6524 Z",

                        IsEnabled = true,
                        IsChecked = true
                    }
                },
                0, 2);

            alignTopGrid.AddChild(
                new Section()
                {
                    Content = new CheckBox()
                    {
                        Text = "CheckBox : Two-State",
                        //TextColor = Color.Black,
                        //Height = "40%",
                        //Height = 35,
                        Tristate = false,
                        HorizontalAlignment = HorizontalAlignmentType.Left,
                        VerticalAlignment = VerticalAlignmentType.Center,
                        Margin = new Thickness(0),
                        Padding = new Thickness(4),

                        CheckedIcon = "M 11.9688 52.2930 C 12.9298 53.0195 14.1485 52.7617 15.6016 51.7071 L 28.0001 42.6133 L 40.4220 51.7071 C 41.8751 52.7617 43.0704 53.0195 44.0548 52.2930 C 45.0157 51.5664 45.2267 50.3711 44.6407 48.6602 L 39.7422 34.0820 L 52.2578 25.0820 C 53.7112 24.0508 54.2968 22.9727 53.9219 21.8008 C 53.5470 20.6758 52.4454 20.1133 50.6406 20.1367 L 35.2891 20.2305 L 30.6251 5.5820 C 30.0626 3.8476 29.2188 2.9805 28.0001 2.9805 C 26.8048 2.9805 25.9610 3.8476 25.3985 5.5820 L 20.7344 20.2305 L 5.3829 20.1367 C 3.5782 20.1133 2.4766 20.6758 2.1016 21.8008 C 1.7032 22.9727 2.3126 24.0508 3.7657 25.0820 L 16.2813 34.0820 L 11.3829 48.6602 C 10.7969 50.3711 11.0079 51.5664 11.9688 52.2930 Z M 15.3438 47.6524 C 15.2969 47.6055 15.3204 47.5820 15.3438 47.4414 L 20.0079 34.0352 C 20.3126 33.1211 20.1485 32.3945 19.3282 31.8320 L 7.6563 23.7695 C 7.5391 23.6992 7.5157 23.6524 7.5391 23.5820 C 7.5626 23.5117 7.6095 23.5117 7.7501 23.5117 L 21.9298 23.7695 C 22.8907 23.7930 23.5001 23.3945 23.8048 22.4336 L 27.8595 8.8633 C 27.9063 8.7227 27.9532 8.6758 28.0001 8.6758 C 28.0704 8.6758 28.1173 8.7227 28.1407 8.8633 L 32.2188 22.4336 C 32.5001 23.3945 33.1329 23.7930 34.0938 23.7695 L 48.2733 23.5117 C 48.4139 23.5117 48.4610 23.5117 48.4845 23.5820 C 48.508 23.6524 48.4610 23.6992 48.3674 23.7695 L 36.6954 31.8320 C 35.8751 32.3945 35.6876 33.1211 36.0157 34.0352 L 40.6798 47.4414 C 40.7032 47.5820 40.7266 47.6055 40.6798 47.6524 C 40.6329 47.7227 40.5626 47.6758 40.4688 47.6055 L 29.1954 39.0039 C 28.4454 38.4180 27.5782 38.4180 26.8282 39.0039 L 15.5548 47.6055 C 15.4610 47.6758 15.3907 47.7227 15.3438 47.6524 Z",
                        UncheckedIcon = "M 11.9688 52.2930 C 12.9298 53.0195 14.1485 52.7617 15.6016 51.7071 L 28.0001 42.6133 L 40.4220 51.7071 C 41.8751 52.7617 43.0704 53.0195 44.0548 52.2930 C 45.0157 51.5664 45.2267 50.3711 44.6407 48.6602 L 39.7422 34.0820 L 52.2578 25.0820 C 53.7112 24.0508 54.2968 22.9727 53.9219 21.8008 C 53.5470 20.6758 52.4454 20.1133 50.6406 20.1367 L 35.2891 20.2305 L 30.6251 5.5820 C 30.0626 3.8476 29.2188 2.9805 28.0001 2.9805 C 26.8048 2.9805 25.9610 3.8476 25.3985 5.5820 L 20.7344 20.2305 L 5.3829 20.1367 C 3.5782 20.1133 2.4766 20.6758 2.1016 21.8008 C 1.7032 22.9727 2.3126 24.0508 3.7657 25.0820 L 16.2813 34.0820 L 11.3829 48.6602 C 10.7969 50.3711 11.0079 51.5664 11.9688 52.2930 Z M 15.3438 47.6524 C 15.2969 47.6055 15.3204 47.5820 15.3438 47.4414 L 20.0079 34.0352 C 20.3126 33.1211 20.1485 32.3945 19.3282 31.8320 L 7.6563 23.7695 C 7.5391 23.6992 7.5157 23.6524 7.5391 23.5820 C 7.5626 23.5117 7.6095 23.5117 7.7501 23.5117 L 21.9298 23.7695 C 22.8907 23.7930 23.5001 23.3945 23.8048 22.4336 L 27.8595 8.8633 C 27.9063 8.7227 27.9532 8.6758 28.0001 8.6758 C 28.0704 8.6758 28.1173 8.7227 28.1407 8.8633 L 32.2188 22.4336 C 32.5001 23.3945 33.1329 23.7930 34.0938 23.7695 L 48.2733 23.5117 C 48.4139 23.5117 48.4610 23.5117 48.4845 23.5820 C 48.508 23.6524 48.4610 23.6992 48.3674 23.7695 L 36.6954 31.8320 C 35.8751 32.3945 35.6876 33.1211 36.0157 34.0352 L 40.6798 47.4414 C 40.7032 47.5820 40.7266 47.6055 40.6798 47.6524 C 40.6329 47.7227 40.5626 47.6758 40.4688 47.6055 L 29.1954 39.0039 C 28.4454 38.4180 27.5782 38.4180 26.8282 39.0039 L 15.5548 47.6055 C 15.4610 47.6758 15.3907 47.7227 15.3438 47.6524 Z",

                        IsEnabled = false,
                        IsChecked = true
                    }
                },
                1, 2);

            alignTopGrid.AddChild(
                new Section()
                {
                    Content = new Button()
                    {
                        Text = "This is a Button",
                        //TextColor = Color.Black,
                        MaxWidth = "75%",
                        Height = "40%",
                        HorizontalAlignment = HorizontalAlignmentType.Right,
                        VerticalAlignment = VerticalAlignmentType.Bottom
                    }
                },
                2, 2);

        }

    }
}
