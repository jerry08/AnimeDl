# AnimeDl
AnimeDl scrapes animes from sites. 

## Sites available currently
* GogoAnime
* Twistmoe
* Zoro
* More will be added soon (for backups)


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

## Example Downloading
Here is an example of using animedl

```c#
using System;
using AnimeDl;
using AnimeDl.Scrapers;

namespace AnimeApp
{
    class Class1
    {
        public void DownloadExample(Quality quality)
        {
            HttpWebRequest downloadRequest = (HttpWebRequest)WebRequest.Create(quality.qualityUrl);

            downloadRequest.Referer = quality.Referer;

            HttpWebResponse downloadResponse = (HttpWebResponse)downloadRequest.GetResponse();
            var stream = downloadResponse.GetResponseStream();
        }
    }
}
```
