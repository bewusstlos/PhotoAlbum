using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace PhotoAlbum
{
    [Activity(Label = "Add Label...", MainLauncher = false, Icon = "@drawable/icon", Theme ="@android:style/Theme.Material.Light.Dialog")]
    public class AddChangeLabel : Activity
    {
        EditText ETLabelName;
        Button BOK;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SQLite.Net.Interop.ISQLitePlatform s = new SQLite.Net.Platform.XamarinAndroid.SQLitePlatformAndroid();
            string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "photos.db");

            RedisServer.RedisManager rm = new RedisServer.RedisManager();

           

            SetContentView(Resource.Layout.add_change_label);
            ETLabelName = FindViewById<EditText>(Resource.Id.edit_label);
            BOK = FindViewById<Button>(Resource.Id.label_ok);
            BOK.Click += delegate
            {
                rm.AddLabel(ETLabelName.Text);
                rm.SerializeLabels();
                this.Finish();
            };
        }
    }
}