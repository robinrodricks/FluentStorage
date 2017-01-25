# Amazon AWS

Amazon AWS implementations reside in a separate package hosted on [NuGet](https://www.nuget.org/packages/Storage.Net.Amazon.Aws/). Follow the link for installation instructions. The package implements S3 storage only.

## Blobs

This document assumes you are already familiar [how blobs are implemented in Storage.Net](../blob-storage/index.md)

Blobs are implemented as [S3](https://aws.amazon.com/s3/) interface. 

The easiest way to create a blob storage is to use the factory method

> todo