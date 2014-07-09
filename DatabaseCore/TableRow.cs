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

namespace Kayateia.Climoo.Database
{
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

/// <summary>
/// Base class for tables.
/// </summary>
public abstract class TableRow
{
	static public IEnumerable<string> GetDBColumns( Type t )
	{
		var members = t.GetProperties().Where( m => GetColumnAttr( m ) != null );
		return members.Select( m => GetColumnAttr( m ).Name ?? m.Name );
	}

	static public IEnumerable<string> GetColumns( Type t )
	{
		var members = t.GetProperties().Where( m => GetColumnAttr( m ) != null );
		return members.Select( m => m.Name );
	}

	static public ColumnAttribute GetColumnAttr( PropertyInfo m )
	{
		return (ColumnAttribute)(m.GetCustomAttributes( typeof( ColumnAttribute ), false ).FirstOrDefault());
	}

	static public TableAttribute GetTableAttr( Type t )
	{
		return (TableAttribute)t.GetCustomAttributes( typeof( TableAttribute ), false ).FirstOrDefault();
	}

	static public ColumnAttribute GetColumnAttr( Type t, string objName )
	{
		var member = t.GetProperty( objName );
		if ( member == null )
			throw new ArgumentException( "Object does not have that column", objName );

		return GetColumnAttr( member );
	}

	static public string GetColumnDBName( Type t, string objName )
	{
		return GetColumnAttr( t, objName ).Name ?? objName;
	}

	static public string GetColumnObjName( Type t, string dbName )
	{
		var members = t.GetProperties().Where( m => GetColumnAttr( m ).Name == dbName );
		if( !members.Any() )
			members = t.GetProperties().Where( m => m.Name == dbName );
		if( !members.Any() || members.Count() > 1 )
			throw new ArgumentException( "Object does not have that DB column, or it's got more than one", dbName );
		return members.First().Name;
	}

	static public string GetTableName( Type t )
	{
		var attr = GetTableAttr( t );
		if( attr == null )
			throw new ArgumentException( "Object does not appear to be a CliMOO row type", t.Name );

		return attr.Name;
	}

	public object GetColumnValue( string col )
	{
		Type t = this.GetType();
		var member = t.GetProperty( col );
		if( member == null )
			throw new ArgumentException( "Column doesn't exist on this row type", col );

		return member.GetValue( this, null );
	}

	public void SetColumnValue( string col, object val )
	{
		Type t = this.GetType();
		var member = t.GetProperty( col );
		if( member == null )
			throw new ArgumentException( "Column doesn't exist on this row type", col );

		member.SetValue( this, val, null );
	}

	static public Type GetColumnType( Type t, string objName )
	{
		var member = t.GetProperty( objName );
		if ( member == null )
			throw new ArgumentException( "Object does not have that column", objName );

		return member.PropertyType;
	}

	static public string GetPKName( Type t )
	{
		return GetTableAttr( t ).PK;
	}

	public ulong GetPK()
	{
		return (ulong)GetColumnValue( GetPKName( this.GetType() ) );
	}

	public void SetPK( ulong pk )
	{
		SetColumnValue( GetPKName( this.GetType() ), pk );
	}
}

public abstract class TableRow<TRow> : TableRow
	where TRow : TableRow
{
}

public class TableAttribute : System.Attribute
{
	public TableAttribute()
	{
		PK = "id";
	}

	public string Name { get; set; }
	public string PK { get; set; }
}

public class ColumnAttribute : System.Attribute
{
	public ColumnAttribute()
	{
		PK = false;
		Big = false;
		Nullable = false;
		Binary = false;
	}

	public string Name { get; set; }
	public bool PK { get; set; }
	public bool Big { get; set; }
	public bool Nullable { get; set; }
	public bool Binary { get; set; }
}

}
