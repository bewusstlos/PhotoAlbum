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
using Android.Graphics;

namespace PhotoAlbum
{
    [Activity(Label = "", MainLauncher = false, Icon = "@drawable/icon", Theme = "@android:style/Theme.Material.NoActionBar")]
    class FullscreenPhotoView : Activity
    {
        //PhotosManager pm;
        ImageView img;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            int photoPath = Intent.GetIntExtra("PhotoId", 0);

            SQLite.Net.Interop.ISQLitePlatform s = new SQLite.Net.Platform.XamarinAndroid.SQLitePlatformAndroid();
            string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "photos.db");

            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.fullscreen_photo_view);

            img = FindViewById<ImageView>(Resource.Id.photo);
            
            
           // img.SetImageBitmap(BitmapFactory.DecodeFile(pm.GetPhotoPath(photoPath)));
        }
    }
}