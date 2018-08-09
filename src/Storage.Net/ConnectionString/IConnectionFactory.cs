using Storage.Net.Blob;

namespace Storage.Net.ConnectionString
{
   /// <summary>
   /// Connection factory is responsible for creating storage instances from connection strings. It
   /// is usually implemented by every external module, however is optional.
   /// </summary>
   public interface IConnectionFactory
   {
      /// <summary>
      /// Creates a blob storage instance from connection string
      /// </summary>
      IBlobStorage CreateBlobStorage(StorageConnectionString connectionString);
   }
}
