using System;
using System.IO;
using System.Net;
using AnimeDl.Scrapers;

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
            AnimeScraper scraper = new AnimeScraper(AnimeSites.GogoAnime);

            string query = "your lie in april";
            //string query = "attack on titan final season part 2";
            //string query = "Ascendance of a Bookworm 3rd season";
            //string query = "naruto";

            var animes = scraper.Search(query, forceLoad: true);
            //var animes = scraper.Search(query, forceLoad: true);
            Console.WriteLine("Animes count: " + animes.Count);

            var episodes = scraper.GetEpisodes(animes[0], forceLoad: true);
            Console.WriteLine("Episodes count: " + episodes.Count);

            var qualities = scraper.GetEpisodeLinks(episodes[0], forceLoad: true);
            Console.WriteLine($"Qualities count: " + qualities.Count);

            //qualities[1].Referer = qualities[0].Referer;
            //DownloadExample(qualities[3], @"D:\video1.mp4");

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