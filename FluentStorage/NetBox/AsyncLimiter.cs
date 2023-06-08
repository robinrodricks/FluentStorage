using System;
using System.Threading.Tasks;
using System.Threading;

namespace System {
	/// <summary>
	/// Limit the amount of async tasks that can run at once
	/// </summary>
	public class AsyncLimiter : IDisposable {
		private class LockRelease : IDisposable {
			private readonly AsyncLimiter _parent;

			public LockRelease(AsyncLimiter parent) {
				_parent = parent;
			}

			public void Dispose() {
				_parent._throttler.Release();
			}
		}

		private readonly SemaphoreSlim _throttler;

		/// <summary>
		/// Creates a guard block that limits number of tasks to run at once.
		/// </summary>
		/// <param name="maxTasks"></param>
		/// <exception cref="ArgumentException"></exception>
		public AsyncLimiter(int maxTasks) {
			if (maxTasks < 1) {
				throw new ArgumentException($"there should be at least one task allowed to run, {maxTasks} is invalid", "maxTasks");
			}

			_throttler = new SemaphoreSlim(maxTasks);
		}

		/// <summary>
		/// Call this method to get a lock on the limiter. The call waits asynchronously
		/// until a slot is available. You are responsible for disposing the result in order
		/// to release the lock.
		/// </summary>
		/// <returns></returns>
		public async Task<IDisposable> AcquireOneAsync() {
			await _throttler.WaitAsync();
			return new LockRelease(this);
		}

		public void Dispose() {
			_throttler.Dispose();
		}
	}
}