using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blade.MG.UI.Controls.Templates
{
    // Shared visual mechanics for text-entry templates (TextBoxTemplate's full chrome and
    // TextFieldTemplate's minimal underline-only chrome), so caret hit-testing and the
    // selection-highlight/cursor-blink drawing can't drift between the two as one or the
    // other gets tweaked independently.
    internal static class TextEntryVisuals
    {
        /// <summary>
        /// Maps an absolute screen X coordinate to the nearest character index in
        /// <paramref name="text"/>, based on where it was last rendered (<paramref name="textRect"/>).
        /// </summary>
        public static int GetCharacterIndexAtX(float screenX, string text, Rectangle textRect, SpriteFontBase font)
        {
            if (text.Length == 0)
            {
                return 0;
            }

            float relativeX = screenX - textRect.Left;
            if (relativeX <= 0)
            {
                return 0;
            }

            float previousWidth = 0f;
            for (int i = 1; i <= text.Length; i++)
            {
                float width = font.MeasureString(text[0..i]).X;
                float charCenter = (previousWidth + width) / 2f;

                if (relativeX < charCenter)
                {
                    return i - 1;
                }

                previousWidth = width;
            }

            return text.Length;
        }

        /// <summary>
        /// Draws the selection highlight (if any) and the blinking caret (if focused and
        /// currently in its "on" phase) over already-rendered text at <paramref name="textRect"/>.
        /// </summary>
        public static void DrawSelectionAndCursor(UIContext context, SpriteBatch spriteBatch, string text, Rectangle textRect, Rectangle clippingRect, int selectionStart, int selectionLength, int cursorPosition, bool cursorFlashOn, bool hasFocus, SpriteFontBase font, Color selectionColor, Color cursorColor)
        {
            if (selectionLength > 0)
            {
                int start = Math.Clamp(selectionStart, 0, text.Length);
                int end = Math.Clamp(start + selectionLength, 0, text.Length);

                float startX = font.MeasureString(text[0..start]).X;
                float endX = font.MeasureString(text[0..end]).X;

                Rectangle selectionRect = new Rectangle((int)(textRect.Left + startX), textRect.Top, (int)(endX - startX), textRect.Height);
                context.Renderer.FillRect(spriteBatch, selectionRect, selectionColor, clippingRect);
            }

            if (cursorFlashOn && hasFocus)
            {
                int cursorPos = Math.Clamp(cursorPosition, 0, text.Length);
                Vector2 textSize = font.MeasureString(text[0..cursorPos]);

                Rectangle cursorRect = new Rectangle((int)(textRect.Left + textSize.X), textRect.Top, 2, textRect.Height);
                context.Renderer.FillRect(spriteBatch, cursorRect, cursorColor, clippingRect);
            }
        }
    }
}
