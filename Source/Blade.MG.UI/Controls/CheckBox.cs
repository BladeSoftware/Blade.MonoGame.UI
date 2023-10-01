using Blade.MG.UI.Components;
using Blade.MG.UI.Controls.Templates;
using Blade.MG.UI.Events;
using Microsoft.Xna.Framework;

namespace Blade.MG.UI.Controls
{
    public class CheckBox : Control
    {
        public Type TemplateType { get; set; } = typeof(CheckBoxTemplate); // TODO: Validate TemplateType extends UIComponent

        public Binding<string> Text;

        public Binding<string> FontName { get; set; } = new Binding<string>();
        public Binding<float> FontSize { get; set; } = new Binding<float>();

        public Binding<bool?> IsChecked { get; set; } = new Binding<bool?>();
        public Binding<bool> Tristate { get; set; } = false;


        public CheckBox()
        {
            IsTabStop = true;
            HitTestVisible = true;

            FontName = null; // Use default
            FontSize = null; // Use default

            Text = string.Empty;
            IsChecked = Tristate?.Value == true ? null : false;

            HorizontalContentAlignment = HorizontalAlignmentType.Left;
            VerticalContentAlignment = VerticalAlignmentType.Center;

        }

        protected override void InitTemplate()
        {
            base.InitTemplate();

            Content = Activator.CreateInstance(TemplateType) as UIComponent;
        }

        public override void Measure(UIContext context, ref Size availableSize, ref Layout parentMinMax)
        {
            base.Measure(context, ref availableSize, ref parentMinMax);

            MergeChildDesiredSize(context, ref availableSize, Content, ref parentMinMax);
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

        // ---=== UI Events ===---

        public override async Task HandleKeyPressAsync(UIWindow uiWindow, UIKeyEvent uiEvent)
        {
            await base.HandleKeyPressAsync(uiWindow, uiEvent);

            if (!uiEvent.Handled && HasFocus.Value)
            {
                HandleKey(uiEvent);
            }
        }

        private void HandleKey(UIKeyEvent uiEvent)
        {
            //if (uiEvent.KeyChar != null)
            //{
            //    AddChar(uiEvent.KeyChar);
            //    uiEvent.Handled = true;
            //}
            //else
            //{
            //    // Handle Special Keys
            //    switch (uiEvent.Key)
            //    {
            //        case Keys.Back:
            //            HandleBackspace();
            //            uiEvent.Handled = true;
            //            break;

            //        case Keys.Delete:
            //            HandleDelete();
            //            uiEvent.Handled = true;
            //            break;
            //    }
            //}
        }

        public override Task HandleClickEventAsync(UIWindow uiWindow, UIClickEvent uiEvent)
        {
            // return base.HandleClickEventAsync(uiWindow, uiEvent);

            if (Tristate.Value)
            {
                IsChecked = IsChecked?.Value switch
                {
                    null => false,
                    false => true,
                    true => null
                };
            }
            else
            {
                IsChecked = IsChecked?.Value switch
                {
                    null => true,
                    false => true,
                    true => false
                };
            }

            return Task.CompletedTask;
        }

    }
}
