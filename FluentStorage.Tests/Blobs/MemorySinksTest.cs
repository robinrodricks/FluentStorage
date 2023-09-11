using System;
using FluentStorage.Blobs.Sinks.Impl;

namespace FluentStorage.Tests.Blobs.Sink {
	public class MemoryGzipSinkTest : SinksTest {
		public MemoryGzipSinkTest() : base(StorageFactory.Blobs.InMemory().WithGzipCompression()) {

		}
	}

	public class MemorySymmetricEncryptionTest : SinksTest {
		[Obsolete("Rijndael is obsolete in .Net 6 and above")]
		public MemorySymmetricEncryptionTest() : base(
		   StorageFactory.Blobs.InMemory().WithSymmetricEncryption("6qg/7EgPmrK9ZY70pnECtZ40g3dDe74czSvWJ+3dj0A=")) {

		}
	}

	public class MemoryCompressedAndEncryptedTest : SinksTest {
		[Obsolete("Rijndael is obsolete in .Net 6 and above")]
		public MemoryCompressedAndEncryptedTest() : base(
		   StorageFactory.Blobs
			  .InMemory()
			  .WithSinks(
				 new GZipSink(),
				 new SymmetricEncryptionSink("To6X5XVaNNMKFfxssJS6biREGpOVZjEIC6T7cc1rJF0="))) {

		}
	}

	public class MemoryAesSymmetricEncryptionTest : SinksTest {
		public MemoryAesSymmetricEncryptionTest() : base(
		   StorageFactory.Blobs.InMemory().WithAesSymmetricEncryption("6qg/7EgPmrK9ZY70pnECtZ40g3dDe74czSvWJ+3dj0A=")) {

		}
	}

	public class MemoryCompressedAndAesEncryptedTest : SinksTest {
		public MemoryCompressedAndAesEncryptedTest() : base(
		   StorageFactory.Blobs
			  .InMemory()
			  .WithSinks(
				 new GZipSink(),
				 new AesSymmetricEncryptionSink("To6X5XVaNNMKFfxssJS6biREGpOVZjEIC6T7cc1rJF0="))) {

		}
	}
}