using System;

namespace FluentStorage.Messaging.Polling {
	interface IPollingPolicy {
		void Reset();

		TimeSpan GetNextDelay();
	}
}
