using Blade.MG.UI.Components;
using Microsoft.Xna.Framework;

namespace Blade.MG.UI.Controls.Templates
{
    public class TabHeaderTemplate : Control
    {
        public Label label1;
        private Label label2;
        private Border tabBorder;
        private StackPanel container;

        public TabHeaderTemplate()
        {
        }

        protected override void InitTemplate()
        {
            base.InitTemplate();


            string item = DataContext?.ToString() ?? "null";

            IsHitTestVisible = true;

            //Margin = new Thickness(1, 1, 3, 2);
            //Background = Theme.SecondaryContainer;

            //Margin = new Thickness(0, 0, -1, 0); // Overlap tabs slightly for connected look
            Margin = new Thickness(0, 0, 0, 0);
            Background = Color.Transparent;


            HorizontalAlignment = HorizontalAlignmentType.Left;
            VerticalAlignment = VerticalAlignmentType.Top;
            //HorizontalContentAlignment = HorizontalAlignmentType.Left;
            //VerticalContentAlignment = VerticalAlignmentType.Center;

            label1 = new Label()
            {
                //Height = "32px",
                Text = item,
                TextColor = Theme.OnSurface,
                Background = Color.Transparent,
                Padding = new Thickness(12, 6, 8, 6),

                //TextColor = Theme.OnSecondaryContainer,
                //Background = Color.Transparent,
                //Padding = new Thickness(4, 2),
                //Width = 100,
                //Height = 100,
                //Margin = new Thickness(10, 0, 0, 0),
                HorizontalAlignment = HorizontalAlignmentType.Left,
                VerticalAlignment = VerticalAlignmentType.Center,
                IsHitTestVisible = false

                //SpriteFont = button.SpriteFont // Use the Button Font
            };

            //label2 = new Label()
            //{
            //    Margin = new Thickness(1),
            //    Width = "16px",
            //    Height = "16px",
            //    Text = "X",
            //    TextColor = Theme.OnSecondaryContainer,
            //    Background = Theme.SecondaryContainer,
            //    HorizontalAlignment = HorizontalAlignmentType.Left,
            //    VerticalAlignment = VerticalAlignmentType.Top,
            //    HorizontalTextAlignment = HorizontalAlignmentType.Center,
            //    VerticalTextAlignment = VerticalAlignmentType.Center,

            //    IsHitTestVisible = true,
            //    Name = "CloseTab1",

            //    OnHoverChanged = (args) =>
            //    {
            //        if (args.Hover)
            //        {
            //            label2.Background = Theme.TertiaryContainer;
            //        }
            //        else
            //        {
            //            label2.Background = Theme.SecondaryContainer;
            //        }

            //        //args.Handled = true;
            //    },

            //    OnPrimaryClick = (sender, args) =>
            //    {
            //        // Use Selected Tab Page rather than Focus
            //        var parentTabPanel = FindParent<TabPanel>();

            //        parentTabPanel.CloseTab(this);
            //        args.Handled = true;
            //    }
            //};

            label2 = new Label()
            {
                Margin = new Thickness(0, 0, 8, 0),
                Width = "16px",
                Height = "16px",
                //Text = "✕", // Using a nicer × character
                Text = "X", // Using a nicer × character
                TextColor = Theme.OnSurface * 0.7f, // Slightly transparent
                Background = Color.Transparent,
                HorizontalAlignment = HorizontalAlignmentType.Center,
                VerticalAlignment = VerticalAlignmentType.Center,
                HorizontalTextAlignment = HorizontalAlignmentType.Center,
                VerticalTextAlignment = VerticalAlignmentType.Center,
                IsHitTestVisible = true,
                CanHover = false,
                Visible = Visibility.Hidden, // Hidden by default
                Name = "CloseTab",

                OnHoverChanged = (args) =>
                {
                    if (args.Hover)
                    {
                        label2.Background = new Color(255, 255, 255, 30);
                        label2.TextColor = Theme.OnSurface;
                    }
                    else
                    {
                        label2.Background = Color.Transparent;
                        label2.TextColor = Theme.OnSurface * 0.7f;
                    }
                },

                OnPrimaryClick = (sender, args) =>
                {
                    var parentTabPanel = FindParent<TabPanel>();
                    parentTabPanel?.CloseTab(this);
                    args.Handled = true;
                }
            };


            // Use the Parent Button's ContentAlignment values for the lable text placement
            //label1.HorizontalContentAlignment = HorizontalContentAlignment;
            //label1.VerticalContentAlignment = VerticalContentAlignment;



            var container = new StackPanel()
            {
                IsHitTestVisible = false,
                HorizontalScrollBarVisible = ScrollBarVisibility.Hidden,
                VerticalScrollBarVisible = ScrollBarVisibility.Hidden,
                Padding = new Thickness(0)
            };

            container.AddChild(label1);
            container.AddChild(label2);

            // Wrap in a border for rounded corners and better styling
            tabBorder = new Border()
            {
                Background = Theme.Surface,
                BorderThickness = new Thickness(1, 1, 1, 0), // No bottom border
                //BorderThickness = new Thickness(1),
                BorderColor = Theme.SurfaceVariant,
                CornerRadius = new CornerRadius(6, 6, 0, 0), // Rounded top corners
                //CornerRadius = new CornerRadius(6),
                IsHitTestVisible = false
            };

            tabBorder.Content = container;
            Content = tabBorder;

            //Content = container;

        }

        // ---=== Handle State Changes ===---

        //public override async Task HandleFocusChangedEventAsync(UIWindow uiWindow, UIFocusChangedEvent uiEvent)
        //{
        //    HasFocus = uiEvent.Focused;

        //    await base.HandleFocusChangedEventAsync(uiWindow, uiEvent);
        //    StateHasChanged();
        //}

        //public override async Task HandleHoverChangedAsync(UIWindow uiWindow, UIHoverChangedEvent uiEvent)
        //{
        //    MouseHover = uiEvent.Hover;

        //    await base.HandleHoverChangedAsync(uiWindow, uiEvent);
        //    StateHasChanged();
        //}

        //public override Task HandleClickEventAsync(UIWindow uiWindow, UIClickEvent uiEvent)
        //{
        //    var parentTabPanel = FindParent<TabPanel>();
        //    parentTabPanel?.SetActiveTab(this);
        //    uiEvent.Handled = true;
        //
        //    return base.HandleClickEventAsync(uiWindow, uiEvent);
        //}

        protected override void HandleStateChange()
        {
            base.HandleStateChange();

            //var label = Content as Label;
            //var label = label1;


            // Use Selected Tab Page rather than Focus
            var parentTabPanel = FindParent<TabPanel>();
            bool mouseHover = MouseHover.Value;
            bool activeTab = parentTabPanel?.IsActiveTab(this) ?? false;

            // Show close button on hover or when active
            if (label2 != null)
            {
                label2.Visible = (mouseHover || activeTab) ? Visibility.Visible : Visibility.Hidden;
            }

            if (tabBorder == null) return;

            if (activeTab)
            {
                // Active tab - bright and prominent
                tabBorder.Background = Theme.Surface;
                tabBorder.BorderColor = Theme.Primary;
                tabBorder.BorderThickness = new Thickness(1, 2, 1, 0); // Thicker top border
                label1.TextColor = Theme.OnSurface;
            }
            else if (mouseHover)
            {
                // Hovered tab - slightly highlighted
                tabBorder.Background = new Color(
                    (int)(Theme.Surface.R * 0.95f),
                    (int)(Theme.Surface.G * 0.95f),
                    (int)(Theme.Surface.B * 0.95f),
                    255
                );
                tabBorder.BorderColor = Theme.SurfaceVariant;
                tabBorder.BorderThickness = new Thickness(1, 1, 1, 0);
                label1.TextColor = Theme.OnSurface;
            }
            else
            {
                // Inactive tab - subtle and recessed
                tabBorder.Background = new Color(
                    (int)(Theme.Surface.R * 0.85f),
                    (int)(Theme.Surface.G * 0.85f),
                    (int)(Theme.Surface.B * 0.85f),
                    255
                );
                tabBorder.BorderColor = new Color(Theme.SurfaceVariant.R, Theme.SurfaceVariant.G, Theme.SurfaceVariant.B, (byte)100);
                tabBorder.BorderThickness = new Thickness(1, 1, 1, 0);
                label1.TextColor = Theme.OnSurface * 0.7f;
            }

            //----

            //if (mouseHover && activeTab)
            //{
            //    Background = Color.SlateBlue;
            //    label.TextColor = Color.LightBlue;
            //}
            //else if (mouseHover)
            //{
            //    Background = Color.SlateBlue; //Theme.SurfaceVariant;
            //    label.TextColor = Color.White;
            //}
            //else if (activeTab)
            //{
            //    Background = Color.DarkSlateBlue;
            //    label.TextColor = Color.White;
            //}
            //else
            //{
            //    // Default state
            //    Background = Color.LightGray;
            //    label.TextColor = Color.Black;
            //}

        }

    }
}
