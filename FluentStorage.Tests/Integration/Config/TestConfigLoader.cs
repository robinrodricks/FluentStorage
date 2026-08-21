using YamlDotNet.Serialization;

namespace FluentStorage.Tests.Integration.Config;

public static class TestConfigLoader {
	private static TestConfig _config;

	/// <summary>
	/// Loads the test config YAML file and returns the settings in a typed object (`ITestConfig`)
	/// </summary>
	public static TestConfig Config {
		get {
			if (_config == null) {

				// get the YAML test config file at the repo root
				string projectDir = Directory.GetParent(AppContext.BaseDirectory)!.Parent!.Parent!.Parent!.Parent!.FullName;
				string configPath = Path.Combine(projectDir, "fluentstorage.yaml");

				// load it if it exists
				if (!File.Exists(configPath)) {
					throw new Exception($"Test config file `{configPath}` does not exist! Please create it using the `fluentstorage.yaml.template` and fill in the required settings.");
				}

				var deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();

				try {
					var text = File.ReadAllText(configPath);
					_config = deserializer.Deserialize<TestConfig>(text);
				}
				catch (Exception ex) {
					throw new Exception("Failed to read test config file `fluentstorage.yaml`! Maybe it is corrupt or invalid! Error: "+ ex.Message);
				}
			}

			return _config;
		}
	}
}