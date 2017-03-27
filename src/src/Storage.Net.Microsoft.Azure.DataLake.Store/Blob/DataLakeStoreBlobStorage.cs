using Microsoft.Rest.Azure.Authentication;
using Storage.Net.Blob;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using System.Net;
using Microsoft.Azure.Management.DataLake.Store;
using System.Collections.Generic;

namespace Storage.Net.Microsoft.Azure.DataLake.Store.Blob
{
   class DataLakeStoreBlobStorage : AsyncBlobStorage
   {
      private readonly string _subscriptionId;
      private readonly string _accountName;
      private readonly string _domain;
      private readonly string _clientId;
      private readonly string _clientSecret;
      private ServiceClientCredentials _credential;
      private DataLakeStoreFileSystemManagementClient _fsClient;

      //some info on how to use sdk here: https://docs.microsoft.com/en-us/azure/data-lake-store/data-lake-store-get-started-net-sdk

      private DataLakeStoreBlobStorage(string subscriptionId, string accountName, string domain, string clientId, string clientSecret, string clientCert)
      {
         _subscriptionId = subscriptionId;
         _accountName = accountName;
         _domain = domain;
         _clientId = clientId;
         _clientSecret = clientSecret;
      }

      public static DataLakeStoreBlobStorage CreateByClientSecret(string subscriptionId, string accountName, NetworkCredential credential)
      {
         return new DataLakeStoreBlobStorage(subscriptionId, accountName, credential.Domain, credential.UserName, credential.Password, null);
      }

      public override Task AppendFromStreamAsync(string id, Stream chunkStream)
      {
         GenericValidation.CheckBlobId(id);
         if (chunkStream == null) throw new ArgumentNullException(nameof(chunkStream));

         throw new NotImplementedException();
      }

      public override Task DeleteAsync(string id)
      {
         GenericValidation.CheckBlobId(id);

         throw new NotImplementedException();
      }

      public override async Task DownloadToStreamAsync(string id, Stream targetStream)
      {
         GenericValidation.CheckBlobId(id);
         if (targetStream == null) throw new ArgumentNullException(nameof(targetStream));

         var client = await GetFsClient();

         using (Stream s = await client.FileSystem.OpenAsync(_accountName, id))
         {
            await s.CopyToAsync(targetStream);
         }
      }

      public override Task<bool> ExistsAsync(string id)
      {
         GenericValidation.CheckBlobId(id);

         throw new NotImplementedException();
      }

      public override Task<BlobMeta> GetMetaAsync(string id)
      {
         GenericValidation.CheckBlobId(id);

         throw new NotImplementedException();
      }

      public override Task<IEnumerable<string>> ListAsync(string prefix)
      {
         GenericValidation.CheckBlobPrefix(prefix);

         throw new NotImplementedException();
      }

      public override Task<Stream> OpenStreamToReadAsync(string id)
      {
         GenericValidation.CheckBlobId(id);

         throw new NotImplementedException();
      }

      public override async Task UploadFromStreamAsync(string id, Stream sourceStream)
      {
         GenericValidation.CheckBlobId(id);
         if (sourceStream == null) throw new ArgumentNullException(nameof(sourceStream));

         var client = await GetFsClient();

         await client.FileSystem.CreateAsync(_accountName, id, sourceStream, true);
      }

      private async Task<DataLakeStoreFileSystemManagementClient> GetFsClient()
      {
         if (_fsClient != null) return _fsClient;

         ServiceClientCredentials creds = await GetCreds();

         var fsClient = new DataLakeStoreFileSystemManagementClient(creds);

         return fsClient;
      }

      private async Task<ServiceClientCredentials> GetCreds()
      {
         if (_credential != null) return _credential;

         if (_clientSecret != null)
         {
            var cc = new ClientCredential(_clientId, _clientSecret);
            _credential = await ApplicationTokenProvider.LoginSilentAsync(_domain, cc);
         }
         else
         {
            //var ac = new ClientAssertionCertificate(_clientSecret, )
            //await ApplicationTokenProvider.LoginSilentWithCertificateAsync(_domain, )
            throw new NotImplementedException();
         }

         return _credential;
      }
   }
}
