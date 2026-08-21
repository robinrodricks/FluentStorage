using FluentStorage.Queue;
using FluentStorage.Queue.Files;
using FluentStorage.Storage;

namespace FluentStorage.ConnectionStrings;

class BuiltInConnectionFactory : IConnectionFactory {
	public IStore CreateStore(ConnectionString connectionString) {
		if (connectionString.Prefix == "disk") {
			connectionString.GetRequired("path", true, out string path);

			return new DiskStore(path);
		}

		if (connectionString.Prefix == "inmemory") {
			return new MemoryStore();
		}

		return null;
	}

	public IQueue CreateQueue(ConnectionString connectionString) {
		if (connectionString.Prefix == "inmemory") {
			connectionString.GetRequired("name", true, out string name);

			return MemoryMessenger.CreateOrGet(name);
		}

		if (connectionString.Prefix == "disk") {
			connectionString.GetRequired("path", true, out string path);

			return new LocalDiskMessenger(path);
		}

		return null;
	}
}