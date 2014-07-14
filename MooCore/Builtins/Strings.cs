namespace Kayateia.Climoo.MooCore.Builtins
{
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Provides some utilities to scripts for dealing with strings. This is here
/// because S# can't natively create arrays with non-object types without some
/// nasty and cumbersome syntax, so this makes it much simpler.
/// </summary>
public static class Strings
{
	static public string[] Split( string s, object[] delims )
	{
		if( delims.All( i => i is char ) )
			return s.Split( (delims.Select( i => (char)i )).ToArray() );
		else if( delims.All( i => i is string ) )
			return s.Split( (delims.Select( i => (string)i )).ToArray(), StringSplitOptions.None );
		else
			throw new ArgumentException( "Arguments to 'delims' are not all the same type, or aren't chars/strings." );
	}

	static public string[] Split( string s, object[] delims, int maxCount )
	{
		if( delims.All( i => i is char ) )
			return s.Split( (delims.Select( i => (char)i )).ToArray(), maxCount );
		else if( delims.All( i => i is string ) )
			return s.Split( (delims.Select( i => (string)i )).ToArray(), maxCount, StringSplitOptions.None );
		else
			throw new ArgumentException( "Arguments to 'delims' are not all the same type, or aren't chars/strings." );
	}
}

}
