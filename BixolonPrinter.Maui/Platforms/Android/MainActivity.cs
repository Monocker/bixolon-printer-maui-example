using Android.App;
using Android.Content.PM;
using Android.OS;

namespace BixolonPrinter.Maui;

[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation |
                          ConfigChanges.UiMode | ConfigChanges.ScreenLayout |
                          ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override async void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        await HandlePermissions();
    }

    private async Task HandlePermissions()
    {
        var permissions = new List<Permissions.BasePermission>();

        if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
        {
            permissions.Add(new Permissions.Bluetooth());
        }
        else
        {
            permissions.Add(new Permissions.LocationWhenInUse());
        }

        foreach (var permission in permissions)
        {
            var status = await permission.CheckStatusAsync();
            if (status != PermissionStatus.Granted)
            {
                await permission.RequestAsync();
            }
        }
    }
}
