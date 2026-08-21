using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentStorage.Storage;

namespace FluentStorage.Queue.Large;

class LargeMessageMessenger : IQueue {
	private readonly IQueue _parentPublisher;
	private readonly IStore _offloadStorage;
	private readonly long _minSizeLarge;
	private readonly bool _keepPublisherOpen;
	private readonly Func<QueueMessage, string> _blobPathGenerator;

	public LargeMessageMessenger(
		IQueue parentPublisher,
		IStore offloadStorage,
		long minSizeLarge,
		Func<QueueMessage, string> blobPathGenerator = null,
		bool keepPublisherOpen = false) {
		_parentPublisher = parentPublisher ?? throw new ArgumentNullException(nameof(parentPublisher));
		_offloadStorage = offloadStorage ?? throw new ArgumentNullException(nameof(offloadStorage));
		_minSizeLarge = minSizeLarge;
		_blobPathGenerator = blobPathGenerator ?? GenerateBlobPath;
		_keepPublisherOpen = keepPublisherOpen;
	}

	private void AddBlobId(QueueMessage message, out string id) {
		id = _blobPathGenerator(message);

		message.Properties[QueueMessage.LargeMessageContentHeaderName] = id;
	}

	private string GenerateBlobPath(QueueMessage message) {
		return StoragePath.Combine("message", Guid.NewGuid().ToString());
	}

	private async Task SendAsync(string channelName, QueueMessage message, CancellationToken cancellationToken = default) {
		if (message?.Content.Length > _minSizeLarge) {
			AddBlobId(message, out string id);

			await _offloadStorage.SetBytes(id, message.Content, false, cancellationToken).ConfigureAwait(false);

			message.Content = null; //delete content
		}

		await _parentPublisher.SendMessages(channelName, new[] { message }).ConfigureAwait(false);
	}



	public Task<List<string>> ListChannels(CancellationToken cancellationToken = default) => throw new NotImplementedException();
	public Task<long> GetMessageCount(string channelName, CancellationToken cancellationToken = default) => throw new NotImplementedException();
	public async Task SendMessages(string channelName, IEnumerable<QueueMessage> messages, CancellationToken cancellationToken = default) {
		await Task.WhenAll(messages.Select(m => SendAsync(channelName, m, cancellationToken))).ConfigureAwait(false);
	}

	public Task<List<QueueMessage>> ReceiveMessages(string channelName, int count = 100, TimeSpan? visibility = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
	public Task<List<QueueMessage>> PeekMessages(string channelName, int count = 100, CancellationToken cancellationToken = default) => throw new NotImplementedException();

	public void Dispose() {
		if (!_keepPublisherOpen) {
			_parentPublisher.Dispose();
		}
	}

	public Task DeleteChannels(IEnumerable<string> channelNames, CancellationToken cancellationToken = default) => throw new NotImplementedException();
	public Task CreateChannels(IEnumerable<string> channelNames, CancellationToken cancellationToken = default) => throw new NotImplementedException();
	public Task DeleteMessages(string channelName, IEnumerable<QueueMessage> messages, CancellationToken cancellationToken = default) => throw new NotImplementedException();
	public Task StartMessageProcessor(string channelName, IQueueProcessor messageProcessor) => throw new NotImplementedException();

}