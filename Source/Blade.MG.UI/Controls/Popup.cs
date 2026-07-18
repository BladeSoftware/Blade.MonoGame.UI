using Blade.MG.UI.Components;
using Microsoft.Xna.Framework;

namespace Blade.MG.UI.Controls
{
    // A reusable floating-window base for non-modal overlays anchored at a screen point (a
    // tooltip, a small inline flyout, etc). ComboBox's DropdownWindow and Menu's own popup each
    // still hand-roll this same "absolutely-positioned Border, clamped to stay on-screen"
    // pattern independently - this doesn't refactor either of those (a separate, riskier change
    // touching working, already-tested code), but new popup-style controls should build on this
    // instead of adding a third copy.
    public class Popup : ModalBase
    {
        private ClampedBorder contentBorder;
        private bool isShown;

        public UIComponent Content
        {
            get => contentBorder?.Content;
            set { if (contentBorder != null) contentBorder.Content = value; }
        }

        public Popup()
        {
            // Unlike Menu/Dialog (ModalBase's usual consumers), a Popup shouldn't steal input
            // from the rest of the UI - a tooltip appearing shouldn't block clicking whatever's
            // underneath it.
            IsModal = false;
        }

        public override void LoadContent()
        {
            base.LoadContent();

            // AddWindow (UIManager.cs) calls Initialize()/LoadContent() unconditionally on every
            // Add() - correct for Initialize (Context/Renderer/SpriteBatch legitimately need to
            // be recreated after a prior Close() disposed them), but a Popup can be legitimately
            // reshown (Close then ShowAt again) many times over its lifetime, and LoadContent
            // must stay idempotent across those calls. Without this guard, every reshow created
            // a brand-new contentBorder and added it as an ADDITIONAL child via AddChild below,
            // never removing the previous one - the reported "an extra tooltip window every time
            // you hover" bug (extra orphaned contentBorder children piling up, not extra
            // UIManager-level windows).
            if (contentBorder != null)
            {
                return;
            }

            contentBorder = new ClampedBorder
            {
                HorizontalAlignment = HorizontalAlignmentType.Absolute,
                VerticalAlignment = VerticalAlignmentType.Absolute,
                Background = Theme.SurfaceContainer,
                BorderColor = Theme.Outline,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Elevation = 4,
                Padding = new Thickness(8),
            };

            base.AddChild(contentBorder);
        }

        /// <summary>
        /// Shows (or moves, if already shown) this popup anchored at a screen position, clamped
        /// to stay fully on-screen.
        /// </summary>
        public void ShowAt(Game game, Point screenPosition)
        {
            var uiManager = game.Services.GetService<UIManager>();
            if (uiManager == null)
            {
                throw new InvalidOperationException("UIManager service not found. Make sure UIManager is added to the Game Services.");
            }

            // contentBorder is only created in LoadContent, which the UIWindow lifecycle only
            // runs once this window is actually added to a UIManager - so Add() must happen
            // before contentBorder is touched the very first time this is shown.
            if (!isShown)
            {
                uiManager.Add(this);
                isShown = true;
            }

            contentBorder.Left = screenPosition.X;
            contentBorder.Top = screenPosition.Y;

            Visible = Visibility.Visible;
        }

        public void Close(Game game)
        {
            if (!isShown)
            {
                return;
            }

            var uiManager = game.Services.GetService<UIManager>();
            uiManager?.Remove(this);
            isShown = false;
        }

        // Anchored popups have no regard for screen edges by default (see Menu's identical
        // MenuPopupBorder) - once Measure has run we know the popup's DesiredSize, so nudge
        // Left/Top back on-screen before Arrange turns them into FinalRect.
        private class ClampedBorder : Border
        {
            public override void Arrange(UIContext context, Rectangle layoutBounds, Rectangle parentLayoutBounds)
            {
                float desiredWidth = DesiredSize.Width;
                float desiredHeight = DesiredSize.Height;

                if (!float.IsNaN(desiredWidth) && desiredWidth > 0 && desiredWidth <= parentLayoutBounds.Width)
                {
                    float maxLeft = Math.Max(parentLayoutBounds.Left, parentLayoutBounds.Right - desiredWidth);
                    Left = Math.Clamp(Left, parentLayoutBounds.Left, maxLeft);
                }

                if (!float.IsNaN(desiredHeight) && desiredHeight > 0 && desiredHeight <= parentLayoutBounds.Height)
                {
                    float maxTop = Math.Max(parentLayoutBounds.Top, parentLayoutBounds.Bottom - desiredHeight);
                    Top = Math.Clamp(Top, parentLayoutBounds.Top, maxTop);
                }

                base.Arrange(context, layoutBounds, parentLayoutBounds);
            }
        }
    }
}
