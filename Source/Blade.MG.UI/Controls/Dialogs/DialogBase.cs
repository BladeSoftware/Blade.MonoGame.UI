using Blade.UI.Models;

namespace Blade.UI.Controls.Dialogs
{
    public class DialogBase : ModalBase
    {
        public List<DialogButton> Buttons { get; set; }
        public DialogResult Result { get; set; }

        public Action<DialogResult> OnClose { get; set; }
        public Func<DialogResult, Task> OnCloseAsync { get; set; }


        protected async Task DialogButtonPressedAsync(string buttonId)
        {
            CloseModal();

            Result = new DialogResult
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
