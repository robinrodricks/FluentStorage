using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentStorage.Storage;

namespace FluentStorage.Queue.Large;

class LargeMessageContentMessageReceiver : IQueueReceiver {
	private readonly IQueueReceiver _parentReceiver;
	private readonly IStore _offloadStorage;

	public LargeMessageContentMessageReceiver(IQueueReceiver parentReceiver, IStore offloadStorage) {
		_parentReceiver = parentReceiver;
		_offloadStorage = offloadStorage;
	}

	public async Task ConfirmMessagesAsync(List<QueueMessage> messages, CancellationToken cancellationToken = default) {
		await _parentReceiver.ConfirmMessagesAsync(messages, cancellationToken).ConfigureAwait(false);

		foreach (QueueMessage message in messages) {
			await DeleteBlobAsync(message).ConfigureAwait(false);
		}
	}

	public async Task DeadLetterMessage(QueueMessage message, string reason, string errorDescription, CancellationToken cancellationToken = default) {
		await _parentReceiver.DeadLetterMessage(message, reason, errorDescription, cancellationToken).ConfigureAwait(false);

		await DeleteBlobAsync(message).ConfigureAwait(false);
	}

	private async Task DeleteBlobAsync(QueueMessage message) {
		if (!message.Properties.TryGetValue(QueueMessage.LargeMessageContentHeaderName, out string fileId)) return;

		message.Properties.Remove(QueueMessage.LargeMessageContentHeaderName);

		await _offloadStorage.DeleteObject(fileId).ConfigureAwait(false);
	}

	public void Dispose() {
		_parentReceiver.Dispose();
	}

	public Task<int> GetMessageCountAsync() => _parentReceiver.GetMessageCountAsync();

	public Task StartMessagePump(Func<List<QueueMessage>, CancellationToken, Task> onMessageAsync, int maxBatchSize = 1, CancellationToken cancellationToken = default) {
		return _parentReceiver.StartMessagePump(
			(mms, ct) => DownloadingMessagePumpAsync(mms, onMessageAsync, ct),
			maxBatchSize, cancellationToken);
	}

	private async Task DownloadingMessagePumpAsync(List<QueueMessage> messages,
		Func<List<QueueMessage>, CancellationToken, Task> onParentMessagesAsync,
		CancellationToken cancellationToken = default) {
		//process messages to download external content
		foreach (QueueMessage message in messages) {
			if (!message.Properties.TryGetValue(QueueMessage.LargeMessageContentHeaderName, out string fileId)) continue;

			message.Content = await _offloadStorage.GetBytes(fileId, cancellationToken).ConfigureAwait(false);
		}

		//now that messages are augmented pass them to parent
		await onParentMessagesAsync(messages, cancellationToken).ConfigureAwait(false);
	}

	public Task KeepAlive(QueueMessage message, TimeSpan? timeToLive = null, CancellationToken cancellationToken = default) =>
		_parentReceiver.KeepAlive(message, timeToLive, cancellationToken);
	public Task<List<QueueMessage>> PeekMessages(int maxMessages, CancellationToken cancellationToken = default) => throw new NotSupportedException();
}