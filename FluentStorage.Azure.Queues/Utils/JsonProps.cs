
namespace FluentStorage.Azure.Queues.Utils;

class JsonProps {
	public JsonProp[] props { get; set; }
}

class JsonProp {
	public string name { get; set; }

	public string value { get; set; }
}