
using FluentStorage.Rules;

namespace FluentStorage.Tests.Unit.Rules;

public class RuleTests {

	private static StoreObject File(string path) {
		return new StoreObject(path, StorageObjectType.File);
	}

	private static StoreObject Folder(string path) {
		return new StoreObject(path, StorageObjectType.Folder);
	}

	//=========================================================================
	// ObjectNameRule
	//=========================================================================

	[Fact]
	public void ObjectNameRule_Whitelist_Match() {
		var rule = new ObjectNameRule(true, new[] { "file.txt" });

		Assert.True(rule.IsAllowed(File("docs/file.txt")));
	}

	[Fact]
	public void ObjectNameRule_Whitelist_NoMatch() {
		var rule = new ObjectNameRule(true, new[] { "image.png" });

		Assert.False(rule.IsAllowed(File("docs/file.txt")));
	}

	[Fact]
	public void ObjectNameRule_Blacklist_Match() {
		var rule = new ObjectNameRule(false, new[] { "secret.txt" });

		Assert.False(rule.IsAllowed(File("docs/secret.txt")));
	}

	[Fact]
	public void ObjectNameRule_Blacklist_NoMatch() {
		var rule = new ObjectNameRule(false, new[] { "secret.txt" });

		Assert.True(rule.IsAllowed(File("docs/file.txt")));
	}

	[Fact]
	public void ObjectNameRule_Folder() {
		var rule = new ObjectNameRule(true, new[] { "Images" });

		Assert.True(rule.IsAllowed(Folder("Root/Images")));
	}

	[Fact]
	public void ObjectNameRule_CaseInsensitive() {
		var rule = new ObjectNameRule(true, new[] { "FILE.TXT" });

		Assert.False(rule.IsAllowed(File("docs/file.txt")));
	}

	//=========================================================================
	// ObjectNameRegexRule
	//=========================================================================

	[Fact]
	public void ObjectNameRegexRule_Whitelist() {
		var rule = new ObjectNameRegexRule(true, new[] { @"^file\d+\.txt$" });

		Assert.True(rule.IsAllowed(File("docs/file123.txt")));
	}

	[Fact]
	public void ObjectNameRegexRule_Whitelist_NoMatch() {
		var rule = new ObjectNameRegexRule(true, new[] { @"^file\d+\.txt$" });

		Assert.False(rule.IsAllowed(File("docs/image.png")));
	}

	[Fact]
	public void ObjectNameRegexRule_Blacklist() {
		var rule = new ObjectNameRegexRule(false, new[] { @"^temp" });

		Assert.False(rule.IsAllowed(File("docs/temp001.txt")));
	}

	//=========================================================================
	// ExtensionRule
	//=========================================================================

	[Fact]
	public void ExtensionRule_Whitelist() {
		var rule = new ExtensionRule(true, new[] { "txt" });

		Assert.True(rule.IsAllowed(File("docs/file.txt")));
	}

	[Fact]
	public void ExtensionRule_Blacklist() {
		var rule = new ExtensionRule(false, new[] { "exe" });

		Assert.False(rule.IsAllowed(File("docs/setup.exe")));
	}

	[Fact]
	public void ExtensionRule_NoExtension() {
		var rule = new ExtensionRule(true, new[] { "" });

		Assert.False(rule.IsAllowed(File("docs/LICENSE.txt")));
	}

	[Fact]
	public void ExtensionRule_MultipleDots() {
		var rule = new ExtensionRule(true, new[] { "gz" });

		Assert.True(rule.IsAllowed(File("archive.tar.gz")));
	}

	[Fact]
	public void ExtensionRule_CaseInsensitive() {
		var rule = new ExtensionRule(true, new[] { "JPG" });

		Assert.True(rule.IsAllowed(File("images/photo.jpg")));
	}

	//=========================================================================
	// FullPathRule
	//=========================================================================

	[Fact]
	public void FullPathRule_Whitelist() {
		var rule = new FullPathRule(true, new[] { "docs/file.txt" });

		Assert.True(rule.IsAllowed(File("docs/file.txt")));
	}

	[Fact]
	public void FullPathRule_Blacklist() {
		var rule = new FullPathRule(false, new[] { "private/file.txt" });

		Assert.False(rule.IsAllowed(File("private/file.txt")));
	}

	[Fact]
	public void FullPathRule_Folder() {
		var rule = new FullPathRule(true, new[] { "Root/Images" });

		Assert.True(rule.IsAllowed(Folder("Root/Images")));
	}

	//=========================================================================
	// FullPathRegexRule
	//=========================================================================

	[Fact]
	public void FullPathRegexRule_Whitelist() {
		var rule = new FullPathRegexRule(true, new[] { @"^docs/.+\.txt$" });

		Assert.True(rule.IsAllowed(File("docs/readme.txt")));
	}

	[Fact]
	public void FullPathRegexRule_Blacklist() {
		var rule = new FullPathRegexRule(false, new[] { @"^private/" });

		Assert.False(rule.IsAllowed(File("private/test.txt")));
	}

	[Fact]
	public void FullPathRegexRule_Folder() {
		var rule = new FullPathRegexRule(true, new[] { @"^Root/.+$" });

		Assert.True(rule.IsAllowed(Folder("Root/Images")));
	}

	//=========================================================================
	// DirectoryNameRule
	//=========================================================================

	[Fact]
	public void DirectoryNameRule_File_UsesFolderPath() {
		var rule = new DirectoryNameRule(true, new[] { "Images" }, 1);

		Assert.True(rule.IsAllowed(File("Root/Images/file.jpg")));
	}

	[Fact]
	public void DirectoryNameRule_Folder_UsesFullPath() {
		var rule = new DirectoryNameRule(true, new[] { "Images" }, 1);

		Assert.True(rule.IsAllowed(Folder("Root/Images")));
	}

	[Fact]
	public void DirectoryNameRule_Blacklist() {
		var rule = new DirectoryNameRule(false, new[] { "Private" });

		Assert.False(rule.IsAllowed(File("Root/Private/file.txt")));
	}

	[Fact]
	public void DirectoryNameRule_StartSegment0() {
		var rule = new DirectoryNameRule(true, new[] { "Root" }, 0);

		Assert.True(rule.IsAllowed(File("Root/Images/file.txt")));
	}

	[Fact]
	public void DirectoryNameRule_StartSegment1() {
		var rule = new DirectoryNameRule(true, new[] { "Images" }, 1);

		Assert.True(rule.IsAllowed(File("Root/Images/file.txt")));
	}

	[Fact]
	public void DirectoryNameRule_StartSegment2() {
		var rule = new DirectoryNameRule(true, new[] { "Photos" }, 2);

		Assert.True(rule.IsAllowed(File("Root/Images/Photos/file.jpg")));
	}

	[Fact]
	public void DirectoryNameRule_SegmentDoesNotExist() {
		var rule = new DirectoryNameRule(true, new[] { "Missing" }, 4);

		Assert.False(rule.IsAllowed(File("Root/Images/file.jpg")));
	}

	//=========================================================================
	// DirectoryNameRegexRule
	//=========================================================================

	[Fact]
	public void DirectoryNameRegexRule_File() {
		var rule = new DirectoryNameRegexRule(true, new[] { @"^Ima.*" }, 1);

		Assert.True(rule.IsAllowed(File("Root/Images/file.png")));
	}

	[Fact]
	public void DirectoryNameRegexRule_Folder() {
		var rule = new DirectoryNameRegexRule(true, new[] { @"^Ima.*" }, 1);

		Assert.True(rule.IsAllowed(Folder("Root/Images")));
	}

	[Fact]
	public void DirectoryNameRegexRule_Blacklist() {
		var rule = new DirectoryNameRegexRule(false, new[] { @"^Private$" });

		Assert.False(rule.IsAllowed(File("Root/Private/file.txt")));
	}

	[Fact]
	public void DirectoryNameRegexRule_StartSegment() {
		var rule = new DirectoryNameRegexRule(true, new[] { @"^Photos$" }, 2);

		Assert.True(rule.IsAllowed(File("Root/Images/Photos/file.jpg")));
	}

	//=========================================================================
	// ObjectNameRule tests
	//=========================================================================

	[Fact]
	public void ObjectNameRule_DuplicateEntries() {
		var rule = new ObjectNameRule(true, new[] { "file.txt", "file.txt", "file.txt" });
		Assert.True(rule.IsAllowed(File("a/file.txt")));
	}

	[Fact]
	public void ObjectNameRule_ExactMatchOnly() {
		var rule = new ObjectNameRule(true, new[] { "file" });
		Assert.False(rule.IsAllowed(File("a/file.txt")));
	}

	[Fact]
	public void ObjectNameRule_DifferentFoldersSameName() {
		var rule = new ObjectNameRule(true, new[] { "file.txt" });
		Assert.True(rule.IsAllowed(File("a/file.txt")));
		Assert.True(rule.IsAllowed(File("b/c/d/file.txt")));
	}

	[Fact]
	public void ObjectNameRule_Unicode() {
		var rule = new ObjectNameRule(true, new[] { "Résumé.pdf" });
		Assert.True(rule.IsAllowed(File("docs/Résumé.pdf")));
	}

	[Fact]
	public void ObjectNameRule_Emoji() {
		var rule = new ObjectNameRule(true, new[] { "😀.png" });
		Assert.True(rule.IsAllowed(File("img/😀.png")));
	}

	[Fact]
	public void ObjectNameRule_Spaces() {
		var rule = new ObjectNameRule(true, new[] { "My File.txt" });
		Assert.True(rule.IsAllowed(File("docs/My File.txt")));
	}

	//=========================================================================
	// ObjectNameRegexRule tests
	//=========================================================================

	[Fact]
	public void ObjectNameRegexRule_ExactRegex() {
		var rule = new ObjectNameRegexRule(true, new[] { "^file$" });
		Assert.False(rule.IsAllowed(File("docs/file.txt")));
	}

	[Fact]
	public void ObjectNameRegexRule_MultiplePatterns() {
		var rule = new ObjectNameRegexRule(true, new[] {
			"^abc.*",
			".*xyz$"
		});

		Assert.True(rule.IsAllowed(File("a/abcdef")));
		Assert.True(rule.IsAllowed(File("a/123xyz")));
		Assert.False(rule.IsAllowed(File("a/hello")));
	}

	[Fact]
	public void ObjectNameRegexRule_MatchesFolderName() {
		var rule = new ObjectNameRegexRule(true, new[] { "^Folder\\d+$" });
		Assert.True(rule.IsAllowed(Folder("Root/Folder123")));
	}

	[Fact]
	public void ObjectNameRegexRule_MatchEverything() {
		var rule = new ObjectNameRegexRule(true, new[] { ".*" });
		Assert.True(rule.IsAllowed(File("anything.whatever")));
	}

	[Fact]
	public void ObjectNameRegexRule_MatchNothing() {
		var rule = new ObjectNameRegexRule(true, new[] { "^$" });
		Assert.False(rule.IsAllowed(File("abc")));
	}

	//=========================================================================
	// ExtensionRule tests
	//=========================================================================

	[Fact]
	public void ExtensionRule_HiddenFile() {
		var rule = new ExtensionRule(true, new[] { "gitignore" });
		Assert.True(rule.IsAllowed(File(".gitignore")));
	}

	[Fact]
	public void ExtensionRule_TrailingDot() {
		var rule = new ExtensionRule(true, new[] { "." });
		Assert.False(rule.IsAllowed(File("file.")));
	}

	[Fact]
	public void ExtensionRule_ManyDots() {
		var rule = new ExtensionRule(true, new[] { "txt" });
		Assert.True(rule.IsAllowed(File("a.b.c.d.e.txt")));
	}

	[Fact]
	public void ExtensionRule_FolderHasNoExtension() {
		var rule = new ExtensionRule(true, new[] { "" });
		Assert.True(rule.IsAllowed(Folder("Images")));
	}

	[Fact]
	public void ExtensionRule_RejectDifferentExtension() {
		var rule = new ExtensionRule(true, new[] { "jpg" });
		Assert.False(rule.IsAllowed(File("photo.png")));
	}

	//=========================================================================
	// FullPathRule tests
	//=========================================================================

	[Fact]
	public void FullPathRule_RootFile() {
		var rule = new FullPathRule(true, new[] { "file.txt" });
		Assert.True(rule.IsAllowed(File("file.txt")));
	}

	[Fact]
	public void FullPathRule_DeepPath() {
		var rule = new FullPathRule(true, new[] { "a/b/c/d/e/f/g.txt" });
		Assert.True(rule.IsAllowed(File("a/b/c/d/e/f/g.txt")));
	}

	[Fact]
	public void FullPathRule_PartialPathFails() {
		var rule = new FullPathRule(true, new[] { "a/b" });
		Assert.True(rule.IsAllowed(File("a/b/file.txt")));
	}

	[Fact]
	public void FullPathRule_SimilarPaths() {
		var rule = new FullPathRule(true, new[] { "abc/file.txt" });

		Assert.True(rule.IsAllowed(File("abc/file.txt")));
		Assert.False(rule.IsAllowed(File("abcd/file.txt")));
	}

	//=========================================================================
	// FullPathRegexRule tests
	//=========================================================================

	[Fact]
	public void FullPathRegexRule_EndsWithTxt() {
		var rule = new FullPathRegexRule(true, new[] { "\\.txt$" });

		Assert.True(rule.IsAllowed(File("a/b/file.txt")));
		Assert.False(rule.IsAllowed(File("a/b/file.jpg")));
	}

	[Fact]
	public void FullPathRegexRule_PrivateFolder() {
		var rule = new FullPathRegexRule(true, new[] { ".*/private/.*" });

		Assert.True(rule.IsAllowed(File("root/private/file.txt")));
		Assert.False(rule.IsAllowed(File("root/public/file.txt")));
	}

	[Fact]
	public void FullPathRegexRule_MultiplePatterns() {
		var rule = new FullPathRegexRule(true, new[] {
			"^docs/",
			"^images/"
		});

		Assert.True(rule.IsAllowed(File("docs/a.txt")));
		Assert.True(rule.IsAllowed(File("images/a.png")));
		Assert.False(rule.IsAllowed(File("music/a.mp3")));
	}

	//=========================================================================
	// DirectoryNameRule tests
	//=========================================================================

	[Fact]
	public void DirectoryNameRule_FileInRoot_NoSegments() {
		var rule = new DirectoryNameRule(true, new[] { "Root" }, 0);
		Assert.False(rule.IsAllowed(File("file.txt")));
	}

	[Fact]
	public void DirectoryNameRule_OneDirectory() {
		var rule = new DirectoryNameRule(true, new[] { "Images" }, 0);
		Assert.True(rule.IsAllowed(File("Images/file.png")));
	}

	[Fact]
	public void DirectoryNameRule_DeepDirectory_AllSegments() {
		var obj = File("A/B/C/D/file.txt");

		Assert.True(new DirectoryNameRule(true, new[] { "A" }, 0).IsAllowed(obj));
		Assert.True(new DirectoryNameRule(true, new[] { "B" }, 1).IsAllowed(obj));
		Assert.True(new DirectoryNameRule(true, new[] { "C" }, 2).IsAllowed(obj));
		Assert.True(new DirectoryNameRule(true, new[] { "D" }, 3).IsAllowed(obj));
	}

	[Fact]
	public void DirectoryNameRule_RepeatedNames() {
		var obj = File("A/A/A/file.txt");

		Assert.True(new DirectoryNameRule(true, new[] { "A" }, 0).IsAllowed(obj));
		Assert.True(new DirectoryNameRule(true, new[] { "A" }, 1).IsAllowed(obj));
		Assert.True(new DirectoryNameRule(true, new[] { "A" }, 2).IsAllowed(obj));
	}

	[Fact]
	public void DirectoryNameRule_RootFolder() {
		var rule = new DirectoryNameRule(true, new[] { "Root" }, 0);
		Assert.True(rule.IsAllowed(Folder("Root")));
	}

	//=========================================================================
	// DirectoryNameRegexRule tests
	//=========================================================================

	[Fact]
	public void DirectoryNameRegexRule_OneDirectory() {
		var rule = new DirectoryNameRegexRule(true, new[] { "^Images$" }, 0);
		Assert.True(rule.IsAllowed(File("Images/file.png")));
	}

	[Fact]
	public void DirectoryNameRegexRule_RepeatedDirectories() {
		var obj = File("A/A/A/file.txt");

		Assert.True(new DirectoryNameRegexRule(true, new[] { "^A$" }, 0).IsAllowed(obj));
		Assert.True(new DirectoryNameRegexRule(true, new[] { "^A$" }, 1).IsAllowed(obj));
		Assert.True(new DirectoryNameRegexRule(true, new[] { "^A$" }, 2).IsAllowed(obj));
	}

	[Fact]
	public void DirectoryNameRegexRule_InvalidSegment() {
		var rule = new DirectoryNameRegexRule(true, new[] { ".*" }, 10);
		Assert.False(rule.IsAllowed(File("A/B/file.txt")));
	}

	[Fact]
	public void DirectoryNameRegexRule_RootFolder() {
		var rule = new DirectoryNameRegexRule(true, new[] { "^Root$" }, 0);
		Assert.True(rule.IsAllowed(Folder("Root")));
	}

	//=========================================================================
	// Empty whitelist/blacklist lists should always allow
	//=========================================================================

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void ObjectNameRule_EmptyList_Allows(bool whitelist) {
		var rule = new ObjectNameRule(whitelist, Array.Empty<string>());

		Assert.True(rule.IsAllowed(File("Docs/file.txt")));
		Assert.True(rule.IsAllowed(Folder("Docs")));
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void ObjectNameRegexRule_EmptyList_Allows(bool whitelist) {
		var rule = new ObjectNameRegexRule(whitelist, Array.Empty<string>());

		Assert.True(rule.IsAllowed(File("Docs/file.txt")));
		Assert.True(rule.IsAllowed(Folder("Docs")));
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void ExtensionRule_EmptyList_Allows(bool whitelist) {
		var rule = new ExtensionRule(whitelist, Array.Empty<string>());

		Assert.True(rule.IsAllowed(File("Docs/file.txt")));
		Assert.True(rule.IsAllowed(File("README")));
		Assert.True(rule.IsAllowed(Folder("Docs")));
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void FullPathRule_EmptyList_Allows(bool whitelist) {
		var rule = new FullPathRule(whitelist, Array.Empty<string>());

		Assert.True(rule.IsAllowed(File("Docs/file.txt")));
		Assert.True(rule.IsAllowed(Folder("Docs")));
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void FullPathRegexRule_EmptyList_Allows(bool whitelist) {
		var rule = new FullPathRegexRule(whitelist, Array.Empty<string>());

		Assert.True(rule.IsAllowed(File("Docs/file.txt")));
		Assert.True(rule.IsAllowed(Folder("Docs")));
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void DirectoryNameRule_EmptyList_Allows(bool whitelist) {
		var rule = new DirectoryNameRule(whitelist, Array.Empty<string>());

		Assert.True(rule.IsAllowed(File("Root/Images/file.png")));
		Assert.True(rule.IsAllowed(Folder("Root/Images")));
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void DirectoryNameRegexRule_EmptyList_Allows(bool whitelist) {
		var rule = new DirectoryNameRegexRule(whitelist, Array.Empty<string>());

		Assert.True(rule.IsAllowed(File("Root/Images/file.png")));
		Assert.True(rule.IsAllowed(Folder("Root/Images")));
	}
}