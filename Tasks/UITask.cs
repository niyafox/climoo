/*
	CliMOO - Multi-User Dungeon, Object Oriented for the web
	Copyright (C) 2010-2014 Kayateia

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

namespace Kayateia.Climoo.Tasks {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

/// <summary>
/// Represents a bit of "scripted" logic for interacting with the user.
/// </summary>
/// <remarks>
/// UITasks are only ever attached to one user context, and are not
/// particularly thread safe.
/// 
/// UITask makes use of a peculiar inversion of intended flow control for
/// the C# iterator concept. What we want to do here is inherently state
/// machine friendly, but we want to avoid state machine syntax and
/// heavyweight threads. The only downside is that we can't atomically pass
/// in the new input line into MoveNext().
///
/// Note that since they are now available in newer .NETs, I really ought
/// to convert this stuff over to use Async objects.
/// </remarks>
public abstract class UITask {
	public UITask(Session.UserContext context) {
		_context = context;
	}

	public enum Action {
		NoAction,
		ToGame,
		Output,
		NewTask
	}

	/// <summary>
	/// Returned from each task "push" to determine what this task wants the
	/// task handler to do.
	/// </summary>
	public struct Result {
		public Action action;
		public string output;
		public UITask newTask;

		// Convenience methods.
		static public Result GetInput() { return new Result() { action = Action.NoAction }; }
		static public Result ToGame() { return new Result() { action = Action.ToGame }; }
		static public Result Output(string s) { return new Result() { action = Action.Output, output = s }; }
		static public Result NewTask(UITask t) { return new Result() { action = Action.NewTask, newTask = t }; }
	}

	/// <summary>
	/// Begin execution of the task.
	/// </summary>
	public Result begin() {
		_input = null;
		_task = runTask();
		_taskEnum = _task.GetEnumerator();
		if (!_taskEnum.MoveNext())
			return new Result();
		else
			return _taskEnum.Current;
	}

	/// <summary>
	/// Continue execution of the task with new input. Pass null to gracefully
	/// end this task.
	/// </summary>
	public Result inputPush(string input) {
		try
		{
			_input = input;

			if (!_taskEnum.MoveNext())
				return new Result();
			else
				return _taskEnum.Current;
		}
		catch( Exception )
		{
			// Because exceptions during enumerations abort the enumeration, we must
			// restart it in order to continue processing. Passing the exception onward
			// will let the user see it.
			begin();
			throw;
		}
	}

	/// <summary>
	/// Call to stop a task before it has run to a hand-off point (ToGame or NewTask).
	/// This is not strictly necessary but it lets tasks finish up business if they want.
	/// </summary>
	public virtual void stop() {
	}

	/// <summary>
	/// Will be executed as a coherent, linear stream of execution (no "states"
	/// or "events" or anything). When the task wants to wait on new input, it
	/// should "yield return" with a Result. Immediately after execution resumes,
	/// _input will contain the next input line from the user.
	/// </summary>
	protected abstract IEnumerable<Result> runTask();

	protected Session.UserContext _context;
	protected string _input;

	IEnumerable<Result> _task;
	IEnumerator<Result> _taskEnum;
}

}
