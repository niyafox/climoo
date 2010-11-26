﻿namespace Kayateia.Climoo.MooCore {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public partial class World {
	public void saveToSql() {
		saveToSql(null);
	}

	public void saveToSql(string checkpointName) {
		using (var context = new Sql.MooCoreSqlDataContext()) {
			context.Connection.Open();
			var trans = context.Connection.BeginTransaction();
			context.Transaction = trans;
			try {
				// Create a new checkpoint.
				var checkpoints = context.GetTable<Sql.Checkpoint>();
				var cp = new Sql.Checkpoint() { time = DateTimeOffset.UtcNow, name = checkpointName };
				checkpoints.InsertOnSubmit(cp);
				context.SubmitChanges();

				var mobtable = context.GetTable<Sql.Mob>();
				var attrtable = context.GetTable<Sql.Attribute>();
				var verbtable = context.GetTable<Sql.Verb>();
				var worldtable = context.GetTable<Sql.World>();

				foreach (Mob m in _objects.Values) {
					var newmob = new Sql.Mob() {
						objectid = m.id,
						parent = m.parentId > 0 ? (int?)m.parentId : null,
						pathid = m.pathId,
						location = m.locationId > 0 ? (int?)m.locationId : null,
						checkpoint = cp.id
					};
					mobtable.InsertOnSubmit(newmob);
					context.SubmitChanges();

					foreach (var name in m.attrList) {
						string strval = null;
						object binval = null;
						var item = m.attrGet(name);
						if (item.isString)
							strval = item.str;
						else
							binval = item.contentsAsBytes;
						var newattr = new Sql.Attribute() {
							mimetype = item.mimetype,
							name = name,
							textcontents = strval,
							datacontents = binval != null ? new System.Data.Linq.Binary((byte[])binval) : null,
							@object = newmob.id
						};
						attrtable.InsertOnSubmit(newattr);
					}

					foreach (var name in m.verbList) {
						var item = m.verbGet(name);
						verbtable.InsertOnSubmit(new Sql.Verb() {
							name = item.name,
							code = item.code,
							@object = newmob.id
						});
					}
				}

				worldtable.InsertOnSubmit(new Sql.World() {
					name = "nextid",
					intvalue = _nextId,
					checkpoint = cp.id
				});
				context.SubmitChanges();

				trans.Commit();
			} catch (Exception) {
				trans.Rollback();
				throw;
			}
		}
	}

	public bool loadFromSql() {
		using (var context = new Sql.MooCoreSqlDataContext()) {
			context.Connection.Open();

			// Find the latest checkpoint.
			var cp = (from c in context.GetTable<Sql.Checkpoint>()
						orderby c.time descending
						select c).FirstOrDefault();
			if (cp == null)
				return false;

			var mobtable = context.GetTable<Sql.Mob>();
			var attrtable = context.GetTable<Sql.Attribute>();
			var verbtable = context.GetTable<Sql.Verb>();
			var worldtable = context.GetTable<Sql.World>();

			// Try to select out the next ID -- if we don't have this, give up.
			int? nextid = (from row in worldtable
						where row.name == "nextid" && row.checkpoint == cp.id
						select row.intvalue).FirstOrDefault();
			if (!nextid.HasValue)
				return false;
			_nextId = nextid.Value;

			// Alright, we're clear to proceed. Go ahead and query every thing
			// out for fast loading, and we'll match up in memory.
			//
			// FIXME: LINQ doesn't support IN(foo). >_< So we have to just load
			// every attribute and verb for every version of every object, ever,
			// and sort through it in memory. Clearly this isn't tenable long term.
			// Perhaps a join or manual SQL later.
			var allmobs = from row in mobtable
							where row.checkpoint == cp.id
							select row;
			var allattrs = from row in attrtable
							select row;
			var allverbs = from row in verbtable
							select row;

			// Match up attributes with mobs.
			var attrmatches = new Dictionary<int,List<Sql.Attribute>>();
			foreach (var attr in allattrs) {
				if (!attrmatches.ContainsKey(attr.@object))
					attrmatches[attr.@object] = new List<Sql.Attribute>();
				attrmatches[attr.@object].Add(attr);
			}

			// Match up verbs with mobs.
			var verbmatches = new Dictionary<int,List<Sql.Verb>>();
			foreach (var verb in allverbs) {
				if (!verbmatches.ContainsKey(verb.@object))
					verbmatches[verb.@object] = new List<Sql.Verb>();
				verbmatches[verb.@object].Add(verb);
			}

			// Load up the mobs themselves.
			foreach (var m in allmobs) {
				var mob = new Mob(this, m.objectid) {
					parentId = m.parent.HasValue ? m.parent.Value : 0,
					locationId = m.location.HasValue ? m.location.Value : 0,
				};
				if (m.pathid != null)
					mob.pathId = m.pathid;

				// Load up its attributes.
				if (attrmatches.ContainsKey(m.id)) {
					foreach (var attr in attrmatches[m.id]) {
						TypedAttribute ta;
						if (attr.textcontents != null)
							ta = TypedAttribute.FromValue(attr.textcontents);
						else
							ta = TypedAttribute.FromPersisted(attr.datacontents.ToArray(), attr.mimetype);
						mob.attrSet(attr.name, ta);
					}
				}

				// Load up its verbs.
				if (verbmatches.ContainsKey(m.id)) {
					foreach (var verb in verbmatches[m.id]) {
						Verb v = new Verb() {
							name = verb.name,
							code = verb.code
						};
						mob.verbSet(verb.name, v);
					}
				}

				_objects[mob.id] = mob;
			}

			// Everything loaded properly.
			return true;
		}
	}

	static public World FromSql() {
		World w = new World();
		if (!w.loadFromSql())
			return null;
		else
			return w;
	}
}

}