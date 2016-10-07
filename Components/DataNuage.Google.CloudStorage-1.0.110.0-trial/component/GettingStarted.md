DataNuage.Google.CloudStorage is a client library that uses Google Cloud Storage to store and retrieve any amount of data at any time, from anywhere.

support@datanuage.com

The first thing you need to do is create a Google account (if you don't already have one).
Once you have created an account, you need to create a project https://console.developers.google.com/project
From the project credentials, https://console.developers.google.com/project/YOUR_PROJECT_ID/apiui/credential your get your Client Id, Client Secret and Project Id.

With your Client Id, Client Secret and Project Id, you can go to a web page and authorize your application to use Google Cloud Storage which gives you a Success Code.

Finally, from that Success Code (temporary), you get a permanent Refresh Token that you can store and reuse later.

Your Client Id, Client Secret, Refresh Token and Project Id need to be passed as parameters of the constructor of the CloudStorage class.
Once you have an instance of CloudStorage, your can create buckets (similar to folders) and objects as in the example below.

YOU SHOULD ONLY UPLOAD DUMMY DATA WHILE USING THE TRIAL VERSION OF THE SOFTWARE AS EVERY TRIAL USER SHARES THE SAME GOOGLE CLOUD STORAGE ACCOUNT.
Any user of the trial version of the SOFTWARE will be able to view, copy, delete any data you might upload during the trial.

```csharp
using DataNuage.Google.CloudStorage;
...

	button.Click += async (_, __) =>
        {
            var random = new Random();

            var cs = new CloudStorage(...);

            var bucket = "my-globally-unique-bucket-name" + random.Next();

            await cs.CreateBucketAsync(bucket);

            await cs.PutObjectAsync(bucket,"myobject","Hello World");

            var s = await s3.GetObjectAsStringAsync(bucket, "myobject");

            await cs.DeleteObjectAsync(bucket, "myobject");

            await cs.DeleteBucketAsync(bucket);
        };
```

