using Blade.MG.UI.Components;
using Blade.MG.UI.Events;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blade.MG.UI.Controls.Templates
{
    public class CheckBoxTemplate : Control
    {
        //private CheckBox checkBox;
        private Label label1;
        //private Border border1;

        private Color backgroundNormal = Color.White;
        private Color backgroundHover = Color.LightGray;
        private Color backgroundFocused = Color.White;

        private Color textColorNormal = Color.Black;
        private Color textColorHover = Color.Black;
        private Color textColorFocused = Color.Black;

        //private Color borderColorNormal = Color.DarkSlateBlue;
        //private Color borderColorHover = Color.White;  // Color.Gray;
        //private Color borderColorFocused = Color.MediumSlateBlue;

        //private int borderThicknessNormal = 2;
        //private int borderThicknessHover = 2;
        //private int borderThicknessFocused = 2;


        public CheckBoxTemplate()
        {
        }

        protected override void InitTemplate()
        {
            base.InitTemplate();

            var checkbox = ParentAs<CheckBox>();


            label1 = new Label()
            {
                Text = checkbox.Text, // Use the Button Text
                //TextColor = textBox.FontColor,  // Use the Button Foreground Color
                FontName = checkbox.FontName, // Use the parent Font
                FontSize = checkbox.FontSize, // Use the parent Font
                Margin = new Thickness(5, 0, 0, 0)
            };

            // Use the Parent Button's ContentAlignment values for the lable text placement
            label1.HorizontalContentAlignment = HorizontalContentAlignment;
            label1.VerticalContentAlignment = VerticalContentAlignment;


            Content = label1;
        }

        // ---=== Handle State Changes ===---

        public override async Task HandleFocusChangedEventAsync(UIWindow uiWindow, UIFocusChangedEvent uiEvent)
        {
            await base.HandleFocusChangedEventAsync(uiWindow, uiEvent);

            StateHasChanged();
        }

        public override async Task HandleHoverChangedAsync(UIWindow uiWindow, UIHoverChangedEvent uiEvent)
        {
            await base.HandleHoverChangedAsync(uiWindow, uiEvent);

            StateHasChanged();
        }

        protected override void HandleStateChange()
        {
            //await base.HandleStateChangeAsync();

            // Normal State
            label1.TextColor.Value = textColorNormal;
            Background.Value = backgroundNormal;

            // Hover State 
            if (MouseHover.Value)
            {
                label1.TextColor.Value = textColorHover;
                Background.Value = backgroundHover;
            }

            // Focused State
            if (HasFocus.Value)
            {
                label1.TextColor.Value = textColorFocused;
                Background.Value = backgroundFocused;
            }


        }

        private RenderTarget2D img;
        private Rectangle boxRect;

        public override void Measure(UIContext context, ref Size availableSize, ref Layout parentMinMax)
        {
            base.Measure(context, ref availableSize, ref parentMinMax);
        }

        public override void Arrange(UIContext context, Rectangle layoutBounds, Rectangle parentLayoutBounds)
        {
            base.Arrange(context, layoutBounds, parentLayoutBounds);

            int boxWidth = Math.Min(FinalRect.Width, FinalRect.Height);
            int boxHeight = boxWidth;
            int padding = 4;

            boxRect = new Rectangle { X = FinalRect.X + 2, Y = (FinalRect.Top + FinalRect.Bottom) / 2 - boxHeight / 2, Width = boxWidth, Height = boxHeight };

            //if (img == null)
            //{
            //    var svg = new SVGDocument();
            //    var path = svg.AddPath("M1837 557L768 1627l-557-558 90-90 467 466 979-978 90 90z");
            //    path.Fill = new SVGFill { FillColor = Color.Red, FillRule = FillRule.Nonzero };
            //    img = svg.ToTexture2D(context.Game.GraphicsDevice, Matrix.CreateScale(1f), new Point(boxWidth, boxHeight), padding);
            //}

        }

        public override void RenderControl(UIContext context, Rectangle layoutBounds, Transform parentTransform)
        {
            base.RenderControl(context, layoutBounds, parentTransform);

            var checkbox = ParentAs<CheckBox>();

            try
            {
                using var spriteBatch = context.Renderer.BeginBatch(transform: parentTransform);
                context.Renderer.ClipToRect(layoutBounds);

                context.Renderer.FillRoundedRect(spriteBatch, boxRect, boxRect.Width / 4, Color.LightGray);
                context.Renderer.DrawRoundedRect(spriteBatch, boxRect, boxRect.Width / 4, Color.Black, 2f);

                if (checkbox.IsChecked?.Value == null)
                {
                    // Indeterminate
                    var insideRect = boxRect;
                    insideRect.Inflate(-3f, -3f);

                    context.Renderer.FillRoundedRect(spriteBatch, insideRect, 4, new Color(Color.Black, 1f));
                }
                else if (checkbox.IsChecked?.Value == true)
                {
                    // Checked
                    if (img != null)
                    {
                        spriteBatch.Draw(img, boxRect, Color.Red);
                    }
                }


            }
            finally
            {
                context.Renderer.EndBatch();
            }

        }

    }
}
