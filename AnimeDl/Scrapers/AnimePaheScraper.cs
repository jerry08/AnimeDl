using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AnimeDl.Extractors.Interfaces;
using AnimeDl.Models;
using AnimeDl.Utils.Extensions;
using Newtonsoft.Json.Linq;

namespace AnimeDl.Scrapers;

public class AnimePaheScraper : BaseScraper
{
    public override string Name { get; set; } = "AnimePahe";

    public override bool IsDubAvailableSeparately { get; set; } = false;

    public override string BaseUrl => "https://animepahe.com";

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

        var response = await _http.SendHttpRequestAsync($"{BaseUrl}/api?m=search&q={Uri.EscapeUriString(query)}");

        if (string.IsNullOrEmpty(response))
            return animes;

        var data = JObject.Parse(response)["data"]!.Select(x => new Anime()
        {
            Id = x["session"]!.ToString(),
            Title = x["title"]!.ToString(),
            Type = x["type"]!.ToString(),
            Episodes = Convert.ToInt32(x["episodes"]),
            Status = x["status"]!.ToString(),
            Season = x["season"]!.ToString(),
            Year = Convert.ToInt32(x["year"]),
            Score = Convert.ToInt32(x["score"]),
            Image = x["poster"]!.ToString()
        });

        animes.AddRange(data);

        return animes;
    }

    public override async Task<Anime> GetAnimeInfoAsync(string id)
    {
        return new();
    }

    public override async Task<List<Episode>> GetEpisodesAsync(string id)
    {
        var episodes = new List<Episode>();

        var response = await _http.SendHttpRequestAsync($"{BaseUrl}/api?m=release&id={id}&sort=episode_asc&page=1");

        var data = JObject.Parse(response);

        var lastPage = Convert.ToInt32(data["last_page"]);

        // Start at index of 2 since we've already gotten the first page above
        for (int i = 2; i <= lastPage; i++)
        {
            response = await _http.SendHttpRequestAsync($"{BaseUrl}/api?m=release&id={id}&sort=episode_asc&page={i}");

            Console.WriteLine(i.ToString());
        }

        return episodes;
    }

    public override async Task<List<VideoServer>> GetVideoServersAsync(string episodeId)
        => await Task.FromResult(new List<VideoServer>() { new("Kwik", new(episodeId)) });

    public override async Task<List<Video>> GetVideosAsync(VideoServer server)
    {
        var list = new List<Video>();

        return await base.GetVideosAsync(server);
    }

    public override IVideoExtractor? GetVideoExtractor(VideoServer server)
    {
        throw new NotImplementedException();
    }
}