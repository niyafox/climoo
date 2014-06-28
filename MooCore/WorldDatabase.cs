namespace Kayateia.Climoo.MooCore
{
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kayateia.Climoo.Database;

/// <summary>
/// Represents one checkpoint of the database.
/// </summary>
public class WorldCheckpoint
{
	public int id;
	public DateTimeOffset time;
	public string name;
}

/// <summary>
/// Implements the world database using a CoreDatabase instance.
/// </summary>
public class WorldDatabase
{
	public WorldDatabase( CoreDatabase db )
	{
		_db = db;
	}

	/// <summary>
	/// Return a list of all saved checkpoints.
	/// </summary>
	public IEnumerable<WorldCheckpoint> checkpoints
	{
		get
		{
			lock( _lock )
			{
				// We convert this to an array here explicitly to avoid lock slicing.
				var results = _db.select( new DBCheckpoint(), null );
				return (from w in results
						select new WorldCheckpoint()
						{
							id = w.id,
							name = w.name,
							time = w.time
						}).ToArray();
			}
		}
	}

	// Returns the most recent (and current) checkpoint.
	int latestCheckpoint
	{
		get
		{
			if( _latestCheckpoint == -1 )
			{
				var sorted = checkpoints.OrderByDescending( c => c.time );
				if( !sorted.Any() )
				{
					// If there isn't an existing one, go ahead and make the first one -- empty database.
					DBCheckpoint dbcheckpoint = new DBCheckpoint()
					{
						name = "initial",
						time = DateTimeOffset.UtcNow
					};
					_db.insert( dbcheckpoint );

					_latestCheckpoint = dbcheckpoint.id;
				}
				else
					_latestCheckpoint = sorted.First().id;
			}

			return _latestCheckpoint;
		}
	}
	int _latestCheckpoint = -1;

	/// <summary>
	/// Remove a checkpoint from the database.
	/// </summary>
	public void checkpointRemove( int chkid )
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// Saves the specified Mob to the current checkpoint, overwriting any previous value.
	/// </summary>
	public void saveMob( Mob m )
	{
		lock( _lock )
		using( var trans = _db.transaction() )
		{
			// Get the current checkpoint ID.
			int curCheckpoint = latestCheckpoint;

			// Is it in the mob table for this checkpoint? If not, we just write out a new one.
			IEnumerable<DBMobTable> mt = _db.select(
				new DBMobTable()
				{
					objectId = m.id
				},
				new string[] { "objectId" }
			);
			if( mt.Count( mtm => mtm.checkpoint == curCheckpoint ) > 0 )
			{
				// Just delete the mobtable entry.
				_db.delete(
					new DBMobTable()
					{
						id = mt.First().id
					},
					new string[] { "id" }
				);

				// Is it used in any other checkpoints? If so, we have to make a new one too.
				if( mt.Count() == 1 )
				{
					// Delete the existing data.
					int mobId = mt.First().mob;
					deleteMobInternal( mobId );
				}
			}

			// Any old object will have been deleted by this point, so we make a new one.
			DBMob dbmob = new DBMob()
			{
				objectId = m.id,
				location = m.locationId,
				owner = m.ownerId,
				parent = m.parentId,
				pathId = m.pathId,
				perms = m.perms
			};
			_db.insert( dbmob );

			// Write out a new mobtable entry.
			DBMobTable dbmobtable = new DBMobTable()
			{
				mob = dbmob.id,
				objectId = m.id,
				checkpoint = curCheckpoint
			};
			_db.insert( dbmobtable );

			// Save out all the attributes.
			foreach( var attrName in m.attrList )
			{
				TypedAttribute attr = m.attrGet( attrName );
				string strval = null;
				byte[] binval = null;
				if ( attr.isString )
					strval = attr.str;
				else
					binval = attr.contentsAsBytes;
				DBAttr dbattr = new DBAttr()
				{
					mime = attr.mimetype,
					name = attrName,
					mob = dbmob.id,
					perms = attr.perms,
					text = strval,
					data = binval
				};
				_db.insert( dbattr );
			}

			// Save out all the verbs.
			foreach( var verbName in m.verbList )
			{
				Verb v = m.verbGet( verbName );
				DBVerb dbverb = new DBVerb()
				{
					name = v.name,
					code = v.code,
					mob = dbmob.id,
					perms = v.perms
				};
				_db.insert( dbverb );
			}

			// This mob was just saved.
			m.lastSave = DateTimeOffset.UtcNow;

			trans.commit();
		}
	}

	/// <summary>
	/// Loads the specified Mob from the current checkpoint.
	/// </summary>
	/// <param name="objectId">The Mob's object ID (not database ID)</param>
	/// <param name="world">The World object to attach this Mob to</param>
	/// <returns>
	/// The loaded Mob, or null if it doesn't exist in this checkpoint.
	/// </returns>
	public Mob loadMob( int objectId, World world )
	{
		// We don't need to write anything here, but the transaction may give us reader semantics too.
		lock( _lock )
		using( var trans = _db.transaction() )
		{
			// Get the current checkpoint ID.
			int curCheckpoint = latestCheckpoint;

			// Find the existing object in the MobTable.
			IEnumerable<DBMobTable> results = _db.select(
				new DBMobTable()
				{
					objectId = objectId,
					checkpoint = curCheckpoint
				},
				new string[] { "objectId", "checkpoint" }
			);
			if( !results.Any() )
				return null;

			int mobDbId = results.First().mob;

			// Get the mob itself.
			IEnumerable<DBMob> mobs = _db.select(
				new DBMob()
				{
					id = mobDbId
				},
				new string[] { "id" }
			);
			if( !results.Any() )
				throw new ArgumentException( "Database error: Mob is in mobtable, but non-existant" );
			DBMob mob = mobs.First();

			// Look for all of its attributes.
			IEnumerable<DBAttr> attrs = _db.select(
				new DBAttr()
				{
					mob = mobDbId
				},
				new string[] { "mob" }
			);

			// And all of its verbs.
			IEnumerable<DBVerb> verbs = _db.select(
				new DBVerb()
				{
					mob = mobDbId
				},
				new string[] { "mob" }
			);

			// Now put it all together into a world object.
			Mob m = new Mob( world, objectId )
			{
				parentId = mob.parent ?? 0,
				locationId = mob.location ?? 0,
				ownerId = mob.owner,
				perms = mob.perms,
				pathId = mob.pathId ?? null
			};
			foreach( DBAttr attr in attrs )
			{
				TypedAttribute ta;
				if( attr.text != null )
					ta = TypedAttribute.FromValue( attr.text );
				else if( attr.data != null )
					ta = TypedAttribute.FromPersisted( attr.data.ToArray(), attr.mime );
				else
					ta = TypedAttribute.FromNull();
				ta.perms = attr.perms;
				m.attrSet(attr.name, ta);
			}
			foreach( DBVerb verb in verbs )
			{
				Verb v = new Verb()
				{
					name = verb.name,
					code = verb.code,
					perms = verb.perms
				};
				m.verbSet(verb.name, v);
			}

			// This mob was just loaded, so its save time is the same.
			m.lastSave = DateTimeOffset.UtcNow;

			return m;
		}
	}

	/// <summary>
	/// Returns the list of all mobs in the current checkpoint.
	/// </summary>
	public IEnumerable<int> mobList
	{
		get
		{
			// We don't need to write anything here, but the transaction may give us reader semantics too.
			lock( _lock )
			using( var trans = _db.transaction() )
			{
				// We convert this to an array here explicitly to avoid lock slicing.
				return (_db.select(
					new DBMobTable()
					{
						checkpoint = latestCheckpoint
					},
					new string[] { "checkpoint" }
				).Select( mt => mt.objectId )).ToArray();
			}
		}
	}

	/// <summary>
	/// Deletes a Mob from the current checkpoint.
	/// </summary>
	/// <param name="objectId">The Mob's object ID (not database ID)</param>
	public void deleteMob( int objectId )
	{
		lock( _lock )
		using( var trans = _db.transaction() )
		{
			// Get the current checkpoint ID.
			int curCheckpoint = latestCheckpoint;

			// Find the existing object in the MobTable.
			IEnumerable<DBMobTable> results = _db.select(
				new DBMobTable()
				{
					objectId = objectId,
				},
				new string[] { "objectId" }
			);

			// If it's not in our checkpoint, just bail.
			DBMobTable mtmIdForUs = results.FirstOrDefault( mtm => mtm.checkpoint == curCheckpoint );
			if( mtmIdForUs == null )
				return;

			// Nuke it from the mobtable for this checkpoint.
			_db.delete(
				new DBMobTable()
				{
					id = mtmIdForUs.id
				},
				new string[] { "id" }
			);

			// If it's the only checkpoint using it, delete the mob too.
			if( results.Count() == 1 )
				deleteMobInternal( mtmIdForUs.id );

			trans.commit();
		}
	}

	// Performs the actual deletion of a mob based on a mob.id rather than mob.object.
	void deleteMobInternal( int mobId )
	{
		_db.delete(
			new DBAttr()
			{
				mob = mobId
			},
			new string[] { "mob" }
		);
		_db.delete(
			new DBVerb()
			{
				mob = mobId
			},
			new string[] { "mob" }
		);
		_db.delete(
			new DBMob()
			{
				id = mobId
			},
			new string[] { "id" }
		);
	}

	/// <summary>
	/// Creates a new checkpoint. After the checkpoint, all modifications will operate
	/// on new copies of the objects in questions. (Checkpoints are independent.)
	/// </summary>
	public void checkpoint( string checkpointName )
	{
		lock( _lock )
		using( var trans = _db.transaction() )
		{
			// Find the ID of the existing newest one.
			int oldCpId = latestCheckpoint;

			// Create a new checkpoint.
			DBCheckpoint cp = new DBCheckpoint()
			{
				name = checkpointName,
				time = DateTimeOffset.UtcNow
			};
			_db.insert( cp );

			// Duplicate the mob table of the previous checkpoint.
			IEnumerable<DBMobTable> oldmts = _db.select(
				new DBMobTable()
				{
					checkpoint = oldCpId
				},
				new string[] { "checkpoint" }
			);
			foreach( DBMobTable mt in oldmts )
				_db.insert(
					new DBMobTable()
					{
						checkpoint = cp.id,
						mob = mt.mob,
						objectId = mt.objectId
					}
				);

			// Duplicate the config table as well. We don't try to COW these.
			IEnumerable<DBConfig> oldcfgs = _db.select(
				new DBConfig()
				{
					checkpoint = oldCpId
				},
				new string[] { "checkpoint" }
			);
			foreach( DBConfig cfg in oldcfgs )
				_db.insert(
					new DBConfig()
					{
						checkpoint = cp.id,
						name = cfg.name,
						intvalue = cfg.intvalue,
						strvalue = cfg.strvalue
					}
				);

			trans.commit();

			// The latest checkpoint is now the new one.
			_latestCheckpoint = cp.id;
		}
	}

	/// <summary>
	/// Returns a piece of arbitrary key/value config, in string form.
	/// </summary>
	/// <param name="key">The key</param>
	/// <returns>The value, assuming it was really a string. Returns null if the value doesn't exist.</returns>
	/// <remarks>This does not convert between int/string. You must use the right one.</remarks>
	public string getConfigString( string key )
	{
		lock( _lock )
		{
			return getConfigInner( key ).strvalue;
		}
	}

	/// <summary>
	/// Returns a piece of arbitrary key/value config, in int form.
	/// </summary>
	/// <param name="key">The key</param>
	/// <returns>The value, assuming it was really an int. Returns null if the value doesn't exist.</returns>
	/// <remarks>This does not convert between int/string. You must use the right one.</remarks>
	public int? getConfigInt( string key )
	{
		lock( _lock )
		{
			return getConfigInner( key ).intvalue;
		}
	}

	// Services requests for both getConfigString and getConfigInt.
	DBConfig getConfigInner( string key )
	{
		int curCheckpoint = latestCheckpoint;
		IEnumerable<DBConfig> cfg = _db.select(
			new DBConfig()
			{
				name = key,
				checkpoint = curCheckpoint
			},
			new string[] { "name", "checkpoint" }
		);
		if( cfg.Count() != 1 )
			return null;
		return cfg.First();
	}

	/// <summary>
	/// Sets a piece of arbitrary key/value config, in string form.
	/// </summary>
	public void setConfigString( string key, string val )
	{
		lock( _lock )
		using( var trans = _db.transaction() )
		{
			setConfigInner(
				new DBConfig()
				{
					name = key,
					strvalue = val,
					checkpoint = latestCheckpoint
				}
			);

			trans.commit();
		}
	}

	/// <summary>
	/// Sets a piece of arbitrary key/value config, in int form.
	/// </summary>
	public void setConfigInt( string key, int val )
	{
		lock( _lock )
		using( var trans = _db.transaction() )
		{
			setConfigInner(
				new DBConfig()
				{
					name = key,
					intvalue = val,
					checkpoint = latestCheckpoint
				}
			);

			trans.commit();
		}
	}

	// Services requests for both setConfigString and setConfigInt.
	void setConfigInner( DBConfig config )
	{
		// Delete any existing one, and then insert the new value.
		_db.delete( config, new string[] { "name", "checkpoint" } );
		_db.insert( config );
	}

	CoreDatabase _db;
	object _lock = new object();
}

}
