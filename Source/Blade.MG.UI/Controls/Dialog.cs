//using Blade.Games;
//using Blade.UI.Controls.Dialogs;
//using Microsoft.VisualStudio.Threading;
//using Microsoft.Xna.Framework;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Blade.UI.Controls
//{
//    public class Dialog
//    {
//        public static void ShowDialog(Game game, MessageDialog dialogWindow)
//        {
//            UIManager.Add(dialogWindow, game);
//        }


//        public static async Task ShowDialogAsync(Game game, MessageDialog dialogWindow)
//        {
//            AsyncManualResetEvent asyncManualResetEvent = new AsyncManualResetEvent(false);

//            var onClose = dialogWindow.OnClose;
//            dialogWindow.OnClose = (result) => { asyncManualResetEvent.Set(); };

//            UIManager.Add(dialogWindow, game);

//            await asyncManualResetEvent.WaitAsync();
//        }

//    }
//}
