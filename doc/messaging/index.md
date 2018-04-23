# Messaging

Messaging is inteded for message passing between one or more systems in disconnected fashion. You can send a message somewhere and current or remote system picks it up for processing later when required. This paradigm somehow fits into [CQRS](https://martinfowler.com/bliki/CQRS.html) and [Message Passing](https://www.defit.org/message-passing/) architectural ideas.

To name a few examples, [Apache Kafka](http://kafka.apache.org/), [RabbitMQ](https://www.rabbitmq.com/), [Azure Service Bus](https://azure.microsoft.com/en-gb/services/service-bus/) are all falling into this category - essentially they are designed to pass messages. Some systems are more advanced to others of course, but most often it doesn't really matter.

Storage.Net supports many messaging providers out of the box, including **Azure Service Bus Topics and Queues**, **Azure Event Hub** and others.

## Using

There are two abstractions available - **message publisher** and **message receiver**. As the name stands, one is publishing messages, and another is receiving them on another end.

### Publishing Messages

To publish messages you will usually construct an instance of `IMessagePublisher` with an appropriate implementation. All the available implementations can be created using factory methods in the `Storage.Net.StorageFactory.Messages` class. More methods appear in that class as you reference an assembly containing specific implementations.

### Receiving Messages

Similarly, to receive messages you can use factory methods to create receivers which all implement `IMessageReceiver` interface.

The primary method of this interface

```csharp
Task StartMessagePumpAsync(
	Func<IEnumerable<QueueMessage>, Task> onMessageAsync,
	int maxBatchSize = 1,
	CancellationToken cancellationToken = default);
```

starts a message pump that listens for incoming queue messages and calls `Func<IEnumerable<QueueMessage>, Task>` as a call back to pass those messages to your code.

`maxBatchSize` is a number specifying how many messages you are ready to handle at once in your callback. Choose this number carefully as specifying number too low will result in slower message processing, whereas number too large will increase RAM requirements for your software.

`cancellationToken` is used to signal the message pump to stop. Not passing any parameter there will result in never stopping message pump. See example below in Use Cases for a pattern on how to use this parameter.


## Serialising/deserialising `QueueMessage`

`QueueMessage` class itself is not a serialisable entity when we talk about JSON or built-in .NET binary serialisation due to the fact it is a functionally rich structure. However, you might want to transfer the whole `QueueMessage` across the wire sometimes. For these purposes you can use built-in binary methods:

```csharp
var qm = new QueueMessage("id", "content");
qm.DequeueCount = 4;
qm.Properties.Add("key", "value");

byte[] wireData = qm.ToByteArray();

//transfer the bytes

QueueMessage receivedMessage = QueueMessage.FromByteArray(wireData);
```

These methods make sure that *all* of the message data is preserved, and also are backward compatible between any changes to this class.


## Use Cases

These example use cases simulate some most common messaging operations which should help you to get started.

### Sending and receiving messages to Azure Event Hub

To start off, you need to create and instance of a `IMessagePublisher` which is an abstract sender, no matter which underlying impelmentation you use. Because we are using Azure Event Hub, the line to create the publisher is as follows:

```csharp
IMessagePublisher publisher = StorageFactory.Messages.AzureEventHubPublisher("connection string");
```

Now let's send a message `hey, mate!` to event hub. To do that we'll have to use the only method on `IMessagePublisher` interface - `PutMessagesAsync`:

```csharp
await publisher.PutMessagesAsync(new[]
{
	new QueueMessage("hey mate!")
});

```

This method accepts an `IEnumerable` of `QueueMessage` which is also an abstract structure wrapping your message, not tied to any implementation. In essence, that's all you need to do to sent a message.

To receive the messages on the other end, you will need to crate an instance of `IMessageReceiver`:

```csharp
IMessageReceiver receiver = StorageFactory.Messages.AzureEventHubReceiver("connection string", "hub path");
```

This instance is an entry point to receiving messages and performing different operations on the message. To listen for the message you'll have to start the message pump first as follows:

```csharp
var cts = new CancellationTokenSource();

await receiver.StartMessagePumpAsync(OnNewMessage, 10, cts.Token);

public async Task OnNewMessage(IEnumerable<QueueMessage> messages)
{
  foreach(QueueMessage message in messages)
  {
    Console.WriteLine($"message received, id: {message.Id}, content: '{message.StringContent}'");
  }
}

// listen for program termination

cts.Cancel();	// cancel the message pump token
```

The `StartMessagePumpAsync` method requires a method which it will call for any new message received, in our case `OnNewMessages`. And that's all you do to listen for messages.

The message pump gets stopped when you cancel the cancellation token passed in `StartMessagePump` method.
