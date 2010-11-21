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

	public override void execute(IEnumerable<string> input) {
		IEnumerator<string> enumer = input.GetEnumerator();
		while (enumer.MoveNext()) {
			var inp = enumer.Current;
			if (inp != null)
				_context.outputPush(inp);
			else
				break;
		}
	}
}

}
