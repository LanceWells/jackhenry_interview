using System.Net.Http.Headers;
using System.Text;
using jh.reddit.client.reddittypes;
using jh.reddit.httpclient;
using jh.reddit.Storage;

namespace jh.reddit.client;

class RedditClient : IRedditClient
{
  private readonly string AUTHED_REDDIT_BASE_API = Environment.GetEnvironmentVariable("AUTHED_REDDIT_BASE_API") ?? "";

  private readonly string REDDIT_BASE_API = Environment.GetEnvironmentVariable("REDDIT_BASE_API") ?? "";

  private readonly string REDDIT_USERNAME = Environment.GetEnvironmentVariable("REDDIT_USERNAME") ?? "";

  private readonly string REDDIT_PASSWORD = Environment.GetEnvironmentVariable("REDDIT_PASSWORD") ?? "";

  private readonly string REDDIT_CLIENT_ID = Environment.GetEnvironmentVariable("REDDIT_CLIENT_ID") ?? "";

  private readonly string REDDIT_CLIENT_SECRET = Environment.GetEnvironmentVariable("REDDIT_CLIENT_SECRET") ?? "";

  private readonly IHttpClient _httpClient;

  private int _tokenLife = 0;

  private DateTime _tokenRefreshedAt;

  private string? _authToken = null;

  public RedditClient(ThrottlingClient httpClient)
  {
    if (REDDIT_BASE_API == "")
    {
      throw new ArgumentNullException("REDDIT_BASE_API not defined in env.");
    }

    _httpClient = httpClient;
  }

  public async Task<GetPostsResponse> GetPostsAsync(GetPostsRequest request)
  {
    ListingThing? listing = null;

    Console.WriteLine($"Getting posts for {request.subreddit}.");

    try
    {
      var postRequest = new HttpRequestMessage(HttpMethod.Get, $"{AUTHED_REDDIT_BASE_API}/r/{request.subreddit}/hot?g=US");
      postRequest.Headers.Authorization = new AuthenticationHeaderValue("bearer", _authToken);
      postRequest.Headers.UserAgent.TryParseAdd("DotnetApp");

      using HttpResponseMessage resp = await _httpClient.SendAsync(postRequest);
      resp.EnsureSuccessStatusCode();

      listing = await resp.Content.ReadFromJsonAsync<ListingThing>();
    }
    catch (HttpRequestException e)
    {
      HttpRequestException newEx = new($"Failed to get posts for subreddit {request.subreddit}", e);
      throw;
    }

    var posts = listing?.Data.Children.Select(
      p => new Post
      {
        Id = p.Data.Name,
        Title = p.Data.Title,
        Score = p.Data.Score,
        SubredditId = request.subreddit,
        User = new User()
        {
          Id = p.Data.AuthorFullname,
          Name = p.Data.Author,
        }
      }
    ) ?? new List<Post>();

    var postsByUser = posts.Aggregate(new Dictionary<string, User>(), (acc, curr) =>
    {
      if (!acc.ContainsKey(curr.User.Id))
      {
        acc.Add(curr.User.Id, new User()
        {
          Id = curr.User.Id,
          Name = curr.User.Name,
          Posts = new List<Post>()
        });
      }

      acc[curr.User.Id].Posts.Add(curr);
      return acc;
    });

    return new GetPostsResponse
    {
      Users = postsByUser.Values,
    };
  }

  // https://github.com/reddit-archive/reddit/wiki/OAuth2-Quick-Start-Example
  public async Task RefreshTokenAsync()
  {
    var remainingTokenLife = DateTime.UtcNow
      // Use a buffer around the token so that we're not using an expired one if we're right on the
      // line.
      .Subtract(TimeSpan.FromSeconds(5))
      .Subtract(_tokenRefreshedAt).TotalSeconds;

    if (_tokenLife > remainingTokenLife)
    {
      return;
    }

    Console.WriteLine($"Refreshing token at {DateTime.Now}");

    var reqParams = new List<KeyValuePair<string, string>>
    {
        new("grant_type", "password"),
        new("username", REDDIT_USERNAME),
        new("password", REDDIT_PASSWORD)
    };

    var basicAuthTokenBytes = Encoding.UTF8.GetBytes($"{REDDIT_CLIENT_ID}:{REDDIT_CLIENT_SECRET}");
    var basicAuthToken = Convert.ToBase64String(basicAuthTokenBytes);

    var tokenRequest = new HttpRequestMessage(HttpMethod.Post, $"{REDDIT_BASE_API}/api/v1/access_token")
    {
      Content = new FormUrlEncodedContent(reqParams),
      Headers = {
        Authorization = new AuthenticationHeaderValue("Basic", basicAuthToken),
      },
    };

    tokenRequest.Headers.UserAgent.TryParseAdd("DotnetApp");

    var response = await _httpClient.SendAsync(tokenRequest);

    try
    {
      response.EnsureSuccessStatusCode();
    }
    catch (HttpRequestException e)
    {
      Console.Error.WriteLine($"Failed to get a new access token: {e}");
      throw;
    }

    var evalResponse = await response.Content.ReadFromJsonAsync<RedditAccessToken>();

    if (evalResponse == null)
    {
      throw new Exception("Refreshed token is null");
    }

    _authToken = evalResponse.AccessToken;
    _tokenLife = evalResponse.ExpiresIn;
    _tokenRefreshedAt = DateTime.Now;
  }
}
