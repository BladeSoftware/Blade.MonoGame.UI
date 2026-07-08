using Blade.MG.UI.Components;
using Blade.MG.UI.Events;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blade.MG.UI.Controls
{
    /// <summary>
    /// A small, round, icon-only clickable control for toolbars/AppBars (e.g. a theme picker
    /// trigger). The icon itself is supplied by the caller via <see cref="DrawIcon"/> - this
    /// control only handles sizing, hit-testing, hover/focus theming and the click event, so it
    /// stays usable regardless of how a consuming app wants to render its icons (primitives,
    /// a pre-rasterized texture, an SVG-to-texture helper, etc.).
    /// </summary>
    public class IconButton : Control
    {
        /// <summary>
        /// Draws the icon centered within the button. Called every frame with the icon's
        /// content rectangle (already inset from the button's edge) and the current icon tint
        /// (theme-driven, overridable via <see cref="IconColor"/>).
        /// </summary>
        public Action<UIContext, SpriteBatch, Rectangle, Color> DrawIcon { get; set; }

        public int IconSize { get; set; } = 20;

        private Binding<Color> iconColor = new Binding<Color>();
        public Binding<Color> IconColor { get => iconColor; set => SetField(ref iconColor, value); }

        public IconButton()
        {
            Width = 40;
            Height = 40;

            IsHitTestVisible = true;
            IsTabStop = true;
            CanHover = true;

            Background = Color.Transparent;
        }

        public override void RenderControl(UIContext context, Rectangle layoutBounds, Transform parentTransform)
        {
            base.RenderControl(context, layoutBounds, parentTransform);

            if (DrawIcon == null)
            {
                return;
            }

            var iconRect = new Rectangle(
                FinalContentRect.Center.X - IconSize / 2,
                FinalContentRect.Center.Y - IconSize / 2,
                IconSize,
                IconSize);

            try
            {
                var spriteBatch = context.Renderer.BeginBatch(transform: parentTransform);
                context.Renderer.ClipToRect(layoutBounds);

                DrawIcon(context, spriteBatch, iconRect, IconColor.Value);
            }
            finally
            {
                context.Renderer.EndBatch();
            }
        }

        // ---=== Handle State Changes ===---

        public override async Task HandleHoverChangedAsync(UIWindow uiWindow, UIHoverChangedEvent uiEvent)
        {
            MouseHover = uiEvent.Hover;

            await base.HandleHoverChangedAsync(uiWindow, uiEvent);

            StateHasChanged();
        }

        public override async Task HandleFocusChangedEventAsync(UIWindow uiWindow, UIFocusChangedEvent uiEvent)
        {
            await base.HandleFocusChangedEventAsync(uiWindow, uiEvent);

            StateHasChanged();
        }

        protected override void HandleStateChange()
        {
            // Normal State
            ApplyThemedValue(this, Background, nameof(Background), Color.Transparent);
            ApplyThemedValue(this, IconColor, nameof(IconColor), Theme.OnSurfaceVariant);

            // Hover / Focused State
            if (MouseHover.Value || HasFocus.Value)
            {
                ApplyThemedValue(this, Background, nameof(Background), Theme.SurfaceVariant);
                ApplyThemedValue(this, IconColor, nameof(IconColor), Theme.OnSurface);
            }
        }
    }
}
