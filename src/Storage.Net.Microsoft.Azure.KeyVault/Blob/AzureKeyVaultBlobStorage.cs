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
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Rest.Azure;
using NetBox.Model;
using System.Threading;

namespace Storage.Net.Microsoft.Azure.KeyVault.Blob
{
   class AzureKeyVaultBlobStorage : AsyncBlobStorage
   {
      private KeyVaultClient _vaultClient;
      private ClientCredential _credential;
      private readonly string _vaultUri;

      public AzureKeyVaultBlobStorage(Uri vaultUri, string azureAadClientId, string azureAadClientSecret)
      {
         _credential = new ClientCredential(azureAadClientId, azureAadClientSecret);

         _vaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(GetAccessToken), GetHttpClient());

         _vaultUri = vaultUri.ToString().Trim('/');
      }

      #region [ IBlobStorage ]

      protected override async Task<IEnumerable<BlobId>> ListAsync(string[] folderPath, string prefix, bool recurse, CancellationToken cancellationToken)
      {
         GenericValidation.CheckBlobPrefix(prefix);

         var secretNames = new List<BlobId>();
         IPage<SecretItem> page = await _vaultClient.GetSecretsAsync(_vaultUri);

         do
         {
            foreach(SecretItem item in page)
            {
               secretNames.Add(new BlobId(null, item.Id, BlobItemKind.File));
            }
         }
         while (page.NextPageLink != null && (page = await _vaultClient.GetSecretsNextAsync(page.NextPageLink)) != null);

         if (prefix == null) return secretNames;

         return secretNames.Where(n => n.Id.StartsWith(prefix));
      }

      public override async Task WriteAsync(string id, Stream sourceStream)
      {
         GenericValidation.CheckBlobId(id);
         GenericValidation.CheckSourceStream(sourceStream);

         string value = Encoding.UTF8.GetString(sourceStream.ToByteArray());

         await _vaultClient.SetSecretAsync(_vaultUri, id, value);
      }

      public override async Task<Stream> OpenReadAsync(string id)
      {
         GenericValidation.CheckBlobId(id);

         SecretBundle secret;
         try
         {
            secret = await _vaultClient.GetSecretAsync(_vaultUri, id);
         }
         catch(KeyVaultErrorException ex)
         {
            TryHandleException(ex);
            throw;
         }

         string value = secret.Value;

         return value.ToMemoryStream();
      }

      public override async Task DeleteAsync(string id)
      {
         GenericValidation.CheckBlobId(id);

         await _vaultClient.DeleteSecretAsync(_vaultUri, id);
      }

      public override async Task AppendAsync(string id, Stream sourceStream)
      {
         GenericValidation.CheckBlobId(id);

         SecretBundle secret = await _vaultClient.GetSecretAsync(id);
         string value = secret.Value;
         value += Encoding.UTF8.GetString(sourceStream.ToByteArray());

         await _vaultClient.SetSecretAsync(_vaultUri, id, value);
      }

      public override async Task<bool> ExistsAsync(string id)
      {
         GenericValidation.CheckBlobId(id);

         SecretBundle secret;

         try
         {
            secret = await _vaultClient.GetSecretAsync(_vaultUri, id);
         }
         catch(KeyVaultErrorException)
         {
            secret = null;
         }

         return secret != null;
      }

      public override async Task<BlobMeta> GetMetaAsync(string id)
      {
         GenericValidation.CheckBlobId(id);

         SecretBundle secret = await _vaultClient.GetSecretAsync(id);
         byte[] data = Encoding.UTF8.GetBytes(secret.Value);

         return new BlobMeta(data.Length, secret.Value.GetHash(HashType.Md5));
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

      private static bool TryHandleException(KeyVaultErrorException ex)
      {
         if(ex.Body.Error.Code == "SecretNotFound")
         {
            throw new StorageException(ErrorCode.NotFound, ex);
         }

         return false;
      }

   }
}
