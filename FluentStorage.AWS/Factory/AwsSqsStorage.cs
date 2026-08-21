using Amazon;
using FluentStorage.AWS.Messaging;
using FluentStorage.Queue;

namespace FluentStorage;

/// <summary>
/// Amazon Web Services SQS factory to create instances of `IQueue` using this provider.
/// </summary>
public static class AwsSqsStorage {

	/// <summary>
	/// Creates Amazon Simple Queue Service publisher
	/// </summary>
	/// <param name="accessKeyId">Access key ID</param>
	/// <param name="secretAccessKey">Secret access key</param>
	public static IQueue PublisherFromCredentials(string accessKeyId,string secretAccessKey,
		string serviceUrl,RegionEndpoint regionEndpoint = null) {

		return new SQSMessenger(accessKeyId, secretAccessKey, serviceUrl, regionEndpoint);
	}

	/// <summary>
	/// Creates Amazon Simple Queue Service Receiver
	/// </summary>
	/// <param name="accessKeyId">Access key ID</param>
	/// <param name="secretAccessKey">Secret access key</param>
	public static IQueueReceiver ReceiverFromCredentials(string accessKeyId, string secretAccessKey,
		string serviceUrl, string queueName, RegionEndpoint regionEndpoint = null) {

		return new SQSMessageReceiver(accessKeyId, secretAccessKey, serviceUrl, queueName, regionEndpoint);
	}

}