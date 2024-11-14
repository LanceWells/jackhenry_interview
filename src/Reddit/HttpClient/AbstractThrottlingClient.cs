namespace jh.reddit.httpclient;

public abstract class AbstractThrottlingClient : IHttpClient
{
  protected abstract HttpClient HttpClient { get; }

  protected abstract DateTime ThrottleUntil { get; }

  public virtual async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
  {
    TimeSpan timeToThrottle = ThrottleUntil - DateTime.Now;
    int msToThrottle = timeToThrottle.Milliseconds;

    if (msToThrottle > 0)
    {
      Console.WriteLine($"Throttling for {msToThrottle}ms.");
      await Task.Delay(msToThrottle);
    }

    HttpResponseMessage resp = await HttpClient.SendAsync(request);
    return resp;
  }
}
