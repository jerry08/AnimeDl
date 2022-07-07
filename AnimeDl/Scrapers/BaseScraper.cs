using System;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using AnimeDl.Scrapers.Interfaces;

namespace AnimeDl.Scrapers;

/// <summary>
/// Filter applied to a anime sites.
/// </summary>
public enum AnimeSites
{
    /// <summary>
    /// Parses anime and information from GogoAnime.
    /// </summary>
    GogoAnime,

    /// <summary>
    /// Parses anime and information from TwistMoe.
    /// </summary>
    TwistMoe,

    /// <summary>
    /// Parses anime and information from Zoro.
    /// </summary>
    Zoro,

    /// <summary>
    /// Parses anime and information from NineAnime.
    /// </summary>
    NineAnime,

    /// <summary>
    /// Parses anime and information from Tenshi.
    /// </summary>
    Tenshi
}

/// <summary>
/// Filter applied to a anime search query.
/// </summary>
public enum SearchFilter
{
    /// <summary>
    /// No filter applied.
    /// </summary>
    None,

    /// <summary>
    /// Search by query.
    /// </summary>
    Find,

    /// <summary>
    /// Search for all animes.
    /// </summary>
    AllList,

    /// <summary>
    /// Search for popular animes.
    /// </summary>
    Popular,

    /// <summary>
    /// Search for ongoing animes.
    /// </summary>
    Ongoing,

    /// <summary>
    /// Search for animes in new season.
    /// </summary>
    NewSeason,

    /// <summary>
    /// Search for last updated animes.
    /// </summary>
    LastUpdated,

    /// <summary>
    /// Search for trending animes.
    /// </summary>
    Trending,

    /// <summary>
    /// Search for anime movies.
    /// </summary>
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

    public virtual Task<List<Anime>> SearchAsync(string searchQuery, SearchFilter searchFilter, int page)
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