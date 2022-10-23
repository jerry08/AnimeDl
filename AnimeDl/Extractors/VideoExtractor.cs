using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using AnimeDl.Models;
using AnimeDl.Extractors.Interfaces;

namespace AnimeDl.Extractors;

/// <summary>
/// Base class for video extractors.
/// </summary>
public abstract class VideoExtractor : IVideoExtractor
{
    public readonly HttpClient _http;

    public readonly VideoServer _server;

    public VideoExtractor(
        HttpClient http,
        VideoServer server)
    {
        _http = http;
        _server = server;
    }

    public abstract Task<List<Video>> Extract();
}