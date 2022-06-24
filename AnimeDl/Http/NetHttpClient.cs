using System;
using System.Collections.Specialized;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AnimeDl;

internal class NetHttpClient
{
    private readonly HttpClient _httpClient;

    public NetHttpClient()
    {
        _httpClient = new HttpClient();
    }

    public NetHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<long> GetFileSizeAsync(
        string url,
        NameValueCollection headers,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Head, url);
        for (int j = 0; j < headers.Count; j++)
        {
            request.Headers.TryAddWithoutValidation(headers.Keys[j]!, headers[j]);
        }

        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken
        );

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Response status code does not indicate success: {(int)response.StatusCode} ({response.StatusCode})." +
                Environment.NewLine +
                "Request:" +
                Environment.NewLine +
                request
            );
        }

        return response.Content.Headers.ContentLength ?? 0;
    }

    public async ValueTask<string> SendHttpRequestAsync(
        string url,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        return await SendHttpRequestAsync(request, cancellationToken);
    }

    public async ValueTask<string> SendHttpRequestAsync(
        string url,
        NameValueCollection headers,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        for (int j = 0; j < headers.Count; j++)
        {
            request.Headers.TryAddWithoutValidation(headers.Keys[j]!, headers[j]);
        }
        return await SendHttpRequestAsync(request, cancellationToken);
    }

    public async ValueTask<string> SendHttpRequestAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken = default)
    {
        // User-agent
        if (!request.Headers.Contains("User-Agent"))
        {
            request.Headers.Add(
                "User-Agent",
                Http.ChromeUserAgent()
            );
        }

        // Set required cookies
        //request.Headers.Add("Cookie", "CONSENT=YES+cb; YSC=DwKYllHNwuw");

        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken
        );

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Response status code does not indicate success: {(int)response.StatusCode} ({response.StatusCode})." +
                Environment.NewLine +
                "Request:" +
                Environment.NewLine +
                request
            );
        }

        //return await response.Content.ReadAsStringAsync(cancellationToken);
        return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
    }
}