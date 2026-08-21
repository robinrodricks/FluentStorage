namespace FluentStorage.Tests.Integration.Storage;

public class MemoryFixture : StoreFixture {
	protected override IStore CreateStorage(TestConfig settings) {
		return StorageFactory.InMemory();
	}
}

public class MemoryTest : IStoreTest, IClassFixture<MemoryFixture> {
	public MemoryTest(MemoryFixture fixture) : base(fixture) {
	}
}