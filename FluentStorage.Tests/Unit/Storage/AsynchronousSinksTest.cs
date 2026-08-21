namespace FluentStorage.Tests.Unit.Storage;

public abstract class AsynchronousSinksTest {
	protected readonly IStore _storage;

	protected AsynchronousSinksTest(IStore storage) {
		_storage = storage;
	}
}