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

        // Re-initializes an already-allocated instance in place (see GridMeasurer.Reset) so a
        // Grid with a stable column/row count doesn't allocate a fresh GridMeasurerSizing per
        // column/row on every Measure pass.
        public void Reset(ColumnDefinition column)
        {
            Size = column.Width;
            MaxSize = column.MaxWidth;
            MinSize = column.MinWidth;
            CalcSize = IsAbsolute ? Size.Value : float.NaN;
        }

        public void Reset(RowDefinition row)
        {
            Size = row.Height;
            MaxSize = row.MaxHeight;
            MinSize = row.MinHeight;
            CalcSize = IsAbsolute ? Size.Value : float.NaN;
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
