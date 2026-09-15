using Azure.Storage.Blobs;
using Azure.Storage.Files.Shares;
using Azure.Core.Pipeline;
using FluentStorage.Azure.Blobs.Storage;
using FluentStorage.Azure.Files.Storage;
using System.Net;
using System.Net.Http;

namespace FluentStorage.Tests.Unit.Storage;

public class AzurePathBaseTests {
	[Fact]
	public async Task BlobPrefix_IsPrependedToBlobPaths() {
		var client = new BlobServiceClient(new Uri("https://account.blob.core.windows.net"));
		var store = new AzureBlobStore(client, "account", containerName: "documents", blobPrefix: "customer-a/archive");

		object task = typeof(AzureBlobStore)
			.GetMethod("GetPartsAsync", BindingFlags.Instance | BindingFlags.NonPublic)
			.Invoke(store, new object[] { "report.csv", false });
		await (Task)task;

		object result = task.GetType().GetProperty("Result").GetValue(task);
		string path = (string)result.GetType().GetField("Item2").GetValue(result);

		path.Should().Be("customer-a/archive/report.csv");
	}

	[Fact]
	public async Task BlobPrefix_IsUsedInTheSdkRequestUrl() {
		var handler = new RecordingHandler();
		var options = new BlobClientOptions { Transport = new HttpClientTransport(new HttpClient(handler)) };
		var client = new BlobServiceClient(new Uri("https://account.blob.core.windows.net"), options);
		var store = new AzureBlobStore(client, "account", containerName: "documents", blobPrefix: "customer-a/archive");

		await store.SetObject("report.csv", new MemoryStream([1]), false);

		handler.RequestUris.Should().Contain(new Uri("https://account.blob.core.windows.net/documents/customer-a/archive/report.csv"));
	}

	[Fact]
	public void DirectoryPath_IsInsertedBelowTheFileShare() {
		var client = new ShareServiceClient(new Uri("https://account.file.core.windows.net"));
		var store = new AzureFilesStore(client, "account", shareName: "documents", directoryPath: "customer-a/archive");

		string[] parts = (string[])typeof(AzureFilesStore)
			.GetMethod("GetStorageParts", BindingFlags.Instance | BindingFlags.NonPublic)
			.Invoke(store, new object[] { "report.csv" });

		parts.Should().Equal("customer-a", "archive", "report.csv");
	}

	private sealed class RecordingHandler : HttpMessageHandler {
		public List<Uri> RequestUris { get; } = new List<Uri>();

		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
			RequestUris.Add(request.RequestUri);
			return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Created));
		}
	}
}
