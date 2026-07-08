using Blade.MG.UI;
using Microsoft.Xna.Framework;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Examples.UI.Components;

namespace Examples.UI.HelpPages
{
    public class HelpPage_DockPanel : Panel
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
                    Title = "Dock Panel",
                    Description = "Left/Right/Top/Bottom/Center regions with draggable splitters between them."
                });

            var dockPanel = new DockPanel();

            var toggles = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Top,
                Margin = new Thickness(30, 10, 30, 10),
                HorizontalScrollBarVisible = ScrollBarVisibility.Hidden,
                VerticalScrollBarVisible = ScrollBarVisibility.Hidden,
            };

            layoutPanel.AddChild(toggles);

            toggles.AddChild(new CheckBox()
            {
                IsChecked = true,
                Margin = new Thickness(0, 0, 20, 0),
                Text = "Left Panel",
                OnValueChanged = value => dockPanel.IsLeftPanelVisible = value ?? false,
            });

            toggles.AddChild(new CheckBox()
            {
                IsChecked = true,
                Margin = new Thickness(0, 0, 20, 0),
                Text = "Right Panel",
                OnValueChanged = value => dockPanel.IsRightPanelVisible = value ?? false,
            });

            toggles.AddChild(new CheckBox()
            {
                IsChecked = true,
                Margin = new Thickness(0, 0, 20, 0),
                Text = "Top Panel",
                OnValueChanged = value => dockPanel.IsTopPanelVisible = value ?? false,
            });

            toggles.AddChild(new CheckBox()
            {
                IsChecked = true,
                Margin = new Thickness(0, 0, 20, 0),
                Text = "Bottom Panel",
                OnValueChanged = value => dockPanel.IsBottomPanelVisible = value ?? false,
            });

            toggles.AddChild(new CheckBox()
            {
                IsChecked = true,
                Text = "Inset Left/Right",
                OnValueChanged = value => dockPanel.InsetLeftRightPanels = value ?? false,
            });

            var border = new Border()
            {
                BorderThickness = new Thickness(1),
                BorderColor = new Binding<Color>(() => Theme.Outline),
                Margin = new Thickness(30, 10, 30, 20),
                Padding = new Thickness(10),
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                Height = 420,
            };

            layoutPanel.AddChild(border);
            layoutPanel.StretchLastChild = true;

            dockPanel.LeftPanel.Background = new Binding<Color>(() => Theme.PrimaryContainer);
            dockPanel.LeftPanel.AddChild(new Label { Text = "Left", TextColor = new Binding<Color>(() => Theme.OnPrimaryContainer), HorizontalAlignment = HorizontalAlignmentType.Center, VerticalAlignment = VerticalAlignmentType.Center });

            dockPanel.RightPanel.Background = new Binding<Color>(() => Theme.SecondaryContainer);
            dockPanel.RightPanel.AddChild(new Label { Text = "Right", TextColor = new Binding<Color>(() => Theme.OnSecondaryContainer), HorizontalAlignment = HorizontalAlignmentType.Center, VerticalAlignment = VerticalAlignmentType.Center });

            dockPanel.TopPanel.Background = new Binding<Color>(() => Theme.TertiaryContainer);
            dockPanel.TopPanel.AddChild(new Label { Text = "Top", TextColor = new Binding<Color>(() => Theme.OnTertiaryContainer), HorizontalAlignment = HorizontalAlignmentType.Center, VerticalAlignment = VerticalAlignmentType.Center });

            dockPanel.BottomPanel.Background = new Binding<Color>(() => Theme.ErrorContainer);
            dockPanel.BottomPanel.AddChild(new Label { Text = "Bottom", TextColor = new Binding<Color>(() => Theme.OnErrorContainer), HorizontalAlignment = HorizontalAlignmentType.Center, VerticalAlignment = VerticalAlignmentType.Center });

            dockPanel.CenterPanel.Background = new Binding<Color>(() => Theme.Surface);
            dockPanel.CenterPanel.AddChild(new Label { Text = "Center", TextColor = new Binding<Color>(() => Theme.OnSurface), HorizontalAlignment = HorizontalAlignmentType.Center, VerticalAlignment = VerticalAlignmentType.Center });

            dockPanel.LeftWidth = "20%";
            dockPanel.RightWidth = "20%";

            border.Content = dockPanel;
        }

    }
}
