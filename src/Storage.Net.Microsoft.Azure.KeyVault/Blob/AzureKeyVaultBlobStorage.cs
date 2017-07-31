using Microsoft.Azure.KeyVault;
using Storage.Net.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Storage.Net.Microsoft.Azure.KeyVault.Blob
{
   class AzureKeyVaultBlobStorage : IBlobStorage
   {
      private KeyVaultClient _vaultClient;
      private ClientCredential _credential;
      private readonly string _vaultUri;

      public AzureKeyVaultBlobStorage(Uri vaultUri, string azureAadClientId, string azureAadClientSecret)
      {
         _credential = new ClientCredential(azureAadClientId, azureAadClientSecret);

         _vaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(GetAccessToken), GetHttpClient());

         _vaultUri = vaultUri.ToString();
      }

      #region [ IBlobStorage ]

      public void Append(string id, Stream sourceStream)
      {
         throw new NotImplementedException();
      }

      public Task AppendAsync(string id, Stream sourceStream)
      {
         throw new NotImplementedException();
      }

      public void Delete(string id)
      {
         throw new NotImplementedException();
      }

      public Task DeleteAsync(string id)
      {
         throw new NotImplementedException();
      }

      public void Dispose()
      {
         throw new NotImplementedException();
      }

      public bool Exists(string id)
      {
         throw new NotImplementedException();
      }

      public Task<bool> ExistsAsync(string id)
      {
         throw new NotImplementedException();
      }

      public BlobMeta GetMeta(string id)
      {
         throw new NotImplementedException();
      }

      public Task<BlobMeta> GetMetaAsync(string id)
      {
         throw new NotImplementedException();
      }

      public IEnumerable<string> List(string prefix)
      {
         throw new NotImplementedException();
      }

      public Task<IEnumerable<string>> ListAsync(string prefix)
      {
         throw new NotImplementedException();
      }

      public Stream OpenRead(string id)
      {
         throw new NotImplementedException();
      }

      public Task<Stream> OpenReadAsync(string id)
      {
         throw new NotImplementedException();
      }

      public void Write(string id, Stream sourceStream)
      {
         throw new NotImplementedException();
      }

      public Task WriteAsync(string id, Stream sourceStream)
      {
         throw new NotImplementedException();
      }

      #endregion

      /// <summary>
      /// Gets the access token
      /// </summary>
      /// <param name="authority"> Authority </param>
      /// <param name="resource"> Resource </param>
      /// <param name="scope"> scope </param>
      /// <returns> token </returns>
      public async Task<string> GetAccessToken(string authority, string resource, string scope)
      {
         var context = new AuthenticationContext(authority, TokenCache.DefaultShared);

         var result = await context.AcquireTokenAsync(resource, _credential);

         return result.AccessToken;
      }

      /// <summary>
      /// Create an HttpClient object that optionally includes logic to override the HOST header
      /// field for advanced testing purposes.
      /// </summary>
      /// <returns>HttpClient instance to use for Key Vault service communication</returns>
      private HttpClient GetHttpClient()
      {
         return new HttpClient();
         //return (HttpClientFactory.Create(new InjectHostHeaderHttpMessageHandler()));
      }
   }
}
