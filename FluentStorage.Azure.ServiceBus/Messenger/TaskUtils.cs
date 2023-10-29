using System;
using System.Threading;
using System.Threading.Tasks;

namespace FluentStorage.Azure.ServiceBus.Messenger;

static class TaskUtils {
	private static readonly TaskFactory _myTaskFactory = new
		TaskFactory(CancellationToken.None,
			TaskCreationOptions.None,
			TaskContinuationOptions.None,
			TaskScheduler.Default);

	internal static void RunSync(Func<Task> func) {
		_myTaskFactory
			.StartNew(func)
			.Unwrap()
			.GetAwaiter()
			.GetResult();
	}
}
