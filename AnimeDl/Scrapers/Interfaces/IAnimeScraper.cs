using System.Threading.Tasks;
using System.Collections.Generic;
using AnimeDl.Models;

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
    /// Gets video servers from specific episode
    /// </summary>
    /// <param name="episode"></param>
    /// <returns></returns>
    Task<List<VideoServer>> GetVideoServersAsync(Episode episode);

    /// <summary>
    /// Gets videos from specific server
    /// </summary>
    /// <param name="server"></param>
    /// <returns></returns>
    Task<List<Video>> GetVideosAsync(VideoServer server);

    /// <summary>
    /// Gets Genres from anime site
    /// </summary>
    /// <returns></returns>
    Task<List<Genre>> GetGenresAsync();
}