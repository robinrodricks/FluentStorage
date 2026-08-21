namespace FluentStorage.Tests.Unit.Queue;

public class LocalDiskMessagingTest : IAsyncLifetime {
	private readonly IQueue _sut;
	private readonly string _path;

	public LocalDiskMessagingTest() {
		_path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
		_sut = QueueFactory.Disk(_path);
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
		IQueueProcessor messageProcessor = Mock.Of<IQueueProcessor>();

		// Act
		Func<Task> startingMessageProcessorWhenChannelNameIsNull = async () => await _sut.StartMessageProcessor(null, messageProcessor)
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
		Func<Task> startingMessageProcessorWhenChannelNameIsNull = async () => await _sut.StartMessageProcessor(channelName, null)
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
		Func<Task> startingMessageProcessorWhenChannelDoesNotExist = async () => await _sut.StartMessageProcessor(channelName, Mock.Of<IQueueProcessor>())
			.ConfigureAwait(false);

		// Assert
		await startingMessageProcessorWhenChannelDoesNotExist.Should().ThrowAsync<Exception>("the specified channel does not exist");
	}

	[Fact]
	public async Task Given_one_processor_process_a_channel_When_an_event_is_sent_in_that_channel_Then_the_process_should_receive_the_event() {
		// Arrange
		string channelName = Guid.NewGuid().ToString();
		QueueMessage message = QueueMessage.FromText(Guid.NewGuid().ToString());

		await _sut.CreateChannel(channelName).ConfigureAwait(false);

		Mock<IQueueProcessor> messageProcessorMock = new();

		// Act
		await _sut.StartMessageProcessor(channelName, messageProcessorMock.Object).ConfigureAwait(false);
		await _sut.SendMessage(channelName, message).ConfigureAwait(false);

		// Assert
		messageProcessorMock.Verify(mock => mock.ProcessMessages(It.IsAny<List<QueueMessage>>()), Times.Once);
		messageProcessorMock.Verify(mock => mock.ProcessMessages(It.Is<List<QueueMessage>>(messages => messages.Count == 1
				&& messages.ElementAt(0).StringContent == message.StringContent)),
			Times.Once);

	}

	[Fact]
	public async Task Given_many_processors_process_a_channel_When_an_event_is_sent_in_that_channel_Then_all_processors_should_receive_the_event() {
		// Arrange
		string channelName = Guid.NewGuid().ToString();
		QueueMessage message = QueueMessage.FromText(Guid.NewGuid().ToString());

		await _sut.CreateChannel(channelName).ConfigureAwait(false);

		MockRepository mockRepository= new MockRepository(MockBehavior.Loose);

		Mock<IQueueProcessor> firstProcessorMock = mockRepository.Create<IQueueProcessor>();
		Mock<IQueueProcessor> secondProcessorMock = mockRepository.Create<IQueueProcessor>();
		Mock<IQueueProcessor> thirdProcessorMock = mockRepository.Create<IQueueProcessor>();

		// Act
		await _sut.StartMessageProcessor(channelName, firstProcessorMock.Object).ConfigureAwait(false);
		await _sut.StartMessageProcessor(channelName, secondProcessorMock.Object).ConfigureAwait(false);
		await _sut.StartMessageProcessor(channelName, thirdProcessorMock.Object).ConfigureAwait(false);

		await _sut.SendMessage(channelName, message).ConfigureAwait(false);

		// Assert
		firstProcessorMock.Verify(mock => mock.ProcessMessages(It.Is<List<QueueMessage>>(messages => messages.Count == 1
				&& messages.ElementAt(0).StringContent == message.StringContent)),
			Times.Once);
		firstProcessorMock.VerifyNoOtherCalls();

		secondProcessorMock.Verify(mock => mock.ProcessMessages(It.Is<List<QueueMessage>>(messages => messages.Count == 1
				&& messages.ElementAt(0).StringContent == message.StringContent)),
			Times.Once);
		secondProcessorMock.VerifyNoOtherCalls();

		thirdProcessorMock.Verify(mock => mock.ProcessMessages(It.Is<List<QueueMessage>>(messages => messages.Count == 1
				&& messages.ElementAt(0).StringContent == message.StringContent)),
			Times.Once);
		thirdProcessorMock.VerifyNoOtherCalls();



	}
}