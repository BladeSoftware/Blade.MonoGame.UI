using Blade.MG.UI;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Blade.MG.UI.Theming;
using Examples.UI.HelpPages;
using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace Examples.UI
{
    /// <summary>
    /// Root screen for the showcase: an AppBar (title + live theme switcher) over a
    /// categorized nav rail and a content pane that hosts whichever HelpPage is selected.
    /// </summary>
    public class ComponentTesterUI : UIWindow
    {
        private Control contentHost;

        private Border selectedNavItem;

        public override void Initialize(Game game)
        {
            base.Initialize(game);
        }

        public override void LoadContent()
        {
            base.LoadContent();

            BuildScreen(base.Game);
        }

        public override void PerformLayout(GameTime gameTime)
        {
            base.PerformLayout(gameTime);
        }

        public void BuildScreen(Game game)
        {
            Background = new Binding<Color>(() => Theme.Background);

            var rootGrid = new Grid()
            {
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
            };
            rootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1f) });
            rootGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Auto) });
            rootGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Star, 1f) });

            base.AddChild(rootGrid);

            rootGrid.AddChild(BuildAppBar(), 0, 0);

            var bodyGrid = new Grid()
            {
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
            };
            bodyGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Pixel, 260) });
            bodyGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1f) });
            bodyGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Star, 1f) });

            rootGrid.AddChild(bodyGrid, 0, 1);

            contentHost = new Control()
            {
                Background = new Binding<Color>(() => Theme.Background),
                Padding = new Thickness(32, 24),
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
            };

            bodyGrid.AddChild(BuildNav(), 0, 0);
            bodyGrid.AddChild(contentHost, 1, 0);

            // Land on the welcome page.
            contentHost.Content = new HelpPage_Welcome();
        }

        private AppBar BuildAppBar()
        {
            var appBar = new AppBar()
            {
                Title = "Blade.MG.UI Showcase",
            };

            var allThemes = DefaultThemes.All;

            var themePicker = new ComboBox()
            {
                ItemsSource = allThemes,
                ItemToString = o => (o as UITheme)?.Name ?? "",
                SelectedItem = allThemes.FirstOrDefault(t => t.Name == UIManager.DefaultTheme.Name),
                Width = 170,
                Height = 40,
                VerticalAlignment = VerticalAlignmentType.Center,
                Margin = new Thickness(0, 0, 4, 0),
                OnSelectionChanged = selected =>
                {
                    if (selected is UITheme theme)
                    {
                        UIManager.SetTheme(theme);
                    }
                },
            };

            appBar.AddAction(themePicker);

            return appBar;
        }

        private StackPanel BuildNav()
        {
            var nav = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                Background = new Binding<Color>(() => Theme.SurfaceVariant),
                Padding = new Thickness(12, 16),
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                HorizontalScrollBarVisible = ScrollBarVisibility.Hidden,
                VerticalScrollBarVisible = ScrollBarVisibility.Auto,
            };

            void AddCategory(string title)
            {
                nav.AddChild(new Label()
                {
                    Text = title.ToUpperInvariant(),
                    FontSize = 13,
                    TextColor = new Binding<Color>(() => Theme.OnSurfaceVariant),
                    Margin = new Thickness(10, nav.Children.Count == 0 ? 0 : 20, 10, 6),
                });
            }

            Border AddPage(string title, Func<UIComponent> factory, bool selectNow = false)
            {
                var wrapper = new Border()
                {
                    CornerRadius = new CornerRadius(14),
                    BorderThickness = new Thickness(0),
                    BorderColor = new Binding<Color>(() => Theme.Primary),
                    Background = Color.Transparent,
                    Margin = new Thickness(3, 3),
                    Padding = new Thickness(2, 2, 2, 2),
                    HorizontalAlignment = HorizontalAlignmentType.Stretch,
                };

                var button = new Button()
                {
                    Text = title,
                    Height = 40,
                    // Deliberately no Padding here: Button.Padding insets its whole internal
                    // template (the rounded pill border1, not just the label), so any nonzero
                    // value here shrinks the visible pill away from this wrapper's selection
                    // ring, leaving a gap. Text breathing room comes from
                    // HorizontalTextAlignment + the template's own small built-in label padding.
                    HorizontalAlignment = HorizontalAlignmentType.Stretch,
                    HorizontalTextAlignment = HorizontalAlignmentType.Center,
                    VerticalTextAlignment = VerticalAlignmentType.Center,
                };

                button.OnActivate = (sender, uiEvents) =>
                {
                    SelectNavItem(wrapper);
                    contentHost.Content = factory();
                };

                wrapper.Content = button;
                nav.AddChild(wrapper);

                if (selectNow)
                {
                    SelectNavItem(wrapper);
                }

                return wrapper;
            }

            AddCategory("Get Started");
            AddPage("Welcome", () => new HelpPage_Welcome(), selectNow: true);
            AddPage("Theming", () => new HelpPage_Theme());

            AddCategory("Layout");
            AddPage("Panel", () => new HelpPage_Panel());
            AddPage("Grid", () => new HelpPage_Grid());
            AddPage("Dock Panel", () => new HelpPage_DockPanel());

            AddCategory("Input");
            AddPage("Button", () => new HelpPage_Button());
            AddPage("Form Fields", () => new HelpPage_FormFields());
            AddPage("Combo Box", () => new HelpPage_ComboBox());

            AddCategory("Display");
            AddPage("Label", () => new HelpPage_Label());
            AddPage("List View", () => new HelpPage_ListView());
            AddPage("Tree View", () => new HelpPage_TreeView());

            AddCategory("Navigation");
            AddPage("Tab Panel", () => new HelpPage_TabPanel());

            AddCategory("Data");
            AddPage("Property Editor", () => new HelpPage_PropertyEditor());

            return nav;
        }

        private void SelectNavItem(Border wrapper)
        {
            if (selectedNavItem != null)
            {
                selectedNavItem.BorderThickness = new Thickness(0);
            }

            selectedNavItem = wrapper;
            selectedNavItem.BorderThickness = new Thickness(2);
        }

    }
}
