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

        // Border's own constructor unconditionally enables caching, which is wrong for content
        // that redraws itself every frame from something other than a Binding<T> (e.g.
        // ProgressBar's indeterminate sweep, computed from DateTime.UtcNow - nothing ever fires
        // Changed for it, so a cached ancestor never knows to invalidate and the sweep freezes
        // after its first render). Defaults to true (existing behavior everywhere else); set to
        // false for a Section hosting continuously-animating content. Named distinctly from the
        // inherited EnableCaching (which would control caching of Section itself, not the inner
        // Border built here) to avoid the two being confused.
        public bool EnableContentCaching { get; set; } = true;

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
                Elevation = 0,
                EnableCaching = EnableContentCaching,
            };

            base.Content = border;

            border.Content = this.Content;
        }

    }
}
