using Amazon;
using Storage.Net.Aws.Blob;
using Storage.Net.Blob;

namespace Storage.Net
{
   /// <summary>
   /// Factory class that implement factory methods for Amazon AWS implementation
   /// </summary>
   public static class Factory
   {

      /// <summary>
      /// Creates an Amazon S3 storage
      /// </summary>
      /// <param name="factory">Factory reference</param>
      /// <param name="accessKeyId">Access key ID</param>
      /// <param name="secretAccessKey">Secret access key</param>
      /// <param name="bucketName">Bucket name</param>
      /// <param name="regionEndpoint">Optionally set region endpoint. When not specified defaults to EU West</param>
      /// <returns>A reference to the created storage</returns>
      public static IBlobStorage AmazonS3BlobStorage(this IBlobStorageFactory factory,
         string accessKeyId,
         string secretAccessKey,
         string bucketName,
         RegionEndpoint regionEndpoint = null)
      {
         return new AwsS3BlobStorageProvider(accessKeyId, secretAccessKey, bucketName, regionEndpoint);
      }
   }
}
