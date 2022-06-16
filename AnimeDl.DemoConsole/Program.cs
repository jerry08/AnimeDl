using System;
using System.IO;
using System.Linq;
using System.Net;
using AnimeDl.Scrapers;

namespace AnimeDl.DemoConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "AnimeDl Demo";

            Example2();
        }

        public static void Example1()
        {
            var scraper = new AnimeScraper(AnimeSites.Zoro);

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
                scraper.GetEpisodeLinks(episodes[0]);
            };

            scraper.OnQualitiesLoaded += (s, e) =>
            {
                var qualities = e.Qualities;

                Console.WriteLine("Qualities count: " + qualities.Count);
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
            //var scraper = new AnimeScraper(AnimeSites.GogoAnime); //Working
            //var scraper = new AnimeScraper(AnimeSites.TwistMoe); //Not working
            //var scraper = new AnimeScraper(AnimeSites.Zoro); //Working
            //var scraper = new AnimeScraper(AnimeSites.NineAnime); //Not working
            var scraper = new AnimeScraper(AnimeSites.Tenshi);

            // Read the anime name
            Console.Write("Enter anime name: ");
            var query = Console.ReadLine() ?? "";

            //string query = "your lie in april";
            //string query = "attack on titan final season part 2";
            //string query = "Ascendance of a Bookworm 3rd season";
            //string query = "naruto";

            var animes = scraper.Search(query, forceLoad: true);
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
            var episodes = scraper.GetEpisodes(animes[animeIndex], forceLoad: true);
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

            var qualities = scraper.GetEpisodeLinks(episodes[episodeIndex], forceLoad: true);
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

            DownloadExample(qualities[qualityIndex], $@"{Environment.CurrentDirectory}\{animes[animeIndex].Title} - Ep {episodes[episodeIndex].EpisodeNumber}.mp4");

            Console.ReadLine();
        }

        public static async void Example3()
        {
            var scraper = new AnimeScraper(AnimeSites.Zoro);

            var animes = await scraper.SearchAsync("your lie in april");
            Console.WriteLine("Animes count: " + animes.Count);

            var episodes = await scraper.GetEpisodesAsync(animes[0]);
            Console.WriteLine("Episodes count: " + episodes.Count);

            var links = await scraper.GetEpisodeLinksAsync(episodes[0]);
            Console.WriteLine("Qualities count: " + links.Count);
        }

        public static void DownloadExample(string url, string filePath)
        {
            DownloadExample(new Quality() 
            { 
                QualityUrl = url
            }, filePath);
        }

        public static void DownloadExample(Quality quality, string filePath)
        {
            HttpWebRequest downloadRequest = (HttpWebRequest)WebRequest.Create(quality.QualityUrl);

            downloadRequest.Headers = quality.Headers;

            HttpWebResponse downloadResponse = (HttpWebResponse)downloadRequest.GetResponse();
            Stream stream = downloadResponse.GetResponseStream();

            //Create a stream for the file
            Stream file = File.Create(filePath);

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
                } while (length > 0); //Repeat until no data is read
            }
            finally
            {
                file?.Close();
                stream?.Close();
            }
        }
    }
}