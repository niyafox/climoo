namespace Kayateia.Climoo {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Runtime.Serialization;

public class XmlPersistence {
	/// <summary>
	/// Loads from a persisted XML file into a data model.
	/// </summary>
	static public T Load<T>(string filename) {
		using (FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read))
			using (XmlReader reader = XmlReader.Create(stream)) {
				DataContractSerializer dcs = new DataContractSerializer(typeof(T));
				return (T)dcs.ReadObject(reader);
			}
	}

	/// <summary>
	/// Saves a data model into a persisted XML file.
	/// </summary>
	static public void Save<T>(string filename, T model) {
		using (FileStream stream = new FileStream(filename, FileMode.CreateNew, FileAccess.Write))
			using (XmlWriter writer = XmlWriter.Create(stream)) {
				DataContractSerializer dcs = new DataContractSerializer(typeof(T));
				dcs.WriteObject(writer, model);
			}
	}
}

}
