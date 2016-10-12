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
    [Activity(Label = "Select Label:", MainLauncher = false, Icon = "@drawable/icon", Theme = "@android:style/Theme.Material.Light.Dialog")]
    class ChangeLabelDialog : Activity
    {
        LinearLayout selector;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SQLite.Net.Interop.ISQLitePlatform s = new SQLite.Net.Platform.XamarinAndroid.SQLitePlatformAndroid();
            string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "photos.db");

            RedisServer.RedisManager rm = new RedisServer.RedisManager();

            

            int photoId = Intent.GetIntExtra("PhotoId", 0);

            SetContentView(Resource.Layout.select_label);
            selector = FindViewById<LinearLayout>(Resource.Id.spinner);

            List<RedisServer.RedisManager.Label> labels =  rm.GetLabelsToList();
            foreach(RedisServer.RedisManager.Label label in labels)
            {
                TextView TVLabel = new TextView(this);
                LinearLayout.LayoutParams lpForTVLabel = new LinearLayout.LayoutParams(-2, -2);
                TVLabel.TextAlignment = TextAlignment.Center;
                lpForTVLabel.SetMargins(20, 20, 20, 0);
                TVLabel.Id = label.Id;
                TVLabel.Text = label.LabelName;
                TVLabel.TextSize = 24;
                TVLabel.Click += delegate
                    {
                       rm.ChangePhotoLabel(photoId, label.Id);
                        rm.SerializeLabels();
                        rm.SerializePhotos();
                       this.Finish();
                    };
                selector.AddView(TVLabel, lpForTVLabel);
            }
        }
    }
}