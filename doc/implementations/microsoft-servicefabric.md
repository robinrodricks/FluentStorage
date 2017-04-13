# Microsoft Service Fabric

Microsoft Service Fabric implementations reside in a separate package hosted on [NuGet](https://www.nuget.org/packages/Storage.Net.Microsoft.ServiceFabric/). Follow the link for installation instructions. The package implements blobs and queues.

## Blobs

> todo

## Queues

> todo

# Appendix 1. Debugging and Contributing

Service Fabric library needs to run inside the cluster in order to debug it, therefore there are a few tricks made in order to make this debuggable, testable and stable.

## Running

There is another solution in `src/service-fabric` folder containing a simple service fabric application with one stateful service. This is to run and debug service fabric storage implementation.

This solution references `Storage.Net` core library and `Storage.Net.Microsoft.ServiceFabric` from the main source folder which allows to debug and edit the code in place while running on a local cluster. This approach is awesome because Service Fabric local clusters and not emulators so we are running as close to the metal as possible.

## Testing

todo