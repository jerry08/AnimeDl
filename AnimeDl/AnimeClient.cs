using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using AnimeDl.Helpers;
using AnimeDl.Scrapers;
using AnimeDl.Scrapers.Events;
using AnimeDl.Scrapers.Interfaces;
using AnimeDl.Utils.Extensions;

namespace AnimeDl;

/// <summary>
/// Client for interacting with anime sites.
/// </summary>
public class AnimeClient
{
    private readonly IAnimeScraper _scraper;

    private readonly HttpClient _httpClient;
    private readonly NetHttpClient _netHttpClient;

    /// <summary>
    /// Checks if site supports dubbed anime
    /// </summary>
    /// <returns></returns>
    public bool IsDubAvailableSeparately => _scraper.GetIsDubAvailableSeparately();

    /// <summary>
    /// Returns the current anime site.
    /// </summary>
    public AnimeSites Site { get; private set; }
    
    /// <summary>
    /// Anime list.
    /// </summary>
    public List<Anime> Animes { get; private set; } = new();

    /// <summary>
    /// Episodes list.
    /// </summary>
    public List<Episode> Episodes { get; private set; } = new();

    /// <summary>
    /// Video links list.
    /// </summary>
    public List<Quality> Qualities { get; private set; } = new();

    /// <summary>
    /// Genres list.
    /// </summary>
    public List<Genre> Genres { get; private set; } = new();

    /// <summary>
    /// Event after anime search is completed.
    /// </summary>
    public event EventHandler<AnimeEventArgs>? OnAnimesLoaded;

    /// <summary>
    /// Event after episodes search is completed.
    /// </summary>
    public event EventHandler<EpisodesEventArgs>? OnEpisodesLoaded;

    /// <summary>
    /// Event after video links search is completed.
    /// </summary>
    public event EventHandler<QualityEventArgs>? OnQualitiesLoaded;

    /// <summary>
    /// Event after genres search is completed.
    /// </summary>
    public event EventHandler<GenreEventArgs>? OnGenresLoaded;

    /// <summary>
    /// Initializes an instance of <see cref="AnimeClient"/>.
    /// </summary>
    public AnimeClient(AnimeSites animeSite, HttpClient httpClient)
    {
        Site = animeSite;
        _httpClient = httpClient;
        _netHttpClient = new NetHttpClient(_httpClient);

        _scraper = animeSite switch
        {
            AnimeSites.GogoAnime => new GogoAnimeScraper(_netHttpClient),
            AnimeSites.TwistMoe => new TwistScraper(_netHttpClient),
            AnimeSites.Zoro => new ZoroScraper(_netHttpClient),
            AnimeSites.NineAnime => new NineAnimeScraper(_netHttpClient),
            AnimeSites.Tenshi => new TenshiScraper(_netHttpClient),
            _ => new GogoAnimeScraper(_netHttpClient),
        };
    }

    /// <summary>
    /// Initializes an instance of <see cref="AnimeClient"/>.
    /// </summary>
    public AnimeClient() : this(AnimeSites.GogoAnime, Http.Client)
    {
    }

    /// <summary>
    /// Initializes an instance of <see cref="AnimeClient"/>.
    /// </summary>
    public AnimeClient(AnimeSites animeSite) : this(animeSite, Http.Client)
    {
    }

    #region Search for Animes
    /// <summary>
    /// Bool to check if anime search is completed.
    /// </summary>
    public virtual bool IsLoadingAnimes { get; protected set; }

    private CancellationTokenSource _searchCancellationTokenSource = new();

    /// <summary>
    /// Cancel anime search.
    /// </summary>
    public void CancelSearch()
    {
        if (!_searchCancellationTokenSource.IsCancellationRequested)
            _searchCancellationTokenSource.Cancel();
        IsLoadingAnimes = false;
    }

    /// <summary>
    /// Search for animes.
    /// </summary>
    public List<Anime> Search(
        string query,
        bool forceLoad = false)
    {
        return Search(query, SearchFilter.Find, 1, forceLoad);
    }

    /// <summary>
    /// Search for animes.
    /// </summary>
    public List<Anime> Search(
        string query,
        SearchFilter searchFilter,
        bool forceLoad = false)
    {
        return Search(query, searchFilter, 1, forceLoad);
    }

    /// <summary>
    /// Search for animes.
    /// </summary>
    public List<Anime> Search(
        string query,
        SearchFilter searchFilter,
        int page,
        bool selectDub = false,
        bool forceLoad = false)
    {
        IsLoadingAnimes = true;

        if (_searchCancellationTokenSource.IsCancellationRequested)
            _searchCancellationTokenSource = new CancellationTokenSource();

        var function = () => SearchAsync(query, searchFilter, page, selectDub);
        if (forceLoad)
        {
            Animes = AsyncHelper.RunSync(function, _searchCancellationTokenSource.Token);
        }
        else
        {
            function().ContinueWith(t =>
            {
                OnAnimesLoaded?.Invoke(this, new AnimeEventArgs(Animes));
            }, _searchCancellationTokenSource.Token, TaskContinuationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
        }

        return Animes;
    }

    /// <summary>
    /// Search for animes.
    /// </summary>
    public async Task<List<Anime>> SearchAsync(
        string query)
    {
        return Animes = await _scraper.SearchAsync(query, SearchFilter.Find, 1, false);
    }

    /// <summary>
    /// Search for animes.
    /// </summary>
    public async Task<List<Anime>> SearchAsync(
        string query,
        SearchFilter searchFilter)
    {
        return Animes = await _scraper.SearchAsync(query, searchFilter, 1, false);
    }

    /// <summary>
    /// Search for animes.
    /// </summary>
    public async Task<List<Anime>> SearchAsync(
        string query,
        SearchFilter searchFilter,
        int page,
        bool selectDub)
    {
        return Animes = await _scraper.SearchAsync(query, searchFilter, page, selectDub);
    }

    public async Task<List<Anime>> SearchAsync(string query, bool selectDub)
    {
        return Animes = await SearchAsync(query, SearchFilter.Find, 0, selectDub);
    }

    public async Task<List<Anime>> SearchAsync(SearchFilter searchFilter)
    {
        return Animes = await SearchAsync("", searchFilter, 0, false);
    }
    #endregion

    #region Get Episodes and Anime details
    /// <summary>
    /// Bool to check if episodes search is completed.
    /// </summary>
    public virtual bool IsLoadingEpisodes { get; protected set; }
    
    private CancellationTokenSource _episodesCancellationTokenSource = new CancellationTokenSource();

    /// <summary>
    /// Cancel episodes search.
    /// </summary>
    public void CancelGetEpisodes()
    {
        if (!_episodesCancellationTokenSource.IsCancellationRequested)
            _episodesCancellationTokenSource.Cancel();
        IsLoadingEpisodes = false;
    }

    /// <summary>
    /// Search for episodes.
    /// </summary>
    public List<Episode> GetEpisodes(Anime anime, bool forceLoad = false)
    {
        IsLoadingEpisodes = true;

        if (_episodesCancellationTokenSource.IsCancellationRequested)
            _episodesCancellationTokenSource = new CancellationTokenSource();

        var function = () => GetEpisodesAsync(anime);
        if (forceLoad)
        {
            Episodes = AsyncHelper.RunSync(function, _episodesCancellationTokenSource.Token);
        }
        else
        {
            function().ContinueWith(t =>
            {
                OnEpisodesLoaded?.Invoke(this, new EpisodesEventArgs(anime, Episodes));
            }, _episodesCancellationTokenSource.Token, TaskContinuationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
        }

        return Episodes;
    }

    /// <summary>
    /// Search for episodes.
    /// </summary>
    public async Task<List<Episode>> GetEpisodesAsync(Anime anime)
    {
        Episodes = await _scraper.GetEpisodesAsync(anime);
        return Episodes;
    }
    #endregion

    #region Episode/Video Links
    /// <summary>
    /// Bool to check if video links search is completed.
    /// </summary>
    public virtual bool IsLoadingEpisodeLinks { get; protected set; }
    
    private CancellationTokenSource _linksCancellationTokenSource = new CancellationTokenSource();

    /// <summary>
    /// Cancel video links search.
    /// </summary>
    public void CancelGetEpisodeLinks()
    {
        if (!_linksCancellationTokenSource.IsCancellationRequested)
            _linksCancellationTokenSource.Cancel();
        IsLoadingEpisodeLinks = false;
    }

    /// <summary>
    /// Search for video links.
    /// </summary>
    public List<Quality> GetEpisodeLinks(Episode episode, bool forceLoad = false)
    {
        IsLoadingEpisodeLinks = true;

        if (_linksCancellationTokenSource.IsCancellationRequested)
            _linksCancellationTokenSource = new CancellationTokenSource();

        var function = () => GetEpisodeLinksAsync(episode);
        if (forceLoad)
        {
            Qualities = AsyncHelper.RunSync(function, _linksCancellationTokenSource.Token);   
        }
        else
        {
            function().ContinueWith(t =>
            {
                OnQualitiesLoaded?.Invoke(this, new QualityEventArgs(Qualities));
            }, _linksCancellationTokenSource.Token, TaskContinuationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
        }

        return Qualities;
    }

    /// <summary>
    /// Search for video links.
    /// </summary>
    public async Task<List<Quality>> GetEpisodeLinksAsync(Episode episode)
    {
        Qualities = await _scraper.GetEpisodeLinksAsync(episode);
        return Qualities;
    }
    #endregion

    #region Search for Anime Genres
    /// <summary>
    /// Bool to check if genres search is completed.
    /// </summary>
    public virtual bool IsLoadingAllGenres { get; protected set; }
    
    private CancellationTokenSource _genresCancellationTokenSource = new CancellationTokenSource();

    /// <summary>
    /// Cancel genres search.
    /// </summary>
    public void CancelGetGenres()
    {
        if (!_genresCancellationTokenSource.IsCancellationRequested)
            _genresCancellationTokenSource.Cancel();
        IsLoadingAllGenres = false;
    }

    /// <summary>
    /// Search for genres.
    /// </summary>
    public List<Genre> GetGenres(bool forceLoad = false)
    {
        IsLoadingAllGenres = true;

        if (_genresCancellationTokenSource.IsCancellationRequested)
            _genresCancellationTokenSource = new CancellationTokenSource();

        var function = () => GetGenresAsync();
        if (forceLoad)
        {
            Genres = AsyncHelper.RunSync(function, _genresCancellationTokenSource.Token);
        }
        else
        {
            function().ContinueWith(t =>
            {
                OnGenresLoaded?.Invoke(this, new GenreEventArgs(Genres));
            }, _genresCancellationTokenSource.Token, TaskContinuationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
        }

        return Genres;
    }

    /// <summary>
    /// Search for genres.
    /// </summary>
    public async Task<List<Genre>> GetGenresAsync()
    {
        Genres = await _scraper.GetGenresAsync();
        return Genres;
    }
    #endregion

    /// <summary>
    /// Downloads an episode
    /// </summary>
    public void Download(
        Quality quality,
        string filePath,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        AsyncHelper.RunSync(() => DownloadAsync(quality, filePath, progress, cancellationToken));
    }

    /// <summary>
    /// Downloads an episode
    /// </summary>
    public async Task DownloadAsync(
        Quality quality,
        string filePath,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, quality.QualityUrl);
        for (int j = 0; j < quality.Headers.Count; j++)
        {
            request.Headers.TryAddWithoutValidation(quality.Headers.Keys[j]!, quality.Headers[j]);
        }

        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken
        );

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Response status code does not indicate success: {(int)response.StatusCode} ({response.StatusCode})." +
                Environment.NewLine +
                "Request:" +
                Environment.NewLine +
                request
            );
        }

        long totalLength = progress is not null ?
            await _netHttpClient.GetFileSizeAsync(quality.QualityUrl,
                quality.Headers, cancellationToken) : 0;

        var stream = await response.Content.ReadAsStreamAsync();

        //Create a stream for the file
        var file = File.Create(filePath.ReplaceInvalidChars());

        try
        {
            //This controls how many bytes to read at a time and send to the client
            int bytesToRead = 10000;

            // Buffer to read bytes in chunk size specified above
            byte[] buffer = new byte[bytesToRead];

            int length;
            do
            {
                // Read data into the buffer.
                length = stream.Read(buffer, 0, bytesToRead);

                // and write it out to the response's output stream
                file.Write(buffer, 0, length);

                // Flush the data
                stream.Flush();

                //Clear the buffer
                buffer = new byte[bytesToRead];

                progress?.Report(((double)file.Length / (double)totalLength * 100) / 100);
            } while (length > 0); //Repeat until no data is read
        }
        finally
        {
            file?.Close();
            stream?.Close();
        }
    }

    /// <summary>
    /// Clear AnimeClient lists.
    /// </summary>
    public void Clear()
    {
        Animes = new();
        Episodes = new();
        Qualities = new();
        Genres = new();
    }
}