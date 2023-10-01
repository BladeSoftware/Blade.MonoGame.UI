using Blade.MG.UI.Components;
using Blade.MG.UI.Events;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System.Reflection;
using System.Runtime.Serialization;

namespace Blade.MG.UI
{
    // Implement DataContractResolver: https://docs.microsoft.com/en-us/dotnet/framework/wcf/samples/datacontractserializer-datacontractresolver-netdatacontractserializer
    // Implement custom serialiser for Colours / binding etc ? https://weblogs.asp.net/pwelter34/444961   :  https://stackoverflow.com/questions/29624215/using-c-sharp-xml-serializer-to-produce-custom-xml-format

    [KnownType("GetKnownTypes")]
    public abstract class UIComponent
    {
        private static IEnumerable<Type> knownTypes = null;

        private UIWindow parentWindow;
        public UIWindow ParentWindow { get { return parentWindow; } private set { SetParentWindow(value); } }
        protected ContentManager ContentManager => ParentWindow?.ContentManager;

        protected UIContext Context => ParentWindow?.Context;

        protected ICollection<UIComponent> privateControls = new List<UIComponent>();

        private object dataContext;
        public object DataContext { get { return dataContext ?? parent?.DataContext; } set { dataContext = value; } }
        public int FrameID;

        private UIComponent parent;
        public virtual UIComponent Parent
        {
            get => parent;
            set
            {
                parent = value;

                if (value is UIWindow) { ParentWindow = (UIWindow)value; }
                else if (value?.ParentWindow != null) { ParentWindow = value?.ParentWindow; }

                if (!TemplateInitialised)
                {
                    InitTemplate();
                }

                StateHasChanged();
            }
        }

        public T ParentAs<T>() where T : UIComponent => Parent as T;

        public static int LastTabOrder = 0;

        public bool TemplateInitialised { get; set; } = false;

        public string Name { get; set; }

        public Length Width { get; set; }
        public Length Height { get; set; }
        public Length MinWidth { get; set; }
        public Length MinHeight { get; set; }
        public Length MaxWidth { get; set; }
        public Length MaxHeight { get; set; }

        public Binding<Thickness> Margin { get; set; } = new Thickness();
        public Binding<Thickness> Padding { get; set; } = new Thickness();

        public Binding<HorizontalAlignmentType> HorizontalAlignment { get; set; }
        public Binding<VerticalAlignmentType> VerticalAlignment { get; set; }
        public Binding<HorizontalAlignmentType> HorizontalContentAlignment { get; set; }
        public Binding<VerticalAlignmentType> VerticalContentAlignment { get; set; }

        public StretchType Stretch { get; set; }
        public StretchDirection StretchDirection { get; set; }

        public int TabIndex { get; set; } = ++LastTabOrder;

        public Binding<bool> IsTabStop { get; set; } = false;
        public Binding<bool> Enabled { get; set; } = true;
        public Binding<Visibility> Visible { get; set; } = Visibility.Visible;

        public bool HitTestVisible { get; set; } = false;

        public Binding<bool> HasFocus { get; set; } = false;
        public Binding<bool> MouseHover = false;


        public Transform Transform { get; set; } = new Transform();


        //----

        protected internal Size DesiredSize { get; set; }
        protected internal Rectangle FinalRect;        // Final Layout Rectangle for Control, including Margin
        protected internal Rectangle FinalContentRect; // Final Layout Rectangle for Control's Content, including Padding
        //protected internal Rectangle visibleRect;     // Final Layout Rectangle for Control clipped to parent control's layout bounds

        internal float ActualWidth { get; set; }
        internal float ActualHeight { get; set; }
        internal float Left { get; set; }
        internal float Top { get; set; }


        public UIComponent()
        {
            Width = float.NaN;
            Height = float.NaN;
            MinWidth = float.NaN;
            MaxWidth = float.NaN;
            MinHeight = float.NaN;
            MaxHeight = float.NaN;

            HorizontalAlignment = HorizontalAlignmentType.Stretch;
            VerticalAlignment = VerticalAlignmentType.Stretch;

            HorizontalContentAlignment = HorizontalAlignmentType.Stretch;
            VerticalContentAlignment = VerticalAlignmentType.Stretch;

            ActualWidth = 0;
            ActualHeight = 0;
        }

        protected virtual void InitTemplate()
        {
            TemplateInitialised = true;
        }

        public void ReInitTemplate(object dataContext)
        {
            if (parent == null)
            {
                throw new Exception("Parent must be populate before calling ReInitTemplate");
            }

            //TemplateInitialised = false;
            DataContext = dataContext;
            InitTemplate();
        }

        public void StateHasChanged()
        {
            HandleStateChange();
        }

        protected virtual void HandleStateChange()
        {

        }

        //public void StateHasChanged()
        //{
        //    JoinableTaskFactory jtf = new JoinableTaskFactory(new JoinableTaskContext());
        //    jtf.Run(() => HandleStateChangeAsync());
        //}

        //public async Task StateHasChangedAsync()
        //{
        //    await HandleStateChangeAsync();
        //}

        //protected virtual async Task HandleStateChangeAsync()
        //{
        //    await Task.CompletedTask;
        //}

        public virtual void Measure(UIContext context, ref Size availableSize, ref Layout parentMinMax)
        {
            MeasureInternal(context, ref availableSize, ref parentMinMax);
        }

        protected void MeasureInternal(UIContext context, ref Size availableSize, ref Layout parentMinMax)
        {
            parentMinMax.Merge(MinWidth, MinHeight, MaxWidth, MaxHeight, availableSize);

            float desiredWidth = float.NaN;
            float desiredHeight = float.NaN;

            if (!FloatHelper.IsNaN(Width))
            {
                desiredWidth = Width.ToPixels(availableSize.Width);
            }

            if (!FloatHelper.IsNaN(Height))
            {
                desiredHeight = Height.ToPixels(availableSize.Height);
            }


            desiredWidth += Margin.Value.Left + Margin.Value.Right;
            desiredHeight += Margin.Value.Top + Margin.Value.Bottom;

            desiredWidth += Padding.Value.Left + Padding.Value.Right;
            desiredHeight += Padding.Value.Top + Padding.Value.Bottom;


            if (Visible.Value == Visibility.Collapsed)
            {
                DesiredSize = new Size(0, 0);
            }
            else
            {
                DesiredSize = new Size(desiredWidth, desiredHeight);

                ClampDesiredSize(availableSize, parentMinMax);
            }
        }


        public virtual void Arrange(UIContext context, Rectangle layoutBounds, Rectangle parentLayoutBounds)
        {
            ArrangeSelf(context, layoutBounds, parentLayoutBounds);
        }

        public void ArrangeSelf(UIContext context, Rectangle layoutBounds, Rectangle parentLayoutBounds)
        {
            if (Visible.Value == Visibility.Collapsed)
            {
                ActualWidth = 0;
                ActualHeight = 0;

                FinalRect = new Rectangle(layoutBounds.Left, layoutBounds.Top, 0, 0);
                FinalContentRect = new Rectangle(layoutBounds.Left, layoutBounds.Top, 0, 0);

                return;
            }

            layoutBounds = new Rectangle(layoutBounds.Left + Margin.Value.Left, layoutBounds.Top + Margin.Value.Top, layoutBounds.Width - Margin.Value.Horizontal, layoutBounds.Height - Margin.Value.Vertical);

            if (HorizontalAlignment.Value != HorizontalAlignmentType.Absolute)
            {
                Left = layoutBounds.Left;
            }

            if (VerticalAlignment.Value != VerticalAlignmentType.Absolute)
            {
                Top = layoutBounds.Top;
            }

            //ActualWidth = layoutBounds.Width;
            //ActualHeight = layoutBounds.Height;


            float desiredWidth = DesiredSize.Width - Margin.Value.Horizontal;
            float desiredHeight = DesiredSize.Height - Margin.Value.Vertical;

            ActualWidth = desiredWidth;
            ActualHeight = desiredHeight;

            if (float.IsNaN(ActualWidth))
            {
                //ActualWidth = desiredWidth;
                ActualWidth = 0;
            }

            if (float.IsNaN(ActualHeight))
            {
                //ActualHeight = desiredHeight;
                ActualHeight = 0;
            }

            if (!FloatHelper.IsNaN(Width))
            {
                //ActualWidth = Width.ToPixels(parentLayoutBounds.Width);
                ActualWidth = Width.ToPixelsWidth(this, parentLayoutBounds);
            }
            else if (!float.IsNaN(desiredWidth) && desiredWidth < ActualWidth)
            {
                if (HorizontalAlignment.Value != HorizontalAlignmentType.Stretch)
                {
                    ActualWidth = desiredWidth;
                }
            }

            if (!FloatHelper.IsNaN(MaxWidth) && ActualWidth > MaxWidth.ToPixels(parentLayoutBounds.Width))
            {
                ActualWidth = MaxWidth.ToPixels(parentLayoutBounds.Width);
            }

            if (!FloatHelper.IsNaN(MinWidth) && ActualWidth < MinWidth.ToPixels(parentLayoutBounds.Width))
            {
                ActualWidth = MinWidth.ToPixels(parentLayoutBounds.Width);
            }


            if (!FloatHelper.IsNaN(Height))
            {
                ActualHeight = Height.ToPixelsHeight(this, parentLayoutBounds);
            }
            else if (!float.IsNaN(desiredHeight) && desiredHeight < ActualHeight)
            {
                if (VerticalAlignment.Value != VerticalAlignmentType.Stretch)
                {
                    ActualHeight = desiredHeight;
                }
            }

            if (!FloatHelper.IsNaN(MaxHeight) && ActualHeight > MaxHeight.ToPixels(parentLayoutBounds.Height))
            {
                ActualHeight = MaxHeight.ToPixels(parentLayoutBounds.Height);
            }

            if (!FloatHelper.IsNaN(MinHeight) && ActualHeight < MinHeight.ToPixels(parentLayoutBounds.Height))
            {
                ActualHeight = MinHeight.ToPixels(parentLayoutBounds.Height);
            }


            if (HorizontalAlignment.Value == HorizontalAlignmentType.Stretch && FloatHelper.IsNaN(Width))
            {
                Left = layoutBounds.Left;
                ActualWidth = layoutBounds.Width;

                if (ActualWidth < MinWidth.ToPixels(parentLayoutBounds.Width)) ActualWidth = MinWidth.ToPixels(parentLayoutBounds.Width);
                if (ActualWidth > MaxWidth.ToPixels(parentLayoutBounds.Width)) ActualWidth = MaxWidth.ToPixels(parentLayoutBounds.Width);
            }

            if (VerticalAlignment.Value == VerticalAlignmentType.Stretch && FloatHelper.IsNaN(Height))
            {
                Top = layoutBounds.Top;
                ActualHeight = layoutBounds.Height;

                if (ActualHeight < MinHeight.ToPixels(parentLayoutBounds.Height)) ActualHeight = MinHeight.ToPixels(parentLayoutBounds.Height);
                if (ActualHeight > MaxHeight.ToPixels(parentLayoutBounds.Height)) ActualHeight = MaxHeight.ToPixels(parentLayoutBounds.Height);
            }

            ////--------------

            //switch (Stretch)
            //{
            //    case StretchType.None:
            //        break;

            //    case StretchType.Fill:
            //        ActualWidth = layoutBounds.Width - Padding.Value.Horizontal;
            //        ActualHeight = layoutBounds.Height - Padding.Value.Vertical;

            //        break;

            //    case StretchType.Uniform:
            //        int minscale = Math.Min(layoutBounds.Width - Padding.Value.Horizontal, layoutBounds.Height - Padding.Value.Vertical);

            //        ActualWidth = minscale;
            //        ActualHeight = minscale;

            //        break;

            //    case StretchType.UniformToFill:
            //        int maxscale = Math.Max(layoutBounds.Width - Padding.Value.Horizontal, layoutBounds.Height - Padding.Value.Vertical);

            //        ActualWidth = maxscale;
            //        ActualHeight = maxscale;

            //        break;

            //}


            ////--------------

            if (ActualWidth != layoutBounds.Width)
            {
                switch (HorizontalAlignment.Value)
                {
                    case HorizontalAlignmentType.Left: Left = layoutBounds.Left; break;
                    case HorizontalAlignmentType.Right: Left = layoutBounds.Left + layoutBounds.Width - ActualWidth; break;
                    case HorizontalAlignmentType.Center: Left = layoutBounds.Left + (layoutBounds.Width - ActualWidth) / 2; break;
                    case HorizontalAlignmentType.Stretch: Left = layoutBounds.Left + (layoutBounds.Width - ActualWidth) / 2; break;  // Handle Stretch like Center if we have a Width Constraint
                        //case HorizontalAlignmentType.Stretch: Left = finalRect.Left; ActualWidth = finalRect.Width; break;
                }
            }

            if (ActualHeight != layoutBounds.Height)
            {
                switch (VerticalAlignment.Value)
                {
                    case VerticalAlignmentType.Top: Top = layoutBounds.Top; break;
                    case VerticalAlignmentType.Bottom: Top = layoutBounds.Top + layoutBounds.Height - ActualHeight; break;
                    case VerticalAlignmentType.Center: Top = layoutBounds.Top + (layoutBounds.Height - ActualHeight) / 2; break;
                    case VerticalAlignmentType.Stretch: Top = layoutBounds.Top + (layoutBounds.Height - ActualHeight) / 2; break; // Handle Stretch like Center if we have a Height Constraint
                        //case VerticalAlignmentType.Stretch: Top = finalRect.Top; ActualHeight = finalRect.Height; break;
                }

            }

            FinalRect = new Rectangle((int)Left, (int)Top, (int)ActualWidth, (int)ActualHeight);

            // Add padding to get content area
            int left = FinalRect.Left + Padding.Value.Left;
            int top = FinalRect.Top + Padding.Value.Top;
            int right = FinalRect.Right - Padding.Value.Right;
            int bottom = FinalRect.Bottom - Padding.Value.Bottom;

            if (left < FinalRect.Left) left = FinalRect.Left;
            if (left > FinalRect.Right) left = FinalRect.Right;
            if (right < FinalRect.Left) right = FinalRect.Left;
            if (right > FinalRect.Right) right = FinalRect.Right;
            if (top < FinalRect.Top) top = FinalRect.Top;
            if (top > FinalRect.Bottom) top = FinalRect.Bottom;
            if (bottom < FinalRect.Top) bottom = FinalRect.Top;
            if (bottom > FinalRect.Bottom) bottom = FinalRect.Bottom;

            int width = right - left;// + 1;
            int height = bottom - top;// + 1;

            if (width < 0) width = 0;
            if (height < 0) height = 0;

            FinalContentRect = new Rectangle(left, top, width, height);

            //clippingRect = Rectangle.Intersect(parent.finalRect, finalRect);
        }

        //public void ArrangeInternal(UIContext context, Rectangle layoutBounds, Rectangle? parentLayoutBounds)
        //{
        //    layoutBounds = new Rectangle(layoutBounds.Left + Margin.Value.Left, layoutBounds.Top + Margin.Value.Top, layoutBounds.Width - Margin.Value.Left - Margin.Value.Right, layoutBounds.Height - Margin.Value.Top - Margin.Value.Bottom);

        //    Left = layoutBounds.Left;
        //    Top = layoutBounds.Top;
        //    ActualWidth = layoutBounds.Width;
        //    ActualHeight = layoutBounds.Height;


        //    float desiredWidth = DesiredSize.Width - Margin.Value.Left - Margin.Value.Right;
        //    float desiredHeight = DesiredSize.Height - Margin.Value.Top - Margin.Value.Bottom;

        //    if (float.IsNaN(ActualWidth))
        //    {
        //        ActualWidth = desiredWidth;
        //    }

        //    if (float.IsNaN(ActualHeight))
        //    {
        //        ActualHeight = desiredHeight;
        //    }

        //    if (!FloatHelper.IsNaN(Width))
        //    {
        //        ActualWidth = Width.ToPixels(layoutBounds.Width);
        //    }
        //    else if (!float.IsNaN(desiredWidth) && desiredWidth < ActualWidth)
        //    {
        //        if (HorizontalAlignment.Value != HorizontalAlignmentType.Stretch)
        //        {
        //            ActualWidth = desiredWidth;
        //        }
        //    }

        //    if (!FloatHelper.IsNaN(MaxWidth) && ActualWidth > MaxWidth.ToPixels(layoutBounds.Width))
        //    {
        //        ActualWidth = MaxWidth.ToPixels(layoutBounds.Width);
        //    }

        //    if (!FloatHelper.IsNaN(MinWidth) && ActualWidth < MinWidth.ToPixels(layoutBounds.Width))
        //    {
        //        ActualWidth = MinWidth.ToPixels(layoutBounds.Width);
        //    }


        //    if (!FloatHelper.IsNaN(Height))
        //    {
        //        ActualHeight = Height.ToPixels(layoutBounds.Height);
        //    }
        //    else if (!float.IsNaN(desiredHeight) && desiredHeight < ActualHeight)
        //    {
        //        if (VerticalAlignment.Value != VerticalAlignmentType.Stretch)
        //        {
        //            ActualHeight = desiredHeight;
        //        }
        //    }

        //    if (!FloatHelper.IsNaN(MaxHeight) && ActualHeight > MaxHeight.ToPixels(layoutBounds.Height))
        //    {
        //        ActualHeight = MaxHeight.ToPixels(layoutBounds.Height);
        //    }

        //    if (!FloatHelper.IsNaN(MinHeight) && ActualHeight < MinHeight.ToPixels(layoutBounds.Height))
        //    {
        //        ActualHeight = MinHeight.ToPixels(layoutBounds.Height);
        //    }


        //    if (HorizontalAlignment.Value == HorizontalAlignmentType.Stretch && FloatHelper.IsNaN(Width))
        //    {
        //        Left = layoutBounds.Left;
        //        ActualWidth = layoutBounds.Width;

        //        if (ActualWidth < MinWidth.ToPixels(layoutBounds.Width)) ActualWidth = MinWidth.ToPixels(layoutBounds.Width);
        //        if (ActualWidth > MaxWidth.ToPixels(layoutBounds.Width)) ActualWidth = MaxWidth.ToPixels(layoutBounds.Width);
        //    }

        //    if (VerticalAlignment.Value == VerticalAlignmentType.Stretch && FloatHelper.IsNaN(Height))
        //    {
        //        Top = layoutBounds.Top;
        //        ActualHeight = layoutBounds.Height;

        //        if (ActualHeight < MinHeight.ToPixels(layoutBounds.Height)) ActualHeight = MinHeight.ToPixels(layoutBounds.Height);
        //        if (ActualHeight > MaxHeight.ToPixels(layoutBounds.Height)) ActualHeight = MaxHeight.ToPixels(layoutBounds.Height);
        //    }

        //    if (ActualWidth != layoutBounds.Width)
        //    {
        //        switch (HorizontalAlignment.Value)
        //        {
        //            case HorizontalAlignmentType.Left: Left = layoutBounds.Left; break;
        //            case HorizontalAlignmentType.Right: Left = layoutBounds.Left + layoutBounds.Width - ActualWidth; break;
        //            case HorizontalAlignmentType.Center: Left = layoutBounds.Left + (layoutBounds.Width - ActualWidth) / 2; break;
        //            case HorizontalAlignmentType.Stretch: Left = layoutBounds.Left + (layoutBounds.Width - ActualWidth) / 2; break;  // Handle Stretch like Center if we have a Width Constraint
        //                //case HorizontalAlignmentType.Stretch: Left = finalRect.Left; ActualWidth = finalRect.Width; break;
        //        }
        //    }

        //    if (ActualHeight != layoutBounds.Height)
        //    {
        //        switch (VerticalAlignment.Value)
        //        {
        //            case VerticalAlignmentType.Top: Top = layoutBounds.Top; break;
        //            case VerticalAlignmentType.Bottom: Top = layoutBounds.Top + layoutBounds.Height - ActualHeight; break;
        //            case VerticalAlignmentType.Center: Top = layoutBounds.Top + (layoutBounds.Height - ActualHeight) / 2; break;
        //            case VerticalAlignmentType.Stretch: Top = layoutBounds.Top + (layoutBounds.Height - ActualHeight) / 2; break; // Handle Stretch like Center if we have a Height Constraint
        //                //case VerticalAlignmentType.Stretch: Top = finalRect.Top; ActualHeight = finalRect.Height; break;
        //        }

        //    }

        //    finalRect = new Rectangle((int)Left, (int)Top, (int)ActualWidth, (int)ActualHeight);

        //    // Add padding to get content area
        //    int left = finalRect.Left + Padding.Value.Left;
        //    int top = finalRect.Top + Padding.Value.Top;
        //    int right = finalRect.Right - Padding.Value.Right;
        //    int bottom = finalRect.Bottom - Padding.Value.Bottom;

        //    if (left < finalRect.Left) left = finalRect.Left;
        //    if (left > finalRect.Right) left = finalRect.Right;
        //    if (right < finalRect.Left) right = finalRect.Left;
        //    if (right > finalRect.Right) right = finalRect.Right;
        //    if (top < finalRect.Top) top = finalRect.Top;
        //    if (top > finalRect.Bottom) top = finalRect.Bottom;
        //    if (bottom < finalRect.Top) bottom = finalRect.Top;
        //    if (bottom > finalRect.Bottom) bottom = finalRect.Bottom;

        //    int width = right - left;// + 1;
        //    int height = bottom - top;// + 1;

        //    if (width < 0) width = 0;
        //    if (height < 0) height = 0;

        //    finalContentRect = new Rectangle(left, top, width, height);

        //    //clippingRect = Rectangle.Intersect(parent.finalRect, finalRect);
        //}

        public virtual Rectangle GetChildBoundingBox(UIContext context, UIComponent child)
        {
            return FinalContentRect;
        }

        public virtual void RenderControl(UIContext context, Rectangle layoutBounds, Transform parentTransform)
        {

        }

        protected void MergeChildDesiredSize(UIContext context, ref Size availableSize, IEnumerable<UIComponent> children, ref Layout parentMinMax)
        {
            float desiredWidth = float.NaN;
            float desiredHeight = float.NaN;

            foreach (var child in children)
            {
                MergeChildDesiredSizeInternal(context, ref availableSize, child, ref desiredWidth, ref desiredHeight, ref parentMinMax);
            }

            if (!FloatHelper.IsNaN(Width))
            {
                desiredWidth = Width.ToPixels(availableSize.Width);
            }

            if (!FloatHelper.IsNaN(Height))
            {
                desiredHeight = Height.ToPixels(availableSize.Height);
            }


            desiredWidth += Margin.Value.Left + Margin.Value.Right;
            desiredHeight += Margin.Value.Top + Margin.Value.Bottom;

            desiredWidth += Padding.Value.Left + Padding.Value.Right;
            desiredHeight += Padding.Value.Top + Padding.Value.Bottom;

            DesiredSize = new Size(desiredWidth, desiredHeight);

            ClampDesiredSize(availableSize, parentMinMax);
        }

        protected void MergeChildDesiredSize(UIContext context, ref Size availableSize, UIComponent child, ref Layout parentMinMax)
        {
            float desiredWidth = float.NaN;
            float desiredHeight = float.NaN;

            if (Visible.Value == Visibility.Collapsed)
            {
                DesiredSize = new Size(0, 0);
                return;
            }

            MergeChildDesiredSizeInternal(context, ref availableSize, child, ref desiredWidth, ref desiredHeight, ref parentMinMax);

            if (!FloatHelper.IsNaN(Width))
            {
                desiredWidth = Width.ToPixels(availableSize.Width);
            }

            if (!FloatHelper.IsNaN(Height))
            {
                desiredHeight = Height.ToPixels(availableSize.Height);
            }


            desiredWidth += Margin.Value.Left + Margin.Value.Right;
            desiredHeight += Margin.Value.Top + Margin.Value.Bottom;

            desiredWidth += Padding.Value.Left + Padding.Value.Right;
            desiredHeight += Padding.Value.Top + Padding.Value.Bottom;

            DesiredSize = new Size(desiredWidth, desiredHeight);

            ClampDesiredSize(availableSize, parentMinMax);
        }

        protected void AddChildDesiredSizeHorizontal(UIContext context, ref Size availableSize, ref Layout parentMinMax, UIComponent child)
        {
            float desiredWidth = DesiredSize.Width;
            float desiredHeight = DesiredSize.Height;

            child.Measure(context, ref availableSize, ref parentMinMax);


            if (!float.IsNaN(child.DesiredSize.Width))
            {
                if (float.IsNaN(desiredWidth))
                {
                    desiredWidth = child.DesiredSize.Width;
                }
                else
                {
                    desiredWidth += child.DesiredSize.Width;
                }
            }


            //MergeChildDesiredSizeInternal(context, availableSize, child, ref desiredWidth, ref desiredHeight, ref parentMinMax);

            //if (!float.IsNaN(DesiredSize.Width))
            //{
            //    desiredWidth += DesiredSize.Width;
            //}

            //if (!float.IsNaN(DesiredSize.Height))
            //{
            //    desiredHeight = DesiredSize.Height;
            //}

            if (!FloatHelper.IsNaN(Width))
            {
                desiredWidth = Width.ToPixels(availableSize.Width);
            }

            if (!FloatHelper.IsNaN(Height))
            {
                desiredHeight = Height.ToPixels(availableSize.Height);
            }


            DesiredSize = new Size(desiredWidth, desiredHeight);

            child.ClampDesiredSize(availableSize, parentMinMax);
        }

        protected void AddChildDesiredSizeVertical(UIContext context, ref Size availableSize, ref Layout parentMinMax, UIComponent child)
        {
            float desiredWidth = DesiredSize.Width;
            float desiredHeight = DesiredSize.Height;

            child.Measure(context, ref availableSize, ref parentMinMax);


            if (!float.IsNaN(child.DesiredSize.Height))
            {
                if (float.IsNaN(desiredHeight))
                {
                    desiredHeight = child.DesiredSize.Height;
                }
                else
                {
                    desiredHeight += child.DesiredSize.Height;
                }
            }




            //MergeChildDesiredSizeInternal(context, availableSize, child, ref desiredWidth, ref desiredHeight, ref parentMinMax);

            //if (!float.IsNaN(DesiredSize.Width))
            //{
            //    desiredWidth = DesiredSize.Width;
            //}

            //if (!float.IsNaN(DesiredSize.Height))
            //{
            //    desiredHeight += DesiredSize.Height;
            //}

            if (!FloatHelper.IsNaN(Width))
            {
                desiredWidth = Width.ToPixels(availableSize.Width);
            }

            if (!FloatHelper.IsNaN(Height))
            {
                desiredHeight = Height.ToPixels(availableSize.Height);
            }


            DesiredSize = new Size(desiredWidth, desiredHeight);

            child.ClampDesiredSize(availableSize, parentMinMax);
        }


        private void MergeChildDesiredSizeInternal(UIContext context, ref Size availableSize, UIComponent child, ref float desiredWidth, ref float desiredHeight, ref Layout parentMinMax)
        {
            if (child != null)
            {
                if (child.Visible.Value == Visibility.Collapsed)
                {
                    desiredWidth = 0;
                    desiredHeight = 0;
                    return;
                }

                child.Measure(context, ref availableSize, ref parentMinMax);

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

            }

        }

        protected void ClampDesiredSize(Size availableSize, Layout parentMinMax)
        {
            float dw = DesiredSize.Width;
            float dh = DesiredSize.Height;

            float minW = parentMinMax.MinWidth;
            float minH = parentMinMax.MinHeight;
            float maxW = parentMinMax.MaxWidth;
            float maxH = parentMinMax.MaxHeight;

            //if (!float.IsNaN(Width))
            //{
            //    dw = Width;
            //}
            if (!FloatHelper.IsNaN(MaxWidth) && (!float.IsNaN(dw) || dw > MaxWidth.ToPixels(availableSize.Width)))
            {
                dw = MaxWidth.ToPixels(availableSize.Width);
            }
            if (!float.IsNaN(maxW) && dw > maxW)
            {
                dw = maxW;
            }

            if (!FloatHelper.IsNaN(MinWidth) && (!float.IsNaN(dw) || dw < MinWidth.ToPixels(availableSize.Width)))
            {
                dw = MinWidth.ToPixels(availableSize.Width);
            }
            if (!float.IsNaN(minW) && (dw < minW || float.IsNaN(dw)))
            {
                dw = minW;
            }

            if (!FloatHelper.IsNaN(Width))
            {
                dw = Width.ToPixels(availableSize.Width) + Margin.Value.Left + Margin.Value.Right + Padding.Value.Left + Padding.Value.Right;
            }


            //if (!float.IsNaN(Height))
            //{
            //    dh = Height;
            //}
            if (!FloatHelper.IsNaN(MaxHeight) && (dh > MaxHeight.ToPixels(availableSize.Height) || float.IsNaN(dh)))
            {
                dh = MaxHeight.ToPixels(availableSize.Height);
            }
            if (!float.IsNaN(maxH) && dh > maxH)
            {
                dh = maxH;
            }

            if (!FloatHelper.IsNaN(MinHeight) && (dh < MinHeight.ToPixels(availableSize.Height) || float.IsNaN(dh)))
            {
                dh = MinHeight.ToPixels(availableSize.Height);
            }
            if (!float.IsNaN(minH) && (dh < minH || float.IsNaN(dh)))
            {
                dh = minH;
            }


            if (!FloatHelper.IsNaN(Height))
            {
                dh = Height.ToPixels(availableSize.Height) + Margin.Value.Top + Margin.Value.Bottom + Padding.Value.Top + Padding.Value.Bottom;
            }


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


        /// <summary>
        /// Only propagate events to the 'locked' control
        /// e.g. If we're dragging a Scrollbar handle we only want the scrollbar handle to receive events
        /// </summary>
        /// <param name="uiWindow"></param>
        /// <param name="component"></param>
        /// <returns></returns>
        protected bool LockEventsToControl(UIWindow uiWindow, UIComponent component)
        {
            if (uiWindow.EventLockedControl == null || uiWindow.EventLockedControl == component)
            {
                uiWindow.EventLockedControl = component;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Stop events from being locked to a single control
        /// </summary>
        /// <param name="uiWindow"></param>
        /// <param name="component"></param>
        /// <returns></returns>
        protected bool UnlockEventsFromControl(UIWindow uiWindow, UIComponent component)
        {
            if (uiWindow.EventLockedControl != null && uiWindow.EventLockedControl == component)
            {
                uiWindow.EventLockedControl = null;
                return true;
            }

            return false;
        }

        public IEnumerable<UIComponent> PrivateControls
        {
            get
            {
                foreach (var control in privateControls)
                {
                    yield return control;
                }
            }
        }

        private void SetParentWindow(UIWindow newParentWindow)
        {
            parentWindow = newParentWindow;

            switch (this)
            {
                case Control control:
                    control.Content?.SetParentWindow(newParentWindow);
                    break;

                case Container container:
                    foreach (var child in container.PrivateControls)
                    {
                        child?.SetParentWindow(newParentWindow);
                    }
                    foreach (var child in container.Children)
                    {
                        child?.SetParentWindow(newParentWindow);
                    }
                    break;
            };
        }

        public Rectangle GetFinalRect() => FinalRect;
        public Rectangle GetFinalContentRect() => FinalContentRect;


        /// <summary>
        /// Find the First Parent control of the requested type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T FindParent<T>() where T : class
        {
            UIComponent ctrl = Parent;

            while (ctrl != null)
            {
                if (ctrl.GetType().IsAssignableTo(typeof(T)))
                {
                    return ctrl as T;
                }

                ctrl = ctrl.Parent;
            }

            return null;
        }

        // ---=== UI Events ===---

        public virtual void Propagate(UIEvent uiEvent, UIWindow uiWindow, Action<UIComponent> action)
        {
            if (uiEvent.Handled) return;

            // If events are locked to a control, then only propgate to that control
            if (uiWindow.EventLockedControl != null)
            {
                if (this == uiWindow.EventLockedControl)
                {
                    return;
                }

                action(uiWindow.EventLockedControl);

                return;
            }


            foreach (UIComponent child in PrivateControls)
            {
                action(child);
                if (uiEvent.Handled) return;
            }


            UIComponent ctrlContent = (this as Control)?.Content;
            if (ctrlContent != null)
            {
                action(ctrlContent);
                if (uiEvent.Handled) return;
            }

            Container container = this as Container;
            if (container != null)
            {
                foreach (var child in container.Children)
                {
                    action(child);
                    if (uiEvent.Handled) return;
                }
            }

        }

        public virtual async Task PropagateAsync(UIEvent uiEvent, UIWindow uiWindow, Func<UIComponent, Task> action)
        {
            if (uiEvent.Handled) return;

            // If events are locked to a control, then only propgate to that control
            if (uiWindow.EventLockedControl != null)
            {
                if (this == uiWindow.EventLockedControl)
                {
                    return;
                }

                await action(uiWindow.EventLockedControl);

                return;
            }


            foreach (UIComponent child in PrivateControls)
            {
                if (child != null)
                {
                    await action(child);
                    if (uiEvent.Handled) return;
                }
            }


            UIComponent ctrlContent = (this as Control)?.Content;
            if (ctrlContent != null)
            {
                await action(ctrlContent);
                if (uiEvent.Handled) return;
            }

            Container container = this as Container;
            if (container != null)
            {
                foreach (var child in container.Children)
                {
                    if (child != null)
                    {
                        await action(child);
                        if (uiEvent.Handled) return;
                    }
                }
            }

        }


        public virtual async Task HandleMouseMoveEventAsync(UIWindow uiWindow, UIMouseMoveEvent uiEvent)
        {
            // Limit Mouse Events to the component layout window
            //if (this.finalRect.Contains(uiEvent.X, uiEvent.Y))
            //{

            // Mouse Move Events need to be propogated to all controls
            await PropagateAsync(uiEvent, uiWindow, async (component) => { await component?.HandleMouseMoveEventAsync(uiWindow, uiEvent); });
            //}
        }

        public virtual async Task HandleMouseDownEventAsync(UIWindow uiWindow, UIMouseDownEvent uiEvent)
        {
            // Limit Mouse Events to the component layout window
            if (FinalRect.Contains(uiEvent.X, uiEvent.Y))
            {
                await PropagateAsync(uiEvent, uiWindow, async (component) =>
                {
                    if (uiEvent.ForcePropogation || component.FinalRect.Contains(uiEvent.X, uiEvent.Y))
                    {
                        await component?.HandleMouseDownEventAsync(uiWindow, uiEvent);
                    }
                });

                //if (this.HitTestVisible) uiEvent.Handled = true;
            }
            //if (this.HitTestVisible && this.FinalRect.Contains(uiEvent.X, uiEvent.Y)) uiEvent.Handled = true;
        }

        public virtual async Task HandleMouseUpEventAsync(UIWindow uiWindow, UIMouseUpEvent uiEvent)
        {
            // Limit Mouse Events to the component layout window
            if (uiEvent.ForcePropogation || FinalRect.Contains(uiEvent.X, uiEvent.Y))
            {
                await PropagateAsync(uiEvent, uiWindow, async (component) =>
                {
                    if (uiEvent.ForcePropogation || component.FinalRect.Contains(uiEvent.X, uiEvent.Y))
                    {
                        await component?.HandleMouseUpEventAsync(uiWindow, uiEvent);
                    }
                });

                //if (this.HitTestVisible) uiEvent.Handled = true;
            }
            //if (this.HitTestVisible && this.FinalRect.Contains(uiEvent.X, uiEvent.Y)) uiEvent.Handled = true;
        }

        public virtual async Task HandleMouseWheelScrollEventAsync(UIWindow uiWindow, UIMouseWheelScrollEvent uiEvent)
        {
            // Limit Mouse Events to the component layout window
            if (FinalRect.Contains(uiEvent.X, uiEvent.Y))
            {
                await PropagateAsync(uiEvent, uiWindow, async (component) =>
                {
                    if (uiEvent.ForcePropogation || component.FinalRect.Contains(uiEvent.X, uiEvent.Y))
                    {
                        await component?.HandleMouseWheelScrollEventAsync(uiWindow, uiEvent);
                    }
                });

                if (HitTestVisible) uiEvent.Handled = true;
            }

            //if (this.HitTestVisible && this.finalRect.Contains(uiEvent.X, uiEvent.Y)) uiEvent.Handled = true;
        }

        public virtual async Task HandleClickEventAsync(UIWindow uiWindow, UIClickEvent uiEvent)
        {
            // Limit Mouse Events to the component layout window
            if (FinalRect.Contains(uiEvent.X, uiEvent.Y))
            {
                await PropagateAsync(uiEvent, uiWindow, async (component) =>
                {
                    if (uiEvent.ForcePropogation || component.FinalRect.Contains(uiEvent.X, uiEvent.Y))
                    {
                        await component?.HandleClickEventAsync(uiWindow, uiEvent);
                    }
                });

                //if (this.HitTestVisible) uiEvent.Handled = true;
            }
            //if (this.HitTestVisible && this.FinalRect.Contains(uiEvent.X, uiEvent.Y)) uiEvent.Handled = true;
        }

        public virtual async Task HandleDoubleClickEventAsync(UIWindow uiWindow, UIClickEvent uiEvent)
        {
            // Limit Mouse Events to the component layout window
            if (FinalRect.Contains(uiEvent.X, uiEvent.Y))
            {
                await PropagateAsync(uiEvent, uiWindow, async (component) =>
                {
                    if (uiEvent.ForcePropogation || component.FinalRect.Contains(uiEvent.X, uiEvent.Y))
                    {
                        await component?.HandleDoubleClickEventAsync(uiWindow, uiEvent);
                    }
                });

                //if (this.HitTestVisible) uiEvent.Handled = true;
            }
            //if (this.HitTestVisible && this.FinalRect.Contains(uiEvent.X, uiEvent.Y)) uiEvent.Handled = true;
        }

        public virtual async Task HandleRightClickEventAsync(UIWindow uiWindow, UIClickEvent uiEvent)
        {
            // Limit Mouse Events to the component layout window
            if (FinalRect.Contains(uiEvent.X, uiEvent.Y))
            {
                await PropagateAsync(uiEvent, uiWindow, async (component) =>
                {
                    if (uiEvent.ForcePropogation || component.FinalRect.Contains(uiEvent.X, uiEvent.Y))
                    {
                        await component?.HandleRightClickEventAsync(uiWindow, uiEvent);
                    }
                });

                //if (this.HitTestVisible) uiEvent.Handled = true;
            }
            //if (this.HitTestVisible && this.FinalRect.Contains(uiEvent.X, uiEvent.Y)) uiEvent.Handled = true;
        }

        public virtual async Task HandleKeyDownAsync(UIWindow uiWindow, UIKeyEvent uiEvent)
        {
            await PropagateAsync(uiEvent, uiWindow, async (component) => { await component?.HandleKeyDownAsync(uiWindow, uiEvent); });
        }

        public virtual async Task HandleKeyUpAsync(UIWindow uiWindow, UIKeyEvent uiEvent)
        {
            await PropagateAsync(uiEvent, uiWindow, async (component) => { await component?.HandleKeyUpAsync(uiWindow, uiEvent); });
        }

        public virtual async Task HandleKeyPressAsync(UIWindow uiWindow, UIKeyEvent uiEvent)
        {
            await PropagateAsync(uiEvent, uiWindow, async (component) => { await component?.HandleKeyPressAsync(uiWindow, uiEvent); });
        }

        public virtual async Task HandleFocusChangedEventAsync(UIWindow uiWindow, UIFocusChangedEvent uiEvent)
        {
            await PropagateAsync(uiEvent, uiWindow, async (component) => { await component?.HandleFocusChangedEventAsync(uiWindow, uiEvent); });

            HasFocus = uiEvent.Focused;
        }

        public virtual async Task HandleHoverChangedAsync(UIWindow uiWindow, UIHoverChangedEvent uiEvent)
        {
            if (uiEvent.ForcePropogation || FinalRect.Contains(uiEvent.X, uiEvent.Y))
            {
                await PropagateAsync(uiEvent, uiWindow, async (component) =>
                {
                    if (component != null)
                    {
                        // Always propogate event if Hover = False as we've aleady moved off that control
                        if (uiEvent.ForcePropogation || component.FinalRect.Contains(uiEvent.X, uiEvent.Y))
                        {
                            await component.HandleHoverChangedAsync(uiWindow, uiEvent);

                            MouseHover = uiEvent.Hover;
                        }
                    }
                });

                if (!uiEvent.Handled)
                {
                    MouseHover = uiEvent.Hover;
                }
            }

        }



        // --- Test Serialising / Deserialising ---
        private static IEnumerable<Type> GetKnownTypes()
        {
            if (knownTypes == null)
                knownTypes = Assembly.GetExecutingAssembly()
                                        .GetTypes()
                                        .Where(t => typeof(UIComponent).IsAssignableFrom(t))
                                        .ToList();
            return knownTypes;
        }


        //[OnSerializing()]
        //internal void OnSerializingMethod(StreamingContext context)
        //{
        //    //            member2 = "This value went into the data file during serialization.";
        //}

        //[OnSerialized()]
        //internal void OnSerializedMethod(StreamingContext context)
        //{
        //    //            member2 = "This value was reset after serialization.";
        //}

        //[OnDeserializing()]
        //internal void OnDeserializingMethod(StreamingContext context)
        //{
        //    //            member3 = "This value was set during deserialization";
        //}

        //[OnDeserialized()]
        //internal void OnDeserializedMethod(StreamingContext context)
        //{
        //    //            member4 = "This value was set after deserialization.";
        //}


        public override string ToString()
        {
            return $"Name={Name} : " + base.ToString();
        }
    }
}
