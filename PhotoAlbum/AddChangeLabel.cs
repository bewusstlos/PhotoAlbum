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
using PhotoManager;

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

            PhotosManager pm = new PhotosManager(s, path);

            if (System.IO.File.Exists(path))
            {
                pm = new PhotosManager(s, path);
            }
            else
            {
                pm = new PhotosManager(s, path, true);
            }

            SetContentView(Resource.Layout.add_change_label);
            ETLabelName = FindViewById<EditText>(Resource.Id.edit_label);
            BOK = FindViewById<Button>(Resource.Id.label_ok);
            BOK.Click += delegate
            {
                pm.AddLabel(ETLabelName.Text);
                this.Finish();
            };
        }
    }
}