using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AnimeDl.Scrapers
{
    public class TenshiScraper : BaseScraper
    {
        public override string BaseUrl => "https://tenshi.moe";

        public WebHeaderCollection CookieHeader = new WebHeaderCollection
        {
            { "Cookie", "__ddg1_=;__ddg2_=;loop-view=thumb" }
        };

        public List<Cookie> Cookies;

        public TenshiScraper()
        {
            Cookies = new List<Cookie>
            {
                new Cookie("__ddg1_", "") { Domain = "tenshi" },
                new Cookie("__ddg2_", "") { Domain = "tenshi" },
                new Cookie("loop-view", "thumb") { Domain = "tenshi" },
            };
        }

        public override async Task<List<Anime>> SearchAsync(string query, SearchType searchType = SearchType.Find, int Page = 1)
        {
            string url = BaseUrl + $"/anime?q={query}&s=vtt-d";

            string html = await Http.GetHtmlAsync(url, CookieHeader, null);

            var animes = new List<Anime>();

            if (string.IsNullOrEmpty(html))
                return animes;

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            var nodes = doc.DocumentNode
                .SelectNodes(".//ul[@class='loop anime-loop thumb']/li")
                .ToList();

            foreach (var node in nodes)
            {
                var anime = new Anime();
                anime.Title = node.SelectSingleNode(".//a").Attributes["title"].Value;
                anime.Link = node.SelectSingleNode(".//a").Attributes["href"].Value;
                anime.Image = node.SelectSingleNode(".//img").Attributes["src"].Value;
                //anime.Summary = node.SelectSingleNode(".//a").Attributes["data-content"].Value;

                animes.Add(anime);
            }

            return animes;
        }

        public override async Task<List<Episode>> GetEpisodesAsync(Anime anime)
        {
            List<Episode> episodes = new List<Episode>();

            string html = await Http.GetHtmlAsync(anime.Link, CookieHeader);

            if (string.IsNullOrEmpty(html))
                return episodes;

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            var nodes = doc.DocumentNode
                .SelectNodes(".//ul[@class='loop episode-loop thumb']/li")
                .ToList();

            foreach (var node in nodes)
            {
                var episode = new Episode();

                episode.EpisodeName = node.SelectSingleNode(".//div[@class='episode-title']").InnerText;
                episode.EpisodeNumber = Convert.ToSingle(node.SelectSingleNode(".//div[@class='episode-slug']").InnerText.Replace("Episode ", ""));
                episode.EpisodeLink = $"{anime.Link}/{episode.EpisodeNumber}";
                episode.Image = node.SelectSingleNode(".//img").Attributes["src"].Value;
                episode.Description = node.SelectSingleNode(".//a").Attributes["data-content"].Value;

                episodes.Add(episode);
            }

            return episodes;
        }

        public override async Task<List<Quality>> GetEpisodeLinksAsync(Episode episode)
        {
            List<Quality> list = new List<Quality>();

            string html = await Http.GetHtmlAsync(episode.EpisodeLink, CookieHeader);

            if (string.IsNullOrEmpty(html))
                return list;

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            var nodes = doc.DocumentNode
                .SelectNodes(".//ul[@class='dropdown-menu']/li/a[@class='dropdown-item']")
                .ToList();

            foreach (var node in nodes)
            {
                string server = node.InnerText.Replace(" ", "").Replace("/-", "");
                bool dub = node.SelectSingleNode(".//span[@title='Audio: English']") != null;

                if (dub)
                {
                    server = $"Dub - ${server}";
                }

                var urlParam = new Uri(node.Attributes["href"].Value).DecodeQueryParameters();
                string url = $"{BaseUrl}/embed?" + urlParam.FirstOrDefault().Key + "=" + urlParam.FirstOrDefault().Value;
                var headers = new WebHeaderCollection()
                {
                    CookieHeader,
                    { "Referer", episode.EpisodeLink }
                };

                html = await Http.GetHtmlAsync(url, headers);

                string unSanitized = html.SubstringAfter("player.source = ").SubstringBefore(";");
                var jObj = JObject.Parse(unSanitized);
                var sources = JArray.Parse(jObj["sources"].ToString());
                
                foreach (var source in sources)
                {
                    list.Add(new Quality()
                    {
                        Headers = CookieHeader,
                        FileType = source["type"].ToString(),
                        Resolution = source["size"].ToString(),
                        QualityUrl = source["src"].ToString(),
                    });
                }
            }

            return list;
        }
    }
}