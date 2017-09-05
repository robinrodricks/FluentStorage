# Implementations

Implementations for specific technolgoies are seperated out to NuGet sub-packages, unless the technology is a part of OS and commonly accessible via core API.

This page lists known implementation of storage primitives known to the contributors.


|I'm interested in|Blobs|Tables|Queues|Package|Notes|
|-----------------|-----|------|------|-------|-----|
|Local Disk|x|x||[![NuGet](https://img.shields.io/nuget/v/Storage.Net.svg)](https://www.nuget.org/packages/Storage.Net/)|blobs map to local folders, tables map to csv files|
|Azure Storage|x|x|x|[![NuGet](https://img.shields.io/nuget/v/Storage.Net.Microsoft.Azure.Storage.svg)](https://www.nuget.org/packages/Storage.Net.Microsoft.Azure.Storage)||
|Azure Event Hub|||x|[![NuGet](https://img.shields.io/nuget/v/Storage.Net.Microsoft.Azure.EventHub.svg)](https://www.nuget.org/packages/Storage.Net.Microsoft.Azure.EventHub)||
|Azure Service Bus|||x|[![NuGet](https://img.shields.io/nuget/v/Storage.Net.Microsoft.Azure.ServiceBus.svg)](https://www.nuget.org/packages/Storage.Net.Microsoft.Azure.ServiceBus/)|queues and topics|
|Azure Key Vault|x|||[![NuGet](https://img.shields.io/nuget/v/Storage.Net.Microsoft.Azure.KeyVault.svg)](https://www.nuget.org/packages/Storage.Net.Microsoft.Azure.KeyVault)|[creating Key Vault](http://isolineltd.com/blog/2017/08/01/Creating-Azure-Key-Vault-for-programmatic-access)|
|Azure Service Fabric|x||x|[![NuGet](https://img.shields.io/nuget/v/Storage.Net.Microsoft.ServiceFabric.svg)](https://www.nuget.org/packages/Storage.Net.Microsoft.ServiceFabric)||
|[Azure Data Lake Store](microsoft-azure-datalakestore.md)|x|||[![NuGet](https://img.shields.io/nuget/v/Storage.Net.Microsoft.Azure.DataLake.Store.svg)](https://www.nuget.org/packages/Storage.Net.Microsoft.Azure.DataLake.Store/)||
|Amazon S3|x|||[![NuGet](https://img.shields.io/nuget/v/Storage.Net.Amazon.Aws.svg)](https://www.nuget.org/packages/Storage.Net.Amazon.Aws)||