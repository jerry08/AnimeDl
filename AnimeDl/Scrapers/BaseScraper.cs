using System;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using AnimeDl.Scrapers.Interfaces;

namespace AnimeDl.Scrapers;

internal abstract class BaseScraper : IAnimeScraper
{
    public abstract string Name { get; set; }

    public virtual string BaseUrl => default!;

    public abstract bool IsDubAvailableSeparately { get; set; }

    public readonly NetHttpClient _netHttpClient;

    public BaseScraper(NetHttpClient netHttpClient)
    {
        _netHttpClient = netHttpClient;
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

    public virtual async Task<List<Quality>> GetEpisodeLinksAsync(Episode episode)
    {
        return await Task.FromResult(new List<Quality>());
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