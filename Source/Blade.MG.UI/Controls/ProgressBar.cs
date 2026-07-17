using Blade.MG.UI.Components;
using Microsoft.Xna.Framework;

namespace Blade.MG.UI.Controls
{
    public class ProgressBar : Control
    {
        public Binding<float> Minimum { get; set; } = 0f;
        public Binding<float> Maximum { get; set; } = 100f;
        public Binding<float> Value { get; set; } = 0f;

        // Animated sweeping highlight instead of a fixed fill, for progress that has no known
        // completion fraction (e.g. an in-flight network request).
        public Binding<bool> IsIndeterminate { get; set; } = false;

        // Wall-clock based, matching PropertyAnimation's own "evaluate on demand from DateTime.Now"
        // approach (see Animations/PropertyAnimation.cs) rather than needing a per-frame Update
        // hook - a repeating sweep doesn't need per-tick state, just the elapsed time so far.
        private readonly DateTime indeterminateStart = DateTime.UtcNow;
        private const double IndeterminateCycleSeconds = 1.5;
        private const float IndeterminateSweepWidthFraction = 0.3f;

        public ProgressBar()
        {
            IsHitTestVisible = false;
            HorizontalAlignment = HorizontalAlignmentType.Stretch;

            Height = 8; // Fixed track thickness by default - callers can still override via Height.
            Background = Theme.SurfaceVariant;
        }

        private float ValueFraction()
        {
            float min = Minimum.Value;
            float max = Maximum.Value;
            if (max <= min)
            {
                return 0f;
            }

            return Math.Clamp((Value.Value - min) / (max - min), 0f, 1f);
        }

        public override void RenderControl(UIContext context, Rectangle layoutBounds, Transform parentTransform)
        {
            if (Visible.Value != Visibility.Visible)
            {
                return;
            }

            Rectangle rect = Rectangle.Intersect(layoutBounds, FinalRect);
            float trackRadius = rect.Height / 2f;

            try
            {
                var spriteBatch = context.Renderer.BeginBatch(transform: parentTransform);
                context.Renderer.ClipToRect(rect);

                context.Renderer.FillRoundedRect(spriteBatch, rect, trackRadius, Background.Value);

                if (IsIndeterminate.Value)
                {
                    double elapsed = (DateTime.UtcNow - indeterminateStart).TotalSeconds;
                    float cyclePosition = (float)((elapsed % IndeterminateCycleSeconds) / IndeterminateCycleSeconds);

                    int sweepWidth = Math.Max(1, (int)(rect.Width * IndeterminateSweepWidthFraction));
                    int travel = rect.Width + sweepWidth;
                    int sweepLeft = rect.Left - sweepWidth + (int)(cyclePosition * travel);

                    Rectangle sweepRect = Rectangle.Intersect(rect, new Rectangle(sweepLeft, rect.Top, sweepWidth, rect.Height));
                    if (sweepRect.Width > 0)
                    {
                        context.Renderer.FillRoundedRect(spriteBatch, sweepRect, trackRadius, Theme.Primary);
                    }
                }
                else
                {
                    int fillWidth = (int)(rect.Width * ValueFraction());
                    if (fillWidth > 0)
                    {
                        Rectangle fillRect = rect with { Width = fillWidth };
                        context.Renderer.FillRoundedRect(spriteBatch, fillRect, trackRadius, Theme.Primary);
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
