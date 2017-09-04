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
using System.Threading;

namespace Storage.Net.Microsoft.Azure.DataLake.Store.Blob
{
   public class DataLakeStoreBlobStorageProvider : IBlobStorageProvider
   {
      private readonly string _accountName;
      private readonly string _domain;
      private readonly string _clientId;
      private readonly string _clientSecret;
      private ServiceClientCredentials _credential;
      private DataLakeStoreFileSystemManagementClient _fsClient;

      //some info on how to use sdk here: https://docs.microsoft.com/en-us/azure/data-lake-store/data-lake-store-get-started-net-sdk

      private DataLakeStoreBlobStorageProvider(string accountName, string domain, string clientId, string clientSecret, string clientCert)
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


      public static DataLakeStoreBlobStorageProvider CreateByClientSecret(string accountName, NetworkCredential credential)
      {
         if (credential == null) throw new ArgumentNullException(nameof(credential));

         if (string.IsNullOrEmpty(credential.Domain))
            throw new ArgumentException("Tenant ID (Domain in NetworkCredential) part is required");

         if (string.IsNullOrEmpty(credential.UserName))
            throw new ArgumentException("Principal ID (Username in NetworkCredential) part is required");

         if (string.IsNullOrEmpty(credential.Password))
            throw new ArgumentException("Principal Secret (Password in NetworkCredential) part is required");

         return new DataLakeStoreBlobStorageProvider(accountName, credential.Domain, credential.UserName, credential.Password, null);
      }

      public async Task<IEnumerable<BlobId>> ListAsync(ListOptions options, CancellationToken cancellationToken)
      {
         if (options == null) options = new ListOptions();

         GenericValidation.CheckBlobPrefix(options.Prefix);

         var client = await GetFsClient();

         var blobs = new List<BlobId>();

         await ListByPrefixIntoContainer(client, options.Prefix ?? "/", blobs, options.Recurse);

         return blobs;
      }

      private async Task ListByPrefixIntoContainer(DataLakeStoreFileSystemManagementClient client, string prefix, List<BlobId> files, bool recurse)
      {
         //todo: not sure whether this should list just top level files or not, but then what to do with folders?

         throw new NotImplementedException();

         /*if (prefix == null)
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

         }*/
      }

      public async Task WriteAsync(string id, Stream sourceStream, bool append)
      {
         GenericValidation.CheckBlobId(id);

         var client = await GetFsClient();

         if (append)
         {
            if ((await ExistsAsync(new[] { id })).First())
            {
               await client.FileSystem.AppendAsync(_accountName, id, new NonCloseableStream(sourceStream));
            }
            else
            {
               await client.FileSystem.CreateAsync(_accountName, id, new NonCloseableStream(sourceStream), true);
            }
         }
         else
         {
            await client.FileSystem.CreateAsync(_accountName, id, new NonCloseableStream(sourceStream), true);
         }
      }

      public async Task<Stream> OpenReadAsync(string id)
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

      public async Task DeleteAsync(IEnumerable<string> ids)
      {
         GenericValidation.CheckBlobId(ids);

         var client = await GetFsClient();

         await Task.WhenAll(ids.Select(id => client.FileSystem.DeleteAsync(_accountName, id)));
      }

      public async Task<IEnumerable<bool>> ExistsAsync(IEnumerable<string> ids)
      {
         GenericValidation.CheckBlobId(ids);

         var client = await GetFsClient();

         var result = new List<bool>();

         foreach (string id in ids)
         {
            try
            {
               await client.FileSystem.GetFileStatusAsync(_accountName, id);

               result.Add(true);
            }
            catch (AdlsErrorException ex) when (ex.Response.StatusCode == HttpStatusCode.NotFound)
            {
               result.Add(false);
            }
         }

         return result;
      }

      public async Task<IEnumerable<BlobMeta>> GetMetaAsync(IEnumerable<string> ids)
      {
         GenericValidation.CheckBlobId(ids);

         DataLakeStoreFileSystemManagementClient client = await GetFsClient();

         return await Task.WhenAll(ids.Select(id => GetMetaAsync(id, client)));
      }

      private async Task<BlobMeta> GetMetaAsync(string id, DataLakeStoreFileSystemManagementClient client)
      {
         FileStatusResult fsr = await client.FileSystem.GetFileStatusAsync(_accountName, id);

         var meta = new BlobMeta(fsr.FileStatus.Length.Value, null);

         return meta;
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

      public void Dispose()
      {
      }
   }
}
