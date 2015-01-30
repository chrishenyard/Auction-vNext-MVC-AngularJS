using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Auction.Synchronization
{
	// http://blogs.msdn.com/b/pfxteam/archive/2012/02/12/10266983.aspx
	public class AsyncSemaphore {
		private readonly static Task _completed = Task.FromResult(true);
		private readonly Dictionary<string, Queue<TaskCompletionSource<bool>>> _waiterContainer = new Dictionary<string, Queue<TaskCompletionSource<bool>>>();
		private readonly Dictionary<string, int> _waiterContainerCount = new Dictionary<string, int>();
		private int _initialCount;

		public AsyncSemaphore(int initialCount) {
			if (initialCount < 0) throw new ArgumentOutOfRangeException("initialCount");
			_initialCount = initialCount;
		}

		public Dictionary<string, Queue<TaskCompletionSource<bool>>> WaiterContainer {
			get {
				return _waiterContainer;
			}
		}

		public Dictionary<string, int> WaiterContainerCount {
			get {
				return _waiterContainerCount;
			}
		}

		public Task WaitAsync(string key) {
			lock (_waiterContainer) {
				Queue<TaskCompletionSource<bool>> waiterQueue;
				var waiterCount = 0;

				if (!_waiterContainer.ContainsKey(key)) {
					waiterQueue = new Queue<TaskCompletionSource<bool>>();
					waiterCount = _initialCount;
                    _waiterContainer.Add(key, waiterQueue);
					_waiterContainerCount.Add(key, _initialCount);
				}
				else {
					waiterQueue = _waiterContainer[key];
					waiterCount = _waiterContainerCount[key];
				}

				if (waiterCount > 0) {
					--waiterCount;
					_waiterContainerCount[key] = waiterCount;
					return _completed;
				}
				else {
					var waiter = new TaskCompletionSource<bool>();
					waiterQueue.Enqueue(waiter);
					return waiter.Task;
				}
			}
		}

		public void Release(string key) {
			TaskCompletionSource<bool> toRelease = null;
			lock (_waiterContainer) {
				var waiterQueue = _waiterContainer[key];
				var waiterCount = _waiterContainerCount[key];

				if (waiterQueue.Count > 0) {
					toRelease = waiterQueue.Dequeue();
				}
				else {
					++waiterCount;
					_waiterContainerCount[key] = waiterCount;
                }
			}
			if (toRelease != null)
				toRelease.SetResult(true);
		}
	}

	// http://blogs.msdn.com/b/pfxteam/archive/2012/02/12/10266988.aspx
	public class AsyncLock {
		private readonly AsyncSemaphore _semaphore;

		public AsyncLock() {
			_semaphore = new AsyncSemaphore(1);
		}

		public AsyncSemaphore AsyncSemaphore {
			get {
				return _semaphore;
			}
		}

		public Task<Releaser> LockAsync(string key) {
			var wait = _semaphore.WaitAsync(key);
			return wait.IsCompleted ?
				Task.FromResult(new Releaser(this, key)) :
				wait.ContinueWith((_, state) => new Releaser((AsyncLock)state, key),
					this, CancellationToken.None,
					TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
		}

		public struct Releaser : IDisposable {
			private readonly AsyncLock _toRelease;
			private readonly string _key;

			internal Releaser(AsyncLock toRelease, string key) { _toRelease = toRelease; _key = key; }

			public void Dispose() {
				if (_toRelease != null)
					_toRelease._semaphore.Release(_key);
			}
		}
	}
}