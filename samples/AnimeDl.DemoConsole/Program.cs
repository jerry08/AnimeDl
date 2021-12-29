using AnimeDl.Scrapers;
using System;
using System.Net;

namespace AnimeDl.DemoConsole
{
    class Program
    {
        static void Main(string[] args)
        {
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
            AnimeScraper scraper = new AnimeScraper(AnimeSites.Zoro);

            var animes = scraper.Search("your lie in april", forceLoad: true);
            Console.WriteLine("Animes count: " + animes.Count);

            var episodes = scraper.GetEpisodes(animes[0], forceLoad: true);
            Console.WriteLine("Episodes count: " + episodes.Count);

            var links = scraper.GetEpisodeLinks(episodes[0], forceLoad: true);
            Console.WriteLine("Episodes count: " + links.Count);
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
            HttpWebRequest downloadRequest = (HttpWebRequest)WebRequest.Create(quality.qualityUrl);

            //downloadRequest.Headers.Add("Upgrade-Insecure-Requests", "1");
            //downloadRequest.UserAgent = "Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/94.0.4606.81 Mobile Safari/537.36 Edg/94.0.992.47";
            //downloadRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9";
            //downloadRequest.Headers.Add("Sec-Fetch-Site", "cross-site");
            //downloadRequest.Headers.Add("Sec-Fetch-Mode", "navigate");
            //downloadRequest.Headers.Add("Sec-Fetch-User", "?1");
            //downloadRequest.Headers.Add("Sec-Fetch-Dest", "document");
            //downloadRequest.Headers.Add("sec-ch-ua", @"""Chromium"";v=""94"", ""Microsoft Edge"";v=""94"", "";Not A Brand"";v=""99""");
            //downloadRequest.Headers.Add("sec-ch-ua-mobile", "?1");
            //downloadRequest.Headers.Add("sec-ch-ua-platform", @"""Android""");
            //downloadRequest.Headers.Add("Accept-Language", "en-US,en;q=0.9");
            
            //downloadRequest.Referer = "https://goload.one/";
            downloadRequest.Referer = quality.Referer;

            HttpWebResponse downloadResponse = (HttpWebResponse)downloadRequest.GetResponse();
            var stream = downloadResponse.GetResponseStream();
        }
    }
}
