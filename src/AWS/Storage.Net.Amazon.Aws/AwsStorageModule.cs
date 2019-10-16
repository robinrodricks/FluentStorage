using System;
using System.Collections.Generic;
using System.Text;
using Amazon;
using Storage.Net.Amazon.Aws.Blobs;
using Storage.Net.Blobs;
using Storage.Net.ConnectionString;
using Storage.Net.KeyValue;
using Storage.Net.Messaging;

namespace Storage.Net.Amazon.Aws
{
   class AwsStorageModule : IExternalModule, IConnectionFactory
   {
      public IConnectionFactory ConnectionFactory => this;

      public IBlobStorage CreateBlobStorage(StorageConnectionString connectionString)
      {
         if(connectionString.Prefix == "aws.s3")
         {
            string keyId = connectionString.Get("keyId");
            string key = connectionString.Get("key");

            if(string.IsNullOrEmpty(keyId) != string.IsNullOrEmpty(key))
            {
               throw new ArgumentException($"connection string requires both 'key' and 'keyId' parameters, or neither.");
            }

            connectionString.GetRequired("bucket", true, out string bucket);
            connectionString.GetRequired("region", true, out string region);

            if(string.IsNullOrEmpty(keyId))
            {
               return new AwsS3BlobStorage(bucket, region);
            }

            return new AwsS3BlobStorage(keyId, key, bucket, region, null);
         }


         return null;
      }

      public IKeyValueStorage CreateKeyValueStorage(StorageConnectionString connectionString) => null;

      public IMessenger CreateMessenger(StorageConnectionString connectionString) => null;
   }
}
