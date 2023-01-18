# FluentStorage


### One Interface To Rule Them All

FluentStorage, originally known as Storage.NET, is a field-tested .NET library that helps to achieve [polycloud techniques](https://www.thoughtworks.com/radar/techniques/polycloud).

It provides a generic interface for popular cloud storage providers like Amazon S3, Azure Service Bus, Azure Event Hub, Azure Storage, Azure Data Lake Store thus abstracting Blob and Messaging services.

It is written entirely in C#, with no external dependency.

FluentStorage is released under the permissive MIT License, so it can be used in both proprietary and free/open source applications.

I'm not really sure why there are so many similar storage providers performing almost identical function but no standard. Why do we need to learn a new SDK to achieve something trivial we've done so many times before? I have no idea. If you don't either, use this library.

Features:

* Abstracts storage implementation like `blobs`, `tables` and `messages` from the .NET Application Developer.

* Provides a generic interface regardless on which storage provider you are using.

* Provides both synchronous and asynchronous alternatives of all methods and implements it to the best effort possible. 

* Supports **Amazon S3**, **Azure Storage**, **Azure Service Bus**, **Azure Event Hub**, **Azure Data Lake Store**, **Azure Key Vault** and many more, out of the box, with hassle-free configuration and zero learning path.

* Implements inmemory and on disk versions of all the abstractions, therefore you can develop fast on a local machine or use vendor free serverless implementations for parts of your applciation which don't require a separate third party backend at a particular point in development.

* Supports `.NET Standard 2.0` and higher.

* Attempts to enforce idential behavior on all implementaions of storage interfaces to the smallest details possible and you will find a lot of test specifications which will help you to add another provider.


## Supported Cloud Services

![Slide](doc/slide.svg)


## Contributors

Special thanks to these awesome people who helped create FluentStorage! Shoutout to [Ivan Gavryliuk](https://github.com/aloneguid) for the original project [Storage.Net](https://github.com/aloneguid/storage).

<!---
<img src="https://github.com/robinrodricks/FluentStorage/raw/master/.github/contributors.png" />
-->

<a href="https://github.com/robinrodricks/FluentStorage/graphs/contributors">
  <img src="https://contributors-img.web.app/image?repo=robinrodricks/FluentStorage" />
</a>
