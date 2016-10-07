using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;

namespace PhotoAlbum
{
    [Activity(Label = "Set row count", MainLauncher = false, Icon = "@drawable/icon", Theme = "@android:style/Theme.Material.Light.Dialog")]
    class ChangeRowCount : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            int rowCount = Intent.GetIntExtra("RowCount", 0);
            SetContentView(Resource.Layout.change_row_count);
            SeekBar sb = FindViewById<SeekBar>(Resource.Id.seek_bar);
            sb.Progress = rowCount;
            sb.Max = 4;
            Button bOk = FindViewById<Button>(Resource.Id.label_ok);
            Button bCancel = FindViewById<Button>(Resource.Id.label_cancel);

             bOk.Click += delegate
             {
                 Intent i = new Intent();

                 i.PutExtra("RowCount", sb.Progress );
                 this.SetResult(Result.Ok, i);
                 this.Finish();
             };


        }
    }
}