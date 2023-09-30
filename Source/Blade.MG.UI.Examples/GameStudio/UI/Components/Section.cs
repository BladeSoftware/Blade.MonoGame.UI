using Blade.UI;
using Blade.UI.Components;
using Blade.UI.Controls;
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

            //var layoutPanel = new StackPanel()
            //{
            //    Orientation = Orientation.Vertical,

            //HorizontalAlignment = HorizontalAlignmentType.Stretch;
            //VerticalAlignment = VerticalAlignmentType.Stretch;
            //Height = "100px";

            //    //HorizontalContentAlignment = HorizontalAlignmentType.Left,
            //    //VerticalContentAlignment = VerticalAlignmentType.Top,

            //    //Margin = new Thickness(50, 50, 50, 50),
            //    //Padding = new Thickness(50, 50, 50, 50),


            //    //MaxHeight = 300,
            //    //MinHeight = 300

            //    //MaxWidth = 300
            //    //MinHeight = 300

            //};


            //border.BorderThickness = 1;
            //border.CornerRadius = 10;
            //border.BorderColor = Color.SlateBlue;
            //border.Background = Color.White;
            //border.Margin = new Thickness(5, 5, 5, 5);


            var border = new Border()
            {
                BorderThickness = 1,
                CornerRadius = 10,
                BorderColor = Color.SlateBlue,
                Background = Color.White,
                Margin = new Thickness(5, 5, 5, 5),
                Elevation = 3
            };

            base.Content = border;

            border.Content = this.Content;



            // Vertical Alignment = Top
            //layoutPanel.AddChild(
            //    new Control()
            //    {
            //        Background = Color.LightBlue,
            //        Height = 100,

            //        HorizontalAlignment = HorizontalAlignmentType.Stretch,
            //        VerticalAlignment = VerticalAlignmentType.Stretch,

            //        Content = new Label()
            //        {
            //            Text = "This is a label",
            //            TextColor = Color.Black,
            //            HorizontalAlignment = HorizontalAlignmentType.Left,
            //            VerticalAlignment = VerticalAlignmentType.Top
            //        }
            //    });


        }

        public override void Measure(UIContext context, ref Size availableSize, ref Layout parentMinMax)
        {
            base.Measure(context, ref availableSize, ref parentMinMax);
        }

        public override void Arrange(UIContext context, Rectangle layoutBounds, Rectangle parentLayoutBounds)
        {
            base.Arrange(context, layoutBounds, parentLayoutBounds);
        }

        public override void RenderControl(UIContext context, Rectangle layoutBounds, Transform parentTransform)
        {
            base.RenderControl(context, layoutBounds, parentTransform);

            //Content?.RenderControl(context, layoutBounds, parentTransform);
        }

    }
}
