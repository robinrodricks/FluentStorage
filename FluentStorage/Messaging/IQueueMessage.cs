using System;
using System.Collections.Generic;
using System.Text;

namespace FluentStorage.Messaging;

public interface IQueueMessage {
		/// Message ID
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// Gets the count of how many time this message was dequeued
		/// </summary>
		public int DequeueCount { get; set; }

		/// <summary>
		/// When present, indicates time when this message becomes visible again
		/// </summary>
		public DateTimeOffset? NextVisibleTime { get; set; }

		/// <summary>
		/// Message content as string
		/// </summary>
		public string StringContent { get; set; }

		/// <summary>
		/// Message content as byte array
		/// </summary>
		public byte[] Content { get; set; }

		/// <summary>
		/// Extra properties for this message
		/// </summary>
		public Dictionary<string, string> Properties { get; }

		/// <summary>
		/// Clones the message
		/// </summary>
		/// <returns></returns>
		public QueueMessage Clone();

		/// <summary>
		/// Extremely compact binary representation of the message.
		/// It's library specific, therefore try not to use it if portability is required.
		/// </summary>
		/// <returns></returns>
		public byte[] ToByteArray();
	}
