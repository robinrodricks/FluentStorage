namespace FluentStorage.Tests.Unit.Queue;

public class LargeMessagePublishingTest {
	private readonly IStore _blobStorage;
	private readonly IQueue _publisher;

	public LargeMessagePublishingTest() {
		_blobStorage = StorageFactory.InMemory();

		_publisher = QueueFactory
			.InMemory(nameof(LargeMessagePublishingTest))
			.HandleLargeContent(_blobStorage, 100);
	}

	[Fact]
	public async Task SendMessage_Small_AllContent() {
		var smallMessage = new QueueMessage(RandomGenerator.GetRandomBytes(50, 50));

		//send small message
		await _publisher.SendMessage("test", smallMessage);
		int blobCount = (await (_blobStorage as StoreBase).ListFileObjects(new StorageListOptions { Recurse = true })).Count;

		//validate that small message was never uploaded
		Assert.Equal(0, blobCount);

		//validate that message does not have
		Assert.False(smallMessage.Properties.ContainsKey(QueueMessage.LargeMessageContentHeaderName));
	}

	[Fact]
	public async Task SendMessage_Large_NoContentAndUploadedAndHasId() {
		var largeMessage = new QueueMessage(RandomGenerator.GetRandomBytes(150, 150));

		//send large message
		await _publisher.SendMessage("test", largeMessage);
		int blobCount = (await (_blobStorage as StoreBase).ListFileObjects(new StorageListOptions { Recurse = true })).Count;

		//validate that small message was uploaded once
		Assert.Equal(1, blobCount);

		//validate that message has offload header
		Assert.True(largeMessage.Properties.ContainsKey(QueueMessage.LargeMessageContentHeaderName));
	}
}