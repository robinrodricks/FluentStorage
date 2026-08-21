using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FluentStorage.Queue;

/// <summary>
/// Interface to manage a single queue across various cloud providers.
/// </summary>
public interface IQueue : IDisposable {

	/// <summary>
	/// Create one or more channels
	/// </summary>
	Task CreateChannels(IEnumerable<string> channelNames, CancellationToken cancellationToken = default);

	/// <summary>
	/// List available channels
	/// </summary>
	Task<List<string>> ListChannels(CancellationToken cancellationToken = default);

	/// <summary>
	/// Physically deletes channels
	/// </summary>
	Task DeleteChannels(IEnumerable<string> channelNames, CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets message count in a channel.
	/// </summary>
	Task<long> GetMessageCount(string channelName, CancellationToken cancellationToken = default);

	/// <summary>
	/// Send messages to a channel
	/// </summary>
	Task SendMessages(string channelName, IEnumerable<QueueMessage> messages, CancellationToken cancellationToken = default);

	/// <summary>
	/// Receive messages from a channel
	/// </summary>
	Task<List<QueueMessage>> ReceiveMessages(
		string channelName,
		int count = 100,
		TimeSpan? visibility = null,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Peek messages in a channel
	/// </summary>
	Task<List<QueueMessage>> PeekMessages(
		string channelName,
		int count = 100,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Deletes messages from the channel
	/// </summary>
	Task DeleteMessages(
		string channelName,
		IEnumerable<QueueMessage> messages,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Starts message processor which listens for new messages asynchronously and passes to the processing host.
	/// </summary>
	/// <param name="channelName">Name of the channel</param>
	/// <param name="messageProcessor">Message processor implementation</param>
	Task StartMessageProcessor(string channelName, IQueueProcessor messageProcessor);
}