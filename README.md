# AnimeDl

[![Version](https://img.shields.io/nuget/v/AnimeDl.svg)](https://nuget.org/packages/AnimeDl)
[![Downloads](https://img.shields.io/nuget/dt/AnimeDl.svg)](https://nuget.org/packages/AnimeDl)
<a href="https://discord.gg/mhxsSMy2Nf"><img src="https://img.shields.io/badge/Discord-7289DA?style=for-the-badge&logo=discord&logoColor=white"></a>

**AnimeDl** scrapes animes from sites.

<br>
<a href="https://www.buymeacoffee.com/jerry08"><img src="https://img.buymeacoffee.com/button-api/?text=Buy me a coffee&emoji=&slug=jerry08&button_colour=FFDD00&font_colour=000000&font_family=Poppins&outline_colour=000000&coffee_colour=ffffff" /></a>
<br>

### 🌟STAR THIS REPOSITORY TO SUPPORT THE DEVELOPER AND ENCOURAGE THE DEVELOPMENT OF THE PROJECT!

<br>

> Please do not attempt to upload AnimeDl or any of it's forks on Playstore or any other Android appstores on the internet. Doing so, may infringe their terms and conditions. This may result to legal action or immediate take-down of the app.

### Official Discord Server

<p align="center">
 <a href="https://discord.gg/mhxsSMy2Nf">
  <img src="https://invidget.switchblade.xyz/mhxsSMy2Nf">
 </a>
</p>

* **Available Anime sources:-**

| SITE                       | STATUS  | DOWNLOADS |
|:--------------------------:|:-------:|:---------:|
| [Gogo](https://gogoanime.ee)       | WORKING | SOME |
| [Zoro](https://zoro.to)            | SEMI-WORKING | SOME |
| [9Anime](https://9anime.center)    | NOT WORKING | NO |
| [Twist](https://twist.moe)         | NOT WORKING | NO |
| [Tenshi](https://tenshi.moe)       | WORKING | YES |


## Install

- 📦 [NuGet](https://nuget.org/packages/AnimeDl): `dotnet add package AnimeDl`


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
            var client = new AnimeClient(AnimeSites.Tenshi);

            client.OnAnimesLoaded += (s, e) =>
            {
                var animes = e.Animes;

                Console.WriteLine("Animes found: ");
                Console.WriteLine();
                for (int i = 0; i < animes.Count; i++)
                {
                    Console.WriteLine($"[{i + 1}] {animes[i].Title}");
                }

                Console.WriteLine();

                // Read the anime number selected
                Console.Write("Enter anime number: ");

                int animeIndex;

                while (!int.TryParse(Console.ReadLine() ?? "", out animeIndex))
                {
                    Console.Clear();
                    Console.WriteLine("You entered an invalid number");
                    Console.Write("Enter anime number: ");
                }

                animeIndex--;

                Console.WriteLine();

                // Read the anime episodes
                var episodes = client.GetEpisodes(animes[animeIndex]);
            };

            client.OnEpisodesLoaded += (s, e) =>
            {
                var episodes = e.Episodes;

                Console.WriteLine("Episodes found: " + episodes.Count);

                // Read the episode number selected
                Console.Write("Enter episode number: ");

                int episodeIndex;

                while (!int.TryParse(Console.ReadLine() ?? "", out episodeIndex))
                {
                    Console.Clear();
                    Console.WriteLine("You entered an invalid number");
                    Console.Write("Enter episode number: ");
                }

                episodeIndex--;

                Console.WriteLine();

                var videos = client.GetEpisodeLinks(episodes[episodeIndex]);
            };

            client.OnVideosLoaded += (s, e) =>
            {
                var videos = e.Videos;

                Console.WriteLine($"Videos found: " + videos.Count);

                for (int i = 0; i < videos.Count; i++)
                {
                    Console.WriteLine($"[{i + 1}] {videos[i].Resolution}");
                }

                Console.WriteLine();

                // Read the episode number selected
                Console.Write("Enter video number: ");

                int videoIndex;

                while (!int.TryParse(Console.ReadLine() ?? "", out videoIndex))
                {
                    Console.Clear();
                    Console.WriteLine("You entered an invalid number");
                    Console.Write("Enter video number: ");
                }

                videoIndex--;

                // Download the stream
                var fileName = $@"{DateTime.Now.Ticks}.mp4";

                using (var progress = new ConsoleProgress())
                    client.Download(videos[videoIndex], fileName, progress);

                Console.WriteLine("Done");
                Console.WriteLine($"Video saved to '{fileName}'");
            };

            // Read the anime name
            Console.Write("Enter anime name: ");
            var query = Console.ReadLine() ?? "";

            //First (Search anime by name)
            client.Search(query);
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
        //Method with force load
        public static async Task Example2()
        {
            var client = new AnimeClient(AnimeSites.Tenshi);

            // Read the anime name
            Console.Write("Enter anime name: ");
            var query = Console.ReadLine() ?? "";

            var animes = client.Search(query, forceLoad: true);
            Console.WriteLine("Animes found: ");
            Console.WriteLine();
            for (int i = 0; i < animes.Count; i++)
            {
                Console.WriteLine($"[{i+1}] {animes[i].Title}");
            }

            Console.WriteLine();

            // Read the anime number selected
            Console.Write("Enter anime number: ");

            int animeIndex;

            while (!int.TryParse(Console.ReadLine() ?? "", out animeIndex))
            {
                Console.Clear();
                Console.WriteLine("You entered an invalid number");
                Console.Write("Enter anime number: ");
            }

            animeIndex--;

            Console.WriteLine();

            // Read the anime episodes
            var episodes = client.GetEpisodes(animes[animeIndex], forceLoad: true);
            Console.WriteLine("Episodes found: " + episodes.Count);

            // Read the episode number selected
            Console.Write("Enter episode number: ");

            int episodeIndex;

            while (!int.TryParse(Console.ReadLine() ?? "", out episodeIndex))
            {
                Console.Clear();
                Console.WriteLine("You entered an invalid number");
                Console.Write("Enter episode number: ");
            }

            episodeIndex--;

            Console.WriteLine();

            var videos = client.GetEpisodeLinks(episodes[episodeIndex], forceLoad: true);
            Console.WriteLine($"Videos found: " + videos.Count);

            for (int i = 0; i < videos.Count; i++)
            {
                Console.WriteLine($"[{i + 1}] {videos[i].Resolution}");
            }

            Console.WriteLine();

            // Read the episode number selected
            Console.Write("Enter video number: ");

            int videoIndex;

            while (!int.TryParse(Console.ReadLine() ?? "", out videoIndex))
            {
                Console.Clear();
                Console.WriteLine("You entered an invalid number");
                Console.Write("Enter video number: ");
            }

            videoIndex--;

            // Download the stream
            var fileName = $@"{Environment.CurrentDirectory}\{animes[animeIndex].Title} - Ep {episodes[episodeIndex].EpisodeNumber}.mp4";

            using (var progress = new ConsoleProgress())
                await client.DownloadAsync(videos[videoIndex], fileName, progress);

            Console.WriteLine("Done");
            Console.WriteLine($"Video saved to '{fileName}'");

            Console.ReadLine();
        }

        //Async Method
        public static async Task Example3()
        {
            var client = new AnimeClient(AnimeSites.Tenshi);

            // Read the anime name
            Console.Write("Enter anime name: ");
            var query = Console.ReadLine() ?? "";

            var animes = await client.SearchAsync(query);
            Console.WriteLine("Animes found: ");
            Console.WriteLine();
            for (int i = 0; i < animes.Count; i++)
            {
                Console.WriteLine($"[{i + 1}] {animes[i].Title}");
            }

            Console.WriteLine();

            // Read the anime number selected
            Console.Write("Enter anime number: ");

            int animeIndex;

            while (!int.TryParse(Console.ReadLine() ?? "", out animeIndex))
            {
                Console.Clear();
                Console.WriteLine("You entered an invalid number");
                Console.Write("Enter anime number: ");
            }

            animeIndex--;

            Console.WriteLine();

            // Read the anime episodes
            var episodes = await client.GetEpisodesAsync(animes[animeIndex]);
            Console.WriteLine("Episodes found: " + episodes.Count);

            // Read the episode number selected
            Console.Write("Enter episode number: ");

            int episodeIndex;

            while (!int.TryParse(Console.ReadLine() ?? "", out episodeIndex))
            {
                Console.Clear();
                Console.WriteLine("You entered an invalid number");
                Console.Write("Enter episode number: ");
            }

            episodeIndex--;

            Console.WriteLine();

            var videos = await client.GetEpisodeLinksAsync(episodes[episodeIndex]);
            Console.WriteLine($"Videos found: " + videos.Count);

            for (int i = 0; i < videos.Count; i++)
            {
                Console.WriteLine($"[{i + 1}] {videos[i].Resolution}");
            }

            Console.WriteLine();

            // Read the episode number selected
            Console.Write("Enter video number: ");

            int videoIndex;

            while (!int.TryParse(Console.ReadLine() ?? "", out videoIndex))
            {
                Console.Clear();
                Console.WriteLine("You entered an invalid number");
                Console.Write("Enter video number: ");
            }

            videoIndex--;

            // Download the stream
            var fileName = $@"{Environment.CurrentDirectory}\{animes[animeIndex].Title} - Ep {episodes[episodeIndex].EpisodeNumber}.mp4";

            using (var progress = new ConsoleProgress())
                await client.DownloadAsync(videos[videoIndex], fileName, progress);

            Console.WriteLine("Done");
            Console.WriteLine($"Video saved to '{fileName}'");

            Console.ReadLine();
        }

        public static async Task Example4()
        {
            var client = new AnimeClient(AnimeSites.GogoAnime);
            var animes = await client.SearchAsync("", SearchFilter.NewSeason);

            Console.WriteLine("Animes found: ");
            Console.WriteLine();
            for (int i = 0; i < animes.Count; i++)
            {
                Console.WriteLine($"[{i + 1}] {animes[i].Title}");
            }

            Console.ReadLine();
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
        public void DownloadExample(AnimeClient client, Video video, string fileName)
        {
            await client.DownloadAsync(video, fileName);
        }
    }
}
```
