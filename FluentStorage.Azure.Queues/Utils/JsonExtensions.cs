using System.Text.Json;

namespace FluentStorage.Azure.Queues.Utils;

static class JsonExtensions {

	public static string ToJsonString(this object instance) {
		return JsonSerializer.Serialize(instance);
	}

	public static T AsJsonObject<T>(this string s) {
		return JsonSerializer.Deserialize<T>(s)!;
	}
}