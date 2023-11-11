using Blade.MG.Primitives;
using Blade.MG.UI.Components;
using Blade.MG.UI.Theming;
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
        public int Priority;
    }

    public partial class UIManager : GameEntity //,UIManagerBase
    {
        public static bool RenderControlHitBoxes = false;

        //private static SemaphoreSlim uiSemaphore = new SemaphoreSlim(1);

        public static UITheme DefaultTheme { get; set; } = DefaultThemes.LightTheme();

        //public static Rectangle SafeLayoutRect = Instance.game.GraphicsDevice.PresentationParameters.IsFullScreen ? Instance.game.TitleSafeArea : Instance.game.Viewport.Bounds;

        public static UIManager Instance { get; set; } = new UIManager();

        protected ConcurrentQueue<UITask> uiTaskQueue = new ConcurrentQueue<UITask>();

        // List of Windows sorted by Priority
        // Windows with a higher priority are on top of windows with lower priorities
        private SortedList<string, UIWindow> uiWindows { get; set; } = new();


        protected UIManager()
        {
        }

        private string ToPriority(int i) => $"{i.ToString("0000")}.{Stopwatch.GetTimestamp()}";

        public void EnqueTask(UITask task)
        {
            uiTaskQueue.Enqueue(task);
        }

        protected void HandleTaskQueue()
        {
            while (uiTaskQueue.TryDequeue(out var task))
            {
                if (task.TaskType == UITaskType.Add)
                {
                    uiWindows.Add(ToPriority(Math.Clamp(task.Priority, 0, 9999)), task.Window);
                }
                else if (task.TaskType == UITaskType.Remove)
                {

                    //Instance.UI.Remove(task.Window);
                    int index = Instance.uiWindows.IndexOfValue(task.Window);
                    if (index >= 0)
                    {
                        Instance.uiWindows.RemoveAt(index);
                    }

                    UnloadWindow(task.Window);
                }
                else if (task.TaskType == UITaskType.Clear)
                {
                    for (int i = Instance.uiWindows.Count - 1; i >= 0; i--)
                    {
                        var window = Instance?.uiWindows?.ElementAtOrDefault(i);
                        Instance?.uiWindows?.RemoveAt(i);
                        UnloadWindow(window.Value.Value);
                    }
                }
                else
                {
                }
            }

        }

        public static void Clear()
        {
            Instance.EnqueTask(new UITask { TaskType = UITaskType.Clear, Window = null });
        }

        public static void Add(UIWindow ui, int priority = 100)
        {
            Instance.EnqueTask(new UITask { TaskType = UITaskType.Add, Window = ui, Priority = priority });
            
            ui.Initialize(Instance.game);
            ui.LoadContent();

            Instance.HandleTaskQueue();
        }

        public static void Add(ICollection<UIWindow> ui, int priority = 100)
        {
            foreach (var uiWindow in ui)
            {
                Instance.EnqueTask(new UITask { TaskType = UITaskType.Add, Window = uiWindow, Priority = priority });

                uiWindow.Initialize(Instance.game);
                uiWindow.LoadContent();
            }

            Instance.HandleTaskQueue();
        }

        public static T Find<T>() where T : UIWindow
        {
            return (T)Instance.uiWindows.Values.FirstOrDefault(p => p.GetType() == typeof(T));
        }

        public static T Remove<T>() where T : UIWindow
        {
            var window = (T)Instance.uiWindows.Values.FirstOrDefault(p => p.GetType() == typeof(T));
            if (window != null)
            {
                Instance.EnqueTask(new UITask { TaskType = UITaskType.Remove, Window = window });
            }

            return window;
        }

        public static T Remove<T>(T instance) where T : UIWindow
        {
            var window = (T)Instance.uiWindows.Values.FirstOrDefault(p => p == instance);
            if (window != null)
            {
                Instance.EnqueTask(new UITask { TaskType = UITaskType.Remove, Window = window });
            }

            return window;
        }

        public static void RemoveAll<T>() where T : UIWindow
        {

            for (int i = Instance.uiWindows.Count - 1; i >= 0; i--)
            {
                var window = Instance?.uiWindows?.Values.ElementAtOrDefault(i);

                if (window?.GetType() == typeof(T))
                {
                    Instance.EnqueTask(new UITask { TaskType = UITaskType.Remove, Window = window });
                }
            }

        }

        public static void RemoveOthers<T>() where T : UIWindow
        {

            for (int i = Instance.uiWindows.Count - 1; i >= 0; i--)
            {
                if (Instance.uiWindows.Values[i] != null && Instance.uiWindows.Values[i].GetType() != typeof(T))
                {
                    var window = Instance.uiWindows.Values.ElementAt(i);

                    Instance.EnqueTask(new UITask { TaskType = UITaskType.Remove, Window = window });
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

        public override async void Update(GameTime gameTime)
        {
            try
            {
                await UpdateAsync(gameTime).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
            }
        }

        public async Task UpdateAsync(GameTime gameTime)
        {
            //base.Logic(gameTime);

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
                    //ui.Logic(gameTime);
                    break;
                }
            }

            // Check if events are locked to a specific control, e.g. scrollbar
            for (int i = uiWindows.Count - 1; i >= 0; i--)
            {
                var ui = uiWindows.Values[i];

                ui.AreEventsLockedToControl = (eventLockedWindow != null);
            }

            // Track focused elements and handle keys
            // Converts physical input into 'abstracted' actions
            await HandleKeyboardInputAsync(eventLockedWindow, eventLockedControl, true, gameTime);
            await HandleMouseInputAsync(eventLockedWindow, eventLockedControl, true, gameTime);
            await HandleGamePadInputAsync(eventLockedWindow, eventLockedControl, true, gameTime);

            // Arrange layout
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


        internal UIComponent SelectFirst(Func<UIComponent, UIComponent, bool> selector, bool useHitTest, Point point = default)
        {
            // Select control at the given X,Y co-ordinate, in reverse order
            for (int i = uiWindows.Count - 1; i >= 0; i--)
            {
                var ui = uiWindows.Values[i];

                var component = ui.SelectFirst(selector, useHitTest, point);
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

        private async Task DispatchEventAsync(UIWindow eventLockedWindow, Func<UIWindow, Task> action)
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

    }
}
