using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Storage.Net.Blobs;
using Storage.Net.Microsoft.Azure.Storage.Blobs.Gen2.Model;

namespace Storage.Net.Microsoft.Azure.Storage.Blobs
{
   /// <summary>
   /// Additional Gen 2 storage operations
   /// </summary>
   public interface IAzureDataLakeStorage : IAzureBlobStorage
   {
      /// <summary>
      /// Lists filesystems using Data Lake native REST API
      /// </summary>
      /// <returns></returns>
      Task<IReadOnlyCollection<Filesystem>> ListFilesystemsAsync(CancellationToken cancellationToken = default);

      /// <summary>
      /// Creates a filesystem
      /// </summary>
      /// <param name="filesystemName"></param>
      /// <param name="cancellationToken"></param>
      /// <returns></returns>
      Task CreateFilesystemAsync(string filesystemName, CancellationToken cancellationToken = default);

      /// <summary>
      /// Deletes a filesystem
      /// </summary>
      /// <param name="filesystem"></param>
      /// <param name="cancellationToken"></param>
      /// <returns></returns>
      Task DeleteFilesystemAsync(string filesystem, CancellationToken cancellationToken = default);

   }

   /// <summary>
   /// Azure blob storage specific operations
   /// </summary>
   public interface IAzureBlobStorage : IBlobStorage
   {
      /// <summary>
      /// Acquires a lease
      /// </summary>
      /// <param name="fullPath">Path to container or blob</param>
      /// <param name="maxLeaseTime">When set, leases for a maximum period of time, otherwise lease is set for infinite time.</param>
      /// <param name="proposedLeaseId">When specified, will use as lease ID.</param>
      /// <param name="waitForRelease">When true, the call will wait for the lease to be released.</param>
      /// <param name="cancellationToken"></param>
      /// <returns></returns>
      Task<AzureStorageLease> AcquireLeaseAsync(
         string fullPath,
         TimeSpan? maxLeaseTime = null,
         string proposedLeaseId = null,
         bool waitForRelease = false,
         CancellationToken cancellationToken = default);

      /// <summary>
      /// Breaks container or blob lease
      /// </summary>
      /// <param name="fullPath"></param>
      /// <param name="ignoreErrors">When set, all the errors are ignored. Good for initialisation scenarios.</param>
      /// <param name="cancellationToken"></param>
      /// <returns></returns>
      Task BreakLeaseAsync(
         string fullPath,
         bool ignoreErrors = false,
         CancellationToken cancellationToken = default);

      /// <summary>
      /// Gets public access type for a specific blob container
      /// </summary>
      /// <param name="containerName"></param>
      /// <param name="cancellationToken"></param>
      /// <returns></returns>
      Task<ContainerPublicAccessType> GetContainerPublicAccessAsync(string containerName, CancellationToken cancellationToken = default);

      /// <summary>
      /// Sets public access type for a specific blob container
      /// </summary>
      /// <param name="containerName"></param>
      /// <param name="containerPublicAccessType"></param>
      /// <param name="cancellationToken"></param>
      /// <returns></returns>
      Task SetContainerPublicAccessAsync(string containerName, ContainerPublicAccessType containerPublicAccessType, CancellationToken cancellationToken = default);

   }
}
