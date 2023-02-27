﻿using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Nager.PublicSuffix;
using Newtonsoft.Json.Linq;
using AnimeDl.Models;
using AnimeDl.Extractors;
using AnimeDl.Exceptions;
using AnimeDl.Utils.Extensions;
using AnimeDl.Extractors.Interfaces;

namespace AnimeDl.Scrapers;

/// <summary>
/// Scraper for interacting with gogoanime.
/// </summary>
public class GogoAnimeScraper : BaseScraper
{
    public override string Name { get; set; } = "Gogo";

    public override bool IsDubAvailableSeparately { get; set; } = true;

    private string _baseUrl { get; set; } = default!;

    public override string BaseUrl => _baseUrl;

    public string CdnUrl { get; private set; } = default!;

    public GogoAnimeScraper(HttpClient http) : base(http)
    {
    }

    private async Task EnsureUrlsSet()
    {
        if (BaseUrl is not null)
            return;

        var json = await _http.SendHttpRequestAsync("https://raw.githubusercontent.com/jerry08/AnimeDl/master/AnimeDl/Data/gogoanime.json");
        if (!string.IsNullOrEmpty(json))
        {
            var jObj = JObject.Parse(json);

            _baseUrl = jObj["base_url"]!.ToString();
            CdnUrl = jObj["cdn_url"]!.ToString();
        }
    }

    public override async Task<List<Anime>> SearchAsync(string query,
        SearchFilter searchFilter, int page, bool selectDub)
    {
        await EnsureUrlsSet();

        //query = selectDub ? query + "%(Dub)" : query;
        //query = query.Replace(" ", "+");

        query = selectDub ? query + " (Dub)" : query;
        query = Uri.EscapeUriString(query);

        var animes = new List<Anime>();

        var response = searchFilter switch
        {
            SearchFilter.Find => await _http.SendHttpRequestAsync($"{BaseUrl}search.html?keyword=" + query),
            SearchFilter.Popular => await _http.SendHttpRequestAsync($"{BaseUrl}popular.html?page=" + page),
            SearchFilter.NewSeason => await _http.SendHttpRequestAsync($"{BaseUrl}new-season.html?page=" + page),
            SearchFilter.LastUpdated => await _http.SendHttpRequestAsync($"{BaseUrl}?page=" + page),
            _ => throw new SearchFilterNotSupportedException("Search filter not supported")
        };

        if (string.IsNullOrEmpty(response))
            return animes;

        var document = new HtmlDocument();
        document.LoadHtml(response);

        var itemsNode = document.DocumentNode.Descendants()
            .FirstOrDefault(node => node.HasClass("items"));

        if (itemsNode is not null)
        {
            var nodes = itemsNode.Descendants()
                .Where(node => node.Name == "li").ToList();
            for (int i = 0; i < nodes.Count; i++)
            {
                var img = "";
                var title = "";
                var category = "";
                var released = "";
                var link = "";

                var imgNode = nodes[i].SelectSingleNode(".//div[@class='img']/a/img");
                if (imgNode is not null)
                    img = imgNode.Attributes["src"].Value;

                var nameNode = nodes[i].SelectSingleNode(".//p[@class='name']/a");
                if (nameNode is not null)
                {
                    category = nameNode.Attributes["href"].Value;
                    title = nameNode.Attributes["title"].Value; //OR name = nameNode.InnerText;
                }

                var releasedNode = nodes[i].SelectSingleNode(".//p[@class='released']");
                if (releasedNode is not null)
                    released = new string(releasedNode.InnerText.Where(char.IsDigit).ToArray());

                if (category.Contains("kyokou-suiri"))
                {

                }

                var id = category.Contains("-episode") ?
                    "/category" + category.Remove(category.LastIndexOf("-episode")) : category;

                link = BaseUrl + category;

                animes.Add(new Anime()
                {
                    Id = id,
                    Site = AnimeSites.GogoAnime,
                    Image = img,
                    Title = title,
                    Category = category,
                    Released = released,
                    Link = link,
                });
            }
        }

        return animes;
    }

    public override async Task<Anime> GetAnimeInfoAsync(string id)
    {
        await EnsureUrlsSet();

        var url = $"{BaseUrl}{id}";

        var anime = new Anime() { Id = id };

        if (id.Contains("-episode"))
        {
            var epsResponse = await _http.SendHttpRequestAsync(url);

            var epsDocument = new HtmlDocument();
            epsDocument.LoadHtml(epsResponse);

            url = epsDocument.DocumentNode
                .SelectSingleNode(".//div[@class='anime-info']/a")?.Attributes["href"]?.Value;

            if (url is null)
                return anime;

            url = $"{BaseUrl}{url}";
        }

        var response = await _http.SendHttpRequestAsync(url);

        if (string.IsNullOrEmpty(response))
            return anime;

        anime.Category = url;

        var document = new HtmlDocument();
        document.LoadHtml(response);

        var animeInfoNodes = document.DocumentNode
            .SelectNodes(".//div[@class='anime_info_body_bg']/p").ToList();

        var imgNode = document.DocumentNode.SelectSingleNode(".//div[@class='anime_info_body_bg']/img");
        if (imgNode is not null)
            anime.Image = imgNode.Attributes["src"].Value;

        var titleNode = document.DocumentNode.SelectSingleNode(".//div[@class='anime_info_body_bg']/h1");
        if (titleNode is not null)
            anime.Title = titleNode.InnerText;

        for (int i = 0; i < animeInfoNodes.Count; i++)
        {
            switch (i)
            {
                case 0: //Bookmarks
                    break;
                case 1: //Type (e.g TV Series)
                    anime.Type = Regex.Replace(animeInfoNodes[i].InnerText, @"\t|\n|\r", "");
                    anime.Type = new Regex("[ ]{2,}", RegexOptions.None).Replace(anime.Type, " ").Trim();
                    anime.Type = anime.Type.Trim();
                    break;
                case 2: //Plot SUmmary
                    anime.Summary = animeInfoNodes[i].InnerText.Trim();
                    break;
                case 3: //Genre
                    var genres = animeInfoNodes[i].InnerText.Replace("Genre:", "").Trim().Split(',');
                    foreach (var genre in genres)
                        anime.Genres.Add(new(genre));
                    break;
                case 4: //Released Year
                    anime.Released = Regex.Replace(animeInfoNodes[i].InnerText, @"\t|\n|\r", "");
                    anime.Released = new Regex("[ ]{2,}", RegexOptions.None).Replace(anime.Released, " ").Trim();
                    break;
                case 5: //Status
                    anime.Status = Regex.Replace(animeInfoNodes[i].InnerText, @"\t|\n|\r", "");
                    anime.Status = new Regex("[ ]{2,}", RegexOptions.None).Replace(anime.Status, " ").Trim();
                    anime.Status = anime.Status.Trim();
                    break;
                case 6: //Other Name
                    anime.OtherNames = Regex.Replace(animeInfoNodes[i].InnerText, @"\t|\n|\r", "");
                    anime.OtherNames = new Regex("[ ]{2,}", RegexOptions.None).Replace(anime.OtherNames, " ").Trim();
                    break;
                default:
                    break;
            }
        }

        return anime;
    }

    public override async Task<List<Episode>> GetEpisodesAsync(string id)
    {
        await EnsureUrlsSet();

        var episodes = new List<Episode>();

        var response = await _http.SendHttpRequestAsync($"{BaseUrl}{id}");

        if (string.IsNullOrEmpty(response))
            return episodes;

        var document = new HtmlDocument();
        document.LoadHtml(response);

        var lastEpisodes = document.DocumentNode.Descendants().Where(x => x.Attributes["ep_end"] is not null)
            .ToList();
        var lastEpisode = lastEpisodes.LastOrDefault()?.Attributes["ep_end"].Value;

        var animeId = document.DocumentNode.Descendants().FirstOrDefault(x => x.Id == "movie_id")?.Attributes["value"].Value;

        var url = $"https://ajax.gogo-load.com/ajax/load-list-episode?ep_start=0&ep_end={lastEpisode}&id={animeId}";
        //response = await _http.SendHttpRequestAsync(CdnUrl + animeId);
        response = await _http.SendHttpRequestAsync(url);

        document = new HtmlDocument();
        document.LoadHtml(response);

        var liNodes = document.DocumentNode.Descendants()
            .Where(node => node.Name == "li").ToList();

        for (int i = 0; i < liNodes.Count; i++)
        {
            var epName = "";
            var href = "";
            var subOrDub = "";

            var hrefNode = liNodes[i].SelectSingleNode(".//a");
            if (hrefNode is not null)
                href = hrefNode.Attributes["href"].Value.Trim();

            var nameNode = liNodes[i].SelectSingleNode(".//div[@class='name']");
            if (nameNode is not null)
                epName = nameNode.InnerText;

            var subDubNode = liNodes[i].SelectSingleNode(".//div[@class='cate']");
            if (subDubNode is not null)
            {
                //subOrDub = subDubNode.Attributes["title"].Value; //OR name = nameNode.InnerText;
                subOrDub = subDubNode.InnerText;
            }

            //var epNumber = Convert.ToSingle(link.Split(new char[] { '-' }).LastOrDefault());
            var epNumber = Convert.ToSingle(epName.ToLower().Replace("ep", "").Trim());

            episodes.Add(new Episode()
            {
                Id = href,
                Link = BaseUrl + href,
                Number = epNumber,
                Name = epName,
            });
        }

        return episodes;
    }

    private string HttpsIfy(string text)
        => string.Join("", text.Take(2)) == "//" ? $"https:{text}" : text;

    public override async Task<List<VideoServer>> GetVideoServersAsync(string episodeId)
    {
        await EnsureUrlsSet();

        var episodeUrl = $"{BaseUrl}{episodeId}";

        var response = await _http.SendHttpRequestAsync(episodeUrl);

        if (string.IsNullOrEmpty(response))
            return new();

        var doc = new HtmlDocument();
        doc.LoadHtml(response);

        //Exception for fire force season 2 episode 1
        if (response.Contains(">404</h1>"))
            response = await _http.SendHttpRequestAsync(episodeUrl + "-1");

        var videoServers = new List<VideoServer>();

        var servers = doc.DocumentNode
            .SelectNodes(".//div[@class='anime_muti_link']/ul/li").ToList();
        for (int i = 0; i < servers.Count; i++)
        {
            var name = servers[i].SelectSingleNode("a").InnerText.Replace("Choose this server", "").Trim();
            var url = HttpsIfy(servers[i].SelectSingleNode("a").Attributes["data-video"].Value);
            var embed = new FileUrl(url);

            videoServers.Add(new VideoServer(name, embed));
        }

        return videoServers.OrderBy(x => x.Name).ToList();
    }

    public override IVideoExtractor? GetVideoExtractor(VideoServer server)
    {
        var domainParser = new DomainParser(new WebTldRuleProvider());
        var domainInfo = domainParser.Parse(server.Embed.Url);

        if (domainInfo.Domain.Contains("gogo")
            || domainInfo.Domain.Contains("goload")
            || domainInfo.Domain.Contains("playgo")
            || domainInfo.Domain.Contains("anihdplay"))
        {
            return new GogoCDN(_http, server);
        }
        else if (domainInfo.Domain.Contains("sb")
            || domainInfo.Domain.Contains("sss"))
        {
            return new StreamSB(_http, server);
        }
        else if (domainInfo.Domain.Contains("fplayer")
            || domainInfo.Domain.Contains("fembed"))
        {
            return new FPlayer(_http, server);
        }

        return null;
    }

    public override async Task<List<Genre>> GetGenresAsync()
    {
        await EnsureUrlsSet();

        var genres = new List<Genre>();

        var response = await _http.SendHttpRequestAsync(BaseUrl);

        if (string.IsNullOrEmpty(response))
            return genres;

        var document = new HtmlDocument();
        document.LoadHtml(response);

        var genresNode = document.DocumentNode.Descendants()
            .FirstOrDefault(node => node.GetClasses().Contains("genre"));

        if (genresNode is not null)
        {
            var nodes = genresNode.Descendants()
                .Where(node => node.Name == "li").ToList();
            for (int i = 0; i < nodes.Count; i++)
            {
                var name = "";
                var link = "";

                var nameNode = nodes[i].SelectSingleNode(".//a");
                if (nameNode is not null)
                {
                    link = nameNode.Attributes["href"].Value;
                    name = nameNode.Attributes["title"].Value; //OR name = nameNode.InnerText;
                }

                genres.Add(new Genre(name, link));
            }
        }

        return genres;
    }
}