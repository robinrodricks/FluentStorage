using Storage.Net.Blob;
using Storage.Net.KeyValue;

namespace Storage.Net.ConnectionString
{
   /// <summary>
   /// Connection factory is responsible for creating storage instances from connection strings. It
   /// is usually implemented by every external module, however is optional.
   /// </summary>
   public interface IConnectionFactory
   {
      /// <summary>
      /// Creates a blob storage instance from connection string if possible. When this factory does not support this connection
      /// string it returns null.
      /// </summary>
      IBlobStorage CreateBlobStorage(StorageConnectionString connectionString);

      /// <summary>
      /// Creates a key-value storage instance from connection string if possible. When this factory does not support this connection
      /// string it returns null.
      /// </summary>
      IKeyValueStorage CreateKeyValueStorage(StorageConnectionString connectionString);
   }
}
