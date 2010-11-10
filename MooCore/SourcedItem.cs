namespace Kayateia.Climoo.MooCore {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Used to carry "source" information about a particular item.
/// </summary>
public class SourcedItem<T> where T:class {
	public SourcedItem(Mob source, string name, T item) {
		_src = source;
		_name = name;
		_item = item;
	}

	public Mob source { get { return _src; } }
	public string name { get { return _name; } }
	public T item { get { return _item; } }

	readonly Mob _src;
	readonly string _name;
	readonly T _item;
}

}
