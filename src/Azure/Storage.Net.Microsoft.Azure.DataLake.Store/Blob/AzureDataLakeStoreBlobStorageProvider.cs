using Microsoft.Rest.Azure.Authentication;
using Storage.Net.Blobs;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Azure.DataLake.Store;

namespace Storage.Net.Microsoft.Azure.DataLake.Store.Blobs
{
   class AzureDataLakeStoreBlobStorageProvider : IBlobStorage
   {
      private readonly string _accountName;
      private readonly string _domain;
      private readonly string _clientId;
      private readonly string _clientSecret;
      private ServiceClientCredentials _credential;
      private AdlsClient _client;

      //some info on how to use sdk here: https://docs.microsoft.com/en-us/azure/data-lake-store/data-lake-store-get-started-net-sdk

      private AzureDataLakeStoreBlobStorageProvider(string accountName, string domain, string clientId, string clientSecret, string clientCert)
      {
         _accountName = accountName ?? throw new ArgumentNullException(nameof(accountName));

         _domain = domain ?? throw new ArgumentNullException(nameof(domain));
         _clientId = clientId;
         _clientSecret = clientSecret;
      }

      /// <summary>
      /// Returns the actual credential object used to authenticate to ADLS. Note that this will only be populated
      /// once you make at least one successful call.
      /// </summary>
      public ServiceClientCredentials Credentials => _credential;

      public int ListBatchSize { get; set; } = 5000;

      public static AzureDataLakeStoreBlobStorageProvider CreateByClientSecret(string accountName, string tenantId, string principalId, string principalSecret)
      {
         return CreateByClientSecret(accountName, new NetworkCredential(principalId, principalSecret, tenantId));
      }

      public static AzureDataLakeStoreBlobStorageProvider CreateByClientSecret(string accountName, NetworkCredential credential)
      {
         if (credential == null) throw new ArgumentNullException(nameof(credential));

         if (string.IsNullOrEmpty(credential.Domain))
            throw new ArgumentException("Tenant ID (Domain in NetworkCredential) part is required");

         if (string.IsNullOrEmpty(credential.UserName))
            throw new ArgumentException("Principal ID (Username in NetworkCredential) part is required");

         if (string.IsNullOrEmpty(credential.Password))
            throw new ArgumentException("Principal Secret (Password in NetworkCredential) part is required");

         return new AzureDataLakeStoreBlobStorageProvider(accountName, credential.Domain, credential.UserName, credential.Password, null);
      }

      public async Task<IReadOnlyCollection<Blob>> ListAsync(ListOptions options, CancellationToken cancellationToken)
      {
         if (options == null) options = new ListOptions();

         AdlsClient client = await GetAdlsClientAsync();

         var browser = new DirectoryBrowser(client, ListBatchSize);
         return await browser.BrowseAsync(options, cancellationToken);
      }

      public async Task WriteAsync(string fullPath, Stream sourceStream, bool append, CancellationToken cancellationToken)
      {
         GenericValidation.CheckBlobFullPath(fullPath);

         AdlsClient client = await GetAdlsClientAsync();

         if (append && (await ExistsAsync(new[] { fullPath }, cancellationToken)).First())
         {
            AdlsOutputStream adlsStream = await client.GetAppendStreamAsync(fullPath, cancellationToken);
            using (var writeStream = new AdlsWriteableStream(adlsStream))
            {
               await sourceStream.CopyToAsync(writeStream);
            }
         }
         else
         {
            AdlsOutputStream adlsStream = await client.CreateFileAsync(fullPath, IfExists.Overwrite,
               createParent:true,
               cancelToken: cancellationToken);

            using (var writeStream = new AdlsWriteableStream(adlsStream))
            {
               await sourceStream.CopyToAsync(writeStream);
            }
         }
      }

      public async Task<Stream> OpenWriteAsync(string fullPath, bool append, CancellationToken cancellationToken)
      {
         GenericValidation.CheckBlobFullPath(fullPath);

         AdlsClient client = await GetAdlsClientAsync();

         AdlsOutputStream stream;

         if(append)
         {
            stream = await client.GetAppendStreamAsync(fullPath, cancellationToken);
         }
         else
         {
            stream = await client.CreateFileAsync(fullPath, IfExists.Overwrite,
               createParent: true,
               cancelToken: cancellationToken);
         }

         return new AdlsWriteableStream(stream);
      }

      public async Task<Stream> OpenReadAsync(string id, CancellationToken cancellationToken)
      {
         GenericValidation.CheckBlobFullPath(id);

         AdlsClient client = await GetAdlsClientAsync();

         try
         {
            AdlsInputStream response = await client.GetReadStreamAsync(id, cancellationToken);

            return response;
         }
         catch (AdlsException ex) when (ex.HttpStatus == HttpStatusCode.NotFound)
         {
            return null;
         }
      }

      public async Task DeleteAsync(IEnumerable<string> ids, CancellationToken cancellationToken)
      {
         GenericValidation.CheckBlobFullPaths(ids);

         AdlsClient client = await GetAdlsClientAsync();

         await Task.WhenAll(ids.Select(id => client.DeleteAsync(id, cancellationToken)));
      }

      public async Task<IReadOnlyCollection<bool>> ExistsAsync(IEnumerable<string> ids, CancellationToken cancellationToken)
      {
         GenericValidation.CheckBlobFullPaths(ids);

         AdlsClient client = await GetAdlsClientAsync();

         var result = new List<bool>();

         foreach (string id in ids)
         {
            bool exists = client.CheckExists(id);

            result.Add(exists);
         }

         return result;
      }

      public async Task<IReadOnlyCollection<Blob>> GetBlobsAsync(IEnumerable<string> ids, CancellationToken cancellationToken)
      {
         GenericValidation.CheckBlobFullPaths(ids);

         AdlsClient client = await GetAdlsClientAsync();

         return await Task.WhenAll(ids.Select(id => GetBlobWithMetaAsync(id, client, cancellationToken)));
      }

      public Task SetBlobsAsync(IEnumerable<Blob> blobs, CancellationToken cancellationToken = default)
      {
         throw new NotSupportedException("ADLS Gen1 doesn't support file metadata");
      }

      private async Task<Blob> GetBlobWithMetaAsync(string id, AdlsClient client, CancellationToken cancellationToken)
      {
         try
         {
            DirectoryEntry entry = await client.GetDirectoryEntryAsync(id, cancelToken: cancellationToken);
            return new Blob(id)
            {
               Size = entry.Length,
               LastModificationTime = entry.LastModifiedTime
            };
         }
         catch(AdlsException ex) when (ex.HttpStatus == HttpStatusCode.NotFound)
         {
            return null;
         }
      }

      private async Task<AdlsClient> GetAdlsClientAsync()
      {
         if (_client != null) return _client;

         ServiceClientCredentials creds = await GetCredsAsync();

         _client = AdlsClient.CreateClient($"{_accountName}.azuredatalakestore.net", creds);

         return _client;
      }

      private async Task<ServiceClientCredentials> GetCredsAsync()
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

      public void Dispose()
      {
      }

      public Task<ITransaction> OpenTransactionAsync()
      {
         return Task.FromResult(EmptyTransaction.Instance);
      }
   }
}
