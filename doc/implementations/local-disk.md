# Local Disk

Local disk implementation is baked into the Storage.Net library itself as local I/O is a part of framework core.

## Blobs

You can map a local directory as `IBlobStorage`

```csharp
IBlobStorage storage = StorageFactory.Blobs.DirectoryFiles(TestDir);
```

which simply stores them as local files. Subfolders are created on demand, as soon as you start introducing path separators into blob IDs.

alternatively, you can create it with a connection string:

```csharp
IBlobStorage storage = StorageFactory.Blobs.FromConnectionString("disk://path=path_to_directory");
```


## Tables

As for tables, you can map a local directory to `ITableStorage`

```csharp
ITableStorage tables = StorageFactory.Tables.CsvFiles(TestDir);
```

and data will be stored in CSV files in that directory. 

For each table a new subfolder will be created called *tableName*.partition and inside the folder you will have files for each partition key named *partitionName*.partition.csv.

## Messaging

There is no implementation for messaging on disk yet.