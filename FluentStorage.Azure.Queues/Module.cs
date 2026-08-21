using FluentStorage.Azure.Queues.Messenger;
using FluentStorage.ConnectionStrings;
using FluentStorage.Queue;
using FluentStorage.Storage;

namespace FluentStorage.Azure.Queues;

class Module : IExternalModule, IConnectionFactory {
	public IConnectionFactory ConnectionFactory => this;

	public IStore CreateStore(ConnectionString connectionString) => null;

	public IQueue CreateQueue(ConnectionString connectionString) {
		if (connectionString.Prefix == ConnectionStringPrefix.AzureQueueStorage) {
			connectionString.GetRequired(ConnectionStringParam.AccountName, true, out string accountName);
			connectionString.GetRequired(ConnectionStringParam.KeyOrPassword, true, out string key);

			return new AzureQueueMessenger(accountName, key);
		}

		return null;
	}

}