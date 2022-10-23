using System.Threading.Tasks;
using System.Collections.Generic;
using AnimeDl.Models;
using AnimeDl.Extractors.Interfaces;

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
    /// <param name="id"></param>
    /// <returns></returns>
    Task<Anime> GetAnimeInfoAsync(string id);

    /// <summary>
    /// Gets episodes from specific anime
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<List<Episode>> GetEpisodesAsync(string id);

    /// <summary>
    /// Gets video servers from specific episode
    /// </summary>
    /// <param name="episodeId"></param>
    /// <returns></returns>
    Task<List<VideoServer>> GetVideoServersAsync(string episodeId);

    /// <summary>
    /// Gets video extractor from specific server
    /// </summary>
    /// <param name="server"></param>
    /// <returns></returns>
    IVideoExtractor GetVideoExtractor(VideoServer server);

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