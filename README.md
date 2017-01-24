# Storage.Net 

[![Visual Studio Team services](https://img.shields.io/vso/build/aloneguid/0227dea8-0e2f-40c1-b170-2e8830087355/15.svg)]()

## Intentions

`Storage.Net` abstracts storage implementation like `blobs`, `tables` and `messages` from the .NET Applicatiion Developer. It's aimed to provide a generic interface regardless on which storage provider you are using.

Storage.Net also implements inmemory and on disk versions of all the abstractions, therefore you can develop fast on local machine or use vendor free serverless implementations for parts of your applciation which don't require a separate third party backend at a particular point in development.

Storage.Net defines three different storage types:

- [**Blob Storage**](doc/blob-storage/index.md) is used to store arbitrary files of any size.
- **Table Storage** is a simplistic way to store non-relational tabular data.
- **Messaging** is an asynchronous mechanism to send simple messages between disconnected system.

## Installation

All packages are available on `nuget` and I consider it the primary release target. This is the list of curated packages we know about:

### Core

todo

> Help Wanted to write the documentation