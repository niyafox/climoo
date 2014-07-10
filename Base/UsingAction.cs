namespace Kayateia.Climoo
{
using System;

/// <summary>
/// Allows running bits of code at dispose and/or non-dispose time.
/// </summary>
public class UsingAction : IDisposable
{
	public UsingAction( Action atDispose, Action atFree = null )
	{
		_atDispose = atDispose;
		_atFree = atFree;
	}

	public void Dispose()
	{
		if( _atDispose != null )
			_atDispose();
		compost();
	}

	/// <summary>
	/// Take-back for the finalization. Call this to call your atFree instead of your atDispose.
	/// </summary>
	public void free()
	{
		if( _atFree != null )
			_atFree();
		compost();
	}

	void compost()
	{
		_atFree = null;
		_atDispose = null;
	}

	Action _atDispose, _atFree;
}

}
