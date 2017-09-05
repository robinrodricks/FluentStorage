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
using System.Threading;

namespace Storage.Net.Aws.Blob
{
   /// <summary>
   /// Amazon S3 storage adapter for blobs
   /// </summary>
   public class AwsS3BlobStorageProvider : IBlobStorageProvider
   {
      private readonly string _bucketName;
      private readonly AmazonS3Client _client;
      private readonly TransferUtility _fileTransferUtility;

      //https://github.com/awslabs/aws-sdk-net-samples/blob/master/ConsoleSamples/AmazonS3Sample/AmazonS3Sample/S3Sample.cs

      /// <summary>
      /// Creates a new instance of <see cref="AwsS3BlobStorageProvider"/>
      /// </summary>
      /// <param name="accessKeyId"></param>
      /// <param name="secretAccessKey"></param>
      /// <param name="bucketName"></param>
      public AwsS3BlobStorageProvider(string accessKeyId, string secretAccessKey, string bucketName)
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
      /// Lists all buckets, optionaly filtering by prefix. Prefix filtering happens on client side.
      /// </summary>
      public async Task<IEnumerable<BlobId>> ListAsync(ListOptions options, CancellationToken cancellationToken)
      {
         if (options == null) options = new ListOptions();

         GenericValidation.CheckBlobPrefix(options.Prefix);

         ListObjectsV2Response response = await _client.ListObjectsV2Async(new ListObjectsV2Request()
         {
            BucketName = _bucketName,
            Prefix = options.Prefix ?? null,
         },
         cancellationToken);

         return response.S3Objects.Select(s3Obj => new BlobId(null, s3Obj.Key, BlobItemKind.File));
      }

      public async Task WriteAsync(string id, Stream sourceStream, bool append = false)
      {
         if (append) throw new NotSupportedException();

         GenericValidation.CheckBlobId(id);
         GenericValidation.CheckSourceStream(sourceStream);

         //http://docs.aws.amazon.com/AmazonS3/latest/dev/HLuploadFileDotNet.html

         await _fileTransferUtility.UploadAsync(sourceStream, _bucketName, id);
      }

      public async Task<Stream> OpenReadAsync(string id)
      {
         GenericValidation.CheckBlobId(id);

         GetObjectResponse response = await GetObjectAsync(id);
         return new AwsS3BlobStorageExternalStream(response);
      }

      public Task DeleteAsync(IEnumerable<string> ids)
      {
         return Task.WhenAll(ids.Select(id => DeleteAsync(id)));
      }

      private Task DeleteAsync(string id)
      {
         GenericValidation.CheckBlobId(id);

         return _client.DeleteObjectAsync(_bucketName, id);
      }

      public async Task<IEnumerable<bool>> ExistsAsync(IEnumerable<string> ids)
      {
         return await Task.WhenAll(ids.Select(ExistsAsync));
      }

      private async Task<bool> ExistsAsync(string id)
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

      public async Task<IEnumerable<BlobMeta>> GetMetaAsync(IEnumerable<string> ids)
      {
         return await Task.WhenAll(ids.Select(GetMetaAsync));
      }

      private async Task<BlobMeta> GetMetaAsync(string id)
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

      public void Dispose()
      {
         throw new NotImplementedException();
      }
   }
}
