﻿using Blade.MG.UI.Components;
using Blade.MG.UI.Events;
using Blade.MG.UI.Services;
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
                using var spriteBatch = context.Renderer.BeginBatch(transform: parentTransform); // Transform.Combine(parentTransform, Transform)

                if (Background.Value != Color.Transparent)
                {
                    context.Renderer.FillRect(spriteBatch, FinalContentRect, Background.Value, layoutBounds);
                }

                SpriteFontBase font = FontService.GetFontOrDefault(label.FontName?.Value, label.FontSize?.Value);

                string labelText = label?.Text?.ToString() ?? "";

                var textDimensions = context.Renderer.DrawString(spriteBatch, FinalContentRect, labelText, font, label.TextColor?.Value, label.HorizontalTextAlignment.Value, label.VerticalTextAlignment.Value, Rectangle.Intersect(layoutBounds, FinalContentRect));

                label.TextRect = textDimensions.TextRect;
                label.TextBaseLine = textDimensions.Baseline;
            }
            finally
            {
                context.Renderer.EndBatch();
            }

        }

    }
}
