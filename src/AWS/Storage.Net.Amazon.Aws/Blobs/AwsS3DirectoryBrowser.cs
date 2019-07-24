using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Storage.Net.Blobs;

namespace Storage.Net.Amazon.Aws.Blobs
{
   class AwsS3DirectoryBrowser
   {
      private readonly AmazonS3Client _client;
      private readonly string _bucketName;

      public AwsS3DirectoryBrowser(AmazonS3Client client, string bucketName)
      {
         _client = client;
         _bucketName = bucketName;
      }

      public async Task<IReadOnlyCollection<Blob>> ListAsync(ListOptions options, CancellationToken cancellationToken)
      {
         var result = new List<Blob>();

         var request = new ListObjectsV2Request()
         {
            BucketName = _bucketName,
            Prefix = options.FilePrefix ?? null
         };

         while(options.MaxResults == null || (result.Count < options.MaxResults))
         {
            ListObjectsV2Response response = await _client.ListObjectsV2Async(request, cancellationToken).ConfigureAwait(false);

            List<Blob> blobs = response.S3Objects
               .Select(ToBlob)
               .Where(options.IsMatch)
               .Where(b => (options.FolderPath == null || StoragePath.ComparePath(b.FolderPath, options.FolderPath)))
               .Where(b => options.BrowseFilter == null || options.BrowseFilter(b))
               .ToList();

            result.AddRange(blobs);

            if(response.NextContinuationToken == null)
               break;

            request.ContinuationToken = response.NextContinuationToken;
         }

         if(options.MaxResults != null && result.Count > options.MaxResults)
         {
            result = result.Take((int)options.MaxResults).ToList();
         }

         return result;
      }

      private static Blob ToBlob(S3Object s3Obj)
      {
         if(s3Obj.Key.EndsWith("/"))
            return new Blob(s3Obj.Key, BlobItemKind.Folder);

         return new Blob(StoragePath.RootFolderPath, s3Obj.Key, BlobItemKind.File) { Size = s3Obj.Size };
      }

      public async Task DeleteRecursiveAsync(string fullPath, CancellationToken cancellationToken)
      {
         var request = new ListObjectsV2Request()
         {
            BucketName = _bucketName,
            Prefix = fullPath + "/"
         };

         while(true)
         {
            ListObjectsV2Response response = await _client.ListObjectsV2Async(request, cancellationToken).ConfigureAwait(false);

            await Task.WhenAll(response.S3Objects.Select(s3 => _client.DeleteObjectAsync(_bucketName, s3.Key, cancellationToken)));

            if(response.NextContinuationToken == null)
               break;

            request.ContinuationToken = response.NextContinuationToken;
         }
      }
   }
}