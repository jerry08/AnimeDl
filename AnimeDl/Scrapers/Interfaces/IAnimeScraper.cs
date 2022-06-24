using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnimeDl.Scrapers.Interfaces;

/// <summary>
/// Endpoints for retrieving information about animes, episodes and videos from the specified site.
/// </summary>
public interface IAnimeScraper
{
    /// <summary>
    /// Get Spotify Catalog information about albums, artists, playlists,
    /// tracks, shows or episodes that match a keyword string.
    /// </summary>
    /// <param name="searchQuery">The request-model which contains required and optional parameters.</param>
    /// <remarks>
    /// https://developer.spotify.com/documentation/web-api/reference-beta/#endpoint-search
    /// </remarks>
    /// <returns></returns>
    Task<List<Anime>> SearchAsync(string searchQuery, SearchType searchType, int page);

    Task<List<Episode>> GetEpisodesAsync(Anime anime);

    Task<List<Quality>> GetEpisodeLinksAsync(Episode episode);

    Task<List<Genre>> GetGenresAsync();
}