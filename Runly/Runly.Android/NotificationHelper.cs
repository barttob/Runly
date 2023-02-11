using System;
using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using Java.IO;

namespace Runly.Droid
{
    internal class NotificationHelper
    {
        private static string foregroundChannelId = "9001";
        private static Context context = global::Android.App.Application.Context;


        public Notification GetServiceStartedNotification()
        {
            var intent = new Intent(context, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.SingleTop);
            intent.PutExtra("Title", "Message");

            PendingIntent pendingIntent;
            pendingIntent = PendingIntent.GetActivity(context, 0, intent, PendingIntentFlags.Immutable);
            /*if (global::Android.OS.Build.VERSION.SdkInt >= BuildVersionCodes.S)
            {
                pendingIntent = PendingIntent.GetActivity(context, 0, intent, PendingIntentFlags.Immutable);
            }
            else
            {
                pendingIntent = PendingIntent.GetActivity(context, 0, intent, PendingIntentFlags.UpdateCurrent);
            }*/

            var notificationBuilder = new NotificationCompat.Builder(context, foregroundChannelId)
                .SetContentTitle("Runly")
                .SetContentText("Tracking steps")
                .SetSmallIcon(Resource.Drawable.icon_steps)
                .SetOngoing(true)
                .SetContentIntent(pendingIntent);

            if (global::Android.OS.Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                NotificationChannel notificationChannel = new NotificationChannel(foregroundChannelId, "Title", NotificationImportance.High);
                notificationChannel.Importance = NotificationImportance.High;
                notificationChannel.EnableLights(true);
                notificationChannel.EnableVibration(true);
                notificationChannel.SetShowBadge(true);
                notificationChannel.SetVibrationPattern(new long[] { 100, 200, 300 });

                var notificationManager = context.GetSystemService(Context.NotificationService) as NotificationManager;
                if (notificationManager != null)
                {
                    notificationBuilder.SetChannelId(foregroundChannelId);
                    notificationManager.CreateNotificationChannel(notificationChannel);
                }
            }

            return notificationBuilder.Build();
        }
    }
}