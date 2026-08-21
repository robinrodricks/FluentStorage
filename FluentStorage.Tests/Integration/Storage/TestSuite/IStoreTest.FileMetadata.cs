using FluentStorage.Utils.Hashing;

namespace FluentStorage.Tests.Integration.Storage.TestSuite;

public partial class IStoreTest {


	// ---------------------------------------------------------------------
	// Metadata & Existence
	// ---------------------------------------------------------------------

	[Fact]
	public async Task ObjectExists_ExistingObject_ReturnsTrue() {
		string file = RandomFile();

		await CreateText(file);

		Assert.True(await _storage.ObjectExists(file));
	}

	[Fact]
	public async Task ObjectExists_MissingObject_ReturnsFalse() {
		Assert.False(await _storage.ObjectExists(RandomFile()));
	}

	[Fact]
	public async Task ObjectsExists_ReturnsStatusForEveryObject() {
		string f1 = RandomFile();
		string f2 = RandomFile();
		string f3 = RandomFile();

		await CreateText(f1);
		await CreateText(f3);

		var result = await _storage.ObjectsExists(new[] { f1, f2, f3 });

		Assert.Equal(3, result.Count);

		Assert.True(result[0]);
		Assert.False(result[1]);
		Assert.True(result[2]);
	}

	[Fact]
	public async Task GetObjectInfo_ReturnsBasicInformation() {
		string file = RandomFile();

		await CreateText(file, "Hello World");

		var obj = await _storage.GetObjectInfo(file);

		Assert.NotNull(obj);

		Assert.Equal(file, obj.FullPath);
		Assert.True(obj.IsFile);
		Assert.False(obj.IsFolder);

		Assert.NotNull(obj.Size);
		Assert.True(obj.Size >= 11);
	}

	[Fact]
	public async Task GetObjectInfo_MissingObject_ReturnsNullOrThrows() {
		string file = RandomFile();

		try {
			var obj = await _storage.GetObjectInfo(file);

			Assert.Null(obj);
		}
		catch {
			// provider is allowed to throw
		}
	}

	[Fact]
	public async Task GetObjectsInfo_ReturnsInformationForMultipleObjects() {
		string f1 = RandomFile();
		string f2 = RandomFile();

		await CreateText(f1, "AAA");
		await CreateText(f2, "BBBB");

		var list = await _storage.GetObjectsInfo(new[] { f1, f2 });

		Assert.Equal(2, list.Count);

		Assert.Contains(list, x => x.FullPath == f1);
		Assert.Contains(list, x => x.FullPath == f2);
	}

	[Fact]
	public async Task SetObjectInfo_DoesNotThrow() {
		string file = RandomFile();

		await CreateText(file);

		var obj = await _storage.GetObjectInfo(file);

		await _storage.SetObjectInfo(obj);

		var updated = await _storage.GetObjectInfo(file);

		Assert.NotNull(updated);
	}

	[Fact]
	public async Task SetObjectsInfo_DoesNotThrow() {
		string f1 = RandomFile();
		string f2 = RandomFile();

		await CreateText(f1);
		await CreateText(f2);

		var list = await _storage.GetObjectsInfo(new[] { f1, f2 });

		await _storage.SetObjectsInfo(list);

		var verify = await _storage.GetObjectsInfo(new[] { f1, f2 });

		Assert.Equal(2, verify.Count);
	}

	[Fact]
	public async Task GetObjectChecksum_DoesNotThrow() {
		string file = RandomFile();

		await CreateText(file, "abcdef");

		var obj = await _storage.GetObjectInfo(file);

		var hash = await _storage.GetObjectChecksum(obj);

		if (hash != null)
			Assert.NotEmpty(hash.Value);
	}


	[Fact]
	public async Task GetChecksum_MD5() {

		// make random data and get its hash
		var blob = new StoreObject(RandomBlobPath());
		var bytes = RandomGenerator.GetRandomBytes(1000, 10000);
		var hash = HashUtility.HashBytes(bytes, StorageHash.MD5);

		// upload object and get its hash
		await _storage.SetBytes(blob, bytes);
		var hash2 = await _storage.GetObjectChecksum(blob, StorageHash.MD5);

		Assert.Equal(hash, hash2.Value);
	}


	[Fact]
	public async Task GetChecksum_SHA1() {

		// make random data and get its hash
		var blob = new StoreObject(RandomBlobPath());
		var bytes = RandomGenerator.GetRandomBytes(1000, 10000);
		var hash = HashUtility.HashBytes(bytes, StorageHash.SHA1);

		// upload object and get its hash
		await _storage.SetBytes(blob, bytes);
		var hash2 = await _storage.GetObjectChecksum(blob, StorageHash.SHA1);

		Assert.Equal(hash, hash2.Value);
	}

	[Fact]
	public async Task GetChecksum_SHA256() {

		// make random data and get its hash
		var blob = new StoreObject(RandomBlobPath());
		var bytes = RandomGenerator.GetRandomBytes(1000, 10000);
		var hash = HashUtility.HashBytes(bytes, StorageHash.SHA256);

		// upload object and get its hash
		await _storage.SetBytes(blob, bytes);
		var hash2 = await _storage.GetObjectChecksum(blob, StorageHash.SHA256);

		Assert.Equal(hash, hash2.Value);
	}

	[Fact]
	public async Task GetChecksum_SHA512() {

		// make random data and get its hash
		var blob = new StoreObject(RandomBlobPath());
		var bytes = RandomGenerator.GetRandomBytes(1000, 10000);
		var hash = HashUtility.HashBytes(bytes, StorageHash.SHA512);

		// upload object and get its hash
		await _storage.SetBytes(blob, bytes);
		var hash2 = await _storage.GetObjectChecksum(blob, StorageHash.SHA512);

		Assert.Equal(hash, hash2.Value);
	}

	[Fact]
	public async Task GetChecksum_CRC32() {

		// make random data and get its hash
		var blob = new StoreObject(RandomBlobPath());
		var bytes = RandomGenerator.GetRandomBytes(1000, 10000);
		var hash = HashUtility.HashBytes(bytes, StorageHash.CRC32);

		// upload object and get its hash
		await _storage.SetBytes(blob, bytes);
		var hash2 = await _storage.GetObjectChecksum(blob, StorageHash.CRC32);

		Assert.Equal(hash, hash2.Value);
	}

	[Fact]
	public async Task GetObjectLength_ReturnsCorrectLength() {
		string file = RandomFile();

		const string text = "Hello World";

		await CreateText(file, text);

		long length = await _storage.GetObjectLength(file);

		Assert.Equal(text.Length, length);
	}

	[Fact]
	public async Task GetObjectLength_MissingObject_ReturnsDefaultValue() {
		long length = await _storage.GetObjectLength(
			RandomFile(),
			defaultValue: 12345);

		Assert.Equal(12345, length);
	}

	[Fact]
	public async Task Metadata_AfterOverwrite_SizeChanges() {
		string file = RandomFile();

		await CreateText(file, "abc");

		long size1 = await _storage.GetObjectLength(file);

		await CreateText(file, "abcdefghijklmnopqrstuvwxyz");

		long size2 = await _storage.GetObjectLength(file);

		Assert.True(size2 > size1);
	}

	[Fact]
	public async Task Metadata_DateModified_IsNotBefore_DateCreated() {
		string file = RandomFile();

		await CreateText(file);

		var obj = await _storage.GetObjectInfo(file);

		if (obj.DateCreated.HasValue && obj.DateModified.HasValue) {
			Assert.True(obj.DateModified >= obj.DateCreated);
		}
	}

	[Fact]
	public async Task Metadata_FullPath_IsCorrect() {
		string file = RandomFile();

		await CreateText(file);

		var obj = await _storage.GetObjectInfo(file);

		Assert.Equal(file, obj.FullPath);
	}

	[Fact]
	public async Task Metadata_Name_IsCorrect() {
		string folder = RandomFolder();

		string file = $"{folder}/hello.txt";

		await CreateText(file);

		var obj = await _storage.GetObjectInfo(file);

		Assert.Equal("hello.txt", obj.Name);
	}

	[Fact]
	public async Task Metadata_FolderPath_IsCorrect() {
		string folder = RandomFolder();

		string file = $"{folder}/hello.txt";

		await CreateText(file);

		var obj = await _storage.GetObjectInfo(file);

		Assert.Equal(folder, obj.FolderPath);
	}



}