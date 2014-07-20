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

namespace Kayateia.Climoo.MooCore.Proxies
{
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Shared utils for dealing with proxies. These are helpful independent
/// of what kind of proxy... Ergo... ;)
/// </summary>
public class Proxy
{
	/// <summary>
	/// "Proxifies" an object. This looks for arrays as well as normal types, and replaces them
	/// with proxies in the output if that would be called for.
	/// </summary>
	/// <remarks>
	/// This is for going from either persisted types or regular MooCore objects to in-game wrappers. 
	/// </remarks>
	static public object Proxify( object o, World w, Player p )
	{
		// We handle null here.
		if( o == null )
			return o;

		// If it's already proxy, don't bother.
		if( o is IProxy )
			return o;

		Type t = o.GetType();

		// Check for other item types.
		if( t.IsPrimitive || o is string )
			return o;
		else if( t.IsArray )
			return ((Array)o).OfType<object>().Select( i => Proxy.Proxify( i, w, p ) ).ToArray();
		else
		{
			// Resort to calling items until we find one that fits.
			foreach( Type pt in s_proxies )
			{
				var m = pt.GetMethod( "Proxify" );
				if( m != null )
				{
					object r = m.Invoke( null, new object[] { o, w, p } );
					if( r != null )
						return r;
				}
			}
		}

		// We don't know what to do with this.
		throw new ArgumentException( "Unable to proxify: {0}".FormatI( o ) );
	}

	/// <summary>
	/// "Deproxifies" an object. This looks for arrays as well as normal types, and replaces any
	/// proxies in the output if that would be called for.
	/// </summary>
	/// <remarks>
	/// This is for going from in-game wrappers to persisted types. If it's possible to represent
	/// an object by a MooCore type or a persisted type (e.g. Mob vs Mob.Ref), prefer the latter,
	/// since this is basically used to convert attribute values.
	/// </remarks>
	static public object Deproxify( object o )
	{
		// We handle null here.
		if( o == null )
			return o;

		Type t = o.GetType();

		// Check for other item types.
		if( t.IsPrimitive || o is string )
			return o;
		else if( t.IsArray )
			return ((Array)o).OfType<object>().Select( i => Proxy.Deproxify( i ) ).ToArray();
		else if( o is IProxy )
		{
			return ((IProxy)o).deproxify();
		}
		else
		{
			throw new ArgumentException( "Unable to deproxify: {0}".FormatI( o ) );
		}
	}

	static Proxy()
	{
		s_proxies = typeof( Proxy ).Assembly.GetTypes()
			.Where( t => t.GetInterfaces()
					.Any( i => i == typeof( IProxy ) )
			).ToArray();
	}

	// Cached list of all proxy object types.
	static Type[] s_proxies = null;
}

/// <summary>
/// Interface that mostly just marks classes as proxies. We'll also use this method to convert
/// from proxies back to non-proxies.
/// </summary>
/// <remarks>
/// In addition to this, there is an implicit static member interface of "Proxify" that takes
/// a non-proxy item and turns it into a proxy:
/// 
/// object Proxify( object o, World w, Player p );
/// </remarks>
public interface IProxy
{
	object deproxify();
}

}
