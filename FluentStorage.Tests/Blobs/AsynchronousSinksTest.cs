using FluentStorage.Blobs;

namespace FluentStorage.Tests.Blobs.Sink {
	public abstract class AsynchronousSinksTest {
		protected readonly IBlobStorage _storage;

		protected AsynchronousSinksTest(IBlobStorage storage) {
			_storage = storage;
		}
	}
}