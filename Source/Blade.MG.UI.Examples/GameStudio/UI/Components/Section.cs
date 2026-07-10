using Blade.MG.UI;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Microsoft.Xna.Framework;

namespace Examples.UI.Components
{
    public class Section : Control
    {
        private UIComponent content;

        public new UIComponent Content
        {
            get { return content; }
            set
            {
                content = value;
            }
        }

        protected override void InitTemplate()
        {
            base.InitTemplate();

            var border = new Border()
            {
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                BorderColor = new Binding<Color>(() => Theme.Outline),
                Background = new Binding<Color>(() => Theme.Surface),
                Margin = new Thickness(5, 5, 5, 5),
                Elevation = 0
            };

            base.Content = border;

            border.Content = this.Content;
        }

    }
}
