using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using AnimeDl.Models;
using AnimeDl.Scrapers.Interfaces;

namespace AnimeDl.Scrapers;

internal abstract class BaseScraper : IAnimeScraper
{
    public abstract string Name { get; set; }

    public virtual string BaseUrl => default!;

    public abstract bool IsDubAvailableSeparately { get; set; }

    public readonly HttpClient _http;

    public BaseScraper(HttpClient http)
    {
        _http = http;
    }

    public virtual Task<List<Anime>> SearchAsync(
        string query,
        SearchFilter searchFilter,
        int page,
        bool selectDub)
    {
        throw new Exception("Search method not implemented");
    }

    public virtual async Task<List<Episode>> GetEpisodesAsync(Anime anime)
    {
        return await Task.FromResult(new List<Episode>());
    }

    public virtual async Task<List<VideoServer>> GetVideoServersAsync(Episode episode)
    {
        return await Task.FromResult(new List<VideoServer>());
    }

    public virtual async Task<List<Video>> GetVideosAsync(VideoServer server)
    {
        return await Task.FromResult(new List<Video>());
    }

    public virtual async Task<List<Genre>> GetGenresAsync()
    {
        return await Task.FromResult(new List<Genre>());
    }

    public virtual WebHeaderCollection GetDefaultHeaders()
    {
        var headers = new WebHeaderCollection
        {
            { "accept-encoding", "gzip, deflate, br" }
        };

        return headers;
    }

    public bool GetIsDubAvailableSeparately() => IsDubAvailableSeparately;
}