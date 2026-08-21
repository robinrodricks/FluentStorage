using System;

namespace FluentStorage.Azure.Blobs.Utils;

internal static class AzureBlobUtils {

	public static Uri GetServiceUri(string accountName, AzureCloudEnvironment cloudEnvironment = default) {
		return AzureStorageIdentity.CreateBlobServiceUri(accountName, cloudEnvironment);
	}

	public static bool TryParseSasUrl(string url, out string accountName, out string containerName, out string sas) {
		try {
			var u = new Uri(url);

			accountName = u.Host.Substring(0, u.Host.IndexOf('.'));
			containerName = u.Segments.Length == 2 ? u.Segments[1] : null;
			sas = u.Query;

			return true;
		}
		catch {
			accountName = null;
			containerName = null;
			sas = null;
			return false;
		}

	}
}