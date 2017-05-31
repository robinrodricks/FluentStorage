using Microsoft.ServiceFabric.Data;
using System;
using System.Threading.Tasks;

namespace Storage.Net.Microsoft.ServiceFabric
{
   class FabricTransactionManager<TCollection> : ITransactionManager
   {
      private readonly IReliableStateManager _stateManager;
      private bool _commited;

      public FabricTransactionManager(IReliableStateManager stateManager, TCollection collection)
      {
         _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
         Tx = _stateManager.CreateTransaction();
         Collection = collection;
      }

      public Task CommitAsync()
      {
         _commited = true;
         return Tx.CommitAsync();
      }

      public ITransaction Tx { get; }

      public TCollection Collection { get; }

      public void Dispose()
      {
         if(!_commited)
         {
            Tx.Abort();
         }

         Tx.Dispose();
      }
   }
}
