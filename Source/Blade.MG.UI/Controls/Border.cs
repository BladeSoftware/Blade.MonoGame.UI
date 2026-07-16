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
            EnableCaching = true;
        }

        protected override int CacheStateHash => HashCode.Combine(base.CacheStateHash, BorderColor.Value, BorderThickness.Value, CornerRadius.Value, Elevation.Value);

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
                RenderShadow(context, parentTransform, cornerRadius);
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
                var spriteBatch = context.Renderer.BeginBatch(
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



        private void RenderShadow(UIContext context, Transform parentTransform, CornerRadius cornerRadius)
        {
            // depthStencilState: DepthStencilState.None - an ancestor Border (e.g. a selection
            // ring wrapping this one, a common pattern for list items) rounds its own corners by
            // zeroing the stencil buffer in its corner cutouts before rendering its Content, and
            // only restores it once that entire subtree (including this Border) has finished
            // rendering - see the corner-radius branch below and its "restore" pass after
            // base.RenderControl. Since a nested Border's own corner often sits well inside its
            // parent's corner cutout box (e.g. a couple of px of padding vs a dozen px of
            // radius), this shadow - drawn before this Border has even set up its own stencil
            // state, let alone restored anyone else's - would otherwise be silently discarded by
            // that still-active ancestor mask, even though it's nowhere near the ancestor's own
            // edge. The border/background *content* doesn't hit this because it's drawn (and,
            // for the stroke, drawn again after this Border's own restore) later, once this
            // Border's own corner state is what's active. The shadow has no such second chance,
            // so it opts out of the stencil test entirely and relies solely on the ordinary
            // rectangular AncestorClipBounds scissor clip below.
            var spriteBatch = context.Renderer.BeginBatch(transform: parentTransform, depthStencilState: DepthStencilState.None);

            var shadowRect = FinalRect with
            {
                X = FinalRect.X + Elevation.Value,
                Y = FinalRect.Y + Elevation.Value
            };

            // The foreground is drawn on top of this shadow afterward, cut to the same corner
            // radius, and its rounding erases the shadow everywhere within its own radius x
            // radius corner box - EXCEPT wherever the (diagonally offset) shadow's own rounded
            // corner still reaches. The worst-covered point in that box is *not* the sharp tip
            // (Right, Bottom) - that's the easiest point to cover, being closest to the shadow's
            // offset center - it's the two points where the foreground's curve meets its flat
            // edges, e.g. (Right - radius, Bottom): full `radius` pixels from the tip along the
            // edge, so still far from the shadow's center even after the offset. A radius shrink
            // that only accounts for the tip (e.g. radius - elevation) leaves that point
            // uncovered. Requiring the shadow's own corner circle to just reach that point and
            // solving distance(point, shadowCenter) = shadowRadius for shadowRadius (a quadratic
            // in shadowRadius) gives the minimum radius that closes the gap everywhere in the
            // box, not just at the tip; +1px on top avoids leaving a razor-thin, mostly
            // anti-aliased-away edge right at that minimum.
            float radius = cornerRadius.MaxRadius;
            float elevation = Elevation.Value;
            float minSafeShadowRadius = radius + 2f * elevation - MathF.Sqrt(2f * elevation * (radius + elevation));
            float shadowRadius = Math.Clamp(minSafeShadowRadius + 1f, 0f, radius);

            // layoutBounds has already been narrowed all the way down to (at most) this
            // control's own FinalRect by the routine parent-to-child "don't draw outside your
            // own box" clipping every level of the tree applies - so it can no longer tell
            // "this edge is just my own box" apart from "this edge is a real ancestor's
            // boundary", and clipping to it directly would always cut the shadow off exactly
            // at this control's own edge. Clip to AncestorClipBounds instead: it's maintained
            // separately (see UIContext.AncestorClipBounds) and only tightens at genuine
            // viewport containers (ScrollPanel/StackPanel/ListView/TreeView/Panel/
            // ExpansionPanel), so a free-floating control's shadow renders fully, while a
            // control flush against its scroll/stack panel's true edge still gets clipped
            // there.
            context.Renderer.ClipToRect(context.AncestorClipBounds);

            context.Renderer.FillRoundedRect(spriteBatch, shadowRect, shadowRadius, new Color(Theme.Shadow, 0.15f));
            context.Renderer.EndBatch();
        }

        private void RenderBorder(UIContext context, Rectangle layoutBounds, Transform parentTransform, CornerRadius cornerRadius, Thickness borderThickness)
        {
            var spriteBatch = context.Renderer.BeginBatch(transform: parentTransform);
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
            var spriteBatch = context.Renderer.BeginBatch(
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
