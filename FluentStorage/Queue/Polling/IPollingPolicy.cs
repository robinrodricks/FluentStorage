using System;

namespace FluentStorage.Queue.Polling;

interface IPollingPolicy {
	void Reset();

	TimeSpan GetNextDelay();
}