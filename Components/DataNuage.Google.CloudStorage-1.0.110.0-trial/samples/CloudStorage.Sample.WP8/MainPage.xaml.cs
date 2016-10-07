namespace CloudStorageSample.WP8
{
    using System.Threading;
    using System.Windows;

    using DataNuage.Google.CloudStorage;

    using Microsoft.Phone.Controls;
    using System;

    public partial class MainPage : PhoneApplicationPage
    {
        private static string CLIENT_ID = "your_google_project_client_id";// Ignored by Trial version

        private static string CLIENT_SECRET = "your_google_project_client_secret";// Ignored by Trial version

        private static string REDIRECT_URI = "urn:ietf:wg:oauth:2.0:oob";

        private static string PROJECT_ID = "your_google_project_id";// Ignored by Trial version

        private static string refreshToken;

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
                        var cloudStorage = new CloudStorage(CLIENT_ID, CLIENT_SECRET, refreshToken, PROJECT_ID);

                        textBlock.Text = "In progress";

                        await cloudStorage.CreateBucketAsync(bucket);
                        textBlock.Text = string.Format("Bucket {0} created", bucket);

                        await cloudStorage.PutObjectAsync(bucket, "myobject", "Hello World");
                        textBlock.Text = string.Format("Object myobject created");

                        var s = await cloudStorage.GetObjectAsStringAsync(bucket, "myobject");
                        textBlock.Text = string.Format("{0} read", s);

                        await cloudStorage.DeleteObjectAsync(bucket, "myobject");
                        textBlock.Text = string.Format("Object myobject deleted");

                        var dummyData = new byte[1000000];
                        cts = new CancellationTokenSource();
                        mustCancel = true;
                        button.Content = "Cancel";
                        button.IsEnabled = true;
                        textBlock.Text = "Uploading big object";

                        try
                        {
                            await cloudStorage.PutObjectAsync(bucket, "big", dummyData, cancellationToken: cts.Token);
                        }
                        catch (OperationCanceledException)
                        {
                            textBlock.Text = "Canceled";
                        }

                        mustCancel = false;
                        button.Content = "Start";
                        button.IsEnabled = false;

                        foreach (var objectName in await cloudStorage.ListObjectsAsync(bucket))
                        {
                            textBlock.Text = string.Format("Deleting {0}", objectName);
                            await cloudStorage.DeleteObjectAsync(bucket, objectName);
                        }

                        await cloudStorage.DeleteBucketAsync(bucket);

                        textBlock.Text = "Success";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK);
                    }

                    button.Content = "Start";
                    button.IsEnabled = true;
                };

            if (CloudStorage.IsTrial)
            {
                webBrowser.Visibility = Visibility.Collapsed;
                button.Visibility = Visibility.Visible;
                textBlock.Visibility = Visibility.Visible;
            }
            else
            {
                webBrowser.Visibility = Visibility.Visible;
                button.Visibility = Visibility.Collapsed;
                textBlock.Visibility = Visibility.Collapsed;

                webBrowser.IsScriptEnabled = true;

                var url = string.Format("https://accounts.google.com/o/oauth2/auth?response_type=code&client_id={0}&redirect_uri={1}&scope=https://www.googleapis.com/auth/devstorage.full_control",
                        CLIENT_ID,
                        REDIRECT_URI);

                webBrowser.Navigated += async (_, __) =>
                    {
                        var title = webBrowser.InvokeScript("eval", "document.title") as string;
                        var left = "Success code=";
                        if (title.StartsWith(left))
                        {
                            // Success Code will only be valid for a short period of time
                            var successCode = title.Substring(left.Length);

                            // Refresh Token is permanent, it can be stored an reused later
                            refreshToken = await CloudStorage.GetRefreshTokenAsync(CLIENT_ID, CLIENT_SECRET, successCode);

                            webBrowser.Visibility = Visibility.Collapsed;
                            button.Visibility = Visibility.Visible;
                            textBlock.Visibility = Visibility.Visible;
                        }


                    };
                webBrowser.Navigate(new Uri(url));
            }
        }
    }
}