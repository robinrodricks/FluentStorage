using Newtonsoft.Json;

namespace Storage.Net.Microsoft.Azure.Storage.Messaging
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
