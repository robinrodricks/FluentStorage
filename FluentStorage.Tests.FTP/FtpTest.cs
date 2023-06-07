using Bogus;

using FluentAssertions;

using FluentFTP;

using FluentStorage.Blobs;

using Xunit.Abstractions;

namespace FluentStorage.Tests.Integration.Ftp {
	public class FtpTest : IClassFixture<FtpFixture>, IAsyncLifetime {

		private IBlobStorage _storage;
		private FtpFixture Fixture { get; }

		private static readonly Faker Faker = new();
		private readonly ITestOutputHelper _outputHelper;

		public FtpTest(FtpFixture ftpFixture) {
			Fixture = ftpFixture;
			StorageFactory.Modules.UseFtpStorage();
		}

		///<inheritdoc/>
		public async Task DisposeAsync() {
			await Fixture.DisposeAsync();
		}

		///<inheritdoc/>
		public async Task InitializeAsync() {
			await Fixture.InitializeAsync();

			AsyncFtpClient client = new("localhost", new System.Net.NetworkCredential(Fixture.UserName, Fixture.Password), Fixture.GetPort(), new FtpConfig { ActivePorts = Fixture.GetPassivePorts() });
			_outputHelper?.WriteLine($"Port utilisé durant le test : {client.Port}");
			_storage = StorageFactory.Blobs.FtpFromFluentFtpClient(client);
		}

		[Fact]
		public async Task Given_Append_is_true_When_calling_WriteAsync_Then_the_file_should_be_uploaded_properly() {

			// Arrange
			byte[] bytesSent = Faker.Random.Bytes(1025);
			const string fullPath = "/test/test-file.txt";
			await _storage.OpenTransactionAsync();
			await _storage.WriteAsync(fullPath, bytesSent.Take(1024).ToArray(), true);

			// Act
			await _storage.WriteAsync(fullPath, bytesSent.Skip(1024).ToArray(), true);

			// Assert
			byte[] received = await _storage.ReadBytesAsync(fullPath);
			received.Should().BeEquivalentTo(bytesSent);

		}
	}
}
