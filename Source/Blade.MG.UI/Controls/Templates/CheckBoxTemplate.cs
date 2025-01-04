using Blade.MG.SVG;
using Blade.MG.UI.Components;
using Blade.MG.UI.Events;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blade.MG.UI.Controls.Templates
{
    public class CheckBoxTemplate : Control
    {
        private Label label1;

        private Color backgroundNormal = Color.White;
        private Color backgroundHover = Color.LightGray;
        private Color backgroundFocused = Color.White;

        private Color textColorNormal = Color.Black;
        private Color textColorHover = Color.Black;
        private Color textColorFocused = Color.Black;


        public CheckBoxTemplate()
        {
        }

        protected override void InitTemplate()
        {
            base.InitTemplate();

            var checkbox = ParentAs<CheckBox>();

            label1 = new Label()
            {
                Text = checkbox.Text, // Use the Button Text
                //TextColor = textBox.FontColor,  // Use the Button Foreground Color
                FontName = checkbox.FontName, // Use the parent Font
                FontSize = checkbox.FontSize, // Use the parent Font
                Margin = new Thickness(25, 5, 2, 5),

                TextColor = checkbox.Theme.Primary,
            };

            Content = label1;
        }

        // ---=== Handle State Changes ===---

        public override async Task HandleFocusChangedEventAsync(UIWindow uiWindow, UIFocusChangedEvent uiEvent)
        {
            await base.HandleFocusChangedEventAsync(uiWindow, uiEvent);

            StateHasChanged();
        }

        public override async Task HandleHoverChangedAsync(UIWindow uiWindow, UIHoverChangedEvent uiEvent)
        {
            await base.HandleHoverChangedAsync(uiWindow, uiEvent);

            StateHasChanged();
        }

        protected override void HandleStateChange()
        {
            //await base.HandleStateChangeAsync();

            // Normal State
            label1.TextColor.Value = textColorNormal;
            Background.Value = backgroundNormal;

            // Hover State 
            if (MouseHover.Value)
            {
                label1.TextColor.Value = textColorHover;
                Background.Value = backgroundHover;
            }

            // Focused State
            if (HasFocus.Value)
            {
                label1.TextColor.Value = textColorFocused;
                Background.Value = backgroundFocused;
            }


        }

        // TODO: Dispose of Images
        private RenderTarget2D defaultCheckedImage;
        private RenderTarget2D iconCheckedImage;
        private RenderTarget2D iconUncheckedImage;
        private Rectangle boxRect;

        public override void Measure(UIContext context, ref Size availableSize, ref Layout parentMinMax)
        {
            base.Measure(context, ref availableSize, ref parentMinMax);
        }

        public override void Arrange(UIContext context, Rectangle layoutBounds, Rectangle parentLayoutBounds)
        {
            base.Arrange(context, layoutBounds, parentLayoutBounds);

            int boxWidth = 20;
            int boxHeight = 20;
            int padding = 0;

            boxRect = new Rectangle { X = FinalRect.X + 2, Y = label1.TextBaseLine.Y - boxHeight + 4, Width = boxWidth, Height = boxHeight };

            if (defaultCheckedImage == null)
            {
                // Default Checked Image
                var svg = new SVGDocument();
                //var path = svg.AddPath("M1837 557L768 1627l-557-558 90-90 467 466 979-978 90 90z");
                //var path = svg.AddPath("M16.972 6.251 c -0.967 -0.538 -2.185 -0.188 -2.72 0.777 l -3.713 6.682 -2.125 -2.125 c -0.781 -0.781 -2.047 -0.781 -2.828 0 c -0.781 0.781 -0.781 2.047 0 2.828 l4 4c.378.379.888.587 1.414.587l.277-.02c.621-.087 1.166-.46 1.471-1.009l5-9c.537-.966.189-2.183-.776-2.72z");
                var path = svg.AddPath("M16.972 6.251c-.967-.538-2.185-.188-2.72.777l-3.713 6.682-2.125-2.125c-.781-.781-2.047-.781-2.828 0-.781.781-.781 2.047 0 2.828l4 4c.378.379.888.587 1.414.587l.277-.02c.621-.087 1.166-.46 1.471-1.009l5-9c.537-.966.189-2.183-.776-2.72z");

                path.Fill = new SVGFill { FillColor = Color.White, FillRule = FillRule.Nonzero };
                defaultCheckedImage = svg.ToTexture2D(context.Game.GraphicsDevice, Matrix.CreateScale(1f), new Point(boxWidth * 20, boxHeight * 20), padding);
            }


            var checkbox = ParentAs<CheckBox>();

            //checkbox.CheckedIcon = "M7.4541,1.6942 C7.65606,1.20862 8.34393,1.20862 8.5459,1.6942 L10.1307,5.50456 C10.1667,5.59112 10.2481,5.65026 10.3416,5.65776 L14.4552,5.98754 C14.9794,6.02957 15.192,6.68377 14.7926,7.0259 L11.6584,9.71062 C11.5872,9.77161 11.5561,9.8673 11.5779,9.95849 L12.5354,13.9727 C12.6574,14.4842 12.1009,14.8885 11.6521,14.6144 L8.13031,12.4633 C8.05031,12.4144 7.94969,12.4144 7.86969,12.4633 L4.34786,14.6144 C3.89906,14.8885 3.34256,14.4842 3.46458,13.9727 L4.42211,9.95849 C4.44387,9.8673 4.41277,9.77161 4.34158,9.71062 L1.20744,7.0259 C0.80804,6.68377 1.0206,6.02957 1.54482,5.98754 L5.65843,5.65776 C5.75188,5.65026 5.83328,5.59112 5.86928,5.50456 L7.4541,1.6942 Z";
            //checkbox.UncheckedIcon = "M27.19,34a2.22,2.22,0,0,1-1.24-.38l-7.46-5a.22.22,0,0,0-.25,0l-7.46,5A2.22,2.22,0,0,1,7.4,31.21l2.45-8.64a.23.23,0,0,0-.08-.24L2.71,16.78a2.22,2.22,0,0,1,1.29-4l9-.34a.23.23,0,0,0,.2-.15l3.1-8.43a2.22,2.22,0,0,1,4.17,0l3.1,8.43a.23.23,0,0,0,.2.15l9,.34a2.22,2.22,0,0,1,1.29,4L27,22.33a.22.22,0,0,0-.08.24l2.45,8.64A2.23,2.23,0,0,1,27.19,34Zm-8.82-7.42A2.21,2.21,0,0,1,19.6,27l7.46,5a.22.22,0,0,0,.34-.25l-2.45-8.64a2.21,2.21,0,0,1,.77-2.35l7.06-5.55a.22.22,0,0,0-.13-.4l-9-.34a2.22,2.22,0,0,1-2-1.46l-3.1-8.43a.22.22,0,0,0-.42,0L15.06,13a2.22,2.22,0,0,1-2,1.46l-9,.34a.22.22,0,0,0-.13.4L11,20.76a2.22,2.22,0,0,1,.77,2.35L9.33,31.75a.21.21,0,0,0,.08.24.2.2,0,0,0,.26,0l7.46-5A2.22,2.22,0,0,1,18.36,26.62Z";

            //checkbox.CheckedIcon = "M311.050164,164.241829 C311.515694,164.468113 311.892625,164.838464 312.122929,165.295868 L316.099751,173.194183 L324.973849,174.469632 C326.268261,174.655674 327.164093,175.837503 326.974745,177.10932 C326.89957,177.614264 326.657566,178.080933 326.285988,178.437488 L319.8697,184.594364 L321.377373,193.280949 C321.597288,194.54801 320.730161,195.75033 319.440591,195.966406 C318.9286,196.052194 318.402102,195.970261 317.94215,195.733219 L309.999845,191.640063 L302.057539,195.733219 C300.899043,196.330263 299.467297,195.891509 298.859646,194.753235 C298.618394,194.301311 298.535005,193.784004 298.622316,193.280949 L300.129989,184.594364 L293.713702,178.437488 C292.777796,177.539421 292.760056,176.065936 293.674077,175.146367 C294.036966,174.781275 294.511926,174.543496 295.02584,174.469632 L303.899939,173.194183 L307.87676,165.295868 C308.456835,164.143789 309.877617,163.671879 311.050164,164.241829 Z";
            //checkbox.UncheckedIcon = "M311.050164,164.241829 C311.515694,164.468113 311.892625,164.838464 312.122929,165.295868 L316.099751,173.194183 L324.973849,174.469632 C326.268261,174.655674 327.164093,175.837503 326.974745,177.10932 C326.89957,177.614264 326.657566,178.080933 326.285988,178.437488 L319.8697,184.594364 L321.377373,193.280949 C321.597288,194.54801 320.730161,195.75033 319.440591,195.966406 C318.9286,196.052194 318.402102,195.970261 317.94215,195.733219 L309.999845,191.640063 L302.057539,195.733219 C300.899043,196.330263 299.467297,195.891509 298.859646,194.753235 C298.618394,194.301311 298.535005,193.784004 298.622316,193.280949 L300.129989,184.594364 L293.713702,178.437488 C292.777796,177.539421 292.760056,176.065936 293.674077,175.146367 C294.036966,174.781275 294.511926,174.543496 295.02584,174.469632 L303.899939,173.194183 L307.87676,165.295868 C308.456835,164.143789 309.877617,163.671879 311.050164,164.241829 Z M305.472113,175.320323 L295.368684,176.772461 L302.673821,183.78225 L300.95729,193.67219 L309.999845,189.012009 L319.042399,193.67219 L317.325868,183.78225 L324.631006,176.772461 L314.527576,175.320323 L309.999845,166.327854 L305.472113,175.320323 Z";

            //checkbox.CheckedIcon = "M 11.9688 52.2930 C 12.9298 53.0195 14.1485 52.7617 15.6016 51.7071 L 28.0001 42.6133 L 40.4220 51.7071 C 41.8751 52.7617 43.0704 53.0195 44.0548 52.2930 C 45.0157 51.5664 45.2267 50.3711 44.6407 48.6602 L 39.7422 34.0820 L 52.2578 25.0820 C 53.7112 24.0508 54.2968 22.9727 53.9219 21.8008 C 53.5470 20.6758 52.4454 20.1133 50.6406 20.1367 L 35.2891 20.2305 L 30.6251 5.5820 C 30.0626 3.8476 29.2188 2.9805 28.0001 2.9805 C 26.8048 2.9805 25.9610 3.8476 25.3985 5.5820 L 20.7344 20.2305 L 5.3829 20.1367 C 3.5782 20.1133 2.4766 20.6758 2.1016 21.8008 C 1.7032 22.9727 2.3126 24.0508 3.7657 25.0820 L 16.2813 34.0820 L 11.3829 48.6602 C 10.7969 50.3711 11.0079 51.5664 11.9688 52.2930 Z M 15.3438 47.6524 C 15.2969 47.6055 15.3204 47.5820 15.3438 47.4414 L 20.0079 34.0352 C 20.3126 33.1211 20.1485 32.3945 19.3282 31.8320 L 7.6563 23.7695 C 7.5391 23.6992 7.5157 23.6524 7.5391 23.5820 C 7.5626 23.5117 7.6095 23.5117 7.7501 23.5117 L 21.9298 23.7695 C 22.8907 23.7930 23.5001 23.3945 23.8048 22.4336 L 27.8595 8.8633 C 27.9063 8.7227 27.9532 8.6758 28.0001 8.6758 C 28.0704 8.6758 28.1173 8.7227 28.1407 8.8633 L 32.2188 22.4336 C 32.5001 23.3945 33.1329 23.7930 34.0938 23.7695 L 48.2733 23.5117 C 48.4139 23.5117 48.4610 23.5117 48.4845 23.5820 C 48.508 23.6524 48.4610 23.6992 48.3674 23.7695 L 36.6954 31.8320 C 35.8751 32.3945 35.6876 33.1211 36.0157 34.0352 L 40.6798 47.4414 C 40.7032 47.5820 40.7266 47.6055 40.6798 47.6524 C 40.6329 47.7227 40.5626 47.6758 40.4688 47.6055 L 29.1954 39.0039 C 28.4454 38.4180 27.5782 38.4180 26.8282 39.0039 L 15.5548 47.6055 C 15.4610 47.6758 15.3907 47.7227 15.3438 47.6524 Z";
            //checkbox.UncheckedIcon = "M 11.9688 52.2930 C 12.9298 53.0195 14.1485 52.7617 15.6016 51.7071 L 28.0001 42.6133 L 40.4220 51.7071 C 41.8751 52.7617 43.0704 53.0195 44.0548 52.2930 C 45.0157 51.5664 45.2267 50.3711 44.6407 48.6602 L 39.7422 34.0820 L 52.2578 25.0820 C 53.7112 24.0508 54.2968 22.9727 53.9219 21.8008 C 53.5470 20.6758 52.4454 20.1133 50.6406 20.1367 L 35.2891 20.2305 L 30.6251 5.5820 C 30.0626 3.8476 29.2188 2.9805 28.0001 2.9805 C 26.8048 2.9805 25.9610 3.8476 25.3985 5.5820 L 20.7344 20.2305 L 5.3829 20.1367 C 3.5782 20.1133 2.4766 20.6758 2.1016 21.8008 C 1.7032 22.9727 2.3126 24.0508 3.7657 25.0820 L 16.2813 34.0820 L 11.3829 48.6602 C 10.7969 50.3711 11.0079 51.5664 11.9688 52.2930 Z M 15.3438 47.6524 C 15.2969 47.6055 15.3204 47.5820 15.3438 47.4414 L 20.0079 34.0352 C 20.3126 33.1211 20.1485 32.3945 19.3282 31.8320 L 7.6563 23.7695 C 7.5391 23.6992 7.5157 23.6524 7.5391 23.5820 C 7.5626 23.5117 7.6095 23.5117 7.7501 23.5117 L 21.9298 23.7695 C 22.8907 23.7930 23.5001 23.3945 23.8048 22.4336 L 27.8595 8.8633 C 27.9063 8.7227 27.9532 8.6758 28.0001 8.6758 C 28.0704 8.6758 28.1173 8.7227 28.1407 8.8633 L 32.2188 22.4336 C 32.5001 23.3945 33.1329 23.7930 34.0938 23.7695 L 48.2733 23.5117 C 48.4139 23.5117 48.4610 23.5117 48.4845 23.5820 C 48.508 23.6524 48.4610 23.6992 48.3674 23.7695 L 36.6954 31.8320 C 35.8751 32.3945 35.6876 33.1211 36.0157 34.0352 L 40.6798 47.4414 C 40.7032 47.5820 40.7266 47.6055 40.6798 47.6524 C 40.6329 47.7227 40.5626 47.6758 40.4688 47.6055 L 29.1954 39.0039 C 28.4454 38.4180 27.5782 38.4180 26.8282 39.0039 L 15.5548 47.6055 C 15.4610 47.6758 15.3907 47.7227 15.3438 47.6524 Z";

            //checkbox.CheckedIcon = "M62 25.154H39.082L32 3l-7.082 22.154H2l18.541 13.693L13.459 61L32 47.309L50.541 61l-7.082-22.152L62 25.154z";
            //checkbox.UncheckedIcon = "M19.38 12.803l-3.38-10.398-3.381 10.398h-11.013l8.925 6.397-3.427 10.395 8.896-6.448 8.895 6.448-3.426-10.395 8.925-6.397h-11.014zM20.457 19.534l2.394 7.261-6.85-4.965-6.851 4.965 2.64-8.005-0.637-0.456-6.228-4.464h8.471l2.606-8.016 2.605 8.016h8.471l-6.864 4.92 0.245 0.744z";



            if (checkbox.CheckedIcon != null && iconCheckedImage == null)
            {
                var svg = new SVGDocument();
                var path = svg.AddPath(checkbox.CheckedIcon);

                path.Fill = new SVGFill { FillColor = Color.White, FillRule = FillRule.Nonzero };
                iconCheckedImage = svg.ToTexture2D(context.Game.GraphicsDevice, Matrix.CreateScale(1f), new Point(boxWidth * 2, boxHeight * 2), padding);
            }

            if (checkbox.UncheckedIcon != null && iconUncheckedImage == null)
            {
                var svg = new SVGDocument();
                var path = svg.AddPath(checkbox.UncheckedIcon);

                //path.Fill = new SVGFill { FillColor = Color.White, FillRule = FillRule.Nonzero };
                path.Stroke = Color.White;
                path.StrokeWidth = 2.5f;
                iconUncheckedImage = svg.ToTexture2D(context.Game.GraphicsDevice, Matrix.CreateScale(1f), new Point(boxWidth * 1, boxHeight * 1), padding);
            }

        }

        public override void RenderControl(UIContext context, Rectangle layoutBounds, Transform parentTransform)
        {
            base.RenderControl(context, layoutBounds, parentTransform);

            var checkbox = ParentAs<CheckBox>();


            Color primaryColor = Color.Red;
            Color secondaryColor = Color.White;
            Color disabledColor = Color.LightGray;
            Color? uncheckedColor = null; // Color.Gray;


            Color checkedColor = checkbox.IsChecked?.Value switch
            {
                true => primaryColor,
                false => Color.Transparent,
                null => primaryColor
            };

            if (!checkbox.IsEnabled.Value)
            {
                primaryColor = disabledColor;
                checkedColor = disabledColor;
            }

            Color borderColor = checkbox.IsChecked?.Value switch
            {
                true => primaryColor,
                false => uncheckedColor ?? primaryColor,
                null => primaryColor
            };


            try
            {
                using var spriteBatch = context.Renderer.BeginBatch(transform: parentTransform);
                context.Renderer.ClipToRect(layoutBounds);

                //context.Renderer.FillRoundedRect(spriteBatch, boxRect, boxRect.Width / 4, Color.LightGray);
                //context.Renderer.DrawRoundedRect(spriteBatch, boxRect, boxRect.Width / 4, Color.Black, 2f);

                if (iconCheckedImage == null)
                {
                    context.Renderer.FillRoundedRect(spriteBatch, boxRect, 2, checkedColor);
                    context.Renderer.DrawRoundedRect(spriteBatch, boxRect, 2, borderColor, 2f);
                }


                if (checkbox.IsChecked?.Value == null)
                {
                    // Indeterminate
                    var insideRect = boxRect;
                    insideRect.Inflate(-6f, -6f);

                    //context.Renderer.FillRoundedRect(spriteBatch, insideRect, 4, new Color(Color.Black, 1f));
                    //context.Renderer.FillRoundedRect(spriteBatch, insideRect, 4, new Color(Color.White, 1f));
                    context.Renderer.FillRect(spriteBatch, insideRect, secondaryColor);
                }
                else if (checkbox.IsChecked?.Value == true)
                {
                    // Checked
                    if (iconCheckedImage != null)
                    {
                        //boxRect.Inflate(-2f, -2f);
                        spriteBatch.Draw(iconCheckedImage, boxRect, primaryColor);
                    }
                    else if (defaultCheckedImage != null)
                    {
                        boxRect.Inflate(-2f, -2f);
                        spriteBatch.Draw(defaultCheckedImage, boxRect, secondaryColor);
                    }
                }
                else if (checkbox.IsChecked?.Value == false)
                {
                    // Unchecked
                    if (iconUncheckedImage != null)
                    {
                        //boxRect.Inflate(-2f, -2f);
                        spriteBatch.Draw(iconUncheckedImage, boxRect, primaryColor);
                    }
                }

            }
            finally
            {
                context.Renderer.EndBatch();
            }

        }

    }
}
