using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Storage.Net.KeyValue;

namespace Storage.Net.Microsoft.Azure.Storage.Tables
{
   /// <summary>
   /// Azure specific operations
   /// </summary>
   public interface IAzureStorageTablesKeyValueStorage : IKeyValueStorage
   {
      /// <summary>
      /// 
      /// </summary>
      Task<IReadOnlyCollection<BlobMetrics>> GetBasicBlobMetricsAsync();
   }
}
