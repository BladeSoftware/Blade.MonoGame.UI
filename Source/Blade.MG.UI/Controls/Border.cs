using Blade.MG.Primitives;
using Blade.MG.UI.Components;
using Blade.MG.UI.Renderer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Blade.MG.UI.Controls
{
    public class Border : Control
    {
        public Binding<Color> BorderColor { get; set; } = new Color();
        public Binding<Thickness> BorderThickness { get; set; } = new Thickness();
        public Binding<CornerRadius> CornerRadius { get; set; } = new CornerRadius();

        public Binding<int> Elevation { get; set; } = 0;


        public Border()
        {
            BorderColor.Value = Color.White;
            BorderThickness.Value = 1;
            CornerRadius.Value = 1f;
            Elevation.Value = 0;

            HorizontalAlignment = HorizontalAlignmentType.Stretch;
            VerticalAlignment = VerticalAlignmentType.Stretch;

            IsHitTestVisible = false;
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
            var cornerRadius = CornerRadius.Value;
            var borderThickness = BorderThickness.Value;
            bool hasCornerRadius = cornerRadius.HasRadius;
            bool hasBorder = borderThickness.HasThickness && BorderColor.Value != Color.Transparent;

            var tmpRect = FinalRect;

            // Draw the shadow if Elevation is > 0
            if (Elevation.Value > 0)
            {
                RenderShadow(context, layoutBounds, parentTransform, cornerRadius);
            }

            // If we have rounded corners, then we need to draw the stencil mask
            // Use the inner rectangle (accounting for border thickness) for proper clipping
            if (hasCornerRadius)
            {
                // DrawStencil(context, FinalRect, cornerRadius);

                // Calculate inner rectangle and inner corner radius
                Rectangle innerRect = new(
                    (int)(FinalRect.X + borderThickness.Left),
                    (int)(FinalRect.Y + borderThickness.Top),
                    (int)(FinalRect.Width - borderThickness.Left - borderThickness.Right),
                    (int)(FinalRect.Height - borderThickness.Top - borderThickness.Bottom)
                );

                CornerRadius innerRadius = new(
                    Math.Max(0, cornerRadius.TopLeft - Math.Max(borderThickness.Left, borderThickness.Top)),
                    Math.Max(0, cornerRadius.TopRight - Math.Max(borderThickness.Right, borderThickness.Top)),
                    Math.Max(0, cornerRadius.BottomRight - Math.Max(borderThickness.Right, borderThickness.Bottom)),
                    Math.Max(0, cornerRadius.BottomLeft - Math.Max(borderThickness.Left, borderThickness.Bottom))
                );

                DrawStencil(context, innerRect, innerRadius);
                //DrawStencil(context, innerRect, innerRadius);

                // Update FinalRect to inner rectangle for content rendering
                FinalRect = innerRect;
            }

            // Render the Contents
            base.RenderControl(context, layoutBounds, parentTransform);

            // Restore FinalRect
            FinalRect = tmpRect;

            // If we have rounded corners, then we need to restore the stencil mask
            if (hasCornerRadius)
            {
                using var spriteBatch = context.Renderer.BeginBatch(
                    depthStencilState: UIRenderer.stencilStateReplaceAlways,
                    blendState: UIRenderer.blendStateStencilOnly,
                    transform: null);
                context.Renderer.FillRect(spriteBatch, FinalRect, Color.White);
                context.Renderer.EndBatch();
            }

            // Render the border
            if (hasBorder)
            {
                RenderBorder(context, layoutBounds, parentTransform, cornerRadius, borderThickness);
            }
        }



        private void RenderShadow(UIContext context, Rectangle layoutBounds, Transform parentTransform, CornerRadius cornerRadius)
        {
            using var spriteBatch = context.Renderer.BeginBatch(transform: parentTransform);
            context.Renderer.ClipToRect(layoutBounds);

            var shadowRect = FinalRect with
            {
                X = FinalRect.X + Elevation.Value,
                Y = FinalRect.Y + Elevation.Value
            };

            // Use max corner radius for shadow (simplified)
            context.Renderer.FillRoundedRect(spriteBatch, shadowRect, cornerRadius.MaxRadius, new Color(Color.LightGray, 0.35f));
            context.Renderer.EndBatch();
        }

        private void RenderBorder(UIContext context, Rectangle layoutBounds, Transform parentTransform, CornerRadius cornerRadius, Thickness borderThickness)
        {
            using var spriteBatch = context.Renderer.BeginBatch(transform: parentTransform);
            context.Renderer.ClipToRect(layoutBounds);

            if (!cornerRadius.HasRadius)
            {
                // Fast path: no corner radius - draw simple rectangles
                DrawSimpleBorder(spriteBatch, FinalRect, borderThickness, BorderColor.Value);
            }
            else if (cornerRadius.IsUniform && borderThickness.IsUniform)
            {
                // Fast path: uniform corner radius and border thickness
                context.Renderer.DrawRoundedRect(spriteBatch, FinalRect, cornerRadius.TopLeft, BorderColor.Value, borderThickness.Uniform);
            }
            else
            {
                // Complex path: draw each side separately with different corner radii
                DrawBorderEdges(spriteBatch, FinalRect, cornerRadius, borderThickness, BorderColor.Value);
            }

            context.Renderer.EndBatch();
        }

        /// <summary>
        /// Draws a simple border without rounded corners (optimized for radius = 0)
        /// </summary>
        private static void DrawSimpleBorder(SpriteBatch spriteBatch, Rectangle rect, Thickness bt, Color color)
        {
            var pixel = Primitives2D.PixelTexture(spriteBatch.GraphicsDevice);

            // Top edge (full width)
            if (bt.Top > 0)
            {
                spriteBatch.Draw(pixel, new Rectangle(rect.Left, rect.Top, rect.Width, (int)bt.Top), color);
            }

            // Bottom edge (full width)
            if (bt.Bottom > 0)
            {
                spriteBatch.Draw(pixel, new Rectangle(rect.Left, (int)(rect.Bottom - bt.Bottom), rect.Width, (int)bt.Bottom), color);
            }

            // Left edge (middle section only, excluding top/bottom overlap)
            if (bt.Left > 0)
            {
                spriteBatch.Draw(pixel, new Rectangle(rect.Left, (int)(rect.Top + bt.Top), (int)bt.Left, (int)(rect.Height - bt.Top - bt.Bottom)), color);
            }

            // Right edge (middle section only, excluding top/bottom overlap)
            if (bt.Right > 0)
            {
                spriteBatch.Draw(pixel, new Rectangle((int)(rect.Right - bt.Right), (int)(rect.Top + bt.Top), (int)bt.Right, (int)(rect.Height - bt.Top - bt.Bottom)), color);
            }
        }

        private static void DrawBorderEdges(SpriteBatch spriteBatch, Rectangle rect, CornerRadius cr, Thickness bt, Color color)
        {
            var pixel = Primitives2D.PixelTexture(spriteBatch.GraphicsDevice);

            // Top edge (full width to cover corners)
            if (bt.Top > 0)
            {
                var topRect = new Rectangle(
                    rect.Left,
                    rect.Top,
                    rect.Width,
                    (int)bt.Top);
                spriteBatch.Draw(pixel, topRect, color);
            }

            // Bottom edge (full width to cover corners)
            if (bt.Bottom > 0)
            {
                var bottomRect = new Rectangle(
                    rect.Left,
                    (int)(rect.Bottom - bt.Bottom),
                    rect.Width,
                    (int)bt.Bottom);
                spriteBatch.Draw(pixel, bottomRect, color);
            }

            // Left edge (middle section only, excluding top/bottom border overlap)
            if (bt.Left > 0)
            {
                var leftRect = new Rectangle(
                    rect.Left,
                    (int)(rect.Top + Math.Max(cr.TopLeft, bt.Top)),
                    (int)bt.Left,
                    (int)(rect.Height - Math.Max(cr.TopLeft, bt.Top) - Math.Max(cr.BottomLeft, bt.Bottom)));
                spriteBatch.Draw(pixel, leftRect, color);
            }

            // Right edge (middle section only, excluding top/bottom border overlap)
            if (bt.Right > 0)
            {
                var rightRect = new Rectangle(
                    (int)(rect.Right - bt.Right),
                    (int)(rect.Top + Math.Max(cr.TopRight, bt.Top)),
                    (int)bt.Right,
                    (int)(rect.Height - Math.Max(cr.TopRight, bt.Top) - Math.Max(cr.BottomRight, bt.Bottom)));
                spriteBatch.Draw(pixel, rightRect, color);
            }

            // Draw corner arcs (only if corners have radius)
            if (cr.TopLeft > 0)
                Primitives2D.DrawArc(spriteBatch, new Vector2(rect.Left + cr.TopLeft, rect.Top + cr.TopLeft), cr.TopLeft, MathHelper.Pi, MathHelper.PiOver2, color, bt.Top);
            if (cr.TopRight > 0)
                Primitives2D.DrawArc(spriteBatch, new Vector2(rect.Right - cr.TopRight, rect.Top + cr.TopRight), cr.TopRight, -MathHelper.PiOver2, MathHelper.PiOver2, color, bt.Top);
            if (cr.BottomRight > 0)
                Primitives2D.DrawArc(spriteBatch, new Vector2(rect.Right - cr.BottomRight, rect.Bottom - cr.BottomRight), cr.BottomRight, 0, MathHelper.PiOver2, color, bt.Bottom);
            if (cr.BottomLeft > 0)
                Primitives2D.DrawArc(spriteBatch, new Vector2(rect.Left + cr.BottomLeft, rect.Bottom - cr.BottomLeft), cr.BottomLeft, MathHelper.PiOver2, MathHelper.PiOver2, color, bt.Bottom);
        }

        /// <summary>
        /// Use a Depth Stencil to render the border background with rounded corners
        /// </summary>
        private void DrawStencil(UIContext context, Rectangle rectangle, CornerRadius cornerRadius)
        {
            using var spriteBatch = context.Renderer.BeginBatch(
                depthStencilState: UIRenderer.stencilStateZeroAlways,
                blendState: UIRenderer.blendStateStencilOnly,
                transform: null);

            // Draw smooth rounded rectangle mask for each corner
            DrawSmoothCornerMask(spriteBatch, rectangle, cornerRadius);
            context.Renderer.EndBatch();
        }

        /// <summary>
        /// Creates a smooth stencil mask using filled rounded rectangles for the corner areas
        /// </summary>
        private static void DrawSmoothCornerMask(SpriteBatch spriteBatch, Rectangle rect, CornerRadius radius)
        {
            // Calculate the area outside each rounded corner that needs to be masked
            float maxRadius = radius.MaxRadius;

            if (maxRadius <= 0) return;

            // Top-Left Corner
            if (radius.TopLeft > 0)
            {
                Rectangle cornerArea = new(rect.Left, rect.Top, (int)radius.TopLeft, (int)radius.TopLeft);
                FillCornerMask(spriteBatch, cornerArea, radius.TopLeft, CornerPosition.TopLeft);
            }

            // Top-Right Corner
            if (radius.TopRight > 0)
            {
                Rectangle cornerArea = new((int)(rect.Right - radius.TopRight), rect.Top, (int)radius.TopRight, (int)radius.TopRight);
                FillCornerMask(spriteBatch, cornerArea, radius.TopRight, CornerPosition.TopRight);
            }

            // Bottom-Left Corner
            if (radius.BottomLeft > 0)
            {
                Rectangle cornerArea = new(rect.Left, (int)(rect.Bottom - radius.BottomLeft), (int)radius.BottomLeft, (int)radius.BottomLeft);
                FillCornerMask(spriteBatch, cornerArea, radius.BottomLeft, CornerPosition.BottomLeft);
            }

            // Bottom-Right Corner
            if (radius.BottomRight > 0)
            {
                Rectangle cornerArea = new((int)(rect.Right - radius.BottomRight), (int)(rect.Bottom - radius.BottomRight), (int)radius.BottomRight, (int)radius.BottomRight);
                FillCornerMask(spriteBatch, cornerArea, radius.BottomRight, CornerPosition.BottomRight);
            }
        }

        private enum CornerPosition { TopLeft, TopRight, BottomLeft, BottomRight }

        /// <summary>
        /// Fills the corner mask area outside the rounded corner
        /// </summary>
        private static void FillCornerMask(SpriteBatch spriteBatch, Rectangle cornerArea, float radius, CornerPosition position)
        {
            var pixel = Primitives2D.PixelTexture(spriteBatch.GraphicsDevice);

            // Calculate center of the arc based on corner position
            Vector2 center = position switch
            {
                CornerPosition.TopLeft => new Vector2(cornerArea.Right, cornerArea.Bottom),
                CornerPosition.TopRight => new Vector2(cornerArea.Left, cornerArea.Bottom),
                CornerPosition.BottomLeft => new Vector2(cornerArea.Right, cornerArea.Top),
                CornerPosition.BottomRight => new Vector2(cornerArea.Left, cornerArea.Top),
                _ => Vector2.Zero
            };

            // Draw each pixel in the corner area and check if it's outside the arc
            for (int y = 0; y < cornerArea.Height; y++)
            {
                for (int x = 0; x < cornerArea.Width; x++)
                {
                    Vector2 pixelPos = new(cornerArea.X + x + 0.5f, cornerArea.Y + y + 0.5f);
                    float distance = Vector2.Distance(pixelPos, center);

                    // If pixel is outside the radius, mask it
                    if (distance > radius)
                    {
                        spriteBatch.Draw(pixel, new Vector2(cornerArea.X + x, cornerArea.Y + y), Color.White);
                    }
                }
            }
        }

    }
}
