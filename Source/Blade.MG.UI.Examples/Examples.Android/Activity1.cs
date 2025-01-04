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

    }
}
