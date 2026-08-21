using System;
using FluentStorage.Azure.Queues;
using FluentStorage.Azure.Queues.Messenger;
using FluentStorage.ConnectionStrings;
using FluentStorage.Queue;

namespace FluentStorage;

public static class AzureQueueStorage {
	/// <summary>
	/// Enable Azure Queue connection string support.
	/// </summary>
	public static void Use() {
		StorageFactory.Use(new Module());
	}

	/// <summary>
	/// Creates an instance of a publisher to Azure Storage Queues
	/// </summary>
	/// <param name="accountName">Account name. Must not be <see langword="null"/> or empty.</param>
	/// <param name="storageKey">Storage key. Must not be <see langword="null"/> or empty.</param>
	/// <param name="serviceUri">Alternative service uri. Pass <see langword="null"/> for default.</param>
	/// <returns>Generic message publisher interface</returns>
	public static IQueue FromCredentials(
		string accountName,
		string storageKey,
		Uri serviceUri = null) {
		if (serviceUri == null)
			return new AzureQueueMessenger(accountName, storageKey);

		return new AzureQueueMessenger(accountName, storageKey, serviceUri);
	}

	/// <summary>
	/// Create a new connection string to connect to Azure Queue
	/// </summary>
	public static ConnectionString CreateConnectionStringFromSharedKey(
		string accountName,
		string accountKey) {
		var cs = new ConnectionString(ConnectionStringPrefix.AzureQueueStorage);
		cs.Parameters[ConnectionStringParam.AccountName] = accountName;
		cs.Parameters[ConnectionStringParam.KeyOrPassword] = accountKey;
		return cs;
	}
}