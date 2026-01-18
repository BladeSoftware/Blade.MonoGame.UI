using Blade.MG.Input;
using Blade.MG.UI.Components;
using Blade.MG.UI.Events;
using Blade.MG.UI.Models;
using Microsoft.Xna.Framework;

namespace Blade.MG.UI.Controls
{
    public class Menu : ModalBase
    {
        public List<IMenuOption> Options { get; set; }

        private MenuResult Result { get; set; }

        public Action<MenuResult> OnClose { get; set; }
        public Func<MenuResult, Task> OnCloseAsync { get; set; }


        public override void LoadContent()
        {
            base.LoadContent();

            BuildScreen(Game);
        }

        public void BuildScreen(Game game)
        {
            // Add a panel that fills the screen and blocks input to underlying controls
            var panel = new Panel
            {
                IsHitTestVisible = true,
                //Background = new Color(Color.DarkGray, 0.75f),
                Background = Color.Transparent,
                Width = new Length(100, LengthUnit.Percent),
                Height = new Length(100, LengthUnit.Percent),
                HorizontalAlignment = HorizontalAlignmentType.Stretch,
                VerticalAlignment = VerticalAlignmentType.Stretch
            };

            base.AddChild(panel);

            var border1 = new Border
            {
                BorderColor = Color.DarkBlue,
                BorderThickness = new Thickness(2f),

                Background = Color.White,

                HorizontalAlignment = HorizontalAlignmentType.Absolute,
                VerticalAlignment = VerticalAlignmentType.Absolute,


                Left = InputManager.Mouse.X,
                Top = InputManager.Mouse.Y,
                //Width = 200,
                //Height = 350

            };

            panel.AddChild(border1);


            var optionsStackPanel = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                HorizontalScrollBarVisible = ScrollBarVisibility.Hidden,
                VerticalScrollBarVisible = ScrollBarVisibility.Hidden,
            };

            border1.Content = optionsStackPanel;

            if (Options != null)
            {
                foreach (var option in Options)
                {
                    switch (option)
                    {
                        case MenuOption menuOption:
                            optionsStackPanel.AddChild(new MenuItem
                            {
                                OnPrimaryClickAsync = async (sender, uiEvent) => { await OptionSelectedAsync(menuOption.Id); uiEvent.Handled = true; }
                            },
                            this,
                            option);

                            break;

                        case MenuSeparator menuSeparator:
                            optionsStackPanel.AddChild(new MenuItemSeparator
                            {
                                Height = 1,
                                Margin = new Thickness(0, 2),
                                Background = Color.White
                            },
                            this,
                            option);

                            break;

                    }
                }
            }

        }

        protected override void RenderLayout(Rectangle layoutRect)
        {
            base.RenderLayout(layoutRect);
        }

        public new async Task<MenuResult> ShowAsync(Game game)
        {
            await base.ShowAsync(game);

            return Result;
        }

        public override async Task HandleMouseClickEventAsync(UIWindow uiWindow, UIClickEvent uiEvent)
        {
            // Call base first to let children handle the event (menu items)
            await base.HandleMouseClickEventAsync(uiWindow, uiEvent);

            // If a menu item handled the event, don't close the menu here
            if (uiEvent.Handled)
            {
                return;
            }

            // Click was outside menu items (on the transparent panel), close without selecting
            CloseModal();

            Result = new MenuResult
            {
                Id = null,
                Data = null,
                Cancelled = true
            };

            OnClose?.Invoke(Result);
            await (OnCloseAsync?.Invoke(Result) ?? Task.CompletedTask);

            ReturnAsyncResult();

            // Mark as handled to prevent further propagation
            uiEvent.Handled = true;
        }

        protected internal void OnMenuItemClicked(MenuItem menuItem)
        {

        }

        protected async Task OptionSelectedAsync(string buttonId)
        {
            CloseModal();

            Result = new MenuResult
            {
                Id = buttonId,
                Data = null,
                Cancelled = false
            };

            OnClose?.Invoke(Result);
            await (OnCloseAsync?.Invoke(Result) ?? Task.CompletedTask);

            ReturnAsyncResult();
        }

    }
}
