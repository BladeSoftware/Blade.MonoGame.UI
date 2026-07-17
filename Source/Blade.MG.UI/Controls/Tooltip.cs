using Blade.MG.Input;
using Blade.MG.UI.Components;
using Microsoft.Xna.Framework;

namespace Blade.MG.UI.Controls
{
    /// <summary>
    /// A hover-delay text popup built on <see cref="Popup"/>. Use the static <see cref="Attach"/>
    /// helper to wire hover-delay show/hide onto any existing control without that control
    /// needing a "ToolTipText" property of its own:
    /// <c>Tooltip.Attach(myButton, game, "Click to save");</c>
    /// </summary>
    public class Tooltip : Popup
    {
        private Label label;

        public Tooltip()
        {
            // Purely informational - never intercepts clicks, and never becomes the hover target
            // itself (which would otherwise fight with whatever control it's describing).
            IsHitTestVisible = false;
            CanHover = false;
        }

        public override void LoadContent()
        {
            base.LoadContent();

            label = new Label
            {
                TextColor = Theme.OnSurfaceVariant,
            };

            Content = label;
        }

        public void SetText(string text)
        {
            if (label != null)
            {
                label.Text.Value = text;
            }
        }

        /// <summary>
        /// Wires a hover-delay Tooltip onto an existing control's own hover events - shows
        /// <paramref name="text"/> near the mouse after <paramref name="delaySeconds"/> of
        /// continuous hover, hides immediately when the hover ends. Composes with (rather than
        /// replacing) any existing OnHoverChangedAsync handler already on the target.
        /// </summary>
        public static void Attach(UIComponentEvents target, Game game, string text, float delaySeconds = 0.6f)
        {
            var tooltip = new Tooltip();
            CancellationTokenSource cancellation = null;

            target.OnHoverChangedAsync += async uiEvent =>
            {
                cancellation?.Cancel();

                if (!uiEvent.Hover)
                {
                    tooltip.Close(game);
                    return;
                }

                cancellation = new CancellationTokenSource();
                CancellationToken token = cancellation.Token;

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds), token);
                }
                catch (TaskCanceledException)
                {
                    return;
                }

                if (token.IsCancellationRequested)
                {
                    return;
                }

                tooltip.SetText(text);
                tooltip.ShowAt(game, new Point(InputManager.Mouse.X + 16, InputManager.Mouse.Y + 16));
            };
        }
    }
}
