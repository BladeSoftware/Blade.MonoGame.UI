using Blade.MG.UI.Animations;
using Blade.MG.UI.Components;
using Blade.MG.UI.Events;
using Microsoft.Xna.Framework;

namespace Blade.MG.UI.Controls.Templates
{
    public class ToggleSwitchTemplate : Control
    {
        private const int TrackWidth = 44;
        private const int TrackHeight = 24;
        private const int ThumbDiameter = 18;
        private const int ThumbInset = 3;

        private Label label1;

        // Eases the thumb between its Off (left) and On (right) resting positions instead of
        // jumping - same pattern as TextBoxTemplate's own labelPosition/labelFontSize animation
        // (see PropertyAnimationManager in Blade.MG.UI.Animations). A private field, not a
        // public property, so it's invisible to UIComponent.EnsureBindingsWired's
        // BindingFlags.Public-only reflection scan - explicitly subscribing its own Changed
        // event to BubbleInvalidation here is required for the slide animation to actually
        // repaint while nested under a cached ancestor Border (see the CheckBox.IsChecked /
        // TextBoxTemplate label-animation bugs fixed earlier this session for the exact failure
        // mode this avoids: the value progresses correctly every frame regardless, but nothing
        // forces a redraw to show it without this subscription).
        private readonly Binding<float> thumbProgress = new Binding<float>(0f);

        // Guards the very first RenderControl call, snapping straight to whatever state the
        // switch should already be in (e.g. constructed with IsOn = true) rather than animating
        // in from the meaningless zeroed-out default above.
        private bool progressInitialized;

        public ToggleSwitchTemplate()
        {
            thumbProgress.Changed += BubbleInvalidation;
        }

        protected override void InitTemplate()
        {
            base.InitTemplate();

            var toggle = ParentAs<ToggleSwitch>();

            // Reads toggle.Padding rather than leaving it at the inherited default of zero - see
            // CheckBoxTemplate.cs / ButtonTemplate.cs's border1 for why this needs to live HERE
            // (the self-painting leaf whose own FinalRect draws the track/thumb below and whose
            // Background - via Control's base RenderControl - also fills FinalRect) rather than
            // relying on toggle.Padding alone, which cancels itself out at ToggleSwitch's own hop.
            // (Known limitation shared with Button's fix - see ButtonTemplate.cs's border1
            // comment: toggle.Padding is counted twice in ToggleSwitch's own DesiredSize.)
            Padding = new Binding<Thickness>(() => toggle.Padding.Value);

            label1 = new Label()
            {
                Text = toggle.Text,
                FontName = toggle.FontName,
                FontSize = toggle.FontSize,
                Margin = new Thickness(TrackWidth + 10, 5, 2, 5),

                // Link to the ToggleSwitch's own TextColor binding so a developer can override
                // it directly (toggle.TextColor = ...) as well as via SetStyleOverride.
                TextColor = toggle.TextColor,
                VerticalTextAlignment = VerticalAlignmentType.Center,
            };

            Content = label1;
        }

        // ---=== Handle State Changes ===---

        public override async Task HandleFocusChangedEventAsync(UIWindow uiWindow, UIFocusChangedEvent uiEvent)
        {
            await base.HandleFocusChangedEventAsync(uiWindow, uiEvent);

            StateHasChanged();
        }

        public override async Task HandleHoverChangedAsync(UIWindow uiWindow, UIHoverChangedEvent uiEvent)
        {
            await base.HandleHoverChangedAsync(uiWindow, uiEvent);

            StateHasChanged();
        }

        protected override void HandleStateChange()
        {
            var toggle = ParentAs<ToggleSwitch>();

            // Normal State
            ApplyThemedValueAnimated(toggle, label1.TextColor, nameof(ToggleSwitch.TextColor), Theme.OnSurface);
            ApplyThemedValueAnimated(toggle, Background, nameof(ToggleSwitch.Background), Color.Transparent);

            // Hover State
            if (MouseHover.Value)
            {
                ApplyThemedValueAnimated(toggle, label1.TextColor, nameof(ToggleSwitch.TextColor), Theme.Primary);
                ApplyThemedValueAnimated(toggle, Background, nameof(ToggleSwitch.Background), Theme.SurfaceVariant);
            }

            // Focused State
            if (HasFocus.Value)
            {
                ApplyThemedValueAnimated(toggle, label1.TextColor, nameof(ToggleSwitch.TextColor), Theme.Primary);
                ApplyThemedValueAnimated(toggle, Background, nameof(ToggleSwitch.Background), Theme.SecondaryContainer);
            }
        }

        public override void RenderControl(UIContext context, Rectangle layoutBounds, Transform parentTransform)
        {
            base.RenderControl(context, layoutBounds, parentTransform);

            var toggle = ParentAs<ToggleSwitch>();
            bool isEnabled = toggle.IsEnabled.Value;

            float targetProgress = toggle.IsOn.Value ? 1f : 0f;
            if (!progressInitialized)
            {
                thumbProgress.Value = targetProgress;
                progressInitialized = true;
            }
            else
            {
                PropertyAnimationManager.AnimateTo(thumbProgress, targetProgress, TimeSpan.FromMilliseconds(150), Easing.EaseOutCubic);
            }

            Color trackOnColor = isEnabled ? Theme.Primary : Theme.Disabled;
            Color trackOffColor = isEnabled ? Theme.SurfaceVariant : Theme.OnDisabled;
            Color thumbOnColor = isEnabled ? Theme.OnPrimary : Theme.OnDisabled;
            Color thumbOffColor = isEnabled ? Theme.Outline : Theme.Disabled;

            Color trackColor = Color.Lerp(trackOffColor, trackOnColor, thumbProgress.Value);
            Color thumbColor = Color.Lerp(thumbOffColor, thumbOnColor, thumbProgress.Value);

            int trackY = FinalRect.Y + (FinalRect.Height - TrackHeight) / 2;
            var trackRect = new Rectangle(FinalRect.X, trackY, TrackWidth, TrackHeight);

            const float thumbTravel = TrackWidth - ThumbDiameter - ThumbInset * 2;
            int thumbX = trackRect.X + ThumbInset + (int)(thumbTravel * thumbProgress.Value);
            int thumbY = trackRect.Y + (TrackHeight - ThumbDiameter) / 2;
            var thumbRect = new Rectangle(thumbX, thumbY, ThumbDiameter, ThumbDiameter);

            try
            {
                var spriteBatch = context.Renderer.BeginBatch(transform: parentTransform);
                context.Renderer.ClipToRect(layoutBounds);

                context.Renderer.FillRoundedRect(spriteBatch, trackRect, TrackHeight / 2, trackColor);
                context.Renderer.FillRoundedRect(spriteBatch, thumbRect, ThumbDiameter / 2, thumbColor);
            }
            finally
            {
                context.Renderer.EndBatch();
            }
        }
    }
}
