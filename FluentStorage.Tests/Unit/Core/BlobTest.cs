namespace FluentStorage.Tests.Unit.Core;

public class BlobTest {
	[Fact]
	public void Is_root_folder_for_root_folder() {
		Assert.True(new StoreObject("/", StorageObjectType.Folder).IsRootFolder);
	}

	[Fact]
	public void Is_root_folder_for_non_root_folder() {
		Assert.False(new StoreObject("/awesome", StorageObjectType.Folder).IsRootFolder);
	}

	[Theory]
	[InlineData("/", null, "")]
	[InlineData("/f0/f1", "f2", "f2/f0")]
	public void Prepend_path(string path, string prefix, string expected) {
		var blob = new StoreObject(path, StorageObjectType.Folder);
		blob.PrependPath(prefix);
		Assert.Equal(expected, blob.FolderPath);
	}
}