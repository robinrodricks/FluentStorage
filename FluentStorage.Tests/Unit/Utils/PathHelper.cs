namespace FluentStorage.Tests.Unit.Utils;

internal class PathHelper {
	internal static string GetFullFilename(string folder, string filename) {
		return Path.Combine(GetLocalPath(folder), filename);
	}

	internal static string GetLocalPath(string folder) {
		string executable = new Uri(Assembly.GetExecutingAssembly().Location).LocalPath;
		return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(executable), folder));
	}
}