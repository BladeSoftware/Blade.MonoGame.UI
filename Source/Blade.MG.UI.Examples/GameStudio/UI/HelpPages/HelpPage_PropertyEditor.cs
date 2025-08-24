using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Examples.UI.Components;
using Microsoft.Xna.Framework;

namespace Blade.MG.UI.Examples.GameStudio.UI.HelpPages
{
    public class HelpPage_PropertyEditor : Panel
    {
        public class ExampleSettings
        {
            public string Name { get; set; }
            public int Health { get; set; }
            public bool IsActive { get; set; }
            public DifficultyLevel Difficulty { get; set; }
        }

        public enum DifficultyLevel
        {
            Easy,
            Medium,
            Hard
        }


        public HelpPage_PropertyEditor()
        {

        }

        protected override void InitTemplate()
        {
            base.InitTemplate();

            var exampleObject = new ExampleSettings
            {
                Name = "Player",
                Health = 100,
                IsActive = true,
                Difficulty = DifficultyLevel.Medium
            };



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
                    Title = "Property Editor"
                });


            var horizStack = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Top,
                Margin = new Thickness(20, 10, 20, 10),
                HorizontalScrollBarVisible = false,
                VerticalScrollBarVisible = false,
            };

            layoutPanel.AddChild(horizStack);



            var border = new Border()
            {
                BorderThickness = 2,
                BorderColor = Color.Gray,
                Margin = new Thickness(20),
                Padding = new Thickness(10),
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
            };

            layoutPanel.AddChild(border);

            var propertyEditor = new PropertyEditor
            {
                TargetObject = exampleObject,
                Margin = new Thickness(20)
            };



            border.Content = propertyEditor;
        }


    }
}