using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using AnimeDl.Extractors;
using AnimeDl.Exceptions;
using AnimeDl.Utils.Extensions;
using AnimeDl.Models;
using Nager.PublicSuffix;

namespace AnimeDl.Scrapers;

internal class ZoroScraper : BaseScraper
{
    public override string Name { get; set; } = "Zoro";

    public override bool IsDubAvailableSeparately { get; set; } = true;

    public override string BaseUrl => "https://zoro.to";

    public ZoroScraper(HttpClient http) : base(http)
    {
    }

    public override async Task<List<Anime>> SearchAsync(string query,
        SearchFilter searchFilter,
        int page,
        bool selectDub)
    {
        query = query.Replace(" ", "+");

        var animes = new List<Anime>();

        var htmlData = searchFilter switch
        {
            SearchFilter.Find => await _http.SendHttpRequestAsync($"{BaseUrl}/search?keyword=" + query),
            SearchFilter.Popular => await _http.SendHttpRequestAsync($"{BaseUrl}/most-popular?page=" + page),
            SearchFilter.NewSeason => await _http.SendHttpRequestAsync($"{BaseUrl}/recently-added?page=" + page),
            SearchFilter.LastUpdated => await _http.SendHttpRequestAsync($"{BaseUrl}/?page=" + page),
            _ => throw new SearchFilterNotSupportedException("Search filter not supported")
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
                Site = AnimeSites.Zoro,
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

        var document = new HtmlDocument();

        //Get anime details
        var html = await _http.SendHttpRequestAsync(anime.Link);
        //https://stackoverflow.com/questions/122641/how-can-i-decode-html-characters-in-c
        //HttpUtility.HtmlDecode();
        document.LoadHtml(HtmlEntity.DeEntitize(html));

        var itemHeadNodes = document.DocumentNode.SelectNodes(".//div[@class='anisc-info-wrap']/div[@class='anisc-info']//span[@class='item-head']");
        //var overviewNode = document.DocumentNode.SelectNodes(".//div[@class='anisc-info-wrap']/div[@class='anisc-info']")[0];
        //anime.Summary = overviewNode.InnerText;

        var overviewNode = itemHeadNodes.Where(x => !string.IsNullOrEmpty(x.InnerHtml)
            && x.InnerHtml.ToLower().Contains("overview")).FirstOrDefault()?
            .ParentNode.SelectSingleNode(".//span[@class='name']")
            ?? itemHeadNodes.Where(x => !string.IsNullOrEmpty(x.InnerHtml)
            && x.InnerHtml.ToLower().Contains("overview")).FirstOrDefault()?
            .ParentNode.SelectSingleNode(".//div[@class='text']");
        if (overviewNode is not null)
            anime.Summary = overviewNode.InnerText.Trim();

        var typeNode = document.DocumentNode.SelectNodes(".//div[@class='film-stats']/span[@class='dot']")
            .FirstOrDefault()!.NextSibling.NextSibling;
        if (typeNode is not null)
            anime.Type = typeNode.InnerText;

        var statusNode = itemHeadNodes.Where(x => !string.IsNullOrEmpty(x.InnerHtml)
            && x.InnerHtml.ToLower().Contains("status")).FirstOrDefault()?
            .ParentNode.SelectSingleNode(".//span[@class='name']");
        if (statusNode is not null)
            anime.Status = statusNode.InnerText;

        var genresNode = itemHeadNodes.Where(x => !string.IsNullOrEmpty(x.InnerHtml)
            && x.InnerHtml.ToLower().Contains("genres")).FirstOrDefault()?
            .ParentNode.SelectNodes(".//a").ToList();
        if (genresNode is not null)
            anime.Genres.AddRange(genresNode.Select(x => new Genre(x.Attributes["title"].Value)));

        var airedNode = itemHeadNodes.Where(x => !string.IsNullOrEmpty(x.InnerHtml)
            && x.InnerHtml.ToLower().Contains("aired")).FirstOrDefault()?
            .ParentNode.SelectSingleNode(".//span[@class='name']");
        if (airedNode is not null)
            anime.Released = airedNode.InnerText;

        var synonymsNode = itemHeadNodes.Where(x => !string.IsNullOrEmpty(x.InnerHtml)
            && x.InnerHtml.ToLower().Contains("synonyms")).FirstOrDefault()?
            .ParentNode.SelectSingleNode(".//span[@class='name']");
        if (synonymsNode is not null)
            //anime.OtherNames = HtmlEntity.DeEntitize(synonymsNode.InnerText);
            anime.OtherNames = synonymsNode.InnerText;

        //Get anime episodes
        var json = await _http.SendHttpRequestAsync(url);
        var jObj = JObject.Parse(json);
        html = jObj["html"]!.ToString();

        document = new HtmlDocument();
        document.LoadHtml(html);

        var nodes = document.DocumentNode.SelectNodes(".//a")
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
                EpisodeName = $"{i + 1} - {title}",
                EpisodeLink = link,
                EpisodeNumber = dataNumber
            });
        }

        return episodes;
    }

    public override async Task<List<VideoServer>> GetVideoServersAsync(Episode episode)
    {
        var dataId = episode.EpisodeLink.Split(new string[] { "ep=" },
            StringSplitOptions.None).Last();
        var url = $"{BaseUrl}/ajax/v2/episode/servers?episodeId={dataId}";

        var json = await _http.SendHttpRequestAsync(url);

        var jObj = JObject.Parse(json);
        var html = jObj["html"]!.ToString();

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var nodes = doc.DocumentNode.Descendants()
            .Where(node => node.HasClass("server-item")).ToList();

        var videoServers = new List<VideoServer>();

        for (int i = 0; i < nodes.Count; i++)
        {
            var dataId2 = nodes[i].Attributes["data-id"].Value;
            var serverName = nodes[i].Attributes["data-type"].Value.ToUpper().Trim() + $" {nodes[i].InnerText.Trim()}";

            var url2 = $"https://zoro.to/ajax/v2/episode/sources?id={dataId2}";
            var json2 = await _http.SendHttpRequestAsync(url2);

            var jObj2 = JObject.Parse(json2);
            var type = jObj2["type"]!.ToString();
            var server = jObj2["server"]!.ToString();

            if (type != "iframe")
            {

            }
            else
            {
                var videoUrl = jObj2["link"]!.ToString();

                videoUrl += dataId;
            }

            var link = jObj2["link"]!.ToString();
            var embedHeaders = new WebHeaderCollection()
            {
                { "Referer", BaseUrl + "/" }
            };

            videoServers.Add(new VideoServer(serverName, new FileUrl(link, embedHeaders)));
        }

        return videoServers;
    }

    public override async Task<List<Video>> GetVideosAsync(VideoServer server)
    {
        var videos = new List<Video>();

        var domainParser = new DomainParser(new WebTldRuleProvider());
        var domainInfo = domainParser.Parse(server.Embed.Url);

        if (domainInfo.Domain.Contains("rapid"))
        {
            videos.AddRange(await new RapidCloud(_http, server).Extract());
        }
        else if (domainInfo.Domain.Contains("sb"))
        {
            videos.AddRange(await new StreamSB(_http, server).Extract());
        }
        else if (domainInfo.Domain.Contains("streamta"))
        {
            videos.AddRange(await new StreamTape(_http, server).Extract());
        }

        return videos;
    }
}