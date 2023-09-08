using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit.Sdk;

namespace FluentStorage.Tests {
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

			var file = new StreamReader(GetFullFilename(_folder, _filename));
			yield return new object[] {file.BaseStream};
		}

		static string GetFullFilename(string folder, string filename)
		{
			string executable = new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath;
			return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(executable), folder, filename));
		}
	}
}