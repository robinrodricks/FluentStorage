using System;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using AzureStorageException = Microsoft.Azure.Storage.StorageException;

namespace Storage.Net.Microsoft.Azure.Storage.Blobs
{
   /// <summary>
   /// Represents a blob lease
   /// </summary>
   public class AzureStorageLease : IDisposable
   {
      private readonly CloudBlobContainer _container;
      private readonly CloudBlockBlob _blob;

      internal AzureStorageLease(CloudBlobContainer container, CloudBlockBlob blob, string leaseId)
      {
         _container = container;
         _blob = blob;
         LeaseId = leaseId;
      }

      /// <summary>
      /// Original Lease ID
      /// </summary>
      public string LeaseId { get; private set; }

      /// <summary>
      /// Original container, or container that is leased.
      /// </summary>
      internal CloudBlobContainer LeaseContainer => _container;

      /// <summary>
      /// Original blob that is leased, or null if contianer is leased.
      /// </summary>
      internal CloudBlockBlob LeasedBlob => _blob;

      /// <summary>
      /// Renews active lease
      /// </summary>
      /// <returns></returns>
      public async Task RenewLeaseAsync()
      {
         if(_blob == null)
         {
            await _container.RenewLeaseAsync(AccessCondition.GenerateLeaseCondition(LeaseId)).ConfigureAwait(false);
         }
         else
         {
            await _blob.RenewLeaseAsync(AccessCondition.GenerateLeaseCondition(LeaseId)).ConfigureAwait(false);
         }
      }

      /// <summary>
      /// Releases the lease
      /// </summary>
      public void Dispose()
      {
         try
         {
            if(_blob == null)
            {
               _container.ReleaseLeaseAsync(AccessCondition.GenerateLeaseCondition(LeaseId)).Wait();
            }
            else
            {
               _blob.ReleaseLeaseAsync(AccessCondition.GenerateLeaseCondition(LeaseId)).Wait();

            }
         }
         catch(AggregateException egex)
            when(
               (egex.InnerException is AzureStorageException sx) && 
               (sx.RequestInformation.ErrorCode == "LeaseIdMismatchWithLeaseOperation"))
         {
            
         }
      }
   }
}
