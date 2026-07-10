using Blade.MG.UI;
using Microsoft.Xna.Framework;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Examples.UI.Components;
using GameStudio;

namespace Examples.UI.HelpPages
{
    public class HelpPage_PropertyEditor : Panel
    {
        public class ExampleSettings
        {
            public string Name { get; set; }
            public int Health { get; set; }
            public bool IsActive { get; set; }
            public DifficultyLevel Difficulty { get; set; }
            public Country Country { get; set; }
        }

        public enum DifficultyLevel
        {
            Easy,
            Medium,
            Hard
        }

        protected override void InitTemplate()
        {
            base.InitTemplate();

            var exampleObject = new ExampleSettings
            {
                Name = "Player",
                Health = 100,
                IsActive = true,
                Difficulty = DifficultyLevel.Medium,
                Country = Country.Canada,
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
                    Title = "Property Editor",
                    Description = "A reflection-driven property grid, generated from a plain object."
                });

            var border = new Border()
            {
                BorderThickness = new Thickness(1),
                BorderColor = new Binding<Color>(() => Theme.Outline),
                Margin = new Thickness(30, 20, 30, 20),
                Padding = new Thickness(10),
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
                Height = 500,
            };

            layoutPanel.AddChild(border);
            layoutPanel.StretchLastChild = true;

            var propertyEditor = new PropertyEditor
            {
                TargetObject = exampleObject,
                Margin = new Thickness(20),
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch,
            };

            border.Content = propertyEditor;
        }

    }
}
