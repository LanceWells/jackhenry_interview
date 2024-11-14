using Microsoft.EntityFrameworkCore;

namespace jh.reddit.Storage;

public class RedditRepository
{
  private readonly RedditInMemoryContext _ctx;

  public RedditRepository(RedditInMemoryContext ctx)
  {
    _ctx = ctx;
  }

  public void SaveUser(User user)
  {
    if (_ctx.Users == null || _ctx.Posts == null)
    {
      return;
    }

    var existingUser = _ctx.Users.Find(user.Id);
    if (existingUser == null)
    {
      _ctx.Users.Add(user);
      _ctx.SaveChanges();
      return;
    }

    existingUser.Name = user.Name;

    foreach (var post in user.Posts)
    {
      var existingPost = _ctx.Posts.Find(post.Id);
      if (existingPost == null)
      {
        _ctx.Posts.Add(new Post()
        {
          Id = post.Id,
          Score = post.Score,
          SubredditId = post.SubredditId,
          Title = post.Title,
          UserId = post.User.Id,
        });
        continue;
      }

      existingPost.Score = post.Score;
    }

    _ctx.SaveChanges();
  }

  public Post? GetTopPost(string subreddit)
  {
    if (_ctx.Posts == null)
    {
      return null;
    }

    var topPost = _ctx
      .Posts
      .Where((p) => p.SubredditId == subreddit)
      .OrderByDescending((p) => p.Score)
      .FirstOrDefault();

    return topPost;
  }

  public User? GetTopUser(string subreddit)
  {
    if (_ctx.Users == null)
    {
      return null;
    }

    var topUser = _ctx
      .Users
      .Include(
        (u) => u
          .Posts
          .Where((p) => p.SubredditId == subreddit)
        )
      .OrderByDescending((u) => u.Posts.Count)
      .FirstOrDefault();

    return topUser;
  }
}
