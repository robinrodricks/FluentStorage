namespace FluentStorage.Tests.Integration.Queue;

public abstract class MessagingFixture : IDisposable {
	private static readonly TestConfig _settings = TestConfigLoader.Config;
	public readonly IQueue Messenger;
	private readonly string _fixtureName;
	protected readonly string _testDir;

	protected MessagingFixture() {
		_fixtureName = GetType().Name;
		string buildDir = new FileInfo(new Uri(Assembly.GetExecutingAssembly().Location).LocalPath).Directory.FullName;
		_testDir = Path.Combine(buildDir, "msg-" + Guid.NewGuid());
		Directory.CreateDirectory(_testDir);

		Messenger = CreateMessenger(_settings);
	}

	protected abstract IQueue CreateMessenger(TestConfig settings);

	public void Dispose() {
		if (Messenger != null)
			Messenger.Dispose();

		Directory.Delete(_testDir, true);
	}
}