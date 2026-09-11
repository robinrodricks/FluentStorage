namespace FluentStorage.Tests.Integration.Storage.TestSuite;

public partial class IStoreTest {


	// ---------------------------------------------------------------------
	// MoveDirectory
	// ---------------------------------------------------------------------

	[Fact]
	public async Task MoveDirectory_WithContent_MovesFilesToDestination() {
		string source = RandomFolder();
		string destination = RandomFolder();

		await CreateText($"{source}/a.txt", "one");
		await CreateText($"{source}/child/b.txt", "two");

		try {
			await _storage.MoveDirectory(source, destination);
		}
		catch (NotImplementedException) {
			return;
		}

		Assert.Equal("one", await _storage.GetText($"{destination}/a.txt"));
		Assert.Equal("two", await _storage.GetText($"{destination}/child/b.txt"));

		Assert.False(await _storage.ObjectExists($"{source}/a.txt"));
	}

	[Fact]
	public async Task MoveDirectory_EmptyFolder_DestinationExists() {
		string source = RandomFolder();
		string destination = RandomFolder();

		try {
			await _storage.CreateDirectory(source, false);

			await _storage.MoveDirectory(source, destination);
		}
		catch (NotImplementedException) {
			return;
		}
		catch (NotSupportedException) {
			return;
		}

		Assert.True(await _storage.DirectoryExists(destination));
	}

	[Fact]
	public async Task MoveDirectory_DestinationParentDoesNotExist_IsCreated() {
		string source = RandomFolder();
		string destination = $"{RandomFolder()}/nested/target";

		await CreateText($"{source}/a.txt", "one");

		try {
			await _storage.MoveDirectory(source, destination);
		}
		catch (NotImplementedException) {
			return;
		}

		Assert.Equal("one", await _storage.GetText($"{destination}/a.txt"));
	}

}
