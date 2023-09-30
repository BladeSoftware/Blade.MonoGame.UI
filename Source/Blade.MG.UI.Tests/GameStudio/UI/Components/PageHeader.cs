using Blade.UI;
using Blade.UI.Components;
using Blade.UI.Controls;
using Microsoft.Xna.Framework;

namespace GameStudio.UI.Editors
{
    public class PageHeader : Control
    {
        public string Title { get; set; }

        protected override void InitTemplate()
        {
            base.InitTemplate();

            HorizontalAlignment = HorizontalAlignmentType.Stretch;
            VerticalAlignment = VerticalAlignmentType.Top;

            Content = new Label()
            {
                Height = 100,
                Text = Title, //"Labels",
                TextColor = Color.Black,
                HorizontalAlignment = HorizontalAlignmentType.Left,
                VerticalAlignment = VerticalAlignmentType.Center,
            };




        }

    }
}
