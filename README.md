![FluentStorage](https://github.com/robinrodricks/FluentStorage/raw/develop/.github/logo.png)

[![Version](https://img.shields.io/nuget/vpre/FluentStorage.svg)](https://www.nuget.org/packages/FluentStorage)
[![Downloads](https://img.shields.io/nuget/dt/FluentStorage.svg)](https://www.nuget.org/packages/FluentStorage)
[![GitHub contributors](https://img.shields.io/github/contributors/robinrodricks/FluentStorage.svg)](https://github.com/robinrodricks/FluentStorage/graphs/contributors)
[![Codacy Badge](https://app.codacy.com/project/badge/Grade/8bc33aa55cb8494da3a7a07dba5316f7)](https://www.codacy.com/gh/robinrodricks/FluentStorage/dashboard)
[![License](https://img.shields.io/github/license/robinrodricks/FluentStorage.svg)](https://github.com/robinrodricks/FluentStorage/blob/develop/LICENSE)


### One Interface To Rule Them All

FluentStorage, originally known as Storage.NET, is a field-tested polycloud .NET cloud storage library that helps you interface with multiple cloud providers from a single unified interface.

It provides a generic interface for popular cloud storage providers like Amazon S3, Azure Service Bus, Azure Event Hub, Azure Storage, Azure Data Lake Store thus abstracting Blob and Messaging services.

It is written entirely in C#, with no external dependency.

FluentStorage is released under the permissive MIT License, so it can be used in both proprietary and free/open source applications.


## Features

* Abstracts storage implementation like `blobs`, `tables` and `messages` from the .NET Application Developer.

* Provides a generic interface regardless on which storage provider you are using.

* Provides both synchronous and asynchronous alternatives of all methods and implements it to the best effort possible. 

* Supports **Amazon S3**, **Azure Storage**, **Azure Service Bus**, **Azure Event Hub**, **Azure Data Lake Store**, **Azure Key Vault** and many more, out of the box, with hassle-free configuration and zero learning path.

* Implements inmemory and on disk versions of all the abstractions, therefore you can develop fast on a local machine or use vendor free serverless implementations for parts of your applciation which don't require a separate third party backend at a particular point in development.

* Supports .NET Standard 2.0 and higher.

* Attempts to enforce idential behavior on all implementaions of storage interfaces to the smallest details possible and you will find a lot of test specifications which will help you to add another provider.



## What's New

In 2023 we added:

* **SFTP** provider [SSH.NET](https://github.com/sshnet/SSH.NET) added 
* **FTP** provider [FluentFTP](https://github.com/robinrodricks/FluentFTP) updated to v44
* **AWS** Nuget bumped to latest versions



## Supported Cloud Services

![Slide](https://raw.githubusercontent.com/robinrodricks/FluentStorage/develop/.github/providers.svg)



## Releases

Stable binaries are released on NuGet, and contain everything you need to use Cloud Storage in your .NET/.NET Standard application.


| Package      		| Latest Version	|  Downloads	|  Documentation	| 
|---------------		|-----------	|-----------		|-----------		|
| **[FluentStorage](https://www.nuget.org/packages/FluentStorage)**      	|     [![Version](https://img.shields.io/nuget/vpre/FluentStorage.svg)](https://www.nuget.org/packages/FluentStorage) 		|  [![Downloads](https://img.shields.io/nuget/dt/FluentStorage.svg)](https://www.nuget.org/packages/FluentStorage) | [Standard](https://github.com/robinrodricks/FluentStorage/wiki/Standard-Storage) |
| **[FluentStorage.AWS](https://www.nuget.org/packages/FluentStorage.AWS)**      	|     [![Version](https://img.shields.io/nuget/vpre/FluentStorage.AWS.svg)](https://www.nuget.org/packages/FluentStorage.AWS) 		|  [![Downloads](https://img.shields.io/nuget/dt/FluentStorage.AWS.svg)](https://www.nuget.org/packages/FluentStorage.AWS) | [S3](https://github.com/robinrodricks/FluentStorage/wiki/AWS-S3-Storage), [SQS](https://github.com/robinrodricks/FluentStorage/wiki/AWS-SQS) |
| **[FluentStorage.GCP](https://www.nuget.org/packages/FluentStorage.GCP)**      	|     [![Version](https://img.shields.io/nuget/vpre/FluentStorage.GCP.svg)](https://www.nuget.org/packages/FluentStorage.GCP) 		|  [![Downloads](https://img.shields.io/nuget/dt/FluentStorage.GCP.svg)](https://www.nuget.org/packages/FluentStorage.GCP) | [GCP](https://github.com/robinrodricks/FluentStorage/wiki/Google-Cloud-Storage) |
| **[FluentStorage.Databricks](https://www.nuget.org/packages/FluentStorage.Databricks)**      	|     [![Version](https://img.shields.io/nuget/vpre/FluentStorage.Databricks.svg)](https://www.nuget.org/packages/FluentStorage.Databricks) 		|  [![Downloads](https://img.shields.io/nuget/dt/FluentStorage.Databricks.svg)](https://www.nuget.org/packages/FluentStorage.Databricks) | [Databricks](https://github.com/robinrodricks/FluentStorage/wiki/Databricks-Storage) |
| **[FluentStorage.FTP](https://www.nuget.org/packages/FluentStorage.FTP)**      	|     [![Version](https://img.shields.io/nuget/vpre/FluentStorage.FTP.svg)](https://www.nuget.org/packages/FluentStorage.FTP) 		|  [![Downloads](https://img.shields.io/nuget/dt/FluentStorage.FTP.svg)](https://www.nuget.org/packages/FluentStorage.FTP) | [FTP](https://github.com/robinrodricks/FluentStorage/wiki/FTP-Storage) |
| **[FluentStorage.SFTP](https://www.nuget.org/packages/FluentStorage.SFTP)**      	|     [![Version](https://img.shields.io/nuget/vpre/FluentStorage.SFTP.svg)](https://www.nuget.org/packages/FluentStorage.SFTP) 		|  [![Downloads](https://img.shields.io/nuget/dt/FluentStorage.SFTP.svg)](https://www.nuget.org/packages/FluentStorage.SFTP) | [SFTP](https://github.com/robinrodricks/FluentStorage/wiki/SFTP-Storage) |
| **[FluentStorage.Azure.Blobs](https://www.nuget.org/packages/FluentStorage.Azure.Blobs)**      	|     [![Version](https://img.shields.io/nuget/vpre/FluentStorage.Azure.Blobs.svg)](https://www.nuget.org/packages/FluentStorage.Azure.Blobs) 		|  [![Downloads](https://img.shields.io/nuget/dt/FluentStorage.Azure.Blobs.svg)](https://www.nuget.org/packages/FluentStorage.Azure.Blobs) | [Blob](https://github.com/robinrodricks/FluentStorage/wiki/Azure-Blob-Storage) |
| **[FluentStorage.Azure.Files](https://www.nuget.org/packages/FluentStorage.Azure.Files)**      	|     [![Version](https://img.shields.io/nuget/vpre/FluentStorage.Azure.Files.svg)](https://www.nuget.org/packages/FluentStorage.Azure.Files) 		|  [![Downloads](https://img.shields.io/nuget/dt/FluentStorage.Azure.Files.svg)](https://www.nuget.org/packages/FluentStorage.Azure.Files) | [File](https://github.com/robinrodricks/FluentStorage/wiki/Azure-Blob-Storage) |
| **[FluentStorage.Azure.EventHub](https://www.nuget.org/packages/FluentStorage.Azure.EventHub)**      	|     [![Version](https://img.shields.io/nuget/vpre/FluentStorage.Azure.EventHub.svg)](https://www.nuget.org/packages/FluentStorage.Azure.EventHub) 		|  [![Downloads](https://img.shields.io/nuget/dt/FluentStorage.Azure.EventHub.svg)](https://www.nuget.org/packages/FluentStorage.Azure.EventHub) | --- |
| **[FluentStorage.Azure.ServiceBus](https://www.nuget.org/packages/FluentStorage.Azure.ServiceBus)**      	|     [![Version](https://img.shields.io/nuget/vpre/FluentStorage.Azure.ServiceBus.svg)](https://www.nuget.org/packages/FluentStorage.Azure.ServiceBus) 		|  [![Downloads](https://img.shields.io/nuget/dt/FluentStorage.Azure.ServiceBus.svg)](https://www.nuget.org/packages/FluentStorage.Azure.ServiceBus) | [ServiceBus](https://github.com/robinrodricks/FluentStorage/wiki/Azure-Service-Bus) |
| **[FluentStorage.Azure.KeyVault](https://www.nuget.org/packages/FluentStorage.Azure.KeyVault)**      	|     [![Version](https://img.shields.io/nuget/vpre/FluentStorage.Azure.KeyVault.svg)](https://www.nuget.org/packages/FluentStorage.Azure.KeyVault) 		|  [![Downloads](https://img.shields.io/nuget/dt/FluentStorage.Azure.KeyVault.svg)](https://www.nuget.org/packages/FluentStorage.Azure.KeyVault) | [KeyVault](https://github.com/robinrodricks/FluentStorage/wiki/Azure-Key-Vault) |
| **[FluentStorage.Azure.ServiceFabric](https://www.nuget.org/packages/FluentStorage.Azure.ServiceFabric)**      	|     [![Version](https://img.shields.io/nuget/vpre/FluentStorage.Azure.ServiceFabric.svg)](https://www.nuget.org/packages/FluentStorage.Azure.ServiceFabric) 		|  [![Downloads](https://img.shields.io/nuget/dt/FluentStorage.Azure.ServiceFabric.svg)](https://www.nuget.org/packages/FluentStorage.Azure.ServiceFabric) | --- |
| **[FluentStorage.Azure.Queues](https://www.nuget.org/packages/FluentStorage.Azure.Queues)**      	|     [![Version](https://img.shields.io/nuget/vpre/FluentStorage.Azure.Queues.svg)](https://www.nuget.org/packages/FluentStorage.Azure.Queues) 		|  [![Downloads](https://img.shields.io/nuget/dt/FluentStorage.Azure.Queues.svg)](https://www.nuget.org/packages/FluentStorage.Azure.Queues) | [Queue](https://github.com/robinrodricks/FluentStorage/wiki/Azure-Queue-Storage) |
| **[FluentStorage.Azure.DataLake](https://www.nuget.org/packages/FluentStorage.Azure.DataLake)**      	|     [![Version](https://img.shields.io/nuget/vpre/FluentStorage.Azure.DataLake.svg)](https://www.nuget.org/packages/FluentStorage.Azure.DataLake) 		|  [![Downloads](https://img.shields.io/nuget/dt/FluentStorage.Azure.DataLake.svg)](https://www.nuget.org/packages/FluentStorage.Azure.DataLake) | [DataLake](https://github.com/robinrodricks/FluentStorage/wiki/Azure-Data-Lake) |




## Platform Support

FluentStorage works on .NET and .NET Standard/.NET Core.

| Platform      		| Binaries Folder	| 
|---------------		|-----------		|
| **.NET 5.0**      	| net50     		| 
| **.NET 6.0**      	| net60     		| 
| **.NET Standard 2.0** | netstandard2.0	| 
| **.NET Standard 2.1** | netstandard2.1	| 

FluentStorage is also supported on these platforms: (via .NET Standard)

  - **Mono** 4.6
  - **Xamarin.iOS** 10.0
  - **Xamarin.Android** 10.0
  - **Universal Windows Platform** 10.0

Binaries for all platforms are built from a single Visual Studio Project. You will need the latest [Visual Studio](https://visualstudio.microsoft.com/downloads/) to build or contribute to FluentStorage.



## Documentation

Check the [Wiki](https://github.com/robinrodricks/FluentStorage/wiki).


## Contributors

Special thanks to these awesome people who helped create FluentStorage! Shoutout to [Ivan Gavryliuk](https://github.com/aloneguid) for the original project [Storage.Net](https://github.com/aloneguid/storage).

<!---
<img src="https://github.com/robinrodricks/FluentStorage/raw/develop/.github/contributors.png" />
-->

<a href="https://github.com/robinrodricks/FluentStorage/graphs/contributors">
  <img src="https://contributors-img.web.app/image?repo=robinrodricks/FluentStorage" />
</a>
