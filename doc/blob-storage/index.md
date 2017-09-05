# Blob Storage

Blob Storage stores files. A file has only two properties - `ID` and raw data. If you build an analogy with disk filesystem, file ID is a file name.

Blob Storage is really simple abstraction - you read or write file data by it's ID, nothing else.

## Using

The entry point to a blog storage is [IBlobStorageProvider](../../src/Storage.Net/Blob/IBlobStorageProvider.cs) interface. This interface is small but contains all possible methods to work with blobs, such as uploading and downloading data, listing storage contents, deleting files etc. The interface is kept small so that new storage providers can be added easily, without implementing a plethora of interface methods.

Although, normally you wouldn't use this interface directly in your code, and insted make use of [BlobStorage](../../src/Storage.Net/Blob/BlobStorage.cs) class, which accepts `IBlobStorageProvider` in it's constructor and exposes a lot of useful and functionality rich methods to work with storage. For instance, `IBlobStorageProvider` upload functionality only works with streams, however `BlobStorage` allows you to upload text, stream, file or even a class as a blob. This is because `BlobStorage` is built on top of it and simply uses streaming underneach to achieve this rich feature set. `BlobStorage` is also provider agnostic, therefore all the rich functionality just works and doesn't have to be reimplemented in underlying data provider.

Usually, first you would instantiate `IBlobStorageProvider` with a specific instance implementing it, for instance Amazon S3 or Microsoft Azure Blob Storage, or even a local file system. The framework makes it trivial to create one.

All the storage implementations can be created either directly or using factory methods available in the `Storage.Net.StorageFactory.Blobs` class. More methods appear in that class as you reference an assembly containing specific implementations.

## Use Cases

These example use cases simulate some most common blob operations which should help you to get started.

### Save file to Azure Blob Storage and read it later

In this example we create a blob storage implementation which happens to be Microsoft Azure blob storage. The project is referencing an appropriate [nuget package](https://www.nuget.org/packages/Storage.Net.Microsoft.Azure.Storage). As blob storage methods promote streaming we create a `MemoryStream` over a string for simplicity sake. In your case the actual stream can come from a variety of sources.

```csharp
using Storage.Net;
using Storage.Net.Blob;
using System.IO;
using System.Text;

namespace Scenario
{
   public class DocumentationScenarios
   {
      public async Task RunAsync()
      {
         //create the storage using a factory method
         IBlobStorage storage = StorageFactory.Blobs.AzureBlobStorage(
            "storage key",
            "storage secret",
            "containername");

         //upload it
         string content = "test content";
         using (var s = new MemoryStream(Encoding.UTF8.GetBytes(content)))
         {
            await storage.WriteAsync("someid", s);
         }

         //read back
         using (var s = new MemoryStream())
         {
            using (Stream ss = await storage.OpenReadAsync("someid"))
            {
               await ss.CopyToAsync(s);

               //content is now "test content"
               content = Encoding.UTF8.GetString(s.ToArray());
            }
         }
      }
   }
}
```

This is really simple, right? However, the code looks really long and boring. If I need to just save and read a string why the hell do I need to dance around with streams? That was examply my point when trying to use external SDKs. Why do we need to work in an ugly way if all we want to do is something simple? Therefore with Storage.Net you can decrease this code to just two lines of code:

```csharp
public async Task BlobStorage_sample2()
{
    IBlobStorageProvider provider = StorageFactory.Blobs.AzureBlobStorage(
		TestSettings.Instance.AzureStorageName,
		TestSettings.Instance.AzureStorageKey,
		"container name");
	BlobStorage storage = new BlobStorage(provider);

    //upload it
    await storage.WriteTextAsync("someid", "test content");

    //read back
    string content = await storage.ReadTextAsync("someid");
}
```

What's happening here? Instead of using a low-leve `IBlobStorageProvider` API we create a `BlobStorage` instance, which has a lot of nice utility methods, and one of them is working with text directly. Note that all of the methods are `async` promoting you to use the async programming achieving the best performance and less resource utilization.

### Save file to a specific folder

This scenario demonstrates how to save files to a folder on local disk. Notice that we are still using `IBlobStorageProvider` interface and don't really care where we are writing to. Here is how we create an instance of `IBlobStorageProvider` which is mapped to `c:\tmp\files` folder:

```csharp
IBlobStorageProvider provider = StorageFactory.Blobs.DirectoryFiles(new DirectoryInfo("c:\\tmp\\files"));
var storage = new BlobStorage(provider);
```

Now let's create a blob called `test.txt` with sample content and see what happened to that folder:

```csharp
await storage.WriteTextAsync("text.txt", "test content");
```

As you can see on disk a file was created with the same name as blob ID:

![Dirtextfile](dirtextfile.png)

This is close, but not exactly what we want, right? I'd like to save it to a specific folder. There is no interface method though to specify the folder name. However, the disk implementation treats forward slashes as folder separators, therefore you can place a file in the folder if you name it like this: `level 0/level 1/in the folder.log`. For example:

```csharp
string subfolderBlobId = StoragePath.Combine("level 0", "level 1", "in the folder.log");
await storage.WriteTextAsync(subfolderBlobId, "test content");
```

Looking back to folder contents:

![Dirtextfile](dirtextfileindir.png)

Job done.

### List all files in a folder

To list the storage contents, you can use `ListAsync` method. It's super rich in functionality, and can do almost everything you want from a storage. Storage.Net sees a storage as a list of files and folders, nested in each other.

`ListAsync` accept a single parameter which is a `ListOptions` class instance, containing different filtering options:

- `FolderPath` to specify root path in the storage to start listing from i.e. root folder.
- `Prefix` allows you to specify file or folder prefix, i.e. only objects starting from this prefix will be included in the result.
- `Recurse` specifies whether to go into subfolders to fetch more results.
- `MaxResults` allows to limit the amount of data returned.

The parameters are self explanatory. If you just want to list all files in all folders:

```csharp
await storage.ListAsync(new ListOptions { Recurse = true });
```

to get only first 10 items from a folder called `folder1`:

```csharp
await storage.ListAsync(new ListOptions { FolderPath = "/folder1", Recurse = false });
```

`ListAsync` returns a list of `BlobId` objects which contain basic information about objects returned, such as:

- `Id` of the object, not containing full path.
- `Kind` of the object which can be either file or folder.
- `FolderPath` telling which folder it resides in.
- `FullPath` is useful to refer to the object in other methods which require a full path to the object.

### Copy files between different storage providers

This sample is useful if you want to transfer files between two storage providers like from Microsot Azure to Amazon S3 or vice versa. Or copy files from the local disk to one of the cloud providers, it doesn't matter as we are using the same interface. You can even build your own blob transfer utility which supports all of the cloud providers.

> todo

## Why BlobStorage?

If all of the examples are using `BlobStorage` and not `IBlobStorageProvider` why do we need the provider at all? Wouldn't it be easier if all factory methods would just return `BlobStorage`? Yes and no. The thing is, `IBlobStorageProvider` is extremely useful in IoC patterns, and when you do dependency injection and testing it's nice to have an interface instead of a fat class. If your code doesn't need it, adding one more line is not that of a big deal, right?