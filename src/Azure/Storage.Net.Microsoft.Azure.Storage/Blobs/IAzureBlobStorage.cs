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
      Task<string> GetStorageSasAsync(SasPolicy accountPolicy);

      /// <summary>
      /// Returns Uri to Azure Blob with Shared Access Token.
      /// </summary>
      Task<string> GetSasUriAsync(string fullPath,
         SharedAccessBlobPolicy sasConstraints,
         SharedAccessBlobHeaders headers = null,
         bool createContainer = false,
         CancellationToken cancellationToken = default);

      /// <summary>
      /// Returns Uri to Azure Blob with read-only Shared Access Token.
      /// </summary>
      Task<string> GetReadOnlySasUriAsync(
         string fullPath,
         SharedAccessBlobHeaders headers = null,
         int minutesToExpiration = 30,
         CancellationToken cancellationToken = default);

      /// <summary>
      /// Returns Uri to Azure Blob with write-only Shared Access Token.
      /// </summary>
      Task<string> GetWriteOnlySasUriAsync(
         string fullPath,
         SharedAccessBlobHeaders headers = null,
         int minutesToExpiration = 30,
         CancellationToken cancellationToken = default);

      /// <summary>
      /// Returns Uri to Azure Blob with read-write Shared Access Token.
      /// </summary>
      Task<string> GetReadWriteSasUriAsync(
         string fullPath,
         SharedAccessBlobHeaders headers = null,
         int minutesToExpiration = 30,
         CancellationToken cancellationToken = default);

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
