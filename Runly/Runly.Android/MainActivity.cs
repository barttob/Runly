using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Android;
using Xamarin.Forms.Platform.Android;
using Android.Content;
using Xamarin.Forms;
using Plugin.LocalNotification;

namespace Runly.Droid
{
    [Activity(Label = "Runly", 
        Icon = "@mipmap/ic_launcher",
        Theme = "@style/MainTheme.Launcher",
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        const int RequestLocationId = 0;
        Intent serviceIntent;
        private const int RequestCode = 5469;

        readonly string[] LocationPermissions =
        {
            Manifest.Permission.AccessCoarseLocation,
            Manifest.Permission.AccessFineLocation
        };

        protected override void OnStart()
        {
            base.OnStart();

            if ((int)Build.VERSION.SdkInt >= 23)
            {
                if (CheckSelfPermission(Manifest.Permission.AccessFineLocation) != Permission.Granted)
                {
                    RequestPermissions(LocationPermissions, RequestLocationId);
                }
                else
                {
                    // Permissions already granted - display a message.
                }
            }
        }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            this.SetStatusBarColor(Xamarin.Forms.Color.FromHex("#192126").ToAndroid());

            LocalNotificationCenter.CreateNotificationChannel();

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            Xamarin.FormsMaps.Init(this, savedInstanceState);

            serviceIntent = new Intent(this, typeof(StepCounterService));
            SetServiceMethods();

            LoadApplication(new App());
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {

            if (requestCode == RequestLocationId)
            {
                if ((grantResults.Length == 1) && (grantResults[0] == (int)Permission.Granted))
                {
                    // Permissions granted - display a message.
                }
                else
                {
                    // Permissions denied - display a message.
                }
            }
            else
            {
                Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            }
        }

        void SetServiceMethods()
        {
            if (!IsServiceRunning(typeof(StepCounterService)))
            {
                if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.S)
                {
                    StartForegroundService(serviceIntent);
                }
                else
                {
                    StartService(serviceIntent);
                }
            }

            /*MessagingCenter.Subscribe<StartServiceMessage>(this, "ServiceStarted", message => {
                System.Console.WriteLine("Working");
                //if (!IsServiceRunning(typeof(StepCounterService)))
                //{
                
                //}
            });

            MessagingCenter.Subscribe<StopServiceMessage>(this, "ServiceStopped", message => {
                if (IsServiceRunning(typeof(StepCounterService)))
                    StopService(serviceIntent);
            });*/
        }

        private bool IsServiceRunning(System.Type cls)
        {
            //Console.WriteLine("Working");
            ActivityManager manager = (ActivityManager)GetSystemService(Context.ActivityService);
            foreach (var service in manager.GetRunningServices(int.MaxValue))
            {
                if (service.Service.ClassName.Equals(Java.Lang.Class.FromType(cls).CanonicalName))
                {
                    return true;
                }
            }
            return false;
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (requestCode == RequestCode)
            {
                if (Android.Provider.Settings.CanDrawOverlays(this))
                {

                }
            }

            base.OnActivityResult(requestCode, resultCode, data);
        }
    }
}