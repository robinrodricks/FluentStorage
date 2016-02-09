using Storage.Net.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Amazon;
using Amazon.S3;
using Amazon.Runtime;
using Amazon.S3.Model;
using Amazon.S3.Transfer;

namespace Storage.Net.Aws.Blob
{
   /// <summary>
   /// Amazon S3 storage adapter for blobs
   /// </summary>
   public class AwsS3BlobStorage : IBlobStorage
   {
      private readonly string _bucketName;
      private readonly AmazonS3Client _client;
      private readonly TransferUtility _fileTransferUtility;

      //https://github.com/awslabs/aws-sdk-net-samples/blob/master/ConsoleSamples/AmazonS3Sample/AmazonS3Sample/S3Sample.cs

      /// <summary>
      /// Creates a new instance of <see cref="AwsS3BlobStorage"/>
      /// </summary>
      /// <param name="accessKeyId"></param>
      /// <param name="secretAccessKey"></param>
      /// <param name="bucketName"></param>
      public AwsS3BlobStorage(string accessKeyId, string secretAccessKey, string bucketName)
      {
         if(accessKeyId == null) throw new ArgumentNullException(nameof(accessKeyId));
         if(secretAccessKey == null) throw new ArgumentNullException(nameof(secretAccessKey));
         if(bucketName == null) throw new ArgumentNullException(nameof(bucketName));

         _client = new AmazonS3Client(new BasicAWSCredentials(accessKeyId, secretAccessKey), RegionEndpoint.EUWest1);
         _fileTransferUtility = new TransferUtility(_client);
         _bucketName = bucketName;

         Initialise();

      }

      private void Initialise()
      {
         try
         {
            _client.PutBucket(new PutBucketRequest { BucketName = _bucketName });
         }
         catch(AmazonS3Exception ex)
         {
            if(ex.ErrorCode == "BucketAlreadyOwnedByYou")
            {
               //ignore this error as bucket already exists
            }
         }
      }

      public void Delete(string id)
      {
         throw new NotImplementedException();
      }

      /// <summary>
      /// Downloads object to the stream
      /// </summary>
      public void DownloadToStream(string id, Stream targetStream)
      {
         using(GetObjectResponse response = GetObject(id))
         {
            response.ResponseStream.CopyTo(targetStream);
         }
      }

      public bool Exists(string id)
      {
         throw new NotImplementedException();
      }

      /// <summary>
      /// Lists all buckets, optionaly filtering by prefix. Prefix filtering happens on client side.
      /// </summary>
      public IEnumerable<string> List(string prefix)
      {
         ListBucketsResponse response = _client.ListBuckets();

         if(string.IsNullOrEmpty(prefix))
         {
            return response.Buckets.Select(b => b.BucketName);
         }

         return response.Buckets.Where(b => b.BucketName.StartsWith(prefix)).Select(b => b.BucketName);
      }

      /// <summary>
      /// Opens AWS stream and returns it. Note that you absolutely have to dispose it yourself in order for this
      /// adapter to close the network connection properly.
      /// </summary>
      /// <param name="id"></param>
      /// <returns></returns>
      public Stream OpenStreamToRead(string id)
      {
         GetObjectResponse response = GetObject(id);
         return new AwsS3BlobStorageExternalStream(response);
      }

      /// <summary>
      /// Uploads the stream using transfer manager
      /// </summary>
      /// <param name="id">Unique resource ID</param>
      /// <param name="sourceStream">Stream to upload</param>
      public void UploadFromStream(string id, Stream sourceStream)
      {
         //http://docs.aws.amazon.com/AmazonS3/latest/dev/HLuploadFileDotNet.html

         _fileTransferUtility.Upload(sourceStream, _bucketName, id);
      }

      private GetObjectResponse GetObject(string key)
      {
         var request = new GetObjectRequest { BucketName = _bucketName, Key = key };

         try
         {
            GetObjectResponse response = _client.GetObject(request);
            return response;
         }
         catch(AmazonS3Exception ex)
         {
            TryHandleException(ex);
            throw;
         }
      }

      private static bool TryHandleException(AmazonS3Exception ex)
      {
         if(ex.ErrorCode == "NoSuchKey")
         {
            throw new StorageException(ErrorCode.NotFound, ex);
         }

         return false;
      }
   }
}
