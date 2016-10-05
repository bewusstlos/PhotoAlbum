With Oracle Mobile Cloud Service, you get the out-of-the-box services that every enterprise mobile app requires, with the consumer quality and high performance your users demand. Xamarin SDK for Oracle Mobile Cloud Service combines Xamarin native apps with the ability to define and implement new enterprise-ready APIs quickly and cleanly from one shared codebase.

## Key Highlights
* Integrate with Oracle MCS straight from Xamarin Platform, all in C#
* Require user authentication for secure access and data management
* Create, read, update, and delete storage objects.
* Easily register client applications for push notifications
* Log custom analytics events and receive app diagnostics
* Take advantage of existing authentication to call custom APIs


## Get Started
```
//Authenticate
bool success = await backend.Authorization.AuthenticateAsync("username", "password");

var storageService = backend.GetService<Storage>();
var collection = await storageService.GetCollectionAsync("SampleFiles");

//Insert all types of data into storage
var newObject = new StorageObject(collection);
newObject.Name = "TestFile.txt";
newObject.LoadPayload("This is a plain text example, we can insert images or other blob data",
   "plain/text");
await collection.PostObjectAsync(newObject);
```
