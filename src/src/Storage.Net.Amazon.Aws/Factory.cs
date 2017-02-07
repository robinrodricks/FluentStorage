using Storage.Net.Aws.Blob;
using Storage.Net.Blob;

namespace Storage.Net
{
   /// <summary>
   /// Factory class that implement factory methods for Amazon AWS implementation
   /// </summary>
   public static class Factory
   {
      public static IBlobStorage AmazonS3BlobStorage(this IBlobStorageFactory factory,
         string accessKeyId,
         string secretAccessKey,
         string bucketName)
      {
         return new AwsS3BlobStorage(accessKeyId, secretAccessKey, bucketName);
      }
   }
}
