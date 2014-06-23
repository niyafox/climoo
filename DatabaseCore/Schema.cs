using System;
using System.Collections.Generic;

namespace Kayateia.Climoo.Database {

/// <summary>
/// Programmatic schema definition, to be used by things that talk to IDatabase.
/// </summary>
/// <remarks>
/// This is to bring some sanity to the relative outlaw nature of IDatabase. It's this
/// or port to Hadoop. :) (Which might not be a bad idea eventually, really.)
///
/// This will also allow database providers to generate table code.
/// </remarks>
public class Schema {
	/// <summary>
	/// Table name constants.
	/// </summary>
	public static class TableNames {
		public const string Mob = "mob";
		public const string Verb = "verb";
		public const string Attr = "attribute";
		public const string Checkpoint = "checkpoint";
		public const string Config = "config";
		public const string Screen = "screen";
		public const string User = "user";
	}

	/// <summary>
	/// Column name constants for the Mob table.
	/// </summary>
	public static class MobColumns {
		public const string Id = "id";
		public const string ObjectId = "objectid";
		public const string Parent = "parentid";
		public const string PathId = "pathid";
		public const string Location = "locationid";
		public const string Perms = "perms";
		public const string Owner = "ownerid";
		public const string Checkpoint = "checkpointid";
	}

	/// <summary>
	/// Column name constants for the Verb table.
	/// </summary>
	public static class VerbColumns {
		public const string Id = "id";
		public const string Name = "name";
		public const string Code = "code";
		public const string Object = "objectid";
		public const string Perms = "perms";
	}

	/// <summary>
	/// Column name constants for the Attr[ibute] table.
	/// </summary>
	public static class AttrColumns {
		public const string Id = "id";
		public const string Text = "textcontents";
		public const string Data = "datacontents";
		public const string Name = "name";
		public const string Mime = "mimetype";
		public const string Object = "objectid";
		public const string Perms = "perms";
	}

	/// <summary>
	/// Column name constants for the Checkpoint table.
	/// </summary>
	public static class CheckpointColumns {
		public const string Id = "id";
		public const string Time = "time";
		public const string Name = "name";
	}

	/// <summary>
	/// Column name constants for the Config table.
	/// </summary>
	public static class ConfigColumns {
		public const string Id = "id";
		public const string Name = "name";
		public const string Int = "intvalue";
		public const string Str = "strvalue";
		public const string Checkpoint = "checkpointid";
	}

	/// <summary>
	/// Column name constants for the Screen table.
	/// </summary>
	public static class ScreenColumns {
		public const string Id = "id";
		public const string Name = "name";
		public const string Text = "text";
	}

	/// <summary>
	/// Column name constants for the User table.
	/// </summary>
	public static class UserColumns {
		public const string Id = "id";
		public const string Login = "login";
		public const string OpenId = "openid";
		public const string Password = "password";
		public const string Object = "objectid";
		public const string Name = "name";
	}

	/// <summary>
	/// Columns can be one of the limited types below.
	/// </summary>
	/// <remarks>
	/// Note that the Blob type may be implemented by saving a pointer to an
	/// external reference on the file system.
	/// </remarks>
	public enum ColumnType {
		Int,
		IntPK,
		IntNullable,
		String,
		StringNullable,
		StringBig,
		StringBigNullable,
		BlobNullable,
		DateTime,
		Bool
	}

	/// <summary>Represents a single table in the database schema.</summary>
	public class TableDef {
		public IDictionary<string, ColumnType> Columns { get; set; }
	}

	Schema() {
		this.Tables = new Dictionary<string, TableDef>() {
			{ TableNames.Mob, new TableDef() {
				Columns = new Dictionary<string, ColumnType>() {
					{ MobColumns.Id, ColumnType.IntPK },
					{ MobColumns.ObjectId, ColumnType.Int },
					{ MobColumns.Parent, ColumnType.IntNullable },
					{ MobColumns.PathId, ColumnType.String },
					{ MobColumns.Location, ColumnType.IntNullable },
					{ MobColumns.Perms, ColumnType.Int },
					{ MobColumns.Owner, ColumnType.Int },
					{ MobColumns.Checkpoint, ColumnType.Int }
				}
			} },
			{ TableNames.Verb, new TableDef() {
				Columns = new Dictionary<string, ColumnType>() {
					{ VerbColumns.Id, ColumnType.IntPK },
					{ VerbColumns.Name, ColumnType.String },
					{ VerbColumns.Code, ColumnType.StringBig },
					{ VerbColumns.Object, ColumnType.Int },
					{ VerbColumns.Perms, ColumnType.Int },
				}
			} },
			{ TableNames.Attr, new TableDef() {
				Columns = new Dictionary<string, ColumnType>() {
					{ AttrColumns.Id, ColumnType.IntPK },
					{ AttrColumns.Text, ColumnType.StringBigNullable },
					{ AttrColumns.Data, ColumnType.BlobNullable },
					{ AttrColumns.Name, ColumnType.String },
					{ AttrColumns.Mime, ColumnType.String },
					{ AttrColumns.Object, ColumnType.Int },
					{ AttrColumns.Perms, ColumnType.Int },
				}
			} },
			{ TableNames.Checkpoint, new TableDef() {
				Columns = new Dictionary<string, ColumnType>() {
					{ CheckpointColumns.Id, ColumnType.IntPK },
					{ CheckpointColumns.Time, ColumnType.DateTime },
					{ CheckpointColumns.Name, ColumnType.String },
				}
			} },
			{ TableNames.Config, new TableDef() {
				Columns = new Dictionary<string, ColumnType>() {
					{ ConfigColumns.Id, ColumnType.IntPK },
					{ ConfigColumns.Name, ColumnType.String },
					{ ConfigColumns.Int, ColumnType.IntNullable },
					{ ConfigColumns.Str, ColumnType.StringBigNullable },
					{ ConfigColumns.Checkpoint, ColumnType.Int },
				}
			} },
			{ TableNames.Screen, new TableDef() {
				Columns = new Dictionary<string, ColumnType>() {
					{ ScreenColumns.Id, ColumnType.IntPK },
					{ ScreenColumns.Name, ColumnType.String },
					{ ScreenColumns.Text, ColumnType.StringBig },
				}
			} },
			{ TableNames.User, new TableDef() {
				Columns = new Dictionary<string, ColumnType>() {
					{ UserColumns.Id, ColumnType.IntPK },
					{ UserColumns.Login, ColumnType.String },
					{ UserColumns.OpenId, ColumnType.Bool },
					{ UserColumns.Password, ColumnType.String },
					{ UserColumns.Object, ColumnType.Int },
					{ UserColumns.Name, ColumnType.String },
				}
			} },
		};
	}

	public IDictionary<string, TableDef> Tables { get; set; }

	static public Schema Global = new Schema();
}

}
