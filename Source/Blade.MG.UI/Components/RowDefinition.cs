namespace Blade.UI.Components
{
    public record class RowDefinition
    {
        public float ActualHeight { get; set; }
        public GridLength Height { get; set; }
        public float MaxHeight { get; set; } = float.NaN;
        public float MinHeight { get; set; } = float.NaN;

        public RowDefinition()
        {

        }

    }
}
