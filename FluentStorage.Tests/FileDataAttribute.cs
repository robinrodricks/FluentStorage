using FluentStorage.Tests.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit.Sdk;

namespace FluentStorage.Tests
{
	[DataDiscoverer("FluentStorage.Tests.FileDataDiscoverer", "FluentStorage.Tests")]
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
}