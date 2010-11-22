namespace Kayateia.Climoo.Tasks {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

/// <summary>
/// Wrapper class that handles feeding input into an active task. This
/// basically just exposes an IEnumerable method that spits back the lines
/// pushed in with the inputPush() method.
/// </summary>
public class TaskFeeder {
	/// <summary>
	/// Adds a single chunk of input to the buffer.
	/// </summary>
	public void inputPush(string text) {
		lock (_mutex) {
			_pending.Enqueue(text);
			_pendingEvent.Set();
		}
	}

	/// <summary>
	/// Implements an IEnumerable<string> of incoming input lines, terminated by a null line.
	/// </summary>
	public IEnumerable<string> inputQueue {
		get {
			string rv = null;
			do {
				_pendingEvent.WaitOne();
				lock (_mutex) {
					// Try again .. thread contention issue.
					if (_pending.Count == 0)
						continue;

					rv = _pending.Dequeue();
					_pendingEvent.Reset();
				}
				if (rv != null)
					yield return rv;
			} while (rv != null);
		}
	}

	/// <summary>
	/// Clears out the input queue, if anything was in it. This is good to do before
	/// switching the feeder to a new task.
	/// </summary>
	public void clearQueue() {
		lock (_mutex) {
			_pending.Clear();
			_pendingEvent.Reset();
		}
	}

	object _mutex = new object();
	Queue<string> _pending = new Queue<string>();
	ManualResetEvent _pendingEvent = new ManualResetEvent(false);
}

}
