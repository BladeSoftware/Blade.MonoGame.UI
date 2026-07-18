using Blade.MG.Primitives;
using Blade.MG.UI.Animations;
using Blade.MG.UI.Components;
using Blade.MG.UI.Models;
using Blade.MG.UI.Theming;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;

namespace Blade.MG.UI
{
    public enum UITaskType
    {
        Add,
        Remove,
        Clear
    }

    public class UITask
    {
        public UITaskType TaskType;
        public UIWindow Window;
        public int ZIndex;
    }

    public partial class UIManager : GameEntity //,UIManagerBase
    {
        public static bool RenderControlHitBoxes = false;

        // Backs the hover-set change-detection in Draw()'s RenderControlHitBoxes diagnostic.
        private string lastLoggedHoverSet = "";

        public static readonly int MaxZIndex = 9999;

        /// <summary>
        /// Gets or sets whether control caching is enabled globally.
        /// </summary>
        public static bool EnableControlCaching { get; set; } = true;


        //private static SemaphoreSlim uiSemaphore = new SemaphoreSlim(1);

        private static UITheme defaultTheme = DefaultThemes.LightTheme();

        /// <summary>
        /// The app's active theme. Read this for the theme to use when creating new
        /// windows/controls, or write it to set the app's default theme at startup (e.g.
        /// UIManager.DefaultTheme = DefaultThemes.DarkTheme(); before adding any windows).
        /// Assigning it after windows already exist swaps them live - see SetTheme, which
        /// this setter delegates to.
        /// </summary>
        public static UITheme DefaultTheme
        {
            get => defaultTheme;
            set => SetTheme(value);
        }

        /// <summary>
        /// Raised after the active theme changes and every existing window has already been
        /// refreshed to use it. Useful for app-level UI (e.g. a theme picker) that wants to
        /// reflect the current selection.
        /// </summary>
        public static event Action<UITheme> ThemeChanged;

        /// <summary>
        /// Swaps the app's active theme immediately: every existing window/control is swept
        /// to re-apply its theme-driven styling (via StateHasChanged), no restart needed.
        /// New windows/controls created afterward also start with this theme. Per-control
        /// style overrides (UIComponentDrawable.SetStyleOverride) are unaffected, since they
        /// take precedence over whatever the theme provides either way.
        /// </summary>
        public static void SetTheme(UITheme theme)
        {
            if (theme == null || ReferenceEquals(theme, defaultTheme))
            {
                return;
            }

            defaultTheme = theme;

            foreach (var instance in Instances)
            {
                instance.RefreshTheme(theme);
            }

            ThemeChanged?.Invoke(theme);
        }

        public static ResourceDict ResourceDict { get; set; } = new ResourceDict(); // Default Resource Dictionaries

        //public static UIManager Instance { get; set; } = new UIManager();

        // Tracks live UIManager instances so SetTheme can refresh their windows. There's
        // normally exactly one per Game; a WeakReference list keeps this from holding a
        // UIManager alive past its Game's lifetime.
        private static readonly List<WeakReference<UIManager>> instances = new();

        private static IEnumerable<UIManager> Instances
        {
            get
            {
                for (int i = instances.Count - 1; i >= 0; i--)
                {
                    if (instances[i].TryGetTarget(out var manager))
                    {
                        yield return manager;
                    }
                    else
                    {
                        instances.RemoveAt(i);
                    }
                }
            }
        }

        protected ConcurrentQueue<UITask> uiTaskQueue = new ConcurrentQueue<UITask>();

        // List of Windows sorted by Priority
        // Windows with a higher priority are on top of windows with lower priorities
        private SortedList<string, UIWindow> uiWindows { get; set; } = new();
        public IReadOnlyList<UIWindow> GetWindows => uiWindows.Values.AsReadOnly();

        // Queue for async continuations to run on the main thread
        private ConcurrentQueue<Action> mainThreadQueue = new ConcurrentQueue<Action>();


        public UIManager(Game game)
        {
            this.game = game;

            instances.Add(new WeakReference<UIManager>(this));
        }

        private void RefreshTheme(UITheme theme)
        {
            foreach (var ui in uiWindows.Values)
            {
                ui.RefreshTheme(theme);
            }
        }

        private string ToPriority(int i) => $"{i.ToString("0000")}.{Stopwatch.GetTimestamp()}";

        public void EnqueTask(UITask task)
        {
            uiTaskQueue.Enqueue(task);
        }

        /// <summary>
        /// Queue an action to run on the main game thread (next Update)
        /// </summary>
        public void QueueOnMainThread(Action action)
        {
            mainThreadQueue.Enqueue(action);
        }

        /// <summary>
        /// Process any queued main thread actions
        /// </summary>
        private void ProcessMainThreadQueue()
        {
            while (mainThreadQueue.TryDequeue(out var action))
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error processing main thread action: {ex.Message}");
                }
            }
        }


        protected void HandleTaskQueue()
        {
            while (uiTaskQueue.TryDequeue(out var task))
            {
                if (task.TaskType == UITaskType.Add)
                {
                    var priority = ToPriority(Math.Clamp(task.ZIndex, 0, MaxZIndex));

                    task.Window.ZIndex = task.ZIndex;

                    uiWindows.Add(priority, task.Window);
                }
                else if (task.TaskType == UITaskType.Remove)
                {
                    int index = uiWindows.IndexOfValue(task.Window);
                    if (index >= 0)
                    {
                        uiWindows.RemoveAt(index);
                    }

                    UnloadWindow(task.Window);
                }
                else if (task.TaskType == UITaskType.Clear)
                {
                    for (int i = uiWindows.Count - 1; i >= 0; i--)
                    {
                        var window = uiWindows?.ElementAtOrDefault(i);
                        uiWindows?.RemoveAt(i);
                        UnloadWindow(window.Value.Value);
                    }
                }
                else
                {
                    throw new Exception($"Unknown UITaskType : {Enum.GetName(task.TaskType)}");
                }
            }

        }

        public void Clear()
        {
            EnqueTask(new UITask { TaskType = UITaskType.Clear, Window = null });
        }


        private void AddWindow(UIWindow ui, int? priority = null)
        {
            ui.Initialize(game);
            ui.LoadContent();

            var task = new UITask { TaskType = UITaskType.Add, Window = ui, ZIndex = priority ?? ui.DefaultZIndex };
            EnqueTask(task);
        }

        // Enqueue-only, like Remove/RemoveAll/RemoveOthers/Clear below - actual insertion into
        // uiWindows only ever happens inside HandleTaskQueue(), called once per frame at the top
        // of Update(). This used to also call HandleTaskQueue() immediately, making Add the only
        // mutation method that took effect synchronously - which meant a window shown as a side
        // effect of something Update()/Draw() was itself in the middle of iterating uiWindows
        // for (e.g. a Popup shown from a click handler, or from another window's own
        // PerformLayout/PreRenderLayout/RenderLayout) could mutate uiWindows while that very
        // enumeration was still active, and SortedList's enumerator throws
        // "Collection was modified after the enumerator was instantiated" for exactly that.
        // Deferring Add like every other mutation removes the failure mode structurally (nothing
        // ever mutates uiWindows outside HandleTaskQueue's own dequeue loop, which doesn't
        // foreach over uiWindows itself) instead of defensively snapshotting every read site.
        public void Add(UIWindow ui, int? priority = null)
        {
            AddWindow(ui, priority);
        }

        public void Add(ICollection<UIWindow> ui, int? priority = null)
        {
            foreach (var uiWindow in ui)
            {
                AddWindow(uiWindow, priority);
            }
        }

        public T Find<T>() where T : UIWindow
        {
            return (T)uiWindows.Values.FirstOrDefault(p => p.GetType() == typeof(T));
        }

        public T Remove<T>() where T : UIWindow
        {
            var window = (T)uiWindows.Values.FirstOrDefault(p => p.GetType() == typeof(T));
            if (window != null)
            {
                EnqueTask(new UITask { TaskType = UITaskType.Remove, Window = window });
            }

            return window;
        }

        public T Remove<T>(T instance) where T : UIWindow
        {
            var window = (T)uiWindows.Values.FirstOrDefault(p => p == instance);
            if (window != null)
            {
                EnqueTask(new UITask { TaskType = UITaskType.Remove, Window = window });
            }

            return window;
        }

        public void RemoveAll<T>() where T : UIWindow
        {

            for (int i = uiWindows.Count - 1; i >= 0; i--)
            {
                var window = uiWindows?.Values.ElementAtOrDefault(i);

                if (window?.GetType() == typeof(T))
                {
                    EnqueTask(new UITask { TaskType = UITaskType.Remove, Window = window });
                }
            }

        }

        public void RemoveOthers<T>() where T : UIWindow
        {

            for (int i = uiWindows.Count - 1; i >= 0; i--)
            {
                if (uiWindows.Values[i] != null && uiWindows.Values[i].GetType() != typeof(T))
                {
                    var window = uiWindows.Values.ElementAt(i);

                    EnqueTask(new UITask { TaskType = UITaskType.Remove, Window = window });
                }
            }

        }


        //public static T Hide<T>() where T : UIWindow
        //{
        //    var window = (T)UI.FirstOrDefault(p => p.GetType() == typeof(T));
        //    if (window != null)
        //    {
        //        UI.Remove(window);
        //        UnloadWindow(window);
        //    }

        //    return window;
        //}

        //public static void HideAll<T>() where T : UIWindow
        //{
        //    for (int i = UI.Count - 1; i >= 0; i--)
        //    {
        //        if (UI[i] != null)
        //        {
        //            var window = UI.ElementAt(i);
        //            UI.RemoveAt(i);
        //            UnloadWindow(window);
        //        }
        //    }
        //}

        //public static void HideOthers<T>() where T : UIWindow
        //{
        //    for (int i = UI.Count - 1; i >= 0; i--)
        //    {
        //        if (UI[i] != null && UI[i].GetType() != typeof(T))
        //        {
        //            var window = UI.ElementAt(i);
        //            UI.RemoveAt(i);
        //            UnloadWindow(window);
        //        }
        //    }
        //}


        protected static void UnloadWindow(UIWindow window)
        {
            if (window != null)
            {
                window.Dispose();
            }
        }

        public override void LoadContent()
        {

        }

        //public override void Update(GameTime gameTime)
        //{
        //    joinableTaskFactory.Run(async () => await UpdateAsync(gameTime));

        //    //await UpdateAsync(gameTime).ConfigureAwait(true);
        //}

        //public async Task UpdateAsync(GameTime gameTime)
        //{
        //    //base.Logic(gameTime);

        //    HandleTaskQueue();

        //    UIWindow eventLockedWindow = null;
        //    UIComponent eventLockedControl = null;

        //    // First check if events are locked to a control in any window
        //    foreach (var ui in uiWindows)
        //    {
        //        if (ui.Value.EventLockedControl != null)
        //        {
        //            eventLockedWindow = ui.Value;
        //            eventLockedControl = ui.Value.EventLockedControl;
        //            //ui.Logic(gameTime);
        //            break;
        //        }
        //    }

        //    // Check if events are locked to a specific control, e.g. scrollbar
        //    for (int i = uiWindows.Count - 1; i >= 0; i--)
        //    {
        //        var ui = uiWindows.Values[i];

        //        ui.AreEventsLockedToControl = (eventLockedWindow != null);
        //    }

        //    // Track focused elements and handle keys
        //    // Converts physical input into 'abstracted' actions
        //    await HandleKeyboardInputAsync(eventLockedWindow, eventLockedControl, true, gameTime);
        //    await HandleMouseInputAsync(eventLockedWindow, eventLockedControl, true, gameTime);
        //    await HandleTouchInputAsync(eventLockedWindow, eventLockedControl, true, gameTime);
        //    await HandleGamePadInputAsync(eventLockedWindow, eventLockedControl, true, gameTime);

        //    // Arrange layout
        //    foreach (var ui in uiWindows)
        //    {
        //        if (ui.Value.Visible.Value == Visibility.Visible)
        //        {
        //            ui.Value.PerformLayout(gameTime);
        //        }
        //    }

        //}

        // Input handlers are async so control event handlers can await things like modal
        // dialogs (e.g. ModalBase.ShowAsync / Menu.ShowAsync), which by design stay pending
        // across many frames until the user interacts with the modal - the game loop keeps
        // running while they wait. That means these handlers must NEVER be blocked on
        // synchronously: doing so (e.g. via JoinableTaskFactory.Run or .GetAwaiter().GetResult())
        // freezes the whole app, because the dialog can only be dismissed by a *later*
        // Update() call that would never get to run.
        //
        // Calling an async method without awaiting it still runs it synchronously up to its
        // first genuine suspension point, so the common case (no dialog involved) completes
        // immediately here, in order, with faults observed right away - only the rare case
        // (a dialog/modal got shown) falls through to running the rest in the background.
        private void RunInputHandler(Func<Task> taskFactory, string handlerName)
        {
            Task task;

            try
            {
                task = taskFactory();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error processing {handlerName} input: {ex}");
                return;
            }

            if (task.IsCompleted)
            {
                if (task.IsFaulted)
                {
                    Debug.WriteLine($"Error processing {handlerName} input: {task.Exception?.InnerException?.Message}");
                }

                return;
            }

            // Still in flight (e.g. a modal dialog is now up, waiting on future frames) -
            // let it continue on its own; just make sure a fault isn't silently swallowed.
            task.ContinueWith(t =>
            {
                Debug.WriteLine($"Error processing {handlerName} input: {t.Exception?.InnerException?.Message}");
            }, TaskContinuationOptions.OnlyOnFaulted);
        }


        /// <summary>
        /// Input is dispatched in a fixed order (keyboard, mouse, touch, gamepad) before
        /// layout runs each frame. Each handler runs synchronously through to completion
        /// unless it triggers a multi-frame operation like a modal dialog, in which case it
        /// continues in the background rather than blocking this Update() call.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            // Process any queued main thread actions
            ProcessMainThreadQueue();

            // Handle window add/remove queue
            HandleTaskQueue();

            UIWindow eventLockedWindow = null;
            UIComponent eventLockedControl = null;

            // First check if events are locked to a control in any window. Safe from
            // "Collection was modified" mid-iteration - Add() (like Remove/RemoveAll/
            // RemoveOthers/Clear) only enqueues; uiWindows itself is only ever mutated inside
            // HandleTaskQueue(), which already ran once, above, before this loop starts, and
            // isn't called again until next frame - nothing between here and then touches
            // uiWindows.
            foreach (var ui in uiWindows.Values)
            {
                if (ui.EventLockedControl != null)
                {
                    eventLockedWindow = ui;
                    eventLockedControl = ui.EventLockedControl;
                    break;
                }
            }

            // Check if events are locked to a specific control
            for (int i = uiWindows.Count - 1; i >= 0; i--)
            {
                var ui = uiWindows.Values[i];
                ui.AreEventsLockedToControl = (eventLockedWindow != null);
            }

            // Handle input in order, so layout below sees this frame's up-to-date input
            // state (hover, focus, scroll offsets, etc.) for the common case where handling
            // completes synchronously (see RunInputHandler for the modal-dialog exception).
            RunInputHandler(() => HandleKeyboardInputAsync(eventLockedWindow, eventLockedControl, true, gameTime), "keyboard");
            RunInputHandler(() => HandleMouseInputAsync(eventLockedWindow, eventLockedControl, true, gameTime), "mouse");
            RunInputHandler(() => HandleTouchInputAsync(eventLockedWindow, eventLockedControl, true, gameTime), "touch");
            RunInputHandler(() => HandleGamePadInputAsync(eventLockedWindow, eventLockedControl, true, gameTime), "gamepad");

            // Apply this frame's active property animations before layout/render read any
            // Binding values they're driving (e.g. a focus change above may have just kicked off
            // a new AnimateTo).
            PropertyAnimationManager.Update();

            // Arrange layout (synchronous) - safe from "Collection was modified" mid-iteration,
            // see the matching comment on the eventLockedWindow loop above. A window's own
            // PerformLayout showing another window (e.g. a Popup) only enqueues that Add; it
            // doesn't touch uiWindows until next frame, so this loop's own enumerator is never
            // invalidated by it.
            foreach (var ui in uiWindows.Values)
            {
                if (ui.Visible.Value == Visibility.Visible)
                {
                    ui.PerformLayout(gameTime);
                }
            }
        }

        ///// <summary>
        ///// Performs pre-rendering for all windows that support caching.
        ///// This should be called before Draw when caching is enabled.
        ///// </summary>
        //public void PreRender(GameTime gameTime)
        //{
        //    if (!EnableControlCaching)
        //        return;

        //    foreach (var ui in uiWindows)
        //    {
        //        if (ui.Value.Visible.Value == Components.Visibility.Visible && ui.Value.EnableControlCaching)
        //        {
        //            ui.Value.PreRenderLayout(gameTime);
        //        }
        //    }
        //}

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime, RenderTarget2D renderTarget)
        {
            // Refresh any dirty cached-control textures before the main draw pass, for every
            // window, so cache population - which needs to switch render targets - never
            // happens mid-way through drawing to the real target. No-ops per window when
            // EnableControlCaching is off. Safe from "Collection was modified" mid-iteration -
            // Draw() never calls HandleTaskQueue() itself, and Add()/Remove()/etc. only enqueue,
            // so nothing can mutate uiWindows for the rest of this frame (e.g. a Tooltip's
            // hover-delay elapsing mid-draw and calling ShowAt just enqueues; it takes effect
            // next frame's Update()).
            foreach (var ui in uiWindows.Values)
            {
                if (ui.Visible.Value == Visibility.Visible)
                {
                    ui.PreRenderLayout(gameTime);
                }
            }

            foreach (var ui in uiWindows.Values)
            {
                if (ui.Visible.Value == Visibility.Visible)
                {
                    ui.RenderLayout(gameTime);
                }
            }

            // Outline control we're hovering over. Only allocate a SpriteBatch for this when
            // the debug overlay is actually enabled, rather than every frame regardless.
            if (RenderControlHitBoxes)
            {
                using SpriteBatch sb = new SpriteBatch(graphicsDevice);

                sb.Begin();

                // Identify exactly what's in the hover list (type/Name/DataContext), not just
                // its rect - a rect alone doesn't say whether it's the control you expect or an
                // unrelated nested one (e.g. a template's own inner Button) with a similar-looking
                // hit box. Logged only when the set actually changes, to Debug.WriteLine
                // (visible in Visual Studio's Output window).
                string current = string.Join(" | ", HoverIterator.Select(c =>
                    $"{c.GetType().Name}(Name={(c as Control)?.Name ?? "?"}, DataContext={c.DataContext?.ToString() ?? "null"}, Rect={c.FinalRect})"));
                if (current != lastLoggedHoverSet)
                {
                    lastLoggedHoverSet = current;
                    Debug.WriteLine($"[HoverDebug] hover set = [{current}]");
                }

                foreach (var control in HoverIterator)
                {
                    Primitives2D.DrawRect(sb, control.FinalContentRect, Color.Blue);
                    Primitives2D.DrawRect(sb, control.FinalRect, Color.Red);
                }
                sb.End();
            }
        }


        internal UIComponent SelectFirst(Func<UIComponent, UIComponent, bool> selector, bool useHitTest, Point? point = null)
        {
            // Select control at the given X,Y co-ordinate, in reverse order
            for (int i = uiWindows.Count - 1; i >= 0; i--)
            {
                var ui = uiWindows.Values[i];

                if (point != null && !ui.ContainsScreenPoint(point.Value))
                {
                    continue;
                }

                var component = ui.SelectFirst(selector, useHitTest, point ?? default);
                if (component != null)
                {
                    return component;
                }

            }

            return null;
        }

        internal IEnumerable<UIComponent> HoverIterator
        {
            get
            {
                for (int i = uiWindows.Count - 1; i >= 0; i--)
                {
                    var ui = uiWindows.Values[i];

                    if (ui.hover.Count > 0)
                    {
                        UIComponent[] hover = ui.hover.ToArray();
                        foreach (var component in hover)
                        {
                            yield return component;
                        }
                    }
                }
            }
        }

        private async Task DispatchEventAsync(UIWindow eventLockedWindow, Point? point, Func<UIWindow, Task> action)
        {
            if (eventLockedWindow != null)
            {
                await action(eventLockedWindow);
                return;
            }

            // If a modal popup/dialog is open, it owns all input exclusively - windows
            // underneath must not also react to the same click/key, even if the point also
            // falls within their bounds (e.g. a menu item rendered on top of a button must not
            // let the click "fall through" to that button). The modal's own hit-testing decides
            // whether the point is inside (handle it) or outside (typically dismiss).
            for (int i = uiWindows.Count - 1; i >= 0; i--)
            {
                if (uiWindows.Values[i].IsModal)
                {
                    await action(uiWindows.Values[i]);
                    return;
                }
            }

            // Despatch events to all windows, in reverse order
            for (int i = uiWindows.Count - 1; i >= 0; i--)
            {
                var ui = uiWindows.Values[i];

                if (point != null && !ui.ContainsScreenPoint(point.Value))
                {
                    continue;
                }

                await action(ui);
            }

        }

        //public static void EnsureTopMost(UIWindow uIWindow)
        //{
        //if (Instance.UI.Values.Last() != uIWindow)
        //{
        //    Instance.EnqueTask(new UITask { TaskType = UITaskType.Remove, Window = uIWindow });
        //    Instance.EnqueTask(new UITask { TaskType = UITaskType.Add, Window = uIWindow });
        //}

        //if (Instance.UI.Last() != uIWindow)
        //{
        //    try
        //    {
        //        uiSemaphore.Wait();

        //        if (Instance.UI.Remove(uIWindow))
        //        {
        //            Instance.UI.Add(uIWindow);
        //        }
        //    }
        //    finally
        //    {
        //        uiSemaphore.Release();
        //    }

        //}
        //}

        public override void Dispose()
        {
            base.Dispose();

            // Image controls need to dispose of their Texture2D images
            // Possibly other controls as well

            foreach (var window in uiWindows.Values)
            {
                window.Dispose();
            }

        }

    }
}
