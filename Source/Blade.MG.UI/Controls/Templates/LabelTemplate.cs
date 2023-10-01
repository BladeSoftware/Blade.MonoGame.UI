using Blade.MG.UI.Components;
using Blade.MG.UI.Events;
using FontStashSharp;
using Microsoft.Xna.Framework;

namespace Blade.MG.UI.Controls.Templates
{
    public class LabelTemplate : Control
    {

        public LabelTemplate()
        {
        }

        protected override void InitTemplate()
        {
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

        }

        public override void RenderControl(UIContext context, Rectangle layoutBounds, Transform parentTransform)
        {
            base.RenderControl(context, layoutBounds, parentTransform);

            var label = ParentAs<Label>();

            try
            {
                context.Renderer.BeginBatch(transform: parentTransform); // Transform.Combine(parentTransform, Transform)

                if (Background.Value != Color.Transparent)
                {
                    ////context.Renderer.FillRect(FinalRect, Background.Value, layoutBounds);
                    context.Renderer.FillRect(FinalContentRect, Background.Value, layoutBounds);
                }

                SpriteFontBase font = context.FontService.GetFontOrDefault(label.FontName?.Value, label.FontSize?.Value);
                context.Renderer.DrawString(FinalContentRect, label.Text.ToString(), font, label.TextColor?.Value, HorizontalContentAlignment.Value, VerticalContentAlignment.Value, Rectangle.Intersect(layoutBounds, FinalContentRect));

                //context.Renderer.DrawString(FinalContentRect, Text.ToString(), SpriteFont?.Value, TextColor?.Value, HorizontalContentAlignment.Value, VerticalContentAlignment.Value, Rectangle.Intersect(layoutBounds, FinalContentRect));
            }
            finally
            {
                context.Renderer.EndBatch();
            }

        }

    }
}
