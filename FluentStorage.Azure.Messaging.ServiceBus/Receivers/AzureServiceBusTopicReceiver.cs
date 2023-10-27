using Azure.Messaging.ServiceBus;

namespace FluentStorage.Azure.Messaging.ServiceBus.Receivers {
	/// <summary>
	/// Implements message receiver on Azure Service Bus Queues
	/// </summary>
	internal class AzureServiceBusTopicReceiver : AzureServiceBusReceiver {
		internal AzureServiceBusTopicReceiver(string connectionString, string topicName, string subscriptionName,
		                                       ServiceBusClientOptions clientOptions = null,
		                                       ServiceBusProcessorOptions processorOptions = null)
			: base(connectionString, "", topicName, subscriptionName, clientOptions, processorOptions) {
		}
	}
}
