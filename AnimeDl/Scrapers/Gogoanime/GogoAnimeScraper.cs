using System;
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
using System.Xml.Linq;

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
        SetData();
    }

    private void SetData()
    {
        var json = _http.Get("https://raw.githubusercontent.com/jerry08/AnimeDl/master/AnimeDl/Data/gogoanime.json");
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
        //query = selectDub ? query + "%(Dub)" : query;
        //query = query.Replace(" ", "+");

        query = selectDub ? query + " (Dub)" : query;
        query = Uri.EscapeUriString(query);

        var animes = new List<Anime>();

        var response = searchFilter switch
        {
            SearchFilter.Find => await _http.SendHttpRequestAsync($"{BaseUrl}search.html?keyword=" + query),
            //SearchFilter.AllList => await _http.SendHttpRequestAsync($"https://animesa.ga/animel.php"),
            SearchFilter.AllList => await _http.SendHttpRequestAsync($"https://animefrenzy.org/anime"),
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
            .Where(node => node.HasClass("items")).FirstOrDefault();

        if (itemsNode is not null)
        {
            var nodes = itemsNode.Descendants()
                .Where(node => node.Name == "li").ToList();
            for (int i = 0; i < nodes.Count; i++)
            {
                //var currentNode = HtmlNode.CreateNode(nodes[i].OuterHtml);

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

                if (category.Contains("-episode"))
                    category = "/category" + category.Remove(category.LastIndexOf("-episode"));

                link = BaseUrl + category;

                animes.Add(new Anime()
                {
                    Id = category,
                    Site = AnimeSites.GogoAnime,
                    Image = img,
                    Title = title,
                    EpisodesNum = 0,
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
        var response = await _http.SendHttpRequestAsync($"{BaseUrl}{id}");

        var anime = new Anime();

        if (string.IsNullOrEmpty(response))
            return anime;

        var document = new HtmlDocument();
        document.LoadHtml(response);

        var animeInfoNodes = document.DocumentNode
            .SelectNodes(".//div[@class='anime_info_body_bg']/p").ToList();

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
        var episodes = new List<Episode>();

        var response = await _http.SendHttpRequestAsync($"{BaseUrl}{id}");

        if (string.IsNullOrEmpty(response))
            return episodes;

        var document = new HtmlDocument();
        document.LoadHtml(response);

        var movieId = document.DocumentNode.Descendants().Where(x => x.Id == "movie_id")
            .FirstOrDefault()?.Attributes["value"].Value;

        //https://ajax.gogocdn.net/ajax/load-list-episode?ep_start=500&ep_end=500&id=133&default_ep=0&alias=naruto-shippuden

        //string sf = $"https://vidstream.pro/info/{movieId}?domain=gogoanime.be&skey=db04c5540929bebd456b9b16643fc436";

        response = await _http.SendHttpRequestAsync(CdnUrl + movieId);

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
    {
        if (string.Join("", text.Take(2)) == "//")
            return $"https:{text}";
        return text;
    }

    public override async Task<List<VideoServer>> GetVideoServersAsync(string episodeId)
    {
        var episodeUrl = $"{BaseUrl}{episodeId}";

        var response = await _http.SendHttpRequestAsync(episodeUrl);

        if (string.IsNullOrEmpty(response))
            return new();

        var doc = new HtmlDocument();
        doc.LoadHtml(response);

        //Exception for fire force season 2 episode 1
        if (response.Contains(@">404</h1>"))
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

        return videoServers;
    }

    public override IVideoExtractor GetVideoExtractor(VideoServer server)
    {
        var domainParser = new DomainParser(new WebTldRuleProvider());
        var domainInfo = domainParser.Parse(server.Embed.Url);

        if (domainInfo.Domain.Contains("gogo")
            || domainInfo.Domain.Contains("goload"))
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

        return default!;
    }

    public override async Task<List<Genre>> GetGenresAsync()
    {
        var genres = new List<Genre>();

        var response = await _http.SendHttpRequestAsync(BaseUrl);

        if (string.IsNullOrEmpty(response))
            return genres;

        var document = new HtmlDocument();
        document.LoadHtml(response);

        var genresNode = document.DocumentNode.Descendants()
            .Where(node => node.GetClasses().Contains("genre")).FirstOrDefault();

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