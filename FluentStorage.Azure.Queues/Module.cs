﻿using FluentStorage.Blobs;
using FluentStorage.ConnectionString;
using FluentStorage.Messaging;

namespace FluentStorage.Azure.Queues {
	class Module : IExternalModule, IConnectionFactory {
		public IConnectionFactory ConnectionFactory => this;

		public IBlobStorage CreateBlobStorage(StorageConnectionString connectionString) => null;

		public IMessenger CreateMessenger(StorageConnectionString connectionString) {
			if (connectionString.Prefix == KnownPrefix.AzureQueueStorage) {
				connectionString.GetRequired(KnownParameter.AccountName, true, out string accountName);
				connectionString.GetRequired(KnownParameter.KeyOrPassword, true, out string key);

				return new AzureStorageQueueMessenger(accountName, key);
			}

			return null;
		}

	}
}
