using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Examples.UI.Components;
using Microsoft.Xna.Framework;

namespace Examples.UI.HelpPages
{
    public class HelpPage_Panel : Panel
    {
        protected override void InitTemplate()
        {
            base.InitTemplate();

            var layoutPanel = new StackPanel()
            {
                Name = "A1",
                Orientation = Orientation.Vertical,

                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
            };


            base.AddChild(layoutPanel);


            layoutPanel.AddChild(
                new PageHeader()
                {
                    Padding = new Thickness(30, 0, 0, 0),
                    Title = "Panels"
                });

            StackPanel stackVertical = new StackPanel()
            {
                Name = "A2",
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                HorizontalScrollBarVisible = ScrollBarVisibility.Hidden,
                VerticalScrollBarVisible = ScrollBarVisibility.Auto,

                Background = Color.PeachPuff,
                Height = "350px",
                Width = "800px"
            };


            Border border = new Border
            {
                CornerRadius = new CornerRadius(10),
                //BorderColor = Color.Red,
                HorizontalAlignment = HorizontalAlignmentType.Center,
                VerticalAlignment = VerticalAlignmentType.Center,
            };

            border.Content = stackVertical;

            layoutPanel.AddChild(border);


            //-------------------------------------------------------------

            StackPanel stackHorizontal0 = new StackPanel()
            {
                Name = "A3",
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                HorizontalAlignment = HorizontalAlignmentType.Left,
                HorizontalScrollBarVisible = ScrollBarVisibility.Hidden,
                VerticalScrollBarVisible = ScrollBarVisibility.Hidden,

                //Background = Color.Blue,
                Height = "200px",
                Width = "100%",
                //Padding = new Thickness(10),
                //Margin = new Thickness(5, 20),

            };

            stackVertical.AddChild(stackHorizontal0);



            // Vertical Align = Top
            stackHorizontal0.AddChild(
                new Section()
                {
                    Name = "TEST1",

                    HorizontalAlignment = HorizontalAlignmentType.Left,
                    VerticalAlignment = VerticalAlignmentType.Stretch,
                    Width = "100%",
                    Height = "100%",

                    Content = new Button()
                    {
                        Text = "This is a Button",
                        //TextColor = Color.Black,
                        //Height = "40%",
                        HorizontalAlignment = HorizontalAlignmentType.Stretch,
                        VerticalAlignment = VerticalAlignmentType.Stretch
                    }
                });


            //-------------------------------------------------------------

            StackPanel stackHorizontal1 = new StackPanel()
            {
                Name = "A4",

                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                HorizontalScrollBarVisible = ScrollBarVisibility.Hidden,
                VerticalScrollBarVisible = ScrollBarVisibility.Hidden,

                Background = Color.PeachPuff,
                Height = "200px",
                Width = "100%",
            };

            stackVertical.AddChild(stackHorizontal1);

            // Vertical Align = Top
            stackHorizontal1.AddChild(
                new Section()
                {
                    Name = "A5",

                    HorizontalAlignment = HorizontalAlignmentType.Left,
                    VerticalAlignment = VerticalAlignmentType.Stretch,
                    Width = "50%",
                    Height = "100%",

                    Content = new Button()
                    {
                        Text = "This is a Button",
                        //TextColor = Color.Black,
                        //Height = "40%",
                        HorizontalAlignment = HorizontalAlignmentType.Stretch,
                        VerticalAlignment = VerticalAlignmentType.Stretch
                    }
                });

            stackHorizontal1.AddChild(
                new Section()
                {
                    Name = "A6",

                    //HorizontalAlignment= HorizontalAlignmentType.Stretch,
                    //VerticalAlignment = VerticalAlignmentType.Stretch,
                    Width = "50%",
                    Height = "100%",

                    Content = new Button()
                    {
                        Text = "This is a Button 2",
                        //TextColor = Color.Black,
                        //Height = "40%",
                        //HorizontalAlignment = HorizontalAlignmentType.Stretch,
                        //VerticalAlignment = VerticalAlignmentType.Top
                    }
                });


            //-------------------------------------------------------------

            StackPanel stackHorizontal2 = new StackPanel()
            {
                Name = "A6-a",

                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                HorizontalScrollBarVisible = ScrollBarVisibility.Hidden,
                VerticalScrollBarVisible = ScrollBarVisibility.Hidden,

                Background = Color.PeachPuff,
                Height = "200px",
                Width = "100%",
            };

            stackVertical.AddChild(stackHorizontal2);

            // Vertical Align = Top
            stackHorizontal2.AddChild(
                new Section()
                {
                    Name = "A7",

                    HorizontalAlignment = HorizontalAlignmentType.Left,
                    VerticalAlignment = VerticalAlignmentType.Stretch,
                    Width = "33%",
                    Height = "100%",

                    Content = new Button()
                    {
                        Text = "This is a Button",
                        //TextColor = Color.Black,
                        //Height = "40%",
                        HorizontalAlignment = HorizontalAlignmentType.Stretch,
                        VerticalAlignment = VerticalAlignmentType.Stretch
                    }
                });

            stackHorizontal2.AddChild(
                new Section()
                {
                    Name = "A8",

                    //HorizontalAlignment= HorizontalAlignmentType.Stretch,
                    //VerticalAlignment = VerticalAlignmentType.Stretch,
                    Width = "33%",
                    Height = "100%",

                    Content = new Button()
                    {
                        Text = "This is a Button 2",
                        //TextColor = Color.Black,
                        //Height = "40%",
                        //HorizontalAlignment = HorizontalAlignmentType.Stretch,
                        //VerticalAlignment = VerticalAlignmentType.Top
                    }
                });

            stackHorizontal2.AddChild(
                new Section()
                {
                    Name = "A9",

                    //HorizontalAlignment= HorizontalAlignmentType.Stretch,
                    //VerticalAlignment = VerticalAlignmentType.Stretch,
                    Width = "34%",
                    Height = "100%",

                    Content = new Button()
                    {
                        Text = "This is a Button 2",
                        //TextColor = Color.Black,
                        //Height = "40%",
                        //HorizontalAlignment = HorizontalAlignmentType.Stretch,
                        //VerticalAlignment = VerticalAlignmentType.Top
                    }
                });

            //-------------------------------------------------------------

            StackPanel stackHorizontal3 = new StackPanel()
            {
                Name = "A10",

                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                HorizontalScrollBarVisible = ScrollBarVisibility.Hidden,
                VerticalScrollBarVisible = ScrollBarVisibility.Hidden,

                Background = Color.PeachPuff,
                Height = "200px",
                Width = "100%"
            };

            stackVertical.AddChild(stackHorizontal3);

            // Vertical Align = Top
            stackHorizontal3.AddChild(
                new Section()
                {
                    Name = "A11",

                    HorizontalAlignment = HorizontalAlignmentType.Left,
                    VerticalAlignment = VerticalAlignmentType.Stretch,
                    Width = "25%",
                    Height = "100%",

                    Content = new Button()
                    {
                        Text = "This is a Button",
                        //TextColor = Color.Black,
                        //Height = "40%",
                        HorizontalAlignment = HorizontalAlignmentType.Stretch,
                        VerticalAlignment = VerticalAlignmentType.Stretch
                    }
                });

            stackHorizontal3.AddChild(
                new Section()
                {
                    HorizontalAlignment = HorizontalAlignmentType.Left,
                    VerticalAlignment = VerticalAlignmentType.Stretch,
                    Width = "25%",
                    Height = "100%",

                    Content = new Button()
                    {
                        Text = "This is a Button",
                        //TextColor = Color.Black,
                        //Height = "40%",
                        HorizontalAlignment = HorizontalAlignmentType.Stretch,
                        VerticalAlignment = VerticalAlignmentType.Stretch
                    }
                });

            stackHorizontal3.AddChild(
                new Section()
                {
                    HorizontalAlignment = HorizontalAlignmentType.Left,
                    VerticalAlignment = VerticalAlignmentType.Stretch,
                    Width = "25%",
                    Height = "100%",

                    Content = new Button()
                    {
                        Text = "This is a Button",
                        //TextColor = Color.Black,
                        //Height = "40%",
                        HorizontalAlignment = HorizontalAlignmentType.Stretch,
                        VerticalAlignment = VerticalAlignmentType.Stretch
                    }
                });

            stackHorizontal3.AddChild(
                new Section()
                {
                    HorizontalAlignment = HorizontalAlignmentType.Left,
                    VerticalAlignment = VerticalAlignmentType.Stretch,
                    Width = "25%",
                    Height = "100%",

                    Content = new Button()
                    {
                        Text = "This is a Button",
                        //TextColor = Color.Black,
                        //Height = "40%",
                        HorizontalAlignment = HorizontalAlignmentType.Stretch,
                        VerticalAlignment = VerticalAlignmentType.Stretch
                    }
                });

        }

    }
}
