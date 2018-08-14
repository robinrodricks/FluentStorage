using System;
using System.Collections.Generic;
using Storage.Net.Blob;
using NetBox.Extensions;
using System.Linq;

namespace Storage.Net.ConnectionString
{
   static class ConnectionStringFactory
   {
      private const string TypeSeparator = "://";
      private static readonly List<IConnectionFactory> Factories = new List<IConnectionFactory>();

      static ConnectionStringFactory()
      {
         Register(new BuiltInConnectionFactory());
      }

      public static void Register(IConnectionFactory factory)
      {
         if (factory == null) throw new ArgumentNullException(nameof(factory));

         Factories.Add(factory);
      }

      public static IBlobStorage CreateBlobStorage(string connectionString)
      {
         if (connectionString == null)
         {
            throw new ArgumentNullException(nameof(connectionString));
         }

         var pcs = new StorageConnectionString(connectionString);

         IBlobStorage instance = Factories
            .Select(f => f.CreateBlobStorage(pcs))
            .Where(b => b != null)
            .FirstOrDefault();

         if (instance == null)
         {
            throw new ArgumentException(
               $"could not create any implementation based on the passed connection string (prefix: {pcs.Prefix}), did you register required external module?",
               nameof(connectionString));
         }

         return instance;
      }

   }
}
