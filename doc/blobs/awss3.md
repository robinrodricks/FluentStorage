# AWS S3

In order to use Microsoft Azure blob storage you need to reference [![NuGet](https://img.shields.io/nuget/v/Storage.Net.Amazon.Aws.svg)](https://www.nuget.org/packages/Storage.Net.Amazon.Aws/) package first. The provider wraps around the standard AWS SDK which is updated regularly.



There are a few overloads in this package, for instance:

```csharp
IBlobStorage storage = StorageFactory.Blobs.AmazonS3BlobStorage(string accessKeyId,
   string secretAccessKey,
   string bucketName,
   RegionEndpoint regionEndpoint = null);
```

Please see `StorageFactory.Blobs` factory entry for more options.

To create with a connection string, first reference the module:

```csharp
StorageFactory.Modules.UseAwsStorage();
```

Then construct using the following format:

```csharp
IBlobStorage storage = StorageFactory.Blobs.FromConnectionString("aws.s3://keyId=...;key=...;bucket=...;region=...");
```

where:
- **keyId** is (optional) access key ID.
- **key** is (optional) secret access key.
- **bucket** is bucket name.
- **region** is an optional value and defaults to `EU West 1` if not specified. At the moment of this wring the following regions are supported. However as we are using the official AWS SDK, when region information changes, storage.net gets automatically updated.
  - `us-east-1`
  - `us-east-2`
  - `us-west-1`
  - `us-west-2`
  - `eu-north-1`
  - `eu-west-1`
  - `eu-west-2`
  - `eu-west-3`
  - `eu-central-1`
  - `ap-northeast-1`
  - `ap-northeast-2`
  - `ap-northeast-3`
  - `ap-south-1`
  - `ap-southeast-1`
  - `ap-southeast-2`
  - `sa-east-1`
  - `us-gov-east-1`
  - `us-gov-west-1`
  - `cn-north-1`
  - `cn-northwest-1`
  - `ca-central-1`

If **keyId** and **key** are omitted, the AWS SDK's default approach to credential resolution will be used. For example: if running in Lambda, it will assume the Lambda execution role; if there are credentials configured in ~/.aws, it will use those; etc.  See [AWS SDK Documentation](https://docs.aws.amazon.com/sdk-for-net/v3/developer-guide/net-dg-config-creds.html) for more details.

#### Performance & Memory Consumption

By default, `S3 SDK` is *extremely ineffective* and I have no idea why. Can it be that .NET developers are not paying enough attention to good quality code or Amazon is just not good in programming? In particular, SDK doesn't deal very well with streaming. When you stream with S3 SDK, it **accumulates data in memory and only uploads it on flush!**. Whoever designed it this way has probably never built real software. Unfortunately, S3 has a limitation that data length needs to be known beforehand in order to create a file. See issues [here](https://github.com/aws/aws-sdk-net/issues/1095) and [here](https://github.com/aws/aws-sdk-net/issues/1073) for more information.

If you ever wonder why your application is slow, say thanks to Amazon engineers.


#### Native Operations

Native operations are exposed via [IAwsS3BlobStorageNativeOperations](../src/AWS/Storage.Net.Amazon.Aws/Blobs/IAwsS3BlobStorage.cs) interface.