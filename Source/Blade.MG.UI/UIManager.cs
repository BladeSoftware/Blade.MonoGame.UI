using Blade.MG.Primitives;
using Blade.MG.UI.Components;
using Blade.MG.UI.Models;
using Blade.MG.UI.Theming;
using Microsoft.VisualStudio.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Concurrent;
using System.Diagnostics;

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

        public static readonly int MaxZIndex = 9999;

        //private static SemaphoreSlim uiSemaphore = new SemaphoreSlim(1);

        public static UITheme DefaultTheme { get; set; } = DefaultThemes.LightTheme();

        public static ResourceDict ResourceDict { get; set; } = new ResourceDict(); // Default Resource Dictionaries

        //public static UIManager Instance { get; set; } = new UIManager();

        protected ConcurrentQueue<UITask> uiTaskQueue = new ConcurrentQueue<UITask>();

        // List of Windows sorted by Priority
        // Windows with a higher priority are on top of windows with lower priorities
        private SortedList<string, UIWindow> uiWindows { get; set; } = new();
        public IReadOnlyList<UIWindow> GetWindows => uiWindows.Values.AsReadOnly();

        // Queue for async continuations to run on the main thread
        private ConcurrentQueue<Action> mainThreadQueue = new ConcurrentQueue<Action>();

        // Track pending async operations (for debugging/monitoring)
        //private int pendingAsyncOperations = 0;


        //private JoinableTaskFactory joinableTaskFactory;


        public UIManager(Game game)
        {
            this.game = game;

            // joinableTaskFactory = new JoinableTaskFactory(new JoinableTaskContext());
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

        public void Add(UIWindow ui, int? priority = null)
        {
            AddWindow(ui, priority);
            HandleTaskQueue();
        }

        public void Add(ICollection<UIWindow> ui, int? priority = null)
        {
            foreach (var uiWindow in ui)
            {
                AddWindow(uiWindow, priority);
            }

            HandleTaskQueue();
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

        // Helper method to handle fire-and-forget with exception logging
        private void FireAndForget(Task task)
        {
            task.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    Debug.WriteLine($"Async error: {t.Exception?.InnerException?.Message}");
                }
            }, TaskContinuationOptions.OnlyOnFaulted);
        }


        /// <summary>
        /// SYNCHRONOUS Update - never blocks, async operations are fire-and-forget
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            // Process any queued main thread actions
            ProcessMainThreadQueue();

            // Handle window add/remove queue
            HandleTaskQueue();

            UIWindow eventLockedWindow = null;
            UIComponent eventLockedControl = null;

            // First check if events are locked to a control in any window
            foreach (var ui in uiWindows)
            {
                if (ui.Value.EventLockedControl != null)
                {
                    eventLockedWindow = ui.Value;
                    eventLockedControl = ui.Value.EventLockedControl;
                    break;
                }
            }

            // Check if events are locked to a specific control
            for (int i = uiWindows.Count - 1; i >= 0; i--)
            {
                var ui = uiWindows.Values[i];
                ui.AreEventsLockedToControl = (eventLockedWindow != null);
            }

            // Handle input - fire-and-forget async operations
            // These methods should NOT be awaited in the game loop
            FireAndForget(HandleKeyboardInputAsync(eventLockedWindow, eventLockedControl, true, gameTime));
            FireAndForget(HandleMouseInputAsync(eventLockedWindow, eventLockedControl, true, gameTime));
            FireAndForget(HandleTouchInputAsync(eventLockedWindow, eventLockedControl, true, gameTime));
            FireAndForget(HandleGamePadInputAsync(eventLockedWindow, eventLockedControl, true, gameTime));

            // Arrange layout (synchronous)
            foreach (var ui in uiWindows)
            {
                if (ui.Value.Visible.Value == Visibility.Visible)
                {
                    ui.Value.PerformLayout(gameTime);
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            //base.Render(spriteBatch, gameTime);

            using (SpriteBatch sb = new SpriteBatch(graphicsDevice))
            {
                try
                {
                    //uiSemaphore.Wait();

                    foreach (var ui in uiWindows)
                    {
                        if (ui.Value.Visible.Value == Visibility.Visible)
                        {
                            ui.Value.RenderLayout(gameTime);
                            //ui.RenderLayout(graphicsDevice.Viewport.Bounds);
                        }
                    }
                }
                finally
                {
                    //uiSemaphore.Release();
                }

                // Outline control we're hovering over
                if (RenderControlHitBoxes)
                {
                    sb.Begin();
                    foreach (var control in HoverIterator)
                    {
                        //Primitives2D.DrawRect(sb, control.clippingRect, Color.Green);
                        Primitives2D.DrawRect(sb, control.FinalContentRect, Color.Blue);
                        Primitives2D.DrawRect(sb, control.FinalRect, Color.Red);

                        //if (control.MouseHover.Value) { Primitives2D.DrawCircle(sb, control.FinalRect.Left, control.FinalRect.Top, 7, Color.Yellow); }
                        //if (control.HasFocus.Value) { Primitives2D.FillCircle(sb, control.FinalRect.Left, control.FinalRect.Top, 5, Color.White); } 
                    }
                    sb.End();
                }

            }
        }


        internal UIComponent SelectFirst(Func<UIComponent, UIComponent, bool> selector, bool useHitTest, Point? point = null)
        {
            // Select control at the given X,Y co-ordinate, in reverse order
            for (int i = uiWindows.Count - 1; i >= 0; i--)
            {
                var ui = uiWindows.Values[i];

                if (point != null && !ui.FinalRect.Contains(point.Value))
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

            // Despatch events to all windows, in reverse order
            for (int i = uiWindows.Count - 1; i >= 0; i--)
            {
                var ui = uiWindows.Values[i];

                if (point != null && !ui.FinalRect.Contains(point.Value))
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

            foreach (var window in uiWindows)
            {
                window.Value.Dispose();
            }

        }

    }
}
