using System;
using System.Collections.Generic;
using System.Linq;
using AnimeDl.Scrapers;
using System.Threading;
using System.Threading.Tasks;
using AnimeDl.Scrapers.Events;

namespace AnimeDl
{
    public class AnimeScraper
    {
        public AnimeSites CurrentSite { get; private set; }

        private List<BaseScraper> AnimeScrapers { get; set; }
        
        private BaseScraper CurrentAnimeScraper
        {
            get
            {
                switch (CurrentSite)
                {
                    case AnimeSites.GogoAnime:
                        return AnimeScrapers.Where(x => x is GogoAnimeScraper).FirstOrDefault();
                    case AnimeSites.TwistMoe:
                        return AnimeScrapers.Where(x => x is TwistScraper).FirstOrDefault();
                    case AnimeSites.Zoro:
                        return AnimeScrapers.Where(x => x is ZoroScraper).FirstOrDefault();
                    case AnimeSites.NineAnime:
                        return AnimeScrapers.Where(x => x is NineAnimeScraper).FirstOrDefault();
                    case AnimeSites.Tenshi:
                        return AnimeScrapers.Where(x => x is TenshiScraper).FirstOrDefault();
                    default:
                        return AnimeScrapers.Where(x => x is GogoAnimeScraper).FirstOrDefault();
                }
            }
        }

        public List<Anime> Animes { get; set; } = new List<Anime>();
        public List<Episode> Episodes { get; set; } = new List<Episode>();
        public List<Quality> Qualities { get; set; } = new List<Quality>();
        public List<Genre> Genres { get; set; } = new List<Genre>();

        public event EventHandler<AnimeEventArgs> OnAnimesLoaded;
        public event EventHandler<EpisodesEventArgs> OnEpisodesLoaded;
        public event EventHandler<QualityEventArgs> OnQualitiesLoaded;
        public event EventHandler<GenreEventArgs> OnGenresLoaded;

        /// <summary>
        /// Applies to GogoAnime only
        /// </summary>
        public int Page { get; set; } = 1;

        public AnimeScraper(AnimeSites animeSite = AnimeSites.GogoAnime)
        {
            CurrentSite = animeSite;

            AnimeScrapers = new List<BaseScraper>
            {
                new GogoAnimeScraper(),
                new TwistScraper(),
                new ZoroScraper(),
                new NineAnimeScraper(),
                new TenshiScraper()
            };
        }

        #region Search for Anime
        public virtual bool IsLoadingAnimes { get; protected set; }
        CancellationTokenSource SearchTokenSource = new CancellationTokenSource();
        public void CancelSearch()
        {
            if (!SearchTokenSource.IsCancellationRequested)
                SearchTokenSource.Cancel();
            IsLoadingAnimes = false;
        }

        public List<Anime> Search(string searchText, SearchType searchType = SearchType.Find, 
            bool forceLoad = false)
        {
            IsLoadingAnimes = true;

            if (SearchTokenSource.IsCancellationRequested)
                SearchTokenSource = new CancellationTokenSource();

            var task = Task.Run(() => SearchAsync(searchText, searchType), SearchTokenSource.Token);

            if (forceLoad)
            {
                //task.Wait();

                while (!task.IsCompleted) {
                }

                if (task.IsFaulted)
                {
                    throw task.Exception;
                }

                IsLoadingAnimes = false;
                Animes = task.Result;
                return Animes;
            }

            //setup delegate to invoke when the background task completes
            task.ContinueWith(t =>
            {
                //this will execute when the background task has completed
                if (t.IsFaulted || t.IsCanceled)
                {
                    //somehow handle exception in t.Exception
                    return;
                }

                Animes = t.Result;
                IsLoadingAnimes = false;

                OnAnimesLoaded?.Invoke(this, new AnimeEventArgs(Animes));
            //}, TaskScheduler.FromCurrentSynchronizationContext());
            }, SearchTokenSource.Token, TaskContinuationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());

            return null;

            //var token = tokenSource.Token;
            //Task.Run(() => SearchAsync(searchText, searchType), token);
        }

        public async Task<List<Anime>> SearchAsync(string searchText, 
            SearchType searchType = SearchType.Find)
        {
            Animes = await CurrentAnimeScraper.SearchAsync(searchText, searchType, Page);

            //return await CurrentAnimeScraper.SearchAsync(searchText, searchType, Page);
            return Animes;
        }
        #endregion

        #region Get Episodes and Anime details
        public virtual bool IsLoadingEpisodes { get; protected set; }
        CancellationTokenSource GetEpisodesTokenSource = new CancellationTokenSource();
        public void CancelGetEpisodes()
        {
            if (!GetEpisodesTokenSource.IsCancellationRequested)
                GetEpisodesTokenSource.Cancel();
            IsLoadingEpisodes = false;
        }

        public List<Episode> GetEpisodes(Anime anime, bool forceLoad = false)
        {
            IsLoadingEpisodes = true;

            if (GetEpisodesTokenSource.IsCancellationRequested)
                GetEpisodesTokenSource = new CancellationTokenSource();

            var task = Task.Run(() => GetEpisodesAsync(anime), GetEpisodesTokenSource.Token);

            if (forceLoad)
            {
                task.Wait();
                IsLoadingEpisodes = false;
                Episodes = task.Result;
                return Episodes;
            }

            //setup delegate to invoke when the background task completes
            task.ContinueWith(t =>
            {
                //this will execute when the background task has completed
                if (t.IsFaulted || t.IsCanceled)
                {
                    //somehow handle exception in t.Exception
                    return;
                }

                Episodes = t.Result;

                IsLoadingEpisodes = false;

                OnEpisodesLoaded?.Invoke(this, new EpisodesEventArgs
                {
                    Episodes = Episodes,
                    Anime = anime
                });
            }, TaskScheduler.FromCurrentSynchronizationContext());

            //Task.Run(() => GetEpisodesAsync(anime));
            return null;
        }

        public async Task<List<Episode>> GetEpisodesAsync(Anime anime)
        {
            Episodes = await CurrentAnimeScraper.GetEpisodesAsync(anime);
            return Episodes;
        }
        #endregion

        #region Episode/Video Links
        public virtual bool IsLoadingEpisodeLinks { get; protected set; }
        CancellationTokenSource GetEpisodeLinksTokenSource = new CancellationTokenSource();
        public void CancelGetEpisodeLinks()
        {
            if (!GetEpisodeLinksTokenSource.IsCancellationRequested)
                GetEpisodeLinksTokenSource.Cancel();
            IsLoadingEpisodeLinks = false;
        }

        public List<Quality> GetEpisodeLinks(Episode episode,
            bool showAllMirrorLinks = false, bool forceLoad = false)
        {
            IsLoadingEpisodeLinks = true;

            if (GetEpisodeLinksTokenSource.IsCancellationRequested)
                GetEpisodeLinksTokenSource = new CancellationTokenSource();

            var task = Task.Run(() => GetEpisodeLinksAsync(episode, showAllMirrorLinks), GetEpisodeLinksTokenSource.Token);

            if (forceLoad)
            {
                task.Wait();
                Qualities = task.Result;
                return Qualities;
            }

            //setup delegate to invoke when the background task completes
            task.ContinueWith(t =>
            {
                //this will execute when the background task has completed
                if (t.IsFaulted || t.IsCanceled)
                {
                    //somehow handle exception in t.Exception
                    return;
                }

                Qualities = t.Result;
                IsLoadingEpisodeLinks = false;

                OnQualitiesLoaded?.Invoke(this, new QualityEventArgs(Qualities));
            }, TaskScheduler.FromCurrentSynchronizationContext());

            return null;
        }

        public async Task<List<Quality>> GetEpisodeLinksAsync(Episode episode, 
            bool showAllMirrorLinks = false)
        {
            Qualities = await CurrentAnimeScraper.GetEpisodeLinksAsync(episode);
            return Qualities;
        }
        #endregion

        #region Search for Anime Genres
        public virtual bool IsLoadingAllGenres { get; protected set; }
        CancellationTokenSource GetAllGenresTokenSource = new CancellationTokenSource();
        public void CancelGetAllGenres()
        {
            if (!GetAllGenresTokenSource.IsCancellationRequested)
                GetAllGenresTokenSource.Cancel();
            IsLoadingAllGenres = false;
        }

        public List<Genre> GetAllGenres(bool forceLoad = false)
        {
            IsLoadingAllGenres = true;

            if (GetAllGenresTokenSource.IsCancellationRequested)
                GetAllGenresTokenSource = new CancellationTokenSource();

            var task = Task.Run(() => GetAllGenresAsync(), GetAllGenresTokenSource.Token);

            if (forceLoad)
            {
                task.Wait();
                IsLoadingAllGenres = false;
                Genres = task.Result;
                return Genres;
            }

            //setup delegate to invoke when the background task completes
            task.ContinueWith(t =>
            {
                //this will execute when the background task has completed
                if (t.IsFaulted || t.IsCanceled)
                {
                    //somehow handle exception in t.Exception
                    return;
                }

                Genres = t.Result;
                IsLoadingAllGenres = false;

                OnGenresLoaded?.Invoke(this, new GenreEventArgs(Genres));
            }, TaskScheduler.FromCurrentSynchronizationContext());

            return null;
        }

        public async Task<List<Genre>> GetAllGenresAsync()
        {
            Genres = await CurrentAnimeScraper.GetGenresAsync();
            return Genres;
        }
        #endregion   
    }
}