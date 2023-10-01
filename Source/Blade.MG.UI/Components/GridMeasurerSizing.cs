namespace Blade.MG.UI.Components
{
    internal class GridMeasurerSizing
    {
        public GridLength Size;
        public float MaxSize;
        public float MinSize;
        public float CalcSize = float.NaN;

        public GridMeasurerSizing()
        {
            MaxSize = float.NaN;
            MinSize = float.NaN;
        }

        public GridMeasurerSizing(ColumnDefinition column)
        {
            Size = column.Width;
            MaxSize = column.MaxWidth;
            MinSize = column.MinWidth;

            if (IsAbsolute)
            {
                CalcSize = Size.Value;
            }
        }

        public GridMeasurerSizing(RowDefinition row)
        {
            Size = row.Height;
            MaxSize = row.MaxHeight;
            MinSize = row.MinHeight;

            if (IsAbsolute)
            {
                CalcSize = Size.Value;
            }
        }

        public bool IsAbsolute
        {
            get { return Size.IsAbsolute; }
        }

        public bool IsAuto
        {
            get { return Size.IsAuto; }
        }

        public bool IsStar
        {
            get { return Size.IsStar; }
        }
    }
}
