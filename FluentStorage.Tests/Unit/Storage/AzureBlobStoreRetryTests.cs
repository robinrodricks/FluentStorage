using Azure;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Storage.Blobs;

using FluentStorage.Azure.Blobs;

namespace FluentStorage.Tests.Unit.Storage;

/// <summary>
/// Unit tests for retrying an Azure Blob upload made through <c>SetObject</c>. No storage account is used:
/// a pipeline policy stands in for the network, sending each request's body and then failing the way a reset
/// connection does, which the Azure SDK retries.
/// </summary>
public sealed class AzureBlobStoreRetryTests {

	private const string ConnectionString =
		"DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;" +
		"AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;" +
		"BlobEndpoint=http://127.0.0.1:1/devstoreaccount1;";

	/// <summary>
	/// Verifies that an upload from a stream that cannot seek - a network stream, or a request body - is sent,
	/// and that every retry resends the whole content. Before, the stream was wrapped in one claiming it could
	/// seek, so the SDK asked for its length, which such a stream cannot give, and nothing was sent at all.
	/// </summary>
	[Fact]
	public async Task SetObject_WithNonSeekableStream_SendsTheContentOnEveryAttempt() {
		byte[] content = Enumerable.Range(0, 200_000).Select(i => (byte)i).ToArray();
		var network = new FailEveryUpload();

		IAzureBlobStore store = AzureBlobStorage.FromClient(Client(network, maxRetries: 2));

		await Assert.ThrowsAnyAsync<Exception>(() => store.SetObject("container/file", new ForwardOnlyStream(content)));

		Assert.Equal(3, network.Bodies.Count);
		Assert.All(network.Bodies, body => Assert.Equal(content, body));
	}

	/// <summary>
	/// Verifies the same for a stream that can seek.
	/// </summary>
	[Fact]
	public async Task SetObject_WithSeekableStream_RetriesWithTheSameBytes() {
		byte[] content = Enumerable.Range(0, 200_000).Select(i => (byte)i).ToArray();
		var network = new FailEveryUpload();

		IAzureBlobStore store = AzureBlobStorage.FromClient(Client(network, maxRetries: 2));

		await Assert.ThrowsAnyAsync<Exception>(() => store.SetObject("container/file", new MemoryStream(content)));

		Assert.Equal(3, network.Bodies.Count);
		Assert.All(network.Bodies, body => Assert.Equal(content, body));
	}

	private static BlobServiceClient Client(HttpPipelinePolicy network, int maxRetries) {
		var options = new BlobClientOptions();
		options.Retry.MaxRetries = maxRetries;
		options.Retry.Delay = TimeSpan.FromMilliseconds(1);
		options.AddPolicy(network, HttpPipelinePosition.PerRetry);
		return new BlobServiceClient(ConnectionString, options);
	}

	/// <summary>
	/// Records the body of every upload request and then fails it as a reset connection would. Any other
	/// request - the container lookup - succeeds, so the upload is reached. Nothing reaches a server.
	/// </summary>
	private sealed class FailEveryUpload : HttpPipelinePolicy {
		public List<byte[]> Bodies { get; } = new();

		public override async ValueTask ProcessAsync(HttpMessage message, ReadOnlyMemory<HttpPipelinePolicy> pipeline) {
			if (message.Request.Method == RequestMethod.Put && message.Request.Content is { } body) {
				var sent = new MemoryStream();
				await body.WriteToAsync(sent, message.CancellationToken);
				Bodies.Add(sent.ToArray());
				throw new IOException("Simulated: the connection was reset after the body was sent");
			}

			message.Response = new OkResponse();
		}

		public override void Process(HttpMessage message, ReadOnlyMemory<HttpPipelinePolicy> pipeline) =>
			throw new NotSupportedException("SetObject uses the asynchronous pipeline");
	}

	private sealed class OkResponse : Response {
		public override int Status => 200;
		public override string ReasonPhrase => "OK";
		public override Stream ContentStream { get; set; } = new MemoryStream();
		public override string ClientRequestId { get; set; } = "";
		public override void Dispose() { }
		protected override bool ContainsHeader(string name) => false;
		protected override IEnumerable<HttpHeader> EnumerateHeaders() => [];
		protected override bool TryGetHeader(string name, out string value) { value = null; return false; }
		protected override bool TryGetHeaderValues(string name, out IEnumerable<string> values) { values = null; return false; }
	}

	/// <summary>
	/// A readable stream that cannot seek, like a network stream.
	/// </summary>
	private sealed class ForwardOnlyStream(byte[] content) : Stream {
		private readonly MemoryStream _inner = new(content);

		public override bool CanRead => true;
		public override bool CanSeek => false;
		public override bool CanWrite => false;
		public override long Length => throw new NotSupportedException();
		public override long Position {
			get => throw new NotSupportedException();
			set => throw new NotSupportedException();
		}

		public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);
		public override void Flush() { }
		public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
		public override void SetLength(long value) => throw new NotSupportedException();
		public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
	}
}
