using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AnimeDl.Exceptions;
using AnimeDl.Extractors;
using AnimeDl.Extractors.Interfaces;
using AnimeDl.Models;
using AnimeDl.Utils.Extensions;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using TaskExecutor;

namespace AnimeDl.Scrapers;

public class AnimePaheScraper : BaseScraper
{
    public override string Name { get; set; } = "AnimePahe";

    public override bool IsDubAvailableSeparately { get; set; } = false;

    public override string BaseUrl => "https://animepahe.com";

    private static readonly Regex _videoServerRegex = new("(.+) · (.+)p \\((.+)MB\\) ?(.*)");

    public AnimePaheScraper(HttpClient http) : base(http)
    {
    }

    public override async Task<List<Anime>> SearchAsync(
        string query,
        SearchFilter searchFilter,
        int page,
        bool selectDub)
    {
        var animes = new List<Anime>();

        var response = searchFilter switch
        {
            SearchFilter.Find => await _http.SendHttpRequestAsync($"{BaseUrl}/api?m=search&q={Uri.EscapeUriString(query)}"),
            SearchFilter.Ongoing => await _http.SendHttpRequestAsync($"{BaseUrl}/api?m=airing&page={page}"),
            _ => throw new SearchFilterNotSupportedException("Search filter not supported")
        };

        if (string.IsNullOrEmpty(response))
            return animes;

        var data = JObject.Parse(response)["data"];
        if (data is null)
            return animes;

        if (searchFilter == SearchFilter.Ongoing)
        {
            return data.Select(x => new Anime()
            {
                Id = x["anime_session"]!.ToString(),
                Title = x["anime_title"]!.ToString(),
                Image = x["snapshot"]!.ToString(),
                Site = AnimeSites.AnimePahe
            }).ToList();
        }

        var list = data.Select(x => new Anime()
        {
            Id = x["session"]!.ToString(),
            Title = x["title"]!.ToString(),
            Type = x["type"]!.ToString(),
            Episodes = Convert.ToInt32(x["episodes"]),
            Status = x["status"]!.ToString(),
            Season = x["season"]!.ToString(),
            Year = Convert.ToInt32(x["year"]),
            Score = Convert.ToInt32(x["score"]),
            Image = x["poster"]!.ToString(),
            Site = AnimeSites.AnimePahe
        });

        animes.AddRange(list);

        return animes;
    }

    public override async Task<Anime> GetAnimeInfoAsync(string id)
    {
        var url = $"{BaseUrl}/anime/{id}";
        var response = await _http.SendHttpRequestAsync(url);

        var document = new HtmlDocument();
        document.LoadHtml(HtmlEntity.DeEntitize(response));

        var anime = new Anime()
        {
            Id = id,
            Link = url,
            Site = AnimeSites.AnimePahe
        };

        anime.Title = document.DocumentNode
            .SelectSingleNode(".//div[contains(@class, 'header-wrapper')]/header/div/h1/span")?
            .InnerText ?? "";

        anime.Image = document.DocumentNode
            .SelectSingleNode(".//header/div/div/div/a/img")!.Attributes["data-src"]!.Value;

        anime.Summary = document.DocumentNode
            .SelectSingleNode(".//div[contains(@class, 'anime-summary')]/div")?
            .InnerText ?? "";

        anime.Genres = document.DocumentNode
            .SelectNodes(".//div[contains(@class, 'anime-info')]/div/ul/li/a")
            .Select(el => new Genre(el.Attributes["title"].Value)).ToList();

        anime.Status = document.DocumentNode
            .SelectSingleNode(".//div[contains(@class, 'anime-info')]/div")?
            .InnerText?.Trim() ?? "";

        return anime;
    }

    public override async Task<List<Episode>> GetEpisodesAsync(string id)
    {
        var list = new List<Episode>();

        var response = await _http.SendHttpRequestAsync($"{BaseUrl}/api?m=release&id={id}&sort=episode_asc&page=1");

        var data = JObject.Parse(response);

        var epsSelector = (JToken el) => new Episode()
        {
            //Description = el["description"]!.ToString(),
            //Id = el["id"]!.ToString(),
            //Id = el["session"]!.ToString(),
            Id = $"{BaseUrl}/play/{id}/{el["session"]}",
            Number = Convert.ToInt32(el["episode"]!),
            Image = el["snapshot"]!.ToString(),
            Description = el["title"]!.ToString(),
            Duration = (float)TimeSpan.Parse(el["duration"]!.ToString()).TotalMilliseconds
        };

        list.AddRange(data["data"]!.Select(epsSelector));

        var lastPage = Convert.ToInt32(data["last_page"]);

        if (lastPage < 2)
            return list;

        // Start at index of 2 since we've already gotten the first page above.
        var functions = Enumerable.Range(2, lastPage - 1).Select(i =>
            (Func<Task<string>>)(async () => await _http.SendHttpRequestAsync(
                $"{BaseUrl}/api?m=release&id={id}&sort=episode_asc&page={i}"
            )));

        var results = await TaskEx.Run(functions, 20);

        list.AddRange(results.SelectMany(response => JObject.Parse(response)["data"]!.Select(epsSelector)));

        return list;
    }

    public override async Task<List<VideoServer>> GetVideoServersAsync(string episodeId)
    {
        //var test = $"{BaseUrl}/play/api?m=links&id={episodeId}";

        var response = await _http.SendHttpRequestAsync(
            //$"{BaseUrl}/api?m=links&id={episodeId}",
            episodeId,
            new Dictionary<string, string>()
            {
                { "Referer", BaseUrl }
            }
        );

        var document = new HtmlDocument();
        document.LoadHtml(HtmlEntity.DeEntitize(response));

        return document.GetElementbyId("pickDownload").SelectNodes(".//a")
            .Select(el =>
            {
                //var match = _videoServerRegex.Match(el.InnerText);
                //var matches = _videoServerRegex.Matches(el.InnerText).OfType<Match>().ToList();
                var match = _videoServerRegex.Match(el.InnerText);
                var groups = match.Groups.OfType<Group>();

                var subgroup = groups.ElementAtOrDefault(1)?.Value;
                var quality = groups.ElementAtOrDefault(2)?.Value;
                var mb = groups.ElementAtOrDefault(3)?.Value;
                var audio = groups.ElementAtOrDefault(4)?.Value;

                var audioName = !string.IsNullOrWhiteSpace(audio) ? $"{audio} " : "";

                return new VideoServer
                {
                    Name = $"{subgroup} {audioName}- {quality}p",
                    Embed = new FileUrl()
                    {
                        Url = el.Attributes["href"]!.Value,
                        Headers = new()
                        {
                            { "Referer", BaseUrl }
                        }
                    }
                };
            }).ToList();
    }

    public override IVideoExtractor? GetVideoExtractor(VideoServer server)
        => new Kwik(_http, server);
}