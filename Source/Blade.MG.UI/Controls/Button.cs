using Blade.MG.UI.Components;
using Blade.MG.UI.Controls.Templates;
using Microsoft.Xna.Framework;

namespace Blade.MG.UI.Controls
{
    public class Button : Control
    {
        public Type TemplateType { get; set; } = typeof(ButtonTemplate); // TODO: Validate TemplateType extends UIComponent

        public Binding<string> Text;

        public Binding<string> FontName { get; set; }
        public Binding<float> FontSize { get; set; }
        public Binding<Color> FontColor { get; set; }

        //public Binding<Border> Border { get; set; }



        public Button()
        {
            Text = null;
            HorizontalAlignment = HorizontalAlignmentType.Stretch;
            VerticalAlignment = VerticalAlignmentType.Stretch;
            HorizontalContentAlignment = HorizontalAlignmentType.Center;
            VerticalContentAlignment = VerticalAlignmentType.Center;

            HitTestVisible = true;
            IsTabStop = true;
        }


        protected override void InitTemplate()
        {
            base.InitTemplate();

            Content = Activator.CreateInstance(TemplateType) as UIComponent;
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
