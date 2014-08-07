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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

using Newtonsoft.Json;

/// <summary>
/// Utility methods for converting object graphs to and from JSON.
/// </summary>
public class JsonPersistence
{
	// This known types array should be filled up with anything of ours that we need to
	// persist, because we need to give them namespace agnostic names.
	static Type[] KnownTypes = new Type[]
	{
		typeof( Mob.Ref ),
		typeof( Perm )
	};
	static TypeNameSerializationBinder Binder = new TypeNameSerializationBinder();
	static JsonSerializerSettings SerializerSettings = new JsonSerializerSettings();

	static JsonPersistence()
	{
		foreach( Type t in KnownTypes )
			Binder.Map( t, MapTypeToSafeName( t ) );
		SerializerSettings.Binder = Binder;

		// We default to tagging everything that isn't completely obvious, because the point
		// of using JSON to begin with is to get away from binary blobs in the data files, not
		// so much to interoperate with random REST services or something.
		SerializerSettings.TypeNameHandling = TypeNameHandling.All;
	}

	/// <summary>
	/// Maps a CLR type to a "safe" name, which is either its CLR full name, or
	/// the type's DataContract specified name.
	/// </summary>
	static public string MapTypeToSafeName( Type t )
	{
		var attrs = t.GetCustomAttributes( typeof( DataContractAttribute ), true );
		if( attrs.Length > 0 )
		{
			DataContractAttribute attr = attrs[0] as DataContractAttribute;
			return "{0}:{1}".FormatI( attr.Namespace, attr.Name );
		}
		else
			return t.FullName;
	}

	/// <summary>
	/// Maps a "safe" name, which is either its CLR full name, or its DataContract
	/// specified name, to a CLR type.
	/// </summary>
	static public Type MapSafeNameToType( string name )
	{
		// If it has colons, it's probably a DataContract name.
		if( name.Contains( ':' ) )
		{
			// Look at all the "known" types for a match.
			foreach( Type t in KnownTypes )
			{
				// We can assume these are data contracted.
				DataContractAttribute attr = t.GetCustomAttributes( typeof( DataContractAttribute ), true )[0] as DataContractAttribute;
				string attrName = "{0}:{1}".FormatI( attr.Namespace, attr.Name );
				if( attrName == name )
					return t;
			}
		}

		// Try it the other way.
		return GetTypeEx( name );
	}

	/// <summary>
	/// Converts a JSON string into an object.
	/// </summary>
	static public object Deserialize( Type t, string json )
	{
		object obj = JsonConvert.DeserializeObject( json, t, SerializerSettings );

		// This is just a double-check -- we should never produce raw JSON LINQ objects.
		if( obj.GetType().FullName.StartsWithI( "newtonsoft" ) )
			throw new ArgumentException( "Improperly deserialized JSON" );

		return obj;
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
		return JsonConvert.SerializeObject( o, SerializerSettings );
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

	// Simple serialization binder that allows us to substitute type lookups as the
	// MS DataContractJsonSerializer can do (and which Mono's cannot).
	//
	// http://stackoverflow.com/a/15881570
	class TypeNameSerializationBinder : SerializationBinder
	{
		readonly Dictionary<Type, string> _typeToName = new Dictionary<Type, string>();
		readonly Dictionary<string, Type> _nameToType = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

		// Adds a new name/type mapping.
		public void Map(Type type, string name)
		{
			_typeToName.Add(type, name);
			_nameToType.Add(name, type);
		}

		// Converts a type into a name.
		public override void BindToName( Type serializedType, out string assemblyName, out string typeName )
		{
			string name;
			if( _typeToName.TryGetValue( serializedType, out name ) )
			{
				assemblyName = null;
				typeName = name;
			}
			else
			{
				assemblyName = serializedType.Assembly.FullName;
				typeName = serializedType.FullName;                
			}
		}

		// Converts a name into a type.
		public override Type BindToType( string assemblyName, string typeName )
		{
			if( assemblyName == null )
			{
				Type type;
				if( _nameToType.TryGetValue( typeName, out type ) )
				{
					return type;
				}
			}
			return Type.GetType( string.Format( "{0}, {1}", typeName, assemblyName ), true );
		}
	}
}

}
