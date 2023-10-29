using Azure.Messaging.ServiceBus;
using FluentStorage.Azure.ServiceBus.Messenger;
using FluentStorage.Azure.ServiceBus.Receivers;
using FluentStorage.Messaging;

namespace FluentStorage.Azure.ServiceBus {
	/// <summary>
	/// Factory class that implement factory methods for Microsoft Azure implememtations
	/// </summary>
	public static class Factory {

		/// <summary>
		/// Creates a new instance of Azure Service Bus Queue by connection string and queue name.
		/// Cast to IAzureServiceBusMessenger to access utility methods for queues, topics and subscriptions
		/// </summary>
		/// <param name="factory">Factory reference</param>
		/// <param name="connectionString">Service Bus connection string pointing to a namespace or an entity</param>
		public static IMessenger AzureServiceBus(this IMessagingFactory factory, string connectionString) {
			return new AzureServiceBusMessenger(connectionString);
		}

		/// <summary>
		/// Creates a new instance of Azure Service Bus Queue by connection string and queue name.
		/// Cast to IAzureServiceBusMessenger to access utility methods for queues, topics and subscriptions
		/// </summary>
		/// <param name="factory">Factory reference</param>
		/// <param name="connectionString">Service Bus connection string pointing to a namespace or an entity</param>
		/// <param name="serviceBusOptions">Service bus clients specific options</param>
		public static IMessenger AzureServiceBus(this IMessagingFactory factory, string connectionString,
		                                                  AzureServiceBusMessengerOptions serviceBusOptions) {
			return new AzureServiceBusMessenger(connectionString,serviceBusOptions);
		}

		/// <summary>
		/// Creates Azure Service Bus Receiver for topic and subscriptions
		/// </summary>
		public static IMessageReceiver AzureServiceBusTopicReceiver(this IMessagingFactory factory,
		   string connectionString,
		   string topicName,
		   string subscriptionName,
		   bool autocompleteMessages = false,
		   ServiceBusClientOptions serviceBusClientOptions = null,
		   ServiceBusProcessorOptions messageProcessorOptions = null) {
			return new AzureServiceBusTopicReceiver(connectionString, topicName, subscriptionName, autocompleteMessages, serviceBusClientOptions, messageProcessorOptions);
		}

		/// <summary>
		/// Creates Azure Service Bus Receiver for queues
		/// </summary>
		public static IMessageReceiver AzureServiceBusQueueReceiver(this IMessagingFactory factory,
		                                                                     string connectionString,
		                                                                     string queueName,
		                                                                     bool autocompleteMessages = false,
		                                                                     ServiceBusClientOptions serviceBusClientOptions = null,
		                                                                     ServiceBusProcessorOptions messageProcessorOptions = null) {
			return new AzureServiceBusQueueReceiver(connectionString, queueName, autocompleteMessages, serviceBusClientOptions, messageProcessorOptions);
		}

	}
}
