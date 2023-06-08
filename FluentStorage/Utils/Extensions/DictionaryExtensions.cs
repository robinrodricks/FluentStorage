using System;
using System.Collections.Generic;

namespace FluentStorage.Utils.Extensions {
	public static class DictionaryExtensions {
		//
		// Summary:
		//     Adds all elements from source to target
		//
		// Parameters:
		//   source:
		//     Source dictionary to get the values from
		//
		//   target:
		//     Target dictionary to add values to
		//
		// Type parameters:
		//   TKey:
		//     Key type
		//
		//   TValue:
		//     Value type
		public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> target, IDictionary<TKey, TValue> source) {
			if (target == null || source == null) {
				return;
			}

			foreach (KeyValuePair<TKey, TValue> item in source) {
				target.Add(item);
			}
		}

		//
		// Summary:
		//     Merges all the keys into target dictionary, overwriting existing values
		//
		// Parameters:
		//   source:
		//     Source dictionary to get the values from
		//
		//   target:
		//     Target dictionary to merge values to
		//
		// Type parameters:
		//   TKey:
		//     Key type
		//
		//   TValue:
		//     Value type
		public static void MergeRange<TKey, TValue>(this IDictionary<TKey, TValue> target, IDictionary<TKey, TValue> source) {
			if (target == null || source == null) {
				return;
			}

			foreach (KeyValuePair<TKey, TValue> item in source) {
				target[item.Key] = item.Value;
			}
		}

		//
		// Summary:
		//     Gets element by key if it exists in the dictionary, otherwise calls specifed
		//     method to create a new element and adds it back to the dictionary
		//
		// Parameters:
		//   target:
		//     Target dictionary
		//
		//   key:
		//     Key to search on
		//
		//   createValue:
		//     Method used to create a new value
		//
		// Type parameters:
		//   TKey:
		//     Key type
		//
		//   TValue:
		//     Value type
		public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> target, TKey key, Func<TValue> createValue) {
			if (target == null) {
				throw new ArgumentNullException("target");
			}

			if (createValue == null) {
				throw new ArgumentNullException("createValue");
			}

			if (!target.TryGetValue(key, out var value)) {
				value = (target[key] = createValue());
			}

			return value;
		}

	}
}