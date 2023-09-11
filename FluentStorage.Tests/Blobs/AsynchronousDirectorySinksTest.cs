using FluentStorage.Blobs;
using FluentStorage.Tests.Utils;
using FluentStorage.Utils.Extensions;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace FluentStorage.Tests.Blobs.Sink
{
	/// <summary>
	/// provides a test method to read a previously encrypted file saved in blob strage in an asynchronous manner
	/// </summary>
	public class AsynchronousDirectoryAesSymmetricEncryptionTest : AsynchronousSinksTest {
		public AsynchronousDirectoryAesSymmetricEncryptionTest() : base(
		   StorageFactory.Blobs
		   .DirectoryFiles(PathHelper.GetLocalPath("Temp"))
		   .WithAesSymmetricEncryption("6qg/7EgPmrK9ZY70pnECtZ40g3dDe74czSvWJ+3dj0A=", "Ucu4aEHU6zTrgZVO+rJMyA=="))
		{
		}

		[Theory]
		[FileData("TestFiles", "e4124b764c1bee11a299005056370622.jpg")]
		[FileData("TestFiles", "d0d829444b1bee11a299005056370622.jpg")]
		public async Task ReadStreamForFile(string folder, string filename, Stream source)
		{
			// the source is unencrpyted, retrieve the file and get it's bytes for comparison
			source.Position = 0;
			var expected = source.ToByteArray();
			source.Dispose();

			// our precooked example saved file will have extension .enc
			// we will copy that file to the blobstorage directory
			// then we will decrypt that and compare to the source file

			var encrytedSourceFileName = $"{filename}.enc";
			var sourceFile = PathHelper.GetFullFilename(folder, encrytedSourceFileName);
			var destinationFile = Path.Combine(PathHelper.GetLocalPath("Temp"), $"{Path.GetFileNameWithoutExtension(Path.GetRandomFileName())}_{encrytedSourceFileName}");
			File.Copy(sourceFile, destinationFile, true);

			using (Stream stream = new MemoryStream()) {
				await _storage.ReadToStreamAsync(sourceFile, stream);
				stream.Position = 0;
				var actual = stream.ToByteArray();
				Assert.Equal(expected, actual);
			}
		}
	}
}