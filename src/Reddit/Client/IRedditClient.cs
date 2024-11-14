using System.Text.Json;
using jh.reddit.Storage;

namespace jh.reddit.client;

public struct GetPostsRequest
{
  public string subreddit;
}

public class GetPostsResponse
{
  public GetPostsResponse()
  {
    Users = new List<User>();
  }

  public required IEnumerable<User> Users { get; set; }

  public override string ToString()
  {
    return JsonSerializer.Serialize(this);
  }
}

public interface IRedditClient
{
  public Task<GetPostsResponse> GetPostsAsync(GetPostsRequest request);

  public Task RefreshTokenAsync();
}
