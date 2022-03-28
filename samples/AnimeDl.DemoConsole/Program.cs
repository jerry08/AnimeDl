using AnimeDl.Scrapers;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;

namespace AnimeDl.DemoConsole
{
    static class DisableConsoleQuickEdit
    {

        const uint ENABLE_QUICK_EDIT = 0x0040;

        // STD_INPUT_HANDLE (DWORD): -10 is the standard input device.
        const int STD_INPUT_HANDLE = -10;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        internal static bool Go()
        {
            IntPtr consoleHandle = GetStdHandle(STD_INPUT_HANDLE);

            // get current console mode
            uint consoleMode;
            if (!GetConsoleMode(consoleHandle, out consoleMode))
            {
                // ERROR: Unable to get console mode.
                return false;
            }

            // Clear the quick edit bit in the mode flags
            consoleMode &= ~ENABLE_QUICK_EDIT;

            // set the new mode
            if (!SetConsoleMode(consoleHandle, consoleMode))
            {
                // ERROR: Unable to set console mode
                return false;
            }

            return true;
        }
    }

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
            //DisableQuickEditMode();
            DisableConsoleQuickEdit.Go();
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
            //string ss = "#EXT-X-STREAM-INF:BANDWIDTH=2959360,RESOLUTION=1920x1080";
            //var sg = ss.SubstringAfter("RESOLUTION=");

            AnimeScraper scraper = new AnimeScraper(AnimeSites.GogoAnime);

            //var animes = scraper.Search("your lie in april", forceLoad: true);
            var animes = scraper.Search("attack on titan final season part 2", forceLoad: true);
            Console.WriteLine("Animes count: " + animes.Count);

            var episodes = scraper.GetEpisodes(animes[0], forceLoad: true);
            Console.WriteLine("Episodes count: " + episodes.Count);

            var qualities = scraper.GetEpisodeLinks(episodes[0], forceLoad: true);
            Console.WriteLine($"Qualities count: " + qualities.Count);

            //qualities[1].Referer = qualities[0].Referer;
            DownloadExample(qualities[0], @"D:\test123.mp4");

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

        public static void DownloadExample(Quality quality, string filePath)
        {
            HttpWebRequest downloadRequest = (HttpWebRequest)WebRequest.Create(quality.QualityUrl);

            //downloadRequest.Referer = quality.Referer;
            downloadRequest.Headers = new WebHeaderCollection()
            {
                { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; rv:91.0) Gecko/20100101 Firefox/91.0" },
                //{ "Accept", "application/json, text/plain, */*" },
                { "Accept", "*/*" },
                { "Accept-Encoding", "gzip, deflate, br" },
                { "Accept-Language", "en-US,en;q=0.5" },
                { "Referer", quality.Referer + "/" },
                { "Origin", quality.Referer },
                //{ "watchsb", "streamsb" },
                //{ "DNT", "1" },
                //{ "Connection", "keep-alive" },
                //{ "Sec-Fetch-Dest", "empty" },
                //{ "Sec-Fetch-Mode", "no-cors" },
                //{ "Sec-Fetch-Site", "same-origin" },
                //{ "TE", "trailers" },
                //{ "Pragma", "no-cache" },
                //{ "Cache-Control", "no-cache" }
            };

            downloadRequest.Headers = new WebHeaderCollection()
            {
                { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; rv:91.0) Gecko/20100101 Firefox/91.0" },
                { "Accept", "application/json, text/plain, */*" },
                { "Accept-Language", "en-US,en;q=0.5" },
                //{ "Referer", url },
                { "watchsb", "streamsb" },
                { "DNT", "1" },
                { "Connection", "keep-alive" },
                { "Sec-Fetch-Dest", "empty" },
                { "Sec-Fetch-Mode", "no-cors" },
                { "Sec-Fetch-Site", "same-origin" },
                { "TE", "trailers" },
                { "Pragma", "no-cache" },
                { "Cache-Control", "no-cache" }
            };

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

    static class ss
    {
        public static string SubstringAfter(this string value, string a)
        {
            int start = value.IndexOf(a);
            if (start != -1)
            {
                start += a.Length;
                return value.Substring(start);
            }

            return null;
        }

        public static string SubstringBefore(this string text, string stopAt)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                int charLocation = text.IndexOf(stopAt, StringComparison.Ordinal);

                if (charLocation > 0)
                {
                    return text.Substring(0, charLocation);
                }
            }

            return string.Empty;
        }
    }
}