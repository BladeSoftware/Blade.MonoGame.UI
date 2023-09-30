using Microsoft.Xna.Framework;

namespace BladeUI.UnitTesting
{
    internal class TestHelper
    {
        public static Rectangle ShrinkRect(Rectangle rectangle, int left, int top, int right, int bottom) => new Rectangle(rectangle.Left + left, rectangle.Top + top, rectangle.Width - left - right, rectangle.Height - top - bottom);
        public static Rectangle ShrinkRect(Rectangle rectangle, int marginLeft, int marginTop, int marginRight, int marginBottom, int paddingLeft, int paddingTop, int paddingRight, int paddingBottom) => new Rectangle(rectangle.Left + marginLeft + paddingLeft, rectangle.Top + marginTop + paddingTop, rectangle.Width - marginLeft - marginRight - paddingLeft - paddingRight, rectangle.Height - marginTop - marginBottom - paddingTop - paddingBottom);

    }
}
