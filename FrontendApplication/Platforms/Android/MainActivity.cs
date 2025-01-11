using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Util;

namespace FrontendApplication
{
    [Activity(Theme = "@style/Maui.SplashTheme",MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // This is a line where you can set a breakpoint
            Log.Debug("MainActivity", "MainActivity is starting...");

            // Add other initialization code here if needed
        }
    }
}