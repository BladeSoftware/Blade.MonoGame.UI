using Blade.MG.UI.Components;
using Blade.MG.UI.Controls.Templates;
using Blade.MG.UI.Events;
using Microsoft.Xna.Framework;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Blade.MG.UI.Controls
{
    public class ListView : StackPanel
    {
        private static int frameID = 0;

        //public Orientation Orientation = Orientation.Vertical;

        [JsonIgnore]
        [XmlIgnore]
        public Type ItemTemplateType { get; set; } = typeof(ListViewItemTemplate);

        private UIComponent TempNodeTemplate = null;

        protected override void InitTemplate()
        {
            base.InitTemplate();

            IsHitTestVisible = true;
        }

        public override void Measure(UIContext context, ref Size availableSize, ref Layout parentMinMax)
        {
            base.Measure(context, ref availableSize, ref parentMinMax);

            // -- Measure Children ---
            float desiredWidth = 0f;
            float desiredHeight = 0f;

            var desiredize = DesiredSize;

            //foreach (var child in Children)
            foreach (var child in CollectionsMarshal.AsSpan(Children))
            {
                child.Measure(context, ref desiredize, ref parentMinMax);

                if (!float.IsNaN(child.DesiredSize.Height))
                {
                    desiredHeight += child.DesiredSize.Height;
                }

                if (!float.IsNaN(child.DesiredSize.Width) && child.DesiredSize.Width > desiredWidth)
                {
                    desiredWidth = child.DesiredSize.Width;
                }

            }


            // -- Merge Child Desired Size with this Stack Panel's Desired Size --
            if (!float.IsNaN(desiredWidth) && !float.IsNaN(DesiredSize.Width) && desiredWidth < DesiredSize.Width)
            {
                desiredWidth = DesiredSize.Width;
            }

            if (!float.IsNaN(desiredHeight) && !float.IsNaN(DesiredSize.Height) && desiredHeight < DesiredSize.Height)
            {
                desiredHeight = DesiredSize.Height;
            }


            DesiredSize = new Size(desiredWidth, desiredHeight);

            ClampDesiredSize(availableSize, parentMinMax);
        }

        private void MeasureOneListItem(UIContext context, ref Size availableSize, ref Layout parentMinMax, ref float desiredWidth, ref float desiredHeight, UIComponent itemTemplate)
        {
            itemTemplate.Measure(context, ref availableSize, ref parentMinMax);

            //if (!collapsed)
            //{
            desiredHeight += itemTemplate.DesiredSize.Height;

            if (itemTemplate.DesiredSize.Width > desiredWidth)
            {
                desiredWidth = itemTemplate.DesiredSize.Width;
            }
            //}
            //else
            //{
            //    itemTemplate.DesiredSize = itemTemplate.DesiredSize with { Width = 0, Height = 0 };
            //}

        }

        /// <summary>
        /// Layout Children
        /// </summary>
        /// <param name="layoutBounds">Size of Parent Container</param>
        public override void Arrange(UIContext context, Rectangle layoutBounds, Rectangle parentLayoutBounds)
        {
            frameID = frameID % 2000000 + 1;

            base.Arrange(context, layoutBounds, parentLayoutBounds);

            // Measure the total child control layout width and height
            float desiredWidth = 0;
            float desiredHeight = 0;


            Rectangle nodeBounds = FinalContentRect;
            Size availableSize = new Size(FinalContentRect.Width, FinalContentRect.Height);
            Layout parentMinMax = new Layout(MinWidth, MinHeight, MaxWidth, MaxHeight, availableSize);

            IEnumerable<object> listItems = DataContext as IEnumerable<object>;

            if (listItems != null)
            {
                foreach (var item in listItems)
                {
                    ArrangeItem(context, ref availableSize, ref parentMinMax, ref layoutBounds, ref nodeBounds, item, ref desiredWidth, ref desiredHeight);
                }
            }


            // Remove stale children
            for (int i = Children.Count() - 1; i >= 0; i--)
            {
                if (Children.ElementAt(i).FrameID != frameID)
                {
                    ((IList<UIComponent>)Children).RemoveAt(i);
                }

            }


            var ListDesiredSize = new Size(desiredWidth, desiredHeight);


            // --=== Re-calculate the Scrollbar Max Values ===--
            // Can't rely on base scroll panel to calculate this as the children are virtualised

            // Substract the available parent area, as we don't need to scroll if everything fits in the available space
            int w = (int)ListDesiredSize.Width - FinalContentRect.Width;
            int h = (int)ListDesiredSize.Height - FinalContentRect.Height;

            // Make sure things don't go negative
            if (w < 0) w = 0;
            if (h < 0) h = 0;

            HorizontalScrollBar.MaxValue = w; // width - finalContentRect.Width;
            VerticalScrollBar.MaxValue = h; // height - finalContentRect.Height;

        }

        private void ArrangeItem(UIContext context, ref Size availableSize, ref Layout parentMinMax, ref Rectangle layoutBounds, ref Rectangle nodeBounds, object itemDataContext, ref float desiredWidth, ref float desiredHeight)
        {
            if (itemDataContext == null)
            {
                return;
            }

            var nodeTemplate = GetNodeTemplate(itemDataContext, out bool isExistingNode);

            nodeTemplate.FrameID = frameID;

            MeasureOneListItem(context, ref availableSize, ref parentMinMax, ref desiredWidth, ref desiredHeight, nodeTemplate);

            nodeBounds.Height = (int)nodeTemplate.DesiredSize.Height;
            nodeTemplate.Arrange(context, nodeBounds, nodeBounds);
            nodeBounds.Y += nodeBounds.Height;

            if (Rectangle.Intersect(GetChildBoundingBox(context, nodeTemplate), FinalRect) != Rectangle.Empty)
            {
                if (!isExistingNode)
                {
                    AddChild(nodeTemplate, this, itemDataContext);
                    TempNodeTemplate = null;
                }
            }
            else if (isExistingNode)
            {
                RemoveChild(nodeTemplate);
            }

        }

        /// <summary>
        /// Calculate Childs layout rectangle
        /// </summary>
        /// <param name="child"></param>
        /// <returns></returns>
        public override Rectangle GetChildBoundingBox(UIContext context, UIComponent child)
        {
            return child.FinalContentRect with { X = child.FinalContentRect.X - HorizontalScrollBar.ScrollOfset, Y = child.FinalContentRect.Y - VerticalScrollBar.ScrollOfset };
        }

        public override void RenderControl(UIContext context, Rectangle layoutBounds, Transform parentTransform)
        {
            if (Visible.Value != Visibility.Visible)
            {
                return;
            }

            base.RenderControl(context, Rectangle.Intersect(layoutBounds, FinalRect), parentTransform);
        }


        private UIComponent GetNodeTemplate(object itemDataContext, out bool isExistingNode)
        {
            var nodeTemplate = Children.Where(p => p.DataContext.GetHashCode() == itemDataContext.GetHashCode()).FirstOrDefault() as UIComponent;
            if (nodeTemplate != null)
            {
                isExistingNode = true;
                return nodeTemplate;
            }

            isExistingNode = false;

            if (TempNodeTemplate == null)
            {
                //TempNodeTemplate = new ListViewItemTemplate();
                TempNodeTemplate = Activator.CreateInstance(ItemTemplateType) as UIComponent;
            }

            InitNodeTemplate(TempNodeTemplate, itemDataContext);

            return TempNodeTemplate;
        }

        private void InitNodeTemplate(UIComponent itemTemplate, object itemDataContext)
        {
            itemTemplate.Parent = this;
            itemTemplate.ReInitTemplate(itemDataContext);
        }


        private object selectedItem;
        public object SelectedItem
        {
            get
            {
                return selectedItem;
            }

            set
            {
                if (selectedItem != value)
                {
                    selectedItem = value;

                    foreach (var child in Children)
                    {
                        if (child.DataContext == value)
                        {
                            SelectedItemTemplate = child;
                            break;
                        }
                    }

                    // TODO: Raise Selected Item Changed
                    // ...
                }
            }
        }

        protected UIComponent selectedItemTemplate;
        protected UIComponent SelectedItemTemplate
        {
            get { return selectedItemTemplate; }
            set
            {
                if (selectedItemTemplate != value)
                {
                    var oldSelectedItem = selectedItemTemplate;
                    selectedItemTemplate = value;

                    // Notify controls that State has changed
                    oldSelectedItem?.StateHasChanged();
                    selectedItemTemplate?.StateHasChanged();
                }
            }
        }


        // ---=== UI Events ===---

        // Override the Click Event handling
        public override async Task HandleClickEventAsync(UIWindow uiWindow, UIClickEvent uiEvent)
        {
            // If we've clicked on a list item, then update SelectedItem
            if (FinalRect.Contains(uiEvent.X, FinalRect.Y))
            {
                foreach (var child in Children)
                {
                    if (child.FinalRect.Contains(uiEvent.X, uiEvent.Y))
                    {
                        SelectedItem = child?.DataContext;
                        SelectedItemTemplate = child;
                        //SelectedItem = selectedItemTemplate?.DataContext;
                        break;
                    }
                }
            }

            await base.HandleClickEventAsync(uiWindow, uiEvent);

            //OnClick?.Invoke(uiEvent);
        }

    }
}
