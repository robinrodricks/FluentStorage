using Newtonsoft.Json;
using FluentStorage.Blobs;
using FluentStorage.ConnectionString;
using FluentStorage.Messaging;

namespace FluentStorage.Databricks {
	class Module : IExternalModule, IConnectionFactory {
		public IConnectionFactory ConnectionFactory => this;

		public IBlobStorage CreateBlobStorage(StorageConnectionString connectionString) {
			if (connectionString.Prefix == KnownPrefix.Databricks) {
				connectionString.GetRequired("baseUri", true, out string baseUri);
				connectionString.GetRequired("token", true, out string token);

				return new DatabricksBlobStorage(baseUri, token);
			}

			return null;
		}

		public IMessenger CreateMessenger(StorageConnectionString connectionString) => null;

		internal static string AsDbJson(object obj) {
			if (obj == null)
				return string.Empty;

			return JsonConvert.SerializeObject(obj, Formatting.Indented,
			   new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
		}
	}
}
