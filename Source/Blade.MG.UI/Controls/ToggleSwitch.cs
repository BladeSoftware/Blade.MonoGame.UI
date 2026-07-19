using Blade.MG.UI.Controls.Templates;
using Blade.MG.UI.Events;
using Microsoft.Xna.Framework;

namespace Blade.MG.UI.Controls
{
    /// <summary>
    /// A Material-style On/Off switch - a sliding thumb on a pill-shaped track, toggled by
    /// clicking anywhere on the control (or Space/Enter while focused - see
    /// UIManager.Keyboard.cs's generic "Enter/Space activates whatever has focus" handling, which
    /// every <see cref="ActivateAsync"/>-implementing control gets for free with no extra code
    /// here). See <see cref="Templates.ToggleSwitchTemplate"/> for the actual track/thumb
    /// rendering and slide animation.
    /// </summary>
    public class ToggleSwitch : TemplatedControl
    {
        public Binding<string> Text;

        [DesignerProperty]
        public Binding<string> FontName { get; set; } = new Binding<string>();
        [DesignerProperty]
        public Binding<float> FontSize { get; set; } = new Binding<float>();

        private Binding<Color> textColor = new Binding<Color>();
        [DesignerProperty]
        public Binding<Color> TextColor { get => textColor; set => SetField(ref textColor, value); }

        // SetField-backed (not a plain auto-property) so ActivateAsync's `IsOn = !IsOn.Value;`
        // reassignment - which goes through Binding<T>'s implicit bool->Binding<bool> cast and
        // would otherwise construct a brand new Binding<bool> instance every toggle - instead
        // updates the EXISTING instance's .Value in place, preserving whatever Changed
        // subscription EnsureBindingsWired (UIComponent.cs) already wired up. See CheckBox's
        // IsChecked for the exact bug this avoids (a fixed regression from this same session).
        private Binding<bool> isOn = new Binding<bool>();
        [DesignerProperty]
        public Binding<bool> IsOn { get => isOn; set => SetField(ref isOn, value); }

        public Action<bool> OnValueChanged { get; set; }
        public Func<bool, Task> OnValueChangedAsync { get; set; }

        public ToggleSwitch()
        {
            TemplateType = typeof(ToggleSwitchTemplate);

            IsTabStop = true;
            IsHitTestVisible = true;

            FontName = null; // Use default
            FontSize = null; // Use default

            Text = string.Empty;
            IsOn = false;
        }

        public override async Task ActivateAsync(UIWindow uiWindow, UIClickEvent uiEvent)
        {
            // Don't change state if not Enabled
            if (!IsEnabled.Value)
            {
                return;
            }

            IsOn = !IsOn.Value;

            OnValueChanged?.Invoke(IsOn.Value);
            if (OnValueChangedAsync != null)
            {
                await OnValueChangedAsync(IsOn.Value);
            }

            await base.ActivateAsync(uiWindow, uiEvent);
        }
    }
}
