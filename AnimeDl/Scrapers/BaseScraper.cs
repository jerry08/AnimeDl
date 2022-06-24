using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using AnimeDl.Scrapers.Interfaces;
using System;

namespace AnimeDl.Scrapers;

public enum AnimeSites
{
    GogoAnime,
    TwistMoe,
    Zoro,
    NineAnime,
    Tenshi
}

//For Gogoanime
public enum SearchType
{
    Find,
    AllList,
    Popular,
    Ongoing,
    NewSeason,
    LastUpdated,
    Trending,
    Movies
}

internal abstract class BaseScraper : IAnimeScraper
{
    public virtual string BaseUrl => default!;

    public readonly NetHttpClient _netHttpClient;

    public BaseScraper(NetHttpClient netHttpClient)
    {
        _netHttpClient = netHttpClient;
    }

    public virtual Task<List<Anime>> SearchAsync(string searchQuery, SearchType searchType, int page)
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
        //var headers = new NameValueCollection();
        var headers = new WebHeaderCollection
        {
            { "accept-encoding", "gzip, deflate, br" }
        };

        return headers;
    }
}