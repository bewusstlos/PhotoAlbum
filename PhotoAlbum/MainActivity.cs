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
using RedisSharp;

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
        int sw;
        int photoRowCount = 2;

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

                            case "Change Row Count":
                                Intent k = new Intent(this, typeof(ChangeRowCount));
                                k.PutExtra("RowCount", photoRowCount);
                                StartActivityForResult(k, 3);
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
            sw = (int)((float)Resources.DisplayMetrics.WidthPixels -50);

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
            RefreshLayout(ref LLRootLayout, pm,photoRowCount);

        }

        public void RefreshLayout(ref LinearLayout RootLayout, PhotosManager pm, int photoRow)
        {
            RootLayout.RemoveAllViews();
            SetLayoutSpoilerStyle(ref RootLayout, pm,photoRow);
        }

        public void SetLayoutSpoilerStyle(ref LinearLayout RootLayout, PhotosManager pm, int photoRowCount)
        {
            List<Label> AllLabels = pm.GetLabelsToList();
            List<Photo> PhotosForEachLabels;

            foreach (Label l in AllLabels)
            {
                PhotosForEachLabels = pm.GetPhotosOfLabelToList(l);

                LinearLayout LEachLabel = new LinearLayout(this);
                LinearLayout.LayoutParams lpForLEachLabel = new LinearLayout.LayoutParams(-1, -2);
                LEachLabel.Orientation = Orientation.Vertical;
                LEachLabel.Elevation = 10;
                lpForLEachLabel.SetMargins(20, 10, 20, 10);

                LinearLayout LEachLabelHeader = new LinearLayout(this);
                LinearLayout.LayoutParams lpForLEachLabelHeader = new LinearLayout.LayoutParams(-1, -2);
                LEachLabelHeader.SetBackgroundColor(Color.ParseColor("#fff5f5f5"));
                LEachLabelHeader.Orientation = Orientation.Horizontal;

               


                LEachLabelHeader.Elevation = 20;

                TextView TVLabelHeader = new TextView(this);
                TVLabelHeader.TextSize = 18;
                TVLabelHeader.SetTextColor(Color.Black);
                LinearLayout.LayoutParams lpForTVLabelHeader = new LinearLayout.LayoutParams(-2, -2);
                TVLabelHeader.TextAlignment = TextAlignment.Center;
                TVLabelHeader.Id = l.Id;
                TVLabelHeader.Drag += HandleDrag;
                TVLabelHeader.Tag = l.LabelName;
                lpForTVLabelHeader.SetMargins(10, 10, 10, 10);
                TVLabelHeader.Text = l.LabelName;

                LEachLabelHeader.AddView(TVLabelHeader, lpForTVLabelHeader);

                //Delete button
                RelativeLayout rlForDeleteButton = new RelativeLayout(this);
                RelativeLayout.LayoutParams lpForRlForDeleteButton = new RelativeLayout.LayoutParams(-2, -2);

                ImageView Delete = new ImageView(this);
                Delete.Drag += HandleDrag;
                Delete.SetImageResource(Resource.Drawable.ic_delete_forever_black_48dp);
                Delete.Visibility = ViewStates.Gone;
                RelativeLayout.LayoutParams lpForDelete = new RelativeLayout.LayoutParams((int)(sw/10.7), (int)(sw/10.7));
                lpForDelete.AddRule(LayoutRules.AlignParentRight);
                rlForDeleteButton.AddView(Delete, lpForDelete);

                LEachLabelHeader.AddView(rlForDeleteButton);
                //

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
                LPhotosTable.Id = l.Id;
                LPhotosTable.ColumnCount = photoRowCount;
                LPhotosTable.RowCount = PhotosForEachLabels.Count / 2 + 1;
                LPhotosTable.SetBackgroundColor(Color.ParseColor("#fff5f5f5"));

                LEachLabelHeader.Click += delegate
                {
                    int count = pm.GetPhotosOfLabelToList(l).Count;
                    if(count <= 0)
                    {
                        Toast.MakeText(this, "No photo in this category...", ToastLength.Short).Show();
                    }

                    else if (LPhotosTable.Visibility == Android.Views.ViewStates.Visible)
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
                    int margin = (sw / photoRowCount) / 30;
                    FrameLayout flForImg = new FrameLayout(this);
                    ImageView img = new ImageView(this);
                    img.SetImageBitmap(BitmapHelpers.LoadAndResizeBitmap(image.Path, sw / photoRowCount - (margin * 2), sw / photoRowCount - (margin * 2)));
                    LinearLayout.LayoutParams lpForImg = new LinearLayout.LayoutParams(sw / photoRowCount - (margin*2), sw / photoRowCount - (margin * 2));
                    img.SetScaleType(ImageView.ScaleType.CenterCrop);
                    //img.Elevation = 5;
                    flForImg.SetPadding(margin, margin, margin, margin);
                    img.Id = image.Id;

                    img.Click += (s, arg) =>
                    {
                        Intent i = new Intent(this, typeof(FullscreenPhotoView));
                        Intent.PutExtra("PhotoId", img.Id);
                        StartActivity(i);
                    };

                    img.LongClick += delegate
                    {
                        var data = ClipData.NewPlainText("label", img.Id.ToString());
                        img.StartDrag(data, new View.DragShadowBuilder(img), img, 0);

                        Delete.Visibility = ViewStates.Visible;
                    };
                    flForImg.AddView(img, lpForImg);
                    LPhotosTable.AddView(flForImg);
                }

                LEachLabelHeader.AddView(ETLabelChange, lpForETLabelChange);
                LEachLabelHeader.AddView(BConfirmChange, lpForBConfirmChange);

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
                                    if (TVLabelHeader.Text != "Unsigned")
                                    {
                                        TVLabelHeader.Visibility = ViewStates.Gone;
                                        ETLabelChange.Visibility = ViewStates.Visible;
                                        BConfirmChange.Visibility = ViewStates.Visible;
                                    }

                                    break;
                                case "Delete":
                                    pm.DeleteLabelWithout(LEachLabelHeader.Id);
                                    RefreshLayout(ref LLRootLayout, pm,photoRowCount);
                                    break;
                            }
                        };
                        menu.Show();
                    }
                };

                BConfirmChange.Click += delegate
                {
                    pm.ChangeLabel(LEachLabelHeader.Id, ETLabelChange.Text);
                    RefreshLayout(ref LLRootLayout, pm,photoRowCount);
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
            if (requestCode == 0)
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

            else if (requestCode == 1)
            {

            }

            else if (requestCode == 2)
            {

            }
            else if(requestCode == 3)
            {
                photoRowCount = data.GetIntExtra("RowCount", 0);
            }
            RefreshLayout(ref LLRootLayout, pm,photoRowCount);
        }


        void HandleDrag(object sender, Android.Views.View.DragEventArgs e)
        {
            var evt = e.Event;
            View t = new View(this);
            switch (evt.Action)
            {
                case DragAction.Started:
                    /* To register your view as a potential drop zone for the current view being dragged
                     * you need to set the event as handled
                     */
                    e.Handled = true;

                    /* An important thing to know is that drop zones need to be visible (i.e. their Visibility)
                     * property set to something other than Gone or Invisible) in order to be considered. A nice workaround
                     * if you need them hidden initially is to have their layout_height set to 1.
                     */

                    break;
                case DragAction.Entered:
                    if (sender.GetType() == typeof(ImageView))
                    {
                        t = sender as ImageView;
                        t.SetBackgroundColor(Color.Red);
                    }
                    break;
                case DragAction.Exited:
                    if (sender.GetType() == typeof(ImageView))
                    {
                        t = sender as ImageView;
                        t.SetBackgroundColor(Color.Transparent);
                    }
                    /* These two states allows you to know when the dragged view is contained atop your drop zone.
                     * Traditionally you will use that tip to display a focus ring or any other similar mechanism
                     * to advertise your view as a drop zone to the user.
                     */

                    break;
                case DragAction.Drop:
                    /* This state is used when the user drops the view on your drop zone. If you want to accept the drop,
                     *  set the Handled value to true like before.
                     */
                    View v = (View)e.Event.LocalState;
                    float x = e.Event.GetX();
                    float y = e.Event.GetY();
                    if (sender.GetType() == typeof(TextView))
                    {
                        t = sender as TextView;
                        pm.ChangePhotoLabel(v.Id, t.Id);
                    }
                    else if (sender.GetType() == typeof(ImageView))
                    {
                        t = sender as ImageView;
                        pm.DeletePhoto(v.Id);
                        t.Visibility = ViewStates.Gone;
                    }
                    /* It's also probably time to get a bit of the data associated with the drag to know what
                     * you want to do with the information.
                     */
                    var data = e.Event.ClipData.GetItemAt(0).Text;
                    RefreshLayout(ref LLRootLayout, pm, photoRowCount);

                    break;
                case DragAction.Ended:
                    /* This is the final state, where you still have possibility to cancel the drop happened.
                     * You will generally want to set Handled to true.
                     */
                    
                    e.Handled = true;
                    break;
            }
        }
    }
}

