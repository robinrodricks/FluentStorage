using Microsoft.Rest.Azure.Authentication;
using Storage.Net.Blob;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using System.Net;
using Microsoft.Azure.Management.DataLake.Store;

namespace Storage.Net.Microsoft.Azure.DataLake.Store.Blob
{
   class DataLakeStoreBlobStorage : AsyncBlobStorage
   {
      private readonly string _subscriptionId;
      private readonly string _domain;
      private readonly string _clientId;
      private readonly string _clientSecret;
      private ServiceClientCredentials _credential;

      //some info on how to use sdk here: https://docs.microsoft.com/en-us/azure/data-lake-store/data-lake-store-get-started-net-sdk

      private DataLakeStoreBlobStorage(string subscriptionId, string domain, string clientId, string clientSecret, string clientCert)
      {
         _subscriptionId = subscriptionId;
         _domain = domain;
         _clientId = clientId;
         _clientSecret = clientSecret;
      }

      public static DataLakeStoreBlobStorage CreateByClientSecret(string subscriptionId, NetworkCredential credential)
      {
         return new DataLakeStoreBlobStorage(subscriptionId, credential.Domain, credential.UserName, credential.Password, null);
      }

      public override async Task UploadFromStreamAsync(string id, Stream sourceStream)
      {
         ServiceClientCredentials creds = await GetCreds();

         new DataLakeStoreFileSystemManagementClient(creds);
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
