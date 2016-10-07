DataNuage.Google.CloudStorage is a client library that uses Google Cloud Storage to store and retrieve any amount of data at any time, from anywhere.

support@datanuage.com

Your client_id, client-secret, refreshToken and projectId need to be passed as parameters of the constructor of the CloudStorage class.
Once you have an instance of CloudStorage, your can create buckets (similar to folders) and objects as in the example below.

YOU SHOULD ONLY UPLOAD DUMMY DATA WHILE USING THE TRIAL VERSION OF THE SOFTWARE AS EVERY TRIAL USER SHARES THE SAME GOOGLE CLOUD STORAGE ACCOUNT.
Any user of the trial version of the SOFTWARE will be able to view, copy, delete any data you might upload during the trial.

```csharp
   using DataNuage.Google.CloudStorage;
...

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
                        await cloudStorage.PutObjectAsync(
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

#if TRIAL
             View.AddSubview(button);
#else
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
#endif
        }
```

# How do I make a Cloud Storage object accessible from a web page ?

If you want your object to be accessible by a browser, C# code not using the DataNuage.Google.CloudStorage library or any HTTP GET request, you need to make it ‘PublicRead’.
You may also have to set its content-type so that it is recognised for what it is (not just a collection of bytes).

```csharp
await PutObjectAsync("bucket name", "object name", data, Acl.PublicRead, "image/jpeg")
```
 
If your file is not a jpeg image, look here for appropriate content types http://en.wikipedia.org/wiki/Internet_media_type

The URL of the public object is always of the form http(s)://storage.googleapis.com/my-globally-unique-bucket-name/my-object-name
Cloud Storage also treat the bucket name as a subdomain so http(s)://my-globally-unique-bucket-name.storage.googleapis.com/my-object-name is valid as well.


# How do I create folders ?

Cloud Storage does not have the concept of folders. It only deals with buckets and objects.
A bucket must have a name that is unique across all Cloud Storage users worldwide and it can only contain objects (no bucket in buckets).

Having said that, it is fairly easy to simulate folder structures by adding slashes ('/') to the object name:

```csharp
var cs = new CloudStorage(CLIENT_ID, CLIENT_SECRET, refreshToken, project);

await cs.PutObjectAsync("my-globally-unique-bucket-name", "myfolder/myotherfolder/obj.txt", "Hello World", Acl.PublicRead);

var wc = new WebClient();
var s = wc.DownloadString("http://storage.googleapis.com/my-globally-unique-bucket-name/myfolder/myotherfolder/obj.txt");

Assert.IsTrue(s == "Hello World");

await cs.DeleteObjectAsync("my-globally-unique-bucket-name", "myfolder/myotherfolder/obj.txt");
```
