using Blade.MG.UI.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blade.MG.UI.Caching
{
    /// <summary>
    /// Base class providing caching functionality for UI controls.
    /// Extend this class or use ControlCache directly to add caching to controls.
    /// </summary>
    public abstract class CacheableControlBase : ICacheable
    {
        private readonly ControlCache _cache = new();

        /// <summary>
        /// Whether caching is enabled for this control.
        /// </summary>
        public bool IsCachingEnabled
        {
            get => _cache.IsCachingEnabled;
            set => _cache.IsCachingEnabled = value;
        }

        /// <summary>
        /// Returns true if the cache needs updating.
        /// </summary>
        public bool IsCacheInvalid => _cache.IsCacheInvalid || !_cache.IsCacheValidFor(GetStateHash(), GetCurrentBounds());

        /// <summary>
        /// The cached render target.
        /// </summary>
        public RenderTarget2D CachedTexture => _cache.CachedTexture;

        /// <summary>
        /// The cached layout bounds.
        /// </summary>
        public Rectangle CachedLayoutBounds => _cache.CachedLayoutBounds;

        /// <summary>
        /// Invalidates the cache.
        /// </summary>
        public void InvalidateCache() => _cache.InvalidateCache();

        /// <summary>
        /// Gets the current bounds of the control. Override to provide actual bounds.
        /// </summary>
        protected abstract Rectangle GetCurrentBounds();

        /// <summary>
        /// Gets a hash representing the visual state of the control.
        /// Override to include all properties that affect rendering.
        /// </summary>
        protected abstract int GetStateHash();

        /// <summary>
        /// Gets the graphics device. Override to provide access.
        /// </summary>
        protected abstract GraphicsDevice GetGraphicsDevice();

        /// <summary>
        /// Performs the actual rendering of the control content.
        /// Called when cache needs updating.
        /// </summary>
        /// <param name="context">UI context.</param>
        /// <param name="localBounds">Local bounds (0,0 based for render target).</param>
        /// <param name="parentTransform">Parent transform.</param>
        protected abstract void RenderContent(UIContext context, Rectangle localBounds, Transform parentTransform);

        /// <summary>
        /// Updates the cache if needed.
        /// </summary>
        public virtual void UpdateCache(UIContext context, Rectangle layoutBounds, Transform parentTransform)
        {
            if (!IsCachingEnabled)
                return;

            var graphicsDevice = GetGraphicsDevice();
            if (graphicsDevice == null)
                return;

            int stateHash = GetStateHash();

            // Check if cache is still valid
            if (_cache.IsCacheValidFor(stateHash, layoutBounds))
                return;

            // Ensure render target is sized correctly
            _cache.EnsureRenderTarget(graphicsDevice, layoutBounds.Width, layoutBounds.Height);

            if (_cache.CachedTexture == null)
                return;

            // Save current render targets
            var savedRenderTargets = graphicsDevice.GetRenderTargets();

            try
            {
                // Switch to cache render target
                graphicsDevice.SetRenderTarget(_cache.CachedTexture);
                graphicsDevice.Clear(Color.Transparent);

                // Render control content to cache using local coordinates (0,0 based)
                var localBounds = new Rectangle(0, 0, layoutBounds.Width, layoutBounds.Height);
                RenderContent(context, localBounds, new Transform());

                // Mark cache as valid
                _cache.MarkCacheValid(stateHash, layoutBounds);
            }
            finally
            {
                // Restore previous render targets
                graphicsDevice.SetRenderTargets(savedRenderTargets);
            }
        }

        /// <summary>
        /// Renders from cache.
        /// </summary>
        public virtual void RenderFromCache(UIContext context, Rectangle layoutBounds, Transform parentTransform)
        {
            if (!IsCachingEnabled || _cache.CachedTexture == null || _cache.CachedTexture.IsDisposed)
                return;

            var spriteBatch = context.Renderer.BeginBatch(transform: parentTransform);

            // Draw cached texture at the control's position
            var sourceRect = new Rectangle(0, 0, layoutBounds.Width, layoutBounds.Height);
            spriteBatch.Draw(_cache.CachedTexture, layoutBounds, sourceRect, Color.White);

            context.Renderer.EndBatch();
        }

        /// <summary>
        /// Disposes the cache.
        /// </summary>
        protected void DisposeCache() => _cache.Dispose();
    }
}
