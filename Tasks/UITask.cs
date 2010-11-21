namespace Kayateia.Climoo.Tasks {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

/// <summary>
/// User Interaction Task:
/// Represents a bit of "scripted" logic for interacting with the user. The
/// task is launched into its own thread which asks an IEnumerable for
/// the next line of user input. This IEnumerable blocks until it actually
/// has a new line of input for the task. If this ever returns null, it is
/// a signal for the task to end immediately.
/// 
/// UITasks are only ever attached to one user context.
/// </summary>
public abstract class UITask {
	public UITask(Session.UserContext context) {
		_context = context;
	}

	/// <summary>
	/// Will be executed in a separate thread as a coherent, linear stream
	/// of execution (no "states" or "events" or anything). The task should
	/// return from this method any time 'input' yields null.
	/// </summary>
	public abstract void execute(IEnumerable<string> input);

	/// <summary>
	/// Begins the task, fed by the specified task feeder.
	/// </summary>
	public void beginTask(TaskFeeder feeder) {
		_thread = new Thread(new ThreadStart(() => {
			this.execute(feeder.inputQueue);
		}));
		_thread.Start();
	}

	/// <summary>
	/// Waits for the task we're attached to to finish. Before calling this,
	/// you should make sure the task's input feeder has a null for it.
	/// </summary>
	public void joinTask() {
		_thread.Join();
		_thread = null;
	}

	protected Session.UserContext _context;

	Thread _thread;
}

}
