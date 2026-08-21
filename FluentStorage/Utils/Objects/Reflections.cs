using System;
using System.Collections.Generic;
using System.Reflection;

namespace FluentStorage.Utils.Objects;

public static class Reflections {

	/// <summary>
	/// Get the value of a property or field on any type of object.
	/// </summary>
	/// <param name="prop">Property name or dot-path of the property</param>
	public static object GetProp(object obj, string prop, bool isPublic = true) {
		if (obj == null) { return null; }

		// fixed types - Dictionary and ExpandoObject
		if (obj is IDictionary<string, object>) {
			var dict = (IDictionary<string, object>)obj;
			object dictValue;
			if (dict.TryGetValue(prop, out dictValue)) {
				return dictValue;
			}
			return null;

		}
		else {

			// dynamic types - use reflection

			// get type info
			Type type = obj.GetType();

			// get field
			var flags = isPublic ? BindingFlags.Public | BindingFlags.Instance : BindingFlags.NonPublic | BindingFlags.Instance;
			FieldInfo info = type.GetField(prop, flags);
			if (info != null) {
				return info.GetValue(obj);
			}
			else {

				// get property
				PropertyInfo info2 = type.GetProperty(prop, flags);
				if (info2 != null) {
					return info2.GetValue(obj, null);
				}
			}

		}

		return null;
	}

	/// <summary>
	/// Get the value of a property or field on any type of object, and typecast it to the given type.
	/// </summary>
	/// <typeparam name="T">Type of the property you want to retrieve</typeparam>
	/// <param name="name">Property name of the property</param>
	public static T GetPropTyped<T>(object obj, string name, bool isPublic = true) {
		var value = GetProp(obj, name, isPublic);
		if (value != null) {
			return (T)value;
		}
		return default(T);
	}

	/// <summary>
	/// Set the value of a property or field on any type of object.
	/// </summary>
	/// <param name="name">Property name of the property</param>
	/// <param name="value">New value you wish to set</param>
	/// <param name="isField">You want to fetch a field (true) or a property (false)?</param>
	public static void SetProp(object obj, string name, object value, bool isPublic = true) {

		// fixed types - Dictionary and ExpandoObject
		if (obj is IDictionary<string, object>) {
			var dict = (IDictionary<string, object>)obj;
			dict.Add(name, value);
		}
		else {

			// dynamic types - use reflection

			// get type info
			Type type = obj.GetType();

			// get field
			var flags = isPublic ? BindingFlags.Public | BindingFlags.Instance : BindingFlags.NonPublic | BindingFlags.Instance;
			FieldInfo info = type.GetField(name, flags);
			if (info != null) {
				info.SetValue(obj, value);
			}
			else {

				// get property
				PropertyInfo info2 = type.GetProperty(name, flags);
				if (info2 != null) {
					info2.SetValue(obj, value);
				}
			}

		}

	}

}