using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using FluentStorage.Messaging;

namespace FluentStorage.Azure.Messaging.ServiceBus.Messenger {
	/// <summary>
	/// Messenger service for Azure ServiceBus, cast to IAzureMessagingServiceBusMessenger to access utility methods for queues, topics and subscriptions
	/// </summary>
	public class AzureServiceBusMessenger : IAzureMessagingServiceBusMessenger {

		private readonly ServiceBusClient _mgmt;
		private readonly ServiceBusAdministrationClient _mgmtAdminClient;
		private readonly AzureServiceBusMessengerOptions _options;

		private readonly ConcurrentDictionary<string, ServiceBusSender> _channelNameToMessageSender = new();
		private readonly ConcurrentDictionary<string, ServiceBusReceiver> _channelNameToMessageReceiver = new();

		internal AzureServiceBusMessenger(string connectionString) {
			_options         = new AzureServiceBusMessengerOptions();
			_mgmt            = new ServiceBusClient(connectionString);
			_mgmtAdminClient = new ServiceBusAdministrationClient(connectionString);
		}

		internal AzureServiceBusMessenger(string connectionString, AzureServiceBusMessengerOptions options) {
			_options         = options;
			_mgmt            = new ServiceBusClient(connectionString, options.ClientOptions);
			_mgmtAdminClient = new ServiceBusAdministrationClient(connectionString, options.AdminClientOptions);
		}

		ServiceBusSender CreateOrGetSenderClient(string channelName) =>
			_channelNameToMessageSender.GetOrAdd(channelName,
				_mgmt.CreateSender(channelName.ToServiceBusChannel().Name, _options.SenderOptions));

		ServiceBusReceiver CreateMessageReceiver(string channelName) {
			var channel = channelName.ToServiceBusChannel();

			return channel.IsQueue || channel.Subscription == ""
				       ? _channelNameToMessageReceiver.GetOrAdd(channelName, _mgmt.CreateReceiver(channel.Name, _options.ReceiverOptions))
				       : _channelNameToMessageReceiver.GetOrAdd(channelName,
					       _mgmt.CreateReceiver(channel.Name, channel.Subscription, _options.ReceiverOptions));
		}

		#region [ IMessenger ]

		public async Task CreateChannelsAsync(IEnumerable<string> channelNames,
		                                      CancellationToken cancellationToken = default) {
			foreach (string channelName in channelNames) {
				var channel = channelName.ToServiceBusChannel();

				if (channel.IsQueue) {
					await CreateQueueAsync(channel.Name, cancellationToken);
				}
				else if (!string.IsNullOrEmpty(channel.Subscription)) {
					await CreateSubScriptionAsync(channel.Name, channel.Subscription, cancellationToken);
				}
				else if (!await _mgmtAdminClient.TopicExistsAsync(channel.Name, cancellationToken).ConfigureAwait(false)) {
					await CreateTopicAsync(channel.Name, cancellationToken);
				}
			}
		}

		public async Task<IReadOnlyCollection<string>> ListChannelsAsync(CancellationToken cancellationToken = default) {
			var channels = new List<string>();

			var queues = _mgmtAdminClient.GetQueuesAsync(cancellationToken).ConfigureAwait(false);
			var topics = _mgmtAdminClient.GetTopicsAsync().ConfigureAwait(false);

			var subscriptionFound = false;

			await foreach (var queue in queues) {
				channels.Add($"{AzureServiceBusChannel.QueuePrefix}{queue.Name}");
			}

			await foreach (var topic in topics) {
				var subscriptions = _mgmtAdminClient.GetSubscriptionsAsync(topic.Name).ConfigureAwait(false);

				await foreach (var subscription in subscriptions) {
					subscriptionFound = true;
					channels.Add($"{AzureServiceBusChannel.TopicPrefix}{subscription.TopicName}/{subscription.SubscriptionName}");
				}
			}

			if (subscriptionFound == false) {
				await foreach (var topic in topics) {
					channels.Add($"{AzureServiceBusChannel.TopicPrefix}{topic.Name}");
				}
			}

			return channels;
		}

		public async Task DeleteChannelsAsync(IEnumerable<string> channelNames, CancellationToken cancellationToken = default) {
			if (channelNames is null)
				throw new ArgumentNullException(nameof(channelNames));

			foreach (string channelName in channelNames) {
				var channel = channelName.ToServiceBusChannel();

				if (channel.IsQueue) {
					await DeleteQueueAsync(channel.Name, cancellationToken);
				}
				else if (channel.Subscription != "") {
					await DeleteSubScriptionAsync(channel.Name, channel.Subscription, cancellationToken);
				}
				else {
					await DeleteTopicAsync(channel.Name, cancellationToken);
				}
			}
		}

		public async Task<long> GetMessageCountAsync(string channelName, CancellationToken cancellationToken = default) {
			if (channelName is null)
				throw new ArgumentNullException(nameof(channelName));

			var channel = channelName.ToServiceBusChannel();

			if (channel.IsQueue)
				return await CountQueueAsync(channel.Name, cancellationToken);

			if (channel.Subscription != "")
				return await CountSubScriptionAsync(channel.Name, channel.Subscription, cancellationToken);

			return await CountTopicAsync(channel.Name, cancellationToken);
		}

		public async Task SendAsync(string channelName, IEnumerable<QueueMessage> messages, CancellationToken cancellationToken = default) {
			var client       = CreateOrGetSenderClient(channelName);
			var messageQueue = new Queue<ServiceBusMessage>();

			foreach (var queueMessage in messages.Select(Converter.ToMessage)) {
				messageQueue.Enqueue(queueMessage);
			}

			int messageCount = messageQueue.Count;

			while (messageQueue.Count > 0) {
				// start a new batch
				using ServiceBusMessageBatch messageBatch = await client.CreateMessageBatchAsync(cancellationToken);

				// add the first message to the batch
				if (messageBatch.TryAddMessage(messageQueue.Peek())) {
					messageQueue.Dequeue();
				}
				else {
					// if the first message can't fit, then it is too large for the batch
					throw new Exception($"Message {messageCount - messageQueue.Count} is too large and cannot be sent.");
				}

				// add as many messages as possible to the current batch
				while (messageQueue.Count > 0 && messageBatch.TryAddMessage(messageQueue.Peek())) {
					messageQueue.Dequeue();
				}

				await client.SendMessagesAsync(messageBatch, cancellationToken);
			}
		}

		public async Task<IReadOnlyCollection<QueueMessage>> ReceiveAsync(string channelName, int count = 100, TimeSpan? visibility = null,
		                                                                   CancellationToken cancellationToken = default) {
			if (channelName is null)
				throw new ArgumentNullException(nameof(channelName));

			var receiver = CreateMessageReceiver(channelName);
			var messages = await receiver.ReceiveMessagesAsync(count, cancellationToken: cancellationToken).ConfigureAwait(false);

			return messages.Select(Converter.ToQueueMessage).ToList();
		}

		public async Task<IReadOnlyCollection<QueueMessage>> PeekAsync(string channelName, int count = 100, CancellationToken cancellationToken = default) {
			if (channelName is null)
				throw new ArgumentNullException(nameof(channelName));

			var receiver = CreateMessageReceiver(channelName);
			var messages = await receiver.PeekMessagesAsync(count, cancellationToken: cancellationToken).ConfigureAwait(false);

			return messages.Select(Converter.ToQueueMessage).ToList();
		}

		/// <exception cref="NotImplementedException"></exception>
		public Task DeleteAsync(string channelName, IEnumerable<QueueMessage> messages,
		                        CancellationToken cancellationToken = default) => throw new NotImplementedException();

		/// <exception cref="NotImplementedException"></exception>
		public Task StartMessageProcessorAsync(string channelName, IMessageProcessor messageProcessor) =>
			throw new NotImplementedException();

		public void Dispose() {
			foreach (var sender in _channelNameToMessageSender) {
				TaskUtils.RunSync(() => sender.Value.CloseAsync());
			}

			foreach (var receiver in _channelNameToMessageReceiver) {
				TaskUtils.RunSync(() => receiver.Value.CloseAsync());
			}

			_mgmt.DisposeAsync();
		}

		#endregion

		#region [ IAzureServiceBusMessenger ]

		public async Task SendToQueueAsync(string queue, IEnumerable<QueueMessage> messages, CancellationToken cancellationToken = default) {
			var channel = $"{AzureServiceBusChannel.QueuePrefix}{queue}";
			await SendAsync(channel, messages, cancellationToken);
		}

		public async Task SendToTopicAsync(string topic, IEnumerable<QueueMessage> messages, CancellationToken cancellationToken = default) {
			var channel = $"{AzureServiceBusChannel.TopicPrefix}{topic}";
			await SendAsync(channel, messages, cancellationToken);
		}
		public async Task SendToSubscriptionAsync(string topic, string subscription, IEnumerable<QueueMessage> messages, CancellationToken cancellationToken = default) {
			var channel = $"{AzureServiceBusChannel.TopicPrefix}{topic}/{subscription}";
			await SendAsync(channel, messages, cancellationToken);
		}
		public async Task CreateQueueAsync(string queue, CancellationToken cancellationToken = default) {
			if (!await _mgmtAdminClient.QueueExistsAsync(queue, cancellationToken).ConfigureAwait(false))
				await _mgmtAdminClient.CreateQueueAsync(queue, cancellationToken);
		}

		public async Task CreateTopicAsync(string topic,CancellationToken cancellationToken = default) {
			if ( !await _mgmtAdminClient.TopicExistsAsync(topic, cancellationToken).ConfigureAwait(false))
				await _mgmtAdminClient.CreateTopicAsync(topic, cancellationToken);
		}

		public async Task CreateSubScriptionAsync(string topic, string subscription, CancellationToken cancellationToken = default) {
			if ( !await _mgmtAdminClient.SubscriptionExistsAsync(topic, subscription, cancellationToken).ConfigureAwait(false))
				await _mgmtAdminClient.CreateSubscriptionAsync(topic, subscription, cancellationToken);
		}

		public async Task DeleteQueueAsync(string queue, CancellationToken cancellationToken = default) {
			if (!await _mgmtAdminClient.QueueExistsAsync(queue, cancellationToken).ConfigureAwait(false))
				await _mgmtAdminClient.DeleteQueueAsync(queue, cancellationToken);
		}

		public async Task DeleteSubScriptionAsync(string topic, string subscription, CancellationToken cancellationToken = default) {
			if ( !await _mgmtAdminClient.SubscriptionExistsAsync(topic, subscription, cancellationToken).ConfigureAwait(false))
				await _mgmtAdminClient.DeleteSubscriptionAsync(topic, subscription, cancellationToken);
		}

		public async Task DeleteTopicAsync(string topic,CancellationToken cancellationToken = default) {
			if ( !await _mgmtAdminClient.TopicExistsAsync(topic, cancellationToken).ConfigureAwait(false))
				await _mgmtAdminClient.DeleteTopicAsync(topic, cancellationToken);
		}

		public async Task<long> CountQueueAsync(string queue, CancellationToken cancellationToken = default) {
			if (!await _mgmtAdminClient.QueueExistsAsync(queue, cancellationToken).ConfigureAwait(false)) {
				var qinfo = await _mgmtAdminClient.GetQueueRuntimePropertiesAsync(queue, cancellationToken);
				return qinfo.HasValue ? qinfo.Value.TotalMessageCount : 0;
			}
			return 0;
		}

		public async Task<long> CountSubScriptionAsync(string topic, string subscription, CancellationToken cancellationToken = default) {
			if (await _mgmtAdminClient.SubscriptionExistsAsync(topic, subscription, cancellationToken).ConfigureAwait(false)) {
				var topicInfo = await _mgmtAdminClient.GetSubscriptionRuntimePropertiesAsync(topic, subscription, cancellationToken);
				return topicInfo.HasValue ? topicInfo.Value.TotalMessageCount : 0;
			}
			return 0;
		}

		public async Task<long> CountTopicAsync(string topic,CancellationToken cancellationToken = default) {
			if (await _mgmtAdminClient.TopicExistsAsync(topic, cancellationToken).ConfigureAwait(false)) {
				var topicInfo = await _mgmtAdminClient.GetTopicRuntimePropertiesAsync(topic, cancellationToken);
				return topicInfo.HasValue ? topicInfo.Value.ScheduledMessageCount : 0;
			}
			return 0;
		}

		#endregion
	}
}
