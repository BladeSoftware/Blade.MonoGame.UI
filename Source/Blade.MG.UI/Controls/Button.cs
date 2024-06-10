using Blade.MG.UI.Components;
using Blade.MG.UI.Controls.Templates;
using Microsoft.Xna.Framework;

namespace Blade.MG.UI.Controls
{
    public class Button : TemplatedControl
    {
        public Binding<string> Text;

        public Binding<HorizontalAlignmentType> HorizontalTextAlignment;
        public Binding<VerticalAlignmentType> VerticalTextAlignment;

        public Binding<string> FontName { get; set; }
        public Binding<float> FontSize { get; set; }
        public Binding<Color> TextColor { get; set; }


        public Button()
        {
            TemplateType = typeof(ButtonTemplate);

            HorizontalAlignment = HorizontalAlignmentType.Stretch;
            VerticalAlignment = VerticalAlignmentType.Stretch;

            HorizontalTextAlignment = HorizontalAlignmentType.Center;
            VerticalTextAlignment = VerticalAlignmentType.Center;

            Text = null;

            IsHitTestVisible = true;
            IsTabStop = true;
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
        }

        // ---=== UI Events ===---
        // ...

    }
}
