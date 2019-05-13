using Android.App;
using Android.Content.PM;
using Android.Views;
using Android.OS;

namespace CustomKeyboard.Droid
{
    [Activity(
        Label = "CustomKeyboard",
        Icon = "@mipmap/icon",
        Theme = "@style/MainTheme",
        MainLauncher = true,
        ScreenOrientation = ScreenOrientation.Portrait,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation,
        WindowSoftInputMode = SoftInput.StateAlwaysHidden,
        LaunchMode = LaunchMode.SingleTask)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            LoadApplication(new App());

            Window.SetSoftInputMode(SoftInput.StateAlwaysHidden);
        }
    }
}