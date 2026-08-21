using Amazon;

namespace FluentStorage.Tests.Integration.Queue;

public class AzureStorageQueueFixture : MessagingFixture {
	protected override IQueue CreateMessenger(TestConfig settings) =>
		AzureQueueStorage.FromCredentials(settings.AzureStorageName, settings.AzureStorageKey);
}

public class AzureStorageQueueTest : MessagingTest, IClassFixture<AzureStorageQueueFixture> {
	public AzureStorageQueueTest(AzureStorageQueueFixture fixture) : base(fixture) {
	}
}



public class InMemoryFixture : MessagingFixture {
	protected override IQueue CreateMessenger(TestConfig settings) {
		return QueueFactory.InMemory("test");
	}
}

public class InMemoryTest : MessagingTest, IClassFixture<InMemoryFixture> {
	public InMemoryTest(InMemoryFixture fixture) : base(fixture) {
	}
}



public class DiskFixture : MessagingFixture {
	protected override IQueue CreateMessenger(TestConfig settings) {
		return QueueFactory.Disk(_testDir);
	}
}

public class DiskTest : MessagingTest, IClassFixture<DiskFixture> {
	public DiskTest(DiskFixture fixture) : base(fixture) {
	}
}



public class AwsSQSFixture : MessagingFixture {
	protected override IQueue CreateMessenger(TestConfig settings) {
		return AwsSqsStorage.PublisherFromCredentials(
			settings.AwsAccessKey,
			settings.AwsSecretKey,
			"https://sqs.us-east-1.amazonaws.com",
			RegionEndpoint.USEast1);
	}
}


public class AwsSQSTest : MessagingTest, IClassFixture<AwsSQSFixture> {
	public AwsSQSTest(AwsSQSFixture fixture) : base(fixture) {
	}
}


public class AzureServiceBusFixture : MessagingFixture {
	protected override IQueue CreateMessenger(TestConfig settings) {
		return AzureServiceBus.FromConnectionString(settings.AzureServiceBusConnectionString);

	}
}

public class AzureServiceBusTopicTest : MessagingTest, IClassFixture<AzureServiceBusFixture> {
	public AzureServiceBusTopicTest(AzureServiceBusFixture fixture) : base(fixture, "t/", "t/fxtopic", receiveChannelSuffix: "/default") {
	}
}

public class AzureServiceBusSubscriptionTest : MessagingTest, IClassFixture<AzureServiceBusFixture> {
	public AzureServiceBusSubscriptionTest(AzureServiceBusFixture fixture) : base(fixture, "t/", "t/fxtopic/fxsubscription", receiveChannelSuffix: "/default") {
	}
}


public class AzureServiceBusQueueTest : MessagingTest, IClassFixture<AzureServiceBusFixture> {
	public AzureServiceBusQueueTest(AzureServiceBusFixture fixture) : base(fixture, "q/", "q/fxqueue") {
	}
}