using Newtonsoft.Json;

namespace System {
	static class JsonExtensions {
		public static string ToJsonString(this object instance) {
			return JsonConvert.SerializeObject(instance, Formatting.None);
		}

		public static T AsJsonObject<T>(this string s) {
			return (T)JsonConvert.DeserializeObject(s, typeof(T));
		}
	}
}