# Storage.Net [![NuGet](https://img.shields.io/nuget/v/Storage.Net.svg)](https://www.nuget.org/packages/Storage.Net/) ![](https://img.shields.io/azure-devops/build/aloneguid/7a9c6ed6-1a57-47cf-941f-f4b09440591b/36.svg) ![](https://img.shields.io/azure-devops/tests/aloneguid/Storage.Net/35.svg?label=integration%20tests)|

![](doc/slide.jpg)

Storage.NET is a field-tested .NET library that helps to achieve [polycloud techniques](https://www.thoughtworks.com/radar/techniques/polycloud). 

It provides generic interface for popular cloud storage providers like Amazon S3, Azure Service Bus, Azure Event Hub, Azure Storage, Azure Data Lake Store thus abstracting Messaging, Blob (object store for unsturctured data) and Table (NoSQL key-value store) services.

It also implements in-memory and on-disk versions of all the abstractions for faster local machine development. [Connection strings](doc/cs.md) are supported too!

## Intentions

**One Library To Rule Them All**

I'm not really sure why there are so many similar storage providers performing almost identical function but no standard. Why do we need to learn a new SDK to achieve something trivial we've done so many times before? I have no idea. If you don't either, use this library.

`Storage.Net` abstracts storage implementation like `blobs`, `tables` and `messages` from the .NET Applicatiion Developer. It's aimed to provide a generic interface regardless on which storage provider you are using. It also provides both synchronous and asynchronous alternatives of all methods and implements it to the best effort possible. 

Storage.Net supports **Azure Service Bus**, **Azure Event Hub**, **Azure Storage**, **Azure Data Lake Store**, **Amazon S3**, **Azure Key Vault** and many more, out of the box, with hassle-free configuration and zero learning path.

### Local Development

Storage.Net also implements inmemory and on disk versions of all the abstractions, therefore you can develop fast on local machine or use vendor free serverless implementations for parts of your applciation which don't require a separate third party backend at a particular point in development.

This framework supports `.NET 4.5.2` and `.NET Standard 1.6`, and all of the plugins exist for all frameworks.

## Implementations

![Storagetypes](doc/storagetypes.png)

Storage.Net defines three different storage types:

- [**Blob Storage**](doc/blob-storage/index.md) is used to store arbitrary files of any size, that do not have any structure. The data is essentially a binary file. Examples of a blog storage is Azure Blob Storage, Amazon S3, local folder etc.
- [**Key-Value Storage**](doc/key-value-storage/index.md) is essentialy a large dictionary where key points to some value. Examples are Azure Table Storage, etcd etc.
- [**Messaging**](doc/messaging/index.md) is an asynchronous mechanism to send and receive messages between disconnected systems. For instance MSMQ, Azure Service Bus, Amazon Simple Queue etc.

## Geting Started

### Blob Storage

Blob Storage stores files. A file has only two properties - `ID` and raw data. If you build an analogy with disk filesystem, file ID is a file name.

Blob Storage is really simple abstraction - you read or write file data by it's ID, nothing else.

The entry point to a blog storage is [IBlobStorage](src/Storage.Net/Blob/IBlobStorage.cs) interface. This interface is small but contains all possible methods to work with blobs, such as uploading and downloading data, listing storage contents, deleting files etc. The interface is kept small so that new storage providers can be added easily, without implementing a plethora of interface methods.

In addition to this interface, there are plency of extension methods which enrich the functionality, therefore you will see more methods than this interface actually declares. They add a lot of useful and functionality rich methods to work with storage. For instance, `IBlobStorage` upload functionality only works with streams, however extension methods allows you to upload text, stream, file or even a class as a blob. Extension methods are also provider agnostic, therefore all the rich functionality just works and doesn't have to be reimplemented in underlying data provider.

All the storage implementations can be created either directly or using factory methods available in the `Storage.Net.StorageFactory.Blobs` class. More methods appear in that class as you reference a **NuGet** package containing specific implementations, however there are a few built-in implementations available out of the box as well. After referencing an appropriate package from NuGet you can call to a storage factory to create a respective storage implementation:

![](doc/storagefactory-intellisense.gif)

You can also use connection strings to create blob storage instances. Connection strings are often useful if you want to completely abstract yourself from the underlying implementation. Please read the appropriate implementation details for connection string details. For instance, to create an instance of Azure Blob Storage provider you could write:

```csharp
IBlobStorage storage = StorageFactory.Blobs.FromConnectionString("azure.blobs://...parameters...");
```

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
            "storage name",
            "storage key");

         //upload it
         string content = "test content";
         using (var s = new MemoryStream(Encoding.UTF8.GetBytes(content)))
         {
            await storage.WriteAsync("mycontainer/someid", s);
         }

         //read back
         using (var s = new MemoryStream())
         {
            using (Stream ss = await storage.OpenReadAsync("mycontainer/someid"))
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
    IBlobStorage storage = StorageFactory.Blobs.AzureBlobStorage(
		"storage name",
		"storage key");

    //upload it
    await storage.WriteTextAsync("mycontainer/someid", "test content");

    //read back
    string content = await storage.ReadTextAsync("mycontainer/someid");
}
```

### Key-Value Storage

> TODO

### Messaging

> TODO

## Index of Supported Providers

There are various implementations/providers of different storage types:

- [In-Memory](doc/implementations/inmemory.md)
- [Local Disk](doc/implementations/local-disk.md) 
- **Microsoft Azure**
  - [Azure Storage (blobs/tables/queues)](doc/implementations/microsoft-azure.md) [![NuGet](https://img.shields.io/nuget/v/Storage.Net.Microsoft.Azure.Storage.svg)](https://www.nuget.org/packages/Storage.Net.Microsoft.Azure.Storage)
  - [Azure Event Hub](doc/implementations/microsoft-azure-eventhub.md) [![NuGet](https://img.shields.io/nuget/v/Storage.Net.Microsoft.Azure.EventHub.svg)](https://www.nuget.org/packages/Storage.Net.Microsoft.Azure.EventHub)
  - Azure Service Bus [![NuGet](https://img.shields.io/nuget/v/Storage.Net.Microsoft.Azure.ServiceBus.svg)](https://www.nuget.org/packages/Storage.Net.Microsoft.Azure.ServiceBus/)
  - [Azure Key Vault](doc/implementations/microsoft-azure-key-vault.md) [![NuGet](https://img.shields.io/nuget/v/Storage.Net.Microsoft.Azure.KeyVault.svg)](https://www.nuget.org/packages/Storage.Net.Microsoft.Azure.KeyVault)
  - [Azure Data Lake Store](doc/implementations/microsoft-azure-datalakestore.md) [![NuGet](https://img.shields.io/nuget/v/Storage.Net.Microsoft.Azure.DataLake.Store.svg)](https://www.nuget.org/packages/Storage.Net.Microsoft.Azure.DataLake.Store/)
  - [Azure Service Fabric](doc/implementations/microsoft-servicefabric.md) [![NuGet](https://img.shields.io/nuget/v/Storage.Net.Microsoft.ServiceFabric.svg)](https://www.nuget.org/packages/Storage.Net.Microsoft.ServiceFabric)
- Amazon S3 [![NuGet](https://img.shields.io/nuget/v/Storage.Net.Amazon.Aws.svg)](https://www.nuget.org/packages/Storage.Net.Amazon.Aws)

## Contributing

All contributions of any size and areas are welcome, being it coding, testing, documentation or consulting. The framework is heavily tested under stress with integration tests, in fact most of the code is tests, not implementation, and this approach is more preferred to adding unstable features.

### Code

Storage.Net tries to enforce idential behavior on all implementaions of storage interfaces to the smallest details possible and you will find a lot of test specifications which will help you to add another provider.

The solution is created in Visual Studio 2017 (Community Edition).

### Documentation

When I think of the best way to document a framework I tend to think that working examples are the best. Adding various real world scenarios with working code is more preferrable than just documenting an untested API.

### Reporting bugs or requesting features

Please use the GitHub issue tracker to do this.

### Support

You can get support by raising an issue here, or contacting me directly for consulting services.
