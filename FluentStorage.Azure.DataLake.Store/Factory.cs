﻿using FluentStorage.Blobs;
using FluentStorage.Azure.DataLake.Store;
using FluentStorage.Azure.DataLake;
using System;
using System.Net;

namespace FluentStorage {
	/// <summary>
	/// Factory class that implement factory methods for Microsoft Azure implememtations
	/// </summary>
	public static class Factory {
		/// <summary>
		/// Adds connection string support for Azure Data Lake Gen 1 and Gen 2
		/// </summary>
		/// <param name="factory"></param>
		/// <returns></returns>
		public static IModulesFactory UseAzureDataLake(this IModulesFactory factory) {
			return factory.Use(new Module());
		}

		/// <summary>
		/// Creates and instance of Azure Data Lake Store client
		/// </summary>
		/// <param name="factory">Factory reference</param>
		/// <param name="accountName">Data Lake account name</param>
		/// <param name="tenantId">Tenant ID</param>
		/// <param name="principalId">Principal ID</param>
		/// <param name="principalSecret">Principal Secret</param>
		/// <param name="listBatchSize">Batch size for list operation for this storage connection. If not set defaults to 5000.</param>
		/// <returns></returns>
		public static IBlobStorage AzureDataLakeGen1StoreByClientSecret(this IBlobStorageFactory factory,
		   string accountName,
		   string tenantId,
		   string principalId,
		   string principalSecret,
		   int listBatchSize = 5000) {
			if (accountName == null)
				throw new ArgumentNullException(nameof(accountName));

			if (tenantId == null)
				throw new ArgumentNullException(nameof(tenantId));

			if (principalId == null)
				throw new ArgumentNullException(nameof(principalId));

			if (principalSecret == null)
				throw new ArgumentNullException(nameof(principalSecret));

			var client = AzureDataLakeGen1Storage.CreateByClientSecret(accountName, new NetworkCredential(principalId, principalSecret, tenantId));
			client.ListBatchSize = listBatchSize;
			return client;
		}
	}
}
