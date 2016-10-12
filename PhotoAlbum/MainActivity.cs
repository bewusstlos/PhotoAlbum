using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using Android.Widget;
using Android.Net.Wifi;
using Java.IO;
using Android.Views.Animations;

using Environment = Android.OS.Environment;
using Uri = Android.Net.Uri;
using Android.Views;

using RedisServer;

namespace PhotoAlbum
{
    public static class App
    {
        public static File _file;
        public static File _dir;
        public static Bitmap bitmap;
    }

    [Activity(Label = "Photo Album", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        LinearLayout LLRootLayout;
        LinearLayout LLContextMenuOwner;
        int sw;
        public RedisManager rm;
        int photoRowCount = 2;

        //Init buttons on Action Bar
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Layout.ActionBarMenu, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        //Events for Action Bar buttons
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
            rm = new RedisManager();
            //Metrics, which need to show layout correctly
            sw = (int)((float)Resources.DisplayMetrics.WidthPixels - 50);
            base.OnCreate(bundle);
            //Init for Redis
            SetContentView(Resource.Layout.Main);

            Deserialize();

            CreateDirectoryForPictures();
            LLRootLayout = FindViewById<LinearLayout>(Resource.Id.RootLayout);
            LLContextMenuOwner = FindViewById<LinearLayout>(Resource.Id.context_menu_popuper);
            RefreshLayout(ref LLRootLayout, rm, photoRowCount);

        }

        public void Deserialize()
        {
            try
            {
                rm.DeserializePhotos();
                rm.DeserializeLabels();
            }
            catch (System.OverflowException e)
            {

            }
        }

        public void Serialize()
        {
            try
            {
                rm.SerializeLabels();
                rm.SerializePhotos();
            }
            catch(OverflowException e)
            {

            }
        }

        public void RefreshLayout(ref LinearLayout RootLayout,RedisManager rm, int photoRow)
        {
            RootLayout.RemoveAllViews();
            SetLayoutSpoilerStyle(ref RootLayout,rm, photoRow);
        }

        //All views creating here
        public void SetLayoutSpoilerStyle(ref LinearLayout RootLayout, RedisManager rm, int photoRowCount)
        {
            Deserialize();

            List<RedisManager.Label> AllLabels = rm.GetLabelsToList();
            //Get All Labels from Redis
            List <RedisManager.Photo > PhotosForEachLabels;

            foreach (RedisManager.Label l in AllLabels)
            {
                PhotosForEachLabels = rm.GetPhotoForSomeLabel(l);
                //Get All Photos for some Label

                //Container for each Category
                LinearLayout LEachLabel = new LinearLayout(this);
                LinearLayout.LayoutParams lpForLEachLabel = new LinearLayout.LayoutParams(-1, -2);
                LEachLabel.Orientation = Orientation.Vertical;
                LEachLabel.Elevation = 10;
                lpForLEachLabel.SetMargins(20, 10, 20, 10);

                //Container for Label Text
                LinearLayout LEachLabelHeader = new LinearLayout(this);
                LinearLayout.LayoutParams lpForLEachLabelHeader = new LinearLayout.LayoutParams(-1, -2);
                LEachLabelHeader.SetBackgroundColor(Color.ParseColor("#fff5f5f5"));
                LEachLabelHeader.Orientation = Orientation.Horizontal;

                

                //Text for Label
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
                RelativeLayout.LayoutParams lpForDelete = new RelativeLayout.LayoutParams((int)(sw/10.7), (int)(sw/10.7));
                lpForDelete.AddRule(LayoutRules.AlignParentRight);
                rlForDeleteButton.AddView(Delete, lpForDelete);

                LEachLabelHeader.AddView(rlForDeleteButton);
                
                //TextView for changing label name
                EditText ETLabelChange = new EditText(this);
                LinearLayout.LayoutParams lpForETLabelChange = new LinearLayout.LayoutParams(-2, -2, 4f);
                LEachLabelHeader.AddView(ETLabelChange, lpForETLabelChange);
                ETLabelChange.Visibility = ViewStates.Gone;

                //Button to confgirm label name changes
                Button BConfirmChange = new Button(this);
                LinearLayout.LayoutParams lpForBConfirmChange = new LinearLayout.LayoutParams(-2, -2, 1f);
                BConfirmChange.Text = "OK";
                LEachLabelHeader.AddView(BConfirmChange, lpForBConfirmChange);
                BConfirmChange.Visibility = ViewStates.Gone;

                //GridLayout for showing all photos in table of each Label
                GridLayout LPhotosTable = new GridLayout(this);
                LPhotosTable.Orientation = GridOrientation.Horizontal;
                LinearLayout.LayoutParams lpForLPhotosTable = new LinearLayout.LayoutParams(-1, -2);
                lpForLPhotosTable.SetMargins(5, 0, 5, 0);
                LPhotosTable.Elevation = 10;
                LPhotosTable.Id = l.Id;
                LPhotosTable.ColumnCount = photoRowCount;
                LPhotosTable.RowCount = PhotosForEachLabels.Count / 2 + 1;
                LPhotosTable.SetBackgroundColor(Color.ParseColor("#fff5f5f5"));

                //Event for showing/hiding category 
                LEachLabelHeader.Click += delegate
                {
                    int count = PhotosForEachLabels.Count;
                    //Photos of some Label
                    if(count <= 0)
                    {
                        Toast.MakeText(this, "No photo in this category...", ToastLength.Short).Show();
                    }

                    else if (LPhotosTable.Visibility == Android.Views.ViewStates.Visible)
                    {

                        HideShowAnimation(ref LPhotosTable, LPhotosTable.RowCount, sw, false);
                        LPhotosTable.Visibility = Android.Views.ViewStates.Invisible;
                    }
                    else
                    {
                        LPhotosTable.Visibility = Android.Views.ViewStates.Visible;
                        HideShowAnimation(ref LPhotosTable, LPhotosTable.RowCount, sw, true);
                    }
                };

                //Layout each photo for current category
                foreach (var image in PhotosForEachLabels)
                {
                    int margin = (sw / photoRowCount) / 30;
                    FrameLayout flForImg = new FrameLayout(this);
                    ImageView img = new ImageView(this);
                    img.SetImageBitmap(BitmapHelpers.LoadAndResizeBitmap(image.ByteArray, sw / photoRowCount - (margin * 2), sw / photoRowCount - (margin * 2)));
                    LinearLayout.LayoutParams lpForImg = new LinearLayout.LayoutParams(sw / photoRowCount - (margin*2), sw / photoRowCount - (margin * 2));
                    img.SetScaleType(ImageView.ScaleType.CenterCrop);
                    img.Elevation = 5;
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

                LEachLabel.AddView(LEachLabelHeader, lpForLEachLabelHeader);

                //Event for popup menu for label
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
                                        TVLabelHeader.Visibility = ViewStates.Gone;
                                        ETLabelChange.Visibility = ViewStates.Visible;
                                        BConfirmChange.Visibility = ViewStates.Visible;

                                        break;
                                    case "Delete":
                                        rm.DeleteLabel(l.Id);
                                        rm.SerializeLabels();
                                        RefreshLayout(ref LLRootLayout,rm, photoRowCount);
                                        break;
                            }
                        };
                        menu.Show();
                    }
                };

                //Event for renaming label
                BConfirmChange.Click += delegate
                {
                    rm.RenameLabel(l.Id, ETLabelChange.Text);
                    rm.SerializeLabels();
                    RefreshLayout(ref LLRootLayout, rm, photoRowCount);
                };
                LEachLabel.AddView(LPhotosTable, lpForLPhotosTable);
                RootLayout.AddView(LEachLabel, lpForLEachLabel);

                Delete.Visibility = ViewStates.Invisible;
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

        //Method, which take care about closing different activities
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == 0)
            {
                try
                {
                    Intent mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
                    Uri contentUri = Uri.FromFile(App._file);
                    rm.AddPhoto(BitmapHelpers.ImageToByteArray(BitmapFactory.DecodeFile(App._file.ToString())));
                    Serialize();
                    mediaScanIntent.SetData(contentUri);
                    SendBroadcast(mediaScanIntent);

                    int width = Resources.DisplayMetrics.WidthPixels / 2 - 30;
                    int height = width;
                    //App.bitmap = App._file.Path.LoadAndResizeBitmap(width, height);
                    if (App.bitmap != null)
                    {
                        App.bitmap = null;
                        GC.Collect();
                    }
                }
                catch
                {

                }
                App._file.Delete();
            }

            else if (requestCode == 1)
            {
                Deserialize();
            }

            else if (requestCode == 2)
            {

            }

            else if(requestCode == 3)
            {
                photoRowCount = data.GetIntExtra("RowCount", 0);
            }
            
            RefreshLayout(ref LLRootLayout, rm, photoRowCount);
        }

        //Event, which control drag`n`drop
        void HandleDrag(object sender, Android.Views.View.DragEventArgs e)
        {
            var evt = e.Event;
            View t = new View(this);
            switch (evt.Action)
            {
                case DragAction.Started:
                    e.Handled = true;
                    break;
                    //Setting color of drop zones on enter
                case DragAction.Entered:
                    if (sender.GetType() == typeof(ImageView))
                    {
                        t = sender as ImageView;
                        t.SetBackgroundColor(Color.Red);
                    }
                    else if (sender.GetType() == typeof(TextView))
                    {
                        t = sender as TextView;
                        t.SetBackgroundColor(Color.Blue);
                    }
                    break;
                    //Setting color back to default
                case DragAction.Exited:
                    if (sender.GetType() == typeof(ImageView))
                    {
                        t = sender as ImageView;
                        t.SetBackgroundColor(Color.Transparent);
                    }
                    else if (sender.GetType() == typeof(TextView))
                    {
                        t = sender as TextView;
                        t.SetBackgroundColor(Color.Transparent);
                    }
                    break;
                    //Actions on drop
                case DragAction.Drop:
                    View v = (View)e.Event.LocalState;
                    float x = e.Event.GetX();
                    float y = e.Event.GetY();
                    if (sender.GetType() == typeof(TextView))
                    {
                        t = sender as TextView;
                        rm.ChangePhotoLabel(v.Id, t.Id);
                        Serialize();
                    }
                    else if (sender.GetType() == typeof(ImageView))
                    {
                        t = sender as ImageView;
                        rm.DeletePhoto(v.Id);
                        Serialize();
                    }
                    var data = e.Event.ClipData.GetItemAt(0).Text;
                    RefreshLayout(ref LLRootLayout, rm, photoRowCount);
                    break;
                case DragAction.Ended:
                    t.Visibility = ViewStates.Invisible;
                    e.Handled = true;
                    break;
            }
        }
    }
}
