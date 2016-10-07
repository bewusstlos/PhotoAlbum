
//Remember to enable the INTERNET permission in the AndroidManifest.xml via the application Properties so that the WebView control can access the internet.

namespace CloudStorageSample.WP8.Android
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;

    using CloudStorageSample.Android;

    using DataNuage.Google.CloudStorage;

    using global::Android.App;
    using global::Android.OS;
    using global::Android.Views;
    using global::Android.Webkit;
    using global::Android.Widget;

    [Activity(Label = "CloudStorageSample.Android", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private static string CLIENT_ID = "your_google_project_client_id";// Ignored by Trial version

        private static string CLIENT_SECRET = "your_google_project_client_secret";// Ignored by Trial version

        private static string REDIRECT_URI = "urn:ietf:wg:oauth:2.0:oob";

        private static string PROJECT_ID = "your_google_project_id";// Ignored by Trial version

        private static string refreshToken;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);

            var button = FindViewById<Button>(Resource.Id.MyButton);
            var text = FindViewById<TextView>(Resource.Id.textView1);
            var webView = FindViewById<WebView>(Resource.Id.LocalWebView);

            if (CloudStorage.IsTrial)
            {
                webView.Visibility = ViewStates.Invisible;
                button.Visibility = ViewStates.Visible;
                text.Visibility = ViewStates.Visible;
            }
            else
            {
                var url = string.Format("https://accounts.google.com/o/oauth2/auth?response_type=code&client_id={0}&redirect_uri={1}&scope=https://www.googleapis.com/auth/devstorage.full_control",
                        CLIENT_ID,
                        REDIRECT_URI);

                webView.SetWebViewClient(new MyWebViewClient(button, text));
                webView.Settings.JavaScriptEnabled = true;
                webView.LoadUrl(url);

                webView.Visibility = ViewStates.Visible;
                button.Visibility = ViewStates.Invisible;
                text.Visibility = ViewStates.Invisible;
            }

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
                    var cloudStorage = new CloudStorage(CLIENT_ID, CLIENT_SECRET, refreshToken, PROJECT_ID);

                    await cloudStorage.CreateBucketAsync(bucket);
                    text.Text = String.Format("Bucket {0} created", bucket);

                    await cloudStorage.PutObjectAsync(bucket, "myobject", "Hello World");
                    text.Text = String.Format("Object myobject created");

                    var s = await cloudStorage.GetObjectAsStringAsync(bucket, "myobject");
                    text.Text = String.Format("{0} read", s);

                    await cloudStorage.DeleteObjectAsync(bucket, "myobject");
                    text.Text = String.Format("Object myobject deleted");

                    var dummyData = new byte[1000000];
                    cts = new CancellationTokenSource();
                    button.Text = "Cancel";
                    button.Enabled = true;

                    try
                    {
                        await cloudStorage.PutObjectAsync(bucket, "big", dummyData, cancellationToken: cts.Token,
                        progress: l => text.Text = string.Format("Upload {0}%", (100 * l) / dummyData.Length));
                    }
                    catch (System.OperationCanceledException)
                    {
                        text.Text = "Canceled";
                    }

                    button.Text = "Start";
                    button.Enabled = false;

                    var list = await cloudStorage.ListObjectsAsync(bucket);

                    foreach (var objectName in list)
                    {
                        await cloudStorage.DeleteObjectAsync(bucket, objectName);
                    }

                    await cloudStorage.DeleteBucketAsync(bucket);
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

        class MyWebViewClient : WebViewClient
        {
            private Button button;

            private TextView text;

            public MyWebViewClient(Button button, TextView text)
            {
                this.button = button;
                this.text = text;
            }

            public override async void OnPageFinished(WebView view, string url)
            {
                var left = "Success code=";
                if (view.Title.StartsWith(left))
                {
                    // Success Code will only be valid for a short period of time
                    var successCode = view.Title.Substring(left.Length);

                    // Refresh Token is permanent, it can be stored an reused later
                    refreshToken = await CloudStorage.GetRefreshTokenAsync(
                            CLIENT_ID,
                            CLIENT_SECRET,
                            successCode);

                    view.Visibility = ViewStates.Invisible;
                    button.Visibility = ViewStates.Visible;
                    text.Visibility = ViewStates.Visible;
                }

                base.OnPageFinished(view, url);
            }
        }
    }
}

