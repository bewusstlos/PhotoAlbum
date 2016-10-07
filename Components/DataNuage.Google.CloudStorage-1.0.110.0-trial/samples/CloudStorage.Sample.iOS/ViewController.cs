using System;

using Foundation;
using UIKit;

namespace CloudStorageSample.iOSunified
{
    using System.Threading;

    using CoreGraphics;

    using DataNuage.Google.CloudStorage;

    public partial class ViewController : UIViewController
    {
        float buttonWidth = 600;
        float buttonHeight = 300;

        public ViewController(IntPtr handle)
            : base(handle)
        {
        }

        private static string CLIENT_ID = "your_google_project_client_id";// Ignored by Trial version

        private static string CLIENT_SECRET = "your_google_project_client_secret";// Ignored by Trial version

        private static string REDIRECT_URI = "urn:ietf:wg:oauth:2.0:oob";

        private static string PROJECT_ID = "your_google_project_id";// Ignored by Trial version

        private static string refreshToken;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.Frame = UIScreen.MainScreen.Bounds;
            View.BackgroundColor = UIColor.White;
            View.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;

            var button = UIButton.FromType(UIButtonType.RoundedRect);

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
                        var cloudStorage = new CloudStorage(CLIENT_ID, CLIENT_SECRET, refreshToken, PROJECT_ID);

                        await cloudStorage.CreateBucketAsync(bucket);
                        button.SetTitle(String.Format("Bucket {0} created", bucket), UIControlState.Normal);

                        await cloudStorage.PutObjectAsync(bucket, "myobject", "Hello World");
                        button.SetTitle(String.Format("Object myobject created"), UIControlState.Normal);

                        var s = await cloudStorage.GetObjectAsStringAsync(bucket, "myobject");
                        button.SetTitle(String.Format("{0} read", s), UIControlState.Normal);

                        await cloudStorage.DeleteObjectAsync(bucket, "myobject");
                        button.SetTitle(String.Format("Object myobject deleted"), UIControlState.Normal);

                        var dummyData = new byte[1000000];
                        cts = new CancellationTokenSource();

                        try
                        {
                            await
                                cloudStorage.PutObjectAsync(
                                    bucket,
                                    "big",
                                    dummyData,
                                    cancellationToken: cts.Token,
                                    progress:
                                        l =>
                                        button.SetTitle(
                                            string.Format("Upload {0}% - Click to cancel", (100 * l) / dummyData.Length),
                                            UIControlState.Normal));
                        }
                        catch (OperationCanceledException)
                        {
                            button.SetTitle("Canceled", UIControlState.Normal);
                        }

                        cts = null;

                        foreach (var objectName in await cloudStorage.ListObjectsAsync(bucket))
                        {
                            await cloudStorage.DeleteObjectAsync(bucket, objectName);
                        }

                        await cloudStorage.DeleteBucketAsync(bucket);
                        button.SetTitle(String.Format("Empty bucket {0} deleted", bucket), UIControlState.Normal);

                        button.SetTitle("Success", UIControlState.Normal);
                    }
                    catch (Exception ex)
                    {
                        new UIAlertView("Error", ex.Message, null, "ok", null).Show();
                        button.SetTitle("Click me", UIControlState.Normal);
                    }
                };

            if (CloudStorage.IsTrial)
            {
                View.AddSubview(button);
            }
            else
            {
                var webView = new UIWebView(View.Bounds);

                View.AddSubview(webView);

                var url = string.Format("https://accounts.google.com/o/oauth2/auth?response_type=code&client_id={0}&redirect_uri={1}&scope=https://www.googleapis.com/auth/drive",
                        CLIENT_ID,
                        REDIRECT_URI);
                webView.LoadRequest(new NSUrlRequest(new NSUrl(url)));
                webView.LoadFinished += async (_, __) =>
                    {
                        var t = webView.EvaluateJavascript("document.title");

                        var left = "Success code=";
                        if (t.StartsWith(left))
                        {
                            webView.RemoveFromSuperview();

                            // Success Code will only be valid for a short period of time
                            var successCode = t.Substring(left.Length);

                            // Refresh Token is permanent, it can be stored an reused later
                            refreshToken = await CloudStorage.GetRefreshTokenAsync(CLIENT_ID, CLIENT_SECRET, successCode);

                            View.AddSubview(button);
                        }
                    };
            }
        }
    }
}

