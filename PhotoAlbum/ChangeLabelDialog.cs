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
    class ChangeLabelDialog : Activity
    {
        Spinner selector;
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

            SetContentView(Resource.Layout.select_label);
            selector = FindViewById<Spinner>(Resource.Id.spinner);

            List<Label> labels =  pm.GetLabelsToList();
            foreach(Label label in labels)
            {
                TextView TVLabel = new TextView(this);
                TVLabel.Id = label.Id;
                TVLabel.Text = label.LabelName;

                selector.AddView(TVLabel);
            }
        }
    }
}