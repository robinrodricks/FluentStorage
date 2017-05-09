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
using Microsoft.Azure.Management.DataLake.Store.Models;
using NetBox.IO;
using Microsoft.Rest.Azure;
using System.Linq;
using System.Collections.Generics;

namespace Storage.Net.Microsoft.Azure.DataLake.Store.Blob
{
   public class DataLakeStoreBlobStorage : AsyncBlobStorage
   {
      private readonly string _accountName;
      private readonly string _domain;
      private readonly string _clientId;
      private readonly string _clientSecret;
      private ServiceClientCredentials _credential;
      private DataLakeStoreFileSystemManagementClient _fsClient;

      //some info on how to use sdk here: https://docs.microsoft.com/en-us/azure/data-lake-store/data-lake-store-get-started-net-sdk

      private DataLakeStoreBlobStorage(string accountName, string domain, string clientId, string clientSecret, string clientCert)
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

      /// <summary>
      /// Returns the actual file system management object. Note that this will only be populated once you make at least one
      /// successful call.
      /// </summary>
      public DataLakeStoreFileSystemManagementClient FsClient => _fsClient;


      public static DataLakeStoreBlobStorage CreateByClientSecret(string accountName, NetworkCredential credential)
      {
         if (credential == null) throw new ArgumentNullException(nameof(credential));

         if (string.IsNullOrEmpty(credential.Domain))
            throw new ArgumentException("Tenant ID (Domain in NetworkCredential) part is required");

         if (string.IsNullOrEmpty(credential.UserName))
            throw new ArgumentException("Principal ID (Username in NetworkCredential) part is required");

         if (string.IsNullOrEmpty(credential.Password))
            throw new ArgumentException("Principal Secret (Password in NetworkCredential) part is required");

         return new DataLakeStoreBlobStorage(accountName, credential.Domain, credential.UserName, credential.Password, null);
      }

      public override async Task AppendFromStreamAsync(string id, Stream chunkStream)
      {
         GenericValidation.CheckBlobId(id);
         if (chunkStream == null) throw new ArgumentNullException(nameof(chunkStream));

         var client = await GetFsClient();

         if (await ExistsAsync(id))
         {
            await client.FileSystem.AppendAsync(_accountName, id, new NonCloseableStream(chunkStream));
         }
         else
         {
            await UploadFromStreamAsync(id, chunkStream);
         }
      }

      public override async Task DeleteAsync(string id)
      {
         GenericValidation.CheckBlobId(id);

         var client = await GetFsClient();

         await client.FileSystem.DeleteAsync(_accountName, id);
      }

      public override async Task<Stream> OpenStreamToReadAsync(string id)
      {
         GenericValidation.CheckBlobId(id);

         var client = await GetFsClient();

         try
         {
            return await client.FileSystem.OpenAsync(_accountName, id);
         }
         catch (CloudException ex) when (ex.Response.StatusCode == HttpStatusCode.NotFound)
         {
            throw new StorageException(ErrorCode.NotFound, ex);
         }
      }

      public override async Task<bool> ExistsAsync(string id)
      {
         GenericValidation.CheckBlobId(id);

         var client = await GetFsClient();

         try
         {
            await client.FileSystem.GetFileStatusAsync(_accountName, id);

            return true;
         }
         catch(AdlsErrorException ex) when (ex.Response.StatusCode == HttpStatusCode.NotFound)
         {
            return false;
         }
      }

      public override async Task<BlobMeta> GetMetaAsync(string id)
      {
         GenericValidation.CheckBlobId(id);

         var client = await GetFsClient();

         //ContentSummaryResult csr = await client.FileSystem.GetContentSummaryAsync(_accountName, id);
         //csr.ContentSummary.
         FileStatusResult fsr = await client.FileSystem.GetFileStatusAsync(_accountName, id);
         //fsr.FileStatus.

         return new BlobMeta(fsr.FileStatus.Length.Value, null);
      }

      public override async Task<IEnumerable<string>> ListAsync(string prefix)
      {
         GenericValidation.CheckBlobPrefix(prefix);

         var client = await GetFsClient();

         var files = new List<string>();

         await ListByPrefixIntoContainer(client, prefix ?? "/", files);

         return files.Select(f => f.TrimStart('/'));
      }

      private async Task ListByPrefixIntoContainer(DataLakeStoreFileSystemManagementClient client, string prefix, List<string> files)
      {
         if (prefix == null)
         {
            prefix = "/";
         }
         else if(!prefix.StartsWith("/"))
         {
            prefix = "/" + prefix;
         }

         try
         {
            FileStatusesResult statuses = await client.FileSystem.ListFileStatusAsync(_accountName, prefix);

            files.AddRange(statuses.FileStatuses.FileStatus
               .Where(fs => fs.Type == FileType.FILE)
               .Select(f => prefix + f.PathSuffix));

            List<string> directories = statuses.FileStatuses.FileStatus
               .Where(fs => fs.Type == FileType.DIRECTORY)
               .Select(fs => prefix + fs.PathSuffix + "/")
               .ToList();

            if (directories.Count > 0)
            {
               foreach (string dirPath in directories)
               {
                  await ListByPrefixIntoContainer(client, dirPath, files);
               }
            }
         }
         catch(AdlsErrorException ex) when (ex.Response.StatusCode == HttpStatusCode.NotFound)
         {

         }
      }

      public override async Task UploadFromStreamAsync(string id, Stream sourceStream)
      {
         GenericValidation.CheckBlobId(id);
         if (sourceStream == null) throw new ArgumentNullException(nameof(sourceStream));

         var client = await GetFsClient();

         await client.FileSystem.CreateAsync(_accountName, id, new NonCloseableStream(sourceStream), true);
      }

      private async Task<DataLakeStoreFileSystemManagementClient> GetFsClient()
      {
         if (_fsClient != null) return _fsClient;

         ServiceClientCredentials creds = await GetCreds();

         _fsClient = new DataLakeStoreFileSystemManagementClient(creds);

         return _fsClient;
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
