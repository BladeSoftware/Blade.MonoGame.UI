namespace Blade.MG.UI.Components
{
    internal class UIHelper
    {
        internal static float Clamp(float value, float min, float max)
        {
            if (!float.IsNaN(min) && value < min)
            {
                value = min;
            }
            if (!float.IsNaN(max) && value > max)
            {
                value = max;
            }

            return value;
        }

    }
}
