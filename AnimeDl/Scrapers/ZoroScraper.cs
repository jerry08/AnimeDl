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
using AnimeDl.Extractors.Interfaces;
using System.Xml.Linq;

namespace AnimeDl.Scrapers;

/// <summary>
/// Scraper for interacting with zoro.
/// </summary>
public class ZoroScraper : BaseScraper
{
    public override string Name { get; set; } = "Zoro";

    public override bool IsDubAvailableSeparately { get; set; } = true;

    public override string BaseUrl => "https://zoro.to";

    public ZoroScraper(HttpClient http) : base(http)
    {
    }

    public override async Task<List<Anime>> SearchAsync(
        string query,
        SearchFilter searchFilter,
        int page,
        bool selectDub)
    {
        query = query.Replace(" ", "+");

        var animes = new List<Anime>();

        var response = searchFilter switch
        {
            SearchFilter.Find => await _http.SendHttpRequestAsync($"{BaseUrl}/search?keyword=" + query),
            SearchFilter.Popular => await _http.SendHttpRequestAsync($"{BaseUrl}/most-popular?page=" + page),
            SearchFilter.NewSeason => await _http.SendHttpRequestAsync($"{BaseUrl}/recently-added?page=" + page),
            SearchFilter.LastUpdated => await _http.SendHttpRequestAsync($"{BaseUrl}/?page=" + page),
            _ => throw new SearchFilterNotSupportedException("Search filter not supported")
        };

        if (string.IsNullOrEmpty(response))
            return animes;

        var document = new HtmlDocument();
        document.LoadHtml(response);

        var nodes = document.DocumentNode.Descendants()
            .Where(node => node.HasClass("flw-item")).ToList();

        for (int i = 0; i < nodes.Count; i++)
        {
            var img = "";
            var title = "";
            var category = "";
            var dataId = "";

            var imgNode = nodes[i].SelectSingleNode(".//img");
            if (imgNode is not null)
                img = imgNode.Attributes["data-src"].Value;

            var dataIdNode = nodes[i].SelectSingleNode(".//a[@data-id]");
            if (dataIdNode is not null)
                dataId = dataIdNode.Attributes["data-id"].Value;

            var nameNode = nodes[i].SelectSingleNode(".//div[@class='film-detail']")
                .SelectSingleNode(".//a");
            if (nameNode is not null)
            {
                category = nameNode.Attributes["href"].Value;
                title = nameNode.Attributes["title"].Value; //OR name = nameNode.InnerText;
            }

            animes.Add(new Anime()
            {
                Id = category,
                Site = AnimeSites.Zoro,
                Image = img,
                Title = title,
                Category = category,
                Link = BaseUrl + category,
            });
        }

        return animes;
    }

    public override async Task<Anime> GetAnimeInfoAsync(string id)
    {
        var dataId = id.Split('-').Last().Split('?')[0];
        var url = $"{BaseUrl}/ajax/v2/episode/list/{dataId}";

        //Get anime details
        var response = await _http.SendHttpRequestAsync($"{BaseUrl}{id}");
        //https://stackoverflow.com/questions/122641/how-can-i-decode-html-characters-in-c
        //HttpUtility.HtmlDecode();

        var anime = new Anime()
        {
            Id = id,
            Site = AnimeSites.Zoro
        };

        if (string.IsNullOrEmpty(response))
            return anime;

        var document = new HtmlDocument();
        document.LoadHtml(HtmlEntity.DeEntitize(response));

        anime.Title = document.DocumentNode
            .SelectSingleNode(".//h2[contains(@class, 'film-name')]")?
            .InnerText ?? "";

        anime.Summary = document.DocumentNode.SelectSingleNode(".//div[contains(@class, 'film-description')]")?
            .InnerText?.Trim() ?? "";

        anime.Image = document.DocumentNode.SelectSingleNode(".//img[contains(@class, 'film-poster-img')]")?
            .Attributes["src"]?.Value?.ToString() ?? "";

        var itemHeadNodes = document.DocumentNode.SelectNodes(".//div[@class='anisc-info-wrap']/div[@class='anisc-info']//span[@class='item-head']");
        //var overviewNode = document.DocumentNode.SelectNodes(".//div[@class='anisc-info-wrap']/div[@class='anisc-info']")[0];
        //anime.Summary = overviewNode.InnerText;

        var overviewNode = itemHeadNodes.FirstOrDefault(x => !string.IsNullOrEmpty(x.InnerHtml)
            && x.InnerHtml.ToLower().Contains("overview"))?
            .ParentNode.SelectSingleNode(".//span[@class='name']")
            ?? itemHeadNodes.FirstOrDefault(x => !string.IsNullOrEmpty(x.InnerHtml)
            && x.InnerHtml.ToLower().Contains("overview"))?
            .ParentNode.SelectSingleNode(".//div[@class='text']");
        if (overviewNode is not null)
            anime.Summary = overviewNode.InnerText.Trim();

        var typeNode = document.DocumentNode.SelectNodes(".//div[@class='film-stats']/span[@class='dot']")
            .FirstOrDefault()!.NextSibling.NextSibling;
        if (typeNode is not null)
            anime.Type = typeNode.InnerText;

        var statusNode = itemHeadNodes.FirstOrDefault(x => !string.IsNullOrEmpty(x.InnerHtml)
            && x.InnerHtml.ToLower().Contains("status"))?
            .ParentNode.SelectSingleNode(".//span[@class='name']");
        if (statusNode is not null)
            anime.Status = statusNode.InnerText;

        var genresNode = itemHeadNodes.FirstOrDefault(x => !string.IsNullOrEmpty(x.InnerHtml)
            && x.InnerHtml.ToLower().Contains("genres"))?
            .ParentNode.SelectNodes(".//a").ToList();
        if (genresNode is not null)
            anime.Genres.AddRange(genresNode.Select(x => new Genre(x.Attributes["title"].Value)));

        var airedNode = itemHeadNodes.FirstOrDefault(x => !string.IsNullOrEmpty(x.InnerHtml)
            && x.InnerHtml.ToLower().Contains("aired"))?
            .ParentNode.SelectSingleNode(".//span[@class='name']");
        if (airedNode is not null)
            anime.Released = airedNode.InnerText;

        var synonymsNode = itemHeadNodes.FirstOrDefault(x => !string.IsNullOrEmpty(x.InnerHtml)
            && x.InnerHtml.ToLower().Contains("synonyms"))?
            .ParentNode.SelectSingleNode(".//span[@class='name']");
        if (synonymsNode is not null)
            //anime.OtherNames = HtmlEntity.DeEntitize(synonymsNode.InnerText);
            anime.OtherNames = synonymsNode.InnerText;

        return anime;
    }

    public override async Task<List<Episode>> GetEpisodesAsync(string id)
    {
        var dataId = id.Split('-').Last().Split('?')[0];
        var url = $"{BaseUrl}/ajax/v2/episode/list/{dataId}";

        //Get anime episodes
        var json = await _http.SendHttpRequestAsync(url);
        var jObj = JObject.Parse(json);
        var response = jObj["html"]!.ToString();

        var document = new HtmlDocument();
        document.LoadHtml(response);

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
                Id = link,
                Name = $"{i + 1} - {title}",
                Link = link,
                Number = dataNumber
            });
        }

        return episodes;
    }

    public override async Task<List<VideoServer>> GetVideoServersAsync(string episodeId) =>
        await GetVideoServersAsync(episodeId);

    public async Task<List<VideoServer>> GetVideoServersAsync(
        string episodeId,
        SubDub subDub = SubDub.All)
    {
        var dataId = episodeId.Split(new string[] { "ep=" }, StringSplitOptions.None).Last();

        var url = $"{BaseUrl}/ajax/v2/episode/servers?episodeId={dataId}";
        var response = await _http.SendHttpRequestAsync(url);

        if (string.IsNullOrEmpty(response))
            return new();

        var data = JObject.Parse(response);
        var html = data["html"]!.ToString();

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var nodes = doc.DocumentNode.Descendants()
            .Where(node => node.HasClass("server-item")).ToList();

        var videoServers = new List<VideoServer>();

        for (int i = 0; i < nodes.Count; i++)
        {
            dataId = nodes[i].Attributes["data-id"].Value;
            var dataType = nodes[i].Attributes["data-type"].Value.ToLower().Trim();
            var serverName = $"({dataType.ToUpper()}) {nodes[i].InnerText.Trim()}";

            if (subDub != SubDub.All)
            {
                if (dataType == "dub" && subDub != SubDub.Dub)
                    continue;

                if (dataType == "sub" && subDub != SubDub.Sub)
                    continue;
            }

            url = $"https://zoro.to/ajax/v2/episode/sources?id={dataId}";
            response = await _http.SendHttpRequestAsync(url);

            data = JObject.Parse(response);
            var type = data["type"]!.ToString();
            var server = data["server"]!.ToString();

            var link = data["link"]!.ToString();
            var embedHeaders = new Dictionary<string, string>()
            {
                { "Referer", BaseUrl + "/" }
            };

            videoServers.Add(new VideoServer(serverName, new FileUrl(link, embedHeaders)));
        }

        return videoServers;
    }

    public override IVideoExtractor? GetVideoExtractor(VideoServer server)
    {
        var domainParser = new DomainParser(new WebTldRuleProvider());
        var domainInfo = domainParser.Parse(server.Embed.Url);

        if (domainInfo.Domain.Contains("rapid"))
        {
            return new RapidCloud(_http, server);
        }
        else if (domainInfo.Domain.Contains("sb"))
        {
            return new StreamSB(_http, server);
        }
        else if (domainInfo.Domain.Contains("streamta"))
        {
            return new StreamTape(_http, server);
        }

        return null;
    }
}