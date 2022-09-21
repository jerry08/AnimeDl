using System;
using System.Net;
using System.Net.Http;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using HtmlAgilityPack;
using AnimeDl.Exceptions;
using AnimeDl.Utils.Extensions;
using AnimeDl.Models;
using AnimeDl.Extractors;
using AnimeDl.Extractors.Interfaces;

namespace AnimeDl.Scrapers;

internal class TenshiScraper : BaseScraper
{
    public override string Name { get; set; } = "Tenshi";

    public override bool IsDubAvailableSeparately { get; set; } = false;

    public override string BaseUrl => "https://tenshi.moe";

    public WebHeaderCollection CookieHeader = new()
    {
        { "Cookie", "__ddg1_=;__ddg2_=;loop-view=thumb" }
    };

    //private readonly List<Cookie> Cookies = new()
    //    {
    //        new Cookie("__ddg1_", "") { Domain = "tenshi" },
    //        new Cookie("__ddg2_", "") { Domain = "tenshi" },
    //        new Cookie("loop-view", "thumb") { Domain = "tenshi" },
    //    };

    public TenshiScraper(HttpClient http) : base(http)
    {
    }

    public override async Task<List<Anime>> SearchAsync(string query,
        SearchFilter searchFilter,
        int page,
        bool selectDub)
    {
        var animes = new List<Anime>();

        //query = query.Replace(" ", "%20");
        query = query.Replace(" ", "+");

        var htmlData = searchFilter switch
        {
            SearchFilter.Find => await _http.SendHttpRequestAsync($"{BaseUrl}/anime?q={query}&s=vtt-d", CookieHeader),
            SearchFilter.NewSeason => await _http.SendHttpRequestAsync($"{BaseUrl}/anime?s=rel-d&page=" + page, CookieHeader),
            _ => throw new SearchFilterNotSupportedException("Search filter not supported")
        };

        if (htmlData is null)
        {
            return animes;
        }

        var doc = new HtmlDocument();
        doc.LoadHtml(htmlData);

        var nodes = doc.DocumentNode
            .SelectNodes(".//ul[@class='loop anime-loop thumb']/li").ToList();

        foreach (var node in nodes)
        {
            var anime = new Anime();
            anime.Site = AnimeSites.Tenshi;
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
        var episodes = new List<Episode>();

        var html = await _http.SendHttpRequestAsync(anime.Link, CookieHeader);
        if (html is null)
            return episodes;

        var document = new HtmlDocument();
        document.LoadHtml(html);

        var synonymNodes = document.DocumentNode.SelectNodes
            (".//ul[@class='info-list']/li[@class='synonym meta-data']/div[@class='info-box']/span[@class='value']");
        if (synonymNodes is not null)
            anime.OtherNames = synonymNodes[0].InnerText.Trim();

        var typeNode = document.DocumentNode.SelectSingleNode
            (".//ul[@class='info-list']/li[@class='type meta-data']/span[@class='value']/a");
        if (typeNode is not null)
            anime.Type = typeNode.InnerText.Trim();

        var statusNode = document.DocumentNode.SelectSingleNode
            (".//ul[@class='info-list']/li[@class='status meta-data']/span[@class='value']/a");
        if (statusNode is not null)
            anime.Status = statusNode.InnerText.Trim();

        var releasedDateNodes = document.DocumentNode.SelectSingleNode
            (".//ul[@class='info-list']/li[@class='release-date meta-data']/span[@class='value']");
        if (releasedDateNodes is not null)
            anime.Released = releasedDateNodes.InnerText.Trim();

        var productionsNodes = document.DocumentNode.SelectNodes
            (".//ul[@class='info-list']/li[@class='production meta-data']/span[@class='value']")
            .ToList();
        if (productionsNodes is not null)
            productionsNodes.ForEach(x => anime.Productions.Add(x.InnerText.Trim()));

        var genreNodes = document.DocumentNode.SelectNodes
            (".//ul[@class='info-list']/li[@class='genre meta-data']//a");
        if (genreNodes is not null)
            anime.Genres.AddRange(genreNodes.Select(x => new Genre(x.InnerHtml.Trim())));

        //var nodes = document.DocumentNode
        //    .SelectNodes(".//ul[@class='loop episode-loop list']/li").ToList();

        var nodes = document.DocumentNode
            .SelectNodes(".//ul[contains(@class, 'episode-loop')]/li").ToList();

        foreach (var node in nodes)
        {
            var episode = new Episode();

            episode.Name = node.SelectSingleNode(".//div[@class='episode-title']").InnerText;
            episode.Number = Convert.ToSingle(node.SelectSingleNode(".//div[@class='episode-slug']").InnerText.Replace("Episode ", ""));
            episode.Link = $"{anime.Link}/{episode.Number}";
            episode.Image = node.SelectSingleNode(".//img").Attributes["src"].Value;
            episode.Description = node.SelectSingleNode(".//a").Attributes["data-content"].Value;

            episodes.Add(episode);
        }

        /*var totalEpisodes = Convert.ToInt32(document.DocumentNode
            .SelectSingleNode(".//section[@class='entry-episodes']/h2/span[@class='badge badge-secondary align-top']").InnerText);

        for (int i = 1; i <= totalEpisodes; i++)
        {
            var episode = new Episode
            {
                EpisodeNumber = i,
                EpisodeName = $"Episode {i}",
                EpisodeLink = $"{anime.Link}/{i}"
            };

            episodes.Add(episode);
        }*/

        return episodes;
    }

    public override async Task<List<VideoServer>> GetVideoServersAsync(Episode episode)
    {
        var videoServers = new List<VideoServer>();

        var html = await _http.SendHttpRequestAsync(episode.Link, CookieHeader);
        if (html is null)
            return videoServers;

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var nodes = doc.DocumentNode
            .SelectNodes(".//ul[@class='dropdown-menu']/li/a[@class='dropdown-item']")
            .ToList();

        foreach (var node in nodes)
        {
            var server = node.InnerText.Replace(" ", "").Replace("/-", "").Trim();
            var dub = node.SelectSingleNode(".//span[@title='Audio: English']") != null;
            server = dub ? $"Dub - {server}" : server;

            var urlParam = new Uri(node.Attributes["href"].Value).DecodeQueryParameters();
            var url = $"{BaseUrl}/embed?" + urlParam.FirstOrDefault().Key + "=" + urlParam.FirstOrDefault().Value;
            var headers = new WebHeaderCollection()
            {
                CookieHeader,
                { "Referer", episode.Link }
            };

            videoServers.Add(new VideoServer(server, new FileUrl(url, headers)));
        }

        return videoServers;
    }

    public override IVideoExtractor GetVideoExtractor(VideoServer server)
    {
        return new TenshiVideoExtractor(_http, server);
    }
}