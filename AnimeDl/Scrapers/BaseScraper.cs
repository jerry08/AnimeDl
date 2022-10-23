using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using AnimeDl.Models;
using AnimeDl.Scrapers.Interfaces;
using AnimeDl.Extractors.Interfaces;

namespace AnimeDl.Scrapers;

/// <summary>
/// Base scraper for interacting with anime sites.
/// </summary>
public abstract class BaseScraper : IAnimeScraper
{
    public abstract string Name { get; set; }

    public virtual string BaseUrl => default!;

    public abstract bool IsDubAvailableSeparately { get; set; }

    public bool GetIsDubAvailableSeparately() => IsDubAvailableSeparately;

    public virtual HttpClient _http { get; set; }

    public BaseScraper(HttpClient http)
        => _http = http;

    public abstract Task<List<Anime>> SearchAsync(string query,
        SearchFilter searchFilter, int page, bool selectDub);

    public virtual async Task<Anime> GetAnimeInfoAsync(string id)
        => await Task.FromResult(new Anime());

    public abstract Task<List<Episode>> GetEpisodesAsync(string id);

    public abstract Task<List<VideoServer>> GetVideoServersAsync(string episodeId);

    public abstract IVideoExtractor GetVideoExtractor(VideoServer server);

    public virtual async Task<List<Video>> GetVideosAsync(VideoServer server)
        => await GetVideoExtractor(server).Extract();

    public virtual async Task<List<Genre>> GetGenresAsync()
        => await Task.FromResult(new List<Genre>());

    public virtual WebHeaderCollection GetDefaultHeaders()
        => new() { { "accept-encoding", "gzip, deflate, br" } };
}