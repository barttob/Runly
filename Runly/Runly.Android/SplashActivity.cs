using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Runtime;
using Android.Widget;
using Java.Lang;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runly.Droid
{
    [Activity(Label = "Runly",
        MainLauncher=true,
        Theme ="@style/MainTheme.Splash",
        NoHistory = true,
        Icon ="@mipmap/ic_launcher")]
    public class SplashActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            //AnimationViewRenderer.Init();

            // Create your application here
        }

        protected override void OnResume()
        {
            base.OnResume();
            //SetContentView(Resource.Layout.splash);
            Task startupWork = new Task(() => { SimulateStartupAsync(); });
            startupWork.Start();
        }

        private async Task SimulateStartupAsync()
        {
            await Task.Delay(millisecondsDelay:1000);

            StartActivity(intent: new Intent(Application.Context, typeof(MainActivity)));
        }
    }
}