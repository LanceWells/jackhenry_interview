using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace jh.reddit.Storage;

public class RedditInMemoryContext : DbContext
{
  public static string RedditDbName = "Reddit";

  public DbSet<Post>? Posts { get; set; }

  public DbSet<User>? Users { get; set; }

  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
    optionsBuilder.UseInMemoryDatabase(RedditDbName);
    base.OnConfiguring(optionsBuilder);
  }
}

public class Post
{
  public string Id { get; set; } = string.Empty;

  public string Title { get; set; } = string.Empty;

  public int Score { get; set; } = 0;

  public string UserId { get; set; } = string.Empty;

  [JsonIgnore]
  public User User { get; set; } = new User();

  public string SubredditId { get; set; } = string.Empty;
}

public class User
{
  public string Id { get; set; } = string.Empty;

  public string Name { get; set; } = string.Empty;

  public List<Post> Posts { get; set; } = new List<Post>();
}
