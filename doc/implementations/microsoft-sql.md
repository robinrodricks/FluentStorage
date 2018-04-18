# Microsoft SQL Server

Microsoft Azure implementations reside in a separate package hosted on [NuGet](https://www.nuget.org/packages/Storage.Net.Mssql/). Follow the link for installation instructions.

This package implements reading and writing to tables.

### Creating a table client

The easiest way to create a table storage is to use the factory method

```csharp
using Storage.Net;

ITableStorage storage = StorageFactory.Tables.MssqlServer("connection_string");
```

You only need to provide a connection string.

Note that when writing to tables, if table doesn't exist, this library will attempt to create one using the first row's schema. If this is not what you want, you can either pre-create the table, or modify the table after it was created. Most notably, automatically generated table won't have best indexes and data types chosen are definitely not the ones you really want (for instance, for strings `NVARCHAR(MAX)` is selected). However, automatically generated schemas are extremely useful for prototyping.

The factory method also accepts an optional `SqlConfiguration` parameter where you can set 3 options:

- Name for Partition Key column.
- Name for Row Key column.
- Timeout for bulk copy operations.

The third option is somewhat important. If you're trying to insert more than `10` records at a time with `.InsertAsync()` and using at least .NET Standard 2.0 or at least .NET 4.5.2, the operation will switch to bulk-insert mode which means you will get a really good insert performance. However, these operations may time out if you supply a lot of rows, therefore you can increase the timeout in this field (default is 1 minute).