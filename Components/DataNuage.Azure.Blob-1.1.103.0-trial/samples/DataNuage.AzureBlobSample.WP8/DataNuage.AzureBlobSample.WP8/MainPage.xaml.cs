using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using DataNuage.AzureBlobSample.WP8.Resources;

namespace DataNuage.AzureBlobSample.WP8
{
    using System.Threading;

    using DataNuage.Azure.Blob;

    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            var random = new Random();
            CancellationTokenSource cts = null;
            bool mustCancel = false;

            button.Click += async (sender, e) =>
            {
                if (mustCancel)
                {
                    cts.Cancel();
                    return;
                }

                button.IsEnabled = false;

                var bucket = "my-unique-bucket-name" + random.Next();

                try
                {
                    var abs = new AzureBlobStorage("<Your Azure Account Id> - ignored by Trial version",
                               "<Your Azure Blob Access Key> - ignored by Trial version");

                    textBlock.Text = "In progress";

                    await abs.CreateBucketAsync(bucket);
                    textBlock.Text = string.Format("Bucket {0} created", bucket);

                    await abs.PutObjectAsync(bucket, "myobject", "Hello World");
                    textBlock.Text = string.Format("Object myobject created");

                    var s = await abs.GetObjectAsStringAsync(bucket, "myobject");
                    textBlock.Text = string.Format("{0} read", s);

                    await abs.DeleteObjectAsync(bucket, "myobject");
                    textBlock.Text = string.Format("Object myobject deleted");

                    var dummyData = new byte[1000000];
                    cts = new CancellationTokenSource();
                    mustCancel = true;
                    button.Content = "Cancel";
                    button.IsEnabled = true;
                    textBlock.Text = "Uploading big object";

                    try
                    {
                        await abs.PutObjectAsync(bucket, "big", dummyData, cancellationToken: cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        textBlock.Text = "Canceled";
                    }

                    mustCancel = false;
                    button.Content = "Start";
                    button.IsEnabled = false;

                    foreach (var objectName in await abs.ListObjectsAsync(bucket))
                    {
                        textBlock.Text = string.Format("Deleting {0}", objectName);
                        await abs.DeleteObjectAsync(bucket, objectName);
                    }

                    await abs.DeleteBucketAsync(bucket);

                    textBlock.Text = "Success";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK);
                }

                button.Content = "Start";
                button.IsEnabled = true;
            };
        }
    }
}