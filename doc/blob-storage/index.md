# Blob Storage

Blob Storage stores files. A file has only two properties - `ID` and raw data. If you build an analogy with disk filesystem, file ID is a file name.

Blob Storage is really simple abstraction - you read or write file data by it's ID, nothing else.

## Using

The entry point to a blog storage is `IBlobStorage` interface. This interface is small but contains all possible methods to work with blobs.

Usually you instantiate `IBlobStorage` with a specific instance implementing it, for instance Amazon S3 or Microsoft Azure Blob Storage, or even a local file system. The framework makes it trivial to create one.

Streaming is heavily utilised and all the interface methods always prefer streamed data.

## Use Cases

These example use cases simulate some most common blob operations which should help you to get started.

### Save file to Azure Blob Storage and read it later

todo

### Save file to a specific folder

todo

### List all files in a folder

todo

### Copy folder from the local filesystem to Amazon S3 preserving folder structure

todo

