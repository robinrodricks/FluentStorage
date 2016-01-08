using Newtonsoft.Json;

namespace Storage.Net.Azure.Queue.Storage
{
   class JsonProps
   {
      [JsonProperty("properties")]
      public JsonProp[] Properties { get; set; }
   }

   class JsonProp
   {
      [JsonProperty("name")]
      public string Name { get; set; }

      [JsonProperty("value")]
      public string Value { get; set; }
   }
}
