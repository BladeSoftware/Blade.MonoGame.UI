using Blade.MG.UI.Components;
using Blade.MG.UI.Controls.Templates;
using Blade.MG.UI.Events;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Blade.MG.UI.Controls
{
    public class ListView : StackPanel
    {
        private static int frameID = 0;

        // Callback when selection changes
        public Action<object> OnSelectionChanged { get; set; }

        // When false, arrow-key navigation only moves HighlightedItem (a pending, not-yet-
        // committed highlight) instead of SelectedItem, and only Enter/Space (or a mouse
        // click) promotes it to SelectedItem/fires OnSelectionChanged. Defaults to true -
        // unchanged, immediate-commit-on-arrow behavior for standalone ListView usage.
        // ComboBox sets this to false on its own dropdown ListView instance, where
        // OnSelectionChanged is wired to close the popup - without this, forwarding arrow
        // keys to the popup would close it on the very first arrow press.
        public bool CommitSelectionImmediately { get; set; } = true;
        public object HighlightedItem { get; private set; }

        [JsonIgnore]
        [XmlIgnore]
        public Type ItemTemplateType { get; set; } = typeof(ListViewItemTemplate);

        private UIComponent TempNodeTemplate = null;

        // Estimated row height/count from the last Arrange pass (average, in case item
        // templates vary in height) - used to scroll a keyboard-selected item into view even
        // though virtualization means it may not currently be a real child.
        private int listItemCount = 0;
        private float listItemHeight = 0f;

        protected override void InitTemplate()
        {
            base.InitTemplate();

            IsHitTestVisible = true;

            // Panel (an ancestor) defaults CanFocus to false. A ListView needs to be
            // focusable itself so HandleKeyPressAsync below can gate keyboard navigation on
            // HasFocus - list items opt out of focus (see ListViewItemTemplate) so this is
            // always the component that ends up focused when the user clicks inside the list.
            CanFocus = true;
        }

        public override void Measure(UIContext context, ref Size availableSize, ref Layout parentMinMax)
        {
            base.Measure(context, ref availableSize, ref parentMinMax);

            // -- Measure Children ---
            float desiredWidth = 0f;
            float desiredHeight = 0f;

            var desiredize = DesiredSize;

            //foreach (var child in CollectionsMarshal.AsSpan(Children))
            foreach (var child in Children)
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

            nodeBounds = nodeBounds with { X = nodeBounds.X - HorizontalScrollOffset,  Y = nodeBounds.Y - VerticalScrollOffset };

            IEnumerable<object> listItems = DataContext as IEnumerable<object>;

            if (listItems != null)
            {
                foreach (var item in listItems)
                {
                    ArrangeItem(context, ref availableSize, ref parentMinMax, ref layoutBounds, ref nodeBounds, item, ref desiredWidth, ref desiredHeight);
                }
            }


            // Remove stale children. Children is a ReadOnlyCollection wrapper (see
            // Container.Children) - casting it to IList<UIComponent> and calling RemoveAt (as
            // this used to) always throws NotSupportedException, since ReadOnlyCollection's
            // IList<T>.RemoveAt is a stub that just throws. That exception fired mid-Arrange,
            // after this frame's surviving/new items were already added below via AddChild but
            // before stale ones got pruned - the visible symptom was old items never clearing
            // when a filter shrank the list, since the removal that should have deleted them
            // never actually completed. RemoveChild operates on the real backing list instead.
            for (int i = Children.Count - 1; i >= 0; i--)
            {
                var child = Children[i];
                if (child.FrameID != frameID)
                {
                    RemoveChild(child);
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

            // ScrollPanel.Arrange (called via base.Arrange above) computes
            // isHorizontallyScrollable/isVerticallyScrollable from a Union of Children's
            // FinalRect - but virtualization means Children only ever holds the (roughly
            // viewport-sized) set of items currently on screen, so that heuristic always
            // measures a content size that's no bigger than the viewport and never reports
            // "scrollable". Override it here using the true (virtualized) desired size just
            // computed above, which is what actually determines whether there's anything to
            // scroll to.
            isHorizontallyScrollable = w > 0;
            isVerticallyScrollable = h > 0;

            HorizontalScrollBar.Visible = BoolToVisibility(IsHorizontalScrollbarVisible);
            VerticalScrollBar.Visible = BoolToVisibility(IsVerticalScrollbarVisible);

            listItemCount = listItems?.Count() ?? 0;
            listItemHeight = listItemCount > 0 ? ListDesiredSize.Height / listItemCount : 0f;
        }

        private void ArrangeItem(UIContext context, ref Size availableSize, ref Layout parentMinMax, ref Rectangle layoutBounds, ref Rectangle nodeBounds, object itemDataContext, ref float desiredWidth, ref float desiredHeight)
        {
            if (itemDataContext == null)
            {
                return;
            }

            var nodeTemplate = GetNodeTemplate(itemDataContext, out bool isExistingNode);

            nodeTemplate.FrameID = frameID;

            // Re-derive selection state from SelectedItem every pass, rather than relying on
            // SelectedItemTemplate object identity - virtualization means the template
            // instance backing a given data item isn't stable (scrolling an item out and
            // back in can hand it a freshly-created template), so identity-based tracking
            // would silently lose the highlight on recycled items.
            if (nodeTemplate is IItemTemplate itemTemplate)
            {
                // HighlightedItem (set while CommitSelectionImmediately is false, e.g. by
                // ComboBox's dropdown list) shows the same IsSelected visual as a committed
                // SelectedItem while the user is browsing with arrow keys, even though nothing
                // has been committed/fired OnSelectionChanged yet.
                bool shouldBeSelected = Equals(itemDataContext, SelectedItem) || Equals(itemDataContext, HighlightedItem);
                if (itemTemplate.IsSelected.Value != shouldBeSelected)
                {
                    itemTemplate.IsSelected.Value = shouldBeSelected;
                    nodeTemplate.StateHasChanged();
                }
            }

            MeasureOneListItem(context, ref availableSize, ref parentMinMax, ref desiredWidth, ref desiredHeight, nodeTemplate);

            nodeBounds.Height = (int)nodeTemplate.DesiredSize.Height;

            nodeTemplate.Arrange(context, nodeBounds, nodeBounds);

            nodeBounds.Y += nodeBounds.Height;

            //if (Rectangle.Intersect(GetChildBoundingBox(context, nodeTemplate), FinalRect) != Rectangle.Empty)
            if (Rectangle.Intersect(nodeTemplate.FinalRect, FinalRect) != Rectangle.Empty)
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
            return child.FinalContentRect;
            //return child.FinalContentRect with { X = child.FinalContentRect.X - HorizontalScrollBar.ScrollOffset, Y = child.FinalContentRect.Y - VerticalScrollBar.ScrollOffset };
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

        public object SelectedItem
        {
            get
            {
                return field;
            }

            set
            {
                if (field != value)
                {
                    field = value;

                    foreach (var child in Children)
                    {
                        if (child.DataContext == value)
                        {
                            SelectedItemTemplate = child;
                            break;
                        }
                    }

                    // Raise Selected Item Changed
                    OnSelectionChanged?.Invoke(field);
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

        // Override Activate so selection-by-position is reached uniformly regardless of which
        // input device triggered it (mouse click, touch tap, gamepad A, keyboard Enter/Space).
        public override async Task ActivateAsync(UIWindow uiWindow, UIClickEvent uiEvent)
        {
            // If we've clicked on a list item, then update SelectedItem
            if (ContainsScreenPoint(new Point(uiEvent.X, uiEvent.Y)))
            {
                foreach (var child in Children)
                {
                    if (child.ContainsScreenPoint(new Point(uiEvent.X, uiEvent.Y)))
                    {
                        SelectedItem = child?.DataContext;
                        SelectedItemTemplate = child;
                        //SelectedItem = selectedItemTemplate?.DataContext;
                        break;
                    }
                }
            }

            await base.ActivateAsync(uiWindow, uiEvent);
        }

        // Override Key Press handling to support Up/Down/Home/End selection - gated on
        // HasFocus (see InitTemplate, which opts the ListView itself into focus) since key
        // events otherwise propagate unconditionally to every component in the tree.
        public override async Task HandleKeyPressAsync(UIWindow uiWindow, UIKeyEvent uiEvent)
        {
            if (!uiEvent.Handled && HasFocus.Value)
            {
                HandleKey(uiEvent);
            }

            await base.HandleKeyPressAsync(uiWindow, uiEvent);
        }

        private void HandleKey(UIKeyEvent uiEvent)
        {
            var listItems = (DataContext as IEnumerable<object>)?.ToList();
            if (listItems == null || listItems.Count == 0)
            {
                return;
            }

            // Enter/Space commit a pending highlight (see CommitSelectionImmediately). When
            // CommitSelectionImmediately is true there's nothing pending to commit - arrow
            // navigation already committed SelectedItem directly - so this is a no-op as far as
            // selection goes. Either way, mark the key handled: leaving it unhandled here falls
            // through to UIManager.Keyboard.cs's default Enter/Space behavior, which synthesizes
            // a click at the *center point* of this (focused) control's FinalRect - i.e.
            // whichever row happens to be at the middle of the visible viewport - silently
            // overwriting the item actually navigated to/highlighted with whatever's on-screen
            // in the middle of the current page.
            if (uiEvent.Key == Keys.Enter || uiEvent.Key == Keys.Space)
            {
                if (!CommitSelectionImmediately && HighlightedItem != null)
                {
                    SelectedItem = HighlightedItem;
                    HighlightedItem = null;
                }

                uiEvent.Handled = true;
                return;
            }

            object referenceItem = CommitSelectionImmediately ? SelectedItem : (HighlightedItem ?? SelectedItem);
            int currentIndex = referenceItem != null ? listItems.IndexOf(referenceItem) : -1;
            int newIndex;

            switch (uiEvent.Key)
            {
                case Keys.Up:
                    newIndex = currentIndex <= 0 ? 0 : currentIndex - 1;
                    break;

                case Keys.Down:
                    newIndex = currentIndex < 0 ? 0 : Math.Min(currentIndex + 1, listItems.Count - 1);
                    break;

                case Keys.Home:
                    newIndex = 0;
                    break;

                case Keys.End:
                    newIndex = listItems.Count - 1;
                    break;

                default:
                    return;
            }

            uiEvent.Handled = true;

            if (newIndex == currentIndex)
            {
                return;
            }

            if (CommitSelectionImmediately)
            {
                SelectedItem = listItems[newIndex];
            }
            else
            {
                HighlightedItem = listItems[newIndex];
            }

            ScrollItemIntoView(newIndex);
        }

        // Items are virtualized, so a keyboard-selected item may not be a real child yet -
        // estimate its position from the average row height tracked in Arrange rather than
        // relying on a FinalRect that may not exist for it this frame.
        private void ScrollItemIntoView(int index)
        {
            if (listItemCount <= 0 || listItemHeight <= 0)
            {
                return;
            }

            int itemTop = (int)(index * listItemHeight);
            int itemBottom = itemTop + (int)listItemHeight;

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

    }
}
