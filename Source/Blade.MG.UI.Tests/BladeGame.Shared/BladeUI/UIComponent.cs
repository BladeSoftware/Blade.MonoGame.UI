using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BladeGame.BladeUI;
using BladeGame.BladeUI.Components;
using BladeGame.Shared.BladeUI.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BladeGame.BladeUI
{
    public abstract class UIComponent
    {
        private UIComponent parent;
        public virtual UIComponent Parent { get => parent; set { parent = value; InitTemplate(); HandleStateChange(); } }

        public static int LastTabOrder = 0;

        public string Name { get; set; }

        public float Width { get; set; }
        public float Height { get; set; }
        public float MinWidth { get; set; }
        public float MinHeight { get; set; }
        public float MaxWidth { get; set; }
        public float MaxHeight { get; set; }

        public Binding<Thickness> Margin { get; set; } = new Binding<Thickness>();
        public Binding<Thickness> Padding { get; set; } = new Binding<Thickness>();

        public Binding<HorizontalAlignmentType> HorizontalAlignment { get; set; }
        public Binding<VerticalAlignmentType> VerticalAlignment { get; set; }
        public Binding<HorizontalAlignmentType> HorizontalContentAlignment { get; set; }
        public Binding<VerticalAlignmentType> VerticalContentAlignment { get; set; }


        public int TabIndex { get; set; } = ++LastTabOrder;

        public Binding<bool> IsTabStop { get; set; } = false;
        public Binding<bool> Enabled { get; set; } = true;
        public Binding<Visibility> Visible { get; set; } = new Binding<Visibility>(Visibility.Visible);

        public bool HasFocus { get; set; }
        public bool HitTestVisible { get; set; } = false;


        //----
        protected internal Size DesiredSize { get; set; }
        protected internal Rectangle finalRect;   // Final Layout Rectangle for Control, including Margin
        protected internal Rectangle finalContentRect; // Final Layout Rectangle for Control's Content, including Padding

        internal float ActualWidth { get; set; }
        internal float ActualHeight { get; set; }
        internal float Left { get; set; }
        internal float Top { get; set; }


        public UIComponent()
        {
            Width = float.NaN;
            Height = float.NaN;
            MinWidth = float.NaN;
            MaxWidth = float.MaxValue;
            MinHeight = float.NaN;
            MaxHeight = float.MaxValue;

            // Default Alignment is Top-Left
            HorizontalAlignment = HorizontalAlignmentType.Stretch;
            VerticalAlignment = VerticalAlignmentType.Stretch;

            HorizontalContentAlignment = HorizontalAlignmentType.Stretch;
            VerticalContentAlignment = VerticalAlignmentType.Stretch;

            ActualWidth = 0;
            ActualHeight = 0;
            //Left = 0;
            //Top = 0;

        }

        protected virtual void InitTemplate()
        {
        }

        protected virtual void HandleStateChange()
        {
        }

        public virtual void Measure(UIContext context, Size availableSize, ref Layout parentMinMax)
        {
            parentMinMax.Merge(MinWidth, MinHeight, MaxWidth, MaxHeight);
        }

        public virtual void Arrange(Rectangle layoutBounds)
        {
            layoutBounds = new Rectangle(layoutBounds.Left + Margin.Value.Left, layoutBounds.Top + Margin.Value.Top, layoutBounds.Width - Margin.Value.Left - Margin.Value.Right, layoutBounds.Height - Margin.Value.Top - Margin.Value.Bottom);

            Left = layoutBounds.Left;
            Top = layoutBounds.Top;
            ActualWidth = layoutBounds.Width;
            ActualHeight = layoutBounds.Height;



            if (float.IsNaN(ActualWidth))
            {
                ActualWidth = DesiredSize.Width;
            }

            if (float.IsNaN(ActualHeight))
            {
                ActualHeight = DesiredSize.Height;
            }

            if (!float.IsNaN(DesiredSize.Width) && DesiredSize.Width < ActualWidth)
            {
                ActualWidth = DesiredSize.Width;
            }

            if (!float.IsNaN(DesiredSize.Height) && DesiredSize.Height < ActualHeight)
            {
                ActualHeight = DesiredSize.Height;
            }

            if (HorizontalAlignment.Value == HorizontalAlignmentType.Stretch && float.IsNaN(Width))
            {
                Left = layoutBounds.Left;
                ActualWidth = layoutBounds.Width;
            }

            if (VerticalAlignment.Value == VerticalAlignmentType.Stretch && float.IsNaN(Height))
            {
                Top = layoutBounds.Top;
                ActualHeight = layoutBounds.Height;
            }

            if (ActualWidth < layoutBounds.Width)
            {
                switch (HorizontalAlignment.Value)
                {
                    case HorizontalAlignmentType.Left: Left = layoutBounds.Left; break;
                    case HorizontalAlignmentType.Right: Left = layoutBounds.Left + layoutBounds.Width - ActualWidth; break;
                    case HorizontalAlignmentType.Center: Left = layoutBounds.Left + (layoutBounds.Width - ActualWidth) / 2; break;
                        //case HorizontalAlignmentType.Stretch: Left = finalRect.Left; ActualWidth = finalRect.Width; break;
                }
            }

            if (ActualHeight < layoutBounds.Height)
            {
                switch (VerticalAlignment.Value)
                {
                    case VerticalAlignmentType.Top: Top = layoutBounds.Top; break;
                    case VerticalAlignmentType.Bottom: Top = layoutBounds.Top + layoutBounds.Height - ActualHeight; break;
                    case VerticalAlignmentType.Center: Top = layoutBounds.Top + (layoutBounds.Height - ActualHeight) / 2; break;
                        //case VerticalAlignmentType.Stretch: Top = finalRect.Top; ActualHeight = finalRect.Height; break;
                }

            }


            finalRect = new Rectangle((int)Left, (int)Top, (int)ActualWidth, (int)ActualHeight);
        }

        public virtual Rectangle GetChildBoundingBox(UIComponent child)
        {
            //float x = 0;
            //float w = 0;
            //float y = 0;
            //float h = 0;

            //x = child.Left;
            //y = child.Top;

            //w = child.ActualWidth;
            //h = child.ActualHeight;

            //return new Rectangle((int)x, (int)y, (int)w, (int)h);
            return finalRect;
        }


        public virtual void RenderControl(UIContext context, Rectangle layoutBounds)
        {
        }

        internal void ClampDesiredSize(Size availableSize, Layout parentMinMax)
        {
            float dw = DesiredSize.Width;
            float dh = DesiredSize.Height;

            float minW = parentMinMax.MinWidth;
            float minH = parentMinMax.MinHeight;
            float maxW = parentMinMax.MaxWidth;
            float maxH = parentMinMax.MaxHeight;

            if (!float.IsNaN(Width))
            {
                dw = Width;
            }
            if (!float.IsNaN(maxW) && (dw > maxW))
            {
                dw = maxW;
            }
            if (!float.IsNaN(minW) && (dw < minW || float.IsNaN(dw)))
            {
                dw = minW;
            }

            if (!float.IsNaN(Height))
            {
                dh = Height;
            }
            if (!float.IsNaN(maxH) && (dh > maxH))
            {
                dh = maxH;
            }
            if (!float.IsNaN(minH) && (dh < minH || float.IsNaN(dh)))
            {
                dh = minH;
            }

            //DesiredSize = new Size(dw, dh);

            //dw = DesiredSize.Width;
            //dh = DesiredSize.Height;

            if (!float.IsNaN(dw))
            {
                if (dw > availableSize.Width)
                {
                    dw = availableSize.Width;
                }
                if (dw < 0f)
                {
                    dw = 0f;
                }
            }

            if (!float.IsNaN(dh))
            {
                if (dh > availableSize.Height)
                {
                    dh = availableSize.Height;
                }
                if (dh < 0f)
                {
                    dh = 0f;
                }
            }

            DesiredSize = new Size(dw, dh);
        }

    }
}
