using System.Text.Json;
using jh.Models;
using jh.reddit.httpclient;
using jh.reddit.client;
using jh.reddit.postmonitor;
using Microsoft.AspNetCore.Mvc;
using jh.reddit.Storage;

namespace jh.Controllers;

[ApiController]
[Route("[controller]")]
public class RedditController : ControllerBase
{
  private readonly ILogger<RedditController> _logger;

  private readonly RedditRepository db;

  public RedditController(ILogger<RedditController> logger)
  {
    _logger = logger;

    var ctx = new RedditInMemoryContext();
    db = new RedditRepository(ctx);
  }

  [HttpPost]
  public IActionResult WatchSubreddit(SubredditWatchDTO request)
  {
    _logger.LogTrace($"WatchSubreddit: {JsonSerializer.Serialize(request)}");

    ThrottlingClient tc = ThrottlingClient.Instance;
    tc.IncrementExpectedThreads();

    var rc = new RedditClient(tc);
    var pm = new PostMonitor(rc, db);

    // FUTURE: Support for cancellation token.
    _ = pm.MonitorPosts(request.Subreddit);

    return new OkObjectResult(null);
  }


  [Route("{subreddit?}/posts/top")]
  [HttpGet]
  public IActionResult GetTopPostForSubreddit(string subreddit)
  {
    var topPost = db.GetTopPost(subreddit);
    Console.WriteLine(topPost);
    return new OkObjectResult(topPost);
  }

  [Route("{subreddit?}/users/top")]
  [HttpGet]
  public IActionResult GetTopUserForSubreddit(string subreddit)
  {
    var topUser = db.GetTopUser(subreddit);
    Console.WriteLine(topUser);
    return new OkObjectResult(topUser);
  }
}
