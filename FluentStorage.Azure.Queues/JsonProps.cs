using Newtonsoft.Json;

namespace FluentStorage.Azure.Queues {
	class JsonProps {
		[JsonProperty("props")]
		public JsonProp[] Properties { get; set; }
	}

	class JsonProp {
		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("value")]
		public string Value { get; set; }
	}
}
