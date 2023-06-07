using Bogus;

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;

namespace FluentStorage.Tests.Integration.Ftp {
	public class FtpFixture : IAsyncLifetime {

		public IContainer FtpContainer { get; }

		private static readonly Faker Faker = new();

		private readonly string _userName;
		private readonly string _password;

		public string UserName => _userName;

		public string Password => _userName;


		public IEnumerable<int> GetPassivePorts() => _passivePorts;

		private readonly ISet<int> _passivePorts;

		private const int MaxUsersCount = 5;

		private const int PassivePortStart = 15_000;

		public int GetPort() => FtpContainer.GetMappedPublicPort(21);

		public FtpFixture() {
			_userName = Faker.Internet.UserName();
			_password = Faker.Internet.Password();
			_passivePorts = new HashSet<int>((MaxUsersCount * 2) + 1);

			ContainerBuilder containerBuilder = new ContainerBuilder()
				.WithAutoRemove(autoRemove: false)
				.WithImage("stilliard/pure-ftpd:latest")
				.WithEnvironment(new Dictionary<string, string> {
					["PUBLICHOST"] = "localhost",
					["FTP_USER_NAME"] = UserName,
					["FTP_USER_PASS"] = Password,
					["FTP_USER_HOME"] = $"/home/{UserName}",
					["FTP_MAX_CLIENTS"] = $"{MaxUsersCount}",
					["FTP_PASSIVE_PORTS"] = $"{PassivePortStart}:{(PassivePortStart + (MaxUsersCount * 2))}"
				})
				.WithPortBinding(21, assignRandomHostPort: true)
				.WithTmpfsMount($"/home/{UserName}/data", AccessMode.ReadWrite)
				.WithTmpfsMount($"/etc/pure-ftpd/passwd", AccessMode.ReadWrite)
				;

			for (int i = 0; i < MaxUsersCount; i++) {
				int port = PassivePortStart + i;
				containerBuilder = containerBuilder.WithPortBinding(port, assignRandomHostPort: true)
								.WithExposedPort(port);
			}

			FtpContainer = containerBuilder.Build();
		}

		///<inheritdoc/>
		public async Task InitializeAsync() {
			await FtpContainer.StartAsync().ConfigureAwait(false);
			//for (int i = 0; i < MaxUsersCount; i++) {
			//	int containerPort = PassivePortStart + i;
			//	_passivePorts.Add(FtpContainer.GetMappedPublicPort(containerPort));
			//}

		}

		///<inheritdoc/>
		public async Task DisposeAsync() => await FtpContainer.StopAsync().ConfigureAwait(false);
	}
}
