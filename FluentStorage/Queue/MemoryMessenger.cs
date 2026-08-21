using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FluentStorage.Queue;

class MemoryMessenger : IQueue {
	private static readonly ConcurrentDictionary<string, MemoryMessenger> _nameToMessenger =
		new ConcurrentDictionary<string, MemoryMessenger>();

	private readonly ConcurrentDictionary<string, ConcurrentQueue<QueueMessage>> _queues =
		new ConcurrentDictionary<string, ConcurrentQueue<QueueMessage>>();


	public Task CreateChannels(IEnumerable<string> channelNames, CancellationToken cancellationToken = default) {
		foreach (string channelName in channelNames) {
			_queues[channelName] = new ConcurrentQueue<QueueMessage>();
		}

		return Task.CompletedTask;
	}

	public Task<long> GetMessageCount(string channelName, CancellationToken cancellationToken = default) {
		if (channelName is null)
			throw new ArgumentNullException(nameof(channelName));

		ConcurrentQueue<QueueMessage> queue = GetQueue(channelName);
		return Task.FromResult((long)queue.Count);
	}

	public Task<List<string>> ListChannels(CancellationToken cancellationToken = default) {
		return Task.FromResult<List<string>>(_queues.Select(q => q.Key).ToList());
	}

	public Task DeleteChannels(IEnumerable<string> channelNames, CancellationToken cancellationToken = default) {
		if (channelNames is null)
			throw new ArgumentNullException(nameof(channelNames));

		foreach (string cn in channelNames) {
			_queues.TryRemove(cn, out ConcurrentQueue<QueueMessage> v);
		}

		return Task.CompletedTask;
	}

	public Task<List<QueueMessage>> PeekMessages(string channelName, int count = 100, CancellationToken cancellationToken = default) {
		if (channelName is null)
			throw new ArgumentNullException(nameof(channelName));

		return Task.FromResult<List<QueueMessage>>(GetMessages(channelName, count, true, null));
	}

	public Task<List<QueueMessage>> ReceiveMessages(
		string channelName, int count = 100, TimeSpan? visibility = null, CancellationToken cancellationToken = default) {
		if (channelName is null)
			throw new ArgumentNullException(nameof(channelName));

		return Task.FromResult<List<QueueMessage>>(GetMessages(channelName, count, false, visibility));
	}

	private List<QueueMessage> GetMessages(string channelName, int count, bool peekOnly, TimeSpan? visibility) {
		var result = new List<QueueMessage>();
		ConcurrentQueue<QueueMessage> queue = GetQueue(channelName);

		DateTime now = DateTime.UtcNow;
		DateTimeOffset nextVisible = now + (visibility ?? TimeSpan.FromMinutes(1));

		while (result.Count < count) {
			if (!queue.TryDequeue(out QueueMessage msg))
				break;

			bool isVisible = msg.NextVisibleTime == null || (msg.NextVisibleTime.Value >= now);

			if (isVisible) {
				result.Add(msg);

				msg.NextVisibleTime = nextVisible;

				if (peekOnly) {
					queue.Enqueue(msg);
				}
			}
			else {
				queue.Enqueue(msg);
			}
		}

		return result;

	}

	public Task SendMessages(string channelName, IEnumerable<QueueMessage> messages, CancellationToken cancellationToken = default) {
		if (channelName is null)
			throw new ArgumentNullException(nameof(channelName));

		if (messages is null)
			throw new ArgumentNullException(nameof(messages));

		ConcurrentQueue<QueueMessage> queue = GetQueue(channelName);
		foreach (QueueMessage qm in messages) {
			queue.Enqueue(qm);
		}
		return Task.CompletedTask;
	}

	public void Dispose() {

	}



	private ConcurrentQueue<QueueMessage> GetQueue(string channelName) {
		return _queues.GetOrAdd(channelName, new ConcurrentQueue<QueueMessage>());
	}

	public static IQueue CreateOrGet(string name) {
		if (_nameToMessenger.TryGetValue(name, out MemoryMessenger messenger))
			return messenger;

		messenger = new MemoryMessenger();
		_nameToMessenger[name] = messenger;
		return messenger;
	}

	public Task DeleteMessages(string channelName, IEnumerable<QueueMessage> messages, CancellationToken cancellationToken = default) => throw new NotImplementedException();
	public Task StartMessageProcessor(string channelName, IQueueProcessor messageProcessor) => throw new NotImplementedException();
}