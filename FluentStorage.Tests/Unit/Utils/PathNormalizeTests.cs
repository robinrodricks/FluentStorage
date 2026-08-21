namespace FluentStorage.Tests.Unit.Utils;

public class PathNormalizeTests {

	// Covers:
	// - Null, empty and root paths
	// - Leading and trailing separator removal
	// - Relative file and folder paths
	// - Backslash to forward slash conversion
	// - Duplicate separator collapsing
	// - Mixed path separator normalization
	// - Preservation of '.' and '..' path segments
	// - Idempotence (normalizing twice yields the same result)
	// - Very long paths

	[Theory]
	[InlineData(null, "")]
	[InlineData("", "")]
	[InlineData("/", "")]
	[InlineData("//", "")]
	[InlineData("///", "")]
	[InlineData("\\", "")]
	[InlineData("\\\\", "")]
	public void Normalize_Empty(string input, string expected) {
		Assert.Equal(expected, StoragePath.Normalize(input));
	}

	[Theory]
	[InlineData("file.txt", "file.txt")]
	[InlineData("/file.txt", "file.txt")]
	[InlineData("\\file.txt", "file.txt")]
	[InlineData("//file.txt", "file.txt")]
	[InlineData("\\\\file.txt", "file.txt")]
	public void Normalize_File(string input, string expected) {
		Assert.Equal(expected, StoragePath.Normalize(input));
	}

	[Theory]
	[InlineData("folder", "folder")]
	[InlineData("/folder", "folder")]
	[InlineData("\\folder", "folder")]
	[InlineData("folder/", "folder")]
	[InlineData("folder\\", "folder")]
	[InlineData("/folder/", "folder")]
	[InlineData("\\folder\\", "folder")]
	public void Normalize_Folder(string input, string expected) {
		Assert.Equal(expected, StoragePath.Normalize(input));
	}

	[Theory]
	[InlineData("folder/file.txt", "folder/file.txt")]
	[InlineData("/folder/file.txt", "folder/file.txt")]
	[InlineData("\\folder\\file.txt", "folder/file.txt")]
	[InlineData("folder\\sub/file.txt", "folder/sub/file.txt")]
	[InlineData("/folder\\sub\\file.txt", "folder/sub/file.txt")]
	public void Normalize_FolderAndFile(string input, string expected) {
		Assert.Equal(expected, StoragePath.Normalize(input));
	}

	[Theory]
	[InlineData("folder//file.txt", "folder/file.txt")]
	[InlineData("folder///sub////file.txt", "folder/sub/file.txt")]
	[InlineData("\\\\folder\\\\sub\\\\file.txt", "folder/sub/file.txt")]
	[InlineData("/folder//sub\\\\file.txt", "folder/sub/file.txt")]
	[InlineData("////folder////sub////", "folder/sub")]
	public void Normalize_DuplicateSeparators(string input, string expected) {
		Assert.Equal(expected, StoragePath.Normalize(input));
	}

	[Theory]
	[InlineData(".", ".")]
	[InlineData("..", "..")]
	[InlineData("./file.txt", "./file.txt")]
	[InlineData("../file.txt", "../file.txt")]
	[InlineData("folder/./file.txt", "folder/./file.txt")]
	[InlineData("folder/../file.txt", "folder/../file.txt")]
	public void Normalize_PreservesDotSegments(string input, string expected) {
		Assert.Equal(expected, StoragePath.Normalize(input));
	}

	[Theory]
	[InlineData("")]
	[InlineData("/")]
	[InlineData("folder")]
	[InlineData("folder/file.txt")]
	[InlineData("/folder//sub\\\\file.txt/")]
	public void Normalize_IsIdempotent(string input) {
		string once = StoragePath.Normalize(input);
		string twice = StoragePath.Normalize(once);

		Assert.Equal(once, twice);
	}

	[Fact]
	public void Normalize_LongPath() {
		string input = string.Join("//", Enumerable.Repeat("folder", 1000));
		string expected = string.Join("/", Enumerable.Repeat("folder", 1000));

		Assert.Equal(expected, StoragePath.Normalize(input));
	}

	[Theory]
	[InlineData("bucket/file.txt", "bucket/file.txt")]
	[InlineData("bucket/folder/file.txt", "bucket/folder/file.txt")]
	[InlineData("bucket//folder///file.txt", "bucket/folder/file.txt")]
	[InlineData("/bucket/folder/file.txt", "bucket/folder/file.txt")]
	[InlineData("\\bucket\\folder\\file.txt", "bucket/folder/file.txt")]

	[InlineData("photos/2026/01/image.jpg", "photos/2026/01/image.jpg")]
	[InlineData("photos\\2026\\01\\image.jpg", "photos/2026/01/image.jpg")]
	[InlineData("/photos//2026///01/image.jpg", "photos/2026/01/image.jpg")]

	[InlineData("documents/report.pdf", "documents/report.pdf")]
	[InlineData("documents\\report.pdf", "documents/report.pdf")]
	[InlineData("/documents/report.pdf/", "documents/report.pdf")]

	[InlineData("C:\\Storage\\file.txt", "C:/Storage/file.txt")]
	[InlineData("C:\\Storage\\Folder\\File.txt", "C:/Storage/Folder/File.txt")]
	[InlineData("\\\\server\\share\\file.txt", "server/share/file.txt")]

	[InlineData("./file.txt", "./file.txt")]
	[InlineData("../file.txt", "../file.txt")]
	[InlineData("folder/./file.txt", "folder/./file.txt")]
	[InlineData("folder/../file.txt", "folder/../file.txt")]

	[InlineData(".hidden", ".hidden")]
	[InlineData("folder/.hidden", "folder/.hidden")]
	[InlineData("folder/.git/config", "folder/.git/config")]

	[InlineData("My Folder/File Name.txt", "My Folder/File Name.txt")]
	[InlineData("日本語/ファイル.txt", "日本語/ファイル.txt")]
	[InlineData("emoji/😀.txt", "emoji/😀.txt")]

	public void Normalize_StorageScenarios(string input, string expected) {
		Assert.Equal(expected, StoragePath.Normalize(input));
	}
}