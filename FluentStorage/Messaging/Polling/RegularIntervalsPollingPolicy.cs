﻿using System;

namespace FluentStorage.Messaging.Polling {
	class RegularIntervalsPollingPolicy : IPollingPolicy {
		private readonly TimeSpan _interval;

		public RegularIntervalsPollingPolicy(TimeSpan interval) {
			_interval = interval;
		}

		public TimeSpan GetNextDelay() => _interval;

		public void Reset() { }
	}
}
