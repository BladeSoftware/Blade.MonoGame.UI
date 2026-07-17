using Blade.MG.UI.Components;
using Blade.MG.UI.Controls.Templates;
using Blade.MG.UI.Services;
using FontStashSharp;
using Microsoft.Xna.Framework;

namespace Blade.MG.UI.Controls
{
    public class Label : TemplatedControl
    {

        public Binding<HorizontalAlignmentType> HorizontalTextAlignment;
        public Binding<VerticalAlignmentType> VerticalTextAlignment;

        [DesignerProperty]
        public Binding<string> Text { get; set; } = string.Empty;
        [DesignerProperty]
        public Binding<string> FontName { get; set; } = new Binding<string>();
        [DesignerProperty]
        public Binding<float> FontSize { get; set; } = new Binding<float>();

        private Binding<Color> textColor = new Binding<Color>();
        [DesignerProperty]
        public Binding<Color> TextColor { get => textColor; set => SetField(ref textColor, value); }

        public Rectangle TextRect { get; set; }// = new Rectangle();
        public Rectangle TextBaseLine { get; set; }// = new Rectangle();

        public Label()
        {
            TemplateType = typeof(LabelTemplate);

            HorizontalTextAlignment = HorizontalAlignmentType.Left;
            VerticalTextAlignment = VerticalAlignmentType.Center;

            Text = null;
            FontName = null; // Use default font
            FontSize = null;
            TextColor = UIManager.DefaultTheme.OnSurface;

            Background = Color.Transparent;

            HorizontalAlignment = HorizontalAlignmentType.Stretch;
            VerticalAlignment = VerticalAlignmentType.Stretch;

            IsHitTestVisible = false;
        }

        public override void Measure(UIContext context, ref Size availableSize, ref Layout parentMinMax)
        {
            if (Visible.Value == Visibility.Collapsed)
            {
                DesiredSize = new Size(0, 0);
                return;
            }

            parentMinMax.Merge(MinWidth, MinHeight, MaxWidth, MaxHeight, availableSize);

            // Label never calls base.Measure (it measures its own text directly, see below), so
            // it doesn't get MeasureSelf's skip-check for free - font measurement is comparatively
            // expensive and Labels are typically the most numerous control in a text-heavy UI, so
            // this is the single highest-value target for skipping. Text/FontName/FontSize are
            // Binding<T> and already bubble into isLayoutDirty on change (see BubbleInvalidation),
            // so TryReuseMeasure's own comparisons (Width/Height/Margin/Padding/availableSize) are
            // all that's needed on top to catch everything this method's math depends on.
            if (TryReuseMeasure(availableSize))
            {
                return;
            }

            float desiredWidth = Width.ToPixels(availableSize.Width);
            float desiredHeight = Height.ToPixels(availableSize.Height);

            if ((FloatHelper.IsNaN(Width) || FloatHelper.IsNaN(Height)) && Text != null)
            {

                SpriteFontBase font = FontService.GetFontOrDefault(FontName?.Value, FontSize?.Value);
                Vector2 textSizeDefault = font.MeasureString("Iyjg");
                Vector2 textSize = font.MeasureString(Text.Value);

                if (FloatHelper.IsNaN(Width))
                {
                    //desiredWidth = textSize.X * 1.03f;  // Size Calculation seems to be off slightly, so add 3%
                    //desiredWidth = textSize.X + 2;
                    desiredWidth = textSize.X + 5;
                }

                if (FloatHelper.IsNaN(Height))
                {
                    desiredHeight = Math.Max(textSize.Y, textSizeDefault.Y);
                }
            }

            if (Margin != null)
            {
                desiredWidth += Margin.Value.Left + Margin.Value.Right;
                desiredHeight += Margin.Value.Top + Margin.Value.Bottom;
            }

            if (Padding != null)
            {
                desiredWidth += Padding.Value.Left + Padding.Value.Right;
                desiredHeight += Padding.Value.Top + Padding.Value.Bottom;
            }

            DesiredSize = new Size(desiredWidth, desiredHeight);

            ClampDesiredSize(availableSize, parentMinMax);

            //base.Measure(context, ref availableSize, ref parentMinMax);
        }

        public override void Arrange(UIContext context, Rectangle layoutBounds, Rectangle parentLayoutBounds)
        {
            base.Arrange(context, layoutBounds, parentLayoutBounds);
        }

        public override void RenderControl(UIContext context, Rectangle layoutBounds, Transform parentTransform)
        {
            if (Visible.Value != Visibility.Visible)
            {
                return;
            }

            base.RenderControl(context, layoutBounds, parentTransform);
        }
    }
}
