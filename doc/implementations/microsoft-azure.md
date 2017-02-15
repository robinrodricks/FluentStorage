# Microsoft Azure

Microsoft Azure implementations reside in a separate package hosted on [NuGet](https://www.nuget.org/packages/Storage.Net.Microsoft.Azure/). Follow the link for installation instructions. The package implements all three aspects of storage - blobs, tables and queues.

> Current implementation only supports .NET 4.5 (Windows) and .NET Standard 1.6 with "portable-net451+win8" target. This is due to limitations in client library for .NET Core from Microsoft.

> Note that only Azure Storage functionality is supported in .NET Core because there is no impelmentation of underlying Service Bus libraries for .NET Core. .NET 4.5 version supports everything.

## Blobs

Blobs are interfacing with the official [Blob Storage](https://azure.microsoft.com/en-gb/services/storage/blobs/) and implement a good subset of functionality from the rich Blob Storage ecosystem in Azure.

This document assumes you are already familiar [how blobs are implemented in Storage.Net](../blob-storage/index.md)

### Creating the blob client

The easiest way to create a blob storage is to use the factory method

```csharp
using Storage.Net;

IBlobStorage storage = StorageFactory.Blobs.AzureBlobStorage("acc_name", "acc_key", "container_name");
```

You need to provide Azure specific information like account name and key. Container name is the root container where blob operations will refer to. If the container doesn't exist it will be created first.

This storage is working with `block blobs` only. We are planning to add `append blobs` support but that requires some architectural changes and as always you're welcome to help.

Another way to create the blob storage is using connection strings, as there is an overloaded method for this:

```csharp
using Storage.Net;

IBlobStorage storage = StorageFactory.Blobs.AzureBlobStorage("connection_string", "container_name");
```

Alternatively, you can construct the blob storage client from the SAS signature token like this:

> todo: create from SAS token

## Tables

> todo

## Messaging

> todo