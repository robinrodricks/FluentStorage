using System;
using FluentStorage.ConnectionStrings;
using FluentStorage.Storage;

namespace FluentStorage;

/// <summary>
/// Helper syntax for creating instances of storage library objects
/// </summary>
public static class StorageFactory {

	/// <summary>
	/// Call to initialise a module
	/// </summary>
	public static void Use(IExternalModule module) {
		if (module == null) {
			throw new ArgumentNullException(nameof(module));
		}

		IConnectionFactory connectionFactory = module.ConnectionFactory;
		if (connectionFactory != null) {
			ConnectionStringFactory.Register(connectionFactory);
		}

	}

	/// <summary>
	/// Creates a IStore instance from a connection string.
	/// </summary>
	public static IStore FromConnectionString(string connectionString) {
		return ConnectionStringFactory.CreateBlobStorage(connectionString);
	}

	/// <summary>
	/// Creates an instance in a specific disk directory
	/// <param name="directoryFullName">Root directory</param>
	/// </summary>
	public static IStore Disk(string directoryFullName) {
		return new DiskStore(directoryFullName);
	}

	/// <summary>
	/// Creates an instance of blob storage which stores everyting in memory. Useful for testing purposes only or if blobs don't
	/// take much space.
	/// </summary>
	/// <returns>In-memory blob storage instance</returns>
	public static IStore InMemory() {
		return new MemoryStore();
	}

}