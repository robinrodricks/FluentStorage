using Aliyun.OSS;
using Aliyun.OSS.Common;
using FluentStorage.Alibaba.Storage;
using FluentStorage.Storage;

namespace FluentStorage;

/// <summary>
/// Factory methods for creating Alibaba Cloud OSS stores.
/// </summary>
public static class AlibabaStorage {

	/// <summary>
	/// Creates a Alibaba OSS storage provider using standard AccessKey authentication.
	/// </summary>
	public static IStore FromCredentials(string endpoint,string bucketName,string accessKeyId,string accessKeySecret) {

		return new AlibabaStore(endpoint,bucketName,accessKeyId,accessKeySecret);
	}

	/// <summary>
	/// Creates a Alibaba OSS storage provider using AccessKey authentication with custom client configuration.
	/// </summary>
	public static IStore FromCredentials(string endpoint, string bucketName, string accessKeyId,
		string accessKeySecret, ClientConfiguration configuration) {

		return new AlibabaStore(endpoint,bucketName,accessKeyId,accessKeySecret,configuration);
	}

	/// <summary>
	/// Creates a Alibaba OSS storage provider using temporary STS credentials.
	/// </summary>
	public static IStore FromSts(string endpoint,string bucketName,string accessKeyId,
		string accessKeySecret,string securityToken) {

		return new AlibabaStore(endpoint,bucketName,accessKeyId,accessKeySecret,securityToken);
	}

	/// <summary>
	/// Creates a Alibaba OSS storage provider using an existing Aliyun Client.
	/// </summary>
	public static IStore FromClient(OssClient client, string bucketName) {

		return new AlibabaStore(client, bucketName);
	}
}