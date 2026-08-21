using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentStorage.Model;
using FluentStorage.Storage;

namespace FluentStorage.Sinks;

class SinkedStore : StoreBase {
	private readonly IStore _parent;
	private readonly ITransformSink[] _sinks;

	public SinkedStore(IStore blobStorage, params ITransformSink[] sinks) {
		if (sinks is null)
			throw new ArgumentNullException(nameof(sinks));

		_parent = blobStorage ?? throw new ArgumentNullException(nameof(blobStorage));
		_sinks = sinks;
	}
	public override void Dispose() => _parent.Dispose();


	public override Task DeleteObjects(IEnumerable<string> fullPaths, CancellationToken cancellationToken = default) {
		return _parent.DeleteObjects(fullPaths, cancellationToken);
	}
	public override Task DeleteObject(string fullPaths, CancellationToken cancellationToken = default) {
		return _parent.DeleteObject(fullPaths, cancellationToken);
	}

	public override Task<List<bool>> ObjectsExists(IEnumerable<string> fullPaths, CancellationToken cancellationToken = default) {
		return _parent.ObjectsExists(fullPaths, cancellationToken);
	}
	public override Task<bool> ObjectExists(string fullPath, CancellationToken cancellationToken = default) {
		return _parent.ObjectExists(fullPath, cancellationToken);
	}

	public override Task<StoreObject> GetObjectInfo(string path, CancellationToken cancellationToken = default) {
		return _parent.GetObjectInfo(path, cancellationToken);
	}
	public override Task<List<StoreObject>> GetObjectsInfo(IEnumerable<string> fullPaths, CancellationToken cancellationToken = default) {
		return _parent.GetObjectsInfo(fullPaths, cancellationToken);
	}

	public override Task<List<StoreObject>> ListObjects(StorageListOptions options = null, CancellationToken cancellationToken = default) {
		return _parent.ListObjects(options, cancellationToken);
	}

	public override Task SetObjectInfo(StoreObject obj, CancellationToken cancellationToken = default) {
		return _parent.SetObjectInfo(obj, cancellationToken);
	}

	public override Task SetObjectsInfo(IEnumerable<StoreObject> objs, CancellationToken cancellationToken = default) {
		return _parent.SetObjectsInfo(objs, cancellationToken);
	}

	public override async Task<Stream> OpenRead(string fullPath, CancellationToken cancellationToken = default) {

		//chain streams
		Stream readStream = await _parent.OpenRead(fullPath, cancellationToken).ConfigureAwait(false);

		if (readStream == null)
			return null;

		foreach (ITransformSink sink in _sinks) {
			readStream = sink.OpenReadStream(fullPath, readStream);
		}

		return readStream;
	}

	public override async Task SetObject(string fullPath, Stream dataSourceStream,bool append = false,
		CancellationToken cancellationToken = default) {
		if (dataSourceStream == null)
			return;

		using (var source = new SinkedStream(dataSourceStream, fullPath, _sinks)) {
			await _parent.SetObject(fullPath, source, append, cancellationToken).ConfigureAwait(false);
		}
	}
	public override async Task SetObject(string fullPath, Stream dataSourceStream, string contentType, bool append, CancellationToken cancellationToken = default) {
		if (dataSourceStream == null)
			return;

		using (var source = new SinkedStream(dataSourceStream, fullPath, _sinks)) {
			await _parent.SetObject(fullPath, source, contentType, append, cancellationToken).ConfigureAwait(false);
		}
	}

}