using Blade.MG.UI.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blade.MG.UI.Caching
{
    /// <summary>
    /// Interface for UI controls that support render caching.
    /// Implementing controls can be rendered to an off-screen RenderTarget2D
    /// and then drawn from cache instead of re-rendering every frame.
    /// </summary>
    public interface ICacheable
    {
        /// <summary>
        /// Whether caching is enabled for this control.
        /// </summary>
        bool IsCachingEnabled { get; set; }

        /// <summary>
        /// Returns true if the control's visual state has changed and requires re-rendering to cache.
        /// </summary>
        bool IsCacheInvalid { get; }

        /// <summary>
        /// The cached render target containing the pre-rendered control.
        /// </summary>
        RenderTarget2D CachedTexture { get; }

        /// <summary>
        /// The layout bounds used when the cache was last rendered.
        /// Used to detect if the control has moved or resized.
        /// </summary>
        Rectangle CachedLayoutBounds { get; }

        /// <summary>
        /// Invalidates the cache, forcing a re-render on the next pre-render pass.
        /// </summary>
        void InvalidateCache();

        /// <summary>
        /// Called during the pre-render phase to update the cached texture if needed.
        /// </summary>
        /// <param name="context">The UI context.</param>
        /// <param name="layoutBounds">The layout bounds for rendering.</param>
        /// <param name="parentTransform">The parent transform.</param>
        void UpdateCache(UIContext context, Rectangle layoutBounds, Transform parentTransform);

        /// <summary>
        /// Renders the control from cache.
        /// </summary>
        /// <param name="context">The UI context.</param>
        /// <param name="layoutBounds">The layout bounds.</param>
        /// <param name="parentTransform">The parent transform.</param>
        void RenderFromCache(UIContext context, Rectangle layoutBounds, Transform parentTransform);
    }
}
