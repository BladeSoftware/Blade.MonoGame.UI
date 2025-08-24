using Blade.MG.UI.Controls.Templates;
using Blade.MG.UI.Events;
using Blade.MG.UI.Models;

namespace Blade.MG.UI.Controls
{
    public class CheckBox : TemplatedControl
    {
        public Binding<string> Text;

        public Binding<string> FontName { get; set; } = new Binding<string>();
        public Binding<float> FontSize { get; set; } = new Binding<float>();

        public Binding<bool?> IsChecked { get; set; } = new Binding<bool?>();
        public Binding<bool> Tristate { get; set; } = false;

        public string CheckedIcon { get; set; }
        public string UncheckedIcon { get; set; }

        public Action<bool?> OnValueChanged { get; set; }
        public Func<bool?, Task> OnValueChangedAsync { get; set; }


        public CheckBox()
        {
            TemplateType = typeof(CheckBoxTemplate);

            IsTabStop = true;
            IsHitTestVisible = true;

            FontName = null; // Use default
            FontSize = null; // Use default

            Text = string.Empty;
            IsChecked = Tristate?.Value == true ? null : false;

            //HorizontalContentAlignment = HorizontalAlignmentType.Left;
            //VerticalContentAlignment = VerticalAlignmentType.Center;

        }

        //protected override void InitTemplate()
        //{
        //    base.InitTemplate();

        //    Content = Activator.CreateInstance(TemplateType) as UIComponent;
        //}

        //public override void Measure(UIContext context, ref Size availableSize, ref Layout parentMinMax)
        //{
        //    base.Measure(context, ref availableSize, ref parentMinMax);

        //    MergeChildDesiredSize(context, ref availableSize, Content, ref parentMinMax);
        //}

        //public override void Arrange(UIContext context, Rectangle layoutBounds, Rectangle parentLayoutBounds)
        //{
        //    base.Arrange(context, layoutBounds, parentLayoutBounds);
        //}

        //public override void RenderControl(UIContext context, Rectangle layoutBounds, Transform parentTransform)
        //{
        //    if (Visible.Value != Visibility.Visible)
        //    {
        //        return;
        //    }

        //    base.RenderControl(context, layoutBounds, parentTransform);

        //}

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

        public override Task HandleMouseClickEventAsync(UIWindow uiWindow, UIClickEvent uiEvent)
        {
            // return base.HandleClickEventAsync(uiWindow, uiEvent);

            // Don't change state if not Enabled
            if (!IsEnabled.Value)
            {
                return Task.CompletedTask;
            }

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

            OnValueChanged?.Invoke(IsChecked);
            if (OnValueChangedAsync != null)
            {
                return OnValueChangedAsync(IsChecked);
            }

            return Task.CompletedTask;
        }

    }
}
