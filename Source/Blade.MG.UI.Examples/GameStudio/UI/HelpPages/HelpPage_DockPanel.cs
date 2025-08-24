using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Examples.UI.Components;
using Microsoft.Xna.Framework;

namespace Blade.MG.UI.Examples.GameStudio.UI.HelpPages
{
    public class HelpPage_DockPanel : Panel
    {
        public HelpPage_DockPanel()
        {
        }

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
                    Title = "Dock Panel"
                });


            var horizStack = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Top,
                Margin = new Thickness(20, 10, 20, 10),
                HorizontalScrollBarVisible = false,
                VerticalScrollBarVisible = false,
            };

            layoutPanel.AddChild(horizStack);


            var dockPanel = new DockPanel();

            horizStack.AddChild(new CheckBox()
            {
                IsChecked = true,
                Margin = new Thickness(0, 0, 20, 0),
                Text = "Left Panel",
                OnValueChanged = (value) => { dockPanel.IsLeftPanelVisible = value ?? false; }
            });

            horizStack.AddChild(new CheckBox()
            {
                IsChecked = true,
                Margin = new Thickness(0, 0, 20, 0),
                Text = "Right Panel",
                OnValueChanged = (value) => { dockPanel.IsRightPanelVisible = value ?? false; }
            });

            horizStack.AddChild(new CheckBox()
            {
                IsChecked = true,
                Margin = new Thickness(0, 0, 20, 0),
                Text = "Top Panel",
                OnValueChanged = (value) => { dockPanel.IsTopPanelVisible = value ?? false; }
            });

            horizStack.AddChild(new CheckBox()
            {
                IsChecked = true,
                Margin = new Thickness(0, 0, 20, 0),
                Text = "Bottom Panel",
                OnValueChanged = (value) => { dockPanel.IsBottomPanelVisible = value ?? false; }
            });


            var border = new Border()
            {
                BorderThickness = 2,
                BorderColor = Color.Gray,
                Margin = new Thickness(20),
                Padding = new Thickness(10),
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
            };

            layoutPanel.AddChild(border);


            dockPanel.LeftPanel.Background = Color.LightBlue;
            dockPanel.LeftPanel.AddChild(new Label { Text = "Left Panel" });

            dockPanel.RightPanel.Background = Color.LightGreen;
            dockPanel.RightPanel.AddChild(new Label { Text = "Right Panel" });

            dockPanel.TopPanel.Background = Color.LightYellow;
            dockPanel.TopPanel.AddChild(new Label { Text = "Top Panel" });

            dockPanel.BottomPanel.Background = Color.LightCoral;
            dockPanel.BottomPanel.AddChild(new Label { Text = "Bottom Panel" });

            dockPanel.CenterPanel.Background = Color.White;
            dockPanel.CenterPanel.AddChild(new Label { Text = "Center Panel", HorizontalAlignment = HorizontalAlignmentType.Center });

            //dockPanel.Background = Color.Red;
            dockPanel.Width = 800;
            dockPanel.Height = 600;

            border.Content = dockPanel;
        }


    }
}