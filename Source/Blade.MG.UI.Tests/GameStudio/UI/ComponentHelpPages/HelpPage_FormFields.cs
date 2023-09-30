using Blade.UI.Components;
using Blade.UI.Controls;

namespace GameStudio.UI.Editors
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

            // Vertical Align = Top
            alignTopGrid.AddChild(
                new Section()
                {
                    Content = new Button()
                    {
                        Name = "TEST",
                        Text = "This is a Button",
                        //TextColor = Color.Black,
                        Width = "75%",
                        Height = "40%",
                        HorizontalAlignment = HorizontalAlignmentType.Left,
                        VerticalAlignment = VerticalAlignmentType.Top
                    }
                },
                0, 0);

            alignTopGrid.AddChild(
                new Section()
                {
                    Content = new Button()
                    {
                        Text = "This is a Button",
                        //TextColor = Color.Black,
                        MaxWidth = "75%",
                        Height = "40%",
                        HorizontalAlignment = HorizontalAlignmentType.Center,
                        VerticalAlignment = VerticalAlignmentType.Top
                    }
                },
                1, 0);

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
                        VerticalAlignment = VerticalAlignmentType.Top
                    }
                },
                2, 0);


            // Vertical Align = Center
            alignTopGrid.AddChild(
                new Section()
                {
                    Content = new CheckBox()
                    {
                        Text = "This is a CheckBox",
                        //TextColor = Color.Black,
                        MaxWidth = "75%",
                        Height = "40%",
                        //Height = 35,
                        HorizontalAlignment = HorizontalAlignmentType.Left,
                        VerticalAlignment = VerticalAlignmentType.Center,
                        Stretch = StretchType.Fill,
                        //StretchDirection = StretchDirectionType.Both,
                        Margin = new Thickness(0),
                        Padding = new Thickness(4)
                    }
                },
                0, 1);

            alignTopGrid.AddChild(
                new Section()
                {
                    Content = new Button()
                    {
                        Text = "This is a Button",
                        //TextColor = Color.Black,
                        MaxWidth = "75%",
                        Height = "40%",
                        HorizontalAlignment = HorizontalAlignmentType.Center,
                        VerticalAlignment = VerticalAlignmentType.Center
                    }
                },
                1, 1);

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
                        VerticalAlignment = VerticalAlignmentType.Center
                    }
                },
                2, 1);


            // Vertical Align = Bottom
            alignTopGrid.AddChild(
                new Section()
                {
                    Content = new Button()
                    {
                        Text = "This is a Button",
                        //TextColor = Color.Black,
                        MaxWidth = "75%",
                        Height = "40%",
                        HorizontalAlignment = HorizontalAlignmentType.Left,
                        VerticalAlignment = VerticalAlignmentType.Bottom
                    }
                },
                0, 2);

            alignTopGrid.AddChild(
                new Section()
                {
                    Content = new Button()
                    {
                        Text = "This is a Button",
                        //TextColor = Color.Black,
                        MaxWidth = "75%",
                        Height = "40%",
                        HorizontalAlignment = HorizontalAlignmentType.Center,
                        VerticalAlignment = VerticalAlignmentType.Bottom
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
