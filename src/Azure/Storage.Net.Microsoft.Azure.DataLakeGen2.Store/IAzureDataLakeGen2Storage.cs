using Storage.Net.Blobs;
using Storage.Net.Microsoft.Azure.DataLakeGen2.Store.Gen2.Interfaces;

namespace Storage.Net.Microsoft.Azure.DataLakeGen2.Store
{
   public interface IAzureDataLakeGen2Storage : IBlobStorage
   {
      IDataLakeGen2Client Client { get; }
   }
}
