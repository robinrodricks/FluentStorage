using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentStorage.Messaging;

namespace FluentStorage.Azure.ServiceBus.Messenger {
	/// <summary>
	/// Provides specific messaging capabilities to Azure Service Bus IMessenger.
	/// </summary>
	public interface IAzureServiceBusMessenger : IMessenger {

		/// <summary>
		/// Sends a collection of messages to the specified queue asynchronously.
		/// </summary>
		/// <param name="queue">The name of the queue.</param>
		/// <param name="messages">The collection of messages to send.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>A task representing the asynchronous operation.</returns>
		Task SendToQueueAsync(string queue, IEnumerable<QueueMessage> messages, CancellationToken cancellationToken = default);

		/// <summary>
		/// Sends a collection of messages to the specified topic asynchronously.
		/// </summary>
		/// <param name="topic">The name of the topic.</param>
		/// <param name="messages">The collection of messages to send.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>A task representing the asynchronous operation.</returns>
		Task SendToTopicAsync(string topic, IEnumerable<QueueMessage> messages, CancellationToken cancellationToken = default);

		/// <summary>
		/// Sends a collection of messages to the specified topic subscription asynchronously.
		/// </summary>
		/// <param name="topic">The name of the topic.</param>
		/// <param name="subscription">The name of the subscription.</param>
		/// <param name="messages">The collection of messages to send.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>A task representing the asynchronous operation.</returns>
		Task SendToSubscriptionAsync(string topic, string subscription, IEnumerable<QueueMessage> messages, CancellationToken cancellationToken = default);

		/// <summary>
		/// Create a new Queue in Azure ServiceBus
		/// </summary>
		/// <param name="name">Queue name</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns></returns>
		Task CreateQueueAsync(string name, CancellationToken cancellationToken = default);

		/// <summary>
		/// Create a new Topic in Azure ServiceBus
		/// </summary>
		/// <param name="topic">Topic name</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns></returns>
		Task CreateTopicAsync(string topic, CancellationToken cancellationToken = default);

		/// <summary>
		/// Create a new Subscription in Azure ServiceBus
		/// </summary>
		/// <param name="topic">Topic name</param>
		/// <param name="subscription">Subscription name</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns></returns>
		Task CreateSubScriptionAsync(string topic, string subscription, CancellationToken cancellationToken = default);

		/// <summary>
		/// Deletes the specified queue asynchronously.
		/// </summary>
		/// <param name="queue">The name of the queue.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>A task representing the asynchronous operation.</returns>
		Task DeleteQueueAsync(string queue, CancellationToken cancellationToken = default);

		/// <summary>
		/// Deletes the specified topic subscription asynchronously.
		/// </summary>
		/// <param name="topic">The name of the topic.</param>
		/// <param name="subscription">The name of the subscription.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>A task representing the asynchronous operation.</returns>
		Task DeleteSubScriptionAsync(string topic, string subscription, CancellationToken cancellationToken = default);

		/// <summary>
		/// Deletes the specified topic asynchronously.
		/// </summary>
		/// <param name="topic">The name of the topic.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>A task representing the asynchronous operation.</returns>
		Task DeleteTopicAsync(string topic, CancellationToken cancellationToken = default);

		/// <summary>
		/// Counts the number of messages in the specified queue asynchronously.
		/// </summary>
		/// <param name="queue">The name of the queue.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>The number of messages in the queue.</returns>
		Task<long> CountQueueAsync(string queue, CancellationToken cancellationToken = default);

		/// <summary>
		/// Counts the number of messages in the specified topic subscription asynchronously.
		/// </summary>
		/// <param name="topic">The name of the topic.</param>
		/// <param name="subscription">The name of the subscription.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>The number of messages in the topic subscription.</returns>
		Task<long> CountSubScriptionAsync(string topic, string subscription, CancellationToken cancellationToken = default);

		/// <summary>
		/// Counts the number of messages in the specified topic asynchronously.
		/// </summary>
		/// <param name="topic">The name of the topic.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>The number of messages in the topic.</returns>
		Task<long> CountTopicAsync(string topic, CancellationToken cancellationToken = default);
	}
}
