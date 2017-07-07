# Storage.Net [![Build status](https://ci.appveyor.com/api/projects/status/ymwk4hx0gb0dleyk?svg=true)](https://ci.appveyor.com/project/aloneguid/storage)

![](doc/slide.jpg)

## Intentions

`Storage.Net` abstracts storage implementation like `blobs`, `tables` and `messages` from the .NET Applicatiion Developer. It's aimed to provide a generic interface regardless on which storage provider you are using. It also provides both synchronous and asynchronous alternatives of all methods and implements it to the best effort possible.

Storage.Net also implements inmemory and on disk versions of all the abstractions, therefore you can develop fast on local machine or use vendor free serverless implementations for parts of your applciation which don't require a separate third party backend at a particular point in development.

This framework supports `.NET 4.5.1` and `.NET Standard 1.6`, however not all of the concrete implementations exist for both frameworks. When not otherwise mentioned assume that functionality is supported everywhere.

Storage.Net defines three different storage types:

- [**Blob Storage**](doc/blob-storage/index.md) is used to store arbitrary files of any size.
- [**Table Storage**](doc/table-storage/index.md) is a simplistic way to store non-relational tabular data.
- [**Messaging**](doc/messaging/index.md) is an asynchronous mechanism to send simple messages between disconnected systems.

## Implementations

There are various implementations of storage types curated on a [separate page](doc/implementations/index.md)

## Installation

All packages are available on `nuget` and I consider it the primary release target. This is the list of curated packages we know about:

## Contributing

All contributions of any size and areas are welcome, being it coding, testing, documentation or consulting. The framework is heavily tested under stress with integration tests, in fact most of the code is tests, not implementation, and this approach is more preferred to adding unstable features.

### Code

Storage.Net tries to enforce idential behavior on all implementaions of storage interfaces to the smallest details possible and you will find a lot of test specifications which will help you to add another provider.

The solution is created in Visual Studio 2017 (Community Edition).

### Documentation

When I think of the best way to document a framework I tend to think that working examples are the best. Adding various real world scenarios with working code is more preferrable than just documenting an untested API.

### Reporting bugs or requesting features

Please use the GitHub issue tracker to do this.