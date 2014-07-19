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

namespace Kayateia.Climoo.MooCore
{
using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;

public class JsonPersistence
{
	static Type[] KnownTypes = new Type[]
	{
		typeof( Mob.Ref ),
		typeof( Perm )
	};

	/// <summary>
	/// Converts a JSON string into an object.
	/// </summary>
	static public object Deserialize( Type t, string json )
	{
		byte[] bytes = System.Text.Encoding.UTF8.GetBytes( json );
		var stream = new MemoryStream( bytes );
		var serializer = new DataContractJsonSerializer( t, KnownTypes );
		return serializer.ReadObject( stream );
	}

	/// <summary>
	/// Converts a JSON string into a typed object.
	/// </summary>
	static public T Deserialize<T>( string json )
	{
		return (T)Deserialize( typeof( T ), json );
	}

	/// <summary>
	/// Converts an object into a JSON string.
	/// </summary>
	static public string Serialize( object o )
	{
		// Unfortunately because we're using object[] to be compatible with S#, we have to
		// specify all types here that might be touched by serialization inside arrays. =_=
		// So in addition to specifying DataContract on them, you must list them here.
		var serializer = new DataContractJsonSerializer( o.GetType(), KnownTypes );
		var stream = new MemoryStream();
		serializer.WriteObject( stream, o );
		byte[] output = stream.ToArray();
		return System.Text.Encoding.UTF8.GetString( output );
	}

	/// <summary>
	/// Returns the Type for a type short name, if it's been loaded into the AppDomain.
	/// </summary>
	/// <remarks>
	/// This is only tangentially related, but it's used elsewhere in CliMOO in JSON serialization.
	/// 
	/// http://stackoverflow.com/a/7286354
	/// </remarks>
	static public Type GetTypeEx( string fullTypeName )
	{
		return Type.GetType( fullTypeName ) ??
				AppDomain.CurrentDomain.GetAssemblies()
						.Select( a => a.GetType( fullTypeName ) )
						.FirstOrDefault( t => t != null );
	}
}

}
