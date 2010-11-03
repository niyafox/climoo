namespace Kayateia.Climoo.MooCore {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public partial class World {
	public void saveToSql() {
		using (var context = new Sql.MooCoreSqlDataContext()) {
			context.Connection.Open();
			var trans = context.Connection.BeginTransaction();
			context.Transaction = trans;
			try {
				// Start out by destroying the whole persisted DB.
				context.ExecuteCommand("delete from [Verb]");
				context.ExecuteCommand("delete from [Attribute]");
				context.ExecuteCommand("delete from [Mob]");
				context.ExecuteCommand("delete from [World]");

				var mobtable = context.GetTable<Sql.Mob>();
				var attrtable = context.GetTable<Sql.Attribute>();
				var verbtable = context.GetTable<Sql.Verb>();
				var worldtable = context.GetTable<Sql.World>();

				foreach (Mob m in _objects.Values) {
					var newmob = new Sql.Mob() {
						id = m.id,
						parent = m.parentId > 0 ? (int?)m.parentId : null,
						pathid = m.pathId,
						location = m.locationId > 0 ? (int?)m.locationId : null
					};
					mobtable.InsertOnSubmit(newmob);
					context.SubmitChanges();

					foreach (var name in m.attrList) {
						string strval = null;
						object binval = null;
						var item = m.attrGet(name);
						if (item.mimetype == "text/plain")
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
					context.SubmitChanges();

					foreach (var name in m.verbList) {
						var item = m.verbGet(name);
						verbtable.InsertOnSubmit(new Sql.Verb() {
							name = item.name,
							code = item.code,
							@object = newmob.id
						});
					}
					context.SubmitChanges();
				}

				worldtable.InsertOnSubmit(new Sql.World() {
					name = "nextid",
					intvalue = _nextId
				});
				context.SubmitChanges();

				trans.Commit();
			} catch (Exception) {
				trans.Rollback();
				throw;
			}
		}
	}
}

}
