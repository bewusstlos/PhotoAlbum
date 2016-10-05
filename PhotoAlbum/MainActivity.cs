using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using Android.Widget;
using Java.IO;
using Android.Views.Animations;

using PhotoManager;
using Environment = Android.OS.Environment;
using Uri = Android.Net.Uri;
using Android.Views;

namespace PhotoAlbum
{
    public static class App
    {
        public static File _file;
        public static File _dir;
        public static Bitmap bitmap;
    }

    [Activity(Label = "PhotoAlbum", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        PhotosManager pm;
        LinearLayout LLRootLayout;
        LinearLayout LLContextMenuOwner;

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Layout.ActionBarMenu, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.TitleFormatted.ToString())
            {
                case "Take Picture":
                    Intent intent = new Intent(MediaStore.ActionImageCapture);
                    App._file = new File(App._dir, String.Format("myPhoto_{0}.jpg", Guid.NewGuid()));
                    intent.PutExtra(MediaStore.ExtraOutput, Uri.FromFile(App._file));
                    StartActivityForResult(intent, 0);
                    break;
                case "Context Menu":
                    PopupMenu contextMenu = new PopupMenu(this, LLContextMenuOwner, GravityFlags.Right);
                    contextMenu.Inflate(Resource.Layout.context_menu);
                    contextMenu.MenuItemClick += (s, arg) =>
                    {
                        switch (arg.Item.TitleFormatted.ToString())
                        {
                            case "Add Label":
                                Intent i = new Intent(this, typeof(AddChangeLabel));
                                StartActivityForResult(i, 1);
                                break;
                        }
                    };
                    contextMenu.Show();
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SQLite.Net.Interop.ISQLitePlatform s = new SQLite.Net.Platform.XamarinAndroid.SQLitePlatformAndroid();
            string path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "photos.db");

            if (System.IO.File.Exists(path))
            {
                pm = new PhotosManager(s, path);
            }
            else
            {
                pm = new PhotosManager(s, path, true);
            }
            SetContentView(Resource.Layout.Main);

            CreateDirectoryForPictures();
            LLRootLayout = FindViewById<LinearLayout>(Resource.Id.RootLayout);
            LLContextMenuOwner = FindViewById<LinearLayout>(Resource.Id.context_menu_popuper);
            RefreshLayout(ref LLRootLayout, pm);

            pm.db.Close();
        }

        public void RefreshLayout(ref LinearLayout RootLayout, PhotosManager pm)
        {

            RootLayout.RemoveAllViews();
            List<Label> AllLabels =  pm.GetLabelsToList();
            List<Photo> PhotosForEachLabels;

            foreach(Label l in AllLabels)
            {
                int sw = (int)((float)Resources.DisplayMetrics.WidthPixels / 2.42f);

                PhotosForEachLabels = pm.GetPhotosOfLabelToList(l);

                LinearLayout LEachLabel = new LinearLayout(this);
                LinearLayout.LayoutParams lpForLEachLabel = new LinearLayout.LayoutParams(-1, -2);
                LEachLabel.Orientation = Orientation.Vertical;
                LEachLabel.Elevation = 10;
                lpForLEachLabel.SetMargins(20, 10, 20, 10);

                LinearLayout LEachLabelHeader = new LinearLayout(this);
                LinearLayout.LayoutParams lpForLEachLabelHeader = new LinearLayout.LayoutParams(-1, -2);
                LEachLabelHeader.SetBackgroundColor(Color.White);
                LEachLabelHeader.Orientation = Orientation.Horizontal;
                LEachLabelHeader.Id = l.Id;
                LEachLabelHeader.Elevation = 20;

                TextView TVLabelHeader = new TextView(this);
                TVLabelHeader.TextSize = 18;
                TVLabelHeader.SetTextColor(Color.Black);
                LinearLayout.LayoutParams lpForTVLabelHeader = new LinearLayout.LayoutParams(-1, -2);
                TVLabelHeader.TextAlignment = TextAlignment.Center;
                lpForTVLabelHeader.SetMargins(10, 10, 10, 10);
                TVLabelHeader.Text = l.LabelName;

                EditText ETLabelChange = new EditText(this);
                LinearLayout.LayoutParams lpForETLabelChange = new LinearLayout.LayoutParams(-2, -2, 4f);
                ETLabelChange.Visibility = ViewStates.Gone;

                Button BConfirmChange = new Button(this);
                LinearLayout.LayoutParams lpForBConfirmChange = new LinearLayout.LayoutParams(-2, -2, 1f);
                BConfirmChange.Text = "OK";
                BConfirmChange.Visibility = ViewStates.Gone;

                GridLayout LPhotosTable = new GridLayout(this);
                LPhotosTable.Orientation = GridOrientation.Horizontal;
                LinearLayout.LayoutParams lpForLPhotosTable = new LinearLayout.LayoutParams(-1, -2);
                lpForLPhotosTable.SetMargins(5, 0, 5, 0);
                LPhotosTable.Elevation = 10;
                LPhotosTable.ColumnCount = 2;
                LPhotosTable.RowCount = PhotosForEachLabels.Count/2 + 1;
                LPhotosTable.SetBackgroundColor(Color.White);

                LEachLabelHeader.Click += delegate
                {

                    if (LPhotosTable.Visibility == Android.Views.ViewStates.Visible)
                    {
                        
                        HideShowAnimation(ref LPhotosTable, LPhotosTable.RowCount, sw, false);
                        LPhotosTable.Visibility = Android.Views.ViewStates.Gone;
                    }
                    else
                    {   
                        LPhotosTable.Visibility = Android.Views.ViewStates.Visible;
                        HideShowAnimation(ref LPhotosTable, LPhotosTable.RowCount, sw, true);
                    } 
                };


                foreach (var image in PhotosForEachLabels)
                {
                    FrameLayout flForImg = new FrameLayout(this);
                    ImageView img = new ImageView(this);
                    img.SetImageBitmap(BitmapHelpers.LoadAndResizeBitmap(image.Path, sw, sw));
                    LinearLayout.LayoutParams lpForImg = new LinearLayout.LayoutParams(sw, sw);
                    img.SetScaleType(ImageView.ScaleType.CenterCrop);
                    img.Elevation = 5;
                    flForImg.SetPadding(sw / 19, sw / 19, sw / 19, sw / 19);
                    img.Id = image.Id;

                    img.Click +=(s,arg)=>
                    {
                        Intent i = new Intent(this, typeof(FullscreenPhotoView));
                        Intent.PutExtra("PhotoId", image.Id);
                        StartActivity(i);
                    };

                    img.LongClick += delegate
                    {
                        PopupMenu menu = new PopupMenu(this, img, GravityFlags.Top);
                        menu.Inflate(Resource.Layout.photo_context_menu);
                        menu.MenuItemClick += (s1, arg1)=>
                        {
                            switch (arg1.Item.TitleFormatted.ToString())
                            {
                                case "Change Label":
                                    Intent i = new Intent(this, typeof(ChangeLabelDialog));
                                    i.PutExtra("PhotoId", img.Id);
                                    StartActivityForResult(i, 2);
                                    break;
                                case "Delete":
                                    pm.DeletePhoto(img.Id);
                                    RefreshLayout(ref LLRootLayout, pm);
                                    break;
                            }
                        };
                        menu.Show();
                    };
                    flForImg.AddView(img, lpForImg);
                    LPhotosTable.AddView(flForImg);
                }

                LEachLabelHeader.AddView(TVLabelHeader, lpForTVLabelHeader);
                LEachLabelHeader.AddView(ETLabelChange, lpForETLabelChange);
                LEachLabelHeader.AddView(BConfirmChange,lpForBConfirmChange);
                LEachLabel.AddView(LEachLabelHeader, lpForLEachLabelHeader);
                LEachLabelHeader.LongClick += delegate
                {
                    if (LEachLabelHeader.Id != 0)
                    {
                        PopupMenu menu = new PopupMenu(this, LEachLabelHeader);
                        menu.Inflate(Resource.Layout.label_context_menu);
                        menu.MenuItemClick += (s, arg) =>
                        {
                            switch (arg.Item.TitleFormatted.ToString())
                            {
                                case "Rename Label":
                                    if(TVLabelHeader.Text != "Unsigned")
                                    {
                                        TVLabelHeader.Visibility = ViewStates.Gone;
                                        ETLabelChange.Visibility = ViewStates.Visible;
                                        BConfirmChange.Visibility = ViewStates.Visible;
                                    }
                                    
                                    break;
                                case "Delete":
                                    pm.DeleteLabelWithout(LEachLabelHeader.Id);
                                    RefreshLayout(ref LLRootLayout, pm);
                                    break;
                            }
                        };
                        menu.Show();
                    }
                };

                BConfirmChange.Click += delegate
                {
                    pm.ChangeLabel(LEachLabelHeader.Id, ETLabelChange.Text);
                    RefreshLayout(ref LLRootLayout, pm);
                };
                LEachLabel.AddView(LPhotosTable, lpForLPhotosTable);
                RootLayout.AddView(LEachLabel, lpForLEachLabel);

            }
        }

        public void HideShowAnimation(ref GridLayout g, int imageCount, int DisplayMetrics, bool isShow)
        {
            Animation anim = new TranslateAnimation(0, 0, -((DisplayMetrics + (DisplayMetrics / 19)) * imageCount), 0);
            Animation backAnim = new TranslateAnimation(0, 0, 0, (DisplayMetrics + (DisplayMetrics / 19)) * imageCount);
            anim.Duration = 500;
            if (isShow)
                g.StartAnimation(anim);
            else
                g.StartAnimation(backAnim);
        }

        private void CreateDirectoryForPictures()
        {
            App._dir = new File(
                Environment.GetExternalStoragePublicDirectory(
                    Environment.DirectoryPictures), "PhotoAlbum");
            if (!App._dir.Exists())
            {
                App._dir.Mkdirs();
            }
        }

        private bool IsThereAnAppToTakePictures()
        {
            Intent intent = new Intent(MediaStore.ActionImageCapture);
            IList<ResolveInfo> availableActivities =
                PackageManager.QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly);
            return availableActivities != null && availableActivities.Count > 0;
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            // Make it available in the gallery
            if(requestCode == 0)
            {
                Intent mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
                Uri contentUri = Uri.FromFile(App._file);
                pm.AddPhoto(App._file.Path);
                mediaScanIntent.SetData(contentUri);
                SendBroadcast(mediaScanIntent);

                // Display in ImageView. We will resize the bitmap to fit the display.
                // Loading the full sized image will consume to much memory
                // and cause the application to crash.




                int width = Resources.DisplayMetrics.WidthPixels / 2 - 30;
                int height = width;
                App.bitmap = App._file.Path.LoadAndResizeBitmap(width, height);
                if (App.bitmap != null)
                {
                    //_imageView.SetImageBitmap(App.bitmap);
                    App.bitmap = null;

                    // Dispose of the Java side bitmap.
                    GC.Collect();
                }
            }
            
            else if(requestCode == 1)
            {

            }

            else if(requestCode == 2)
            {

            }
            RefreshLayout(ref LLRootLayout, pm);
        }
    }
}

