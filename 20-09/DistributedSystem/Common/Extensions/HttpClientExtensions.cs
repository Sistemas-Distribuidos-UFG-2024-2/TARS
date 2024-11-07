using Serilog;

namespace Common.Extensions;

public static class HttpClientExtensions
{
    public static async Task<HttpResponseMessage> SendRequestAsync(this HttpClient client, string url,
        HttpMethod method,
        HttpContent? content = default,
        IDictionary<string, string>? headers = default,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Log.Information("{BaseAddress}{Url} [{Method}] IN", client.BaseAddress, url, method);
            using var request = new HttpRequestMessage(method, url);
            if (content != null)
            {
                request.Content = content;
            }

            if (headers != null)
            {
                foreach (var (key, value) in headers)
                {
                    request.Headers.Add(key, value);
                }
            }

            var response = await client.SendAsync(request, cancellationToken);

            Log.Information("{BaseAddress}{Url} [{Method}] OUT - {StatusCode}", client.BaseAddress, url, method,
                response.StatusCode);

            return response;
        }
        catch (Exception e)
        {
            Log.Error(e, "Exception thrown while fetching {BaseAddress}{Url} [{Method}]", client.BaseAddress, url,
                method);
            throw;
        }
    }
}