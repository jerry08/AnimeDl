using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AnimeDl.DemoConsole.Utils;
using AnimeDl.Scrapers;
using AnimeDl.Anilist;

namespace AnimeDl.DemoConsole;

public static class Program
{
    private readonly static AnimeClient _client = new(AnimeSites.GogoAnime);
    private readonly static AnilistClient _client2 = new();

    public static async Task Main()
    {
        Console.Title = "AnimeDl Demo";

        await Example3();
        //await AnilistExample1();
    }

    public static async Task AnilistExample1()
    {
        // Read the anime name
        Console.Write("Enter anime name: ");
        var query = Console.ReadLine() ?? "";

        //var ss1 = await _client2.GetRecentlyUpdatedAsync();
        var searchResults = await _client2.SearchAsync("ANIME", search: query);

        //Filter animes to "ANIME" format
        //var animes = searchResults?.Results.Where(x => x.Format == "ANIME").ToList();
        
        //Filter to animes available by mal
        var animes = searchResults?.Results.Where(x => x.IdMal is not null).ToList();

        Console.WriteLine("Animes found: ");
        Console.WriteLine();
        //for (int i = 0; i < animes?.Results.Count; i++)
        //{
        //    Console.Write($"[{i + 1}] English Title: {animes.Results[i].Title?.English}");
        //    Console.Write($"  / (Romaji Title: {animes.Results[i].Title?.Romaji})");
        //    Console.WriteLine($"  / (Native Title: {animes.Results[i].Title?.Native})");
        //}

        for (int i = 0; i < animes?.Count; i++)
        {
            Console.Write($"[{i + 1}] Name: {animes[i].Name}");
            Console.WriteLine($"  / (Name Romaji: {animes[i].NameRomaji})");
            Console.WriteLine();
        }

        //var tt = await _client.FindBestMatch("86");

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

        var test = await _client2.GetMediaDetailsAsync(animes![animeIndex]);
        
        var test2 = await _client.AutoSearch(animes[animeIndex]);

        Console.WriteLine($"Specific anime found: {test2?.Title}");
        Console.WriteLine();
    }

    //Method with events
    public static void Example1()
    {
        _client.OnAnimesLoaded += (s, e) =>
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
            var episodes = _client.GetEpisodes(animes[animeIndex]);
        };

        _client.OnEpisodesLoaded += (s, e) =>
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

            var videoServers = _client.GetVideoServers(episodes[episodeIndex]);
        };

        _client.OnVideoServersLoaded += (s, e) =>
        {
            var videoServers = e.VideoServers;

            for (int i = 0; i < videoServers.Count; i++)
            {
                Console.WriteLine($"[{i + 1}] {videoServers[i].Name}");
            }

            Console.WriteLine();

            // Read the server index selected
            Console.Write("Enter server index: ");

            int videoServerIndex;

            while (!int.TryParse(Console.ReadLine() ?? "", out videoServerIndex))
            {
                Console.Clear();
                Console.WriteLine("You entered an invalid server index");
                Console.Write("Enter server index: ");
            }

            videoServerIndex--;

            var videos = _client.GetVideos(videoServers[videoServerIndex]);
        };

        _client.OnVideosLoaded += (s, e) =>
        {
            var videos = e.Videos;

            Console.WriteLine($"Videos found: " + videos.Count);

            for (int i = 0; i < videos.Count; i++)
            {
                Console.WriteLine($"[{i + 1}] {videos[i].Resolution} - {videos[i].Format}");
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
                _client.Download(videos[videoIndex], fileName, progress);

            Console.WriteLine("Done");
            Console.WriteLine($"Video saved to '{fileName}'");
        };

        // Read the anime name
        Console.Write("Enter anime name: ");
        var query = Console.ReadLine() ?? "";

        //First (Search anime by name)
        _client.Search(query);

        System.Threading.Thread.Sleep(-1);
    }

    //Method with force load
    public static async Task Example2()
    {
        // Read the anime name
        Console.Write("Enter anime name: ");
        var query = Console.ReadLine() ?? "";

        var animes = _client.Search(query, forceLoad: true);
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
        var episodes = _client.GetEpisodes(animes[animeIndex], forceLoad: true);
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

        var videoServers = _client.GetVideoServers(episodes[episodeIndex], forceLoad: true);

        for (int i = 0; i < videoServers.Count; i++)
        {
            Console.WriteLine($"[{i + 1}] {videoServers[i].Name}");
        }

        Console.WriteLine();

        // Read the server index selected
        Console.Write("Enter server index: ");

        int videoServerIndex;

        while (!int.TryParse(Console.ReadLine() ?? "", out videoServerIndex))
        {
            Console.Clear();
            Console.WriteLine("You entered an invalid server index");
            Console.Write("Enter server index: ");
        }

        videoServerIndex--;

        var videos = _client.GetVideos(videoServers[videoServerIndex], forceLoad: true);
        Console.WriteLine($"Videos found: " + videos.Count);

        for (int i = 0; i < videos.Count; i++)
        {
            Console.WriteLine($"[{i + 1}] {videos[i].Resolution} - {videos[i].Format}");
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
        var fileName = $@"{Environment.CurrentDirectory}\{animes[animeIndex].Title} - Ep {episodes[episodeIndex].Number}.mp4";

        using (var progress = new ConsoleProgress())
            await _client.DownloadAsync(videos[videoIndex], fileName, progress);

        Console.WriteLine("Done");
        Console.WriteLine($"Video saved to '{fileName}'");

        Console.ReadLine();
    }

    //Async Method
    public static async Task Example3()
    {
        // Read the anime name
        Console.Write("Enter anime name: ");
        var query = Console.ReadLine() ?? "";

        var animes = await _client.SearchAsync(query, selectDub: false);
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
        var episodes = await _client.GetEpisodesAsync(animes[animeIndex]);
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

        var videoServers = await _client.GetVideoServersAsync(episodes[episodeIndex]);

        for (int i = 0; i < videoServers.Count; i++)
        {
            Console.WriteLine($"[{i + 1}] {videoServers[i].Name}");
        }

        Console.WriteLine();

        // Read the server index selected
        Console.Write("Enter server index: ");

        int videoServerIndex;

        while (!int.TryParse(Console.ReadLine() ?? "", out videoServerIndex))
        {
            Console.Clear();
            Console.WriteLine("You entered an invalid server index");
            Console.Write("Enter server index: ");
        }

        videoServerIndex--;

        var videos = await _client.GetVideosAsync(videoServers[videoServerIndex]);
        Console.WriteLine($"Videos found: " + videos.Count);

        for (int i = 0; i < videos.Count; i++)
        {
            Console.WriteLine($"[{i + 1}] {videos[i].Resolution} - {videos[i].Format} (Size: {videos[i].Size})");
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
        var fileName = $@"{Environment.CurrentDirectory}\{animes[animeIndex].Title.ReplaceInvalidChars()} - Ep {episodes[episodeIndex].Number}.mp4";

        using (var progress = new ConsoleProgress())
            await _client.DownloadAsync(videos[videoIndex], fileName, progress);

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

    public static string ReplaceInvalidChars(this string fileName)
    {
        return string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
    }
}