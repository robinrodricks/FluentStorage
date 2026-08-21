using Bogus;
using FluentFTP;

namespace FluentStorage.Tests.Integration.Ftp;

/// <summary>
/// This test uses Testcontainers to create a PureFTP based docker server, and run our FTP tests against it.
/// </summary>
public class FtpDockerTest : IClassFixture<FtpDockerFixture>, IAsyncLifetime {

	private IStore _storage;
	private FtpDockerFixture Fixture { get; }

	private static readonly Faker Faker = new();

	private readonly ITestOutputHelper _outputHelper;

	public FtpDockerTest(ITestOutputHelper outputHelper, FtpDockerFixture ftpFixture) {
		_outputHelper = outputHelper;
		Fixture = ftpFixture;
		FtpStorage.Use();
	}

	///<inheritdoc/>
	public Task DisposeAsync() => Task.CompletedTask;

	///<inheritdoc/>
	public Task InitializeAsync() {
		AsyncFtpClient client = new("localhost", Fixture.UserName, Fixture.Password, Fixture.GetPort());
		_outputHelper?.WriteLine($"Port utilisé durant le test : {client.Port}");
		_storage = FtpStorage.FromClient(client);

		return Task.CompletedTask;
	}

	[Fact]
	public async Task Given_Append_is_true_When_calling_WriteAsync_Then_the_file_should_be_uploaded_properly() {

		// Arrange
		byte[] bytesSent = Faker.Random.Bytes(1025);
		const string fullPath = "/test/test-file.txt";
		await _storage.SetBytes(fullPath, bytesSent.Take(1024).ToArray(), true);

		// Act
		await _storage.SetBytes(fullPath, bytesSent.Skip(1024).ToArray(), true);

		// Assert
		byte[] received = await _storage.GetBytes(fullPath);
		received.Should().BeEquivalentTo(bytesSent);

	}
}