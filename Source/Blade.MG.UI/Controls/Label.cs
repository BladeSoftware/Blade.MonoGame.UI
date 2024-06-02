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

        public Binding<string> Text { get; set; } = string.Empty;
        public Binding<string> FontName { get; set; } = new Binding<string>();
        public Binding<float> FontSize { get; set; } = new Binding<float>();
        public Binding<Color> TextColor { get; set; } = new Binding<Color>();

        public Rectangle TextRect { get; set; } = new Rectangle();
        public Rectangle TextBaseLine { get; set; } = new Rectangle();

        public Label()
        {
            TemplateType = typeof(LabelTemplate);

            HorizontalTextAlignment = HorizontalAlignmentType.Left;
            VerticalTextAlignment = VerticalAlignmentType.Center;

            Text = null;
            FontName = null; // Use default font
            FontSize = null;
            TextColor.Value = Color.White;

            Background = Color.Transparent;

            HorizontalAlignment = HorizontalAlignmentType.Stretch;
            VerticalAlignment = VerticalAlignmentType.Stretch;
            //HorizontalContentAlignment = HorizontalAlignmentType.Center;
            //VerticalContentAlignment = VerticalAlignmentType.Center;

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
