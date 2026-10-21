using System.IO;
using System.Linq;
using System.Threading;
using FluentStorage.Storage;

namespace FluentStorage.Tests.Unit.Storage;

/// <summary>
/// Unit tests covering concurrent use of the in-memory store (<see cref="StorageFactory.InMemory"/>).
/// </summary>
public class MemoryStoreConcurrencyTests {

	/// <summary>
	/// Verifies that two appends made at the same time are both kept. The test holds each append at the
	/// same point until both have reached it, so they overlap on every run.
	/// </summary>
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public async Task SetObject_TwoConcurrentAppends_KeepsBothAppends(bool targetExists) {
		var store = StorageFactory.InMemory();
		if (targetExists)
			await store.SetObject("a.bin", new MemoryStream(new byte[] { 0 }), append: false);

		using var bothReading = new CountdownEvent(2);
		Task first = Task.Run(() => store.SetObject("a.bin", new WaitForOtherReadersStream(bothReading), append: true));
		Task second = Task.Run(() => store.SetObject("a.bin", new WaitForOtherReadersStream(bothReading), append: true));
		await Task.WhenAll(first, second);

		Assert.Equal(targetExists ? 3 : 2, await store.GetObjectLength("a.bin"));
	}

	/// <summary>
	/// Verifies that another thread can write while a listing is in progress, without breaking the
	/// listing or having to wait for it. The write is made from the listing's
	/// <see cref="StorageListOptions.BrowseFilter"/>, so it always happens mid-listing.
	/// </summary>
	[Fact]
	public async Task ListObjects_WhenAnotherThreadWritesDuringListing_ReturnsWithoutError() {
		var store = StorageFactory.InMemory();
		for (int i = 0; i < 3; i++)
			await store.SetObject($"f/{i}.bin", new MemoryStream(new byte[] { 1 }), append: false);

		bool writerCompleted = false;
		var options = new StorageListOptions {
			Recurse = true,
			BrowseFilter = _ => {
				if (!writerCompleted)
					writerCompleted = Task.Run(() => store.SetObject("f/new.bin", new MemoryStream(new byte[] { 1 }), append: false))
						.Wait(TimeSpan.FromSeconds(2));
				return true;
			}
		};

		await store.ListObjects(options);

		Assert.True(writerCompleted);
	}

	/// <summary>
	/// Verifies that writes to different paths from several threads keep every object. Nothing can
	/// make two writes collide on demand, so this test relies on making many of them.
	/// </summary>
	[Fact]
	public async Task SetObject_ParallelWritesToDistinctPaths_KeepsEveryObject() {
		var store = StorageFactory.InMemory();
		const int count = 1000;

		await Parallel.ForEachAsync(Enumerable.Range(0, count), new ParallelOptions { MaxDegreeOfParallelism = 4 },
			async (i, ct) => await store.SetObject($"f/{i}.bin", new MemoryStream(new byte[] { 1 }), append: false, ct));

		List<StoreObject> objects = await store.ListObjects(new StorageListOptions { Recurse = true });
		Assert.Equal(count, objects.Count);
	}

	/// <summary>
	/// Verifies that object info handed out earlier is not changed by a later write, as with DiskStore,
	/// so a caller can never read a half-updated <see cref="StoreObject.Size"/> and <see cref="StoreObject.MD5"/>.
	/// </summary>
	[Fact]
	public async Task GetObjectInfo_AfterTheObjectIsOverwritten_StillDescribesTheOriginalVersion() {
		var store = StorageFactory.InMemory();
		await store.SetObject("x.bin", new MemoryStream(new byte[] { 1 }), append: false);
		StoreObject info = await store.GetObjectInfo("x.bin");

		await store.SetObject("x.bin", new MemoryStream(new byte[100]), append: false);

		Assert.Equal(1, info.Size);
	}

	/// <summary>
	/// Verifies that where a <see cref="MemoryStream"/> is read from has not changed: an overwrite reads it from the
	/// start, and an append to an existing object reads from its current position.
	/// </summary>
	[Fact]
	public async Task SetObject_MemoryStreamNotAtStart_KeepsThePositionRules() {
		var store = StorageFactory.InMemory();
		var source = new MemoryStream(new byte[] { 1, 2, 3 });

		source.Position = 2;
		await store.SetObject("x.bin", source, append: false);
		source.Position = 2;
		await store.SetObject("x.bin", source, append: true);

		using Stream stored = await store.OpenRead("x.bin");
		Assert.Equal(new byte[] { 1, 2, 3, 3 }, stored.ToByteArray());
	}

	/// <summary>
	/// Verifies that editing an object returned by a listing or <see cref="IStore.GetObjectInfo"/> does not
	/// change the store, as on DiskStore, <see cref="FluentStorage.AWS.Storage.S3Store"/> and <see cref="FluentStorage.Azure.Blobs.Storage.AzureBlobStore"/>, which return new objects on every call. Such an edit would
	/// otherwise happen outside the store's lock.
	/// </summary>
	[Fact]
	public async Task ReturnedObjects_WhenEditedByTheCaller_DoNotChangeTheStore() {
		var store = StorageFactory.InMemory();
		await store.SetObject("x.bin", new MemoryStream(new byte[] { 1 }), append: false);

		(await store.ListObjects(new StorageListOptions())).Single().Metadata["listed"] = "edited";
		(await store.GetObjectInfo("x.bin")).Metadata["info"] = "edited";

		Assert.Empty((await store.GetObjectInfo("x.bin")).Metadata);
	}

	/// <summary>
	/// Verifies that the list of paths passed to <see cref="IStore.GetObjectsInfo"/> is read before the
	/// store is locked. Here, reading the
	/// list waits for a write on another thread, which could never finish if the store were locked.
	/// </summary>
	[Fact]
	public async Task GetObjectsInfo_WhenThePathListIsProducedLazily_DoesNotBlockOtherWriters() {
		var store = StorageFactory.InMemory();
		await store.SetObject("a.bin", new MemoryStream(new byte[] { 1 }), append: false);
		bool writerCompleted = false;

		IEnumerable<string> Paths() {
			yield return "a.bin";
			writerCompleted = Task.Run(() => store.SetObject("b.bin", new MemoryStream(new byte[] { 1 }), append: false))
				.Wait(TimeSpan.FromSeconds(2));
			yield return "b.bin";
		}

		await store.GetObjectsInfo(Paths());

		Assert.True(writerCompleted);
	}

	/// <summary>
	/// Verifies that the stream from <see cref="IStore.OpenRead"/> is read-only, so a caller cannot change
	/// the stored bytes behind the store's back and leave its <see cref="StoreObject.Size"/> and
	/// <see cref="StoreObject.MD5"/> stale.
	/// </summary>
	[Fact]
	public async Task OpenRead_WritingToTheReturnedStream_DoesNotChangeTheStoredObject() {
		var store = StorageFactory.InMemory();
		await store.SetObject("x.bin", new MemoryStream(new byte[] { 1, 1, 1 }), append: false);

		using (Stream stream = await store.OpenRead("x.bin")) {
			Assert.Throws<NotSupportedException>(() => stream.Write(new byte[] { 9, 9, 9 }, 0, 3));
		}

		using Stream reread = await store.OpenRead("x.bin");
		Assert.Equal(new byte[] { 1, 1, 1 }, reread.ToByteArray());
	}

	/// <summary>
	/// A one-byte stream whose first read waits until all the streams sharing its <see cref="CountdownEvent"/>
	/// have reached that read (or 2 seconds have passed). Writes that read these streams therefore overlap
	/// on every run.
	/// </summary>
	private sealed class WaitForOtherReadersStream : Stream {
		private readonly CountdownEvent _allReading;
		private bool _done;

		public WaitForOtherReadersStream(CountdownEvent allReading) {
			_allReading = allReading;
		}

		public override int Read(byte[] buffer, int offset, int count) {
			if (_done)
				return 0;
			_allReading.Signal();
			_allReading.Wait(TimeSpan.FromSeconds(2));
			buffer[offset] = 1;
			_done = true;
			return 1;
		}

		public override bool CanRead => true;
		public override bool CanSeek => false;
		public override bool CanWrite => false;
		public override long Length => throw new NotSupportedException();
		public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
		public override void Flush() { }
		public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
		public override void SetLength(long value) => throw new NotSupportedException();
		public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
	}
}
