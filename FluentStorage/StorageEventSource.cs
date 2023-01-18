using System.Diagnostics.Tracing;

namespace FluentStorage {
	/// <summary>
	/// Internal logger
	/// </summary>
	[EventSource(Name = "FluentStorage")]
	public sealed class StorageEventSource : EventSource {
		/// <summary>
		/// Logger instance
		/// </summary>
		public static StorageEventSource Log { get; } = new StorageEventSource();

		/// <summary>
		/// Writes generic debug message
		/// </summary>
		/// <param name="subsystem"></param>
		/// <param name="message"></param>
		[Event(1, Level = EventLevel.Verbose, Message = "{0}: {1}")]
		public void Debug(string subsystem, string message) {
			if (this.IsEnabled()) {
				this.WriteEvent(1, subsystem, message);
			}
		}

		/// <summary>
		/// Writes generic error message
		/// </summary>
		/// <param name="subsystem"></param>
		/// <param name="message"></param>
		[Event(2, Level = EventLevel.Error, Message = "{0}: {1}")]
		public void Error(string subsystem, string message) {
			if (this.IsEnabled()) {
				this.WriteEvent(2, subsystem, message);
			}
		}

	}
}
