using System.Text.Json.Serialization;

namespace jh.reddit.client.reddittypes;

public class RedditAccessToken
{
  [JsonPropertyName("access_token")]
  public string AccessToken { get; set; } = string.Empty;

  [JsonPropertyName("token_type")]
  public string TokenType { get; set; } = string.Empty;

  [JsonPropertyName("expires_in")]
  public int ExpiresIn { get; set; } = 0;

  [JsonPropertyName("scope")]
  public string Scope { get; set; } = string.Empty;
}
