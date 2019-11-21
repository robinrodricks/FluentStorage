using System.Collections.Generic;
using System.Threading.Tasks;
using Storage.Net.Microsoft.Azure.Storage.Tables;
using Xunit;

namespace Storage.Net.Tests.Integration.Azure
{
   [Trait("Category", "KeyValue")]
   public class LeakyAzureTableStorageTest
   {
      private readonly IAzureStorageTablesKeyValueStorage _tables;

      public LeakyAzureTableStorageTest()
      {
         ITestSettings settings = Settings.Instance;

         _tables = StorageFactory.KeyValue.AzureTableStorage(
            settings.AzureStorageName, settings.AzureStorageKey);
      }

      [Fact]
      public async Task Metrics_BasicBlobMetrics()
      {
         IReadOnlyCollection<BlobMetrics> metrics = await _tables.GetBasicBlobMetricsAsync();
         Assert.True(metrics.Count > 0);
      }
   }
}
