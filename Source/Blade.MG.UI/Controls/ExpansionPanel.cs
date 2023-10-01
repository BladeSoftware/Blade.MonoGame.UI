using Blade.MG.UI.Components;
using Blade.MG.UI.Controls.Templates;
using Blade.MG.UI.Models;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace Blade.MG.UI.Controls
{

    public class ExpansionPanel : StackPanel
    {
        private static int frameID = 0;
        public Type ItemTemplateType { get; set; } = typeof(TreeNodeTemplate);


        public ITreeNode RootNode { get; set; }

        private UIComponent TempNodeTemplate = null;



        //private Size NodesDesiredSize { get; set; }

        private Stopwatch swArrange;
        private long min = long.MaxValue, max = long.MinValue, sum = 0, cnt = 0, avg = 0;


        public ExpansionPanel()
        {
            HitTestVisible = true;
        }

        protected override void InitTemplate()
        {
            base.InitTemplate();
        }

        public override void Measure(UIContext context, ref Size availableSize, ref Layout parentMinMax)
        {
            base.Measure(context, ref availableSize, ref parentMinMax);


            // -- Measure Children ---
            float desiredWidth = 0f;
            float desiredHeight = 0f;


            //MeasureTree(context, availableSize, ref parentMinMax, ref desiredWidth, ref desiredHeight);


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
            //NodesDesiredSize = DesiredSize;

            ClampDesiredSize(availableSize, parentMinMax);
        }


        private void MeasureOneNode(UIContext context, ref Size availableSize, ref Layout parentMinMax, ref bool collapsed, ref float desiredWidth, ref float desiredHeight, UIComponent nodeTemplate)
        {
            nodeTemplate.Measure(context, ref availableSize, ref parentMinMax);

            if (!collapsed)
            {
                desiredHeight += nodeTemplate.DesiredSize.Height;

                if (nodeTemplate.DesiredSize.Width > desiredWidth)
                {
                    desiredWidth = nodeTemplate.DesiredSize.Width;
                }
            }
            else
            {
                nodeTemplate.DesiredSize = nodeTemplate.DesiredSize with { Width = 0, Height = 0 };
            }


            //nodeTemplate.Visible = collapsed ? Visibility.Hidden : Visibility.Visible;

            //if (collapsed)
            //{
            //    nodeTemplate.DesiredSize = nodeTemplate.DesiredSize with { Width = 0, Height = 0 };
            //}

            //desiredHeight += nodeTemplate.DesiredSize.Height;

            //if (nodeTemplate.DesiredSize.Width > desiredWidth)
            //{
            //    desiredWidth = nodeTemplate.DesiredSize.Width;
            //}
        }

        /// <summary>
        /// Layout Children
        /// </summary>
        /// <param name="layoutBounds">Size of Parent Container</param>
        public override void Arrange(UIContext context, Rectangle layoutBounds, Rectangle parentLayoutBounds)
        {
            // Arrange the layout for the inherited scroll panel and it's scrollbars
            //base.Arrange(context, layoutBounds);
            //RemoveAllChildren();

            float desiredWidth = 0;
            float desiredHeight = 0;

            swArrange = Stopwatch.StartNew();
            ArrangeTree(context, layoutBounds, ref desiredWidth, ref desiredHeight);
            swArrange.Stop();

            var nodesDesiredSize = new Size(desiredWidth, desiredHeight);


            base.Arrange(context, layoutBounds, parentLayoutBounds);


            // --=== Re-calculate the Scrollbar Max Values ===--
            // Can't rely on base scroll panel to calculate this as the children are virtualised

            // Substract the available parent area, as we don't need to scroll if everything fits in the available space
            int w = (int)nodesDesiredSize.Width - FinalContentRect.Width;
            int h = (int)nodesDesiredSize.Height - FinalContentRect.Height;

            // Make sure things don't go negative
            if (w < 0) w = 0;
            if (h < 0) h = 0;


            HorizontalScrollBar.MaxValue = w;
            VerticalScrollBar.MaxValue = h;


            if (swArrange.ElapsedMilliseconds < min) min = swArrange.ElapsedMilliseconds;
            if (swArrange.ElapsedMilliseconds > max) max = swArrange.ElapsedMilliseconds;
            sum += swArrange.ElapsedMilliseconds;
            cnt++;
            avg = sum / cnt;

            //Trace.WriteLine($"Arrange = {swArrange.ElapsedMilliseconds} : min={min} : max={max} : avg={avg} : cnt={cnt}");
        }


        private void ArrangeTree(UIContext context, Rectangle layoutBounds, ref float desiredWidth, ref float desiredHeight)
        {
            // Arrange the Tree Nodes
            if (RootNode == null)
            {
                return;
            }

            frameID = frameID % 2000000 + 1;

            Rectangle nodeBounds = layoutBounds;
            Size availableSize = new Size(layoutBounds.Width, layoutBounds.Height);
            Layout parentMinMax = new Layout(MinWidth, MinHeight, MaxWidth, MaxHeight, availableSize);

            ArrangeNode(context, ref availableSize, ref parentMinMax, ref layoutBounds, ref nodeBounds, RootNode, false, 0, ref desiredWidth, ref desiredHeight);

            // Remove stale children
            for (int i = Children.Count() - 1; i >= 0; i--)
            {
                if (Children.ElementAt(i).FrameID != frameID)
                {
                    ((IList<UIComponent>)Children).RemoveAt(i);
                }

            }

        }

        private void ArrangeNode(UIContext context, ref Size availableSize, ref Layout parentMinMax, ref Rectangle layoutBounds, ref Rectangle nodeBounds, ITreeNode node, bool collapsed, int depth, ref float desiredWidth, ref float desiredHeight)
        {
            if (node == null)
            {
                return;
            }

            var nodeTemplate = GetNodeTemplate(node, out bool isExistingNode);

            nodeTemplate.FrameID = frameID;

            MeasureOneNode(context, ref availableSize, ref parentMinMax, ref collapsed, ref desiredWidth, ref desiredHeight, nodeTemplate);

            nodeBounds.Height = (int)nodeTemplate.DesiredSize.Height;
            //nodeTemplate.Arrange(context, nodeBounds with { X = nodeBounds.X + 20 * depth, Width = nodeBounds.Width - 20 * depth });  // Indent child nodes
            nodeTemplate.Padding.Value = nodeTemplate.Padding.Value with { Left = 20 * depth };
            nodeTemplate.Arrange(context, nodeBounds, nodeBounds);

            nodeBounds.Y += nodeBounds.Height;

            if (!collapsed && Rectangle.Intersect(GetChildBoundingBox(context, nodeTemplate), FinalRect) != Rectangle.Empty)
            {
                if (!isExistingNode)
                {
                    AddChild(nodeTemplate, this, node);
                    TempNodeTemplate = null;
                }
            }
            else if (isExistingNode)
            {
                RemoveChild(nodeTemplate);
            }

            collapsed = collapsed || !node.IsExpanded;

            if (node.Children != null)
            {
                //foreach (var childNode in CollectionsMarshal.AsSpan<ITreeNode>((List<ITreeNode>)node.Nodes))
                foreach (var childNode in node.Children)
                {
                    ArrangeNode(context, ref availableSize, ref parentMinMax, ref layoutBounds, ref nodeBounds, childNode, collapsed, depth + 1, ref desiredWidth, ref desiredHeight);
                }
            }

        }

        /// <summary>
        /// Calculate Childs layout rectangle
        /// </summary>
        /// <param name="child"></param>
        /// <returns></returns>
        public override Rectangle GetChildBoundingBox(UIContext context, UIComponent child)
        {
            return child.FinalRect with { X = child.FinalRect.X - HorizontalScrollBar.ScrollOfset, Y = child.FinalRect.Y - VerticalScrollBar.ScrollOfset };
            //return child.finalContentRect with { X = child.finalContentRect.X - HorizontalScrollBar.ScrollOfset, Y = child.finalContentRect.Y - VerticalScrollBar.ScrollOfset };
        }

        public override void RenderControl(UIContext context, Rectangle layoutBounds, Transform parentTransform)
        {
            base.RenderControl(context, Rectangle.Intersect(layoutBounds, FinalRect), parentTransform);
        }


        private int focusedNode = -1;
        private ITreeNode selectedNode = null;


        private UIComponent GetNodeTemplate(ITreeNode node, out bool isExistingNode)
        {
            var nodeTemplate = Children.Where(p => p.DataContext.GetHashCode() == node.GetHashCode()).FirstOrDefault() as UIComponent;
            if (nodeTemplate != null)
            {
                isExistingNode = true;
                return nodeTemplate;
            }

            isExistingNode = false;

            if (TempNodeTemplate == null)
            {
                //TempNodeTemplate = new TreeNodeTemplate();
                TempNodeTemplate = Activator.CreateInstance(ItemTemplateType) as UIComponent;
            }

            InitNodeTemplate(TempNodeTemplate, node);


            ((UIComponentEvents)TempNodeTemplate).OnFocusChanged = (eventSource, uiEvent) =>
            {
                var nodeTemplate = (UIComponent)eventSource;
                var treeNode = nodeTemplate.DataContext as ITreeNode;

                if (!uiEvent.Focused && focusedNode == nodeTemplate.DataContext.GetHashCode())
                {
                    UIComponent childNode = FindChildByHash(nodeTemplate.DataContext.GetHashCode());
                    if (childNode != null)
                    {
                        childNode.HasFocus = false;
                        childNode.StateHasChanged();
                    }

                    //for (int i = Children.Count() - 1; i >= 0; i--)
                    //{
                    //    if (Children.ElementAt(i).DataContext.GetHashCode() == nodeTemplate.DataContext.GetHashCode())
                    //    {
                    //        UIComponent childNode = Children.ElementAt(i) as UIComponent;
                    //        childNode.HasFocus = false;
                    //        childNode.StateHasChanged();
                    //    }

                    //}

                    focusedNode = -1;

                }
                else if (uiEvent.Focused)
                {
                    // De-select previously selected node
                    if (selectedNode != null)
                    {
                        selectedNode.IsSelected = false;

                        UIComponent childNode = FindChildByHash(selectedNode.GetHashCode());
                        if (childNode != null)
                        {
                            childNode.StateHasChanged();
                        }

                    }

                    // Select new node
                    selectedNode = treeNode;
                    treeNode.IsSelected = true;
                    focusedNode = nodeTemplate.DataContext.GetHashCode();
                }
            };

            TempNodeTemplate.HasFocus = focusedNode == TempNodeTemplate.DataContext.GetHashCode();
            TempNodeTemplate.StateHasChanged();

            return TempNodeTemplate;
        }

        private void InitNodeTemplate(UIComponent nodeTemplate, ITreeNode node)
        {
            //TreeNode treeNode = node as TreeNode;

            nodeTemplate.Parent = this;
            nodeTemplate.ReInitTemplate(node);
        }

        private UIComponent FindChildByHash(int hash)
        {
            for (int i = Children.Count() - 1; i >= 0; i--)
            {
                if (Children.ElementAt(i).DataContext.GetHashCode() == hash)
                {
                    return Children.ElementAt(i);
                }
            }

            return null;
        }

        // ---=== UI Events ===---

        //public override void HandleHoverChanged(UIWindow uiWindow, UIHoverChangedEvent uiEvent)
        //{
        //    base.HandleHoverChanged(uiWindow, uiEvent);
        //}

    }
}
