using AnimeDl.Scrapers;
using System;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;

namespace AnimeDl.DemoConsole
{
    class Program
    {
        // http://msdn.microsoft.com/en-us/library/ms686033(VS.85).aspx
        [DllImport("kernel32.dll")]
        public static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        private const uint ENABLE_EXTENDED_FLAGS = 0x0080;

        private static void DisableQuickEditMode()
        {
            // Disable QuickEdit Mode
            // Quick Edit mode freezes the app to let users select text.
            // We don't want that. We want the app to run smoothly in the background.
            // - https://stackoverflow.com/q/4453692
            // - https://stackoverflow.com/a/4453779
            // - https://stackoverflow.com/a/30517482

            IntPtr handle = Process.GetCurrentProcess().MainWindowHandle;
            SetConsoleMode(handle, ENABLE_EXTENDED_FLAGS);
        }

        static void Main(string[] args)
        {
            DisableQuickEditMode();
            Example2();
        }

        public static void Example1()
        {
            AnimeScraper scraper = new AnimeScraper(AnimeSites.Zoro);

            scraper.OnAnimesLoaded += (s, e) =>
            {
                var animes = e.Animes;

                Console.WriteLine("Animes count: " + animes.Count);

                //Second (get episodes from specific anime)
                scraper.GetEpisodes(animes[0]);
            };

            scraper.OnEpisodesLoaded += (s, e) =>
            {
                var episodes = e.Episodes;

                Console.WriteLine("Episodes count: " + episodes.Count);

                //Thrid (get video links from specific episode).
                //Can download video from links
                scraper.GetEpisodeLinks(episodes[0]);
            };

            //Optional (Only gets all genres from )
            //scraper.OnGenresLoaded += (s, e) =>
            //{
            //    var genres = e.Genres;
            //
            //    Console.WriteLine("Genres count: " + genres.Count);
            //};
            //scraper.GetAllGenres();

            //First (Search anime by name)
            scraper.Search("your lie in april");
        }

        public static void Example2()
        {
            AnimeScraper scraper = new AnimeScraper(AnimeSites.GogoAnime);

            var animes = scraper.Search("your lie in april", forceLoad: true);
            Console.WriteLine("Animes count: " + animes.Count);

            var episodes = scraper.GetEpisodes(animes[0], forceLoad: true);
            Console.WriteLine("Episodes count: " + episodes.Count);

            var links = scraper.GetEpisodeLinks(episodes[0], forceLoad: true);
            Console.WriteLine($"Episodes count: " + links.Count);

            Console.ReadLine();
        }

        public static async void Example3()
        {
            AnimeScraper scraper = new AnimeScraper(AnimeSites.Zoro);

            var animes = await scraper.SearchAsync("your lie in april");
            Console.WriteLine("Animes count: " + animes.Count);

            var episodes = await scraper.GetEpisodesAsync(animes[0]);
            Console.WriteLine("Episodes count: " + episodes.Count);

            var links = await scraper.GetEpisodeLinksAsync(episodes[0]);
            Console.WriteLine("Episodes count: " + links.Count);
        }

        public static void DownloadExample(Quality quality)
        {
            HttpWebRequest downloadRequest = (HttpWebRequest)WebRequest.Create(quality.QualityUrl);
            
            //downloadRequest.Referer = "https://goload.one/";
            downloadRequest.Referer = quality.Referer;

            HttpWebResponse downloadResponse = (HttpWebResponse)downloadRequest.GetResponse();
            var stream = downloadResponse.GetResponseStream();
        }
    }
}
