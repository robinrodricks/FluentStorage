using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentStorage.Messaging;

namespace FluentStorage.Azure.Messaging.ServiceBus.Messenger {
	/// <summary>
	/// Azure Service Bus specific information
	/// </summary>
	public interface IAzureMessagingServiceBusMessenger : IMessenger {
		public Task SendToQueueAsync(string queue, IEnumerable<IQueueMessage> messages,
		                             CancellationToken cancellationToken = default);

		public Task SendToTopicAsync(string topic, IEnumerable<IQueueMessage> messages,
		                             CancellationToken cancellationToken = default);

		public Task SendToSubscriptionAsync(string topic, string subscription,
		                                    IEnumerable<IQueueMessage> messages,
		                                    CancellationToken cancellationToken = default);

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

		public Task DeleteQueueAsync(string queue, CancellationToken cancellationToken = default);

		public Task DeleteSubScriptionAsync(string topic, string subscription,
		                                    CancellationToken cancellationToken = default);

		public Task DeleteTopicAsync(string topic, CancellationToken cancellationToken = default);

		public Task<long> CountQueueAsync(string queue, CancellationToken cancellationToken = default);

		public Task<long> CountSubScriptionAsync(string topic, string subscription,
		                                         CancellationToken cancellationToken = default);

		public Task<long> CountTopicAsync(string topic, CancellationToken cancellationToken = default);
	}
}
