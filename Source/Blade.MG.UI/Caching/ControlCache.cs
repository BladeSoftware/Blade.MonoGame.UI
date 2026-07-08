using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blade.MG.UI.Caching
{
    /// <summary>
    /// Manages render target caching for a UI control.
    /// Handles cache validation, creation, and lifecycle.
    /// </summary>
    public class ControlCache : IDisposable
    {
        private RenderTarget2D _cachedTexture;
        private Rectangle _cachedLayoutBounds;
        private int _cachedStateHash;
        private bool _isCacheValid;
        private bool _isDisposed;

        /// <summary>
        /// The cached render target texture.
        /// </summary>
        public RenderTarget2D CachedTexture => _cachedTexture;

        /// <summary>
        /// The layout bounds when the cache was last rendered.
        /// </summary>
        public Rectangle CachedLayoutBounds => _cachedLayoutBounds;

        /// <summary>
        /// Whether caching is currently enabled.
        /// </summary>
        public bool IsCachingEnabled { get; set; } = true;

        /// <summary>
        /// Returns true if the cache needs to be updated.
        /// </summary>
        public bool IsCacheInvalid => !_isCacheValid || _cachedTexture == null || _cachedTexture.IsDisposed;

        /// <summary>
        /// Invalidates the cache, forcing a re-render.
        /// </summary>
        public void InvalidateCache()
        {
            _isCacheValid = false;
        }

        /// <summary>
        /// Checks if the cache is still valid for the given state and bounds.
        /// </summary>
        /// <param name="stateHash">Hash representing the control's visual state.</param>
        /// <param name="layoutBounds">Current layout bounds.</param>
        /// <returns>True if cache is valid and can be used.</returns>
        public bool IsCacheValidFor(int stateHash, Rectangle layoutBounds)
        {
            if (!_isCacheValid || _cachedTexture == null || _cachedTexture.IsDisposed)
                return false;

            // Check if state has changed
            if (_cachedStateHash != stateHash)
                return false;

            // Check if size has changed (position changes don't require re-cache)
            if (_cachedLayoutBounds.Width != layoutBounds.Width || _cachedLayoutBounds.Height != layoutBounds.Height)
                return false;

            return true;
        }

        /// <summary>
        /// Ensures the render target is properly sized for the control.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device.</param>
        /// <param name="width">Required width.</param>
        /// <param name="height">Required height.</param>
        /// <returns>True if a new render target was created.</returns>
        public bool EnsureRenderTarget(GraphicsDevice graphicsDevice, int width, int height)
        {
            if (width <= 0 || height <= 0)
                return false;

            // Check if existing render target is sufficient
            if (_cachedTexture != null && !_cachedTexture.IsDisposed &&
                _cachedTexture.Width >= width && _cachedTexture.Height >= height)
            {
                return false;
            }

            // Dispose old texture
            _cachedTexture?.Dispose();

            // Create new render target with some padding to reduce recreations.
            // Uses a stencil-capable depth format because cached content (e.g. Border's
            // rounded corners) may rely on the stencil buffer while being rendered into
            // the cache texture.
            int paddedWidth = Math.Max(width, 64);
            int paddedHeight = Math.Max(height, 32);

            _cachedTexture = new RenderTarget2D(
                graphicsDevice,
                paddedWidth,
                paddedHeight,
                false,
                SurfaceFormat.Color,
                DepthFormat.Depth24Stencil8,
                0,
                RenderTargetUsage.PreserveContents);

            _isCacheValid = false;
            return true;
        }

        /// <summary>
        /// Marks the cache as valid after a successful render.
        /// </summary>
        /// <param name="stateHash">The state hash that was rendered.</param>
        /// <param name="layoutBounds">The layout bounds that were rendered.</param>
        public void MarkCacheValid(int stateHash, Rectangle layoutBounds)
        {
            _cachedStateHash = stateHash;
            _cachedLayoutBounds = layoutBounds;
            _isCacheValid = true;
        }

        /// <summary>
        /// Disposes the cached render target.
        /// </summary>
        public void Dispose()
        {
            if (!_isDisposed)
            {
                _cachedTexture?.Dispose();
                _cachedTexture = null;
                _isDisposed = true;
            }
        }
    }
}
