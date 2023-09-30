namespace Blade.UI.Components
{
    internal class GridMeasurerSizing
    {
        public GridLength Size;
        public float MaxSize;
        public float MinSize;
        public float CalcSize = float.NaN;

        public GridMeasurerSizing()
        {
            this.MaxSize = float.NaN;
            this.MinSize = float.NaN;
        }

        public GridMeasurerSizing(ColumnDefinition column)
        {
            this.Size = column.Width;
            this.MaxSize = column.MaxWidth;
            this.MinSize = column.MinWidth;

            if (IsAbsolute)
            {
                CalcSize = Size.Value;
            }
        }

        public GridMeasurerSizing(RowDefinition row)
        {
            this.Size = row.Height;
            this.MaxSize = row.MaxHeight;
            this.MinSize = row.MinHeight;

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
