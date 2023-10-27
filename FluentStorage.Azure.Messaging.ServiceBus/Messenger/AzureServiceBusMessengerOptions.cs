using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

namespace FluentStorage.Azure.Messaging.ServiceBus.Messenger;

/// <summary>
/// Represents the options for configuring the Azure Service Bus messenger.
/// </summary>
public class AzureServiceBusMessengerOptions {
	/// <summary>
	/// Gets or sets the options for configuring the Service Bus client.
	/// </summary>
	public ServiceBusClientOptions ClientOptions { get; set; } = new();

	/// <summary>
	/// Gets or sets the options for configuring the Service Bus administration client.
	/// </summary>
	public ServiceBusAdministrationClientOptions AdminClientOptions { get; set; } = new();

	/// <summary>
	/// Gets or sets the options for configuring the Service Bus sender.
	/// </summary>
	public ServiceBusSenderOptions SenderOptions { get; set; } = new();

	/// <summary>
	/// Gets or sets the options for configuring the Service Bus receiver.
	/// </summary>
	public ServiceBusReceiverOptions ReceiverOptions { get; set; } = new();
}
