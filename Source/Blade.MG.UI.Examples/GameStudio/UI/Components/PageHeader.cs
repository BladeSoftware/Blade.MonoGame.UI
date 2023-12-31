﻿using Blade.MG.UI;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Microsoft.Xna.Framework;

namespace Examples.UI.Components
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
