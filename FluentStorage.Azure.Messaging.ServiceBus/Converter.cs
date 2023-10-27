using System;
using System.Collections.Generic;
using Azure.Messaging.ServiceBus;
using FluentStorage.Messaging;
using QueueMessage = FluentStorage.Messaging.QueueMessage;

namespace FluentStorage.Azure.Messaging.ServiceBus {
	static class Converter {
		public static ServiceBusMessage ToMessage(QueueMessage message) {
			if (message == null)
				throw new ArgumentNullException(nameof(message));

			var result = new ServiceBusMessage(message.Content);
			if (message.Id != null) {
				result.MessageId = message.Id;
			}
			if (message.Properties != null && message.Properties.Count > 0) {
				foreach (KeyValuePair<string, string> prop in message.Properties) {
					result.ApplicationProperties.Add(prop.Key, prop.Value);
				}
			}
			return result;
		}

		public static QueueMessage ToQueueMessage(ServiceBusReceivedMessage message) {
			string id = message.MessageId;

			var result = new QueueMessage(id, message.Body.ToArray());
			result.DequeueCount = message.DeliveryCount;
			if (message.ApplicationProperties != null && message.ApplicationProperties.Count > 0) {
				foreach (KeyValuePair<string, object> pair in message.ApplicationProperties) {
					result.Properties[pair.Key] = pair.Value?.ToString();
				}
			}

			return result;
		}
	}
}
