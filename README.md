# Storage

![](https://aloneguid.visualstudio.com/DefaultCollection/_apis/public/build/definitions/323c5f4c-c814-452d-9eaf-1006c83fd44c/2/badge)

Storage.Net abstracts storage implementation from the applicatiion developer. The library started as an internal experiment in one of the companies I worked for and quickly grown into a separate library as I saw more and more repeating patterns.

Storage.Net abstracts the following types of storage patterns:

- [Blobs](https://github.com/aloneguid/storage/wiki/Blobs)
- [Simple tables](https://github.com/aloneguid/storage/wiki/Tables)
- [Queues](https://github.com/aloneguid/storage/wiki/Queues)

The biggest advantage of Storage.Net framework comparing to similar frameworks like [NServiceBus](http://particular.net/nservicebus) or [MassTransit](http://masstransit-project.com/) and others is that it's not just aimed to abstract different big vendor implementation, but to also define a common easy standard for those technologies.

Storage.Net also implements inmemory and on disk versions of all the abstractions, therefore you can develop fast on local machine or use vendor free serverless implementations for parts of your applciation which don't require a separate third party backend yet.

Read the [Wiki](https://github.com/aloneguid/storage/wiki) for more information on how to work with those.

This framework is available through NuGet:

- [Core Package](https://www.nuget.org/packages/Storage.Net)
- [Windows Azure Package](https://www.nuget.org/packages/Storage.Net.WindowsAzure)
