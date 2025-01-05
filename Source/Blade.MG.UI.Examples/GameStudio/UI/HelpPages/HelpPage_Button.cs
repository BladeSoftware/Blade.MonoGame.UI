using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Examples.UI.Components;

namespace Examples.UI.HelpPages
{
    public class HelpPage_Button : Panel
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
                    Title = "Buttons"
                });


            // Vertical Alignment = Top
            var alignTopGrid = new Grid()
            {
                Name = "TESTx",
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
                        Name = "TEST-BUTTON-1",
                        Text = "This is a Button - 1",
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
                        Name = "TEST-BUTTON-2",
                        Text = "This is a Button - 2",
                        //TextColor = Color.Black,
                        MaxWidth = "75%",
                        Height = "40%",
                        HorizontalAlignment = HorizontalAlignmentType.Center,
                        VerticalAlignment = VerticalAlignmentType.Top,
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
                    Content = new Button()
                    {
                        Text = "This is a Button",
                        //TextColor = Color.Black,
                        MaxWidth = "75%",
                        Height = "40%",
                        HorizontalAlignment = HorizontalAlignmentType.Left,
                        VerticalAlignment = VerticalAlignmentType.Center
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
