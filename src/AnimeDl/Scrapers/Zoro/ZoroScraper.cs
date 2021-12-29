using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using AnimeDl.Extractors;

namespace AnimeDl.Scrapers
{
    public class ZoroScraper : BaseScraper
    {
        public override string BaseUrl => "https://zoro.to";

        string AjaxListEpisodes = "/ajax/v2/episode/list/";
        string AjaxEpisodeServers = "/ajax/v2/episode/servers";

        public override async Task<List<Anime>> SearchAsync(string searchText,
            SearchType searchType = SearchType.Find, int Page = 1)
        {
            searchText = searchText.Replace(" ", "+");

            List<Anime> animes = new List<Anime>();

            string htmlData = "";

            switch (searchType)
            {
                case SearchType.Find:
                    htmlData = await Utils.GetHtmlAsync($"{BaseUrl}/search?keyword=" + searchText);
                    break;
                case SearchType.Popular:
                    htmlData = await Utils.GetHtmlAsync($"{BaseUrl}/popular.html?page=" + Page);
                    break;
                case SearchType.NewSeason:
                    htmlData = await Utils.GetHtmlAsync($"{BaseUrl}/new-season.html?page=" + Page);
                    break;
                case SearchType.LastUpdated:
                    htmlData = await Utils.GetHtmlAsync($"{BaseUrl}/?page=" + Page);
                    break;
                case SearchType.Trending:
                    break;
                default:
                    break;
            }

            if (string.IsNullOrEmpty(htmlData))
                return animes;

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(htmlData);

            var nodes = document.DocumentNode.Descendants()
                .Where(node => node.HasClass("flw-item")).ToList();

            for (int i = 0; i < nodes.Count; i++)
            {
                string img = "";
                string title = "";
                string category = "";
                string dataId = "";
                string link = "";

                HtmlNode imgNode = nodes[i].SelectSingleNode(".//img");
                if (imgNode != null)
                {
                    img = imgNode.Attributes["data-src"].Value;
                }

                var dataIdNode = nodes[i].SelectSingleNode(".//a[@data-id]");
                if (dataIdNode != null)
                {
                    dataId = dataIdNode.Attributes["data-id"].Value;
                }

                var nameNode = nodes[i].SelectSingleNode(".//div[@class='film-detail']")
                    .SelectSingleNode(".//a");
                if (nameNode != null)
                {
                    category = nameNode.Attributes["href"].Value;
                    title = nameNode.Attributes["title"].Value; //OR name = nameNode.InnerText;
                }

                link = BaseUrl + category;

                animes.Add(new Anime()
                {
                    Id = i + 1,
                    Image = img,
                    Title = title,
                    EpisodesNum = 0,
                    Category = category,
                    Link = link,
                });
            }

            return animes;
        }

        public override async Task<List<Episode>> GetEpisodesAsync(Anime anime)
        {
            string dataId = anime.Category.Split('-').Last().Split('?')[0];
            string url = $"{BaseUrl}{AjaxListEpisodes}{dataId}";

            string json = await Utils.GetHtmlAsync(url);
            var jObj = JObject.Parse(json);
            var html = jObj["html"].ToString();

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            var nodes = doc.DocumentNode.SelectNodes(".//a");

            List<Episode> episodes = new List<Episode>();
            for (int i = 0; i < nodes.Count; i++)
            {
                string title = nodes[i].Attributes["title"].Value;
                int dataNumber = Convert.ToInt32(nodes[i].Attributes["data-number"].Value);
                string dataId2 = nodes[i].Attributes["data-id"].Value;
                string link = nodes[i].Attributes["href"].Value;

                episodes.Add(new Episode()
                {
                    EpisodeName = title,
                    EpisodeLink = link,
                    EpisodeNumber = dataNumber
                });
            }

            return episodes;
        }

        public override async Task<List<Quality>> GetEpisodeLinksAsync(Episode episode, bool showAllMirrorLinks = false)
        {
            List<Quality> list = new List<Quality>();

            string dataId = episode.EpisodeLink.Split(new string[] { "ep=" }, 
                StringSplitOptions.None).Last();
            string url = $"{BaseUrl}{AjaxEpisodeServers}?episodeId={dataId}";

            string json = await Utils.GetHtmlAsync(url);

            var jObj = JObject.Parse(json);
            var html = jObj["html"].ToString();

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            //var nodes = doc.DocumentNode.SelectNodes(".//div[@class='item server-item']");
            var nodes = doc.DocumentNode.Descendants()
                .Where(node => node.HasClass("server-item")).ToList();

            for (int i = 0; i < nodes.Count; i++)
            {
                string dataId2 = nodes[i].Attributes["data-id"].Value;
                string url2 = $"https://zoro.to/ajax/v2/episode/sources?id={dataId2}";
                
                string title = nodes[i].Attributes["data-type"].Value;

                string json2 = await Utils.GetHtmlAsync(url2);

                var jObj2 = JObject.Parse(json2);
                string type = jObj2["type"].ToString();

                if (type != "iframe")
                {

                }
                else
                {
                    string qualityUrl = jObj2["link"].ToString();

                    list.Add(new Quality()
                    {
                        qualityUrl = qualityUrl,
                    });
                }
            }

            for (int i = 0; i < list.Count; i++)
            {
                var quality = list[i];

                var tt = await new Streamsb().ExtractUrl(quality.qualityUrl);
            }

            //string sources = "https://zoro.to/ajax/v2/episode/sources?id=833274";

            return list;
        }
    }
}