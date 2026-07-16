using Blade.MG.UI.Caching;
using Blade.MG.UI.Components;
using Blade.MG.UI.Events;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Blade.MG.UI
{

    public abstract class UIComponent : IDisposable
    {
        [JsonIgnore]
        private UIWindow parentWindow;

        [JsonIgnore]
        public UIWindow ParentWindow { get { return parentWindow; } private set { SetParentWindow(value); } }

        [JsonIgnore]
        protected ContentManager ContentManager => ParentWindow?.ContentManager;

        [JsonIgnore]
        protected GraphicsDevice GraphicsDevice => ParentWindow?.Context?.GraphicsDevice;

        [JsonIgnore]
        protected UIContext Context => ParentWindow?.Context;

        /// <summary>
        /// TODO: Find a better name for these 'internal' components
        /// </summary>
        [JsonIgnore]
        protected IReadOnlyList<UIComponent> InternalChildren => internalChildren.AsReadOnly();
        private List<UIComponent> internalChildren = new List<UIComponent>();


        private object dataContext;

        [JsonIgnore]
        public object DataContext { get { return dataContext; } set { dataContext = value; } }
        //public object DataContext { get { return dataContext ?? parent?.DataContext; } set { dataContext = value; } }


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

                // Only build the visual template once we actually have a live ParentWindow to
                // build it against (ContentManager/GraphicsDevice/Context all flow from
                // ParentWindow) - if this component was attached to a not-yet-windowed parent
                // (e.g. while UIDocumentSerializer.LoadNode builds a tree bottom-up, before the
                // root is rooted into a live UIWindow), defer to SetParentWindow, which
                // re-checks this once the real ParentWindow is backfilled.
                if (!TemplateInitialised && ParentWindow != null)
                {
                    InitTemplate();
                    TemplateInitialised = true;
                }

                StateHasChanged();
            }
        }

        public T ParentAs<T>() where T : UIComponent => Parent as T;

        public static int LastTabOrder = 0;

        // Pure runtime state (has InitTemplate run yet?), never document data - a freshly
        // deserialized component must always start false so its own InitTemplate (which builds
        // its internal visual template: border/label/chrome, everything that actually draws)
        // still runs when it's attached to a parent. Without [JsonIgnore], saving a design after
        // it's been laid out at least once bakes "TemplateInitialised": true into every control's
        // JSON; reloading that file then force-sets true on each freshly-constructed component
        // before it's ever parented, permanently skipping InitTemplate for the whole tree - the
        // symptom is layout/FinalRect being entirely correct while nothing ever renders.
        [JsonIgnore]
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
        //public Binding<HorizontalAlignmentType> HorizontalContentAlignment { get; set; }
        //public Binding<VerticalAlignmentType> VerticalContentAlignment { get; set; }

        public int TabIndex { get; set; } = ++LastTabOrder;

        public Binding<bool> IsTabStop { get; set; } = false;
        public Binding<bool> IsEnabled { get; set; } = true;
        public Binding<Visibility> Visible { get; set; } = Visibility.Visible;

        public bool IsHitTestVisible { get; set; } = false;
        public bool CanHover { get; set; } = true;
        public bool CanFocus { get; set; } = true;  // Same meaning as IsHitTestVisible ??

        public Binding<bool> HasFocus { get; set; } = false;
        public Binding<bool> MouseHover { get; set; } = false;


        public Transform Transform { get; set; } = new Transform();


        public string ViewState
        {
            get
            {
                if (HasFocus) return ViewStates.Focused;
                if (MouseHover) return ViewStates.Hover;
                return "";
            }
        }

        protected void SetField<T>(ref Binding<T> field, T value)
        {
            if (field == null)
            {
                field = new Binding<T>();
            }

            field.Value = value;
        }

        protected void SetField<T>(ref Binding<T> field, Binding<T> value)
        {
            if (field == null)
            {
                field = new Binding<T>();
            }

            if (value.IsImplicitCast)
            {
                field.SetFromBinding(value);
            }
            else
            {
                // Replacing the field's Binding<T> instance wholesale (rather than mutating
                // its Value) bypasses whatever Changed subscription EnsureBindingsWired set up
                // on the old instance - re-wire it onto the new one so cache-invalidation
                // bubbling (see BubbleInvalidation) keeps working after a rebind.
                if (bindingsWired)
                {
                    field.Changed -= OnOwnBindingChanged;
                    value.Changed += OnOwnBindingChanged;
                }

                field = value;
            }
        }

        //protected void SetField<T>(ref Binding<T> field, DynamicBinding<Binding<T>> value)
        //{
        //    if (field == null)
        //    {
        //        field = new Binding<T>();
        //    }

        //    //field.SetFromBinding(value);
        //    field = value.Binding;
        //}

        //----

        public virtual void AddInternalChild(UIComponent item, UIComponent parent = null, object dataContext = null)
        {
            // See the matching comment on Container.AddChild - same "don't clobber an
            // already-set DataContext" reasoning applies here.
            item.DataContext = dataContext ?? item.DataContext ?? DataContext ?? parent?.DataContext;

            item.Parent = parent ?? this;
            internalChildren.Add(item);
            BubbleInvalidation();
        }

        public bool RemoveInternalChild(UIComponent item)
        {
            if (!internalChildren.Remove(item))
            {
                return false;
            }

            item.Parent = null;
            BubbleInvalidation();
            return true;
        }

        public void RemoveAllInternalChildren()
        {
            foreach (var child in internalChildren)
            {
                child.Parent = null;
            }

            internalChildren.Clear();
            BubbleInvalidation();
        }

        public int IndexOfInternalChild(UIComponent item)
        {
            return internalChildren.IndexOf(item);
        }

        //----


        protected internal Size DesiredSize { get; set; }
        protected internal Rectangle FinalRect;        // Final Layout Rectangle for Control, including Margin
        protected internal Rectangle FinalContentRect; // Final Layout Rectangle for Control's Content, including Padding
        //protected internal Rectangle visibleRect;     // Final Layout Rectangle for Control clipped to parent control's layout bounds

        // Cascaded (self + all ancestors) render transform, recomputed once per layout pass in
        // ArrangeSelf - the single choke point every Arrange path already funnels through - so
        // it's always in sync with FinalRect and available to hit-testing without redoing any
        // matrix work per input event. Kept separate from the Transform.Combine(...) chain the
        // render pass threads through RenderControl calls: duplicating a couple of cheap matrix
        // multiplies (only for the rare transformed subtree) is far cheaper and lower-risk than
        // rewiring every RenderControl override to consume a shared cached value.
        public bool HasEffectiveTransform { get; private set; } = false;
        public Matrix EffectiveTransform { get; private set; } = Matrix.Identity;
        private Matrix effectiveTransformInverse = Matrix.Identity;

        // World-space axis-aligned bounding box of FinalRect after EffectiveTransform is
        // applied (equals FinalRect when untransformed). Used as a cheap reject before the
        // precise per-point hit test, and available to callers that need a transformed
        // control's true on-screen extents (e.g. invalidation, custom clip regions).
        public Rectangle EffectiveExtents { get; private set; }

        internal float ActualWidth { get; set; }
        internal float ActualHeight { get; set; }
        internal float Left { get; set; }
        internal float Top { get; set; }

        internal bool IsWidthVirtual { get; set; }
        internal bool IsHeightVirtual { get; set; }


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

            //HorizontalContentAlignment = HorizontalAlignmentType.Stretch;
            //VerticalContentAlignment = VerticalAlignmentType.Stretch;

            ActualWidth = 0;
            ActualHeight = 0;

            IsWidthVirtual = false;
            IsHeightVirtual = false;
        }

        // TemplateInitialised itself is set by the Parent setter after calling this, not here -
        // see that comment for why. Overrides are free to skip calling base.InitTemplate()
        // entirely (there's nothing left in it to lose) without needing to remember any
        // workaround.
        protected virtual void InitTemplate()
        {
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
            TemplateInitialised = true;
        }

        public void StateHasChanged()
        {
            HandleStateChange();
        }

        protected virtual void HandleStateChange()
        {

        }

        // ---=== Render-cache invalidation bubbling ===---
        //
        // CacheStateHash (see UIComponentDrawable/ICacheable) only covers a control's own
        // Binding<T> properties, so a cached ancestor (e.g. a Border with EnableCaching) has
        // no way to notice a change happening inside its Content/Children. To fix that,
        // every control auto-subscribes to its own bindings' Changed events (wired lazily,
        // once per instance, in EnsureBindingsWired) and on any change walks up the live
        // Parent chain calling InvalidateCache() on every ICacheable ancestor - not just the
        // nearest one, since caches can nest.

        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> bindingPropertiesByType = new();
        private bool bindingsWired;

        private static PropertyInfo[] GetBindingProperties(Type type)
        {
            return bindingPropertiesByType.GetOrAdd(type, t =>
                t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                 .Where(p => typeof(IBinding).IsAssignableFrom(p.PropertyType) && p.GetIndexParameters().Length == 0 && p.CanRead)
                 .ToArray());
        }

        /// <summary>
        /// Lazily subscribes to the Changed event of every IBinding-typed property this
        /// control has. Safe to call every frame (guarded by bindingsWired) - called from
        /// Measure rather than the constructor, since derived-class field initializers for
        /// Binding&lt;T&gt; properties (e.g. Border.BorderColor) haven't run yet when the base
        /// UIComponent() constructor executes.
        /// </summary>
        private void EnsureBindingsWired()
        {
            if (bindingsWired) return;
            bindingsWired = true;

            foreach (var prop in GetBindingProperties(GetType()))
            {
                if (prop.GetValue(this) is IBinding binding)
                {
                    binding.Changed += OnOwnBindingChanged;
                }
            }
        }

        private void OnOwnBindingChanged()
        {
            BubbleInvalidation();
        }

        /// <summary>
        /// Invalidates this control's render cache (if it has one) and every ancestor's, all
        /// the way to the root - so a change to a deeply nested control correctly invalidates
        /// every cached container above it, not just the nearest one.
        /// </summary>
        protected internal void BubbleInvalidation()
        {
            if (this is ICacheable cacheable)
            {
                cacheable.InvalidateCache();
            }

            Parent?.BubbleInvalidation();
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
            EnsureBindingsWired();

            MeasureSelf(context, ref availableSize, ref parentMinMax);

            // Measure the Internal Components
            MergeChildDesiredSize(context, ref availableSize, InternalChildren, ref parentMinMax);

        }

        protected void MeasureSelf(UIContext context, ref Size availableSize, ref Layout parentMinMax)
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

                UpdateEffectiveTransform();

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


            float desiredWidth = DesiredSize.Width - Margin.Value.Horizontal;
            float desiredHeight = DesiredSize.Height - Margin.Value.Vertical;

            ActualWidth = desiredWidth;
            ActualHeight = desiredHeight;

            if (float.IsNaN(ActualWidth))
            {
                ActualWidth = 0;
            }

            if (float.IsNaN(ActualHeight))
            {
                ActualHeight = 0;
            }

            if (!FloatHelper.IsNaN(Width))
            {
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


            // Dont allow Stretch if the Width is virtualized i.e. We don't have a defined width
            if (HorizontalAlignment.Value == HorizontalAlignmentType.Stretch && FloatHelper.IsNaN(Width))
            {
                bool isParentWidthVirtual = parent?.IsWidthVirtual ?? false;

                if (!isParentWidthVirtual || ActualWidth < layoutBounds.Width)
                {
                    Left = layoutBounds.Left;
                    ActualWidth = layoutBounds.Width;
                }

            }

            // Dont allow Stretch if the Height is virtualized i.e. We don't have a defined height
            if (VerticalAlignment.Value == VerticalAlignmentType.Stretch && FloatHelper.IsNaN(Height))
            {
                bool isParentHeightVirtual = parent?.IsHeightVirtual ?? false;

                if (!isParentHeightVirtual || ActualHeight < layoutBounds.Height)
                {
                    Top = layoutBounds.Top;
                    ActualHeight = layoutBounds.Height;
                }

            }


            if (ActualWidth < MinWidth.ToPixels(parentLayoutBounds.Width)) ActualWidth = MinWidth.ToPixels(parentLayoutBounds.Width);
            if (ActualWidth > MaxWidth.ToPixels(parentLayoutBounds.Width)) ActualWidth = MaxWidth.ToPixels(parentLayoutBounds.Width);
            if (ActualHeight < MinHeight.ToPixels(parentLayoutBounds.Height)) ActualHeight = MinHeight.ToPixels(parentLayoutBounds.Height);
            if (ActualHeight > MaxHeight.ToPixels(parentLayoutBounds.Height)) ActualHeight = MaxHeight.ToPixels(parentLayoutBounds.Height);

            //--------------

            // Every branch below resolves to the same Left/Top when Actual == layoutBounds
            // (the (layoutBounds.Size - Actual) term is zero), so it's always safe to run this
            // unconditionally. Gating it on ActualWidth/Height != layoutBounds.Width/Height (as
            // this used to) skips it whenever a control has an explicit fixed Width/Height that
            // happens to exactly match its available bounds (e.g. Stretch + a fixed size sized
            // to fit by an auto-sized parent) - since the earlier "virtualized stretch" fast
            // path above only fires for Width/Height == NaN, that combination left Left/Top
            // completely unset for this pass, rendering at whatever stale position they last
            // held.
            switch (HorizontalAlignment.Value)
            {
                case HorizontalAlignmentType.Left: Left = layoutBounds.Left; break;
                case HorizontalAlignmentType.Right: Left = layoutBounds.Left + layoutBounds.Width - ActualWidth; break;
                case HorizontalAlignmentType.Center: Left = layoutBounds.Left + (layoutBounds.Width - ActualWidth) / 2; break;
                case HorizontalAlignmentType.Stretch: Left = layoutBounds.Left + (layoutBounds.Width - ActualWidth) / 2; break;  // Handle Stretch like Center if we have a Width Constraint
            }

            switch (VerticalAlignment.Value)
            {
                case VerticalAlignmentType.Top: Top = layoutBounds.Top; break;
                case VerticalAlignmentType.Bottom: Top = layoutBounds.Top + layoutBounds.Height - ActualHeight; break;
                case VerticalAlignmentType.Center: Top = layoutBounds.Top + (layoutBounds.Height - ActualHeight) / 2; break;
                case VerticalAlignmentType.Stretch: Top = layoutBounds.Top + (layoutBounds.Height - ActualHeight) / 2; break; // Handle Stretch like Center if we have a Height Constraint
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

            UpdateEffectiveTransform();

            // Arrange the Internal Components
            foreach (var child in internalChildren)
            {
                child?.Arrange(context, FinalContentRect, layoutBounds);
            }

        }

        /// <summary>
        /// Recomputes the cascaded (self + ancestors) render transform, its inverse, and the
        /// resulting world-space extents. Must run after FinalRect/CalcCenterPoint inputs are
        /// current for this node, and after the parent's own ArrangeSelf has already run (which
        /// is always true here, since Arrange always visits parents before children).
        /// </summary>
        private void UpdateEffectiveTransform()
        {
            bool parentHasTransform = parent?.HasEffectiveTransform ?? false;
            HasEffectiveTransform = parentHasTransform || !Transform.IsIdentity;

            if (!HasEffectiveTransform)
            {
                // Fast path: no rotation/scale/translation anywhere from this node up to the
                // root, so skip all matrix math - hit-testing and extents just fall back to
                // FinalRect directly, exactly like before this feature existed.
                EffectiveTransform = Matrix.Identity;
                effectiveTransformInverse = Matrix.Identity;
                EffectiveExtents = FinalRect;
                return;
            }

            var t = Transform with { ParentMatrix = parent?.EffectiveTransform ?? Matrix.Identity };
            t.CalcCenterPoint(this);
            EffectiveTransform = t.GetMatrix();

            // Guard against degenerate transforms (e.g. a zero Scale component) where the
            // matrix has no inverse - such a control has no visible area, so it should simply
            // never be hit-testable rather than risk NaN/Infinity propagating through.
            if (Math.Abs(EffectiveTransform.Determinant()) < 1e-6f)
            {
                effectiveTransformInverse = Matrix.Identity;
                EffectiveExtents = Rectangle.Empty;
                return;
            }

            effectiveTransformInverse = Matrix.Invert(EffectiveTransform);
            EffectiveExtents = CalculateTransformedExtents(FinalRect, EffectiveTransform);
        }

        private static Rectangle CalculateTransformedExtents(Rectangle rect, Matrix matrix)
        {
            Vector2 p0 = Vector2.Transform(new Vector2(rect.Left, rect.Top), matrix);
            Vector2 p1 = Vector2.Transform(new Vector2(rect.Right, rect.Top), matrix);
            Vector2 p2 = Vector2.Transform(new Vector2(rect.Right, rect.Bottom), matrix);
            Vector2 p3 = Vector2.Transform(new Vector2(rect.Left, rect.Bottom), matrix);

            float minX = MathF.Min(MathF.Min(p0.X, p1.X), MathF.Min(p2.X, p3.X));
            float maxX = MathF.Max(MathF.Max(p0.X, p1.X), MathF.Max(p2.X, p3.X));
            float minY = MathF.Min(MathF.Min(p0.Y, p1.Y), MathF.Min(p2.Y, p3.Y));
            float maxY = MathF.Max(MathF.Max(p0.Y, p1.Y), MathF.Max(p2.Y, p3.Y));

            return new Rectangle((int)MathF.Floor(minX), (int)MathF.Floor(minY), (int)MathF.Ceiling(maxX - minX), (int)MathF.Ceiling(maxY - minY));
        }

        /// <summary>
        /// Hit-tests a screen-space point against this control, accounting for any
        /// rotation/scale/translation applied to it or any of its ancestors. Untransformed
        /// controls (the common case) take the exact same fast path as a plain
        /// FinalRect.Contains check. Transformed controls get a cheap AABB reject against
        /// EffectiveExtents before the precise test, which transforms the point into the
        /// control's local (pre-transform) space via the cached inverse matrix and checks it
        /// against the untransformed FinalRect - equivalent to a rotated point-in-rect test,
        /// without needing to build/test a quad every call.
        /// </summary>
        public bool ContainsScreenPoint(Point point)
        {
            if (!HasEffectiveTransform)
            {
                return FinalRect.Contains(point);
            }

            if (!EffectiveExtents.Contains(point))
            {
                return false;
            }

            Vector2 local = Vector2.Transform(new Vector2(point.X, point.Y), effectiveTransformInverse);

            return FinalRect.Contains((int)MathF.Round(local.X), (int)MathF.Round(local.Y));
        }

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

            if (Visible.Value == Visibility.Collapsed)
            {
                DesiredSize = new Size(0, 0);
                return;
            }

            foreach (var child in internalChildren)
            {
                MergeChildDesiredSizeInternal(context, ref availableSize, child, ref desiredWidth, ref desiredHeight, ref parentMinMax);
            }

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
                    // A collapsed child contributes nothing to the accumulated desired size,
                    // but must not wipe out the max already accumulated from prior siblings.
                    return;
                }

                //if (IsWidthVirtual || IsHeightVirtual)
                //{
                //    var virtualAvailableSize = new Size(IsWidthVirtual ? float.NaN : availableSize.Width, IsHeightVirtual ? float.NaN : availableSize.Height);
                //    child.Measure(context, ref virtualAvailableSize, ref parentMinMax);
                //}
                //else
                //{
                child.Measure(context, ref availableSize, ref parentMinMax);
                //}

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
            if (!FloatHelper.IsNaN(MaxWidth) && (dw > MaxWidth.ToPixels(availableSize.Width) || float.IsNaN(dw)))
            {
                dw = MaxWidth.ToPixels(availableSize.Width);
            }
            if (!float.IsNaN(maxW) && dw > maxW)
            {
                dw = maxW;
            }

            if (!FloatHelper.IsNaN(MinWidth) && (dw < MinWidth.ToPixels(availableSize.Width) || float.IsNaN(dw)))
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
                foreach (var control in internalChildren)
                {
                    yield return control;
                }
            }
        }

        private void SetParentWindow(UIWindow newParentWindow)
        {
            parentWindow = newParentWindow;

            // A component may have had its Parent assigned before a live ParentWindow existed
            // (e.g. while UIDocumentSerializer.LoadNode builds a tree bottom-up) - in that case
            // the Parent setter deferred InitTemplate. Now that a real window is finally
            // available, run it.
            if (!TemplateInitialised && parentWindow != null)
            {
                InitTemplate();
                TemplateInitialised = true;
            }

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
            }
            ;
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

        // ----------

        /// <summary>
        /// Shared implementation behind the position-filtered event handlers below (mouse
        /// down/up/click/double-click/right-click/wheel-scroll, tap, long-press, primary/
        /// secondary/multi-click). Only dispatches if the event falls within this control's
        /// FinalRect (or the event has ForcePropagation set, used e.g. to let a MouseUp
        /// reach a control it was dragging even if the mouse has since left its bounds),
        /// then propagates to children under the same bounds check.
        /// </summary>
        protected async Task DispatchPositionedEventAsync<TEvent>(UIWindow uiWindow, TEvent uiEvent, Func<UIComponent, UIWindow, TEvent, Task> handler)
            where TEvent : UIEvent, IPositionedEvent
        {
            if (uiEvent.ForcePropagation || ContainsScreenPoint(new Point(uiEvent.X, uiEvent.Y)))
            {
                await PropagateAsync(uiEvent, uiWindow, async (component) =>
                {
                    if (uiEvent.ForcePropagation || (component.ContainsScreenPoint(new Point(uiEvent.X, uiEvent.Y)) && component.Visible.Value == Visibility.Visible))
                    {
                        await handler(component, uiWindow, uiEvent);
                    }
                });
            }
        }

        /// <summary>
        /// Shared implementation behind the handlers that propagate unconditionally to every
        /// descendant regardless of position (mouse move, key down/up/press).
        /// </summary>
        protected async Task DispatchEventAsync<TEvent>(UIWindow uiWindow, TEvent uiEvent, Func<UIComponent, UIWindow, TEvent, Task> handler)
            where TEvent : UIEvent
        {
            await PropagateAsync(uiEvent, uiWindow, async (component) => await handler(component, uiWindow, uiEvent));
        }

        public virtual Task HandleTapEventAsync(UIWindow uiWindow, UIClickEvent uiEvent) =>
            DispatchPositionedEventAsync(uiWindow, uiEvent, (component, w, e) => component.HandleTapEventAsync(w, e));

        public virtual Task HandleLongPressEventAsync(UIWindow uiWindow, UIClickEvent uiEvent) =>
            DispatchPositionedEventAsync(uiWindow, uiEvent, (component, w, e) => component.HandleLongPressEventAsync(w, e));

        public virtual Task HandleMouseMoveEventAsync(UIWindow uiWindow, UIMouseMoveEvent uiEvent) =>
            DispatchEventAsync(uiWindow, uiEvent, (component, w, e) => component.HandleMouseMoveEventAsync(w, e));

        public virtual Task HandleMouseDownEventAsync(UIWindow uiWindow, UIMouseDownEvent uiEvent) =>
            DispatchPositionedEventAsync(uiWindow, uiEvent, (component, w, e) => component.HandleMouseDownEventAsync(w, e));

        public virtual Task HandleMouseUpEventAsync(UIWindow uiWindow, UIMouseUpEvent uiEvent) =>
            DispatchPositionedEventAsync(uiWindow, uiEvent, (component, w, e) => component.HandleMouseUpEventAsync(w, e));

        public virtual Task HandleMouseWheelScrollEventAsync(UIWindow uiWindow, UIMouseWheelScrollEvent uiEvent) =>
            DispatchPositionedEventAsync(uiWindow, uiEvent, (component, w, e) => component.HandleMouseWheelScrollEventAsync(w, e));

        public virtual Task HandleMouseClickEventAsync(UIWindow uiWindow, UIClickEvent uiEvent) =>
            DispatchPositionedEventAsync(uiWindow, uiEvent, (component, w, e) => component.HandleMouseClickEventAsync(w, e));

        public virtual Task HandleMouseDoubleClickEventAsync(UIWindow uiWindow, UIClickEvent uiEvent) =>
            DispatchPositionedEventAsync(uiWindow, uiEvent, (component, w, e) => component.HandleMouseDoubleClickEventAsync(w, e));

        public virtual Task HandleMouseRightClickEventAsync(UIWindow uiWindow, UIClickEvent uiEvent) =>
            DispatchPositionedEventAsync(uiWindow, uiEvent, (component, w, e) => component.HandleMouseRightClickEventAsync(w, e));

        public virtual Task HandleKeyDownAsync(UIWindow uiWindow, UIKeyEvent uiEvent) =>
            DispatchEventAsync(uiWindow, uiEvent, (component, w, e) => component.HandleKeyDownAsync(w, e));

        public virtual Task HandleKeyUpAsync(UIWindow uiWindow, UIKeyEvent uiEvent) =>
            DispatchEventAsync(uiWindow, uiEvent, (component, w, e) => component.HandleKeyUpAsync(w, e));

        public virtual Task HandleKeyPressAsync(UIWindow uiWindow, UIKeyEvent uiEvent) =>
            DispatchEventAsync(uiWindow, uiEvent, (component, w, e) => component.HandleKeyPressAsync(w, e));

        public virtual async Task HandleFocusChangedEventAsync(UIWindow uiWindow, UIFocusChangedEvent uiEvent)
        {
            await PropagateAsync(uiEvent, uiWindow, async (component) => { await component?.HandleFocusChangedEventAsync(uiWindow, uiEvent); });

            HasFocus = uiEvent.Focused;
        }

        public virtual async Task HandleHoverChangedAsync(UIWindow uiWindow, UIHoverChangedEvent uiEvent)
        {
            bool outerGate = uiEvent.ForcePropagation || ContainsScreenPoint(new Point(uiEvent.X, uiEvent.Y));

            if (outerGate)
            {
                await PropagateAsync(uiEvent, uiWindow, async (component) =>
                {
                    if (component != null)
                    {
                        bool innerGate = uiEvent.ForcePropagation || component.ContainsScreenPoint(new Point(uiEvent.X, uiEvent.Y));

                        // Always propogate event if Hover = False as we've aleady moved off that control
                        if (innerGate)
                        {
                            await component.HandleHoverChangedAsync(uiWindow, uiEvent);
                            MouseHover = uiEvent.Hover;
                        }
                    }
                });

                if (!uiEvent.Handled && CanHover)
                {
                    MouseHover = uiEvent.Hover;
                }
            }

        }

        public virtual void Dispose()
        {
        }

        public override string ToString()
        {
            return $"Name={Name} : " + base.ToString();
        }

    }
}
