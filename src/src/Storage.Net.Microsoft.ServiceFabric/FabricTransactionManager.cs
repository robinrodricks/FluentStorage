using Microsoft.ServiceFabric.Data;
using System;
using System.Threading.Tasks;

namespace Storage.Net.Microsoft.ServiceFabric
{
   class FabricTransactionManager : ITransactionManager
   {
      private readonly IReliableStateManager _stateManager;
      private readonly ITransaction _tx;
      private bool _commited;

      public FabricTransactionManager(IReliableStateManager stateManager)
      {
         _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
         _tx = _stateManager.CreateTransaction();
      }

      public Task Commit()
      {
         _commited = true;
         return _tx.CommitAsync();
      }

      public ITransaction Tx => _tx;

      public void Dispose()
      {
         if(!_commited)
         {
            _tx.Abort();
         }
      }
   }
}
