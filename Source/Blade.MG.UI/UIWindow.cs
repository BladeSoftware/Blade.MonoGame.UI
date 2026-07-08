using Blade.MG.Input;
using Blade.MG.Primitives;
using Blade.MG.UI.Caching;
using Blade.MG.UI.CompiledResources;
using Blade.MG.UI.Components;
using Blade.MG.UI.Events;
using Blade.MG.UI.Models;
using Blade.MG.UI.Renderer;
using Blade.MG.UI.Services;
using Blade.MG.UI.Theming;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Blade.MG.UI
{
    public partial class UIWindow : Container, IDisposable  // , IGameEntity
    {
        [JsonIgnore]
        [XmlIgnore]
        public Game Game => Context.Game;

        [JsonIgnore]
        [XmlIgnore]
        public new UIContext Context { get; private set; }

        [JsonIgnore]
        [XmlIgnore]
        internal UIComponent focusedComponent = null;

        [JsonIgnore]
        [XmlIgnore]
        internal List<UIComponent> hover = new List<UIComponent>();

        [JsonIgnore]
        [XmlIgnore]
        internal bool AreEventsLockedToControl { get; set; } = false;

        [JsonIgnore]
        [XmlIgnore]
        internal UIComponent EventLockedControl { get; set; }

        [JsonIgnore]
        [XmlIgnore]
        public new ContentManager ContentManager => Game?.Content;

        //[JsonIgnore]
        //[XmlIgnore]
        ////public Viewport Viewport => Game.GraphicsDevice.Viewport;
        //public Rectangle BackBufferBounds => new Rectangle(0, 0, GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight);

        [JsonIgnore]
        [XmlIgnore]
        public ResourceDict ResourceDict { get; set; } = new ResourceDict(); // Component Resource Dictionary

        public int DefaultZIndex { get; set; } = 100;
        public int ZIndex { get; internal set; }

        /// <summary>
        /// True for popups/dialogs (e.g. ModalBase, ComboBox's dropdown) that must own all
        /// input exclusively while open: clicks/keys go only to this window, even if they also
        /// land within a window underneath it, and the modal itself decides whether a given
        /// point is "inside" (handle it) or "outside" (typically dismiss). See
        /// UIManager.DispatchEventAsync, which routes all input to the topmost modal window
        /// when one is present instead of hit-testing every open window.
        /// </summary>
        public bool IsModal { get; set; } = false;


        public virtual void Initialize(Game game)
        {
            Context = new UIContext
            {
                Game = game,
                Pixel = Primitives2D.PixelTexture(game.GraphicsDevice),
                Theme = UIManager.DefaultTheme
            };

            Context.Renderer = new UIRenderer(Context);

            FontService.RegisterFont("Default", DefaultFont.Data);
        }

        /// <summary>
        /// Applies a newly-active theme to this window: updates Context.Theme, then sweeps
        /// every control in the tree (internal children, Control.Content, Container.Children)
        /// calling StateHasChanged() so theme-driven styling re-applies immediately. Called
        /// automatically by UIManager.SetTheme; no need to call this directly under normal use.
        /// </summary>
        public void RefreshTheme(UITheme theme)
        {
            if (Context != null)
            {
                Context.Theme = theme;
            }

            RefreshThemeRecursive(this);
        }

        private static void RefreshThemeRecursive(UIComponent component)
        {
            if (component == null)
            {
                return;
            }

            component.StateHasChanged();

            foreach (var child in component.PrivateControls)
            {
                RefreshThemeRecursive(child);
            }

            if (component is Control control && control.Content != null)
            {
                RefreshThemeRecursive(control.Content);
            }

            if (component is Container container)
            {
                foreach (var child in container.Children)
                {
                    RefreshThemeRecursive(child);
                }
            }
        }

        public virtual void LoadContent()
        {
        }

        public virtual void PerformLayout(GameTime gameTime)
        {
            Context.GameTime = gameTime;

            var layoutRect = GetBackBufferRect();

            PerformLayout(layoutRect);
        }

        [JsonIgnore]
        [XmlIgnore]
        private PreRenderContext preRenderContext;

        [JsonIgnore]
        [XmlIgnore]
        private PreRenderContext PreRenderContext => preRenderContext ??= new PreRenderContext(Context);

        /// <summary>
        /// Refreshes any dirty cached-control textures (see ICacheable / EnableCaching)
        /// before the main draw pass. Must run before RenderLayout each frame: populating a
        /// cache requires temporarily switching the active render target, which would
        /// clobber whatever's currently being drawn if it happened mid-way through the main
        /// pass instead of before it.
        /// </summary>
        public virtual void PreRenderLayout(GameTime gameTime)
        {
            if (!UIManager.EnableControlCaching)
            {
                return;
            }

            Context.GameTime = gameTime;

            CollectCacheableControls(Children);
            PreRenderContext.ProcessPendingUpdates();
        }

        private void CollectCacheableControls(IEnumerable<UIComponent> components)
        {
            foreach (var component in components)
            {
                if (component == null || component.Visible.Value != Visibility.Visible)
                {
                    continue;
                }

                if (component is ICacheable cacheable)
                {
                    PreRenderContext.RegisterForPreRender(cacheable, component.FinalRect, component.Transform);
                }

                CollectCacheableControls(component.PrivateControls);

                if (component is Control control && control.Content != null)
                {
                    CollectCacheableControls(new[] { control.Content });
                }

                if (component is Container container)
                {
                    CollectCacheableControls(container.Children);
                }
            }
        }

        public virtual void RenderLayout(GameTime gameTime)
        {
            Context.GameTime = gameTime;

            // var layoutRect = Game.GraphicsDevice.PresentationParameters.IsFullScreen ? Viewport.TitleSafeArea : Viewport.Bounds;
            var layoutRect = GetBackBufferRect();

            RenderLayout(layoutRect);
        }

        private Rectangle GetBackBufferRect() => new Rectangle(0, 0, Game.GraphicsDevice.PresentationParameters.BackBufferWidth, Game.GraphicsDevice.PresentationParameters.BackBufferHeight);


        protected virtual void PerformLayout(Rectangle layoutRect)
        {
            /*
            int left = layoutRect.Left + Margin.Value.Left;
            int top = layoutRect.Top + Margin.Value.Top;
            int width = layoutRect.Width - (Margin.Value.Left + Margin.Value.Right);
            int height = layoutRect.Height - (Margin.Value.Top + Margin.Value.Bottom);

            if (width < 0) width = 0;
            if (height < 0) height = 0;

            FinalRect = new Rectangle(left, top, width, height);

            left += Padding.Value.Left;
            top += Padding.Value.Top;
            width -= Padding.Value.Left + Padding.Value.Right;
            height -= Padding.Value.Top + Padding.Value.Bottom;

            FinalContentRect = new Rectangle(left, top, width, height);

            //Size availableSize = new Size(layoutRect.Width, layoutRect.Height);
            Size availableSize = new Size(FinalContentRect.Width, FinalContentRect.Height);

             */

            Size availableSize = new Size(layoutRect.Width, layoutRect.Height);

            Layout parentMinMax = new Layout(MinWidth, MinHeight, MaxWidth, MaxHeight, availableSize);


            MeasureSelf(Context, ref availableSize, ref parentMinMax);

            ArrangeSelf(Context, layoutRect, layoutRect);

            // Have Children determine their Desired Size
            foreach (var child in Children)
            {
                child.Measure(Context, ref availableSize, ref parentMinMax);
            }

            // Arrange Layout
            foreach (var child in Children)
            {
                child.Arrange(Context, FinalContentRect, FinalContentRect);
            }
        }

        protected virtual void RenderLayout(Rectangle layoutRect)
        {
            // Render Layout

            // layoutRect is relative to the BackBuffer i.e. (0,0) is the Top-Left pixel
            // However. the Viewport on the screen may be offset, so adjust the layoutRect to be relative to the Viewport
            var viewportBounds = Game.GraphicsDevice.Viewport.Bounds;
            Game.GraphicsDevice.ScissorRectangle = layoutRect with { X = layoutRect.X + viewportBounds.X, Y = layoutRect.Y + viewportBounds.Y };

            Transform.CalcCenterPoint(this);

            // Clear Stencil Buffer
            Context.Renderer.ClearStencilBuffer();

            foreach (var child in Children)
            {
                RenderChildOrFromCache(child, Context, layoutRect, Transform.Combine(Transform, child.Transform, child));
            }

        }


        private IEnumerable<UIComponent> SelectAny(Func<UIComponent, bool> selector)
        {
            List<UIComponent> selected = new List<UIComponent>();

            // Local Function to handle recursion
            void SelectComponentsInternal(UIComponent current)
            {
                if (selector(current))
                {
                    selected.Add(current);
                }

                if (current as Control != null)
                {
                    if (((Control)current).Content != null)
                    {
                        SelectComponentsInternal(((Control)current).Content);
                    }
                }
                else if (current as Container != null)
                {
                    foreach (UIComponent child in ((Container)current).Children)
                    {
                        SelectComponentsInternal(child);
                    }
                }
                else
                {
                }
            }

            // Start Recursive Search
            SelectComponentsInternal(this);

            return selected;
        }

        internal UIComponent SelectFirst(Func<UIComponent, UIComponent, bool> selector, bool useHitTest, Point point = default)
        {
            UIComponent selected = null;

            // Local Function to handle recursion
            void SelectComponentsInternal(UIComponent current, UIComponent parent)
            {
                foreach (UIComponent child in current.PrivateControls)
                {
                    if (child != null)
                    {
                        if (!useHitTest || Rectangle.Intersect(current.FinalRect, child.FinalRect).Contains(point))
                        {
                            SelectComponentsInternal(child, current);

                            if (selected != null)
                            {
                                return;
                            }
                        }
                    }
                }

                if (current as Control != null)
                {
                    if (((Control)current).Content != null)
                    {
                        if (!useHitTest || Rectangle.Intersect(current.FinalRect, ((Control)current).Content.FinalRect).Contains(point))
                        {
                            SelectComponentsInternal(((Control)current).Content, current);

                            if (selected != null)
                            {
                                return;
                            }
                        }
                    }
                }
                else if (current as Container != null)
                {
                    foreach (UIComponent child in ((Container)current).Children)
                    {
                        if (!useHitTest || Rectangle.Intersect(current.FinalRect, child.FinalRect).Contains(point))
                        {
                            SelectComponentsInternal(child, current);

                            if (selected != null)
                            {
                                return;
                            }
                        }
                    }
                }
                else
                {
                }

                if (!useHitTest || Rectangle.Intersect(parent.FinalRect, current.FinalRect).Contains(point))
                {
                    if (selector(current, parent))
                    {
                        selected = current;
                        return;
                    }
                }

            }

            if (EventLockedControl != null)
            {
                SelectComponentsInternal(EventLockedControl, this);
            }
            else
            {
                // Start Recursive Search
                SelectComponentsInternal(this, this);
            }

            return selected;
        }

        internal async Task RaiseFocusChangedEventAsync(UIComponent component, UIWindow uiWindow)
        {
            if (focusedComponent != null && component != null && focusedComponent == component)
            {
                return;
            }

            // Unfocus previous component
            if (focusedComponent != null && focusedComponent.HasFocus.Value)
            {
                focusedComponent.HasFocus = false;

                UIComponentEvents focusedCtrl1 = focusedComponent as UIComponentEvents;
                if (focusedCtrl1 != null)
                {
                    await focusedCtrl1.HandleFocusChangedEventAsync(uiWindow, new UIFocusChangedEvent { Focused = false });
                }
            }

            // Focus new component
            focusedComponent = component;
            if (focusedComponent != null && !focusedComponent.HasFocus.Value)
            {
                focusedComponent.HasFocus = true;

                UIComponentEvents focusedCtrl = focusedComponent as UIComponentEvents;
                if (focusedCtrl != null)
                {
                    await focusedCtrl.HandleFocusChangedEventAsync(uiWindow, new UIFocusChangedEvent { Focused = true });
                }
            }

        }

        internal async Task RaiseHoverEnterEventAsync(UIComponent component, UIWindow uiWindow)
        {
            UIComponentEvents ctrl = component as UIComponentEvents;
            if (ctrl != null)
            {
                await ctrl.HandleHoverChangedAsync(uiWindow, new UIHoverChangedEvent { Hover = true, X = InputManager.Mouse.X, Y = InputManager.Mouse.Y });
            }
        }

        internal async Task RaiseHoverLeaveEventAsync(UIComponent component, UIWindow uiWindow)
        {
            UIComponentEvents ctrl = component as UIComponentEvents;
            if (ctrl != null)
            {
                // Always propogate Hover Leave event as we've aleady moved off that control
                await ctrl.HandleHoverChangedAsync(uiWindow, new UIHoverChangedEvent { Hover = false, X = InputManager.Mouse.X, Y = InputManager.Mouse.Y, ForcePropagation = true });
            }
        }


        private async Task HandleTabNext()
        {
            int lastTabOrder = -1;

            if (focusedComponent != null)
            {
                lastTabOrder = focusedComponent.TabIndex;
            }

            //Func<UIComponent, bool> selector = (p) => (p.TabIndex > lastTabOrder) && (p.IsTabStop == true);
            bool selector(UIComponent p) => p.TabIndex > lastTabOrder && p.IsTabStop.Value == true;

            IEnumerable<UIComponent> selected = SelectAny(selector);
            if (lastTabOrder > 0 && selected.Count() == 0)
            {
                lastTabOrder = -1;
                selected = SelectAny(selector);
            }

            UIComponent nextControl = selected.FirstOrDefault();
            if (nextControl != null)
            {
                await RaiseFocusChangedEventAsync(nextControl, this);
            }
        }

        public async Task SetFocusAsync(UIComponent control)
        {
            await RaiseFocusChangedEventAsync(control, this);
        }

        public override void Dispose()
        {
            hover.Clear();

            Context?.Renderer?.Dispose();
            Context = null;

            base.Dispose();
        }

    }
}
