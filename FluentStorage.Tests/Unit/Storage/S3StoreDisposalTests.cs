using System.Net;
using System.Net.Http;
using Amazon.Runtime;
using Amazon.S3;
using FluentStorage.AWS.Storage;

namespace FluentStorage.Tests.Unit.Storage;

public class S3StoreDisposalTests {

	[Fact]
	public async Task Dispose_ReleasesTheSdkHttpClientAfterUse() {
		using var transport = new RecordingHttpClient();
		var factory = new PerClientHttpClientFactory(transport);
		using var store = CreateStore(factory);

		await store.GetClient();

		Assert.Equal(1, factory.CreateCount);
		Assert.Equal(1, transport.Handler.AclRequestCount);
		Assert.Equal(0, transport.DisposeCount);
		Assert.Equal(0, transport.Handler.DisposeCount);

		store.Dispose();

		Assert.Equal(1, transport.DisposeCount);
		Assert.Equal(1, transport.Handler.DisposeCount);
	}

	[Fact]
	public async Task Dispose_CanBeRepeatedWithoutReleasingTheClientAgain() {
		using var transport = new RecordingHttpClient();
		using var store = CreateStore(new PerClientHttpClientFactory(transport));
		await store.GetClient();

		store.Dispose();
		store.Dispose();
		store.Dispose();

		Assert.Equal(1, transport.DisposeCount);
		Assert.Equal(1, transport.Handler.DisposeCount);
	}

	[Fact]
	public void Dispose_DoesNotInitializeTheBucket() {
		using var transport = new RecordingHttpClient();
		using var store = CreateStore(new PerClientHttpClientFactory(transport));

		store.Dispose();
		store.Dispose();

		Assert.Equal(0, transport.Handler.AclRequestCount);
	}

	private static S3Store CreateStore(HttpClientFactory factory) {
		return new S3Store("access-key", "secret-key", null, "test-bucket", new AmazonS3Config {
			ServiceURL = "https://s3.test.invalid",
			AuthenticationRegion = "us-east-1",
			ForcePathStyle = true,
			HttpClientFactory = factory,
			MaxErrorRetry = 0,
			Timeout = TimeSpan.FromSeconds(5)
		});
	}

	private sealed class PerClientHttpClientFactory : HttpClientFactory {
		private readonly HttpClient _client;
		private int _createCount;

		public PerClientHttpClientFactory(HttpClient client) {
			_client = client;
		}

		public int CreateCount => Volatile.Read(ref _createCount);

		public override HttpClient CreateHttpClient(IClientConfig clientConfig) {
			Interlocked.Increment(ref _createCount);
			return _client;
		}

		public override bool UseSDKHttpClientCaching(IClientConfig clientConfig) => true;

		public override bool DisposeHttpClientsAfterUse(IClientConfig clientConfig) => false;

		// A null key gives this SDK client its own cache, released when that client is disposed.
		public override string GetConfigUniqueString(IClientConfig clientConfig) => null;
	}

	private sealed class RecordingHttpClient : HttpClient {
		private int _disposeCount;

		public RecordingHttpClient() : this(new BucketExistsHandler()) {
		}

		private RecordingHttpClient(BucketExistsHandler handler) : base(handler) {
			Handler = handler;
		}

		public BucketExistsHandler Handler { get; }

		public int DisposeCount => Volatile.Read(ref _disposeCount);

		protected override void Dispose(bool disposing) {
			if (disposing) {
				Interlocked.Increment(ref _disposeCount);
			}
			base.Dispose(disposing);
		}
	}

	private sealed class BucketExistsHandler : HttpMessageHandler {
		private int _aclRequestCount;
		private int _disposeCount;

		public int AclRequestCount => Volatile.Read(ref _aclRequestCount);

		public int DisposeCount => Volatile.Read(ref _disposeCount);

		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
			Assert.Equal(HttpMethod.Get, request.Method);
			Assert.Equal("s3.test.invalid", request.RequestUri.Host);
			Assert.Equal("/test-bucket/", request.RequestUri.AbsolutePath);
			Assert.Contains("acl", request.RequestUri.Query);
			Interlocked.Increment(ref _aclRequestCount);
			var response = new HttpResponseMessage(HttpStatusCode.OK) {
				Content = new StringContent(
					"<AccessControlPolicy xmlns=\"http://s3.amazonaws.com/doc/2006-03-01/\"><Owner><ID>test-owner</ID></Owner><AccessControlList /></AccessControlPolicy>",
					System.Text.Encoding.UTF8,
					"application/xml")
			};
			response.Headers.Add("x-amz-bucket-region", "us-east-1");
			return Task.FromResult(response);
		}

		protected override void Dispose(bool disposing) {
			if (disposing) {
				Interlocked.Increment(ref _disposeCount);
			}
			base.Dispose(disposing);
		}
	}
}
