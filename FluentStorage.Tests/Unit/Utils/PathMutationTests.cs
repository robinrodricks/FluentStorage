namespace FluentStorage.Tests.Unit.Utils;

public class PathMutationTests {
	// --------------------------------------------------------------------
	// Combine
	// --------------------------------------------------------------------

	[Theory]
	[InlineData(new string[] { }, "")]
	[InlineData(new[] { "" }, "")]
	[InlineData(new[] { "folder" }, "folder")]
	[InlineData(new[] { "folder", "file.txt" }, "folder/file.txt")]
	[InlineData(new[] { "/folder/", "/file.txt/" }, "folder/file.txt")]
	[InlineData(new[] { "\\folder\\", "\\sub\\", "\\file.txt" }, "folder/sub/file.txt")]
	public void Combine_ReturnsExpectedPath(string[] parts, string expected) {
		Assert.Equal(expected, StoragePath.Combine(parts));
	}

	[Fact]
	public void Combine_Null_ReturnsEmpty() {
		Assert.Equal(string.Empty, StoragePath.Combine(null));
	}

	[Fact]
	public void Combine_IgnoresNullAndEmptyParts() {
		string[] parts = { null, "", "/", "folder", null, "", "file.txt" };

		Assert.Equal("folder/file.txt", StoragePath.Combine(parts));
	}

	// --------------------------------------------------------------------
	// Split
	// --------------------------------------------------------------------

	[Fact]
	public void Split_Null_ReturnsNull() {
		Assert.Null(StoragePath.Split(null));
	}

	[Theory]
	[InlineData("")]
	[InlineData("/")]
	[InlineData("//")]
	[InlineData("\\")]
	public void Split_EmptyPath_ReturnsEmptyArray(string path) {
		Assert.Empty(StoragePath.Split(path));
	}

	[Theory]
	[InlineData("file.txt", new[] { "file.txt" })]
	[InlineData("/file.txt", new[] { "file.txt" })]
	[InlineData("folder/file.txt", new[] { "folder", "file.txt" })]
	[InlineData("\\folder\\file.txt", new[] { "folder", "file.txt" })]
	[InlineData("/folder//sub///file.txt", new[] { "folder", "sub", "file.txt" })]
	[InlineData("../folder/file.txt", new[] { "..", "folder", "file.txt" })]
	[InlineData("./folder", new[] { ".", "folder" })]
	public void Split_ReturnsExpectedParts(string path, string[] expected) {
		Assert.Equal(expected, StoragePath.Split(path));
	}

	// --------------------------------------------------------------------
	// GetParent
	// --------------------------------------------------------------------

	[Fact]
	public void GetParent_Null_ReturnsNull() {
		Assert.Null(StoragePath.GetParent(null));
	}

	[Theory]
	[InlineData("", null)]
	[InlineData("/", null)]
	[InlineData("file.txt", "")]
	[InlineData("/file.txt", "")]
	[InlineData("folder/file.txt", "folder")]
	[InlineData("folder/sub/file.txt", "folder/sub")]
	[InlineData("/folder/sub/file.txt", "folder/sub")]
	[InlineData("folder/sub/", "folder")]
	[InlineData("\\folder\\sub\\file.txt", "folder/sub")]
	[InlineData("../file.txt", "..")]
	[InlineData("./file.txt", ".")]
	public void GetParent_ReturnsExpectedParent(string path, string expected) {
		Assert.Equal(expected, StoragePath.GetParent(path));
	}

	[Fact]
	public void CombineAndSplit_Roundtrip() {
		string[] parts = { "folder", "sub", "file.txt" };

		string combined = StoragePath.Combine(parts);

		Assert.Equal(parts, StoragePath.Split(combined));
	}

	[Theory]
	[InlineData("")]
	[InlineData("file.txt")]
	[InlineData("folder/file.txt")]
	[InlineData("/folder//sub///file.txt")]
	public void Combine_IsIdempotent(string path) {
		string normalized = StoragePath.Normalize(path);

		Assert.Equal(normalized, StoragePath.Combine(StoragePath.Split(normalized)));
	}



}