using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FluentStorage.Messaging.Files {
	/// <summary>
	/// Messages themselves can be human readable. THe speed is not an issue because the main bottleneck is disk anyway.
	/// </summary>
	class LocalDiskMessenger : IMessenger {
		private const string FileExtension = ".snm";

		private readonly string _root;
#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
		private readonly IDictionary<string, (FileSystemWatcher Watcher, FileSystemEventHandler handler, ISet<IMessageProcessor> Processors)> _watchers;
#endif

		public LocalDiskMessenger(string directoryPath) {
			_root = directoryPath ?? throw new ArgumentNullException(nameof(directoryPath));
#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
			_watchers = new ConcurrentDictionary<string, (FileSystemWatcher Watcher, FileSystemEventHandler handler, ISet<IMessageProcessor> Processors)>();
#endif
		}

		private static string GenerateDiskId() {
			DateTime now = DateTime.UtcNow;

			//generate sortable file name, so that we can get the oldest item or newest item easily
			return now.ToString("yyyy-MM-dd-hh-mm-ss-ffff") + FileExtension;
		}

		private static QueueMessage ToQueueMessage(FileInfo fi) {
			byte[] content = File.ReadAllBytes(fi.FullName);

			var result = QueueMessage.FromByteArray(content);
			result.Id = Path.GetFileNameWithoutExtension(fi.Name);
			return result;
		}

		private IReadOnlyCollection<FileInfo> GetMessageFiles(string channelName) {
			string directoryPath = Path.Combine(_root, channelName);

			if (!Directory.Exists(directoryPath))
				return new List<FileInfo>();

			return new DirectoryInfo(directoryPath).GetFiles("*" + FileExtension, SearchOption.TopDirectoryOnly);
		}

		private string GetMessagePath(string channelName) {
			string dir = GetChannelPath(channelName);
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			return Path.Combine(dir, GenerateDiskId());

		}
		private string GetChannelPath(string channelName) => Path.Combine(_root, channelName);

		#region [ IMessenger ]

		public Task CreateChannelsAsync(IEnumerable<string> channelNames, CancellationToken cancellation = default) {
			foreach (string channelName in channelNames) {
				string path = Path.Combine(_root, channelName);
				if (!Directory.Exists(path))
					Directory.CreateDirectory(path);
			}

			return Task.CompletedTask;
		}


		public Task<IReadOnlyCollection<string>> ListChannelsAsync(CancellationToken cancellationToken = default) {
			return Task.FromResult<IReadOnlyCollection<string>>(new DirectoryInfo(_root).GetDirectories().Select(d => d.Name).ToList());
		}

		public Task DeleteChannelsAsync(IEnumerable<string> channelNames, CancellationToken cancellationToken = default) {
			if (channelNames is null)
				throw new ArgumentNullException(nameof(channelNames));

			foreach (string channelName in channelNames) {
				string dir = Path.Combine(_root, channelName);
				if (Directory.Exists(dir))
					Directory.Delete(dir, true);

#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
				if (_watchers.TryGetValue(channelName, out (FileSystemWatcher Watcher, FileSystemEventHandler Handler, ISet<IMessageProcessor> Processors) watcherAndProcessor)) {
					watcherAndProcessor.Watcher.Created -= watcherAndProcessor.Handler;
					watcherAndProcessor.Watcher.Dispose();
				}

				_watchers.Remove(channelName);
#endif
			}

			return Task.CompletedTask;
		}

		public Task<long> GetMessageCountAsync(string channelName, CancellationToken cancellationToken = default) {
			return Task.FromResult<long>(GetMessageFiles(channelName).Count);
		}

		public Task SendAsync(string channelName, IEnumerable<QueueMessage> messages, CancellationToken cancellationToken = default) {
			if (channelName is null)
				throw new ArgumentNullException(nameof(channelName));

			if (messages is null)
				throw new ArgumentNullException(nameof(messages));

			foreach (QueueMessage msg in messages) {
				if (msg == null)
					throw new ArgumentNullException(nameof(msg));

				string filePath = GetMessagePath(channelName);

				File.WriteAllBytes(filePath, msg.ToByteArray());
			}

			return Task.FromResult(true);
		}

		public Task<IReadOnlyCollection<QueueMessage>> ReceiveAsync(
		   string channelName,
		   int count = 100,
		   TimeSpan? visibility = null,
		   CancellationToken cancellationToken = default) {
			return Task.FromResult(GetMessages(channelName, count));
		}

		public Task<IReadOnlyCollection<QueueMessage>> PeekAsync(string channelName, int count = 100, CancellationToken cancellationToken = default) {
			return Task.FromResult(GetMessages(channelName, count));
		}

		public void Dispose() {

#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
			foreach ((_, (FileSystemWatcher watcher, FileSystemEventHandler handler, ISet<IMessageProcessor> processors)) in _watchers) {

				watcher.Created -= handler;
				watcher?.Dispose();
			}
#endif
		}

		public Task DeleteAsync(string channelName, IEnumerable<QueueMessage> messages, CancellationToken cancellationToken = default) => throw new NotImplementedException();

		#endregion

		private IReadOnlyCollection<QueueMessage> GetMessages(
		   string channelName,
		   int count) {
			//get all files (not efficient, but we hope there won't be many)
			IReadOnlyCollection<FileInfo> files = GetMessageFiles(channelName);

			//sort files so that oldest appear first, take max and return
			return files
			   .OrderBy(f => f.Name)
			   .Take(count)
			   .Select(ToQueueMessage)
			   .ToList();
		}

		///<inheritdoc/>
		public Task StartMessageProcessorAsync(string channelName, IMessageProcessor messageProcessor) {

#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER

			if (string.IsNullOrWhiteSpace(channelName)) {
				throw new ArgumentNullException(nameof(channelName), $"'{nameof(channelName)}' cannot be null or whitespace");
			}

			if (messageProcessor is null) {
				throw new ArgumentNullException(nameof(messageProcessor), $"'{nameof(messageProcessor)}' cannot be null");
			}

			HashSet<IMessageProcessor> messageProcessors = new();

			if (_watchers.TryGetValue(channelName, out (FileSystemWatcher Watcher, FileSystemEventHandler Handler, ISet<IMessageProcessor> Processors) watcherAndProcessor)) {

				watcherAndProcessor.Watcher.Changed -= watcherAndProcessor.Handler; // avoid memory leaks
				watcherAndProcessor.Watcher.Dispose();
				_watchers.Remove(channelName);

				messageProcessors.AddRange(watcherAndProcessor.Processors);
			}

			if (messageProcessors.Add(messageProcessor)) {
				FileSystemWatcher watcher = new(GetChannelPath(channelName), $"*{FileExtension}");
				FileSystemEventHandler fileSystemEventHandler = (sender, args) => {
					if (args.ChangeType == WatcherChangeTypes.Changed) {
						FileInfo messageFileInfo = new(args.FullPath);

						QueueMessage queueMessage = ToQueueMessage(messageFileInfo);

						foreach (IMessageProcessor item in messageProcessors) {
							try {
								item.ProcessMessagesAsync(new[] { queueMessage }).GetAwaiter().GetResult();
							}
							catch (Exception) {
								// swalllow the exception has no caller can could be notified anyway
							}
						}
					}
				};

				watcher.Changed += fileSystemEventHandler;
				watcher.EnableRaisingEvents = true;

				_watchers.Add(channelName, (watcher, fileSystemEventHandler, messageProcessors));
			}

			return Task.CompletedTask;
#else
			throw new NotImplementedException();
#endif

		}

		private void FileSystemErrorHandler(object sender, ErrorEventArgs args) {

			throw new StorageException("An expected error occurred", args.GetException());
		}
	}
}