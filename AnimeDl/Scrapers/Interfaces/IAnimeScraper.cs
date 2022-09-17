using System.Threading.Tasks;
using System.Collections.Generic;

namespace AnimeDl.Scrapers.Interfaces;

/// <summary>
/// Endpoints for retrieving information about animes, episodes and videos from the specified site.
/// </summary>
public interface IAnimeScraper
{
    /// <summary>
    /// Checks if site supports dubbed anime
    /// </summary>
    /// <returns></returns>
    public bool GetIsDubAvailableSeparately();

    /// <summary>
    /// Searchs for specific anime
    /// </summary>
    /// <param name="query"></param>
    /// <param name="searchFilter"></param>
    /// <param name="page"></param>
    /// <param name="selectDub"></param>
    /// <returns></returns>
    Task<List<Anime>> SearchAsync(string query, SearchFilter searchFilter, int page, bool selectDub);

    /// <summary>
    /// Gets episodes from specific anime
    /// </summary>
    /// <param name="anime"></param>
    /// <returns></returns>
    Task<List<Episode>> GetEpisodesAsync(Anime anime);

    /// <summary>
    /// Gets episode links from specific episode
    /// </summary>
    /// <param name="episode"></param>
    /// <returns></returns>
    Task<List<Quality>> GetEpisodeLinksAsync(Episode episode);

    /// <summary>
    /// Gets Genres from anime site
    /// </summary>
    /// <returns></returns>
    Task<List<Genre>> GetGenresAsync();
}