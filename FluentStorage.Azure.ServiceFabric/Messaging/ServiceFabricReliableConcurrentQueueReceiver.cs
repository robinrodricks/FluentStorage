﻿using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using FluentStorage.Messaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace FluentStorage.Microsoft.ServiceFabric.Messaging {
	class ServiceFabricReliableConcurrentQueueReceiver : AbstractServiceFabricReliableQueueReceiver {
		private readonly IReliableStateManager _stateManager;
		private readonly string _queueName;

		public ServiceFabricReliableConcurrentQueueReceiver(IReliableStateManager stateManager, string queueName, TimeSpan scanInterval)
		   : base(stateManager, queueName, scanInterval) {
			_stateManager = stateManager;
			_queueName = queueName;
		}

		protected override Task<int> GetMessageCountAsync(IReliableState reliableState, ServiceFabricTransaction transaction) {
			IReliableConcurrentQueue<byte[]> collection = (IReliableConcurrentQueue<byte[]>)reliableState;

			return Task.FromResult((int)collection.Count);
		}

		protected override async Task<ConditionalValue<byte[]>> TryDequeueAsync(ServiceFabricTransaction tx, IReliableState collectionBase, CancellationToken cancellationToken) {
			IReliableConcurrentQueue<byte[]> collection = (IReliableConcurrentQueue<byte[]>)collectionBase;

			ConditionalValue<byte[]> message = await collection.TryDequeueAsync(tx.Tx, cancellationToken, TimeSpan.FromSeconds(4)).ConfigureAwait(false);

			return message;
		}

		protected override async Task<IReliableState> GetCollectionAsync() {
			return await _stateManager.GetOrAddAsync<IReliableConcurrentQueue<byte[]>>(_queueName).ConfigureAwait(false);
		}
	}
}
