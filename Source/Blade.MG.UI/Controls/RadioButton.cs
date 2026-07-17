using Blade.MG.Primitives;
using Blade.MG.UI.Components;
using Blade.MG.UI.Events;
using Microsoft.Xna.Framework;

namespace Blade.MG.UI.Controls
{
    // Deliberately a single self-contained Control (like ScrollBar/Slider/ProgressBar) rather
    // than a TemplatedControl + separate *Template.cs file - the visual (a label plus one
    // circular indicator) is simple enough that the extra indirection wouldn't earn its keep.
    public class RadioButton : Control
    {
        public Binding<string> Text { get; set; } = string.Empty;

        private Binding<Color> textColor = new Binding<Color>();
        public Binding<Color> TextColor { get => textColor; set => SetField(ref textColor, value); }

        public Binding<bool> IsChecked { get; set; } = false;

        // RadioButtons sharing the same GroupName within the same UIWindow are mutually
        // exclusive - checking one unchecks the others. Empty/null GroupName means "no group",
        // i.e. behaves like a standalone toggle.
        public string GroupName { get; set; } = string.Empty;

        public Action<bool> OnValueChanged { get; set; }
        public Func<bool, Task> OnValueChangedAsync { get; set; }

        private const int IndicatorSize = 18;
        private const int OuterRadius = IndicatorSize / 2;
        private const int InnerRadius = 5;

        private Label label1;
        private bool mouseHover;

        public RadioButton()
        {
            IsHitTestVisible = true;
            IsTabStop = true;

            Background = Color.Transparent;
        }

        protected override void InitTemplate()
        {
            base.InitTemplate();

            label1 = new Label
            {
                Text = Text,
                TextColor = TextColor,
                Margin = new Thickness(IndicatorSize + 8, 4, 4, 4),
                HorizontalAlignment = HorizontalAlignmentType.Left,
                VerticalAlignment = VerticalAlignmentType.Center,
            };

            Content = label1;
        }

        public override void RenderControl(UIContext context, Rectangle layoutBounds, Transform parentTransform)
        {
            if (Visible.Value != Visibility.Visible)
            {
                return;
            }

            base.RenderControl(context, layoutBounds, parentTransform);

            Rectangle rect = Rectangle.Intersect(layoutBounds, FinalRect);
            Vector2 center = new(FinalRect.Left + 2 + OuterRadius, FinalRect.Center.Y);

            bool isChecked = IsChecked.Value;
            Color ringColor = isChecked || mouseHover ? Theme.Primary : Theme.Outline;

            try
            {
                var spriteBatch = context.Renderer.BeginBatch(transform: parentTransform);
                context.Renderer.ClipToRect(rect);

                Primitives2D.DrawCircle(spriteBatch, center, OuterRadius, ringColor, 2f);

                if (isChecked)
                {
                    Primitives2D.FillCircle(spriteBatch, center, InnerRadius, Theme.Primary);
                }
            }
            finally
            {
                context.Renderer.EndBatch();
            }
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

            if (uiEvent.Hover != mouseHover)
            {
                mouseHover = uiEvent.Hover;
                StateHasChanged();
            }
        }

        protected override void HandleStateChange()
        {
            if (label1 == null)
            {
                return;
            }

            ApplyThemedValue(this, label1.TextColor, nameof(TextColor), mouseHover ? Theme.Primary : Theme.OnSurface);
        }

        public override async Task ActivateAsync(UIWindow uiWindow, UIClickEvent uiEvent)
        {
            if (!IsEnabled.Value)
            {
                return;
            }

            // Unlike CheckBox, clicking an already-checked radio button is a no-op - a radio
            // group has no "none selected" state to toggle back into once one option is chosen.
            if (!IsChecked.Value)
            {
                IsChecked.Value = true;
                UncheckOthersInGroup();

                OnValueChanged?.Invoke(true);
                if (OnValueChangedAsync != null)
                {
                    await OnValueChangedAsync(true);
                }
            }

            await base.ActivateAsync(uiWindow, uiEvent);
        }

        private void UncheckOthersInGroup()
        {
            if (string.IsNullOrEmpty(GroupName) || ParentWindow == null)
            {
                return;
            }

            foreach (var component in WalkTree(ParentWindow))
            {
                if (component is RadioButton other && other != this && other.GroupName == GroupName && other.IsChecked.Value)
                {
                    other.IsChecked.Value = false;
                }
            }
        }

        private static IEnumerable<UIComponent> WalkTree(UIComponent root)
        {
            if (root == null)
            {
                yield break;
            }

            yield return root;

            foreach (var child in root.PrivateControls)
            {
                foreach (var descendant in WalkTree(child))
                {
                    yield return descendant;
                }
            }

            if (root is Control control && control.Content != null)
            {
                foreach (var descendant in WalkTree(control.Content))
                {
                    yield return descendant;
                }
            }

            if (root is Container container)
            {
                foreach (var child in container.Children)
                {
                    foreach (var descendant in WalkTree(child))
                    {
                        yield return descendant;
                    }
                }
            }
        }
    }
}
