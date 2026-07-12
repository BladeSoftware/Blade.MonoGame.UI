using Blade.MG.UI.Components;
using Blade.MG.UI.Controls.Templates;
using Blade.MG.UI.Events;
using Blade.MG.UI.Models;
using Microsoft.VisualStudio.Threading;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Blade.MG.UI.Controls
{

    public class TreeView : StackPanel
    {
        private static int frameID = 0;

        [JsonIgnore]
        public Type ItemTemplateType { get; set; } = typeof(TreeNodeTemplate);


        // TODO: Set from DataContext ? If this is ever implemented, mirror ListView's own use
        // of DataContext as its real items source - Container.AddChild now preserves an
        // already-set DataContext across an attach (see Container.AddChild), which is exactly
        // what makes that safe. Storing the root node value anywhere that gets silently
        // overwritten by AddChild's cascade would reintroduce the same bug ListView had.
        public ITreeNode RootNode { get; set; }
        public bool ShowRootNode { get; set; } = true;

        public float NodeIndentPerLevel { get; set; } = 20f;

        private UIComponent TempNodeTemplate = null;


        private int focusedNodeHash = -1;
        private ITreeNode selectedNode = null;

        // Read-only view of the currently selected node, for callers that just want to query
        // current state (e.g. an example page showing "Selected: ...") without having to wire
        // up OnSelectedNodeChanged themselves.
        public ITreeNode SelectedNode => selectedNode;

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

        private float _totalNodeWidth = 0;


        private float? _cachedNodeHeight;
        private float? _cachedNodeWidth;
        private Thickness? _cachedPadding;

        private void MeasureTree(UIContext context, Size availableSize, ref Layout parentMinMax, ref float desiredWidth, ref float desiredHeight)
        {
            _totalNodeWidth = 0;

            _cachedNodeHeight = null;
            _cachedNodeWidth = null;
            _cachedPadding = null;

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
                if (_cachedNodeHeight == null)
                {
                    var nodeTemplate = GetNodeTemplate(node, out bool isExistingNode);

                    nodeTemplate.FrameID = frameID;

                    MeasureOneNode(context, ref availableSize, ref parentMinMax, ref collapsed, ref desiredWidth, ref desiredHeight, nodeTemplate);
                }

                desiredHeight = _cachedNodeHeight.Value;
                desiredWidth = _cachedNodeWidth.Value;


                //desiredHeight += desiredHeight;
                //desiredHeight += (int)nodeTemplate.DesiredSize.Height;

                //float nodeWidth = Padding.Value.Horizontal + nodeTemplate.DesiredSize.Width + nodeTemplate.Padding.Value.Horizontal + nodeTemplate.Margin.Value.Horizontal;
                //float nodeWidth = nodeTemplate.DesiredSize.Width;
                float nodeWidth = desiredWidth; //nodeTemplate.DesiredSize.Width;
                //nodeWidth = 800;  // TODO: Remove
                if (nodeWidth > _totalNodeWidth)
                {
                    _totalNodeWidth = nodeWidth;
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

            _cachedNodeHeight = nodeTemplate.DesiredSize.Height;
            _cachedNodeWidth = FinalContentRect.Width; //nodeTemplate.DesiredSize.Width;
            _cachedPadding = nodeTemplate.Padding.Value;

            if (!collapsed)
            {
                desiredHeight += nodeTemplate.DesiredSize.Height;
                //desiredHeight += _cachedNodeHeight.Value;
            }
            else
            {
                nodeTemplate.DesiredSize = nodeTemplate.DesiredSize with { Height = 0 };
            }

            if (nodeTemplate.DesiredSize.Width > desiredWidth)
            {
                desiredWidth = nodeTemplate.DesiredSize.Width;
                //desiredWidth = _cachedNodeWidth.Value;
            }
        }

        //private void MeasureOneNode(UIContext context, ref Size availableSize, ref Layout parentMinMax, ref bool collapsed, ref float desiredWidth, ref float desiredHeight, UIComponent nodeTemplate)
        //{
        //    var newSize = new Size(float.NaN, float.NaN);

        //    nodeTemplate.Measure(context, ref newSize, ref parentMinMax);
        //    //nodeTemplate.Measure(context, ref availableSize, ref parentMinMax);

        //    if (!collapsed)
        //    {
        //        desiredHeight += nodeTemplate.DesiredSize.Height;
        //    }
        //    else
        //    {
        //        //nodeTemplate.DesiredSize = nodeTemplate.DesiredSize with { Width = 0, Height = 0 };
        //        nodeTemplate.DesiredSize = nodeTemplate.DesiredSize with { Height = 0 };
        //    }

        //    if (nodeTemplate.DesiredSize.Width > desiredWidth)
        //    {
        //        desiredWidth = nodeTemplate.DesiredSize.Width;
        //    }
        //}

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

            // Node positions are anchored on this TreeView's own on-screen position (layoutBounds),
            // not parentLayoutBounds - parentLayoutBounds is the grandparent container's content
            // area, which doesn't account for any sibling controls (e.g. a toolbar StackPanel)
            // positioned above this TreeView within that same area. Anchoring on parentLayoutBounds
            // silently shifted every node up by that sibling's height, pushing the root node (and
            // therefore the whole tree) partly or fully above this TreeView's own visible top edge.
            var treeLayoutBounds = layoutBounds with { Width = layoutBounds.Width - verticalScrollBarWidth, Height = layoutBounds.Height - horizontalScrollBarHeight };

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

            // Use this TreeView's own Padding here, not Parent's - Parent is the container
            // holding this TreeView, and its Margin/Padding is irrelevant to where nodes start
            // within this control's own content area. layoutBounds is already this TreeView's
            // own on-screen position with no margin included (margin is resolved by whoever
            // arranged this control), so Margin is not added here either.
            nodeBounds = nodeBounds with
            {
                X = layoutBounds.X + Padding.Value.Left,
                Y = layoutBounds.Y + Padding.Value.Top - (int)(_cachedNodeHeight.Value),
            };

            ArrangeNode(context, ref availableSize, ref parentMinMax, ref layoutBounds, ref nodeBounds, RootNode, false, true, 0, ref desiredWidth, ref desiredHeight);

            // Remove stale children. Children is a read-only view (Container.Children returns
            // children.AsReadOnly()) - RemoveChild is the actual mutation API; a raw
            // ((IList<UIComponent>)Children).RemoveAt(i) throws NotSupportedException since
            // ReadOnlyCollection<T> rejects every mutating IList<T> member.
            for (int i = Children.Count() - 1; i >= 0; i--)
            {
                UIComponent staleChild = Children.ElementAt(i);
                if (staleChild.FrameID != frameID)
                {
                    RemoveChild(staleChild);
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


            //nodeBounds.Height = (int)nodeTemplate.DesiredSize.Height;
            nodeBounds.Height = collapsed ? 0 : (int)_cachedNodeHeight.Value;
            nodeBounds.Width = (int)_totalNodeWidth; // TODO: Checking

            desiredHeight += nodeBounds.Height;
            desiredWidth = nodeBounds.Width;


            ////nodeTemplate.Arrange(context, nodeBounds with { X = nodeBounds.X + 20 * depth, Width = nodeBounds.Width - 20 * depth });  // Indent child nodes

            desiredWidth = _totalNodeWidth;

            //nodeTemplate.Padding.Value = nodeTemplate.Padding.Value with { Left = 20 * depth };

            //nodeBounds.Width = (int)desiredWidth;
            //nodeTemplate.Arrange(context, nodeBounds, nodeBounds);

            nodeBounds.Y += nodeBounds.Height;

            var nodeFinalRect = new Rectangle(nodeBounds.X - HorizontalScrollBar.ScrollOffset, nodeBounds.Y - VerticalScrollBar.ScrollOffset, nodeBounds.Width, nodeBounds.Height);

            //if (!collapsed && Rectangle.Intersect(GetChildBoundingBox(context, nodeTemplate), FinalRect) != Rectangle.Empty)
            if (!collapsed && Rectangle.Intersect(nodeFinalRect, FinalRect) != Rectangle.Empty)
            {
                var nodeTemplate = GetNodeTemplate(node, out bool isExistingNode);

                nodeTemplate.FrameID = frameID;

                nodeTemplate.Padding.Value = nodeTemplate.Padding.Value with { Left = (int)(NodeIndentPerLevel * depth) };

                MeasureOneNode(context, ref availableSize, ref parentMinMax, ref collapsed, ref desiredWidth, ref desiredHeight, nodeTemplate);
                if (desiredWidth > _totalNodeWidth)
                {
                    _totalNodeWidth = (int)desiredWidth;
                    _cachedNodeWidth = (int)desiredWidth;
                }


                nodeTemplate.Arrange(context, nodeBounds, nodeBounds);

                if (!isExistingNode)
                {
                    AddChild(nodeTemplate, this, node);
                    TempNodeTemplate = null;
                }
            }
            else
            {
                var nodeTemplate = GetExistingNodeTemplate(node);
                if (nodeTemplate != null)
                {
                    RemoveChild(nodeTemplate);
                }
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

        //private void ArrangeNode(UIContext context, ref Size availableSize, ref Layout parentMinMax, ref Rectangle layoutBounds, ref Rectangle nodeBounds, ITreeNode node, bool collapsed, bool isRoot, int depth, ref float desiredWidth, ref float desiredHeight)
        //{
        //    if (node == null)
        //    {
        //        return;
        //    }

        //    if (isRoot && !ShowRootNode)
        //    {
        //        depth = -1;
        //        goto SkipNode;
        //    }

        //    var nodeTemplate = GetNodeTemplate(node, out bool isExistingNode);

        //    nodeTemplate.FrameID = frameID;

        //    MeasureOneNode(context, ref availableSize, ref parentMinMax, ref collapsed, ref desiredWidth, ref desiredHeight, nodeTemplate);

        //    //nodeBounds.Height = (int)nodeTemplate.DesiredSize.Height;
        //    nodeBounds.Height = (int)nodeTemplate.DesiredSize.Height;


        //    //nodeTemplate.Arrange(context, nodeBounds with { X = nodeBounds.X + 20 * depth, Width = nodeBounds.Width - 20 * depth });  // Indent child nodes

        //    //if (desiredWidth < totalNodeWidth)
        //    //{
        //    desiredWidth = _totalNodeWidth;
        //    //}

        //    nodeTemplate.Padding.Value = nodeTemplate.Padding.Value with { Left = 20 * depth };

        //    nodeBounds.Width = (int)desiredWidth;
        //    nodeTemplate.Arrange(context, nodeBounds, nodeBounds);

        //    nodeBounds.Y += nodeBounds.Height;

        //    if (!collapsed && Rectangle.Intersect(GetChildBoundingBox(context, nodeTemplate), FinalRect) != Rectangle.Empty)
        //    {
        //        if (!isExistingNode)
        //        {
        //            AddChild(nodeTemplate, this, node);
        //            TempNodeTemplate = null;
        //        }
        //    }
        //    else if (isExistingNode)
        //    {
        //        RemoveChild(nodeTemplate);
        //    }

        //    collapsed = collapsed || !node.IsExpanded;

        //SkipNode:

        //    if (node.Children != null)
        //    {
        //        //foreach (var childNode in CollectionsMarshal.AsSpan<ITreeNode>((List<ITreeNode>)node.Nodes))
        //        foreach (var childNode in node.Children)
        //        {
        //            ArrangeNode(context, ref availableSize, ref parentMinMax, ref layoutBounds, ref nodeBounds, childNode, collapsed, false, depth + 1, ref desiredWidth, ref desiredHeight);
        //        }
        //    }

        //}

        /// <summary>
        /// Calculate Childs layout rectangle
        /// </summary>
        /// <param name="child"></param>
        /// <returns></returns>
        public override Rectangle GetChildBoundingBox(UIContext context, UIComponent child)
        {
            //var childText = ((dynamic)child)?.label1?.Text;
            //if (string.Equals(childText, "FreeFall")) { };

            return child.FinalRect with { X = child.FinalRect.X - HorizontalScrollBar.ScrollOffset, Y = child.FinalRect.Y - VerticalScrollBar.ScrollOffset };
            //return child.finalContentRect with { X = child.finalContentRect.X - HorizontalScrollBar.ScrollOffset, Y = child.finalContentRect.Y - VerticalScrollBar.ScrollOffset };
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


            ((UIComponentEvents)TempNodeTemplate).OnActivateAsync = async (eventSource, uiEvent) =>
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

                joinableTaskFactory.Run(async () => await TempNodeTemplate.HandleFocusChangedEventAsync(ParentWindow, new UIFocusChangedEvent { Focused = hasFocus, ForcePropagation = false, Handled = false }));

                //TempNodeTemplate.HandleFocusChangedEventAsync(ParentWindow, new UIFocusChangedEvent { Focused = hasFocus, ForcePropagation = false, Handled = false });
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
                DeselectCurrentNode(treeNode);

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
                var treeNode = nodeTemplate.DataContext as ITreeNode;

                DeselectCurrentNode(treeNode);

                focusedNodeHash = nodeTemplate.DataContext.GetHashCode();

                selectedNode = treeNode;
                selectedNode.IsSelected = true;

                await ParentWindow.SetFocusAsync(nodeTemplate);

                await HandleSelectedNodeChangedAsync(ParentWindow, UIEvent.Empty, selectedNode);

                nodeTemplate.StateHasChanged();
            }
        }

        // Clears IsSelected on whatever was selected before - mirrors the de-select step
        // GetNodeTemplate's mouse-driven OnFocusChangedAsync already does. Without this, moving
        // selection programmatically (keyboard nav - see MoveSelectionAsync/MoveToParentAsync/
        // MoveToFirstChildAsync) left the previously-focused node's IsSelected stuck true, since
        // only the newly focused node's blur/focus roundtrip was reconciled, not the old one's -
        // the old node's own blur event fires *after* focusedNodeHash has already been
        // overwritten to the new node's hash below, so its OnFocusChangedAsync's own hash check
        // never matches and never runs its de-select branch.
        private void DeselectCurrentNode(ITreeNode newlySelectedNode)
        {
            if (selectedNode == null || selectedNode == newlySelectedNode)
            {
                return;
            }

            selectedNode.IsSelected = false;

            UIComponent oldChildNode = FindChildByHash(selectedNode.GetHashCode());
            oldChildNode?.StateHasChanged();
        }

        // ---=== Keyboard navigation ===---

        // There's no cached flat/visible-order list to walk (rendering is done via recursive,
        // virtualized Measure/Arrange calls - see MeasureNode/ArrangeNode) and ITreeNode doesn't
        // track its own parent, so keyboard nav (Up/Down/Left-to-parent) needs its own
        // depth-first walk that mirrors the same "collapsed = collapsed || !node.IsExpanded"
        // skip-logic already used there, capturing each node's parent along the way.
        private List<(ITreeNode Node, ITreeNode Parent, int Depth)> GetVisibleNodes()
        {
            var result = new List<(ITreeNode, ITreeNode, int)>();

            if (RootNode == null)
            {
                return result;
            }

            void Walk(ITreeNode node, ITreeNode parent, int depth, bool isRoot)
            {
                bool skip = isRoot && !ShowRootNode;

                if (!skip)
                {
                    result.Add((node, parent, depth));
                }

                // A hidden root's children are always walked regardless of the root's own
                // IsExpanded - matches MeasureNode/ArrangeNode, where the "collapsed" flag
                // computed from node.IsExpanded is only applied when the node itself was
                // actually rendered (!skipNode).
                if (node.Children != null && (skip || node.IsExpanded))
                {
                    foreach (var child in node.Children)
                    {
                        Walk(child, node, skip ? 0 : depth + 1, false);
                    }
                }
            }

            Walk(RootNode, null, 0, true);

            return result;
        }

        // Focuses/selects a node (creating its template on demand via SetFocusAsync/
        // GetNodeTemplate if it isn't already a real child) and scrolls it into view -
        // the shared tail end of every keyboard-nav operation below.
        private async Task FocusVisibleNodeAsync(ITreeNode node)
        {
            if (node == null)
            {
                return;
            }

            await SetFocusAsync(node);

            var visible = GetVisibleNodes();
            int index = visible.FindIndex(n => Equals(n.Node, node));
            if (index >= 0)
            {
                ScrollNodeIntoView(index);
            }
        }

        // Up (direction -1) / Down (direction +1) - moves to the previous/next node in visible
        // document order, without crossing into a collapsed subtree.
        public async Task MoveSelectionAsync(ITreeNode currentNode, int direction)
        {
            var visible = GetVisibleNodes();
            int index = visible.FindIndex(n => Equals(n.Node, currentNode));
            if (index < 0)
            {
                return;
            }

            int newIndex = Math.Clamp(index + direction, 0, visible.Count - 1);
            if (newIndex != index)
            {
                await FocusVisibleNodeAsync(visible[newIndex].Node);
            }
        }

        // Left, when the current node is already collapsed (or has no children) - moves up to
        // the owning parent, matching common tree-view UX (e.g. Windows Explorer).
        public async Task MoveToParentAsync(ITreeNode currentNode)
        {
            var visible = GetVisibleNodes();
            var entry = visible.FirstOrDefault(n => Equals(n.Node, currentNode));
            if (entry.Parent != null)
            {
                await FocusVisibleNodeAsync(entry.Parent);
            }
        }

        // Right, when the current node is already expanded - moves down into its first child.
        public async Task MoveToFirstChildAsync(ITreeNode currentNode)
        {
            if (currentNode?.Children != null && currentNode.Children.Count > 0)
            {
                await FocusVisibleNodeAsync(currentNode.Children[0]);
            }
        }

        // Items are virtualized, so a keyboard-focused node may not be a real child yet -
        // estimate its position from the last-measured node height (nodes are assumed uniform
        // height) rather than relying on a FinalRect that may not exist for it this frame -
        // mirrors ListView.ScrollItemIntoView.
        private void ScrollNodeIntoView(int index)
        {
            if (_cachedNodeHeight is not float nodeHeight || nodeHeight <= 0)
            {
                return;
            }

            int itemTop = (int)(index * nodeHeight);
            int itemBottom = itemTop + (int)nodeHeight;

            int viewTop = VerticalScrollBar.ScrollOffset;
            int viewBottom = viewTop + FinalContentRect.Height;

            if (itemTop < viewTop)
            {
                VerticalScrollBar.ScrollOffset = Math.Max(0, Math.Min(itemTop, VerticalScrollBar.MaxValue));
            }
            else if (itemBottom > viewBottom)
            {
                VerticalScrollBar.ScrollOffset = Math.Max(0, Math.Min(itemBottom - FinalContentRect.Height, VerticalScrollBar.MaxValue));
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
