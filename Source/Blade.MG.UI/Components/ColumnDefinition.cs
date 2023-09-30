namespace Blade.UI.Components
{
    public record class ColumnDefinition
    {
        public GridLength Width { get; set; }
        public float MaxWidth { get; set; } = float.NaN;
        public float MinWidth { get; set; } = float.NaN;

        public float ActualWidth { get; set; }

        public ColumnDefinition()
        {

        }

    }
}
