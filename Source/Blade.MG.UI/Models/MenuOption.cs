namespace Blade.UI.Models
{
    public class MenuOption : IMenuOption
    {
        public string Id { get; set; }
        public string Text { get; set; }

        public int? Width { get; set; } = 200;

        public bool Disabled { get; set; } = false;

        public override string ToString()
        {
            return Text ?? "";
        }

    }
}
