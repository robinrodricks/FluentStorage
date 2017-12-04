# Microsoft SQL Server

Microsoft Azure implementations reside in a separate package hosted on [NuGet](https://www.nuget.org/packages/Storage.Net.Mssql/). Follow the link for installation instructions. The package implements all three aspects of storage - blobs, tables and queues.

This package implements reading and writing to tables.


### Creating the blob client

The easiest way to create a blob storage is to use the factory method

```csharp
using Storage.Net;

ITableStorage storage = StorageFactory.Tables.MssqlServer("connection_string");
```

You only need to provide a connection string.

Note that when writing to tables, if table doesn't exist, this library will attempt to create one using the first row's schema. If this is not what you want, you can either pre-create the table, or modify the table after it was created.