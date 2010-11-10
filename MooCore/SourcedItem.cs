namespace Kayateia.Climoo.MooCore {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Used to carry "source" information about a particular item.
/// </summary>
public class SourcedItem<T> where T:class {
	public SourcedItem(Mob source, T item) {
		_src = source;
		_item = item;
	}

	public Mob source { get { return _src; } }
	public T item { get { return _item; } }

	readonly Mob _src;
	readonly T _item;
}

}
