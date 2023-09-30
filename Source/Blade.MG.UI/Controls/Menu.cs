using Blade.MG.Input;
using Blade.UI.Components;
using Blade.UI.Events;
using Blade.UI.Models;
using Microsoft.Xna.Framework;

namespace Blade.UI.Controls
{
    public class Menu : ModalBase
    {
        public List<IMenuOption> Options { get; set; }

        public MenuResult Result { get; set; }

        public Action<MenuResult> OnClose { get; set; }
        public Func<MenuResult, Task> OnCloseAsync { get; set; }


        public override void LoadContent()
        {
            base.LoadContent();

            BuildScreen(Game);
        }

        public void BuildScreen(Game game)
        {
            HitTestVisible = true;

            var border1 = new Border
            {
                BorderColor = Color.DarkBlue,
                BorderThickness = 2f,

                Background = Color.White,

                HorizontalAlignment = HorizontalAlignmentType.Absolute,
                VerticalAlignment = VerticalAlignmentType.Absolute,


                Left = InputManager.MouseState.X,
                Top = InputManager.MouseState.Y,
                //Width = 200,
                //Height = 350

            };

            AddChild(border1);


            var optionsStackPanel = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                HorizontalScrollBarVisible = false,
                VerticalScrollBarVisible = false,
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
                                OnClickAsync = async (sender, uiEvent) => { await OptionSelectedAsync(menuOption.Id); }
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

                    };

                }
            }

        }

        protected override void RenderLayout(Rectangle layoutRect)
        {
            base.RenderLayout(layoutRect);
        }

        public override async Task HandleClickEventAsync(UIWindow uiWindow, UIClickEvent uiEvent)
        {
            CloseModal();

            Result = new MenuResult
            {
                Id = null,
                Data = null
            };

            OnClose?.Invoke(Result);
            await (OnCloseAsync?.Invoke(Result) ?? Task.CompletedTask);

            ReturnAsyncResult();

            await base.HandleClickEventAsync(uiWindow, uiEvent);
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
                Data = null
            };

            OnClose?.Invoke(Result);
            await (OnCloseAsync?.Invoke(Result) ?? Task.CompletedTask);

            ReturnAsyncResult();
        }

    }
}
