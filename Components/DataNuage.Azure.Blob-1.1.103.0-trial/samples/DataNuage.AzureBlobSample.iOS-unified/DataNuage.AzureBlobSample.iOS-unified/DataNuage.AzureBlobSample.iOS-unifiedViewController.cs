namespace DataNuage.AzureBlobSample.iOSunified
{
    using CoreGraphics;
    using DataNuage.Azure.Blob;
    using System;
    using System.Threading;
    using UIKit;

    public partial class DataNuage_AzureBlobSample_iOS_unifiedViewController : UIViewController
    {
        UIButton button;
        float buttonWidth = 600;
        float buttonHeight = 300;

        static bool UserInterfaceIdiomIsPhone
        {
            get { return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone; }
        }

        public DataNuage_AzureBlobSample_iOS_unifiedViewController(IntPtr handle)
            : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.Frame = UIScreen.MainScreen.Bounds;
            View.BackgroundColor = UIColor.White;
            View.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;

            button = UIButton.FromType(UIButtonType.RoundedRect);

            button.Frame = new CGRect(
                View.Frame.Width / 2 - buttonWidth / 2,
                View.Frame.Height / 2 - buttonHeight / 2,
                buttonWidth,
                buttonHeight);

            button.SetTitle("Click me", UIControlState.Normal);

            CancellationTokenSource cts = null;

            button.TouchUpInside += async (sender, e) =>
            {
                if (cts != null)
                {
                    cts.Cancel();
                    return;
                }

                button.SetTitle("In progress", UIControlState.Normal);

                var random = new Random();

                var bucket = "my-unique-bucket-name" + random.Next();

                try
                {
                    var abs = new AzureBlobStorage("<Your Azure Account Id> - ignored by Trial version",
                                "<Your Azure Blob Access Key> - ignored by Trial version");

                    await abs.CreateBucketAsync(bucket);
                    button.SetTitle(String.Format("Bucket {0} created", bucket), UIControlState.Normal);

                    await abs.PutObjectAsync(bucket, "myobject", "Hello World");
                    button.SetTitle(String.Format("Object myobject created"), UIControlState.Normal);

                    var s = await abs.GetObjectAsStringAsync(bucket, "myobject");
                    button.SetTitle(String.Format("{0} read", s), UIControlState.Normal);

                    await abs.DeleteObjectAsync(bucket, "myobject");
                    button.SetTitle(String.Format("Object myobject deleted"), UIControlState.Normal);

                    var dummyData = new byte[1000000];
                    cts = new CancellationTokenSource();

                    try
                    {
                        await abs.PutObjectAsync(
                            bucket,
                            "big",
                            dummyData,
                            cancellationToken: cts.Token,
                                progress: l => button.SetTitle(
                                        string.Format("Upload {0}% - Click to cancel", (100 * l) / dummyData.Length),
                                        UIControlState.Normal));
                    }
                    catch (OperationCanceledException)
                    {
                        button.SetTitle("Canceled", UIControlState.Normal);
                    }

                    cts = null;

                    foreach (var objectName in await abs.ListObjectsAsync(bucket))
                    {
                        await abs.DeleteObjectAsync(bucket, objectName);
                    }

                    await abs.DeleteBucketAsync(bucket);
                    button.SetTitle(String.Format("Empty bucket {0} deleted", bucket), UIControlState.Normal);

                    button.SetTitle("Success", UIControlState.Normal);
                }
                catch (Exception ex)
                {
                    new UIAlertView("Error", ex.Message, null, "ok", null).Show();
                    button.SetTitle("Click me", UIControlState.Normal);
                }
            };

            button.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleTopMargin |
                UIViewAutoresizing.FlexibleBottomMargin;

            View.AddSubview(button);
        }
    }
}