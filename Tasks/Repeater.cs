namespace Kayateia.Climoo.Tasks {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Really simple test UITask that just repeats what the user types.
/// </summary>
public class Repeater : UITask {
	public Repeater(Session.UserContext context) : base(context) {
	}

	protected override IEnumerable<Result> runTask() {
		for ( ; ; ) {
			yield return Result.GetInput();
			_context.outputPush(_input);
		}
	}
}

}
