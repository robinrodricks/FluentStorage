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
using NetBox;
using System.Threading.Tasks;

namespace Storage.Net.Aws.Blob
{
   /// <summary>
   /// Amazon S3 storage adapter for blobs
   /// </summary>
   public class AwsS3BlobStorage : AsyncBlobStorage
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
         _client = new AmazonS3Client(new BasicAWSCredentials(accessKeyId, secretAccessKey), RegionEndpoint.EUWest1);
         _fileTransferUtility = new TransferUtility(_client);
         _bucketName = bucketName ?? throw new ArgumentNullException(nameof(bucketName));

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

      /// <summary>
      /// deletes object from current bucket
      /// </summary>
      public override async Task DeleteAsync(string id)
      {
         GenericValidation.CheckBlobId(id);

         await _client.DeleteObjectAsync(_bucketName, id);
      }

      /// <summary>
      /// Checks if the object exists by trying to fetch the details
      /// </summary>
      public override async Task<bool> ExistsAsync(string id)
      {
         GenericValidation.CheckBlobId(id);

         try
         {
            using (await GetObjectAsync(id))
            {

            }
         }
         catch (StorageException ex)
         {
            if (ex.ErrorCode == ErrorCode.NotFound) return false;
         }

         return true;
      }

      /// <summary>
      /// Gets blob metadata
      /// </summary>
      public override async Task<BlobMeta> GetMetaAsync(string id)
      {
         GenericValidation.CheckBlobId(id);

         try
         {
            using (GetObjectResponse obj = await GetObjectAsync(id))
            {
               //ETag contains actual MD5 hash, not sure why!

               return new BlobMeta(
                  obj.ContentLength,
                  obj.ETag.Trim('\"'));  
            }
         }
         catch (StorageException ex) when (ex.ErrorCode == ErrorCode.NotFound)
         {
            return null;
         }
      }

      /// <summary>
      /// Lists all buckets, optionaly filtering by prefix. Prefix filtering happens on client side.
      /// </summary>
      public override async Task<IEnumerable<BlobItem>> ListAsync(string folderPath, string prefix, bool recurse)
      {
         GenericValidation.CheckBlobPrefix(prefix);

         ListObjectsV2Response response = await _client.ListObjectsV2Async(new ListObjectsV2Request()
         {
            BucketName = _bucketName,
            Prefix = prefix ?? null
         });

         return response.S3Objects.Select(s3Obj => new BlobItem(s3Obj.Key));
      }

      /// <summary>
      /// Opens AWS stream and returns it. Note that you absolutely have to dispose it yourself in order for this
      /// adapter to close the network connection properly.
      /// </summary>
      /// <param name="id"></param>
      /// <returns></returns>
      public override async Task<Stream> OpenReadAsync(string id)
      {
         GenericValidation.CheckBlobId(id);

         GetObjectResponse response = await GetObjectAsync(id);
         return new AwsS3BlobStorageExternalStream(response);
      }

      /// <summary>
      /// Uploads the stream using transfer manager
      /// </summary>
      /// <param name="id">Unique resource ID</param>
      /// <param name="sourceStream">Stream to upload</param>
      public override async Task WriteAsync(string id, Stream sourceStream)
      {
         GenericValidation.CheckBlobId(id);
         GenericValidation.CheckSourceStream(sourceStream);

         //http://docs.aws.amazon.com/AmazonS3/latest/dev/HLuploadFileDotNet.html

         await _fileTransferUtility.UploadAsync(sourceStream, _bucketName, id);
      }

      /// <summary>
      /// Simulates append operation in a very inefficient way as AWS doesn't support appending to blobs. Avoid at all costs!
      /// </summary>
      /// <param name="id"></param>
      /// <param name="sourceStream"></param>
      public override void Append(string id, Stream sourceStream)
      {
         GenericValidation.CheckBlobId(id);
         GenericValidation.CheckSourceStream(sourceStream);

         throw new NotSupportedException();
      }

      private async Task<GetObjectResponse> GetObjectAsync(string key)
      {
         var request = new GetObjectRequest { BucketName = _bucketName, Key = key };

         try
         {
            GetObjectResponse response = await _client.GetObjectAsync(request);
            return response;
         }
         catch (AmazonS3Exception ex)
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
