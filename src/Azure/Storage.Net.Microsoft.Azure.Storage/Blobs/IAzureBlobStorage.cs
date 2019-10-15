using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Storage.Blob;
using Storage.Net.Blobs;

namespace Storage.Net.Microsoft.Azure.Storage.Blobs
{
   /// <summary>
   /// Provides access to native operations
   /// </summary>
   public interface IAzureBlobStorage : IBlobStorage
   {
      /// <summary>
      /// Returns reference to the native Azure SD blob client.
      /// </summary>
      CloudBlobClient NativeBlobClient { get; }

      /// <summary>
      /// Gets Shared Access Signature for the entire storage account
      /// </summary>
      /// <returns></returns>
      Task<string> GetStorageSasAsync(AccountSasPolicy accountPolicy, bool includeUrl);

      /// <summary>
      /// Gets Shared Access Signature for a blob container
      /// </summary>
      /// <returns></returns>
      Task<string> GetContainerSasAsync(string containerName, ContainerSasPolicy containerSasPolicy, bool includeUrl);

      /// <summary>
      /// Acquires a lease
      /// </summary>
      /// <param name="fullPath"></param>
      /// <param name="maxLeaseTime"></param>
      /// <param name="waitForRelease">When true, the call will wait for the lock to be released</param>
      /// <param name="cancellationToken"></param>
      /// <returns></returns>
      Task<BlobLease> AcquireBlobLeaseAsync(string fullPath, TimeSpan maxLeaseTime, bool waitForRelease = false, CancellationToken cancellationToken = default);
   }
}
