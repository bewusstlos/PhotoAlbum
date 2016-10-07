DataNuage.Azure.Blob is a client library that uses Azure Blob Storage to store and retrieve any amount of data at any time, from anywhere.

support@datanuage.com

The first thing you need to do is create a Microosft Azure account http://azure.microsoft.com/en-gb/pricing/free-trial/
Then you need to create a Storage Account and go to Manage Access Keys.

Your storage account name and access key (primary or secondary, it doesn't matter) need to be passed as parameters of the constructor of the AzureBlobStorage class.
Once you have an instance of AzureBlobStorage, your can create buckets (aka Containers, similar to folders) and objects (aka Blobs) as in the example below.

YOU SHOULD ONLY UPLOAD DUMMY DATA WHILE USING THE TRIAL VERSION OF THE SOFTWARE AS EVERY TRIAL USER SHARES THE SAME AZURE STORAGE ACCOUNT.
Any user of the trial version of the SOFTWARE will be able to view, copy, delete any data you might upload during the trial.

```csharp
using DataNuage.Azure.Blob;
...

	button.Click += async (_, __) =>
        {
            var random = new Random();

            var cs = new AzureBlobStorage("<Your Azure Account Name> - ignored by Trial version",
                        "<Your Azure Blob Access Key> - ignored by Trial version");

            var bucket = "my-unique-bucket-name" + random.Next();

            await cs.CreateBucketAsync(bucket);

            await cs.PutObjectAsync(bucket,"myobject","Hello World");

            var s = await s3.GetObjectAsStringAsync(bucket, "myobject");

            await cs.DeleteObjectAsync(bucket, "myobject");

            await cs.DeleteBucketAsync(bucket);
        };
```

