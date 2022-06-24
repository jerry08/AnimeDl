using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using HtmlAgilityPack;
using AnimeDl.Extractors;
using AnimeDl.Exceptions;

namespace AnimeDl.Scrapers;

internal class ZoroScraper : BaseScraper
{
    public override string BaseUrl => "https://zoro.to";

    public ZoroScraper(NetHttpClient netHttpClient) : base(netHttpClient)
    {
    }

    public override async Task<List<Anime>> SearchAsync(string searchQuery,
        SearchType searchType,
        int page)
    {
        searchQuery = searchQuery.Replace(" ", "+");

        var animes = new List<Anime>();

        var htmlData = searchType switch
        {
            SearchType.Find => await _netHttpClient.SendHttpRequestAsync($"{BaseUrl}/search?keyword=" + searchQuery),
            SearchType.Popular => await _netHttpClient.SendHttpRequestAsync($"{BaseUrl}/popular.html?page=" + page),
            SearchType.NewSeason => await _netHttpClient.SendHttpRequestAsync($"{BaseUrl}/new-season.html?page=" + page),
            SearchType.LastUpdated => await _netHttpClient.SendHttpRequestAsync($"{BaseUrl}/?page=" + page),
            _ => throw new SearchTypeNotSupportedException("Search type not supported")
        };

        if (htmlData is null)
        {
            return animes;
        }

        var document = new HtmlDocument();
        document.LoadHtml(htmlData);

        var nodes = document.DocumentNode.Descendants()
            .Where(node => node.HasClass("flw-item")).ToList();

        for (int i = 0; i < nodes.Count; i++)
        {
            var img = "";
            var title = "";
            var category = "";
            var dataId = "";
            var link = "";

            var imgNode = nodes[i].SelectSingleNode(".//img");
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
        var dataId = anime.Category.Split('-').Last().Split('?')[0];
        var url = $"{BaseUrl}/ajax/v2/episode/list/{dataId}";

        var json = await _netHttpClient.SendHttpRequestAsync(url);
        var jObj = JObject.Parse(json);
        var html = jObj["html"]!.ToString();

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var nodes = doc.DocumentNode.SelectNodes(".//a")
            .Where(x => x.Attributes["data-page"] == null).ToList();

        var episodes = new List<Episode>();
        for (int i = 0; i < nodes.Count; i++)
        {
            var title = nodes[i].Attributes["title"].Value;
            var dataNumber = Convert.ToInt32(nodes[i].Attributes["data-number"].Value);
            var dataId2 = nodes[i].Attributes["data-id"].Value;
            var link = nodes[i].Attributes["href"].Value;

            episodes.Add(new Episode()
            {
                EpisodeName = title,
                EpisodeLink = link,
                EpisodeNumber = dataNumber
            });
        }

        return episodes;
    }

    /*private async Task<string> GetM3u8FromRapidCloud(string url)
    {
        var headers = new WebHeaderCollection()
        {
            { "Referer", "https://zoro.to/" },
            { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:94.0) Gecko/20100101 Firefox/94.0" }
        };

        url = $"{url}&autoPlay=1&oa=0";
        string text = await _netHttpClient.SendHttpRequestAsync(url, headers);

        return text;
    }*/

    public override async Task<List<Quality>> GetEpisodeLinksAsync(Episode episode)
    {
        var list = new List<Quality>();

        var dataId = episode.EpisodeLink.Split(new string[] { "ep=" }, 
            StringSplitOptions.None).Last();
        var url = $"{BaseUrl}/ajax/v2/episode/servers?episodeId={dataId}";

        var json = await _netHttpClient.SendHttpRequestAsync(url);

        var jObj = JObject.Parse(json);
        var html = jObj["html"]!.ToString();

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        //var nodes = doc.DocumentNode.SelectNodes(".//div[@class='item server-item']");
        var nodes = doc.DocumentNode.Descendants()
            .Where(node => node.HasClass("server-item")).ToList();

        for (int i = 0; i < nodes.Count; i++)
        {
            var dataId2 = nodes[i].Attributes["data-id"].Value;
            var title = nodes[i].Attributes["data-type"].Value;

            var url2 = $"https://zoro.to/ajax/v2/episode/sources?id={dataId2}";
            var json2 = await _netHttpClient.SendHttpRequestAsync(url2);

            var jObj2 = JObject.Parse(json2);
            var type = jObj2["type"]!.ToString();
            var server = jObj2["server"]!.ToString();

            if (type != "iframe")
            {

            }
            else
            {
                var qualityUrl = jObj2["link"]!.ToString();

                switch (server)
                {
                    case "4":
                    case "1":
                        //rapidvideo
                        list.AddRange(await new RapidCloud(_netHttpClient).ExtractQualities(qualityUrl));
                        break;
                    case "5":
                        //StreamSB
                        list.AddRange(await new StreamSB2(_netHttpClient).ExtractQualities(qualityUrl));
                        break;
                    case "3":
                        //streamtape
                        list.AddRange(await new StreamTape(_netHttpClient).ExtractQualities(qualityUrl));
                        break;
                    default:
                        break;
                }
            }

            if (list.Count > 0)
            {
                break;
            }
        }

        return list;
    }
}