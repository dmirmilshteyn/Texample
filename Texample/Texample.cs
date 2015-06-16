using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Content.PM;

namespace Texample
{
    [Activity(MainLauncher = true,
        Icon = "@drawable/icon",
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden
#if __ANDROID_11__
        , HardwareAccelerated = false
#endif
        )]
    public class Texample : Activity
    {
        TexampleView view;

        protected override void OnCreate(Bundle bundle) {
            base.OnCreate(bundle);
            RequestWindowFeature(WindowFeatures.NoTitle);
            Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);

            view = new TexampleView(this);
            SetContentView(view);
        }

        protected override void OnPause() {
            base.OnPause();
            view.Pause();
        }

        protected override void OnResume() {
            base.OnResume();
            view.Resume();
        }
    }
}

