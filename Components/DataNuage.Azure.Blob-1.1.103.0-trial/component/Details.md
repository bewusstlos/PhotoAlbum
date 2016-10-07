DataNuage.Azure.Blob is a client library that uses Microsoft's Azure Blob Storage to store and retrieve any amount of data at any time, from anywhere.

support@datanuage.com

Your Account and AccessKey need to be passed as parameters of the constructor of the AzureBlobStorage class.
Once you have an instance of AzureBlobStorage, your can create buckets (similar to folders) and objects as in the example below.

YOU SHOULD ONLY UPLOAD DUMMY DATA WHILE USING THE TRIAL VERSION OF THE SOFTWARE AS EVERY TRIAL USER SHARES THE SAME AZURE STORAGE ACCOUNT.
Any user of the trial version of the SOFTWARE will be able to view, copy, delete any data you might upload during the trial.

```csharp
using DataNuage.Azure.Blob;
...

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
            var abs = new AzureBlobStorage("<Your Azure Account Name> - ignored by Trial version",
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
```

# How do I make a Blob Storage Object accessible from a web page ?

If you want your object to be accessible by a browser, C# code not using the DataNuage.Azure.Blob library or any HTTP GET request, you need to make its bucket 'PublicRead'.
You may also have to set the object content-type so that it is recognised for what it is (not just a collection of bytes).

```csharp
await abs.CreateBucketAsync("my-public-bucket", BucketAcl.PublicRead);
await abs.PutObjectAsync("my-public-bucket", "mybitmap.jpg", data, contentType: "image/jpeg");
var wc = new WebClient();
var bitmapBytes = wc.DownloadData(abs.GetUrl("my-public-bucket", "mybitmap.jpg"));
```
 
If your file is not a jpeg image, look here for appropriate content types http://en.wikipedia.org/wiki/Internet_media_type

The URL of the public object is always of the form http(s)://myaccountname.blob.core.windows.net/mybucket/myobject


# How do I create folders ?

Azure Blob Storage does not have the concept of folders. It only deals with buckets and objects.
A bucket can only contain objects (no bucket in buckets).

Having said that, it is fairly easy to simulate folder structures by adding slashes ('/') to the object name:

```csharp
var abs = new AzureBlobStorage("<Your Azure Account Name> - ignored by Trial version",
                        "<Your Azure Blob Access Key> - ignored by Trial version");

await abs.CreateBucketAsync("my-public-bucket", BucketAcl.PublicRead);
await abs.PutObjectAsync("my-public-bucket", "myfolder/myotherfolder/obj.txt", "Hello World", Acl.PublicRead);

var wc = new WebClient();
var s = wc.DownloadString("http://myaccountname.blob.core.windows.net/my-public-bucket/myfolder/myotherfolder/obj.txt");
```

# I don't feel comfortable storing my Access Key on the device. Is there an alternative ?

You can use a feature called Shared Access Signature or SAS for short which let you create pre-authorized URLs.
Typically, you would use it like this:

```csharp
// START OF SERVER CODE
var abs = new AzureBlobStorage("...", "...");

// Create the bucket (do it only the first time)
await abs.CreateBucketAsync("my-sas-test-bucket");

await abs.PutObjectAsync("my-sas-test-bucket", "myobject", "Hello");

var url = abs.GetSharedAccessSignature(
"my-sas-test-bucket",
"myobject",
DateTime.UtcNow.AddMinutes(-30), // Start date with some margin for error between server times
DateTime.UtcNow.AddDays(1), // Valid for one day
Access.Read | Access.Write | Access.Delete);
//END OF SERVER CODE


// Send read/write/delete urls (strings) to device by whichever means you see fit


//START OF DEVICE CODE (NO ACCESS KEY NEEDED)
var sas = new SharedAccessSignature(url);

var hello = await sas.GetObjectAsStringAsync(); // Hello
await sas.PutObjectAsync("Goodbye");// Set object content to 'Goodbye'
await sas.DeleteObjectAsync();// Delete object
//END OF DEVICE CODE
```

However, the recommended approach is to use access policies (which you can disable if pre-authorized URLs get leaked):

```csharp
// START OF SERVER CODE
var abs = new AzureBlobStorage("...", "...");

// Create the bucket (do it only the first time)
await abs.CreateBucketAsync("my-sas-test-bucket");

await abs.SetAccessPolicyAsync(
    "my-sas-test-bucket",
    "mypolicy",
    DateTime.UtcNow.AddMinutes(-30), // Start date with some margin for error between server times
    DateTime.UtcNow.AddMonths(1), // Valid for one month
    Access.Read | Access.Write | Access.Delete);

await abs.PutObjectAsync("my-sas-test-bucket", "myobject", "Hello");

var url = abs.GetSharedAccessSignature(
"my-sas-test-bucket",
"mypolicy");
//END OF SERVER CODE


// Send the url (string) to the device by whichever means you see fit


//START OF DEVICE CODE (NO ACCESS KEY NEEDED)
var sas = new SharedAccessSignature(url);

var hello = await sas.GetObjectAsStringAsync(); // Hello
await sas.PutObjectAsync("Goodbye");// Set object content to 'Goodbye'
await sas.DeleteObjectAsync();// Delete object
//END OF DEVICE CODE
```

Note that you can have up to 5 access policies per bucket/container and that they cannot be deleted (but you can update then with no rights or and expiry date in the past).

# Can I use Shared Access Signature (SAS) to let the client create/list/delete objects/blobs ?

Yes, you can create SAS on buckets/containers.

```csharp
// START OF SERVER CODE
var abs = new AzureBlobStorage("...", "...");

// Create the bucket (do it only the first time)
await abs.CreateBucketAsync("my-sas-test-bucket");

await abs.SetAccessPolicyAsync(
    "my-sas-test-bucket",
    "mypolicy",
    DateTime.UtcNow.AddMinutes(-30), // Start date with some margin for error between server times
    DateTime.UtcNow.AddMonths(1), // Valid for one month
    Access.Read | Access.Write | Access.Delete | Access.List);

var url = abs.GetSharedAccessSignature(
"my-sas-test-bucket", // No object/blob name, apply to the entire bucket/container
"mypolicy");
//END OF SERVER CODE


// Send url (string) to device by whichever means you see fit


//START OF DEVICE CODE (NO ACCESS KEY NEEDED)
var sas = new SharedAccessSignature(url);

await sas.PutObjectWithNameAsync("myobject", "Hello");// Create the object/blob

var hello = await sas.GetObjectAsStringAsync("myobject"); // Hello
await sas.PutObjectWithNameAsync("myobject", "Goodbye");// Set object content to 'Goodbye'

foreach (var name in await sas.ListObjectsAsync())
{
    await sas.DeleteObjectAsync(name);// Delete object
}
//END OF DEVICE CODE
```