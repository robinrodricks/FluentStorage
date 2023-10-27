using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

namespace FluentStorage.Azure.Messaging.ServiceBus.Messenger;

public class AzureServiceBusMessengerOptions {
	public ServiceBusClientOptions               ClientOptions      { get; set; } = new();
	public ServiceBusAdministrationClientOptions AdminClientOptions { get; set; } = new();
	public ServiceBusSenderOptions               SenderOptions      { get; set; } = new();
	public ServiceBusReceiverOptions             ReceiverOptions    { get; set; } = new();
}
