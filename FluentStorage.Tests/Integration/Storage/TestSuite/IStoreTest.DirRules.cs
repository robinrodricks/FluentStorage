	using FluentStorage.Rules;

	namespace FluentStorage.Tests.Integration.Storage.TestSuite;

	/// <summary>
	/// Integration tests verifying that `UploadDirectory` and `DownloadDirectory` correctly apply
	/// rule filters, in whitelist-only, blacklist-only and mixed (whitelist + blacklist) setups.
	///
	/// Every test checks BOTH directions:
	///   - upload with rules applied, then download everything (no rules) to see what actually landed.
	///   - upload everything (no rules), then download with rules applied to see what actually landed.
	/// </summary>
	public partial class IStoreTest {


		// =====================================================================
		// Folder-rule cascading — a folder rule must accept/reject everything under that folder,
		// not just its direct children.
		// =====================================================================

		[Fact]
		public async Task Rules_FolderRule_Whitelist_CascadesIntoNestedSubfolders() {
			var rules = new List<StorageRule> { new DirectoryNameRule(true, new[] { "two" }) };
			var expected = new[]
			{
				new LocalFile("one/two/c.txt", 25),
				new LocalFile("one/two/three/d.txt", 35),
				new LocalFile("one/two/three/four/e.txt", 45),
				new LocalFile("one/two/three/four/five/f.txt", 55),
			};
			await AssertUploadAndDownload(DeepTree, rules, expected);
		}

		[Fact]
		public async Task Rules_FolderRule_Blacklist_CascadesIntoNestedSubfolders() {
			var rules = new List<StorageRule> { new DirectoryNameRule(false, new[] { "three" }) };
			var expected = new[]
			{
				new LocalFile("a.txt", 5),
				new LocalFile("one/b.txt", 15),
				new LocalFile("one/two/c.txt", 25),
			};
			await AssertUploadAndDownload(DeepTree, rules, expected);
		}

		// =====================================================================
		// ExtensionRule case-insensitivity
		// =====================================================================

		[Fact]
		public async Task Rules_ExtensionRule_Whitelist_IsCaseInsensitive() {
			var rules = new List<StorageRule> { new ExtensionRule(true, new[] { "txt" }) };
			var expected = new[]
			{
				new LocalFile("lower.txt", 10),
				new LocalFile("upper.TXT", 20),
				new LocalFile("mixed.TxT", 30),
			};
			await AssertUploadAndDownload(ExtensionCaseTree, rules, expected);
		}

		[Fact]
		public async Task Rules_ExtensionRule_Blacklist_IsCaseInsensitive() {
			var rules = new List<StorageRule> { new ExtensionRule(false, new[] { "jpg" }) };
			var expected = new[]
			{
				new LocalFile("lower.txt", 10),
				new LocalFile("upper.TXT", 20),
				new LocalFile("mixed.TxT", 30),
				new LocalFile("doc.pdf", 60),
			};
			await AssertUploadAndDownload(ExtensionCaseTree, rules, expected);
		}

		// =====================================================================
		// 1 RULE — every rule type, whitelist and blacklist.
		// =====================================================================

		[Fact]
		public async Task Rules_OneRule_ExtensionRule_Whitelist() {
			var rules = new List<StorageRule> { new ExtensionRule(true, new[] { "txt", "md" }) };
			var expected = new[]
			{
				new LocalFile("a.txt", 10),
				new LocalFile("docs/readme.txt", 40),
				new LocalFile("docs/notes.md", 50),
				new LocalFile("docs/archive/old.txt", 60),
			};
			await AssertUploadAndDownload(WideTree, rules, expected);
		}

		[Fact]
		public async Task Rules_OneRule_ExtensionRule_Blacklist() {
			var rules = new List<StorageRule> { new ExtensionRule(false, new[] { "tmp", "log", "exe", "pdb" }) };
			var expected = new[]
			{
				new LocalFile("a.txt", 10),
				new LocalFile("docs/readme.txt", 40),
				new LocalFile("docs/notes.md", 50),
				new LocalFile("docs/archive/old.txt", 60),
				new LocalFile("images/photo.jpg", 80),
				new LocalFile("images/photo.png", 90),
				new LocalFile("images/thumbs/thumb1.jpg", 100),
				new LocalFile("images/thumbs/thumb2.png", 110),
				new LocalFile("src/main.cs", 120),
				new LocalFile("src/helper.cs", 130),
				new LocalFile("node_modules/lib/index.js", 170),
				new LocalFile("node_modules/package.json", 180),
				new LocalFile(".git/config", 190),
				new LocalFile(".git/HEAD", 200),
			};
			await AssertUploadAndDownload(WideTree, rules, expected);
		}

		[Fact]
		public async Task Rules_OneRule_DirectoryNameRule_Whitelist() {
			// "docs" cascades into docs/archive as well.
			var rules = new List<StorageRule> { new DirectoryNameRule(true, new[] { "docs" }) };
			var expected = new[]
			{
				new LocalFile("docs/readme.txt", 40),
				new LocalFile("docs/notes.md", 50),
				new LocalFile("docs/archive/old.txt", 60),
				new LocalFile("docs/archive/old.log", 70),
			};
			await AssertUploadAndDownload(WideTree, rules, expected);
		}

		[Fact]
		public async Task Rules_OneRule_DirectoryNameRule_Blacklist() {
			var rules = new List<StorageRule> { new DirectoryNameRule(false, new[] { "node_modules", ".git" }) };
			var expected = new[]
			{
				new LocalFile("a.txt", 10),
				new LocalFile("b.log", 20),
				new LocalFile("c.tmp", 30),
				new LocalFile("docs/readme.txt", 40),
				new LocalFile("docs/notes.md", 50),
				new LocalFile("docs/archive/old.txt", 60),
				new LocalFile("docs/archive/old.log", 70),
				new LocalFile("images/photo.jpg", 80),
				new LocalFile("images/photo.png", 90),
				new LocalFile("images/thumbs/thumb1.jpg", 100),
				new LocalFile("images/thumbs/thumb2.png", 110),
				new LocalFile("src/main.cs", 120),
				new LocalFile("src/helper.cs", 130),
				new LocalFile("src/bin/app.exe", 140),
				new LocalFile("src/bin/app.pdb", 150),
				new LocalFile("src/obj/temp.tmp", 160),
			};
			await AssertUploadAndDownload(WideTree, rules, expected);
		}

		[Fact]
		public async Task Rules_OneRule_DirectoryNameRegexRule_Whitelist() {
			// Matches the top-level "images" and "src" folders; cascades into their subfolders too.
			var rules = new List<StorageRule> { new DirectoryNameRegexRule(true, new[] { "^images$", "^src$" }) };
			var expected = new[]
			{
				new LocalFile("images/photo.jpg", 80),
				new LocalFile("images/photo.png", 90),
				new LocalFile("images/thumbs/thumb1.jpg", 100),
				new LocalFile("images/thumbs/thumb2.png", 110),
				new LocalFile("src/main.cs", 120),
				new LocalFile("src/helper.cs", 130),
				new LocalFile("src/bin/app.exe", 140),
				new LocalFile("src/bin/app.pdb", 150),
				new LocalFile("src/obj/temp.tmp", 160),
			};
			await AssertUploadAndDownload(WideTree, rules, expected);
		}

		[Fact]
		public async Task Rules_OneRule_DirectoryNameRegexRule_Blacklist() {
			var rules = new List<StorageRule> { new DirectoryNameRegexRule(false, new[] { "^bin$", "^obj$" }) };
			var expected = new[]
			{
				new LocalFile("a.txt", 10),
				new LocalFile("b.log", 20),
				new LocalFile("c.tmp", 30),
				new LocalFile("docs/readme.txt", 40),
				new LocalFile("docs/notes.md", 50),
				new LocalFile("docs/archive/old.txt", 60),
				new LocalFile("docs/archive/old.log", 70),
				new LocalFile("images/photo.jpg", 80),
				new LocalFile("images/photo.png", 90),
				new LocalFile("images/thumbs/thumb1.jpg", 100),
				new LocalFile("images/thumbs/thumb2.png", 110),
				new LocalFile("src/main.cs", 120),
				new LocalFile("src/helper.cs", 130),
				new LocalFile("node_modules/lib/index.js", 170),
				new LocalFile("node_modules/package.json", 180),
				new LocalFile(".git/config", 190),
				new LocalFile(".git/HEAD", 200),
			};
			await AssertUploadAndDownload(WideTree, rules, expected);
		}

		[Fact]
		public async Task Rules_OneRule_ObjectNameRule_Whitelist() {
			var rules = new List<StorageRule> { new ObjectNameRule(true, new[] { "package.json", "readme.txt" }) };
			var expected = new[]
			{
				new LocalFile("docs/readme.txt", 40),
				new LocalFile("node_modules/package.json", 180),
			};
			await AssertUploadAndDownload(WideTree, rules, expected);
		}

		[Fact]
		public async Task Rules_OneRule_ObjectNameRule_Blacklist() {
			var rules = new List<StorageRule> { new ObjectNameRule(false, new[] { "temp.tmp", "HEAD" }) };
			var expected = new[]
			{
				new LocalFile("a.txt", 10),
				new LocalFile("b.log", 20),
				new LocalFile("c.tmp", 30),
				new LocalFile("docs/readme.txt", 40),
				new LocalFile("docs/notes.md", 50),
				new LocalFile("docs/archive/old.txt", 60),
				new LocalFile("docs/archive/old.log", 70),
				new LocalFile("images/photo.jpg", 80),
				new LocalFile("images/photo.png", 90),
				new LocalFile("images/thumbs/thumb1.jpg", 100),
				new LocalFile("images/thumbs/thumb2.png", 110),
				new LocalFile("src/main.cs", 120),
				new LocalFile("src/helper.cs", 130),
				new LocalFile("src/bin/app.exe", 140),
				new LocalFile("src/bin/app.pdb", 150),
				new LocalFile("node_modules/lib/index.js", 170),
				new LocalFile("node_modules/package.json", 180),
				new LocalFile(".git/config", 190),
			};
			await AssertUploadAndDownload(WideTree, rules, expected);
		}

		[Fact]
		public async Task Rules_OneRule_ObjectNameRegexRule_Whitelist() {
			var rules = new List<StorageRule> { new ObjectNameRegexRule(true, new[] { @"\.cs$", @"^package\.json$" }) };
			var expected = new[]
			{
				new LocalFile("src/main.cs", 120),
				new LocalFile("src/helper.cs", 130),
				new LocalFile("node_modules/package.json", 180),
			};
			await AssertUploadAndDownload(WideTree, rules, expected);
		}

		[Fact]
		public async Task Rules_OneRule_ObjectNameRegexRule_Blacklist() {
			var rules = new List<StorageRule> { new ObjectNameRegexRule(false, new[] { "^thumb", @"\.tmp$" }) };
			var expected = new[]
			{
				new LocalFile("a.txt", 10),
				new LocalFile("b.log", 20),
				new LocalFile("docs/readme.txt", 40),
				new LocalFile("docs/notes.md", 50),
				new LocalFile("docs/archive/old.txt", 60),
				new LocalFile("docs/archive/old.log", 70),
				new LocalFile("images/photo.jpg", 80),
				new LocalFile("images/photo.png", 90),
				new LocalFile("src/main.cs", 120),
				new LocalFile("src/helper.cs", 130),
				new LocalFile("src/bin/app.exe", 140),
				new LocalFile("src/bin/app.pdb", 150),
				new LocalFile("node_modules/lib/index.js", 170),
				new LocalFile("node_modules/package.json", 180),
				new LocalFile(".git/config", 190),
				new LocalFile(".git/HEAD", 200),
			};
			await AssertUploadAndDownload(WideTree, rules, expected);
		}

		[Fact]
		public async Task Rules_OneRule_FullPathRule_Whitelist() {
			var rules = new List<StorageRule> { new FullPathRule(true, new[] { "bin", "thumbs" }) };
			var expected = new[]
			{
				new LocalFile("images/thumbs/thumb1.jpg", 100),
				new LocalFile("images/thumbs/thumb2.png", 110),
				new LocalFile("src/bin/app.exe", 140),
				new LocalFile("src/bin/app.pdb", 150),
			};
			await AssertUploadAndDownload(WideTree, rules, expected);
		}

		[Fact]
		public async Task Rules_OneRule_FullPathRule_Blacklist() {
			var rules = new List<StorageRule> { new FullPathRule(false, new[] { "archive", "obj" }) };
			var expected = new[]
			{
				new LocalFile("a.txt", 10),
				new LocalFile("b.log", 20),
				new LocalFile("c.tmp", 30),
				new LocalFile("docs/readme.txt", 40),
				new LocalFile("docs/notes.md", 50),
				new LocalFile("images/photo.jpg", 80),
				new LocalFile("images/photo.png", 90),
				new LocalFile("images/thumbs/thumb1.jpg", 100),
				new LocalFile("images/thumbs/thumb2.png", 110),
				new LocalFile("src/main.cs", 120),
				new LocalFile("src/helper.cs", 130),
				new LocalFile("src/bin/app.exe", 140),
				new LocalFile("src/bin/app.pdb", 150),
				new LocalFile("node_modules/lib/index.js", 170),
				new LocalFile("node_modules/package.json", 180),
				new LocalFile(".git/config", 190),
				new LocalFile(".git/HEAD", 200),
			};
			await AssertUploadAndDownload(WideTree, rules, expected);
		}

		[Fact]
		public async Task Rules_OneRule_FullPathRegexRule_Whitelist() {
			var rules = new List<StorageRule> { new FullPathRegexRule(true, new[] { @"\.jpg$", "/bin/" }) };
			var expected = new[]
			{
				new LocalFile("images/photo.jpg", 80),
				new LocalFile("images/thumbs/thumb1.jpg", 100),
				new LocalFile("src/bin/app.exe", 140),
				new LocalFile("src/bin/app.pdb", 150),
			};
			await AssertUploadAndDownload(WideTree, rules, expected);
		}

		[Fact]
		public async Task Rules_OneRule_FullPathRegexRule_Blacklist() {
			var rules = new List<StorageRule> { new FullPathRegexRule(false, new[] { @"\.log$", @"\.tmp$" }) };
			var expected = new[]
			{
				new LocalFile("a.txt", 10),
				new LocalFile("docs/readme.txt", 40),
				new LocalFile("docs/notes.md", 50),
				new LocalFile("docs/archive/old.txt", 60),
				new LocalFile("images/photo.jpg", 80),
				new LocalFile("images/photo.png", 90),
				new LocalFile("images/thumbs/thumb1.jpg", 100),
				new LocalFile("images/thumbs/thumb2.png", 110),
				new LocalFile("src/main.cs", 120),
				new LocalFile("src/helper.cs", 130),
				new LocalFile("src/bin/app.exe", 140),
				new LocalFile("src/bin/app.pdb", 150),
				new LocalFile("node_modules/lib/index.js", 170),
				new LocalFile("node_modules/package.json", 180),
				new LocalFile(".git/config", 190),
				new LocalFile(".git/HEAD", 200),
			};
			await AssertUploadAndDownload(WideTree, rules, expected);
		}

		// =====================================================================
		// 2 RULES — whitelist+whitelist, blacklist+blacklist, whitelist+blacklist.
		// =====================================================================

		[Fact]
		public async Task Rules_TwoRules_WhitelistAndWhitelist() {
			// Must be .txt/.cs AND live under docs or src (cascading).
			var rules = new List<StorageRule>
			{
				new ExtensionRule(true, new[] { "txt", "cs" }),
				new DirectoryNameRule(true, new[] { "docs", "src" }),
			};
			var expected = new[]
			{
				new LocalFile("docs/readme.txt", 40),
				new LocalFile("docs/archive/old.txt", 60),
				new LocalFile("src/main.cs", 120),
				new LocalFile("src/helper.cs", 130),
			};
			await AssertUploadAndDownload(WideTree, rules, expected);
		}

		[Fact]
		public async Task Rules_TwoRules_BlacklistAndBlacklist() {
			var rules = new List<StorageRule>
			{
				new ExtensionRule(false, new[] { "tmp", "log" }),
				new DirectoryNameRule(false, new[] { "node_modules", ".git" }),
			};
			var expected = new[]
			{
				new LocalFile("a.txt", 10),
				new LocalFile("docs/readme.txt", 40),
				new LocalFile("docs/notes.md", 50),
				new LocalFile("docs/archive/old.txt", 60),
				new LocalFile("images/photo.jpg", 80),
				new LocalFile("images/photo.png", 90),
				new LocalFile("images/thumbs/thumb1.jpg", 100),
				new LocalFile("images/thumbs/thumb2.png", 110),
				new LocalFile("src/main.cs", 120),
				new LocalFile("src/helper.cs", 130),
				new LocalFile("src/bin/app.exe", 140),
				new LocalFile("src/bin/app.pdb", 150),
			};
			await AssertUploadAndDownload(WideTree, rules, expected);
		}

		[Fact]
		public async Task Rules_TwoRules_WhitelistAndBlacklist_ByPathAndName() {
			// Path must contain "images" or "src", but never the compiled binaries.
			var rules = new List<StorageRule>
			{
				new FullPathRule(true, new[] { "images", "src" }),
				new ObjectNameRule(false, new[] { "app.exe", "app.pdb" }),
			};
			var expected = new[]
			{
				new LocalFile("images/photo.jpg", 80),
				new LocalFile("images/photo.png", 90),
				new LocalFile("images/thumbs/thumb1.jpg", 100),
				new LocalFile("images/thumbs/thumb2.png", 110),
				new LocalFile("src/main.cs", 120),
				new LocalFile("src/helper.cs", 130),
				new LocalFile("src/obj/temp.tmp", 160),
			};
			await AssertUploadAndDownload(WideTree, rules, expected);
		}

		[Fact]
		public async Task Rules_TwoRules_WhitelistAndBlacklist_ByNameRegexAndPathRegex() {
			// Keep .cs/.json files, but never anything under node_modules.
			var rules = new List<StorageRule>
			{
				new ObjectNameRegexRule(true, new[] { @"\.cs$", @"\.json$" }),
				new FullPathRegexRule(false, new[] { "node_modules/" }),
			};
			var expected = new[]
			{
				new LocalFile("src/main.cs", 120),
				new LocalFile("src/helper.cs", 130),
			};
			await AssertUploadAndDownload(WideTree, rules, expected);
		}

		// =====================================================================
		// 3 RULES
		// =====================================================================

		[Fact]
		public async Task Rules_ThreeRules_AllWhitelist() {
			var rules = new List<StorageRule>
			{
				new ExtensionRule(true, new[] { "jpg", "png", "cs" }),
				new DirectoryNameRule(true, new[] { "images", "src" }),
				new ObjectNameRule(true, new[] { "photo.jpg", "main.cs", "thumb1.jpg" }),
			};
			var expected = new[]
			{
				new LocalFile("images/photo.jpg", 80),
				new LocalFile("images/thumbs/thumb1.jpg", 100),
				new LocalFile("src/main.cs", 120),
			};
			await AssertUploadAndDownload(WideTree, rules, expected);
		}

		[Fact]
		public async Task Rules_ThreeRules_AllBlacklist() {
			var rules = new List<StorageRule>
			{
				new ExtensionRule(false, new[] { "log", "tmp" }),
				new DirectoryNameRule(false, new[] { "node_modules", ".git" }),
				new ObjectNameRule(false, new[] { "app.exe", "app.pdb" }),
			};
			var expected = new[]
			{
				new LocalFile("a.txt", 10),
				new LocalFile("docs/readme.txt", 40),
				new LocalFile("docs/notes.md", 50),
				new LocalFile("docs/archive/old.txt", 60),
				new LocalFile("images/photo.jpg", 80),
				new LocalFile("images/photo.png", 90),
				new LocalFile("images/thumbs/thumb1.jpg", 100),
				new LocalFile("images/thumbs/thumb2.png", 110),
				new LocalFile("src/main.cs", 120),
				new LocalFile("src/helper.cs", 130),
			};
			await AssertUploadAndDownload(WideTree, rules, expected);
		}

		[Fact]
		public async Task Rules_ThreeRules_TwoWhitelistOneBlacklist() {
			var rules = new List<StorageRule>
			{
				new FullPathRule(true, new[] { "docs", "images" }),
				new ExtensionRule(true, new[] { "txt", "jpg", "png", "md" }),
				new ObjectNameRule(false, new[] { "thumb2.png" }),
			};
			var expected = new[]
			{
				new LocalFile("docs/readme.txt", 40),
				new LocalFile("docs/notes.md", 50),
				new LocalFile("docs/archive/old.txt", 60),
				new LocalFile("images/photo.jpg", 80),
				new LocalFile("images/photo.png", 90),
				new LocalFile("images/thumbs/thumb1.jpg", 100),
			};
			await AssertUploadAndDownload(WideTree, rules, expected);
		}

		[Fact]
		public async Task Rules_ThreeRules_OneWhitelistTwoBlacklist() {
			var rules = new List<StorageRule>
			{
				new DirectoryNameRule(true, new[] { "src" }),
				new ExtensionRule(false, new[] { "exe", "pdb" }),
				new ObjectNameRule(false, new[] { "temp.tmp" }),
			};
			var expected = new[]
			{
				new LocalFile("src/main.cs", 120),
				new LocalFile("src/helper.cs", 130),
			};
			await AssertUploadAndDownload(WideTree, rules, expected);
		}

		// =====================================================================
		// 4 RULES
		// =====================================================================

		[Fact]
		public async Task Rules_FourRules_AllWhitelist() {
			var rules = new List<StorageRule>
			{
				new DirectoryNameRule(true, new[] { "images", "src", "docs" }),
				new ExtensionRule(true, new[] { "jpg", "cs", "txt" }),
				new ObjectNameRule(true, new[] { "photo.jpg", "main.cs", "readme.txt", "helper.cs", "thumb1.jpg" }),
				new FullPathRule(true, new[] { "photo", "main", "readme", "helper", "thumb1" }),
			};
			var expected = new[]
			{
				new LocalFile("docs/readme.txt", 40),
				new LocalFile("images/photo.jpg", 80),
				new LocalFile("images/thumbs/thumb1.jpg", 100),
				new LocalFile("src/main.cs", 120),
				new LocalFile("src/helper.cs", 130),
			};
			await AssertUploadAndDownload(WideTree, rules, expected);
		}

		[Fact]
		public async Task Rules_FourRules_AllBlacklist() {
			var rules = new List<StorageRule>
			{
				new ExtensionRule(false, new[] { "tmp", "log" }),
				new DirectoryNameRule(false, new[] { "node_modules", ".git" }),
				new ObjectNameRule(false, new[] { "app.exe", "app.pdb" }),
				new FullPathRule(false, new[] { "thumbs" }),
			};
			var expected = new[]
			{
				new LocalFile("a.txt", 10),
				new LocalFile("docs/readme.txt", 40),
				new LocalFile("docs/notes.md", 50),
				new LocalFile("docs/archive/old.txt", 60),
				new LocalFile("images/photo.jpg", 80),
				new LocalFile("images/photo.png", 90),
				new LocalFile("src/main.cs", 120),
				new LocalFile("src/helper.cs", 130),
			};
			await AssertUploadAndDownload(WideTree, rules, expected);
		}

		[Fact]
		public async Task Rules_FourRules_TwoWhitelistTwoBlacklist() {
			var rules = new List<StorageRule>
			{
				new DirectoryNameRule(true, new[] { "docs", "images", "src" }),
				new FullPathRegexRule(true, new[] { @"\.(txt|md|cs|jpg|png)$" }),
				new ObjectNameRule(false, new[] { "old.log", "temp.tmp" }),
				new ExtensionRule(false, new[] { "exe", "pdb" }),
			};
			var expected = new[]
			{
				new LocalFile("docs/readme.txt", 40),
				new LocalFile("docs/notes.md", 50),
				new LocalFile("docs/archive/old.txt", 60),
				new LocalFile("images/photo.jpg", 80),
				new LocalFile("images/photo.png", 90),
				new LocalFile("images/thumbs/thumb1.jpg", 100),
				new LocalFile("images/thumbs/thumb2.png", 110),
				new LocalFile("src/main.cs", 120),
				new LocalFile("src/helper.cs", 130),
			};
			await AssertUploadAndDownload(WideTree, rules, expected);
		}

		[Fact]
		public async Task Rules_FourRules_OneWhitelistThreeBlacklist() {
			var rules = new List<StorageRule>
			{
				new DirectoryNameRule(true, new[] { "images" }),
				new ExtensionRule(false, new[] { "png" }),
				new ObjectNameRule(false, new[] { "thumb2.png" }),
				new FullPathRegexRule(false, new[] { "thumbs/thumb1" }),
			};
			var expected = new[]
			{
				new LocalFile("images/photo.jpg", 80),
			};
			await AssertUploadAndDownload(WideTree, rules, expected);
		}
	}