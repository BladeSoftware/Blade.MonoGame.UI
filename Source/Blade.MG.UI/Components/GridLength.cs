namespace Blade.MG.UI.Components
{
    public struct GridLength
    {
        public GridUnitType GridUnitType;
        public float Value;

        public bool IsAbsolute
        {
            get { return GridUnitType == GridUnitType.Pixel; }
        }

        public bool IsAuto
        {
            get { return GridUnitType == GridUnitType.Auto; }
        }

        public bool IsStar
        {
            get { return GridUnitType == GridUnitType.Star; }
        }

        public GridLength(GridUnitType gridUnitType, float value = 0f)
        {
            if (gridUnitType == GridUnitType.Star && value == 0f)
            {
                value = 1f;
            }

            GridUnitType = gridUnitType;
            Value = value;
        }
    }
}
