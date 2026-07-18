using Blade.MG.UI;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Examples.UI.Components;
using Microsoft.Xna.Framework;

namespace Examples.UI.HelpPages
{
    public class HelpPage_Popup : Panel
    {
        // Created lazily on first click - Popup.Content silently no-ops until LoadContent has
        // run (which only happens once it's actually added to the UIManager via ShowAt/Show), so
        // the content is populated right after the very first ShowAt call rather than at
        // construction time. Reused (moved, not recreated) on subsequent clicks.
        private Popup infoPopup;
        private bool infoPopupContentSet;

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
                    Title = "Popups",
                    Description = "A non-modal floating overlay anchored at a screen point, clamped to stay on-screen. The base Tooltip and ComboBox's dropdown both build on this."
                });

            var grid = new Grid()
            {
                Width = "80%",
                HorizontalAlignment = HorizontalAlignmentType.Center,
                Margin = new Thickness(0, 20, 0, 0),
            };

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Auto) });

            layoutPanel.AddChild(grid);

            grid.AddChild(BuildSection(), 0, 0);
        }

        private Section BuildSection()
        {
            var stack = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignmentType.Left,
                HorizontalScrollBarVisible = ScrollBarVisibility.Hidden,
                VerticalScrollBarVisible = ScrollBarVisibility.Hidden,
            };

            stack.AddChild(new Label { Text = "Click to show a popup anchored just below the button:", Margin = new Thickness(0, 0, 0, 12) });

            var showButton = new Button
            {
                Text = "Show Popup",
                Width = 160,
                HorizontalAlignment = HorizontalAlignmentType.Left,
                OnActivate = (sender, uiEvent) =>
                {
                    var button = (UIComponent)sender;
                    var game = ParentWindow.Game;

                    Rectangle buttonRect = button.GetFinalRect();
                    Point anchor = new Point(buttonRect.Left, buttonRect.Bottom + 6);

                    infoPopup ??= new Popup();
                    infoPopup.ShowAt(game, anchor);

                    if (!infoPopupContentSet)
                    {
                        infoPopupContentSet = true;
                        infoPopup.Content = BuildPopupContent(game);
                    }
                },
            };

            stack.AddChild(showButton);

            return new Section() { Content = stack, Margin = new Thickness(5, 10) };
        }

        private UIComponent BuildPopupContent(Game game)
        {
            var content = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                HorizontalScrollBarVisible = ScrollBarVisibility.Hidden,
                VerticalScrollBarVisible = ScrollBarVisibility.Hidden,
            };

            content.AddChild(new Label
            {
                Text = "This is a Popup - a non-modal overlay.",
                TextColor = new Binding<Color>(() => Theme.OnSurface),
                Margin = new Thickness(0, 0, 0, 8),
            });

            content.AddChild(new Button
            {
                Text = "Close",
                Width = 100,
                Height = 32,
                OnActivate = (sender, uiEvent) => infoPopup.Close(game),
            });

            return content;
        }
    }
}
