using System.Text.Json.Serialization;

namespace jh.reddit.client.reddittypes;

interface IThing<T>
{
  [JsonPropertyName("kind")]
  string Kind { get; set; }

  [JsonPropertyName("data")]
  T Data { get; set; }
}
