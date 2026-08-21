using FluentStorage.ConnectionStrings;
using FluentStorage.Queue;
using FluentStorage.Queue.Files;

namespace FluentStorage;

public static class QueueFactory {

	/// <summary>
	/// Creates message publisher
	/// </summary>
	public static IQueue FromConnectionString(string connectionString) {
		return ConnectionStringFactory.CreateMessager(connectionString);
	}

	/// <summary>
	/// Creates a message publisher that uses local disk directory as a backing store
	/// </summary>
	/// <param name="factory"></param>
	/// <param name="path">Path to directory to use as a backing store. If it doesn't exist, it will be created.</param>
	/// <returns></returns>
	public static IQueue Disk(string path) {
		return new LocalDiskMessenger(path);
	}


	/// <summary>
	/// Creates a message publisher which holds messages in memory.
	/// </summary>
	/// <param name="factory"></param>
	/// <param name="name">Memory buffer name. Publishers with the same name will contain identical messages. Querying a publisher again
	/// with the same name returns an identical publisher. To create a receiver for this memory bufffer use the same name.</param>
	public static IQueue InMemory(string name) {
		return MemoryMessenger.CreateOrGet(name);
	}


}