namespace jh.reddit.httpclient;

public interface IHttpClient
{
  public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request);
}
