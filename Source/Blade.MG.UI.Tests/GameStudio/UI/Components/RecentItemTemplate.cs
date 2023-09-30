//using Blade.UI;
//using Blade.UI.Components;
//using Blade.UI.Controls;
//using Blade.UI.Events;
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
//using System.Threading.Tasks;

//namespace GameStudio.UI.Components
//{
//    public class RecentItemTemplate : Control
//    {
//        public Grid grid { get; set; }
//        public Label label0;
//        public Label label1;

//        private Color backgroundNormal = new Color(Color.DarkSlateBlue, 0.55f);
//        private Color backgroundHover = new Color(Color.SlateBlue, 0.80f);
//        private Color backgroundFocused = new Color(Color.SlateBlue, 0.80f);

//        private Color textColorNormal = Color.White;
//        private Color textColorHover = Color.White;
//        private Color textColorFocused = Color.White;


//        private Color borderColorNormal = Color.Orange;

//        private int borderThicknessNormal = 2;

//        private Binding<SpriteFont> SpriteFont { get; set; }

//        public RecentItemTemplate()
//        {
//        }

//        protected override void InitTemplate()
//        {
//            base.InitTemplate();

//            HitTestVisible = true;

//            RecentItem item = DataContext as RecentItem;

//            this.HorizontalAlignment = Parent.HorizontalAlignment;
//            this.VerticalAlignment = Parent.VerticalAlignment;
//            this.HorizontalContentAlignment = HorizontalAlignmentType.Left; //Parent.HorizontalContentAlignment;
//            this.VerticalContentAlignment = VerticalAlignmentType.Center; //Parent.VerticalContentAlignment;


//            grid = new Grid();
//            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 1f) });
//            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridUnitType.Star, 3f) });
//            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridUnitType.Auto) });


//            label0 = new Label()
//            {
//                Text = item?.Name,
//                TextColor = Color.Black,
//                Background = Color.Transparent,
//                //Width = 100,
//                //Height = 100,
//                Margin = new Thickness(10, 0, 0, 0)

//                //SpriteFont = button.SpriteFont // Use the Button Font
//            };

//            label1 = new Label()
//            {
//                Text = item?.Path,
//                TextColor = Color.Black,
//                Background = Color.Transparent,
//                //Width = 100,
//                //Height = 100,
//                Margin = new Thickness(10, 0, 0, 0)

//                //SpriteFont = button.SpriteFont // Use the Button Font
//            };

//            // Use the Parent Button's ContentAlignment values for the lable text placement
//            label1.HorizontalContentAlignment = HorizontalContentAlignment;
//            label1.VerticalContentAlignment = VerticalContentAlignment;


//            grid.AddChild(label0, 0, 0);
//            grid.AddChild(label1, 1, 0);

//            grid.Padding = new Thickness(0, 5);

//            Content = grid;
//        }

//        // ---=== Handle State Changes ===---

//        public override async Task HandleFocusChangedEventAsync(UIWindow uiWindow, UIFocusChangedEvent uiEvent)
//        {
//            HasFocus = uiEvent.Focused;

//            await base.HandleFocusChangedEventAsync(uiWindow, uiEvent);
//            HandleStateChange();
//        }

//        public override async Task HandleHoverChangedAsync(UIWindow uiWindow, UIHoverChangedEvent uiEvent)
//        {
//            MouseHover = uiEvent.Hover;

//            await base.HandleHoverChangedAsync(uiWindow, uiEvent);
//            HandleStateChange();
//        }

//        protected override void HandleStateChange()
//        {
//            ListView parent = Parent as ListView;


//            // Normal State
//            ((Grid)Content).Background = Color.Transparent;
//            //label1.TextColor.Value = textColorNormal;
//            //border1.Background.Value = backgroundNormal;
//            //border1.BorderThickness.Value = borderThicknessNormal;
//            //border1.BorderColor.Value = borderColorNormal;


//            //// Focused State
//            //if (Focused.Value)
//            //{
//            //    label1.TextColor.Value = textColorFocused;
//            //    border1.Background.Value = backgroundFocused;
//            //    border1.BorderThickness.Value = borderThicknessFocused;
//            //    border1.BorderColor.Value = borderColorFocused;
//            //}


//            // Hover State 
//            if (MouseHover.Value)
//            {
//                //label1.TextColor.Value = textColorHover;
//                //border1.Background.Value = backgroundHover;
//                //border1.BorderThickness.Value = borderThicknessHover;
//                //border1.BorderColor.Value = borderColorHover;

//                ((Grid)Content).Background = Color.LightBlue;
//            }


//            // Selected State
//            if (this.DataContext != null && parent?.SelectedItem == this.DataContext)
//            {
//                ((Grid)Content).Background = Color.CornflowerBlue;
//            }
//        }

//    }
//}
