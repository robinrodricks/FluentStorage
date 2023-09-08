using System;
using System.IO;
using System.Threading.Tasks;
using FluentStorage.Blobs;
using FluentStorage.Blobs.Sinks.Impl;
using FluentStorage.Utils.Extensions;
using Xunit;

namespace FluentStorage.Tests.Blobs.Sink {
	public class GzipSinkTest : SinksTest {
		public GzipSinkTest() : base(StorageFactory.Blobs.InMemory().WithGzipCompression()) {

		}
	}

	public class SymmetricEncryptionTest : SinksTest {
		[Obsolete("Rijndael is obsolete in .Net 6 and above")]
		public SymmetricEncryptionTest() : base(
		   StorageFactory.Blobs.InMemory().WithSymmetricEncryption("6qg/7EgPmrK9ZY70pnECtZ40g3dDe74czSvWJ+3dj0A=")) {

		}
	}

	public class CompressedAndEncryptedTest : SinksTest {
		[Obsolete("Rijndael is obsolete in .Net 6 and above")]
		public CompressedAndEncryptedTest() : base(
		   StorageFactory.Blobs
			  .InMemory()
			  .WithSinks(
				 new GZipSink(),
				 new SymmetricEncryptionSink("To6X5XVaNNMKFfxssJS6biREGpOVZjEIC6T7cc1rJF0="))) {

		}
	}

	public class AesSymmetricEncryptionTest : SinksTest {
		public AesSymmetricEncryptionTest() : base(
		   StorageFactory.Blobs.InMemory().WithAesSymmetricEncryption("6qg/7EgPmrK9ZY70pnECtZ40g3dDe74czSvWJ+3dj0A=")) {

		}
	}

	public class CompressedAndAesEncryptedTest : SinksTest {
		public CompressedAndAesEncryptedTest() : base(
		   StorageFactory.Blobs
			  .InMemory()
			  .WithSinks(
				 new GZipSink(),
				 new AesSymmetricEncryptionSink("To6X5XVaNNMKFfxssJS6biREGpOVZjEIC6T7cc1rJF0="))) {

		}
	}

	public abstract class SinksTest {
		private readonly IBlobStorage storage;

		protected SinksTest(IBlobStorage storage) {
			this.storage = storage;
		}

		[Theory]
		[InlineData(null)]
		[InlineData("sample")]
		[InlineData("123")]
		[InlineData("123123123123123123123123123123123123123123123123123123123123123123123123123123123123123123123123123123123123123123123123")]
		public async Task Roundtrip(string sample) {
			await storage.WriteTextAsync("target.txt", sample);

			Assert.Equal(sample, await storage.ReadTextAsync("target.txt"));
		}

		[Theory]
		[InlineData("sample")]
		[InlineData("123")]
		[InlineData("123123123123123123123123123123123123123123123123123123123123123123123123123123123123123123123123123123123123123123123123")]
		public async Task RoundtripStream(string sample)
		{
			Stream source = sample.ToMemoryStream();
			source.Position = 0;
			await storage.WriteAsync("target.txt", source);

			Stream stream = new MemoryStream();
			await storage.ReadToStreamAsync("target.txt", stream);
			Assert.Equal(sample, FromStream(stream));
		}

		[Theory]
		[FileData("TestFiles", "e4124b764c1bee11a299005056370622.jpg")]
		[FileData("TestFiles", "d0d829444b1bee11a299005056370622.jpg")]
		public async Task RoundtripStreamForFile(Stream source)
		{
			source.Position = 0;
			await storage.WriteAsync("target.txt", source);

			Stream stream = new MemoryStream();
			await storage.ReadToStreamAsync("target.txt", stream);
			Assert.Equal(source.ToByteArray(), stream.ToByteArray());
		}

		static string FromStream(Stream stream) {
			using var reader = new StreamReader(stream);
			stream.Position = 0;
			return reader.ReadToEnd();
		}
	}
}