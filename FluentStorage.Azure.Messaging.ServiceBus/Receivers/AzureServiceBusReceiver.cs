using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using FluentStorage.Messaging;
using IMessageReceiver = FluentStorage.Messaging.IMessageReceiver;

namespace FluentStorage.Azure.Messaging.ServiceBus.Receivers {
	internal abstract class AzureServiceBusReceiver : IMessageReceiver {
		private readonly string _queueName;
		private readonly string _topicName;
		private readonly string _subscriptionName;

		private readonly ConcurrentDictionary<string, ServiceBusReceivedMessage> _messageIdToBrokeredMessage = new();
		private readonly ServiceBusReceiver _receiverClient;
		private readonly ServiceBusProcessorOptions _messageHandlerOptions;
		private readonly bool _autoComplete;
		private readonly ServiceBusClient _mgmt;
		private Func<IReadOnlyCollection<IQueueMessage>, CancellationToken, Task> _onMessage;

		protected AzureServiceBusReceiver(string connectionstring,
		                                  string queueName,
		                                  string topicName,
		                                  string subscriptionName,
		                                  ServiceBusClientOptions clientOptions,
		                                  ServiceBusProcessorOptions processorOptions) {
			_queueName        = queueName;
			_topicName        = topicName;
			_subscriptionName = subscriptionName;

			clientOptions ??= new ServiceBusClientOptions();

			_mgmt = new ServiceBusClient(connectionstring, clientOptions);

			_receiverClient = !string.IsNullOrEmpty(queueName)
				                  ? _mgmt.CreateReceiver(queueName)
				                  : _mgmt.CreateReceiver(topicName, subscriptionName);

			_messageHandlerOptions = processorOptions ??
			                         new ServiceBusProcessorOptions {
				                         AutoCompleteMessages = false,
				                         /*
				                          * In fact, what the property actually means is the maximum about of time they lock renewal will happen for internally on the subscription client.
				                          * So if you set this to 24 hours e.g. Timespan.FromHours(24) and your processing was to take 12 hours, it would be renewed. However, if you set
				                          * this to 12 hours using Timespan.FromHours(12) and your code ran for 24, when you went to complete the message it would give a lockLost exception
				                          * (as I was getting above over shorter intervals!).
				                          *
				                          * in fact, Microsoft's implementation runs a background task that periodically renews the message lock until it expires.
				                          */
				                         MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(10), //should be in fact called "max processing time"
				                         MaxConcurrentCalls = 1
			                         };

			_autoComplete = _messageHandlerOptions.AutoCompleteMessages;

			//note: we can't use management SDK as it requires high priviledged SP in Azure
		}

		public async Task ConfirmMessagesAsync(IReadOnlyCollection<IQueueMessage> messages,
		                                       CancellationToken cancellationToken = default) {
			if (_autoComplete)
				return;

			await Task.WhenAll(messages.Select(ConfirmAsync)).ConfigureAwait(false);
		}

		private async Task ConfirmAsync(IQueueMessage message) {
			//delete the message and get the deleted element, very nice method!
			if (!_messageIdToBrokeredMessage.TryRemove(message.Id, out ServiceBusReceivedMessage bm))
				return;

			await _receiverClient.CompleteMessageAsync(bm).ConfigureAwait(false);
		}

		public Task<int> GetMessageCountAsync() => throw new NotSupportedException();


		public async Task DeadLetterAsync(IQueueMessage message, string reason, string errorDescription,
		                                  CancellationToken cancellationToken = default) {
			if (_autoComplete)
				return;

			if (!_messageIdToBrokeredMessage.TryRemove(message.Id, out ServiceBusReceivedMessage bm))
				return;

			await _receiverClient.DeadLetterMessageAsync(bm, cancellationToken: cancellationToken)
			                     .ConfigureAwait(false);
		}

		public async Task KeepAliveAsync(IQueueMessage message, TimeSpan? timeToLive = null,
		                                 CancellationToken cancellationToken = default) {
			if (_autoComplete)
				return;

			if (!_messageIdToBrokeredMessage.TryGetValue(message.Id, out ServiceBusReceivedMessage bm))
				return;

			await _receiverClient.RenewMessageLockAsync(bm, cancellationToken).ConfigureAwait(false);
		}

		public Task<ITransaction> OpenTransactionAsync() {
			return Task.FromResult(EmptyTransaction.Instance);
		}


		public async Task StartMessagePumpAsync(
			Func<IReadOnlyCollection<IQueueMessage>, CancellationToken, Task> onMessageAsync,
			int maxBatchSize = 1,
			CancellationToken cancellationToken = default) {
			_onMessage = onMessageAsync ?? throw new ArgumentNullException(nameof(onMessageAsync));

			var processor = !string.IsNullOrEmpty(_queueName)
				                ? _mgmt.CreateProcessor(_queueName, _messageHandlerOptions)
				                : _mgmt.CreateProcessor(_topicName, _subscriptionName, _messageHandlerOptions);

			processor.UpdatePrefetchCount(maxBatchSize);

			cancellationToken.Register(() => {
				processor.ProcessMessageAsync -= ProcessorOnProcessMessage;
				processor.ProcessErrorAsync   -= DefaultExceptionReceiverHandler;
				processor.StopProcessingAsync(cancellationToken).ConfigureAwait(false);
				processor.DisposeAsync();
			});

			processor.ProcessMessageAsync += ProcessorOnProcessMessage;
			processor.ProcessErrorAsync   += DefaultExceptionReceiverHandler;

			await processor.StartProcessingAsync(cancellationToken).ConfigureAwait(false);
		}

		private async Task ProcessorOnProcessMessage(ProcessMessageEventArgs args) {
			IQueueMessage qm = Converter.ToQueueMessage(args.Message);
			if (!_autoComplete)
				_messageIdToBrokeredMessage[qm.Id] = args.Message;

			await _onMessage(new[] { qm }, args.CancellationToken).ConfigureAwait(false);
		}

		private Task DefaultExceptionReceiverHandler(ProcessErrorEventArgs args) {
			if (args?.Exception is OperationCanceledException) {
				// operation cancelled, ignore
			}

			if (args != null) {
				// the error source tells me at what point in the processing an error occurred
				Console.WriteLine(args.ErrorSource);
				// the fully qualified namespace is available
				Console.WriteLine(args.FullyQualifiedNamespace);
				// as well as the entity path
				Console.WriteLine(args.EntityPath);
				Console.WriteLine(args.Exception.ToString());
			}

			//extra handling code
			return Task.FromResult(true);
		}

		public async Task<IReadOnlyCollection<IQueueMessage>> PeekMessagesAsync(
			int maxMessages, CancellationToken cancellationToken = default) {
			var peek = await _receiverClient
			       .PeekMessagesAsync(maxMessages, cancellationToken: cancellationToken)
			       .ConfigureAwait(false);

			return peek.Select(Converter.ToQueueMessage).ToList();
		}

		public void Dispose() {
			_receiverClient.CloseAsync();
			_mgmt.DisposeAsync();
		}
	}
}
