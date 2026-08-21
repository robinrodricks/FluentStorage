namespace FluentStorage.Tests.Unit.Storage;

public class MemoryGzipSinkTest : SinksTest {
	public MemoryGzipSinkTest() : base(StorageFactory.InMemory().WithGzipCompression()) {

	}
}

public class MemorySymmetricEncryptionTest : SinksTest {
	[Obsolete("Rijndael is obsolete in .Net 6 and above")]
	public MemorySymmetricEncryptionTest() : base(
		StorageFactory.InMemory().WithSymmetricEncryption("6qg/7EgPmrK9ZY70pnECtZ40g3dDe74czSvWJ+3dj0A=")) {

	}
}

public class MemoryCompressedAndEncryptedTest : SinksTest {
	[Obsolete("Rijndael is obsolete in .Net 6 and above")]
	public MemoryCompressedAndEncryptedTest() : base(
		StorageFactory
			.InMemory()
			.WithSinks(
				new GZipSink(),
				new SymmetricEncryptionSink("To6X5XVaNNMKFfxssJS6biREGpOVZjEIC6T7cc1rJF0="))) {

	}
}

public class MemoryAesSymmetricEncryptionTest : SinksTest {
	public MemoryAesSymmetricEncryptionTest() : base(
		StorageFactory.InMemory().WithAesSymmetricEncryption("6qg/7EgPmrK9ZY70pnECtZ40g3dDe74czSvWJ+3dj0A=")) {

	}
}

public class MemoryCompressedAndAesEncryptedTest : SinksTest {
	public MemoryCompressedAndAesEncryptedTest() : base(
		StorageFactory
			.InMemory()
			.WithSinks(
				new GZipSink(),
				new AesSymmetricEncryptionSink("To6X5XVaNNMKFfxssJS6biREGpOVZjEIC6T7cc1rJF0="))) {

	}
}