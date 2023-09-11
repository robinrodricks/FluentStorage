using System;
using FluentStorage.Blobs.Sinks.Impl;
using FluentStorage.Tests.Utils;

namespace FluentStorage.Tests.Blobs.Sink {
	public class DirectoryGzipSinkTest : SinksTest {
		public DirectoryGzipSinkTest() : base(StorageFactory.Blobs
			.DirectoryFiles(PathHelper.GetLocalPath("Temp"))
			.WithGzipCompression())
		{
		}
	}

	public class DirectorySymmetricEncryptionTest : SinksTest {
		[Obsolete("Rijndael is obsolete in .Net 6 and above")]
		public DirectorySymmetricEncryptionTest() : base(
		   StorageFactory.Blobs
			.DirectoryFiles(PathHelper.GetLocalPath("Temp"))
			.WithSymmetricEncryption("6qg/7EgPmrK9ZY70pnECtZ40g3dDe74czSvWJ+3dj0A="))
		{
		}
	}

	public class DirectoryCompressedAndEncryptedTest : SinksTest {
		[Obsolete("Rijndael is obsolete in .Net 6 and above")]
		public DirectoryCompressedAndEncryptedTest() : base(
		   StorageFactory.Blobs
			  .DirectoryFiles(PathHelper.GetLocalPath("Temp"))
			  .WithSinks(
				 new GZipSink(),
				 new SymmetricEncryptionSink("To6X5XVaNNMKFfxssJS6biREGpOVZjEIC6T7cc1rJF0=")))
		{
		}
	}

	public class DirectoryAesSymmetricEncryptionTest : SinksTest {
		public DirectoryAesSymmetricEncryptionTest() : base(
		   StorageFactory.Blobs
		   .DirectoryFiles(PathHelper.GetLocalPath("Temp"))
		   .WithAesSymmetricEncryption("6qg/7EgPmrK9ZY70pnECtZ40g3dDe74czSvWJ+3dj0A=", "Ucu4aEHU6zTrgZVO+rJMyA=="))
		{
		}
	}

	public class DirectoryCompressedAndAesEncryptedTest : SinksTest {
		public DirectoryCompressedAndAesEncryptedTest() : base(
		   StorageFactory.Blobs
			  .DirectoryFiles(PathHelper.GetLocalPath("Temp"))
			  .WithSinks(
				 new GZipSink(),
				 new AesSymmetricEncryptionSink("6qg/7EgPmrK9ZY70pnECtZ40g3dDe74czSvWJ+3dj0A=", "Ucu4aEHU6zTrgZVO+rJMyA==")
				))
		{
		}
	}
}