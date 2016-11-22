using Newtonsoft.Json;

namespace Storage.Net.Azure.Messaging.Storage
{
   class JsonProps
   {
      [JsonProperty("props")]
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
