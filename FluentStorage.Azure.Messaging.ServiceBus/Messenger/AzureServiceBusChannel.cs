using System;

namespace FluentStorage.Azure.Messaging.ServiceBus.Messenger;

internal class AzureServiceBusChannel {
	internal const string TopicPrefix = "t/";
	internal const string QueuePrefix = "q/";

	internal AzureServiceBusChannel(bool isQueue, string name, string subscription) {
		IsQueue      = isQueue;
		Name         = name;
		Subscription = subscription;
	}

	internal bool   IsQueue      { get; }
	internal string Name         { get; }
	internal string Subscription { get; }
}

internal static class AzureServiceBusChannelExtensions {
	internal static AzureServiceBusChannel ToServiceBusChannel(this string channelName) {

		if (channelName.StartsWith(AzureServiceBusChannel.QueuePrefix)) {
			var queue = channelName.Substring(2);
			return new AzureServiceBusChannel(true, queue, "");
		}

		if (channelName.StartsWith(AzureServiceBusChannel.TopicPrefix)) {
			var topic   = channelName.Substring(2);
			var arrPath = topic.Split('/');
			if (arrPath.Length == 1)
				return new AzureServiceBusChannel(false, topic, "");

			if (arrPath.Length != 2) {
				throw new ArgumentException(
					$"Channel '{channelName}' is not a valid subscription name. It should start with '{AzureServiceBusChannel.TopicPrefix}' and then /<subscription name>. e.g. topicTest/subScriptionTest",
					nameof(channelName));
			}

			return new AzureServiceBusChannel(false, arrPath[0], arrPath[1]);
		}

		throw new ArgumentException(
			$"Channel '{channelName}' is not a valid channel name. It should start with '{AzureServiceBusChannel.QueuePrefix}' for queues, '{AzureServiceBusChannel.TopicPrefix}' for topics and subscriptions",
			nameof(channelName));
	}
}
