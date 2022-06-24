using System;
using System.Threading.Tasks;
using AnimeDl.DemoConsole.Utils;
using AnimeDl.Scrapers;

namespace AnimeDl.DemoConsole;

public static class Program
{
    public static async Task Main()
    {
        Console.Title = "AnimeDl Demo";

        //Example1();
        //await Example2();
        await Example3();
    }

    //Method with events
    public static void Example1()
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

            var qualities = client.GetEpisodeLinks(episodes[episodeIndex]);
        };

        client.OnQualitiesLoaded += (s, e) =>
        {
            var qualities = e.Qualities;

            Console.WriteLine($"Qualities found: " + qualities.Count);

            for (int i = 0; i < qualities.Count; i++)
            {
                Console.WriteLine($"[{i + 1}] {qualities[i].Resolution}");
            }

            Console.WriteLine();

            // Read the episode number selected
            Console.Write("Enter quality number: ");

            int qualityIndex;

            while (!int.TryParse(Console.ReadLine() ?? "", out qualityIndex))
            {
                Console.Clear();
                Console.WriteLine("You entered an invalid number");
                Console.Write("Enter quality number: ");
            }

            qualityIndex--;

            // Download the stream
            var fileName = $@"{DateTime.Now.Ticks}.mp4";

            using (var progress = new ConsoleProgress())
                client.Download(qualities[qualityIndex], fileName, progress);

            Console.WriteLine("Done");
            Console.WriteLine($"Video saved to '{fileName}'");
        };

        // Read the anime name
        Console.Write("Enter anime name: ");
        var query = Console.ReadLine() ?? "";

        //First (Search anime by name)
        client.Search(query);

        System.Threading.Thread.Sleep(-1);
    }

    //Method with force load
    public static async Task Example2()
    {
        //var client = new AnimeClient(AnimeSites.GogoAnime); //Working
        //var client = new AnimeClient(AnimeSites.TwistMoe); //Not working
        //var client = new AnimeClient(AnimeSites.Zoro); //Working
        //var client = new AnimeClient(AnimeSites.NineAnime); //Not working
        var client = new AnimeClient(AnimeSites.Tenshi); //Working

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

        var qualities = client.GetEpisodeLinks(episodes[episodeIndex], forceLoad: true);
        Console.WriteLine($"Qualities found: " + qualities.Count);

        for (int i = 0; i < qualities.Count; i++)
        {
            Console.WriteLine($"[{i + 1}] {qualities[i].Resolution}");
        }

        Console.WriteLine();

        // Read the episode number selected
        Console.Write("Enter quality number: ");

        int qualityIndex;

        while (!int.TryParse(Console.ReadLine() ?? "", out qualityIndex))
        {
            Console.Clear();
            Console.WriteLine("You entered an invalid number");
            Console.Write("Enter quality number: ");
        }

        qualityIndex--;

        // Download the stream
        var fileName = $@"{Environment.CurrentDirectory}\{animes[animeIndex].Title} - Ep {episodes[episodeIndex].EpisodeNumber}.mp4";

        using (var progress = new ConsoleProgress())
            await client.DownloadAsync(qualities[qualityIndex], fileName, progress);

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

        var qualities = await client.GetEpisodeLinksAsync(episodes[episodeIndex]);
        Console.WriteLine($"Qualities found: " + qualities.Count);

        for (int i = 0; i < qualities.Count; i++)
        {
            Console.WriteLine($"[{i + 1}] {qualities[i].Resolution}");
        }

        Console.WriteLine();

        // Read the episode number selected
        Console.Write("Enter quality number: ");

        int qualityIndex;

        while (!int.TryParse(Console.ReadLine() ?? "", out qualityIndex))
        {
            Console.Clear();
            Console.WriteLine("You entered an invalid number");
            Console.Write("Enter quality number: ");
        }

        qualityIndex--;

        // Download the stream
        var fileName = $@"{Environment.CurrentDirectory}\{animes[animeIndex].Title} - Ep {episodes[episodeIndex].EpisodeNumber}.mp4";

        using (var progress = new ConsoleProgress())
            await client.DownloadAsync(qualities[qualityIndex], fileName, progress);

        Console.WriteLine("Done");
        Console.WriteLine($"Video saved to '{fileName}'");

        Console.ReadLine();
    }
}