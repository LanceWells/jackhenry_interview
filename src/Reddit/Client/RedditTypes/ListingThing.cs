using System.Text.Json.Serialization;

namespace jh.reddit.client.reddittypes;

public class ListingContents
{
  [JsonPropertyName("after")]
  public string After { get; set; } = string.Empty;

  [JsonPropertyName("children")]
  public List<PostThing> Children { get; set; } = new List<PostThing>();
}

public class ListingThing : IThing<ListingContents>
{
  public ListingThing()
  { }

  public string Kind { get; set; } = "listing";

  public ListingContents Data { get; set; } = new ListingContents();
}
