namespace Kayateia.Climoo
{
using System;

/// <summary>
/// Wrap an IDisposable object so that it can be used transaction-style. Use
/// this object in the actual using() clause, and then extract your object from
/// it when you know you don't want to dispose it.
/// </summary>
public class RecoverableUsing<T> : IDisposable
	where T : IDisposable
{
	public RecoverableUsing( T obj )
	{
		_obj = obj;
	}

	public void Dispose()
	{
		if( _obj != null )
			_obj.Dispose();
	}

	public T save()
	{
		var rv = _obj;
		_obj = default(T);
		return rv;
	}

	T _obj;
}

}
