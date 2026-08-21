namespace FluentStorage.Tests.Integration.Queue;

public abstract class MessagingTest : IAsyncLifetime {
	private readonly MessagingFixture _fixture;
	private readonly string _channelPrefix;
	private readonly string _receiveChannelSuffix;
	private readonly IQueue _msg;
	private readonly string _qn;

	protected MessagingTest(MessagingFixture fixture, string channelPrefix = null, string channelFixedName = null, string receiveChannelSuffix = null) {
		_fixture = fixture;
		_channelPrefix = channelPrefix;
		_qn = channelFixedName ?? NewChannelName();
		_receiveChannelSuffix = receiveChannelSuffix;
		_msg = fixture.Messenger;
	}

	public async Task InitializeAsync() {
		try {
			await _msg.CreateChannel(_qn);
		}
		catch (NotSupportedException) {

		}
	}

	private string NewChannelName() {
		return $"{_channelPrefix}{Guid.NewGuid().ToString()}";
	}

	public async Task DisposeAsync() {
		//clear up all the channels

		try {
			await _msg.DeleteChannel(_qn);
		}
		catch { }
	}

	[Fact]
	public async Task SendMessage_OneMessage_DoesntCrash() {
		var qm = QueueMessage.FromText("test");

		await _msg.SendMessage(_qn, qm);
	}

	[Fact]
	public async Task SendMessage_NullChannel_ArgumentException() {
		await Assert.ThrowsAsync<ArgumentNullException>(() => _msg.SendMessage(null, QueueMessage.FromText("test")));
	}

	[Fact]
	public async Task SendMessage_NullMessages_ArgumentException() {
		await Assert.ThrowsAsync<ArgumentNullException>(() => _msg.SendMessages(_qn, null));
	}

	[Fact]
	public async Task Channels_list_doesnt_crash() {
		await _msg.ListChannels();
	}

	[Fact]
	public async Task Channels_Create_list_contains_created_channel() {
		string channelName = NewChannelName();

		try {
			await _msg.CreateChannel(channelName);
		}
		catch (NotSupportedException) {
			return;
		}

		//some providers don't list channels immediately as they are eventually consistent

		const int maxRetries = 10;

		for (int i = 0; i < maxRetries; i++) {
			List<string> channels = await _msg.ListChannels();

			if (channels.Contains(channelName))
				return;

			await Task.Delay(TimeSpan.FromSeconds(5));
		}

		Assert.Fail($"channel not found after {maxRetries} retries.");
	}

	[Fact]
	public async Task Channels_delete_goesaway() {
		string channelName = NewChannelName();

		try {
			await _msg.CreateChannel(channelName);
		}
		catch (NotSupportedException) {
			return;
		}

		await _msg.DeleteChannel(channelName);

		List<string> channels = await _msg.ListChannels();

		Assert.DoesNotContain(channelName, channels);

	}

	[Fact]
	public async Task Channels_delete_null_list_argument_exception() {
		await Assert.ThrowsAsync<ArgumentNullException>(() => _msg.DeleteChannels(null));
	}

	[Fact]
	public async Task MessageCount_Send_One_Count_Changes() {
		long count1;

		try {
			count1 = await _msg.GetMessageCount(_qn + _receiveChannelSuffix);
		}
		catch (NotSupportedException) {
			return;
		}

		await _msg.SendMessage(_qn, QueueMessage.FromText("bla bla"));

		long count2 = await _msg.GetMessageCount(_qn + _receiveChannelSuffix);

		Assert.NotEqual(count1, count2);
	}

	[Fact]
	public async Task MessageCount_Null_ThrowsArgumentNull() {
		await Assert.ThrowsAsync<ArgumentNullException>(() => _msg.GetMessageCount(null));
	}

	[Fact]
	public async Task MessageCount_NonExistentQueue_Return0() {
		try {
			Assert.Equal(0, await _msg.GetMessageCount(NewChannelName() + _receiveChannelSuffix));
		}
		catch (NotSupportedException) {

		}
	}

	[Fact]
	public async Task Receive_SendOne_Received() {
		string tag = await SendAsync();

		try {
			List<QueueMessage> messages = await _msg.ReceiveMessages(_qn);

			Assert.Contains(messages, m => m.Properties.TryGetValue("tag", out string itag) && itag == tag);
		}
		catch (NotSupportedException) {

		}
	}

	[Fact]
	public async Task Peek_NullChannel_ThrowsArgumentNull() {
		await Assert.ThrowsAsync<ArgumentNullException>(() => _msg.PeekMessages(null));
	}

	[Fact]
	public async Task Peek_SendMessage_HasAtLeaseOne() {
		try {
			await SendAsync();

			List<QueueMessage> messages = await _msg.PeekMessages(_qn);

			Assert.NotEmpty(messages);
		}
		catch (NotSupportedException) {

		}
	}

	[Fact]
	public async Task Receive_NullChannel_ArgumentNullException() {
		await Assert.ThrowsAsync<ArgumentNullException>(() => _msg.ReceiveMessages(null));
	}

	private async Task<string> SendAsync() {
		string tag = Guid.NewGuid().ToString();

		var msg = QueueMessage.FromText("hm");
		msg.Properties["tag"] = tag;

		await _msg.SendMessage(_qn, msg);

		return tag;
	}
}