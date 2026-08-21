using System;
using System.Threading;
using System.Threading.Tasks;

namespace FluentStorage.Queue;

/// <summary>
/// Extensions for <see cref="IQueue"/>
/// </summary>
public static class MessengerExtensions {

	/// <summary>
	/// Create a single channel
	/// </summary>
	public static Task CreateChannel(this IQueue messenger, string channelName, CancellationToken cancellationToken = default) {
		return messenger.CreateChannels(new[] { channelName }, cancellationToken);
	}

	/// <summary>
	/// Puts a new message to the back of the queue.
	/// </summary>
	public static Task SendMessage(this IQueue messenger, string channelName, QueueMessage message, CancellationToken cancellationToken = default) {
		if (channelName is null)
			throw new ArgumentNullException(nameof(channelName));

		if (message == null)
			throw new ArgumentNullException(nameof(message));

		return messenger.SendMessages(channelName, new[] { message }, cancellationToken);
	}

	/// <summary>
	/// Deletes a single channel
	/// </summary>
	public static Task DeleteChannel(this IQueue messenger, string channelName, CancellationToken cancellationToken = default) {
		return messenger.DeleteChannels(new[] { channelName }, cancellationToken);
	}
}