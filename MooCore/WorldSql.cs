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
				context.ExecuteCommand("delete from [Mob]");
				context.ExecuteCommand("delete from [Attribute]");
				context.ExecuteCommand("delete from [Verb]");
				context.ExecuteCommand("delete from [World]");

				var mobtable = context.GetTable<Sql.Mob>();
				var attrtable = context.GetTable<Sql.Attribute>();
				var verbtable = context.GetTable<Sql.Verb>();
				var worldtable = context.GetTable<Sql.World>();

				foreach (Mob m in _objects.Values) {
					var newmob = new Sql.Mob() {
						id = m.id,
						parent = m.parentId > 1 ? (int?)m.parentId : null,
						pathid = m.pathId,
						location = m.locationId > 0 ? (int?)m.locationId : null
					};
					mobtable.InsertOnSubmit(newmob);
					context.SubmitChanges();

					foreach (var item in m.attributes) {
						var val = item.Value;
						string strval = null;
						string mimetype = "text/plain";
						object binval = null;
						if (val is TypedAttribute) {
							var ta = (TypedAttribute)val;
							mimetype = ta.mimetype;
							if (ta.mimetype == "text/plain")
								strval = (string)ta.contents;
							else
								binval = ta.contents;
						} else
							strval = (string)val;
						var newattr = new Sql.Attribute() {
							mimetype = mimetype,
							name = item.Key,
							textcontents = strval,
							datacontents = new System.Data.Linq.Binary((byte[])binval),
							@object = newmob.id
						};
						attrtable.InsertOnSubmit(newattr);
					}
					context.SubmitChanges();

					foreach (var item in m.verbs) {
						verbtable.InsertOnSubmit(new Sql.Verb() {
							name = item.Value.name,
							code = item.Value.code,
							@object = newmob.id
						});
					}
					context.SubmitChanges();
				}

				trans.Commit();
			} catch (Exception) {
				trans.Rollback();
				throw;
			}
		}
	}
}

}
