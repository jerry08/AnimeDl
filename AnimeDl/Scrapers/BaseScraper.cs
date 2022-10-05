using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using AnimeDl.Models;
using AnimeDl.Scrapers.Interfaces;
using AnimeDl.Extractors.Interfaces;

namespace AnimeDl.Scrapers;

internal abstract class BaseScraper : IAnimeScraper
{
    public abstract string Name { get; set; }

    public virtual string BaseUrl => default!;

    public abstract bool IsDubAvailableSeparately { get; set; }

    public bool GetIsDubAvailableSeparately() => IsDubAvailableSeparately;

    public readonly HttpClient _http;

    public BaseScraper(HttpClient http)
        => _http = http;

    public virtual async Task<List<Anime>> SearchAsync(
        string query,
        SearchFilter searchFilter,
        int page,
        bool selectDub)
    {
        throw new Exception("Search method not implemented");
    }

    public virtual async Task<List<Episode>> GetEpisodesAsync(Anime anime)
        => await Task.FromResult(new List<Episode>());

    public virtual async Task<List<VideoServer>> GetVideoServersAsync(Episode episode)
        => await Task.FromResult(new List<VideoServer>());

    public abstract IVideoExtractor GetVideoExtractor(VideoServer server);

    public virtual async Task<List<Video>> GetVideosAsync(VideoServer server)
        => await GetVideoExtractor(server).Extract();

    public virtual async Task<List<Genre>> GetGenresAsync()
        => await Task.FromResult(new List<Genre>());

    public virtual WebHeaderCollection GetDefaultHeaders()
        => new() { { "accept-encoding", "gzip, deflate, br" } };
}