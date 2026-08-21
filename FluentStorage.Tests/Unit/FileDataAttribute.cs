namespace FluentStorage.Tests.Unit;

//[DataDiscoverer("FluentStorage.Tests.Unit.FileDataDiscoverer", "FluentStorage.Tests.Unit")]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public class FileDataAttribute : DataAttribute
{
	private readonly string _filename;
	private readonly string _folder;

	public FileDataAttribute(string folder, string filename) {
		_filename = filename;
		_folder = folder;
	}

	public override IEnumerable<object[]> GetData(MethodInfo testMethod)
	{
		var pars = testMethod.GetParameters();
		var parameterTypes = pars.Select(par => par.ParameterType).ToArray();

		var file = new StreamReader(PathHelper.GetFullFilename(_folder, _filename));
		yield return new object[] { _folder, _filename, file.BaseStream};
	}
}