# Implementations

Implementations for specific technolgoies are seperated out to NuGet sub-packages, unless the technology is a part of OS and commonly accessible via core API.

This page lists known implementation of storage primitives known to the contributors.


|I'm interested in|Implemented As|Package|Notes|
|-----------------|--------------|-------|-----|
|Local disk|blobs, tables|[![NuGet](https://img.shields.io/nuget/v/Storage.Net.svg)](https://www.nuget.org/packages/Storage.Net/)|blobs map to local folders, tables map to csv files|
|Azure Blob Storage|blobs|[![NuGet](https://img.shields.io/nuget/v/Storage.Net.Microsoft.Azure.Storage.svg)](https://www.nuget.org/packages/Storage.Net.Microsoft.Azure.Storage)||
|Azure Table Storage|tables|[![NuGet](https://img.shields.io/nuget/v/Storage.Net.Microsoft.Azure.Storage.svg)](https://www.nuget.org/packages/Storage.Net.Microsoft.Azure.Storage)||
|Azure Storage Queues|queues|[![NuGet](https://img.shields.io/nuget/v/Storage.Net.Microsoft.Azure.Storage.svg)](https://www.nuget.org/packages/Storage.Net.Microsoft.Azure.Storage)||
|Azure Event Hub|queues|[![NuGet](https://img.shields.io/nuget/v/Storage.Net.Microsoft.Azure.EventHub.svg)](https://www.nuget.org/packages/Storage.Net.Microsoft.Azure.EventHub)||
|Azure Service Bus|queues|[![NuGet](https://img.shields.io/nuget/v/Storage.Net.Microsoft.Azure.ServiceBus.svg)](https://www.nuget.org/packages/Storage.Net.Microsoft.Azure.ServiceBus/)||
|Azure Key Vault|blobs|[![NuGet](https://img.shields.io/nuget/v/Storage.Net.Microsoft.Azure.KeyVault.svg)](https://www.nuget.org/packages/Storage.Net.Microsoft.Azure.KeyVault)|[creating Key Vault](http://isolineltd.com/blog/2017/08/01/Creating-Azure-Key-Vault-for-programmatic-access)|
|Azure Service Fabric|blobs, queues|[![NuGet](https://img.shields.io/nuget/v/Storage.Net.Microsoft.ServiceFabric.svg)](https://www.nuget.org/packages/Storage.Net.Microsoft.ServiceFabric)||
|Amazon S3|blobs|[![NuGet](https://img.shields.io/nuget/v/Storage.Net.Amazon.Aws.svg)](https://www.nuget.org/packages/Storage.Net.Amazon.Aws)||