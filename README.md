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


## Supported Cloud Services

![Slide](doc/slide.svg)


## Releases

Stable binaries are released on NuGet, and contain everything you need to use Cloud Storage in your .NET/.NET Standard application.


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
