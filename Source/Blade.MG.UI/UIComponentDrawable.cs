using Blade.MG.UI.Animations;
using Blade.MG.UI.Caching;
using Blade.MG.UI.Components;
using Blade.MG.UI.Controls;
using Blade.MG.UI.Controls.Templates;
using Blade.MG.UI.Theming;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Text.Json.Serialization;

namespace Blade.MG.UI
{
    public abstract class UIComponentDrawable : UIComponentEvents, ICacheable
    {
        [JsonIgnore]
        public string ResourceKey { get => resourceKey ?? this.GetType().Name; set => resourceKey = value; }
        private string resourceKey;

        [JsonIgnore]
        public UITheme Theme => ParentWindow?.Context?.Theme ?? (this as UIWindow)?.Context?.Theme ?? UIManager.DefaultTheme;

        // ---=== Per-control style overrides ===---
        //
        // Controls source color/font/etc. from the active Theme by default, re-applied
        // reactively whenever state (hover/focus/selected) or the theme itself changes (see
        // HandleStateChange / UIManager.SetTheme). An application can override a specific
        // property on a specific instance - e.g. "this one TextBox should always have a red
        // background, regardless of theme or hover state" - the same way a CSS inline style
        // beats a stylesheet rule. Framework/template code must apply themed defaults via
        // ApplyThemedValue (not a direct assignment) for overrides set here to stick.

        private Dictionary<string, object> styleOverrides;

        /// <summary>
        /// Sets an explicit override for a themed property on this control instance, which
        /// then takes precedence over whatever the active theme/state would otherwise apply.
        /// Use nameof() for propertyName so a rename is caught at compile time, e.g.:
        /// <c>myTextBox.SetStyleOverride(nameof(TextBox.Background), Color.Red);</c>
        /// Takes effect immediately.
        /// </summary>
        public void SetStyleOverride<T>(string propertyName, T value)
        {
            styleOverrides ??= new Dictionary<string, object>();
            styleOverrides[propertyName] = value;
            StateHasChanged();
        }

        /// <summary>
        /// Removes a previously-set style override, reverting the property to whatever the
        /// active theme/state provides. Takes effect immediately.
        /// </summary>
        public void ClearStyleOverride(string propertyName)
        {
            if (styleOverrides != null && styleOverrides.Remove(propertyName))
            {
                StateHasChanged();
            }
        }

        public bool HasStyleOverride(string propertyName) => styleOverrides != null && styleOverrides.ContainsKey(propertyName);

        public bool TryGetStyleOverride<T>(string propertyName, out T value)
        {
            if (styleOverrides != null && styleOverrides.TryGetValue(propertyName, out var obj) && obj is T typed)
            {
                value = typed;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Applies a theme/state-driven default to a themed Binding&lt;T&gt; property,
        /// unless <paramref name="owner"/> has an explicit override for
        /// <paramref name="propertyName"/> (see SetStyleOverride), in which case the
        /// override wins. Framework/template code should always go through this instead of
        /// assigning a themed property directly, so an application's override survives
        /// future theme/state changes. <paramref name="owner"/> is usually the control the
        /// application interacts with (e.g. a Button), while <paramref name="target"/> may
        /// belong to an internal child (e.g. the button template's Label).
        /// </summary>
        protected static void ApplyThemedValue<T>(UIComponentDrawable owner, Binding<T> target, string propertyName, T themeValue)
        {
            if (target == null)
            {
                return;
            }

            target.Value = (owner != null && owner.TryGetStyleOverride(propertyName, out T overrideValue)) ? overrideValue : themeValue;
        }

        // Same override-resolution as ApplyThemedValue, but eases toward the resolved value via
        // PropertyAnimationManager instead of snapping instantly - for hover/focus transitions
        // where an instant color/thickness jump reads as cheap. Only Color/float have
        // PropertyAnimationManager.AnimateTo overloads (matching TextBoxTemplate's existing
        // usage), so this doesn't take an arbitrary T like ApplyThemedValue does.
        private static readonly TimeSpan DefaultStateTransitionDuration = TimeSpan.FromMilliseconds(100);

        protected static void ApplyThemedValueAnimated(UIComponentDrawable owner, Binding<Color> target, string propertyName, Color themeValue, TimeSpan? duration = null, Func<float, float> easing = null)
        {
            if (target == null)
            {
                return;
            }

            Color resolved = (owner != null && owner.TryGetStyleOverride(propertyName, out Color overrideValue)) ? overrideValue : themeValue;
            PropertyAnimationManager.AnimateTo(target, resolved, duration ?? DefaultStateTransitionDuration, easing ?? Easing.EaseOutCubic);
        }

        protected static void ApplyThemedValueAnimated(UIComponentDrawable owner, Binding<float> target, string propertyName, float themeValue, TimeSpan? duration = null, Func<float, float> easing = null)
        {
            if (target == null)
            {
                return;
            }

            float resolved = (owner != null && owner.TryGetStyleOverride(propertyName, out float overrideValue)) ? overrideValue : themeValue;
            PropertyAnimationManager.AnimateTo(target, resolved, duration ?? DefaultStateTransitionDuration, easing ?? Easing.EaseOutCubic);
        }

        // See the matching comment on UIComponent's Margin/Padding/Visible/etc. - routing through
        // SetField keeps a reassignment like `control.Background = Color.Red` from silently
        // orphaning whatever Changed subscription EnsureBindingsWired already set up here.
        private Binding<Color> background = Color.Transparent;
        [DesignerProperty]
        public Binding<Color> Background { get => background; set => SetField(ref background, value); }

        [JsonIgnore]
        public Texture2D BackgroundTexture { get; set; }

        [JsonIgnore]
        public TextureLayout BackgroundLayout { get; set; }

        //public T GetResourceValue<T>(string property)
        //{
        //    return GetResourceValue<T>(ResourceKey, property);
        //}

        //public T GetResourceValue<T>(string resource, string property)
        //{
        //    string value = GetResourceValue(resource, property);

        //    Type typeT = typeof(T);

        //    if (typeT == typeof(Color))
        //    {
        //        var color = ((UIColor)Activator.CreateInstance(typeof(UIColor), value)).ToColor();
        //        return (T)Activator.CreateInstance(typeof(Color), color.R, color.G, color.B, color.A);
        //    }

        //    if (typeT == typeof(UIColor) || typeT == typeof(Length) || typeT == typeof(Thickness))
        //    {
        //        return (T)Activator.CreateInstance(typeT, value);
        //    }

        //    try
        //    {
        //        return (T)Convert.ChangeType(value, typeT);
        //    }
        //    catch (Exception ex)
        //    {
        //        return default(T);
        //    }

        //    //if (typeT.IsEquivalentTo(typeof(string)))
        //    //{
        //    //    return (T)Convert.ChangeType("ABC", typeof(string));
        //    //}

        //    //if (typeT.IsEquivalentTo(typeof(float)))
        //    //{
        //    //    return (T)Convert.ChangeType("123.45", typeof(float));
        //    //}

        //    //return default(T);
        //}


        //public string GetResourceValue(string property)
        //{
        //    return GetResourceValue(ResourceKey, property);
        //}

        //public string GetResourceValue(string resource, string property)
        //{
        //    //Debug.WriteLine($"GetResourceValue : {resource}, {property}");

        //    // TODO: Decide on the order of property inheritance
        //    string value;

        //    // First try the local window resource dictionary
        //    var windowResourceDict = ParentWindow?.ResourceDict;
        //    if (windowResourceDict != null)
        //    {
        //        if (windowResourceDict.TryGetValue(resource, property, out value))
        //        {
        //            return value;
        //        }

        //        if (windowResourceDict.TryGetValue(property, out value))
        //        {
        //            return value;
        //        }
        //    }

        //    // Then the global resource dictionary
        //    if (UIManager.ResourceDict != null)
        //    {
        //        if (UIManager.ResourceDict.TryGetValue(resource, property, out value))
        //        {
        //            return value;
        //        }

        //        if (UIManager.ResourceDict.TryGetValue(property, out value))
        //        {
        //            return value;
        //        }
        //    }

        //    return "";
        //}

        // ---=== Off-screen / back-buffer caching (ICacheable) ===---
        //
        // A control opts in to caching (explicitly via EnableCaching, or implicitly via
        // ShouldAutoCache) to have its entire visual output - its own drawing plus any
        // children - rendered once to an off-screen RenderTarget2D and then reused across
        // frames by blitting that texture, instead of re-issuing all of its draw calls
        // every frame. This is a net win for controls whose rendering is comparatively
        // expensive (multiple draw batches, stencil masking, per-pixel loops) but whose
        // visual state changes rarely.
        //
        // Populating the cache requires switching the active render target, which would
        // clobber whatever is currently being drawn (e.g. the back buffer) if done
        // mid-frame during the normal draw pass. So cache updates only happen during a
        // dedicated pre-render pass (see UIWindow.PreRenderLayout / PreRenderContext) that
        // runs before the main draw pass for the frame; UpdateCache saves and restores the
        // previously-active render target(s) around its own SetRenderTarget call so the
        // main pass always starts from a clean, correct target.

        private readonly ControlCache renderCache = new();

        /// <summary>
        /// Explicit opt-in: when true, this control's rendering is cached to an off-screen
        /// texture and reused across frames until its size or CacheStateHash changes (or
        /// InvalidateCache() is called). Best suited to controls with expensive/complex
        /// visuals that change state infrequently (e.g. static panels, list items).
        /// </summary>
        [JsonIgnore]
        public bool EnableCaching { get; set; } = false;

        /// <summary>
        /// Hook for a control type to decide, from its own current state, whether it's
        /// worth caching right now - without requiring the app to opt in manually. Border
        /// uses this to auto-cache only while it's using its expensive rounded-corner /
        /// stencil rendering path. Combines with EnableCaching (either being true enables
        /// caching).
        /// </summary>
        protected virtual bool ShouldAutoCache => false;

        /// <summary>
        /// A cheap hash of whatever state affects this control's rendered pixels (besides
        /// its size, which is tracked separately). The cache is refreshed whenever this
        /// changes. Override to combine in control-specific bindings, e.g.:
        /// <c>protected override int CacheStateHash => HashCode.Combine(base.CacheStateHash, MyColor.Value);</c>
        /// </summary>
        protected virtual int CacheStateHash => Background.Value.GetHashCode();

        /// <summary>
        /// Forces the cache to be refreshed on the next pre-render pass, e.g. after
        /// changing state that CacheStateHash doesn't cover.
        /// </summary>
        public void InvalidateCache() => renderCache.InvalidateCache();

        bool ICacheable.IsCachingEnabled { get => EnableCaching || ShouldAutoCache; set => EnableCaching = value; }
        bool ICacheable.IsCacheInvalid => !renderCache.IsCacheValidFor(CacheStateHash, FinalRect);
        RenderTarget2D ICacheable.CachedTexture => renderCache.CachedTexture;
        Rectangle ICacheable.CachedLayoutBounds => renderCache.CachedLayoutBounds;

        void ICacheable.UpdateCache(UIContext context, Rectangle layoutBounds, Transform parentTransform)
        {
            var graphicsDevice = context?.GraphicsDevice;
            if (graphicsDevice == null || layoutBounds.Width <= 0 || layoutBounds.Height <= 0)
            {
                return;
            }

            int stateHash = CacheStateHash;
            if (renderCache.IsCacheValidFor(stateHash, layoutBounds))
            {
                return;
            }

            renderCache.EnsureRenderTarget(graphicsDevice, layoutBounds.Width, layoutBounds.Height);
            if (renderCache.CachedTexture == null)
            {
                return;
            }

            var savedRenderTargets = graphicsDevice.GetRenderTargets();
            var savedScissor = graphicsDevice.ScissorRectangle;

            // Absolute (screen-space) coordinates need to become texture-local (0,0-based)
            // coordinates: not just for the sprites we draw (handled by the transform
            // passed to RenderControl) but also for scissor-rect clipping, which operates
            // in absolute device coordinates unaffected by that transform. So the
            // control's (and its descendants') FinalRect/FinalContentRect are temporarily
            // shifted to be relative to this cache texture, rendered, then shifted back.
            int dx = -layoutBounds.Left;
            int dy = -layoutBounds.Top;

            try
            {
                graphicsDevice.SetRenderTarget(renderCache.CachedTexture);
                graphicsDevice.ScissorRectangle = new Rectangle(0, 0, layoutBounds.Width, layoutBounds.Height);
                graphicsDevice.Clear(Color.Transparent);

                // Prime the stencil buffer the same way UIWindow.RenderLayout does, since
                // content relying on stencil masking (e.g. Border's rounded corners)
                // expects this baseline to already be in place.
                context.Renderer.ClearStencilBuffer();

                TranslateSubtree(this, dx, dy);
                try
                {
                    var localBounds = new Rectangle(0, 0, layoutBounds.Width, layoutBounds.Height);
                    var localTransform = Transform.Combine(new Transform(), this.Transform, this);

                    // This pass renders into texture-local (0,0-based) coordinates rather than
                    // screen space, so AncestorClipBounds (see UIContext) must be reset to
                    // match - otherwise a cached control's drop shadow would be clipped
                    // against whatever screen-space rect was left over from the main window's
                    // last RenderLayout pass.
                    var savedAncestorClipBounds = context.AncestorClipBounds;
                    context.AncestorClipBounds = localBounds;

                    RenderControl(context, localBounds, localTransform);

                    context.AncestorClipBounds = savedAncestorClipBounds;
                }
                finally
                {
                    TranslateSubtree(this, -dx, -dy);
                }

                renderCache.MarkCacheValid(stateHash, layoutBounds);
            }
            finally
            {
                graphicsDevice.SetRenderTargets(savedRenderTargets);
                graphicsDevice.ScissorRectangle = savedScissor;
            }
        }

        void ICacheable.RenderFromCache(UIContext context, Rectangle layoutBounds, Transform parentTransform)
        {
            var texture = renderCache.CachedTexture;
            if (texture == null || texture.IsDisposed)
            {
                return;
            }

            // layoutBounds here is the parent-provided clip region (same as RenderControl's
            // layoutBounds), not this control's own draw rect - that's FinalRect, exactly as
            // every RenderControl implementation already uses it.
            var spriteBatch = context.Renderer.BeginBatch(transform: parentTransform);
            context.Renderer.ClipToRect(layoutBounds);
            var sourceRect = new Rectangle(0, 0, FinalRect.Width, FinalRect.Height);
            spriteBatch.Draw(texture, FinalRect, sourceRect, Color.White);
            context.Renderer.EndBatch();
        }

        /// <summary>
        /// Recursively shifts FinalRect/FinalContentRect for a control and everything it
        /// draws (internal children, Control.Content, Container.Children) by (dx, dy).
        /// Used to temporarily re-base a subtree's absolute screen coordinates to be
        /// relative to its own cache texture before rendering into it.
        /// </summary>
        private static void TranslateSubtree(UIComponent component, int dx, int dy)
        {
            if (component == null)
            {
                return;
            }

            component.FinalRect = new Rectangle(component.FinalRect.X + dx, component.FinalRect.Y + dy, component.FinalRect.Width, component.FinalRect.Height);
            component.FinalContentRect = new Rectangle(component.FinalContentRect.X + dx, component.FinalContentRect.Y + dy, component.FinalContentRect.Width, component.FinalContentRect.Height);

            foreach (var child in component.PrivateControls)
            {
                TranslateSubtree(child, dx, dy);
            }

            if (component is Control control && control.Content != null)
            {
                TranslateSubtree(control.Content, dx, dy);
            }

            if (component is Container container)
            {
                foreach (var child in container.Children)
                {
                    TranslateSubtree(child, dx, dy);
                }
            }
        }

        /// <summary>
        /// Renders <paramref name="child"/> normally, unless it has caching enabled and an
        /// up-to-date cached texture, in which case that texture is blitted instead of
        /// re-issuing the child's (and its descendants') draw calls.
        /// </summary>
        protected static void RenderChildOrFromCache(UIComponent child, UIContext context, Rectangle layoutBounds, Transform transform)
        {
            if (child == null)
            {
                return;
            }

            if (child is ICacheable cacheable && cacheable.IsCachingEnabled && !cacheable.IsCacheInvalid && cacheable.CachedTexture != null)
            {
                cacheable.RenderFromCache(context, layoutBounds, transform);
                return;
            }

            child.RenderControl(context, layoutBounds, transform);
        }

        public override void Dispose()
        {
            renderCache.Dispose();
            base.Dispose();
        }

        public override void RenderControl(UIContext context, Rectangle layoutBounds, Transform parentTransform)
        {

            base.RenderControl(context, layoutBounds, parentTransform);

            if (Visible.Value != Visibility.Visible)
            {
                return;
            }

            if (Background.Value != Color.Transparent)
            {
                try
                {
                    var spriteBatch = context.Renderer.BeginBatch(transform: parentTransform);
                    context.Renderer.FillRect(spriteBatch, FinalRect, Background.Value, layoutBounds);
                }
                finally
                {
                    context.Renderer.EndBatch();
                }
            }

            if (BackgroundTexture != null && BackgroundLayout != null)
            {
                var layoutParams = BackgroundLayout.GetLayoutRect(BackgroundTexture, FinalRect);
                var scale = new Vector2(layoutParams.Scale.X / BackgroundLayout.TextureScale.X, layoutParams.Scale.Y / BackgroundLayout.TextureScale.Y);
                context.Renderer.DrawTexture(BackgroundTexture, layoutParams.LayoutRect, BackgroundLayout, scale, layoutBounds);
            }

            // Render the Internal Components
            foreach (var child in InternalChildren)
            {
                RenderChildOrFromCache(child, context, layoutBounds, Transform.Combine(parentTransform, child.Transform, child));
            }

        }

    }
}


