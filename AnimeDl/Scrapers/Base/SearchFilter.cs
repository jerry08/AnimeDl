namespace AnimeDl.Scrapers;

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