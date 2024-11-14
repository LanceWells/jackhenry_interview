using System.Text.Json.Serialization;

namespace jh.reddit.client.reddittypes;

public class PostContents
{
  [JsonPropertyName("subreddit")]
  public string Subreddit { get; set; } = string.Empty;

  [JsonPropertyName("author_fullname")]
  public string AuthorFullname { get; set; } = string.Empty;

  [JsonPropertyName("author")]
  public string Author { get; set; } = string.Empty;

  [JsonPropertyName("title")]
  public string Title { get; set; } = string.Empty;

  [JsonPropertyName("score")]
  public int Score { get; set; } = 0;

  [JsonPropertyName("name")]
  public string Name { get; set; } = string.Empty;
}

public class PostThing : IThing<PostContents>
{
  public PostThing()
  {
  }

  public string Kind { get; set; } = "t3";

  public PostContents Data { get; set; } = new PostContents();
}
