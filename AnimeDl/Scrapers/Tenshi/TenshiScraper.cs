using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using HtmlAgilityPack;
using AnimeDl.Utils.Extensions;
using AnimeDl.Exceptions;

namespace AnimeDl.Scrapers;

internal class TenshiScraper : BaseScraper
{
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

    public TenshiScraper(NetHttpClient netHttpClient) : base(netHttpClient)
    {
    }

    public override async Task<List<Anime>> SearchAsync(string searchQuery,
        SearchFilter searchFilter,
        int page)
    {
        var animes = new List<Anime>();

        //searchQuery = searchQuery.Replace(" ", "%20");
        searchQuery = searchQuery.Replace(" ", "+");

        var htmlData = searchFilter switch
        {
            SearchFilter.Find => await _netHttpClient.SendHttpRequestAsync($"{BaseUrl}/anime?q={searchQuery}&s=vtt-d", CookieHeader),
            SearchFilter.NewSeason => await _netHttpClient.SendHttpRequestAsync($"{BaseUrl}/anime?s=rel-d&page=" + page, CookieHeader),
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

        var html = await _netHttpClient.SendHttpRequestAsync(anime.Link, CookieHeader);
        if (html is null)
        {
            return episodes;
        }

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

        /*var nodes = document.DocumentNode
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
        }*/

        var totalEpisodes = Convert.ToInt32(document.DocumentNode
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
        }

        return episodes;
    }

    public override async Task<List<Quality>> GetEpisodeLinksAsync(Episode episode)
    {
        var list = new List<Quality>();

        var html = await _netHttpClient.SendHttpRequestAsync(episode.EpisodeLink, CookieHeader);
        if (html is null)
        {
            return list;
        }

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var nodes = doc.DocumentNode
            .SelectNodes(".//ul[@class='dropdown-menu']/li/a[@class='dropdown-item']")
            .ToList();

        foreach (var node in nodes)
        {
            var server = node.InnerText.Replace(" ", "").Replace("/-", "");
            bool dub = node.SelectSingleNode(".//span[@title='Audio: English']") != null;

            if (dub)
            {
                server = $"Dub - ${server}";
            }

            var urlParam = new Uri(node.Attributes["href"].Value).DecodeQueryParameters();
            var url = $"{BaseUrl}/embed?" + urlParam.FirstOrDefault().Key + "=" + urlParam.FirstOrDefault().Value;
            var headers = new WebHeaderCollection()
            {
                CookieHeader,
                { "Referer", episode.EpisodeLink }
            };

            html = await _netHttpClient.SendHttpRequestAsync(url, headers);

            var unSanitized = html.SubstringAfter("player.source = ").SubstringBefore(";");
            var jObj = JObject.Parse(unSanitized);
            var sources = JArray.Parse(jObj["sources"]!.ToString());
            
            foreach (var source in sources)
            {
                list.Add(new Quality()
                {
                    Headers = CookieHeader,
                    IsM3U8 = source["src"]!.ToString().Contains(".m3u8"),
                    FileType = source["type"]!.ToString(),
                    Resolution = source["size"]!.ToString(),
                    QualityUrl = source["src"]!.ToString(),
                });
            }
        }

        return list;
    }
}