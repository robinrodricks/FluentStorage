using FluentStorage.Tests.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace FluentStorage.Tests
{
	/// <summary>
	/// Implementation of <see cref="IDataDiscoverer"/> used to discover the data
	/// provided by <see cref="FileDataAttribute"/>.
	/// </summary>
	public class FileDataDiscoverer : IDataDiscoverer
	{
		/// <inheritdoc/>
		public IEnumerable<object[]> GetData(IAttributeInfo dataAttribute, IMethodInfo testMethod)
		{
			var args = dataAttribute.GetConstructorArguments().ToArray();
			Console.WriteLine(args.Length);
			var file = new StreamReader(PathHelper.GetFullFilename(args[0].ToString(), args[1].ToString()));
			yield return new object[] { args[0].ToString(), args[1].ToString(), file.BaseStream };
		}

		public bool SupportsDiscoveryEnumeration(IAttributeInfo dataAttribute, IMethodInfo testMethod) => true;
	}
}