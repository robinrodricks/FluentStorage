using System.Security.Cryptography.X509Certificates;
using FluentStorage.Mongo.Storage;
using FluentStorage.Storage;
using MongoDB.Driver;

namespace FluentStorage;

/// <summary>
/// Factory methods for creating MongoDB GridFS stores.
/// </summary>
public static class MongoGridStorage {

	/// <summary>
	/// Creates a MongoDB GridFS storage provider using a MongoDB connection string (eg. "mongodb+srv://user:pass@cluster0.mongodb.net").
	/// </summary>
	public static IStore FromConnectionString(string connectionString, string databaseName, string bucketName = "fs") {
		return new MongoGridStore(connectionString, databaseName, bucketName);
	}

	/// <summary>
	/// Creates a MongoDB GridFS storage provider using credentials (SCRAM auth).
	/// </summary>
	public static IStore FromCredentials(string host,int port,string username,string password,string databaseName,
		string bucketName = "fs",string authDatabase = null,bool useSsl = false) {
		return new MongoGridStore(host, port, username, password, databaseName, bucketName, authDatabase, useSsl);
	}

	/// <summary>
	/// Creates a MongoDB GridFS storage provider using a MongoDB client and database instance.
	/// </summary>
	public static IStore FromClient(MongoClient client, IMongoDatabase database, string bucketName = "fs") {
		return new MongoGridStore(client, database, bucketName);
	}

	/// <summary>
	/// Creates a MongoDB GridFS storage provider using X.509 client certificate (mutual-TLS auth), used for MongoDB Atlas / enterprise deployments.
	/// </summary>
	public static IStore FromClientCertificate(string host,int port, X509Certificate2 clientCertificate,
		string databaseName, string bucketName = "fs") {
		return new MongoGridStore(host, port, clientCertificate, databaseName, bucketName);
	}

	/// <summary>
	/// Creates a MongoDB GridFS storage provider using a `MongoClientSettings` object, for callers who need full control.
	/// </summary>
	public static IStore FromClientSettings(MongoClientSettings clientSettings, string databaseName, string bucketName = "fs") {
		return new MongoGridStore(clientSettings, databaseName, bucketName);
	}
}