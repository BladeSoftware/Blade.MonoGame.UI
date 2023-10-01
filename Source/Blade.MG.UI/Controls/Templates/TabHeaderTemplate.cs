using Blade.MG.UI.Components;
using Microsoft.Xna.Framework;

namespace Blade.MG.UI.Controls.Templates
{
    public class TabHeaderTemplate : Control
    {
        public Label label1;
        private Label label2;

        public TabHeaderTemplate()
        {
        }

        protected override void InitTemplate()
        {
            base.InitTemplate();


            string item = DataContext?.ToString() ?? "null";

            HitTestVisible = true;

            Margin = new Thickness(1, 1, 3, 2);
            Background = Theme.SecondaryContainer;

            HorizontalAlignment = HorizontalAlignmentType.Left;
            VerticalAlignment = VerticalAlignmentType.Top;
            HorizontalContentAlignment = HorizontalAlignmentType.Left;
            VerticalContentAlignment = VerticalAlignmentType.Center;

            label1 = new Label()
            {
                Height = "24px",
                Text = item,
                TextColor = Theme.OnSecondaryContainer,
                //Background = Color.Transparent,
                Padding = new Thickness(4, 2),
                //Width = 100,
                //Height = 100,
                //Margin = new Thickness(10, 0, 0, 0),
                HorizontalAlignment = HorizontalAlignmentType.Left,
                VerticalAlignment = VerticalAlignmentType.Center

                //SpriteFont = button.SpriteFont // Use the Button Font
            };

            label2 = new Label()
            {
                Margin = new Thickness(1),
                Width = "16px",
                Height = "16px",
                Text = "X",
                TextColor = Theme.OnSecondaryContainer,
                Background = Theme.SecondaryContainer,
                HorizontalAlignment = HorizontalAlignmentType.Left,
                VerticalAlignment = VerticalAlignmentType.Top,

                HitTestVisible = true,
                Name = "CloseTab1",

                OnHoverChanged = (args) =>
                {
                    if (args.Hover)
                    {
                        label2.Background = Theme.TertiaryContainer;
                    }
                    else
                    {
                        label2.Background = Theme.SecondaryContainer;
                    }

                    //args.Handled = true;
                },

                OnClick = (sender, args) =>
                {
                    // Use Selected Tab Page rather than Focus
                    var parentTabPanel = FindParent<TabPanel>();

                    parentTabPanel.CloseTab(this);
                    args.Handled = true;
                }
            };


            // Use the Parent Button's ContentAlignment values for the lable text placement
            //label1.HorizontalContentAlignment = HorizontalContentAlignment;
            //label1.VerticalContentAlignment = VerticalContentAlignment;



            var container = new StackPanel()
            {
                HitTestVisible = false,
                HorizontalScrollBarVisible = false,
                VerticalScrollBarVisible = false
            };

            container.AddChild(label1);
            container.AddChild(label2);

            //Content = label1;
            Content = container;


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
            var label = label1;


            // Use Selected Tab Page rather than Focus
            var parentTabPanel = FindParent<TabPanel>();

            bool mouseHover = MouseHover.Value;
            bool activeTab = parentTabPanel?.IsActiveTab(this) ?? false;


            if (mouseHover && activeTab)
            {
                Background = Color.SlateBlue;
                label.TextColor = Color.LightBlue;
            }
            else if (mouseHover)
            {
                Background = Color.SlateBlue; //Theme.SurfaceVariant;
                label.TextColor = Color.White;
            }
            else if (activeTab)
            {
                Background = Color.DarkSlateBlue;
                label.TextColor = Color.White;
            }
            else
            {
                // Default state
                Background = Color.LightGray;
                label.TextColor = Color.Black;
            }

        }

    }
}
