using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Specialized;
using AnimeDl.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace AnimeDl.Utils.Extensions;

public static class HttpExtensions
{
    public static async ValueTask<HttpResponseMessage> HeadAsync(
        this HttpClient http,
        string requestUri,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Head, requestUri);
        return await http.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken
        );
    }

    public static async ValueTask<Stream> GetStreamAsync(
        this HttpClient http,
        string requestUri,
        long? from = null,
        long? to = null,
        bool ensureSuccess = true,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        request.Headers.Range = new RangeHeaderValue(from, to);

        var response = await http.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken
        );

        if (ensureSuccess)
            response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }

    public static async ValueTask<long?> TryGetContentLengthAsync(
        this HttpClient http,
        string requestUri,
        bool ensureSuccess = true,
        CancellationToken cancellationToken = default)
    {
        using var response = await http.HeadAsync(requestUri, cancellationToken);

        if (ensureSuccess)
            response.EnsureSuccessStatusCode();

        return response.Content.Headers.ContentLength;
    }

    //Mine
    public static string Get(
        this HttpClient http,
        string url,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        return AsyncHelper.RunSync(() => http.SendHttpRequestAsync(request, cancellationToken), cancellationToken);
    }

    public static async ValueTask<string> GetAsync(
        this HttpClient http,
        string url,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        return await http.SendHttpRequestAsync(request, cancellationToken);
    }

    public static async ValueTask<string> PostAsync(
        this HttpClient http,
        string url,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        return await http.SendHttpRequestAsync(request, cancellationToken);
    }

    public static async ValueTask<string> PostAsync(
        this HttpClient http,
        string url,
        NameValueCollection headers,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        for (int j = 0; j < headers.Count; j++)
            request.Headers.TryAddWithoutValidation(headers.Keys[j]!, headers[j]);

        return await http.SendHttpRequestAsync(request, cancellationToken);
    }

    public static async ValueTask<string> PostAsync(
        this HttpClient http,
        string url,
        NameValueCollection headers,
        HttpContent content,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        for (int j = 0; j < headers.Count; j++)
            request.Headers.TryAddWithoutValidation(headers.Keys[j]!, headers[j]);

        request.Content = content;

        return await http.SendHttpRequestAsync(request, cancellationToken);
    }

    public static async ValueTask<long> GetFileSizeAsync(
        this HttpClient http,
        string url,
        Dictionary<string, string> headers,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Head, url);
        for (int j = 0; j < headers.Count; j++)
            request.Headers.TryAddWithoutValidation(headers.ElementAt(j).Key, headers.ElementAt(j).Value);

        using var response = await http.SendAsync(
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

    public static async ValueTask<string> SendHttpRequestAsync(
        this HttpClient http,
        string url,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        return await http.SendHttpRequestAsync(request, cancellationToken);
    }

    public static async ValueTask<string> SendHttpRequestAsync(
        this HttpClient http,
        string url,
        Dictionary<string, string> headers,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        for (int j = 0; j < headers.Count; j++)
            request.Headers.TryAddWithoutValidation(headers.ElementAt(j).Key, headers.ElementAt(j).Value);

        return await http.SendHttpRequestAsync(request, cancellationToken);
    }

    public static async Task<string> SendHttpRequestAsync(
        this HttpClient http,
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

        using var response = await http.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken
        );

        if (response.StatusCode == HttpStatusCode.NotFound)
            return string.Empty;

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

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}