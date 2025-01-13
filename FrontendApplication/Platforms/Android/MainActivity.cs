using Android.App;
using Android.Content.PM;
using Android.OS;
using Android;
using Android.Content;
using Android.Runtime;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Plugin.Firebase.CloudMessaging;
using Plugin.Firebase.DynamicLinks;

namespace FrontendApplication
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            HandleIntent(Intent);
            CreateNotificationChannelIfNeeded();
        }

        private static void HandleIntent(Intent intent)
        {
            FirebaseCloudMessagingImplementation.OnNewIntent(intent);
            _ = FirebaseDynamicLinksImplementation.HandleDynamicLinkAsync(intent);
        }

        private void RequestPushNotificationsPermission()
        {
            if(Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu && ContextCompat.CheckSelfPermission(this, Manifest.Permission.PostNotifications) != Permission.Granted) {
                ActivityCompat.RequestPermissions(this, new[] { Manifest.Permission.PostNotifications }, 0); ;
            }
        }

        private void CreateNotificationChannelIfNeeded()
        {
            if(Build.VERSION.SdkInt >= BuildVersionCodes.O) {
                CreateNotificationChannel();
            }
        }

        private void CreateNotificationChannel()
        {
            var channelId = $"{PackageName}.general";
            var notificationManager = (NotificationManager) GetSystemService(NotificationService);
            var channel = new NotificationChannel(channelId, "General", NotificationImportance.Default);
            notificationManager.CreateNotificationChannel(channel);
            FirebaseCloudMessagingImplementation.ChannelId = channelId;
            FirebaseCloudMessagingImplementation.SmallIconRef = Resource.Drawable.notification_bg_normal;
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            HandleIntent(intent);
        }
    }
}