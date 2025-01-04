using Blade.MG.UI.Components;
using Blade.MG.UI.Events;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blade.MG.UI.Controls
{
    public class ScrollBar : Control
    {
        public Orientation Orientation { get; set; } = Orientation.Vertical;

        private int maxValue = 100;
        public int MaxValue { get => maxValue; set => maxValue = Math.Max(0, value); }

        public int ScrollOfset { get; set; } = 0;

        public int BarThickness { get; set; } = 20;

        private int barMargin { get; set; } = 6;

        //public Color Background { get; set; }
        public Color GrabHandle { get; set; }

        public Color EndCaps { get; set; }

        public bool dragging = false;
        public bool mouseHover = false;

        public ScrollBar()
        {
            IsHitTestVisible = true;

            Background = Color.LightGray;
            GrabHandle = Color.Gray;

            EndCaps = Color.Gray;

            //EndCaps = new Color((Background.R + GrabHandle.R) / 2, (Background.G + GrabHandle.G) / 2, (Background.B + GrabHandle.B) / 2);
        }

        public override void Measure(UIContext context, ref Size availableSize, ref Layout parentMinMax)
        {
            if (ScrollOfset > MaxValue)
            {
                ScrollOfset = MaxValue;
            }

            if (Orientation == Orientation.Horizontal)
            {
                Width = float.NaN;
                Height = BarThickness;
                HorizontalAlignment = HorizontalAlignmentType.Stretch;
                VerticalAlignment = VerticalAlignmentType.Bottom;
            }
            else // (Orientation == Orientation.Vertical)
            {
                Width = BarThickness;
                Height = float.NaN;
                HorizontalAlignment = HorizontalAlignmentType.Right;
                VerticalAlignment = VerticalAlignmentType.Stretch;
            }

            base.Measure(context, ref availableSize, ref parentMinMax);
        }

        /// <summary>
        /// Layout Children
        /// </summary>
        /// <param name="layoutBounds">Size of Parent Container</param>
        public override void Arrange(UIContext context, Rectangle layoutBounds, Rectangle parentLayoutBounds)
        {
            bool saveWidthVirtual = Parent.IsWidthVirtual;
            bool saveHeightVirtual = Parent.IsHeightVirtual;

            // Ignore the Virtualized Flags when laying out the Scrollbars
            Parent.IsWidthVirtual = false;
            Parent.IsHeightVirtual = false;

            base.Arrange(context, layoutBounds, parentLayoutBounds);

            Parent.IsWidthVirtual = saveWidthVirtual;
            Parent.IsHeightVirtual = saveHeightVirtual;

        }

        public override void RenderControl(UIContext context, Rectangle layoutBounds, Transform parentTransform)
        {
            if (Visible.Value != Visibility.Visible)
            {
                return;
            }

            base.RenderControl(context, FinalRect, parentTransform);

            float scrollFactor = 0f;

            if (MaxValue > 0)
            {
                scrollFactor = ScrollOfset / (float)MaxValue;
            }

            if (scrollFactor < 0)
            {
                scrollFactor = 0;
            }
            if (scrollFactor > 1)
            {
                scrollFactor = 1;
            }

            int minScrollGrabSize = 20;
            int scrollGrabSize = 20;
            int endcapLength = 20;

            try
            {
                using var spriteBatch = context.Renderer.BeginBatch(transform: parentTransform);

                Rectangle rect = FinalRect;
                if (Orientation == Orientation.Horizontal)
                {
                    // Horizontal Scrollbar
                    int fullWidth = rect.Width; // + BarThickness;

                    scrollGrabSize = rect.Width - MaxValue - 2 * endcapLength;
                    if (scrollGrabSize < minScrollGrabSize) scrollGrabSize = minScrollGrabSize;

                    context.Renderer.FillRect(spriteBatch, new Rectangle(rect.Left, rect.Bottom - BarThickness, fullWidth, BarThickness), Background.Value);
                    ////context.Renderer.FillRoundedRect(new Rectangle(rect.Left, rect.Bottom - BarThickness, rect.Width, BarThickness), 5f, Background);


                    // Draw Triangle endcaps
                    context.Renderer.FillRect(spriteBatch, new Rectangle(rect.Left, rect.Bottom - BarThickness, BarThickness, BarThickness), Background.Value);
                    context.Renderer.FillRect(spriteBatch, new Rectangle(rect.Right - BarThickness, rect.Bottom - BarThickness, BarThickness, BarThickness), Background.Value);

                    //var arrowTexture = Content.Load<Texture2D>("Images/arrow_right_20x20");
                    //var arrowTexture = context.Game.Content.Load<Texture2D>("Images/arrow_right_small");
                    var arrowTexture = context.LoadContent<Texture2D>("Images/arrow_right_small");

                    spriteBatch.Draw(arrowTexture, new Vector2(rect.Left + BarThickness, rect.Bottom - BarThickness), null, EndCaps, 0f, new Vector2(0, 0), new Vector2(-1f, 1f), SpriteEffects.None, 0f);
                    spriteBatch.Draw(arrowTexture, new Vector2(rect.Right - BarThickness, rect.Bottom - BarThickness), null, EndCaps, 0f, new Vector2(0, 0), new Vector2(1f, 1f), SpriteEffects.None, 0f);

                    int dist = (int)((rect.Width - 2 * endcapLength - scrollGrabSize) * scrollFactor);
                    if (dist >= 0)
                    {
                        //context.Renderer.FillRect(new Rectangle(rect.Left + dist, rect.Bottom - BarThickness, minScrollGrabSize, BarThickness), GrabHandle);
                        context.Renderer.FillRoundedRect(spriteBatch, new Rectangle(rect.Left + endcapLength + dist, rect.Bottom - BarThickness + barMargin, scrollGrabSize, BarThickness - barMargin * 2), (BarThickness - 2 * barMargin) / 2f, GrabHandle);
                    }
                }
                else
                {
                    // Vertical Scrollbar
                    int fullHeight = rect.Height;// + BarThickness;

                    scrollGrabSize = rect.Height - MaxValue - 2 * endcapLength;
                    if (scrollGrabSize < minScrollGrabSize) scrollGrabSize = minScrollGrabSize;

                    context.Renderer.FillRect(spriteBatch, new Rectangle(rect.Right - BarThickness, rect.Top, BarThickness, fullHeight), Background.Value);
                    ////context.Renderer.FillRoundedRect(new Rectangle(rect.Right - BarThickness, rect.Top, BarThickness, rect.Height), 5f, Background);

                    // Draw Triangle endcaps
                    context.Renderer.FillRect(spriteBatch, new Rectangle(rect.Right - BarThickness, rect.Top, BarThickness, endcapLength), Background.Value);
                    context.Renderer.FillRect(spriteBatch, new Rectangle(rect.Right - BarThickness, rect.Bottom - endcapLength, BarThickness, endcapLength), Background.Value);

                    //var arrowTexture = context.Content.Load<Texture2D>("Images/arrow_up_20x20");
                    //var arrowTexture = context.Game.Content.Load<Texture2D>("Images/arrow_up_small");
                    var arrowTexture = context.LoadContent<Texture2D>("Images/arrow_up_small");

                    spriteBatch.Draw(arrowTexture, new Vector2(rect.Right - BarThickness, rect.Top), null, EndCaps, 0f, new Vector2(0, 0), 1f, SpriteEffects.None, 0f);
                    spriteBatch.Draw(arrowTexture, new Vector2(rect.Right - BarThickness, rect.Bottom), null, EndCaps, 0f, new Vector2(0, 0), new Vector2(1f, -1f), SpriteEffects.None, 0f);

                    //context.SpriteBatch.Draw(UIRenderer.FilledTriangleTexture(context.SpriteBatch), new Vector2(rect.Right - BarThickness, rect.Top), null, GrabHandle, 0f, new Vector2(0, 0), 1f, SpriteEffects.None, 0f);
                    //context.SpriteBatch.Draw(UIRenderer.FilledTriangleTexture(context.SpriteBatch), new Vector2(rect.Right - BarThickness, rect.Bottom - endcapLength), null, GrabHandle, 0f, new Vector2(0, 0), 1f, SpriteEffects.None, 0f);

                    int dist = (int)((rect.Height - 2 * endcapLength - scrollGrabSize) * scrollFactor);
                    if (dist >= 0)
                    {
                        //context.Renderer.FillRect(new Rectangle(rect.Right - BarThickness, rect.Top + dist, BarThickness, minScrollGrabSize), GrabHandle);
                        context.Renderer.FillRoundedRect(spriteBatch, new Rectangle(rect.Right - BarThickness + barMargin, rect.Top + endcapLength + dist, BarThickness - barMargin * 2, scrollGrabSize), (BarThickness - 2 * barMargin) / 2f, GrabHandle);
                    }
                }

            }
            finally
            {
                context.Renderer.EndBatch();

            }

        }

        ///// <summary>
        ///// Returns the UIWindow this control is bound to
        ///// </summary>
        //public UIWindow ParentWindow
        //{
        //    get
        //    {
        //        UIComponent component = this.Parent;
        //        while (component != null)
        //        {
        //            if (component is UIWindow)
        //            {
        //                return component as UIWindow;
        //            }

        //            component = component.Parent;
        //        }
        //        return null;
        //    }
        //}



        public override async Task HandleHoverChangedAsync(UIWindow uiWindow, UIHoverChangedEvent uiEvent)
        {
            await base.HandleHoverChangedAsync(uiWindow, uiEvent);

            if (uiEvent.Hover != mouseHover)
            {
                mouseHover = uiEvent.Hover;
                StateHasChanged();
            }

        }

        public override async Task HandleMouseDownEventAsync(UIWindow uiWindow, UIMouseDownEvent uiEvent)
        {
            if (uiEvent.Handled) return;

            if (uiEvent.PrimaryButton.Pressed && FinalRect.Contains(uiEvent.X, uiEvent.Y))
            {
                LockEventsToControl(uiWindow, this);

                dragging = true;
                dragStartX = uiEvent.X;
                dragStartY = uiEvent.Y;

                HandleDrag(uiEvent.X, uiEvent.Y);

                uiEvent.Handled = true;

                StateHasChanged();

                return;
            }

            await base.HandleMouseDownEventAsync(uiWindow, uiEvent);
        }

        public override async Task HandleMouseUpEventAsync(UIWindow uiWindow, UIMouseUpEvent uiEvent)
        {
            if (uiEvent.Handled) return;

            if (dragging)
            {
                UnlockEventsFromControl(uiWindow, this);

                dragging = false;
                uiEvent.Handled = true;

                StateHasChanged();

                return;
            }

            await base.HandleMouseUpEventAsync(uiWindow, uiEvent);
        }

        public override async Task HandleMouseWheelScrollEventAsync(UIWindow uiWindow, UIMouseWheelScrollEvent uiEvent)
        {
            if (uiEvent.Handled) return;

            // Handle mouse scroll event
            if (Visible.Value == Visibility.Visible && Orientation == Orientation.Horizontal && uiEvent.HorizontalScroll != 0)
            {
                ScrollOfset += uiEvent.HorizontalScroll / 2;  // Control direction and scroll speed
                uiEvent.Handled = true;
            }
            else if (Visible.Value == Visibility.Visible && Orientation == Orientation.Vertical && uiEvent.VerticalScroll != 0)
            {
                ScrollOfset -= uiEvent.VerticalScroll / 2;   // Control direction and scroll speed
                uiEvent.Handled = true;
            }

            // Make sure we don't exceed the limits
            if (ScrollOfset < 0) ScrollOfset = 0;
            if (ScrollOfset > MaxValue) ScrollOfset = MaxValue;


            await base.HandleMouseWheelScrollEventAsync(uiWindow, uiEvent);
        }

        public override async Task HandleMouseMoveEventAsync(UIWindow uiWindow, UIMouseMoveEvent uiEvent)
        {
            if (uiEvent.Handled) return;

            if (dragging)
            {
                HandleDrag(uiEvent.X, uiEvent.Y);

                uiEvent.Handled = true;
                return;
            }

            await base.HandleMouseMoveEventAsync(uiWindow, uiEvent);
        }


        private int dragStartX;
        private int dragStartY;

        private void HandleDrag(int mouseX, int mouseY)
        {
            float factor;

            if (Orientation == Orientation.Horizontal)
            {
                factor = (mouseX - FinalRect.Left) / (float)(FinalRect.Right - FinalRect.Left);
            }
            else
            {
                factor = (mouseY - FinalRect.Top) / (float)(FinalRect.Bottom - FinalRect.Top);
            }

            ScrollOfset = (int)(MaxValue * factor);

            if (ScrollOfset < 0) ScrollOfset = 0;
            if (ScrollOfset > MaxValue) ScrollOfset = MaxValue;

        }

        protected override void HandleStateChange()
        {
            //await base.HandleStateChangeAsync();

            if (mouseHover || dragging)
            {
                barMargin = 6;
                EndCaps = Color.Gray;
                GrabHandle = Color.Gray;
            }
            else
            {
                barMargin = 8;
                //EndCaps = Color.LightGray;
                //GrabHandle = new Color((Color.LightGray.R + Color.Gray.R) / 2, (Color.LightGray.G + Color.Gray.G) / 2, (Color.LightGray.B + Color.Gray.B) / 2);

                EndCaps = new Color(Color.Gray, 0.2f);
                GrabHandle = new Color(Color.Gray, 0.75f);
            }
        }
    }
}
