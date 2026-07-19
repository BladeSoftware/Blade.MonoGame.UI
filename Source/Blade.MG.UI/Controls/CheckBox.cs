using Blade.MG.UI.Controls.Templates;
using Blade.MG.UI.Events;
using Blade.MG.UI.Models;
using Microsoft.Xna.Framework;

namespace Blade.MG.UI.Controls
{
    public class CheckBox : TemplatedControl
    {
        public Binding<string> Text;

        [DesignerProperty]
        public Binding<string> FontName { get; set; } = new Binding<string>();
        [DesignerProperty]
        public Binding<float> FontSize { get; set; } = new Binding<float>();

        private Binding<Color> textColor = new Binding<Color>();
        [DesignerProperty]
        public Binding<Color> TextColor { get => textColor; set => SetField(ref textColor, value); }

        // SetField-backed (unlike a plain auto-property) so that ActivateAsync's
        // `IsChecked = IsChecked?.Value switch {...}` reassignment - which goes through
        // Binding<T>'s implicit bool? -> Binding<bool?> cast and would otherwise construct a
        // BRAND NEW Binding<bool?> instance every single toggle - instead updates the EXISTING
        // instance's .Value in place. Without this, every toggle silently discarded whatever
        // Changed subscription EnsureBindingsWired (UIComponent.cs) had wired up, so the new
        // instance never bubbled a cache invalidation on its own: the first click after gaining
        // focus appeared to work only because HasFocus (a real SetField-backed Binding) changed
        // on the same click and bubbled independently; every subsequent click, with focus
        // unchanged, toggled the underlying value but never forced a redraw to show it.
        private Binding<bool?> isChecked = new Binding<bool?>();
        [DesignerProperty]
        public Binding<bool?> IsChecked { get => isChecked; set => SetField(ref isChecked, value); }
        [DesignerProperty]
        public Binding<bool> Tristate { get; set; } = false;

        [DesignerProperty]
        public string CheckedIcon { get; set; }
        [DesignerProperty]
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
            // Handle keyboard input if this control has focus
            if (!uiEvent.Handled && HasFocus.Value)
            {
                HandleKey(uiEvent);
            }

            // Propagate to children
            await base.HandleKeyPressAsync(uiWindow, uiEvent);
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

        public override async Task ActivateAsync(UIWindow uiWindow, UIClickEvent uiEvent)
        {
            // Don't change state if not Enabled
            if (!IsEnabled.Value)
            {
                return;
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
                await OnValueChangedAsync(IsChecked);
            }

            // Previously never called (the equivalent base call here was commented out), so
            // OnActivate/OnActivateAsync never fired for CheckBox - now they do, consistent with
            // every other control that has real activation behavior.
            await base.ActivateAsync(uiWindow, uiEvent);
        }

    }
}
