using Blade.MG.UI;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Examples.UI.Components;
using Microsoft.Xna.Framework;

namespace Examples.UI.HelpPages
{
    public class HelpPage_TabPanel : Panel
    {
        protected override void InitTemplate()
        {
            base.InitTemplate();

            var layoutPanel = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                HorizontalScrollBarVisible = ScrollBarVisibility.Hidden,
                VerticalScrollBarVisible = ScrollBarVisibility.Hidden,
            };

            base.AddChild(layoutPanel);

            layoutPanel.AddChild(
                new PageHeader()
                {
                    Padding = new Thickness(30, 0, 0, 0),
                    Title = "Tab Panel",
                    Description = "Groups related content behind a row of tab headers."
                });

            var tabPanel = new TabPanel()
            {
                Margin = new Thickness(30, 10, 30, 20),
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
            };

            layoutPanel.AddChild(tabPanel);
            layoutPanel.StretchLastChild = true;

            tabPanel.AddTab(BuildTabContent("The Tab Panel control groups several pages of content behind a row of headers - click a header to switch pages."), "Overview", setAsActiveTab: true);
            tabPanel.AddTab(BuildTabContent("Tab content can be any control tree - StackPanel, Grid, another Panel - just like any other Content property in the library."), "Layout");
            tabPanel.AddTab(BuildTabContent("Tab headers are themed automatically; DividerColor controls the line drawn under the header row."), "Styling");
            tabPanel.AddTab(BuildTabContent("AddTab takes the content, a data context (used as the header text here), and an optional 'set as active' flag."), "Usage");
        }

        private static Section BuildTabContent(string message)
        {
            return new Section()
            {
                Margin = new Thickness(10),
                Content = new Label()
                {
                    Text = message,
                    FontSize = 15,
                    TextColor = new Binding<Color>(() => UIManager.DefaultTheme.OnSurface),
                    HorizontalAlignment = HorizontalAlignmentType.Left,
                    VerticalAlignment = VerticalAlignmentType.Top,
                    Padding = new Thickness(20),
                }
            };
        }
    }
}
