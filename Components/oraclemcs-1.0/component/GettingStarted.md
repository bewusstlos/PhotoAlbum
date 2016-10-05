The included sample has a number of configuration steps that need to be done for the app to function fully. Referer to the [Using Mobile Cloud Service](https://docs.oracle.com/cloud/latest/mobilecs_gs/index.html) guide for further information.

1. Sign up for an [Oracle Mobile Cloud Service](https://cloud.oracle.com/mobile) account if you do not have have one already.

2. Follow the steps outlined in the [Set up a Mobile Backend](https://docs.oracle.com/cloud/latest/mobilecs_gs/MCSUA/GUID-2E3B0B63-6DE6-4CF8-9FC1-BCA6F4EF4F7E.htm#MCSUA124) guide. For the sample app, you need to call your backend "XamarinBackend".

3. Now you need to create a user for your new backend. Follow the steps outlined in the [Getting Started with User Management](http://docs.oracle.com/cloud/latest/mobilecs_gs/MCSGU/mcs-gs-ums.html) guide. Make a note of the role that you assign to the test user.

4. [Create a storage collection](http://www.oracle.com/pls/topic/lookup?ctx=cloud&id=mcsgs_storage) to be used by the sample app (optional). Instructions in sections 7-6 and 7-7, "Creating a Collection" of Using MCS. Use the name "XamarinFiles" for the collection for the sample app. Assign the role you created earlier "Read-Write Access" permissions to the collection. 

5. Create a custom API for the sample app (optional). The custom API will use the sample data feature of MCS. See the [Getting Started with APIs and Custom Code](http://www.oracle.com/pls/topic/lookup?ctx=cloud&id=mcsgs_customapis) guide for more information.  
  * Set the API Name to "customapitest"
  * Add a new resource with the path "test"
  * Add a new "GET" method.
  * Switch to the response tab and click "Add Response".
  * Click "Add Media Type" and set the example response to "On"
  * Click "Add Example" and enter the response body:
  ```
  {
	"sample": "This is a test response from the custom api"
  }
  ```
  * Click "Save"

6. Configure push notifications (optional). Push notifications require you to create apps in the Google Play Developer console and with the Apple Developer Center. You can refer to the [Getting Started with Notifications](http://docs.oracle.com/cloud/latest/mobilecs_gs/MCSGN/mcs-gs-notifications.html) guide for details.

###Using the SDK
#### Configuration
Create a McsConfiguration.json file at the root of your project. 
```
{
    "mobileBackends": {
        "BackendNameHere": {
            "default": true,
            "baseUri": "http://baseUri.com",
            "applicationKey": "applicationKey",
            "authorization": {
                "basicAuth": {
                    "backendId": "backendId",
                    "anonymousToken": "anonymousToken"
                }
            }
        }
    },
    "logLevel": "Verbose"
}
```
#### Intial Setup
```
//Configure the MobileBackendManager, here we load from embedded json
var json = ResourceLoader.GetEmbeddedResourceStream(Assembly.GetAssembly(typeof(Application)),
   "McsConfiguration.json");
MobileBackendManager.Manager.Configuration = new MobileBackendManagerConfiguration(json);

var backend = MobileBackendManager.Manager.DefaultMobileBackend;


//You must authenticate first
bool success = await backend.Authorization.AuthenticateAsync("username", "password");
```

#### Storage

```
var storageService = backend.GetService<Storage>();
var collection = await storageService.GetCollectionAsync("XamarinFiles");

//Insert all types of data into storage
var newObject = new StorageObject(collection);
newObject.Name = "TestFile.txt";
newObject.LoadPayload("This is a plain text example, we can insert images or other blob data",
   "plain/text");
newObject = await collection.PostObjectAsync(newObject);

//Get the object by Id
var retrievedObject = await collection.GetObjectAsync(newObject.Id);

//Update the object
retrievedObject.LoadPayload("This is a plain text example updated", "plain/text");
retrievedObject = await collection.PutObjectAsync(newObject);

//Get all objects from the collection page by page
var allObjects = await collection.GetObjectsAsync(0, 100);

//Delete the object
await collection.DeleteObjectAsync(retrievedObject.Id);

```

#### Analytics

```
var analyticsService = backend.GetService<Analytics>();

var analyticsEvent = new AnalyticsEvent("TheCustomEvent");
analyticsEvent.Properties["sample"] = "data";
analyticsService.LogEvent(analyticsEvent);

analyticsService.LogEvent("AnotherEvent");

await analyticsService.FlushAsync();

```

#### Notifications

```
var notificationsService = backend.GetService<Notifications>();

//Register
await notificationsService.RegisterForNotificationsAsync(   
   "Your device token returned from provider");

//De-register
await notificationsService.RegisterForNotificationsAsync(   
   "Your device token returned from provider");
      

```

#### Calling Custom APIs

```
using (var client = backend.CreateHttpClient())
{

    var response =
        await client.GetAsync(backend.CustomCodeUri + "/customapitest/test");
    response.EnsureSuccessStatusCode();
    var json = await response.Content.ReadAsStringAsync();

}

```
