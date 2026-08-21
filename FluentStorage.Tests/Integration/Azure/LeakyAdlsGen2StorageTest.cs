using FluentStorage.Azure.Blobs;
using FluentStorage.Azure.Blobs.DataLake.Model;

namespace FluentStorage.Tests.Integration.Azure;

public class LeakyAdlsGen2StorageTest : IAsyncLifetime {
	private readonly TestConfig _settings;
	private readonly IAzureDataLakeStore _storage;
	private static readonly string Filesystem = nameof(LeakyAdlsGen2StorageTest).ToLower();

	public LeakyAdlsGen2StorageTest() {
		_settings = TestConfigLoader.Config;
		_storage = AzureDataLakeStorage.FromAzureAd(
			_settings.AzureDataLakeStorageName,
			_settings.AzureTenantId,
			_settings.AzureClientId,
			_settings.AzureClientSecret);
	}

	[Fact]
	public async Task Authenticate_with_shared_key() {
		IAzureDataLakeStore authInstance =
			AzureDataLakeStorage.FromSharedKey(_settings.AzureDataLakeStorageName,
				_settings.AzureDataLakeStorageKey);

		//trigger any operation
		await authInstance.ListObjects();
	}

	[Fact]
	public async Task Authenticate_with_service_principal() {
		//needs to have "Storage Blob Data Owner"

		IStore authInstance = AzureDataLakeStorage.FromAzureAd(
			_settings.AzureDataLakeStorageName,
			_settings.AzureTenantId,
			_settings.AzureClientId,
			_settings.AzureClientSecret);

		//trigger any operation
		await authInstance.ListObjects();
	}

	[Fact]
	public async Task FS_list_doesnt_crash() {
		List<Filesystem> list = await _storage.ListFilesystems();

		Assert.True(list.Count > 0);
	}

	[Fact]
	public async Task FS_Creates_deletes_and_lists() {
		string filesystem = "createfs-" + Guid.NewGuid().ToString();

		try {
			//await _storage.DeleteFilesystemAsync(filesystem);
			Assert.DoesNotContain(await _storage.ListFilesystems(), x => x.Name == filesystem);

			await _storage.CreateFilesystem(filesystem);
			Assert.Contains(await _storage.ListFilesystems(), x => x.Name == filesystem);
		}
		finally {
			await _storage.DeleteFilesystem(filesystem);
		}
		Assert.DoesNotContain(await _storage.ListFilesystems(), x => x.Name == filesystem);
	}

	[Fact]
	public async Task FS_GetProperties() {
		string fsName = "propfs";

		await _storage.SetText(fsName + "/fff", "test");

		StoreObject fsBlob = await _storage.GetObjectInfo(fsName);


	}

	[Fact]
	public async Task Acl_assign_permisssions_to_file_for_user() {
		string path = StoragePath.Combine(Filesystem, Guid.NewGuid().ToString());
		string userId = _settings.AzureDataLakeOperatorObjectId;

		//write something
		await _storage.SetText(path, "perm?");

		//check that user has no permissions
		AccessControl access = await _storage.GetAccessControl(path);
		Assert.DoesNotContain(access.Acl, x => x.Identity == userId);

		//assign user a write permission
		access.Acl.Add(new AclEntry(ObjectType.User, userId, false, true, false));
		await _storage.SetAccessControl(path, access);

		//check user has permissions now
		access = await _storage.GetAccessControl(path);
		AclEntry userAcl = access.Acl.First(e => e.Identity == userId);
		Assert.False(userAcl.CanRead);
		Assert.True(userAcl.CanWrite);
		Assert.False(userAcl.CanExecute);
	}

	[Fact]
	public async Task Acl_get_with_upn() {
		string path = StoragePath.Combine(Filesystem, Guid.NewGuid().ToString());
		string userId = _settings.AzureDataLakeOperatorObjectId;

		//write something
		await _storage.SetText(path, "perm?");

		//check that user has no permissions
		AccessControl access = await _storage.GetAccessControl(path, true);
		Assert.DoesNotContain(access.Acl, x => x.Identity == userId);

		//assign user a write permission
		access.Acl.Add(new AclEntry(ObjectType.User, userId, false, true, false));
		await _storage.SetAccessControl(path, access);

		//check user has permissions now
		access = await _storage.GetAccessControl(path, true);
		Assert.True(access.Acl.First().Identity.Contains('@'));
	}

	[Fact]
	public async Task Acl_assign_non_default_permisssions_to_directory_for_user() {
		string directoryPath = StoragePath.Combine(Filesystem, "aclnondefault");
		string filePath = StoragePath.Combine(directoryPath, Guid.NewGuid().ToString());
		string userId = _settings.AzureDataLakeOperatorObjectId;

		//write something
		await _storage.SetText(filePath, "perm?");

		//check that user has no permissions
		AccessControl access = await _storage.GetAccessControl(directoryPath);
		Assert.DoesNotContain(access.Acl, x => x.Identity == userId);

		//assign user a write permission
		access.Acl.Add(new AclEntry(ObjectType.User, userId, false, true, false));
		await _storage.SetAccessControl(directoryPath, access);

		//check user has permissions now
		access = await _storage.GetAccessControl(directoryPath);
		AclEntry userAcl = access.Acl.First(e => e.Identity == userId);
		Assert.False(userAcl.CanRead);
		Assert.True(userAcl.CanWrite);
		Assert.False(userAcl.CanExecute);
	}

	[Fact]
	public async Task Acl_assign_default_permisssions_to_directory_for_user() {
		string directoryPath = StoragePath.Combine(Filesystem, "acldefault");
		string filePath = StoragePath.Combine(directoryPath, Guid.NewGuid().ToString());
		string userId = _settings.AzureDataLakeOperatorObjectId;

		//write something
		await _storage.SetText(filePath, "perm?");

		//check that user has no permissions
		AccessControl access = await _storage.GetAccessControl(directoryPath);
		Assert.DoesNotContain(access.Acl, x => x.Identity == userId);

		//assign user a write permission
		access.DefaultAcl.Add(new AclEntry(ObjectType.User, userId, false, true, false));
		await _storage.SetAccessControl(directoryPath, access);

		//check user has permissions now
		access = await _storage.GetAccessControl(directoryPath);
		AclEntry userAcl = access.DefaultAcl.First(e => e.Identity == userId);
		Assert.False(userAcl.CanRead);
		Assert.True(userAcl.CanWrite);
		Assert.False(userAcl.CanExecute);
	}

	[Fact]
	public async Task Acl_assign_non_default_permisssions_to_filesystem_for_user() {
		string filesystem = "aclnondefault";
		string userId = _settings.AzureDataLakeOperatorObjectId;

		//create filesystem
		await _storage.CreateFilesystem(filesystem);

		//check that user has no permissions
		AccessControl access = await _storage.GetAccessControl(filesystem);
		Assert.DoesNotContain(access.Acl, x => x.Identity == userId);

		//assign user a write permission
		access.Acl.Add(new AclEntry(ObjectType.User, userId, false, true, false));
		await _storage.SetAccessControl(filesystem, access);

		//check user has permissions now
		access = await _storage.GetAccessControl(filesystem);

		//delete filesystem
		await _storage.DeleteFilesystem(filesystem);

		AclEntry userAcl = access.Acl.First(e => e.Identity == userId);
		Assert.False(userAcl.CanRead);
		Assert.True(userAcl.CanWrite);
		Assert.False(userAcl.CanExecute);
	}

	[Fact]
	public async Task Acl_assign_default_permisssions_to_filesystem_for_user() {
		string filesystem = "acldefault";
		string userId = _settings.AzureDataLakeOperatorObjectId;

		//create filesystem
		await _storage.CreateFilesystem(filesystem);

		//check that user has no permissions
		AccessControl access = await _storage.GetAccessControl(filesystem);
		Assert.DoesNotContain(access.Acl, x => x.Identity == userId);

		//assign user a write permission
		access.DefaultAcl.Add(new AclEntry(ObjectType.User, userId, false, true, false));
		await _storage.SetAccessControl(filesystem, access);

		//check user has permissions now
		access = await _storage.GetAccessControl(filesystem);

		//delete filesystem
		await _storage.DeleteFilesystem(filesystem);

		AclEntry userAcl = access.DefaultAcl.First(e => e.Identity == userId);
		Assert.False(userAcl.CanRead);
		Assert.True(userAcl.CanWrite);
		Assert.False(userAcl.CanExecute);
	}

	public async Task InitializeAsync() {
		//drop all blobs in test storage
		List<StoreObject> topLevel =
			(await _storage.ListDirectory(Filesystem, false)).ToList();

		try {
			await _storage.DeleteObjects(topLevel.Select(f => f.FullPath));
		}
		catch {
			//suppress exception to resume test attempt
		}
	}

	public Task DisposeAsync() {
		return Task.CompletedTask;
	}
}