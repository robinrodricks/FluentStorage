using FluentStorage.Utils.Files;

namespace FluentStorage.Tests.Unit.Utils;

public class TempFileTest {
	[Fact]
	public void NotInUse() {
		using (var tf = new TempFile()) {

		}
	}

	[Fact]
	public void InUse() {
		string path;

		using (var tf = new TempFile()) {
			path = tf;
			File.WriteAllText(tf, "test");
		}

		Assert.False(File.Exists(path));
	}
}