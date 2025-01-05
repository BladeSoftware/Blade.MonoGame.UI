using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Microsoft.Xna.Framework;

namespace Examples.Android
{
    [Activity(
        Label = "@string/app_name",
        MainLauncher = true,
        //Icon = "@drawable/icon",
        AlwaysRetainTaskState = true,
        LaunchMode = LaunchMode.SingleInstance,
        ScreenOrientation = ScreenOrientation.FullUser,   
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize
    )]
    public class Activity1 : AndroidGameActivity
    {
        private TestGame _game;
        private View _view;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            //using (var game = new TestGame())
            //    game.Run();

            _game = new TestGame();

            _view = _game.Services.GetService(typeof(View)) as View;

            SetContentView(_view);
            _game.Run();

        }

        protected override void OnResume()
        {
            base.OnResume();

            // When we resume (which also seems to happen on startup), hide the system UI to go to full screen mode.
            HideSystemUI();
        }


        private void HideSystemUI()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
            {
                View decorView = Window.DecorView;
                if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
                {
                    decorView.WindowInsetsController?.SetSystemBarsAppearance((int)SystemUiFlags.Immersive, (int)SystemUiFlags.Immersive);
                }
                else
                {
                    decorView.SystemUiVisibility = (StatusBarVisibility)(SystemUiFlags.Immersive | SystemUiFlags.Fullscreen | SystemUiFlags.HideNavigation | SystemUiFlags.ImmersiveSticky);
                }
                this.Immersive = true;
            }
        }
    }
}
