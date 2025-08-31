using Blade.MG.UI.Components;
using Blade.MG.UI.Controls.Templates;
using Blade.MG.UI.Events;
using Blade.MG.UI.Models;
using Microsoft.VisualStudio.Threading;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Blade.MG.UI.Controls
{

    public class TreeView : StackPanel
    {
        private static int frameID = 0;

        [JsonIgnore]
        [XmlIgnore]
        public Type ItemTemplateType { get; set; } = typeof(TreeNodeTemplate);


        public ITreeNode RootNode { get; set; }  // TODO: Set from DataContext ?
        public bool ShowRootNode { get; set; } = true;


        private UIComponent TempNodeTemplate = null;


        private int focusedNodeHash = -1;
        private ITreeNode selectedNode = null;

        public Action<ITreeNode> OnSelectedNodeChanged { get; set; }
        public Func<ITreeNode, Task> OnSelectedNodeChangedAsync { get; set; }

        public Action<object, UIClickEvent> OnNodeClick { get; set; }
        public Func<object, UIClickEvent, Task> OnNodeClickAsync { get; set; }

        public Action<object, UIClickEvent> OnNodeRightClick { get; set; }
        public Func<object, UIClickEvent, Task> OnNodeRightClickAsync { get; set; }

        public Action<object, UIClickEvent> OnNodeMultiClick { get; set; }
        public Func<object, UIClickEvent, Task> OnNodeMultiClickAsync { get; set; }

        public Func<object, string, Task<bool>> OnNodeRenamedAsync { get; set; }

        private JoinableTaskFactory joinableTaskFactory;

        //private Size NodesDesiredSize { get; set; }

        private Stopwatch swArrange;
        private long min = long.MaxValue;
        private long max = long.MinValue;
        private long sum = 0;
        private long cnt = 0;
        private long avg = 0;


        protected override void InitTemplate()
        {
            base.InitTemplate();

            joinableTaskFactory = new JoinableTaskFactory(new JoinableTaskContext());

            Orientation = Orientation.Vertical;

            IsHitTestVisible = true;
        }

        public override void Measure(UIContext context, ref Size availableSize, ref Layout parentMinMax)
        {
            base.Measure(context, ref availableSize, ref parentMinMax);


            // -- Measure Children ---
            float desiredWidth = 0f;
            float desiredHeight = 0f;


            MeasureTree(context, availableSize, ref parentMinMax, ref desiredWidth, ref desiredHeight);


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

        float totalNodeWidth = 0;

        private void MeasureTree(UIContext context, Size availableSize, ref Layout parentMinMax, ref float desiredWidth, ref float desiredHeight)
        {
            totalNodeWidth = 0;

            // Arrange the Tree Nodes
            if (RootNode == null)
            {
                return;
            }

            MeasureNode(context, ref availableSize, ref parentMinMax, RootNode, false, true, 0, ref desiredWidth, ref desiredHeight);
        }


        private void MeasureNode(UIContext context, ref Size availableSize, ref Layout parentMinMax, ITreeNode node, bool collapsed, bool isRoot, int depth, ref float desiredWidth, ref float desiredHeight)
        {
            if (node == null)
            {
                return;
            }

            bool skipNode = false;
            if (isRoot && !ShowRootNode)
            {
                depth = -1;
                skipNode = true;
            }

            if (!skipNode)
            {
                var nodeTemplate = GetNodeTemplate(node, out bool isExistingNode);

                nodeTemplate.FrameID = frameID;

                MeasureOneNode(context, ref availableSize, ref parentMinMax, ref collapsed, ref desiredWidth, ref desiredHeight, nodeTemplate);

                desiredHeight += (int)nodeTemplate.DesiredSize.Height;

                //float nodeWidth = Padding.Value.Horizontal + nodeTemplate.DesiredSize.Width + nodeTemplate.Padding.Value.Horizontal + nodeTemplate.Margin.Value.Horizontal;
                float nodeWidth = nodeTemplate.DesiredSize.Width;
                if (nodeWidth > totalNodeWidth)
                {
                    totalNodeWidth = nodeWidth;
                }


                collapsed = collapsed || !node.IsExpanded;
            }

            if (node.Children != null)
            {
                //foreach (var childNode in CollectionsMarshal.AsSpan<ITreeNode>((List<ITreeNode>)node.Nodes))
                foreach (var childNode in node.Children)
                {
                    MeasureNode(context, ref availableSize, ref parentMinMax, childNode, collapsed, false, depth + 1, ref desiredWidth, ref desiredHeight);
                }
            }

        }

        private void MeasureOneNode(UIContext context, ref Size availableSize, ref Layout parentMinMax, ref bool collapsed, ref float desiredWidth, ref float desiredHeight, UIComponent nodeTemplate)
        {
            var newSize = new Size(float.NaN, float.NaN);
            nodeTemplate.Measure(context, ref newSize, ref parentMinMax);
            //nodeTemplate.Measure(context, ref availableSize, ref parentMinMax);

            if (!collapsed)
            {
                desiredHeight += nodeTemplate.DesiredSize.Height;
            }
            else
            {
                //nodeTemplate.DesiredSize = nodeTemplate.DesiredSize with { Width = 0, Height = 0 };
                nodeTemplate.DesiredSize = nodeTemplate.DesiredSize with { Height = 0 };
            }

            if (nodeTemplate.DesiredSize.Width > desiredWidth)
            {
                desiredWidth = nodeTemplate.DesiredSize.Width;
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
            //base.Arrange(context, layoutBounds);
            base.ArrangeSelf(context, layoutBounds, parentLayoutBounds);

            //RemoveAllChildren();

            float desiredWidth = 0;
            float desiredHeight = 0;

            swArrange = Stopwatch.StartNew();

            int verticalScrollBarWidth = IsVerticalScrollbarVisible ? (int)VerticalScrollBar.Width.ToPixels() : 0;
            int horizontalScrollBarHeight = IsHorizontalScrollbarVisible ? (int)HorizontalScrollBar.Height.ToPixels() : 0;

            var treeLayoutBounds = parentLayoutBounds with { Width = layoutBounds.Width - verticalScrollBarWidth, Height = layoutBounds.Height - horizontalScrollBarHeight };

            ArrangeTree(context, treeLayoutBounds, ref desiredWidth, ref desiredHeight);

            //ArrangeTree(context, layoutBounds, ref desiredWidth, ref desiredHeight);
            swArrange.Stop();

            var nodesDesiredSize = new Size(desiredWidth, desiredHeight);


            base.Arrange(context, layoutBounds, parentLayoutBounds);


            // --=== Re-calculate the Scrollbar Max Values ===--
            // Can't rely on base scroll panel to calculate this as the children are virtualised

            // Substract the available parent area, as we don't need to scroll if everything fits in the available space
            int w = (int)nodesDesiredSize.Width + Padding.Value.Horizontal + Margin.Value.Horizontal - FinalContentRect.Width;
            int h = (int)nodesDesiredSize.Height + Padding.Value.Vertical + Margin.Value.Vertical - FinalContentRect.Height;

            // Make sure things don't go negative
            if (w < 0) w = 0;
            if (h < 0) h = 0;


            HorizontalScrollBar.MaxValue = w;
            VerticalScrollBar.MaxValue = h;

            //isHorizontallyScrollable = (FinalContentRect.Width < nodesDesiredSize.Width);
            //isVerticallyScrollable = (FinalContentRect.Height < nodesDesiredSize.Height);
            isHorizontallyScrollable = w > 0;
            isVerticallyScrollable = h > 0;

            HorizontalScrollBar.Visible = BoolToVisibility(IsHorizontalScrollbarVisible);
            VerticalScrollBar.Visible = BoolToVisibility(IsVerticalScrollbarVisible);


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

            ArrangeNode(context, ref availableSize, ref parentMinMax, ref layoutBounds, ref nodeBounds, RootNode, false, true, 0, ref desiredWidth, ref desiredHeight);

            // Remove stale children
            for (int i = Children.Count() - 1; i >= 0; i--)
            {
                if (Children.ElementAt(i).FrameID != frameID)
                {
                    ((IList<UIComponent>)Children).RemoveAt(i);
                }
            }

        }

        private void ArrangeNode(UIContext context, ref Size availableSize, ref Layout parentMinMax, ref Rectangle layoutBounds, ref Rectangle nodeBounds, ITreeNode node, bool collapsed, bool isRoot, int depth, ref float desiredWidth, ref float desiredHeight)
        {
            if (node == null)
            {
                return;
            }

            if (isRoot && !ShowRootNode)
            {
                depth = -1;
                goto SkipNode;
            }

            var nodeTemplate = GetNodeTemplate(node, out bool isExistingNode);

            nodeTemplate.FrameID = frameID;

            MeasureOneNode(context, ref availableSize, ref parentMinMax, ref collapsed, ref desiredWidth, ref desiredHeight, nodeTemplate);

            nodeBounds.Height = (int)nodeTemplate.DesiredSize.Height;
            //nodeTemplate.Arrange(context, nodeBounds with { X = nodeBounds.X + 20 * depth, Width = nodeBounds.Width - 20 * depth });  // Indent child nodes

            //if (desiredWidth < totalNodeWidth)
            //{
            desiredWidth = totalNodeWidth;
            //}

            nodeTemplate.Padding.Value = nodeTemplate.Padding.Value with { Left = 20 * depth };

            nodeBounds.Width = (int)desiredWidth;
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

        SkipNode:

            if (node.Children != null)
            {
                //foreach (var childNode in CollectionsMarshal.AsSpan<ITreeNode>((List<ITreeNode>)node.Nodes))
                foreach (var childNode in node.Children)
                {
                    ArrangeNode(context, ref availableSize, ref parentMinMax, ref layoutBounds, ref nodeBounds, childNode, collapsed, false, depth + 1, ref desiredWidth, ref desiredHeight);
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
            //var childText = ((dynamic)child)?.label1?.Text;
            //if (string.Equals(childText, "FreeFall")) { };

            return child.FinalRect with { X = child.FinalRect.X - HorizontalScrollBar.ScrollOfset, Y = child.FinalRect.Y - VerticalScrollBar.ScrollOfset };
            //return child.finalContentRect with { X = child.finalContentRect.X - HorizontalScrollBar.ScrollOfset, Y = child.finalContentRect.Y - VerticalScrollBar.ScrollOfset };
        }

        public override void RenderControl(UIContext context, Rectangle layoutBounds, Transform parentTransform)
        {
            base.RenderControl(context, Rectangle.Intersect(layoutBounds, FinalRect), parentTransform);
        }

        /// <summary>
        /// Returns an existing node template if one exists or null
        /// A node template will only exist if the node is currently visible
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected UIComponent GetExistingNodeTemplate(ITreeNode node)
        {
            var nodeTemplate = Children.Where(p => p.DataContext.GetHashCode() == node.GetHashCode()).FirstOrDefault() as UIComponent;

            return nodeTemplate;
        }

        /// <summary>
        /// Returns an existing node template if one exists otherwise creates and returns a new node template
        /// </summary>
        /// <param name="node"></param>
        /// <param name="isExistingNode"></param>
        /// <returns></returns>
        protected UIComponent GetNodeTemplate(ITreeNode node, out bool isExistingNode)
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


            ((UIComponentEvents)TempNodeTemplate).OnPrimaryClickAsync = async (eventSource, uiEvent) =>
            {
                await HandleNodeClickAsync(ParentWindow, uiEvent, ((UIComponent)eventSource).DataContext as ITreeNode);
            };

            ((UIComponentEvents)TempNodeTemplate).OnSecondaryClickAsync = async (eventSource, uiEvent) =>
            {
                await HandleNodeRightClickAsync(ParentWindow, uiEvent, ((UIComponent)eventSource).DataContext as ITreeNode);
            };

            ((UIComponentEvents)TempNodeTemplate).OnMultiClickAsync = async (eventSource, uiEvent) =>
            {
                await HandleNodeMultiClickAsync(ParentWindow, uiEvent, ((UIComponent)eventSource).DataContext as ITreeNode);
            };


            ((UIComponentEvents)TempNodeTemplate).OnFocusChangedAsync = async (eventSource, uiEvent) =>
            {
                var nodeTemplate = (UIComponent)eventSource;
                var treeNode = nodeTemplate.DataContext as ITreeNode;

                if (!uiEvent.Focused && focusedNodeHash == nodeTemplate.DataContext.GetHashCode())
                {
                    UIComponent childNode = FindChildByHash(nodeTemplate.DataContext.GetHashCode());
                    if (childNode != null)
                    {
                        childNode.HasFocus = false;
                        childNode.StateHasChanged();
                    }

                    focusedNodeHash = -1;

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
                    focusedNodeHash = nodeTemplate.DataContext.GetHashCode();

                    // Raise Selected Node changed event
                    await HandleSelectedNodeChangedAsync(ParentWindow, UIEvent.Empty, selectedNode);
                }
            };

            // Check if TempNodeTemplate has focus
            bool hasFocus = focusedNodeHash == TempNodeTemplate.DataContext.GetHashCode();

            // Check if the Focused State of TempNodeTemplate matches the hasFocus value i.e.
            // If TempNoteTemplate is the Current Focused Component but TempNodeTemplate.HasFocus = False
            // Or TempNoteTemplate is not the Current Focused Component but TempNodeTemplate.HasFocus = True
            // Then fire the FocusChanged Event
            if (hasFocus != TempNodeTemplate.HasFocus.Value)
            {
                //TempNodeTemplate.HasFocus = hasFocus;
                ParentWindow.focusedComponent = TempNodeTemplate;

                joinableTaskFactory.Run(async () => await TempNodeTemplate.HandleFocusChangedEventAsync(ParentWindow, new UIFocusChangedEvent { Focused = hasFocus, ForcePropogation = false, Handled = false }));

                //TempNodeTemplate.HandleFocusChangedEventAsync(ParentWindow, new UIFocusChangedEvent { Focused = hasFocus, ForcePropogation = false, Handled = false });
            }

            //TempNodeTemplate.HasFocus = (focusedNode == TempNodeTemplate.DataContext.GetHashCode());
            TempNodeTemplate.StateHasChanged();

            return TempNodeTemplate;
        }

        private void InitNodeTemplate(UIComponent nodeTemplate, ITreeNode node)
        {
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

        public void ReInitTemplate(ITreeNode treeNode)
        {
            var nodeTemplate = GetExistingNodeTemplate(treeNode);
            //var nodeTemplate = GetNodeTemplate(treeNode, out _);
            if (nodeTemplate != null)
            {
                nodeTemplate.ReInitTemplate(treeNode);
            }
        }

        public async Task SetFocusAsync(ITreeNode treeNode)
        {
            //var nodeTemplate = GetExistingNodeTemplate(treeNode);
            var nodeTemplate = GetNodeTemplate(treeNode, out _);
            if (nodeTemplate != null)
            {
                focusedNodeHash = treeNode.GetHashCode();

                selectedNode = treeNode;
                selectedNode.IsSelected = true;

                await ParentWindow.SetFocusAsync(nodeTemplate);

                await HandleSelectedNodeChangedAsync(ParentWindow, UIEvent.Empty, selectedNode);

                nodeTemplate.StateHasChanged();
            }
        }

        public async Task SetFocusAsync(UIComponent nodeTemplate)
        {
            if (nodeTemplate != null)
            {
                focusedNodeHash = nodeTemplate.DataContext.GetHashCode();

                selectedNode = nodeTemplate.DataContext as ITreeNode;
                selectedNode.IsSelected = true;

                await ParentWindow.SetFocusAsync(nodeTemplate);

                await HandleSelectedNodeChangedAsync(ParentWindow, UIEvent.Empty, selectedNode);

                nodeTemplate.StateHasChanged();
            }
        }


        // ---=== UI Events ===---

        public virtual async Task HandleSelectedNodeChangedAsync(UIWindow uiWindow, UIEvent uiEvent, ITreeNode treeNode)
        {
            OnSelectedNodeChanged?.Invoke(treeNode);
            await (OnSelectedNodeChangedAsync?.Invoke(treeNode) ?? Task.CompletedTask);
        }

        public virtual async Task HandleNodeClickAsync(UIWindow uiWindow, UIClickEvent uiEvent, ITreeNode treeNode)
        {
            OnNodeClick?.Invoke(treeNode, uiEvent);
            await (OnNodeClickAsync?.Invoke(treeNode, uiEvent) ?? Task.CompletedTask);
        }

        public virtual async Task HandleNodeRightClickAsync(UIWindow uiWindow, UIClickEvent uiEvent, ITreeNode treeNode)
        {
            OnNodeRightClick?.Invoke(treeNode, uiEvent);
            await (OnNodeRightClickAsync?.Invoke(treeNode, uiEvent) ?? Task.CompletedTask);
        }

        public virtual async Task HandleNodeMultiClickAsync(UIWindow uiWindow, UIClickEvent uiEvent, ITreeNode treeNode)
        {
            OnNodeMultiClick?.Invoke(treeNode, uiEvent);
            await (OnNodeMultiClickAsync?.Invoke(treeNode, uiEvent) ?? Task.CompletedTask);
        }

    }
}
