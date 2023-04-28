using FluentAssertions;

using FluentStorage.Messaging;

using NetBox.Extensions;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Xunit;

namespace FluentStorage.Tests.Messaging {


	public class LocalDiskMessagingTest : IAsyncLifetime {
		private readonly IMessenger _sut;
		private readonly string _path;
		private readonly StoreEventMessageProcessor _messageProcessor;

		public LocalDiskMessagingTest() {
			_path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			_messageProcessor = new StoreEventMessageProcessor();
			_sut = StorageFactory.Messages.Disk(_path);
		}

		///<inheritdoc/>
		public Task DisposeAsync() {
			_sut.Dispose();

			return Task.CompletedTask;
		}

		///<inheritdoc/>
		public Task InitializeAsync() {

			try {
				Directory.Delete(_path);
			}
			catch (Exception) {
			}

			return Task.CompletedTask;
		}

		[Fact]
		public void Should_throw_ArgumentNullException_when_channelName_is_null() {
			// Assert
			IMessageProcessor messageProcessor = new StoreEventMessageProcessor();

			// Act
			Func<Task> startingMessageProcessorWhenChannelNameIsNull = async () => await _sut.StartMessageProcessorAsync(null, messageProcessor)
																							 .ConfigureAwait(false);

			// Assert
			startingMessageProcessorWhenChannelNameIsNull.Should()
														 .ThrowExactlyAsync<ArgumentNullException>("channelName cannot be null")
														 .Where(ex => !string.IsNullOrWhiteSpace(ex.ParamName));
		}

		[Fact]
		public void Should_throw_ArgumentNullException_when_messageProcessor_is_null() {
			// Assert
			string channelName = Guid.NewGuid().ToString();

			// Act
			Func<Task> startingMessageProcessorWhenChannelNameIsNull = async () => await _sut.StartMessageProcessorAsync(channelName, null)
																							 .ConfigureAwait(false);

			// Assert
			startingMessageProcessorWhenChannelNameIsNull.Should()
														 .ThrowExactlyAsync<ArgumentNullException>("message processor cannot be null")
														 .Where(ex => !string.IsNullOrWhiteSpace(ex.ParamName));
		}

		[Fact]
		public async Task Should_throw_when_channel_does_not_exist() {
			// Arrange
			string channelName = Guid.NewGuid().ToString();

			// Act
			Func<Task> startingMessageProcessorWhenChannelDoesNotExist = async () => await _sut.StartMessageProcessorAsync(channelName, _messageProcessor)
																							  .ConfigureAwait(false);

			// Assert
			await startingMessageProcessorWhenChannelDoesNotExist.Should().ThrowAsync<StorageException>("the specified channel does not exist");
		}
	}


	public class StoreEventMessageProcessor : IMessageProcessor {

		public IReadOnlyCollection<QueueMessage> Messages => _messages.ToImmutableArray();

		private readonly IList<QueueMessage> _messages;

		public StoreEventMessageProcessor() {
			_messages = new List<QueueMessage>();
		}

		///<inheritdoc/>
		public Task ProcessMessagesAsync(IReadOnlyCollection<QueueMessage> messages) {
			_messages.AddRange(messages);

			return Task.CompletedTask;
		}
	}
}
