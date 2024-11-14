namespace jh.reddit.httpclient;

public class ThrottlingClient : AbstractThrottlingClient
{
  private readonly HttpClient _httpClient;

  private DateTime _throttleUntil;

  private int _expectedThreadCount;

  public ThrottlingClient(HttpClient httpClient, int expectedThreadCount)
  {
    _httpClient = httpClient;
    _expectedThreadCount = expectedThreadCount;
  }

  protected override HttpClient HttpClient => _httpClient;

  protected override DateTime ThrottleUntil => _throttleUntil;

  public override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage requestMessage)
  {
    var resp = await base.SendAsync(requestMessage);

    bool hasRequiredHeaders = true;
    IEnumerable<string>? ratelimitRemainHeader = null;
    IEnumerable<string>? ratelimitResetHeader = null;

    hasRequiredHeaders = hasRequiredHeaders && resp.Headers.TryGetValues("x-ratelimit-remaining", out ratelimitRemainHeader);
    hasRequiredHeaders = hasRequiredHeaders && resp.Headers.TryGetValues("x-ratelimit-reset", out ratelimitResetHeader);

    if (!hasRequiredHeaders)
    {
      return resp;
    }


    bool validRemainHeader = float.TryParse(ratelimitRemainHeader?.First() ?? "0", out float ratelimitRemain);
    bool validResetHeader = int.TryParse(ratelimitResetHeader?.First() ?? "0", out int rateLimitReset);

    if (validRemainHeader && validResetHeader)
    {
      // If we're right on the edge, that can result in some race conditions where we hit the last
      // allowed call before we should.
      float rateLimitWithBuffer = ratelimitRemain - 3 * _expectedThreadCount;
      double msToThrottle = Math.Ceiling(rateLimitReset * 1000 / rateLimitWithBuffer * _expectedThreadCount);
      _throttleUntil = DateTime.Now + TimeSpan.FromMilliseconds(msToThrottle);
    }

    return resp;
  }

  public void IncrementExpectedThreads()
  {
    _expectedThreadCount++;
  }

  private static ThrottlingClient? instance = null;

  private static readonly object lockObject = new();

  public static ThrottlingClient Instance
  {
    get
    {
      lock (lockObject)
      {
        instance ??= new ThrottlingClient(new HttpClient(), 0);
        return instance;
      }
    }
  }
}
