using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Specialized;
using Newtonsoft.Json.Linq;
using AnimeDl.Models;
using AnimeDl.Helpers;
using AnimeDl.Scrapers;
using AnimeDl.Scrapers.Events;
using AnimeDl.Scrapers.Interfaces;
using AnimeDl.Utils;
using AnimeDl.Utils.Extensions;
using AnimeDl.Anilist;
using AnimeDl.Anilist.Models;
using AnimeDl.Aniskip;

namespace AnimeDl;

/// <summary>
/// Client for interacting with anime sites.
/// </summary>
public class AnimeClient
{
    private readonly IAnimeScraper _scraper;

    private readonly HttpClient _http;

    /// <summary>
    /// Client for interacting with aniskip api.
    /// </summary>
    public AniskipClient Aniskip { get; }

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
    /// Video servers list.
    /// </summary>
    public List<VideoServer> VideoServers { get; private set; } = new();

    /// <summary>
    /// Video links list.
    /// </summary>
    public List<Video> Videos { get; private set; } = new();

    /// <summary>
    /// Genres list.
    /// </summary>
    public List<Genre> Genres { get; private set; } = new();

    /// <summary>
    /// Event after anime search is completed.
    /// </summary>
    public event EventHandler<AnimeEventArgs>? OnAnimesLoaded;
    
    /// <summary>
    /// Event after anime search is completed.
    /// </summary>
    public event EventHandler<AnimeInfoEventArgs>? OnAnimeInfoLoaded;

    /// <summary>
    /// Event after episodes search is completed.
    /// </summary>
    public event EventHandler<EpisodesEventArgs>? OnEpisodesLoaded;

    /// <summary>
    /// Event after video links search is completed.
    /// </summary>
    public event EventHandler<VideoServerEventArgs>? OnVideoServersLoaded;
    
    /// <summary>
    /// Event after video links search is completed.
    /// </summary>
    public event EventHandler<VideoEventArgs>? OnVideosLoaded;

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
        _http = httpClient;
        Aniskip = new(httpClient);

        _scraper = animeSite switch
        {
            AnimeSites.GogoAnime => new GogoAnimeScraper(_http),
            AnimeSites.Zoro => new ZoroScraper(_http),
            AnimeSites.NineAnime => new NineAnimeScraper(_http),
            AnimeSites.Tenshi => new TenshiScraper(_http),
            _ => new GogoAnimeScraper(_http),
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

    /*public async Task<List<Anime>> FindBestMatch(string query)
    {
        var ss = await new AnilistClient().SearchAsync("ANIME", search: "86 season 2");

        var list = new List<Anime>();

        var url = "https://api.myanimelist.net/v2/anime";
        url = "https://api.myanimelist.net/v2/anime?q=one&limit=4";
        var animeURL = "${config.url}/${animeID}?fields=id,title,main_picture,alternative_titles,start_date,end_date,synopsis,mean,rank,popularity,num_list_users,num_scoring_users,status,genres,my_list_status,num_episodes,start_season,background,related_anime,related_manga,studios,";
        var clientId = "057e21278f0a33a664133e70dce8047d";
        var headers = new NameValueCollection()
        {
            { "X-MAL-CLIENT-ID", clientId }
        };

        //Keyword too short
        if (query.Length < 2)
        {

        }
        else if (query.Length == 2)
        {
            query += query[1];
        }

        query = query.Replace(" ", "%20");

        //url = "https://myanimelist.net/anime.php?cat=anime&q=86%20&type=0&score=0&status=0&p=0&r=0&sm=0&sd=0&sy=0&em=0&ed=0&ey=0&c%5B%5D=a&c%5B%5D=b&c%5B%5D=c&c%5B%5D=f";
        url = $"https://api.myanimelist.net/v2/anime?q={query}";

        url = "https://api.myanimelist.net/v2/anime/21?fields=id,title,main_picture,alternative_titles,start_date,end_date,synopsis,mean,rank,popularity,num_list_users,num_scoring_users,status,genres,my_list_status,num_episodes,start_season,background,related_anime,related_manga,studios,";

        //var client = new HttpClient();
        var tt = await _http.SendHttpRequestAsync(url, headers);

        return list;
    }

    [Obsolete("test")]
    public virtual async Task<Anime?> AutoSearch(Media mediaObj)
    {
        var response = await SearchAsync(mediaObj.Name!);
        //response ??= await SearchAsync(mediaObj.NameRomaji!);
        response = response.Count <= 0 ? await SearchAsync(mediaObj.NameRomaji!) : response;

        if (response is null)
            return null;

        response = response.OrderBy(x => LevenshteinDistance.Compute(
            mediaObj.Name!.ToLower(), x.Title.ToLower())).ToList();

        if (response.Count <= 0 || LevenshteinDistance.Compute(
            mediaObj.Name!.ToLower(), response[0].Title.ToLower()) > 1)
        {
            var idMal = mediaObj.IdMal;

            //Find by mal
            var url = $"https://api.myanimelist.net/v2/anime/{idMal}?fields=id,title,main_picture,alternative_titles,start_date,end_date,synopsis,mean,rank,popularity,num_list_users,num_scoring_users,status,genres,my_list_status,num_episodes,start_season,background,related_anime,related_manga,studios,";

            var clientId = "057e21278f0a33a664133e70dce8047d";
            var headers = new NameValueCollection()
            {
                { "X-MAL-CLIENT-ID", clientId }
            };

            var tt = await _http.SendHttpRequestAsync(url, headers);

            var title = JObject.Parse(tt)?["title"]?.ToString();
            response = await SearchAsync(title!);

            response = response.OrderBy(x => LevenshteinDistance.Compute(
                title!.ToLower(), x.Title.ToLower())).ToList();
        }

        //for (int i = 0; i < response.Count; i++)
        //{
        //    var anime = response[i];
        //
        //    var distance = LevenshteinDistance.Compute(
        //        mediaObj.Name!.ToLower(), anime.Title.ToLower());
        //}

        return response[0];
    }*/

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
        return Search(query, SearchFilter.Find, 0, forceLoad);
    }

    /// <summary>
    /// Search for animes.
    /// </summary>
    public List<Anime> Search(
        string query,
        bool selectDub,
        bool forceLoad = false)
    {
        return Search(query, SearchFilter.Find, 0, forceLoad, selectDub);
    }

    /// <summary>
    /// Search for animes.
    /// </summary>
    public List<Anime> Search(
        string query,
        SearchFilter searchFilter,
        bool forceLoad = false)
    {
        return Search(query, searchFilter, 0, forceLoad);
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
            _searchCancellationTokenSource = new();

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
            }, _searchCancellationTokenSource.Token);
        }

        return Animes;
    }

    /// <summary>
    /// Search for animes.
    /// </summary>
    public async Task<List<Anime>> SearchAsync(
        string query)
    {
        return Animes = await SearchAsync(query, SearchFilter.Find, 1, false);
    }

    /// <summary>
    /// Search for animes.
    /// </summary>
    public async Task<List<Anime>> SearchAsync(
        string query,
        SearchFilter searchFilter)
    {
        return Animes = await SearchAsync(query, searchFilter, 1, false);
    }

    /// <summary>
    /// Search for animes.
    /// </summary>
    public async Task<List<Anime>> SearchAsync(string query, bool selectDub)
    {
        return Animes = await SearchAsync(query, SearchFilter.Find, 0, selectDub);
    }

    /// <summary>
    /// Search for animes.
    /// </summary>
    public async Task<List<Anime>> SearchAsync(SearchFilter searchFilter)
    {
        return Animes = await SearchAsync("", searchFilter, 0, false);
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
        if (searchFilter == SearchFilter.Find && string.IsNullOrEmpty(query))
            return new();

        return Animes = await _scraper.SearchAsync(query, searchFilter, page, selectDub);
    }
    #endregion

    #region Get Episodes
    /// <summary>
    /// Bool to check if episodes search is completed.
    /// </summary>
    public virtual bool IsLoadingEpisodes { get; protected set; }
    
    private CancellationTokenSource _episodesCancellationTokenSource = new();

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
    public List<Episode> GetEpisodes(string id, bool forceLoad = false)
    {
        IsLoadingEpisodes = true;

        if (_episodesCancellationTokenSource.IsCancellationRequested)
            _episodesCancellationTokenSource = new();

        var function = () => GetEpisodesAsync(id);
        if (forceLoad)
        {
            Episodes = AsyncHelper.RunSync(function, _episodesCancellationTokenSource.Token);
        }
        else
        {
            function().ContinueWith(t =>
            {
                OnEpisodesLoaded?.Invoke(this, new EpisodesEventArgs(Episodes));
            }, _episodesCancellationTokenSource.Token);
        }

        return Episodes;
    }

    /// <summary>
    /// Search for episodes.
    /// </summary>
    public async Task<List<Episode>> GetEpisodesAsync(string id)
    {
        Episodes = await _scraper.GetEpisodesAsync(id);
        return Episodes;
    }
    #endregion

    #region Get Anime Info
    /// <summary>
    /// Bool to check if loading anime info is completed.
    /// </summary>
    public virtual bool IsLoadingAnimeInfo { get; protected set; }

    private CancellationTokenSource _animeInfoCancellationTokenSource = new();

    /// <summary>
    /// Cancel loading anime info.
    /// </summary>
    public void CancelGetAnimeInfo()
    {
        if (!_animeInfoCancellationTokenSource.IsCancellationRequested)
            _animeInfoCancellationTokenSource.Cancel();
        IsLoadingAnimeInfo = false;
    }

    /// <summary>
    /// Search for anime info.
    /// </summary>
    public Anime GetAnimeInfo(string id, bool forceLoad = false)
    {
        IsLoadingAnimeInfo = true;

        if (_animeInfoCancellationTokenSource.IsCancellationRequested)
            _animeInfoCancellationTokenSource = new();

        var function = () => GetAnimeInfoAsync(id);
        if (forceLoad)
        {
            return AsyncHelper.RunSync(function, _episodesCancellationTokenSource.Token);
        }
        else
        {
            function().ContinueWith(t =>
            {
                OnAnimeInfoLoaded?.Invoke(this, new(t.Result));
            }, _episodesCancellationTokenSource.Token);
        }

        return new();
    }

    /// <summary>
    /// Search for anime info.
    /// </summary>
    public async Task<Anime> GetAnimeInfoAsync(string id)
    {
        return await _scraper.GetAnimeInfoAsync(id);
    }
    #endregion

    #region Video Servers
    /// <summary>
    /// Bool to check if video links search is completed.
    /// </summary>
    public virtual bool IsLoadingVideoServers { get; protected set; }

    private CancellationTokenSource _videoServersCancellationTokenSource = new();

    /// <summary>
    /// Cancel video links search.
    /// </summary>
    public void CancelGetVideoServers()
    {
        if (!_videoServersCancellationTokenSource.IsCancellationRequested)
            _videoServersCancellationTokenSource.Cancel();
        IsLoadingVideoServers = false;
    }

    /// <summary>
    /// Search for video links.
    /// </summary>
    public List<VideoServer> GetVideoServers(string episodeId, bool forceLoad = false)
    {
        IsLoadingVideoServers = true;

        if (_videoServersCancellationTokenSource.IsCancellationRequested)
            _videoServersCancellationTokenSource = new();

        var function = () => GetVideoServersAsync(episodeId);
        if (forceLoad)
        {
            VideoServers = AsyncHelper.RunSync(function, _videoServersCancellationTokenSource.Token);
        }
        else
        {
            function().ContinueWith(t =>
            {
                OnVideoServersLoaded?.Invoke(this, new VideoServerEventArgs(VideoServers));
            }, _videoServersCancellationTokenSource.Token);
        }

        return VideoServers;
    }

    /// <summary>
    /// Search for video links.
    /// </summary>
    public async Task<List<VideoServer>> GetVideoServersAsync(string episodeId)
    {
        return VideoServers = await _scraper.GetVideoServersAsync(episodeId);
    }
    #endregion

    #region Videos
    /// <summary>
    /// Bool to check if video links search is completed.
    /// </summary>
    public virtual bool IsLoadingVideos { get; protected set; }
    
    private CancellationTokenSource _videosCancellationTokenSource = new();

    /// <summary>
    /// Cancel videos search.
    /// </summary>
    public void CancelGetVideos()
    {
        if (!_videosCancellationTokenSource.IsCancellationRequested)
            _videosCancellationTokenSource.Cancel();
        IsLoadingVideos = false;
    }

    /// <summary>
    /// Search for videos.
    /// </summary>
    public List<Video> GetVideos(VideoServer server,
        bool showSizeIfAvailable = true, bool forceLoad = false)
    {
        IsLoadingVideos = true;

        if (_videosCancellationTokenSource.IsCancellationRequested)
            _videosCancellationTokenSource = new();

        var function = () => GetVideosAsync(server, showSizeIfAvailable);
        if (forceLoad)
        {
            Videos = AsyncHelper.RunSync(function, _videosCancellationTokenSource.Token);
        }
        else
        {
            function().ContinueWith(t =>
            {
                OnVideosLoaded?.Invoke(this, new VideoEventArgs(Videos, server));
            }, _videosCancellationTokenSource.Token);
        }

        return Videos;
    }

    /// <summary>
    /// Search for video links.
    /// </summary>
    public async Task<List<Video>> GetVideosAsync(VideoServer server,
        bool showSizeIfAvailable = true)
    {
        Videos = await _scraper.GetVideosAsync(server);
        
        if (showSizeIfAvailable)
        {
            foreach (var video in Videos)
            {
                if (video.Format == VideoType.Container)
                    video.Size = await _http.GetFileSizeAsync(video.VideoUrl, video.Headers);
            }
        }

        return Videos;
    }
    #endregion

    #region Search for Anime Genres
    /// <summary>
    /// Bool to check if genres search is completed.
    /// </summary>
    public virtual bool IsLoadingAllGenres { get; protected set; }
    
    private CancellationTokenSource _genresCancellationTokenSource = new();

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
            _genresCancellationTokenSource = new();

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
            }, _genresCancellationTokenSource.Token);
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
        string url,
        NameValueCollection headers,
        string filePath,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        AsyncHelper.RunSync(() => DownloadAsync(url, headers, filePath, progress, cancellationToken), cancellationToken);
    }

    /// <summary>
    /// Downloads an episode
    /// </summary>
    public async Task DownloadAsync(
        string url,
        NameValueCollection headers,
        string filePath,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        for (int j = 0; j < headers.Count; j++)
            request.Headers.TryAddWithoutValidation(headers.Keys[j]!, headers[j]);

        using var response = await _http.SendAsync(
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
            await _http.GetFileSizeAsync(url,
                headers, cancellationToken) : 0;

        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        //Create a stream for the file
        var file = File.Create(filePath);

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
                await file.WriteAsync(buffer, 0, length);

                // Flush the data
                await stream.FlushAsync();

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
        VideoServers = new();
        Videos = new();
        Genres = new();
    }
}