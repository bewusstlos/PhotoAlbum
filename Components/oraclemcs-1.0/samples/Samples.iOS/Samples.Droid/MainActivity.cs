using System;
using System.Reflection;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Gcm;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Oracle.Cloud.Mobile.Configuration;
using Oracle.Cloud.Mobile.MobileBackend;
using PushNotification.Plugin;

[assembly: Permission(Name = "@PACKAGE_NAME@.permission.C2D_MESSAGE")]
[assembly: UsesPermission(Name = "android.permission.WAKE_LOCK")]
[assembly: UsesPermission(Name = "@PACKAGE_NAME@.permission.C2D_MESSAGE")]
[assembly: UsesPermission(Name = "com.google.android.c2dm.permission.RECEIVE")]
[assembly: UsesPermission(Name = "android.permission.GET_ACCOUNTS")]
[assembly: UsesPermission(Name = "android.permission.INTERNET")]


// Gives the app permission to register and receive messages.
[assembly: UsesPermission(Name = "com.google.android.c2dm.permission.RECEIVE")]

// Needed to keep the processor from sleeping when a message arrives
[assembly: UsesPermission(Name = "android.permission.WAKE_LOCK")]
[assembly: UsesPermission(Name = "android.permission.RECEIVE_BOOT_COMPLETED")]

namespace Samples.Droid
{

    [Activity(Label = "Samples", Icon = "@drawable/icon", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            var json = ResourceLoader.GetEmbeddedResourceStream(Assembly.GetAssembly(typeof(MainActivity)), "McsConfiguration.json");
            MobileBackendManager.Manager.Configuration = new MobileBackendManagerConfiguration(json);
            CrossPushNotification.Initialize<NotificationsListener>("<ANDROID SENDER ID>");
            LoadApplication(new Samples.App());
        }
    }
}

