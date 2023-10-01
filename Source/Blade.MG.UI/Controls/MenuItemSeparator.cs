using Blade.MG.UI.Components;
using Blade.MG.UI.Events;
using Microsoft.Xna.Framework;

namespace Blade.MG.UI.Controls
{
    public class MenuItemSeparator : Control
    {

        public MenuItemSeparator()
        {
            HorizontalAlignment = HorizontalAlignmentType.Stretch;
            VerticalAlignment = VerticalAlignmentType.Stretch;
            HorizontalContentAlignment = HorizontalAlignmentType.Center;
            VerticalContentAlignment = VerticalAlignmentType.Center;

            HitTestVisible = true;
            IsTabStop = false;

            Padding = new Thickness(16, 0, 5, 0);
            //Margin = new Thickness(0, 10);


        }

        protected override void InitTemplate()
        {
            base.InitTemplate();

            //Content = new MenuItemTemplate();
        }

        public override void Measure(UIContext context, ref Size availableSize, ref Layout parentMinMax)
        {
            base.Measure(context, ref availableSize, ref parentMinMax);
        }

        public override void Arrange(UIContext context, Rectangle layoutBounds, Rectangle parentLayoutBounds)
        {
            base.Arrange(context, layoutBounds, parentLayoutBounds);
        }

        public override void RenderControl(UIContext context, Rectangle layoutBounds, Transform parentTransform)
        {
            base.RenderControl(context, layoutBounds, parentTransform);

            try
            {
                context.Renderer.BeginBatch(transform: parentTransform); // Transform.Combine(parentTransform, Transform)

                context.Renderer.DrawRect(FinalContentRect, Color.LightGray);

                //if (this.Background.Value != Color.Transparent)
                //{
                //    context.Renderer.FillRect(FinalRect, Background.Value, layoutBounds);
                //}

                //SpriteFontBase font = context.FontService.GetFontOrDefault(FontName?.Value, FontSize?.Value);
                //context.Renderer.DrawString(FinalContentRect, Text.ToString(), font, TextColor?.Value, HorizontalContentAlignment.Value, VerticalContentAlignment.Value, Rectangle.Intersect(layoutBounds, FinalContentRect));

                ////context.Renderer.DrawString(FinalContentRect, Text.ToString(), SpriteFont?.Value, TextColor?.Value, HorizontalContentAlignment.Value, VerticalContentAlignment.Value, Rectangle.Intersect(layoutBounds, FinalContentRect));
            }
            finally
            {
                context.Renderer.EndBatch();
            }

        }

        // ---=== UI Events ===---
        public override async Task HandleClickEventAsync(UIWindow uiWindow, UIClickEvent uiEvent)
        {
            if (!FinalRect.Contains(uiEvent.X, uiEvent.Y))
            {
                return;
            }

            uiEvent.Handled = true;

            await Task.CompletedTask;
        }

    }
}
