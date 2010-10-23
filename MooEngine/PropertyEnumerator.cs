namespace Kayateia.Climoo.MooEngine {
using System.Collections.Generic;
using System.ComponentModel;

internal class PropertyEnumerator {
    static public IEnumerable<PropertyValue> GetProperties(object o) {
        if (o != null) {
            var props = TypeDescriptor.GetProperties(o);
            foreach (PropertyDescriptor prop in props) {
                object val = prop.GetValue(o);
                if (val != null) {
                    yield return new PropertyValue { Name = prop.Name, Value = val };
                }
            }
        }
    }

	public sealed class PropertyValue {
        public string Name { get; set; }
        public object Value { get; set; }
    }
}

}
