using Blade.MG.UI.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Blade.MG.UI.Caching
{
    /// <summary>
    /// Context used during the pre-render phase to collect and update control caches.
    /// </summary>
    public class PreRenderContext
    {
        private readonly List<ICacheable> _cacheableControls = new();
        private readonly List<(ICacheable Control, Rectangle Bounds, Transform Transform)> _pendingUpdates = new();

        /// <summary>
        /// The graphics device.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; }

        /// <summary>
        /// The UI context.
        /// </summary>
        public UIContext UIContext { get; }

        /// <summary>
        /// Creates a new pre-render context.
        /// </summary>
        public PreRenderContext(UIContext uiContext)
        {
            UIContext = uiContext;
            GraphicsDevice = uiContext?.GraphicsDevice;
        }

        /// <summary>
        /// Registers a cacheable control for pre-rendering.
        /// </summary>
        public void RegisterForPreRender(ICacheable control, Rectangle layoutBounds, Transform parentTransform)
        {
            if (control.IsCachingEnabled && control.IsCacheInvalid)
            {
                _pendingUpdates.Add((control, layoutBounds, parentTransform));
            }
        }

        /// <summary>
        /// Processes all pending cache updates.
        /// This should be called before the main render pass.
        /// </summary>
        public void ProcessPendingUpdates()
        {
            if (_pendingUpdates.Count == 0)
                return;

            foreach (var (control, bounds, transform) in _pendingUpdates)
            {
                control.UpdateCache(UIContext, bounds, transform);
            }

            _pendingUpdates.Clear();
        }

        /// <summary>
        /// Clears any pending updates.
        /// </summary>
        public void Clear()
        {
            _pendingUpdates.Clear();
        }
    }
}
