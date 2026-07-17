using Blade.MG.Primitives;
using Blade.MG.UI.Components;
using Blade.MG.UI.Events;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Blade.MG.UI.Controls
{
    // A standalone value slider - the mouse-position-to-value math is the same drag logic
    // ScrollBar.HandleDrag already has, extracted here without ScrollBar's endcap/arrow/
    // proportional-thumb-size chrome (a Slider's thumb is a fixed size regardless of the
    // Min/Max range, unlike a scrollbar's, which represents the visible fraction of content).
    public class Slider : Control
    {
        public Binding<float> Minimum { get; set; } = 0f;
        public Binding<float> Maximum { get; set; } = 100f;
        public Binding<float> Value { get; set; } = 0f;

        public Orientation Orientation { get; set; } = Orientation.Horizontal;

        public Action<float> OnValueChanged { get; set; }
        public Func<float, Task> OnValueChangedAsync { get; set; }

        public int TrackThickness { get; set; } = 4;
        public int ThumbSize { get; set; } = 16;

        private bool dragging = false;
        private bool mouseHover = false;

        public Slider()
        {
            IsHitTestVisible = true;
            IsTabStop = true;

            Background = Theme.SurfaceVariant;
        }

        public override void Measure(UIContext context, ref Size availableSize, ref Layout parentMinMax)
        {
            if (Orientation == Orientation.Horizontal)
            {
                Width = float.NaN;
                Height = ThumbSize;
                HorizontalAlignment = HorizontalAlignmentType.Stretch;
            }
            else
            {
                Width = ThumbSize;
                Height = float.NaN;
                VerticalAlignment = VerticalAlignmentType.Stretch;
            }

            base.Measure(context, ref availableSize, ref parentMinMax);
        }

        private float ClampedValue()
        {
            float min = Minimum.Value;
            float max = Maximum.Value;
            if (max < min)
            {
                (min, max) = (max, min);
            }

            return Math.Clamp(Value.Value, min, max);
        }

        private float ValueFactor()
        {
            float min = Minimum.Value;
            float max = Maximum.Value;
            if (max <= min)
            {
                return 0f;
            }

            return (ClampedValue() - min) / (max - min);
        }

        private void SetValueFromFactor(float factor)
        {
            factor = Math.Clamp(factor, 0f, 1f);

            float min = Minimum.Value;
            float max = Maximum.Value;
            float newValue = min + factor * (max - min);

            if (newValue == Value.Value)
            {
                return;
            }

            Value.Value = newValue;

            OnValueChanged?.Invoke(newValue);
            if (OnValueChangedAsync != null)
            {
                _ = OnValueChangedAsync(newValue);
            }
        }

        public override void RenderControl(UIContext context, Rectangle layoutBounds, Transform parentTransform)
        {
            if (Visible.Value != Visibility.Visible)
            {
                return;
            }

            Rectangle rect = Rectangle.Intersect(layoutBounds, FinalRect);
            float factor = ValueFactor();

            Color thumbColor = dragging || mouseHover ? Theme.Primary : Theme.Outline;

            try
            {
                var spriteBatch = context.Renderer.BeginBatch(transform: parentTransform);
                context.Renderer.ClipToRect(rect);

                if (Orientation == Orientation.Horizontal)
                {
                    Rectangle trackRect = new(rect.Left, rect.Center.Y - TrackThickness / 2, rect.Width, TrackThickness);
                    context.Renderer.FillRoundedRect(spriteBatch, trackRect, TrackThickness / 2f, Background.Value);

                    int thumbX = rect.Left + (int)((rect.Width - ThumbSize) * factor);
                    Rectangle thumbRect = new(thumbX, rect.Center.Y - ThumbSize / 2, ThumbSize, ThumbSize);
                    Primitives2D.FillCircle(spriteBatch, thumbRect.Center.X, thumbRect.Center.Y, ThumbSize / 2f, thumbColor);
                }
                else
                {
                    Rectangle trackRect = new(rect.Center.X - TrackThickness / 2, rect.Top, TrackThickness, rect.Height);
                    context.Renderer.FillRoundedRect(spriteBatch, trackRect, TrackThickness / 2f, Background.Value);

                    int thumbY = rect.Top + (int)((rect.Height - ThumbSize) * factor);
                    Rectangle thumbRect = new(rect.Center.X - ThumbSize / 2, thumbY, ThumbSize, ThumbSize);
                    Primitives2D.FillCircle(spriteBatch, thumbRect.Center.X, thumbRect.Center.Y, ThumbSize / 2f, thumbColor);
                }
            }
            finally
            {
                context.Renderer.EndBatch();
            }
        }

        // ---=== UI Events ===---

        public override async Task HandleHoverChangedAsync(UIWindow uiWindow, UIHoverChangedEvent uiEvent)
        {
            await base.HandleHoverChangedAsync(uiWindow, uiEvent);

            if (uiEvent.Hover != mouseHover)
            {
                mouseHover = uiEvent.Hover;
                StateHasChanged();
            }
        }

        public override async Task HandleMouseDownEventAsync(UIWindow uiWindow, UIMouseDownEvent uiEvent)
        {
            if (uiEvent.Handled) return;

            if (uiEvent.PrimaryButton.Pressed && ContainsScreenPoint(new Point(uiEvent.X, uiEvent.Y)))
            {
                LockEventsToControl(uiWindow, this);

                dragging = true;
                HandleDrag(uiEvent.X, uiEvent.Y);

                uiEvent.Handled = true;
                StateHasChanged();

                return;
            }

            await base.HandleMouseDownEventAsync(uiWindow, uiEvent);
        }

        public override async Task HandleMouseUpEventAsync(UIWindow uiWindow, UIMouseUpEvent uiEvent)
        {
            if (uiEvent.Handled) return;

            if (dragging)
            {
                UnlockEventsFromControl(uiWindow, this);

                dragging = false;
                uiEvent.Handled = true;
                StateHasChanged();

                return;
            }

            await base.HandleMouseUpEventAsync(uiWindow, uiEvent);
        }

        public override async Task HandleMouseMoveEventAsync(UIWindow uiWindow, UIMouseMoveEvent uiEvent)
        {
            if (uiEvent.Handled) return;

            if (dragging)
            {
                HandleDrag(uiEvent.X, uiEvent.Y);

                uiEvent.Handled = true;
                return;
            }

            await base.HandleMouseMoveEventAsync(uiWindow, uiEvent);
        }

        private void HandleDrag(int mouseX, int mouseY)
        {
            float factor = Orientation == Orientation.Horizontal
                ? (mouseX - (FinalRect.Left + ThumbSize / 2f)) / (float)(FinalRect.Width - ThumbSize)
                : (mouseY - (FinalRect.Top + ThumbSize / 2f)) / (float)(FinalRect.Height - ThumbSize);

            SetValueFromFactor(factor);
        }

        public override async Task HandleKeyPressAsync(UIWindow uiWindow, UIKeyEvent uiEvent)
        {
            if (!uiEvent.Handled && HasFocus.Value)
            {
                HandleKey(uiEvent);
            }

            await base.HandleKeyPressAsync(uiWindow, uiEvent);
        }

        private void HandleKey(UIKeyEvent uiEvent)
        {
            float min = Minimum.Value;
            float max = Maximum.Value;
            float step = (max - min) / 20f;
            if (step == 0f)
            {
                step = 1f;
            }

            switch (uiEvent.Key)
            {
                case Keys.Left:
                case Keys.Down:
                    Value.Value = Math.Clamp(Value.Value - step, min, max);
                    OnValueChanged?.Invoke(Value.Value);
                    uiEvent.Handled = true;
                    break;

                case Keys.Right:
                case Keys.Up:
                    Value.Value = Math.Clamp(Value.Value + step, min, max);
                    OnValueChanged?.Invoke(Value.Value);
                    uiEvent.Handled = true;
                    break;

                case Keys.Home:
                    Value.Value = min;
                    OnValueChanged?.Invoke(Value.Value);
                    uiEvent.Handled = true;
                    break;

                case Keys.End:
                    Value.Value = max;
                    OnValueChanged?.Invoke(Value.Value);
                    uiEvent.Handled = true;
                    break;
            }
        }

        protected override void HandleStateChange()
        {
        }
    }
}
