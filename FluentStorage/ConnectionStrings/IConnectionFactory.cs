using FluentStorage.Queue;
using FluentStorage.Storage;

namespace FluentStorage.ConnectionStrings;

/// <summary>
/// Connection factory is responsible for creating storage instances from connection strings. It
/// is usually implemented by every external module, however is optional.
/// </summary>
public interface IConnectionFactory {
	/// <summary>
	/// Creates a IStore instance from a connection string if possible.
	/// If this factory does not support this connection string it returns null.
	/// </summary>
	IStore CreateStore(ConnectionString connectionString);

	/// <summary>
	/// Creates a message publisher
	/// </summary>
	IQueue CreateQueue(ConnectionString connectionString);
}