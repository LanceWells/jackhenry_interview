using jh.reddit.client;
using jh.reddit.Storage;

namespace jh.reddit.postmonitor;

public class PostMonitor
{
  private IRedditClient _client;

  private readonly RedditRepository _db;

  public PostMonitor(IRedditClient client, RedditRepository db)
  {
    _client = client;
    _db = db;
  }

  public async Task MonitorPosts(string subreddit)
  {
    // FUTURE: Add a cancellation token.
    while (true)
    {
      await _client.RefreshTokenAsync();

      var request = new GetPostsRequest()
      {
        subreddit = subreddit,
      };

      var response = await _client.GetPostsAsync(request);

      foreach (var user in response.Users)
      {
        _db.SaveUser(user);
      }
    }
  }
}
