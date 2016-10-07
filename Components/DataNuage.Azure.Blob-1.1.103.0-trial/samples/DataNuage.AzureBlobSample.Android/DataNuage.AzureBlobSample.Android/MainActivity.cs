using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace DataNuage.AzureBlobSample.Android
{
    using System.Threading;

    using DataNuage.Azure.Blob;

    [Activity(Label = "DataNuage.AzureBlobSample.Android", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);

            var button = FindViewById<Button>(Resource.Id.MyButton);
            var text = FindViewById<TextView>(Resource.Id.textView1);
            var random = new Random();

            CancellationTokenSource cts = null;

            button.Click += async (sender, e) =>
                {
                    if (button.Text == "Cancel")
                    {
                        cts.Cancel();
                        return;
                    }

                    text.Text = "In progress...";
                    button.Enabled = false;


                    var bucket = "my-unique-bucket-name" + random.Next();

                    try
                    {
                        var abs = new AzureBlobStorage("<Your Azure Account Id> - ignored by Trial version",
                          "<Your Azure Blob Access Key> - ignored by Trial version");

                        await abs.CreateBucketAsync(bucket);
                        text.Text = String.Format("Bucket {0} created", bucket);

                        await abs.PutObjectAsync(bucket, "myobject", "Hello World");
                        text.Text = String.Format("Object myobject created");

                        var s = await abs.GetObjectAsStringAsync(bucket, "myobject");
                        text.Text = String.Format("{0} read", s);

                        await abs.DeleteObjectAsync(bucket, "myobject");
                        text.Text = String.Format("Object myobject deleted");

                        var dummyData = new byte[1000000];
                        cts = new CancellationTokenSource();
                        button.Text = "Cancel";
                        button.Enabled = true;

                        try
                        {
                            await abs.PutObjectAsync(
                                bucket,
                                "big",
                                dummyData,
                                cancellationToken: cts.Token,
                                progress: l =>text.Text = string.Format("Upload {0}%", (100 * l) / dummyData.Length));
                        }
                        catch (System.OperationCanceledException)
                        {
                            text.Text = "Canceled";
                        }

                        button.Text = "Start";
                        button.Enabled = false;

                        foreach (var objectName in await abs.ListObjectsAsync(bucket))
                        {
                            await abs.DeleteObjectAsync(bucket, objectName);
                        }

                        await abs.DeleteBucketAsync(bucket);
                        text.Text = String.Format("Empty bucket {0} deleted", bucket);

                        text.Text = "Success";
                        button.Enabled = true;
                    }
                    catch (Exception ex)
                    {
                        new AlertDialog.Builder(this)
                            .SetPositiveButton("Ok", (_, __) => { })
                            .SetMessage(ex.Message)
                            .SetTitle("Error")
                            .Show();

                        button.Text = "Start";
                        button.Enabled = true;
                    }
                };
        }
    }
}