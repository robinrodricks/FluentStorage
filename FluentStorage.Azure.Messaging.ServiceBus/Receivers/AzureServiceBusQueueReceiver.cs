using Azure.Messaging.ServiceBus;

namespace FluentStorage.Azure.Messaging.ServiceBus.Receivers {
	/// <summary>
	/// Implements message receiver on Azure Service Bus Queues
	/// </summary>
	internal class AzureServiceBusQueueReceiver : AzureServiceBusReceiver {
		internal AzureServiceBusQueueReceiver(string connectionString, string queueName,
		                                      bool autocompleteMessages = false,
		                                      ServiceBusClientOptions clientOptions = null,
		                                      ServiceBusProcessorOptions processorOptions = null)
			: base(connectionString, queueName, "", "", autocompleteMessages, clientOptions, processorOptions) {
		}
	}
}
