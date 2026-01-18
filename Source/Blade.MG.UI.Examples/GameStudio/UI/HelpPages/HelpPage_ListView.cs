using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Blade.MG.UI.Controls.Templates;
using Examples.UI.Components;
using GameStudio;
using Microsoft.Xna.Framework;
using System;

namespace Blade.MG.UI.Examples.GameStudio.UI.HelpPages
{
    public class HelpPage_ListView : Panel
    {

        public HelpPage_ListView()
        {

        }

        protected override void InitTemplate()
        {
            base.InitTemplate();


            var layoutPanel = new StackPanel()
            {
                Orientation = Orientation.Vertical,

                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
            };


            base.AddChild(layoutPanel);

            layoutPanel.AddChild(
                new PageHeader()
                {
                    Padding = new Thickness(30, 0, 0, 0),
                    Title = "List View"
                });



            var border = new Border()
            {
                BorderThickness = new Thickness(2),
                BorderColor = Color.Gray,
                Margin = new Thickness(20),
                Padding = new Thickness(10),
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                //Background = Color.HotPink,
                //Width = 800,
                Height = 600,
            };

            layoutPanel.AddChild(border);
            layoutPanel.StretchLastChild = true;


            // ListView for dropdown items
            var ListView = new ListView()
            {
                ItemTemplateType = typeof(ListViewItemTemplate),
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
            };

            ListView.DataContext = Enum.GetNames(typeof(Country));

            border.Content = ListView;


        }


    }
}