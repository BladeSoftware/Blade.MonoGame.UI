using System;
using System.Collections.Generic;
using System.Text;
using BladeGame.BladeUI.Components;
using BladeGame.Shared.BladeUI;
using BladeGame.Shared.BladeUI.Components;
using BladeGame.Shared.BladeUI.Events;
using Microsoft.Xna.Framework;

namespace BladeGame.BladeUI
{
    public class Control : UIComponent
    {
        public Binding<bool> Focused = false;
        public Binding<bool> MouseHover = false;

        private UIComponent content;
        public UIComponent Content
        {
            get
            {
                return content;
            }

            set
            {
                content = value;
                if (content != null)
                {
                    content.Parent = this;
                }
                
            }
        }


        public override Rectangle GetChildBoundingBox(UIComponent child)
        {
            //float x = 0;
            //float w = 0;
            //float y = 0;
            //float h = 0;

            //x = Left + Padding.Left;
            //y = Top + Padding.Top;

            //w = ActualWidth - Padding.Left - Padding.Right;
            //h = ActualHeight - Padding.Top - Padding.Bottom;

            //return new Rectangle((int)x, (int)y, (int)w, (int)h);
            return finalContentRect;
        }

        //public override void Measure(UIContext context, Size availableSize)
        //{
        //    base.Measure(context, availableSize);

        //    DesiredSize = new Size(DesiredSize.Width + Padding.Left + Padding.Right, DesiredSize.Height + Padding.Top + Padding.Bottom);

        //    ClampDesiredSize(availableSize);
        //}

        public override void Measure(UIContext context, Size availableSize, ref Layout parentMinMax)
        {
            base.Measure(context, availableSize, ref parentMinMax);

            float desiredWidth = float.NaN;
            float desiredHeight = float.NaN;

            //availableSize = new Size(availableSize.Width - Padding.Left - Padding.Right, availableSize.Height - Padding.Top - Padding.Bottom);

            var child = Content;

            child.Measure(context, availableSize, ref parentMinMax);

            if (!float.IsNaN(child.DesiredSize.Width))
            {
                if (float.IsNaN(desiredWidth))
                {
                    desiredWidth = child.DesiredSize.Width;
                }
                else if (child.DesiredSize.Width > desiredWidth)
                {
                    desiredWidth = child.DesiredSize.Width;
                }
            }

            if (!float.IsNaN(child.DesiredSize.Height))
            {
                if (float.IsNaN(desiredHeight))
                {
                    desiredHeight = child.DesiredSize.Height;
                }
                else if (child.DesiredSize.Height > desiredHeight)
                {
                    desiredHeight = child.DesiredSize.Height;
                }
            }


            if (!float.IsNaN(Width))
            {
                desiredWidth = Width;
            }

            if (!float.IsNaN(Height))
            {
                desiredHeight = Height;
            }


            desiredWidth += Margin.Value.Left + Margin.Value.Right;
            desiredHeight += Margin.Value.Top + Margin.Value.Bottom;

            desiredWidth += Padding.Value.Left + Padding.Value.Right;
            desiredHeight += Padding.Value.Top + Padding.Value.Bottom;

            DesiredSize = new Size(desiredWidth, desiredHeight);

            //DesiredSize = new Size(desiredWidth + Padding.Left + Padding.Right, desiredHeight + Padding.Top + Padding.Bottom);

            ClampDesiredSize(availableSize, parentMinMax);
        }

        public override void Arrange(Rectangle layoutBounds)
        {
            base.Arrange(layoutBounds);


            int left = finalRect.Left + Padding.Value.Left;
            int top = finalRect.Top + Padding.Value.Top;
            int right = finalRect.Right - Padding.Value.Right;
            int bottom = finalRect.Bottom - Padding.Value.Bottom;

            if (left < finalRect.Left) left = finalRect.Left;
            if (left > finalRect.Right) left = finalRect.Right;
            if (right < finalRect.Left) right = finalRect.Left;
            if (right > finalRect.Right) right = finalRect.Right;
            if (top < finalRect.Top) top = finalRect.Top;
            if (top > finalRect.Bottom) top = finalRect.Bottom;
            if (bottom < finalRect.Top) bottom = finalRect.Top;
            if (bottom > finalRect.Bottom) bottom = finalRect.Bottom;

            int width = right - left;// + 1;
            int height = bottom - top;// + 1;

            if (width < 0) width = 0;
            if (height < 0) height = 0;

            finalContentRect = new Rectangle(left, top, width, height);

            if (Content != null)
            {
                Content.Arrange(finalContentRect);
            }
        }

        public override void RenderControl(UIContext context, Rectangle layoutBounds)
        {
            base.RenderControl(context, layoutBounds);

            // Render Content control
            if (Content != null)
            {
                Content.RenderControl(context, finalRect);
            }
        }

        // ---=== UI Events ===---

        public virtual void HandleFocusChangedEvent(UIFocusChangedEvent uiEvent)
        {
            if (uiEvent.Handled)
            {
                return;
            }

            Control contentCtrl = Content as Control;

            if (contentCtrl != null)
            {
                contentCtrl.HandleFocusChangedEvent(uiEvent);
            }

            Focused = uiEvent.Focused;
        }

        public virtual void HandleHoverChanged(UIHoverChangedEvent uiEvent)
        {
            if (uiEvent.Handled)
            {
                return;
            }

            Control contentCtrl = Content as Control;

            if (contentCtrl != null)
            {
                contentCtrl.HandleHoverChanged(uiEvent);
            }

            MouseHover = uiEvent.Hover;
        }

        public virtual void HandleMouseDownEvent(UIEvent uiEvent)
        {
            if (uiEvent.Handled)
            {
                return;
            }

            Control contentCtrl = Content as Control;

            if (contentCtrl != null)
            {
                contentCtrl.HandleMouseDownEvent(uiEvent);
            }
        }

        public virtual void HandleMouseUpEvent(UIEvent uiEvent)
        {
            if (uiEvent.Handled)
            {
                return;
            }

            Control contentCtrl = Content as Control;

            if (contentCtrl != null)
            {
                contentCtrl.HandleMouseUpEvent(uiEvent);
            }
        }

        public virtual void HandleClickEvent(UIEvent uiEvent)
        {
            if (uiEvent.Handled)
            {
                return;
            }

            Control contentCtrl = Content as Control;

            if (contentCtrl != null)
            {
                contentCtrl.HandleClickEvent(uiEvent);
            }
        }


    }
}
