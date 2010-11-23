﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Kayateia.Climoo.MooCore.Sql
{
	using System.Data.Linq;
	using System.Data.Linq.Mapping;
	using System.Data;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Linq;
	using System.Linq.Expressions;
	using System.ComponentModel;
	using System;
	
	
	[global::System.Data.Linq.Mapping.DatabaseAttribute(Name="climoo-tc")]
	public partial class MooCoreSqlDataContext : System.Data.Linq.DataContext
	{
		
		private static System.Data.Linq.Mapping.MappingSource mappingSource = new AttributeMappingSource();
		
    #region Extensibility Method Definitions
    partial void OnCreated();
    partial void InsertMob(Mob instance);
    partial void UpdateMob(Mob instance);
    partial void DeleteMob(Mob instance);
    partial void InsertVerb(Verb instance);
    partial void UpdateVerb(Verb instance);
    partial void DeleteVerb(Verb instance);
    partial void InsertWorld(World instance);
    partial void UpdateWorld(World instance);
    partial void DeleteWorld(World instance);
    partial void InsertAttribute(Attribute instance);
    partial void UpdateAttribute(Attribute instance);
    partial void DeleteAttribute(Attribute instance);
    partial void InsertCheckpoint(Checkpoint instance);
    partial void UpdateCheckpoint(Checkpoint instance);
    partial void DeleteCheckpoint(Checkpoint instance);
    #endregion
		
		public MooCoreSqlDataContext() : 
				base(global::Kayateia.Climoo.MooCore.Properties.Settings.Default.climooConnectionString, mappingSource)
		{
			OnCreated();
		}
		
		public MooCoreSqlDataContext(string connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public MooCoreSqlDataContext(System.Data.IDbConnection connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public MooCoreSqlDataContext(string connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public MooCoreSqlDataContext(System.Data.IDbConnection connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public System.Data.Linq.Table<Mob> Mobs
		{
			get
			{
				return this.GetTable<Mob>();
			}
		}
		
		public System.Data.Linq.Table<Verb> Verbs
		{
			get
			{
				return this.GetTable<Verb>();
			}
		}
		
		public System.Data.Linq.Table<World> Worlds
		{
			get
			{
				return this.GetTable<World>();
			}
		}
		
		public System.Data.Linq.Table<Attribute> Attributes
		{
			get
			{
				return this.GetTable<Attribute>();
			}
		}
		
		public System.Data.Linq.Table<Checkpoint> Checkpoints
		{
			get
			{
				return this.GetTable<Checkpoint>();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.Mob")]
	public partial class Mob : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private int _id;
		
		private System.Nullable<int> _parent;
		
		private System.Nullable<int> _location;
		
		private string _pathid;
		
		private int _checkpoint;
		
		private int _objectid;
		
		private EntitySet<Mob> _Mobs;
		
		private EntitySet<Mob> _Mobs1;
		
		private EntitySet<Verb> _Verbs;
		
		private EntitySet<Attribute> _Attributes;
		
		private EntityRef<Mob> _Mob1;
		
		private EntityRef<Mob> _Mob2;
		
		private EntityRef<Checkpoint> _Checkpoint1;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnidChanging(int value);
    partial void OnidChanged();
    partial void OnparentChanging(System.Nullable<int> value);
    partial void OnparentChanged();
    partial void OnlocationChanging(System.Nullable<int> value);
    partial void OnlocationChanged();
    partial void OnpathidChanging(string value);
    partial void OnpathidChanged();
    partial void OncheckpointChanging(int value);
    partial void OncheckpointChanged();
    partial void OnobjectidChanging(int value);
    partial void OnobjectidChanged();
    #endregion
		
		public Mob()
		{
			this._Mobs = new EntitySet<Mob>(new Action<Mob>(this.attach_Mobs), new Action<Mob>(this.detach_Mobs));
			this._Mobs1 = new EntitySet<Mob>(new Action<Mob>(this.attach_Mobs1), new Action<Mob>(this.detach_Mobs1));
			this._Verbs = new EntitySet<Verb>(new Action<Verb>(this.attach_Verbs), new Action<Verb>(this.detach_Verbs));
			this._Attributes = new EntitySet<Attribute>(new Action<Attribute>(this.attach_Attributes), new Action<Attribute>(this.detach_Attributes));
			this._Mob1 = default(EntityRef<Mob>);
			this._Mob2 = default(EntityRef<Mob>);
			this._Checkpoint1 = default(EntityRef<Checkpoint>);
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_id", DbType="Int IDENTITY NOT NULL", IsPrimaryKey=true, IsDbGenerated=true)]
		public int id
		{
			get
			{
				return this._id;
			}
			set
			{
				if ((this._id != value))
				{
					this.OnidChanging(value);
					this.SendPropertyChanging();
					this._id = value;
					this.SendPropertyChanged("id");
					this.OnidChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_parent", DbType="Int")]
		public System.Nullable<int> parent
		{
			get
			{
				return this._parent;
			}
			set
			{
				if ((this._parent != value))
				{
					if (this._Mob2.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnparentChanging(value);
					this.SendPropertyChanging();
					this._parent = value;
					this.SendPropertyChanged("parent");
					this.OnparentChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_location", DbType="Int")]
		public System.Nullable<int> location
		{
			get
			{
				return this._location;
			}
			set
			{
				if ((this._location != value))
				{
					if (this._Mob1.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnlocationChanging(value);
					this.SendPropertyChanging();
					this._location = value;
					this.SendPropertyChanged("location");
					this.OnlocationChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_pathid", DbType="VarChar(50)")]
		public string pathid
		{
			get
			{
				return this._pathid;
			}
			set
			{
				if ((this._pathid != value))
				{
					this.OnpathidChanging(value);
					this.SendPropertyChanging();
					this._pathid = value;
					this.SendPropertyChanged("pathid");
					this.OnpathidChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_checkpoint")]
		public int checkpoint
		{
			get
			{
				return this._checkpoint;
			}
			set
			{
				if ((this._checkpoint != value))
				{
					if (this._Checkpoint1.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OncheckpointChanging(value);
					this.SendPropertyChanging();
					this._checkpoint = value;
					this.SendPropertyChanged("checkpoint");
					this.OncheckpointChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_objectid")]
		public int objectid
		{
			get
			{
				return this._objectid;
			}
			set
			{
				if ((this._objectid != value))
				{
					this.OnobjectidChanging(value);
					this.SendPropertyChanging();
					this._objectid = value;
					this.SendPropertyChanged("objectid");
					this.OnobjectidChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="Mob_Mob", Storage="_Mobs", ThisKey="objectid", OtherKey="location")]
		public EntitySet<Mob> Mobs
		{
			get
			{
				return this._Mobs;
			}
			set
			{
				this._Mobs.Assign(value);
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="Mob_Mob1", Storage="_Mobs1", ThisKey="objectid", OtherKey="parent")]
		public EntitySet<Mob> Mobs1
		{
			get
			{
				return this._Mobs1;
			}
			set
			{
				this._Mobs1.Assign(value);
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="Mob_Verb", Storage="_Verbs", ThisKey="id", OtherKey="object")]
		public EntitySet<Verb> Verbs
		{
			get
			{
				return this._Verbs;
			}
			set
			{
				this._Verbs.Assign(value);
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="Mob_Attribute", Storage="_Attributes", ThisKey="id", OtherKey="object")]
		public EntitySet<Attribute> Attributes
		{
			get
			{
				return this._Attributes;
			}
			set
			{
				this._Attributes.Assign(value);
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="Mob_Mob", Storage="_Mob1", ThisKey="location", OtherKey="objectid", IsForeignKey=true)]
		public Mob Mob1
		{
			get
			{
				return this._Mob1.Entity;
			}
			set
			{
				Mob previousValue = this._Mob1.Entity;
				if (((previousValue != value) 
							|| (this._Mob1.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._Mob1.Entity = null;
						previousValue.Mobs.Remove(this);
					}
					this._Mob1.Entity = value;
					if ((value != null))
					{
						value.Mobs.Add(this);
						this._location = value.objectid;
					}
					else
					{
						this._location = default(Nullable<int>);
					}
					this.SendPropertyChanged("Mob1");
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="Mob_Mob1", Storage="_Mob2", ThisKey="parent", OtherKey="objectid", IsForeignKey=true)]
		public Mob Mob2
		{
			get
			{
				return this._Mob2.Entity;
			}
			set
			{
				Mob previousValue = this._Mob2.Entity;
				if (((previousValue != value) 
							|| (this._Mob2.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._Mob2.Entity = null;
						previousValue.Mobs1.Remove(this);
					}
					this._Mob2.Entity = value;
					if ((value != null))
					{
						value.Mobs1.Add(this);
						this._parent = value.objectid;
					}
					else
					{
						this._parent = default(Nullable<int>);
					}
					this.SendPropertyChanged("Mob2");
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="Checkpoint_Mob", Storage="_Checkpoint1", ThisKey="checkpoint", OtherKey="id", IsForeignKey=true)]
		public Checkpoint Checkpoint1
		{
			get
			{
				return this._Checkpoint1.Entity;
			}
			set
			{
				Checkpoint previousValue = this._Checkpoint1.Entity;
				if (((previousValue != value) 
							|| (this._Checkpoint1.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._Checkpoint1.Entity = null;
						previousValue.Mobs.Remove(this);
					}
					this._Checkpoint1.Entity = value;
					if ((value != null))
					{
						value.Mobs.Add(this);
						this._checkpoint = value.id;
					}
					else
					{
						this._checkpoint = default(int);
					}
					this.SendPropertyChanged("Checkpoint1");
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
		
		private void attach_Mobs(Mob entity)
		{
			this.SendPropertyChanging();
			entity.Mob1 = this;
		}
		
		private void detach_Mobs(Mob entity)
		{
			this.SendPropertyChanging();
			entity.Mob1 = null;
		}
		
		private void attach_Mobs1(Mob entity)
		{
			this.SendPropertyChanging();
			entity.Mob2 = this;
		}
		
		private void detach_Mobs1(Mob entity)
		{
			this.SendPropertyChanging();
			entity.Mob2 = null;
		}
		
		private void attach_Verbs(Verb entity)
		{
			this.SendPropertyChanging();
			entity.Mob = this;
		}
		
		private void detach_Verbs(Verb entity)
		{
			this.SendPropertyChanging();
			entity.Mob = null;
		}
		
		private void attach_Attributes(Attribute entity)
		{
			this.SendPropertyChanging();
			entity.Mob = this;
		}
		
		private void detach_Attributes(Attribute entity)
		{
			this.SendPropertyChanging();
			entity.Mob = null;
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.Verb")]
	public partial class Verb : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private int _id;
		
		private string _name;
		
		private string _code;
		
		private int _object;
		
		private EntityRef<Mob> _Mob;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnidChanging(int value);
    partial void OnidChanged();
    partial void OnnameChanging(string value);
    partial void OnnameChanged();
    partial void OncodeChanging(string value);
    partial void OncodeChanged();
    partial void OnobjectChanging(int value);
    partial void OnobjectChanged();
    #endregion
		
		public Verb()
		{
			this._Mob = default(EntityRef<Mob>);
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_id", AutoSync=AutoSync.OnInsert, DbType="Int NOT NULL IDENTITY", IsPrimaryKey=true, IsDbGenerated=true)]
		public int id
		{
			get
			{
				return this._id;
			}
			set
			{
				if ((this._id != value))
				{
					this.OnidChanging(value);
					this.SendPropertyChanging();
					this._id = value;
					this.SendPropertyChanged("id");
					this.OnidChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_name", DbType="VarChar(50) NOT NULL", CanBeNull=false)]
		public string name
		{
			get
			{
				return this._name;
			}
			set
			{
				if ((this._name != value))
				{
					this.OnnameChanging(value);
					this.SendPropertyChanging();
					this._name = value;
					this.SendPropertyChanged("name");
					this.OnnameChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_code", DbType="VarChar(MAX) NOT NULL", CanBeNull=false)]
		public string code
		{
			get
			{
				return this._code;
			}
			set
			{
				if ((this._code != value))
				{
					this.OncodeChanging(value);
					this.SendPropertyChanging();
					this._code = value;
					this.SendPropertyChanged("code");
					this.OncodeChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Name="object", Storage="_object", DbType="Int NOT NULL")]
		public int @object
		{
			get
			{
				return this._object;
			}
			set
			{
				if ((this._object != value))
				{
					if (this._Mob.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnobjectChanging(value);
					this.SendPropertyChanging();
					this._object = value;
					this.SendPropertyChanged("@object");
					this.OnobjectChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="Mob_Verb", Storage="_Mob", ThisKey="object", OtherKey="id", IsForeignKey=true)]
		public Mob Mob
		{
			get
			{
				return this._Mob.Entity;
			}
			set
			{
				Mob previousValue = this._Mob.Entity;
				if (((previousValue != value) 
							|| (this._Mob.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._Mob.Entity = null;
						previousValue.Verbs.Remove(this);
					}
					this._Mob.Entity = value;
					if ((value != null))
					{
						value.Verbs.Add(this);
						this._object = value.id;
					}
					else
					{
						this._object = default(int);
					}
					this.SendPropertyChanged("Mob");
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.World")]
	public partial class World : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private int _id;
		
		private System.Nullable<int> _intvalue;
		
		private string _strvalue;
		
		private string _name;
		
		private int _checkpoint;
		
		private EntityRef<Checkpoint> _Checkpoint1;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnidChanging(int value);
    partial void OnidChanged();
    partial void OnintvalueChanging(System.Nullable<int> value);
    partial void OnintvalueChanged();
    partial void OnstrvalueChanging(string value);
    partial void OnstrvalueChanged();
    partial void OnnameChanging(string value);
    partial void OnnameChanged();
    partial void OncheckpointChanging(int value);
    partial void OncheckpointChanged();
    #endregion
		
		public World()
		{
			this._Checkpoint1 = default(EntityRef<Checkpoint>);
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_id", AutoSync=AutoSync.OnInsert, DbType="Int NOT NULL IDENTITY", IsPrimaryKey=true, IsDbGenerated=true)]
		public int id
		{
			get
			{
				return this._id;
			}
			set
			{
				if ((this._id != value))
				{
					this.OnidChanging(value);
					this.SendPropertyChanging();
					this._id = value;
					this.SendPropertyChanged("id");
					this.OnidChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_intvalue", DbType="Int")]
		public System.Nullable<int> intvalue
		{
			get
			{
				return this._intvalue;
			}
			set
			{
				if ((this._intvalue != value))
				{
					this.OnintvalueChanging(value);
					this.SendPropertyChanging();
					this._intvalue = value;
					this.SendPropertyChanged("intvalue");
					this.OnintvalueChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_strvalue", DbType="VarChar(255)")]
		public string strvalue
		{
			get
			{
				return this._strvalue;
			}
			set
			{
				if ((this._strvalue != value))
				{
					this.OnstrvalueChanging(value);
					this.SendPropertyChanging();
					this._strvalue = value;
					this.SendPropertyChanged("strvalue");
					this.OnstrvalueChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_name", DbType="VarChar(50)")]
		public string name
		{
			get
			{
				return this._name;
			}
			set
			{
				if ((this._name != value))
				{
					this.OnnameChanging(value);
					this.SendPropertyChanging();
					this._name = value;
					this.SendPropertyChanged("name");
					this.OnnameChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_checkpoint")]
		public int checkpoint
		{
			get
			{
				return this._checkpoint;
			}
			set
			{
				if ((this._checkpoint != value))
				{
					if (this._Checkpoint1.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OncheckpointChanging(value);
					this.SendPropertyChanging();
					this._checkpoint = value;
					this.SendPropertyChanged("checkpoint");
					this.OncheckpointChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="Checkpoint_World", Storage="_Checkpoint1", ThisKey="checkpoint", OtherKey="id", IsForeignKey=true)]
		public Checkpoint Checkpoint1
		{
			get
			{
				return this._Checkpoint1.Entity;
			}
			set
			{
				Checkpoint previousValue = this._Checkpoint1.Entity;
				if (((previousValue != value) 
							|| (this._Checkpoint1.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._Checkpoint1.Entity = null;
						previousValue.Worlds.Remove(this);
					}
					this._Checkpoint1.Entity = value;
					if ((value != null))
					{
						value.Worlds.Add(this);
						this._checkpoint = value.id;
					}
					else
					{
						this._checkpoint = default(int);
					}
					this.SendPropertyChanged("Checkpoint1");
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.Attribute")]
	public partial class Attribute : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private int _id;
		
		private string _textcontents;
		
		private System.Data.Linq.Binary _datacontents;
		
		private string _name;
		
		private string _mimetype;
		
		private int _object;
		
		private EntityRef<Mob> _Mob;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnidChanging(int value);
    partial void OnidChanged();
    partial void OntextcontentsChanging(string value);
    partial void OntextcontentsChanged();
    partial void OndatacontentsChanging(System.Data.Linq.Binary value);
    partial void OndatacontentsChanged();
    partial void OnnameChanging(string value);
    partial void OnnameChanged();
    partial void OnmimetypeChanging(string value);
    partial void OnmimetypeChanged();
    partial void OnobjectChanging(int value);
    partial void OnobjectChanged();
    #endregion
		
		public Attribute()
		{
			this._Mob = default(EntityRef<Mob>);
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_id", AutoSync=AutoSync.OnInsert, DbType="Int NOT NULL IDENTITY", IsPrimaryKey=true, IsDbGenerated=true)]
		public int id
		{
			get
			{
				return this._id;
			}
			set
			{
				if ((this._id != value))
				{
					this.OnidChanging(value);
					this.SendPropertyChanging();
					this._id = value;
					this.SendPropertyChanged("id");
					this.OnidChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_textcontents", DbType="VarChar(MAX)")]
		public string textcontents
		{
			get
			{
				return this._textcontents;
			}
			set
			{
				if ((this._textcontents != value))
				{
					this.OntextcontentsChanging(value);
					this.SendPropertyChanging();
					this._textcontents = value;
					this.SendPropertyChanged("textcontents");
					this.OntextcontentsChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_datacontents", DbType="VarBinary(MAX)", UpdateCheck=UpdateCheck.Never)]
		public System.Data.Linq.Binary datacontents
		{
			get
			{
				return this._datacontents;
			}
			set
			{
				if ((this._datacontents != value))
				{
					this.OndatacontentsChanging(value);
					this.SendPropertyChanging();
					this._datacontents = value;
					this.SendPropertyChanged("datacontents");
					this.OndatacontentsChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_name", DbType="VarChar(50) NOT NULL", CanBeNull=false)]
		public string name
		{
			get
			{
				return this._name;
			}
			set
			{
				if ((this._name != value))
				{
					this.OnnameChanging(value);
					this.SendPropertyChanging();
					this._name = value;
					this.SendPropertyChanged("name");
					this.OnnameChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_mimetype", DbType="VarChar(50) NOT NULL", CanBeNull=false)]
		public string mimetype
		{
			get
			{
				return this._mimetype;
			}
			set
			{
				if ((this._mimetype != value))
				{
					this.OnmimetypeChanging(value);
					this.SendPropertyChanging();
					this._mimetype = value;
					this.SendPropertyChanged("mimetype");
					this.OnmimetypeChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Name="object", Storage="_object", DbType="Int NOT NULL")]
		public int @object
		{
			get
			{
				return this._object;
			}
			set
			{
				if ((this._object != value))
				{
					if (this._Mob.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnobjectChanging(value);
					this.SendPropertyChanging();
					this._object = value;
					this.SendPropertyChanged("@object");
					this.OnobjectChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="Mob_Attribute", Storage="_Mob", ThisKey="object", OtherKey="id", IsForeignKey=true)]
		public Mob Mob
		{
			get
			{
				return this._Mob.Entity;
			}
			set
			{
				Mob previousValue = this._Mob.Entity;
				if (((previousValue != value) 
							|| (this._Mob.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._Mob.Entity = null;
						previousValue.Attributes.Remove(this);
					}
					this._Mob.Entity = value;
					if ((value != null))
					{
						value.Attributes.Add(this);
						this._object = value.id;
					}
					else
					{
						this._object = default(int);
					}
					this.SendPropertyChanged("Mob");
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="")]
	public partial class Checkpoint : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private int _id;
		
		private System.DateTimeOffset _time;
		
		private string _name;
		
		private EntitySet<Mob> _Mobs;
		
		private EntitySet<World> _Worlds;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnidChanging(int value);
    partial void OnidChanged();
    partial void OntimeChanging(System.DateTimeOffset value);
    partial void OntimeChanged();
    partial void OnnameChanging(string value);
    partial void OnnameChanged();
    #endregion
		
		public Checkpoint()
		{
			this._Mobs = new EntitySet<Mob>(new Action<Mob>(this.attach_Mobs), new Action<Mob>(this.detach_Mobs));
			this._Worlds = new EntitySet<World>(new Action<World>(this.attach_Worlds), new Action<World>(this.detach_Worlds));
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_id", IsPrimaryKey=true, IsDbGenerated=true)]
		public int id
		{
			get
			{
				return this._id;
			}
			set
			{
				if ((this._id != value))
				{
					this.OnidChanging(value);
					this.SendPropertyChanging();
					this._id = value;
					this.SendPropertyChanged("id");
					this.OnidChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_time")]
		public System.DateTimeOffset time
		{
			get
			{
				return this._time;
			}
			set
			{
				if ((this._time != value))
				{
					this.OntimeChanging(value);
					this.SendPropertyChanging();
					this._time = value;
					this.SendPropertyChanged("time");
					this.OntimeChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_name")]
		public string name
		{
			get
			{
				return this._name;
			}
			set
			{
				if ((this._name != value))
				{
					this.OnnameChanging(value);
					this.SendPropertyChanging();
					this._name = value;
					this.SendPropertyChanged("name");
					this.OnnameChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="Checkpoint_Mob", Storage="_Mobs", ThisKey="id", OtherKey="checkpoint")]
		public EntitySet<Mob> Mobs
		{
			get
			{
				return this._Mobs;
			}
			set
			{
				this._Mobs.Assign(value);
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="Checkpoint_World", Storage="_Worlds", ThisKey="id", OtherKey="checkpoint")]
		public EntitySet<World> Worlds
		{
			get
			{
				return this._Worlds;
			}
			set
			{
				this._Worlds.Assign(value);
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
		
		private void attach_Mobs(Mob entity)
		{
			this.SendPropertyChanging();
			entity.Checkpoint1 = this;
		}
		
		private void detach_Mobs(Mob entity)
		{
			this.SendPropertyChanging();
			entity.Checkpoint1 = null;
		}
		
		private void attach_Worlds(World entity)
		{
			this.SendPropertyChanging();
			entity.Checkpoint1 = this;
		}
		
		private void detach_Worlds(World entity)
		{
			this.SendPropertyChanging();
			entity.Checkpoint1 = null;
		}
	}
}
#pragma warning restore 1591
