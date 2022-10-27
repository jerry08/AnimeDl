using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using AnimeDl.Utils;
using AnimeDl.Utils.Extensions;

namespace AnimeDl.Aniskip;

/// <summary>
/// Client for interacting with aniskip api.
/// </summary>
public class AniskipClient
{
    private readonly HttpClient _http;

    /// <summary>
    /// Initializes an instance of <see cref="AniskipClient"/>.
    /// </summary>
    public AniskipClient(HttpClient httpClient)
    {
        _http = httpClient;
    }

    /// <summary>
    /// Initializes an instance of <see cref="AniskipClient"/>.
    /// </summary>
    public AniskipClient() : this(Http.Client)
    {
    }

    /// <summary>
    /// Gets the skip times associated with the episode.
    /// </summary>
    public async ValueTask<List<Stamp>?> GetAsync(
        int malId, int episodeNumber, long episodeLength,
        CancellationToken cancellationToken = default)
    {
        var url = $"https://api.aniskip.com/v2/skip-times/{malId}/{episodeNumber}?types[]=ed&types[]=mixed-ed&types[]=mixed-op&types[]=op&types[]=recap&episodeLength={episodeLength}";

        var response = await _http.SendHttpRequestAsync(url, cancellationToken);
        if (response is null)
            return null;

        var result = JsonConvert.DeserializeObject<AniSkipResponse>(response);

        if (result is not null && result.IsFound)
            return result.Results;
        else
            return null;
    }
}