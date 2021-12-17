# AnimeDl
Anime Scraper

## Example 1: Non-Async Method
Here is an example of using animedl

```c#
using System;
using AnimeDl;
using AnimeDl.Scrapers;

namespace AnimeApp
{
    class Class1
    {
        public void Example1() 
        {
            AnimeScraper scraper = new AnimeScraper(AnimeSites.GogoAnime);

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

        public void Example2()
        {
            AnimeScraper scraper = new AnimeScraper();

            var animes = scraper.Search("your lie in april", forceLoad: true);
            Console.WriteLine("Animes count: " + animes.Count);

            var episodes = scraper.GetEpisodes(animes[0], forceLoad: true);
            Console.WriteLine("Episodes count: " + episodes.Count);

            var links = scraper.GetEpisodeLinks(episodes[0], forceLoad: true);
            Console.WriteLine("Episodes count: " + links.Count);
        }

        public async void Example3()
        {
            AnimeScraper scraper = new AnimeScraper();

            var animes = await scraper.SearchAsync("your lie in april");
            Console.WriteLine("Animes count: " + animes.Count);

            var episodes = await scraper.GetEpisodesAsync(animes[0]);
            Console.WriteLine("Episodes count: " + episodes.Count);

            var links = await scraper.GetEpisodeLinksAsync(episodes[0]);
            Console.WriteLine("Episodes count: " + links.Count);
        }

        public void DownloadExample(string url)
        {
            HttpWebRequest downloadRequest = (HttpWebRequest)WebRequest.Create(url);

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
            downloadRequest.Referer = "https://goload.one/";
            //downloadRequest.Headers.Add("Accept-Language", "en-US,en;q=0.9");

            HttpWebResponse downloadResponse = (HttpWebResponse)downloadRequest.GetResponse();
            var stream = downloadResponse.GetResponseStream();
        }
    }
}
```


## Example 2: Async Method
Here is an example of using animedl

```c#
using System;
using AnimeDl;
using AnimeDl.Scrapers;

namespace AnimeApp
{
    class Class1
    {
        public async void Example3()
        {
            AnimeScraper scraper = new AnimeScraper();

            var animes = await scraper.SearchAsync("your lie in april");
            Console.WriteLine("Animes count: " + animes.Count);

            var episodes = await scraper.GetEpisodesAsync(animes[0]);
            Console.WriteLine("Episodes count: " + episodes.Count);

            var links = await scraper.GetEpisodeLinksAsync(episodes[0]);
            Console.WriteLine("Episodes count: " + links.Count);
        }
    }
}
```
