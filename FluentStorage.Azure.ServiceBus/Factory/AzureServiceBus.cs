using Azure.Messaging.ServiceBus;
using FluentStorage.Azure.ServiceBus.Messenger;
using FluentStorage.Azure.ServiceBus.Receivers;
using FluentStorage.Queue;

namespace FluentStorage;

/// <summary>
/// Factory class that implement factory methods for Microsoft Azure implememtations
/// </summary>
public static class AzureServiceBus {

	/// <summary>
	/// Creates a new instance of Azure Service Bus Queue by connection string and queue name.
	/// Cast to IAzureServiceBusMessenger to access utility methods for queues, topics and subscriptions
	/// </summary>
	/// <param name="connectionString">Service Bus connection string pointing to a namespace or an entity</param>
	public static IAzureServiceBus FromConnectionString( string connectionString) {
		return new AzureServiceBusMessenger(connectionString);
	}

	/// <summary>
	/// Creates a new instance of Azure Service Bus Queue by connection string and queue name.
	/// Cast to IAzureServiceBusMessenger to access utility methods for queues, topics and subscriptions
	/// </summary>
	/// <param name="connectionString">Service Bus connection string pointing to a namespace or an entity</param>
	/// <param name="serviceBusOptions">Service bus clients specific options</param>
	public static IAzureServiceBus FromConnectionString( string connectionString,
		AzureServiceBusMessengerOptions serviceBusOptions) {
		return new AzureServiceBusMessenger(connectionString,serviceBusOptions);
	}

	/// <summary>
	/// Creates Azure Service Bus Receiver for topic and subscriptions
	/// </summary>
	public static IQueueReceiver ForTopicReceiver(
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
	public static IQueueReceiver ForReceiver(
		string connectionString,
		string queueName,
		bool autocompleteMessages = false,
		ServiceBusClientOptions serviceBusClientOptions = null,
		ServiceBusProcessorOptions messageProcessorOptions = null) {
		return new AzureServiceBusQueueReceiver(connectionString, queueName, autocompleteMessages, serviceBusClientOptions, messageProcessorOptions);
	}

}