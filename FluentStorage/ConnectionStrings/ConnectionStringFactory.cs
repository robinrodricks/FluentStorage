using System;
using System.Collections.Generic;
using System.Linq;
using FluentStorage.Queue;
using FluentStorage.Storage;

namespace FluentStorage.ConnectionStrings;

static class ConnectionStringFactory {
	private const string TypeSeparator = "://";
	private static readonly List<IConnectionFactory> Factories = new List<IConnectionFactory>();

	static ConnectionStringFactory() {
		Register(new BuiltInConnectionFactory());
	}

	public static void Register(IConnectionFactory factory) {
		if (factory == null) throw new ArgumentNullException(nameof(factory));

		Factories.Add(factory);
	}

	public static IStore CreateBlobStorage(string connectionString) {
		return Create(connectionString, (factory, cs) => factory.CreateStore(cs));
	}

	public static IQueue CreateMessager(string connectionString) {
		return Create(connectionString, (factory, cs) => factory.CreateQueue(cs));
	}


	private static TInstance Create<TInstance>(string connectionString, Func<IConnectionFactory, ConnectionString, TInstance> createAction)
		where TInstance : class {
		if (connectionString == null) {
			throw new ArgumentNullException(nameof(connectionString));
		}

		var pcs = new ConnectionString(connectionString);

		TInstance instance = Factories
			.Select(f => createAction(f, pcs))
			.FirstOrDefault(b => b != null);

		if (instance == null) {
			throw new ArgumentException(
				$"could not create any implementation based on the passed connection string (prefix: {pcs.Prefix}), did you register required external module?",
				nameof(connectionString));
		}

		return instance;
	}

}