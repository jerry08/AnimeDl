using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AnimeDl.Scrapers;

internal class TwistScraper : BaseScraper
{
    public override string BaseUrl => "https://twist.moe";

    //string ApiUrl => "https://twist.moe/api/anime";
    private string ApiUrl => "https://api.twist.moe/api/anime";

    private string ActiveCdnUrl => "https://air-cdn.twist.moe";
    private string CdnUrl => "https://cdn.twist.moe";
    //string AccessToken => "0df14814b9e590a1f26d3071a4ed7974";

    private string AesKey => "267041df55ca2b36f2e322d05ee2c9cf";

    private List<Anime> AllAnimes = new();

    public TwistScraper(NetHttpClient netHttpClient) : base(netHttpClient)
    {
    }

    public async Task<List<Anime>> LoadAllAnimeAsync()
    {
        //var test3 = DateTime.Parse("1/24/2020 6:06:07 PM");
        //var test4 = DateTime.Parse("1/24/2020 6:06:07 PM", System.Globalization.CultureInfo.InvariantCulture);
        //
        //System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("en-GB");
        ////var test5 = DateTime.Parse("1/24/2020 6:06:07 PM");
        //var test6 = DateTime.Parse("1/5/2020 6:06:07 PM", System.Globalization.CultureInfo.InvariantCulture);

        //System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("en-GB");
        //string json = _netHttpClient.SendHttpRequestAsync(ApiUrl);
        string json = await _netHttpClient.SendHttpRequestAsync(ApiUrl, GetDefaultHeaders());
        //System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("en-GB");

        if (string.IsNullOrEmpty(json))
            return AllAnimes;

        //var list = (JArray)JsonConvert.DeserializeObject(json);
        //var list = JObject.Parse(json);
        var list = JArray.Parse(json);
        AllAnimes = list.Select(x => {
            DateTime? createdAt = null;
            DateTime? updatedAt = null;

            if (!string.IsNullOrEmpty(x["created_at"]?.ToString()))
            {
                createdAt = DateTime.Parse(x["created_at"]!.ToString(), System.Globalization.CultureInfo.InvariantCulture);
                //createdAt = DateTime.ParseExact(x["created_at"].ToString(), "yyyyMMddTHHmmssZ", System.Globalization.CultureInfo.InvariantCulture);
            }

            if (!string.IsNullOrEmpty(x["updated_at"]?.ToString()))
            {
                createdAt = DateTime.Parse(x["updated_at"]!.ToString(), System.Globalization.CultureInfo.InvariantCulture);
            }

            return new Anime()
            {
                Site = AnimeSites.TwistMoe,
                Title = x["title"]!.ToString(),
                AltTitle = x["alt_title"]!.ToString(),
                Hb_Id = !string.IsNullOrEmpty(x["hb_id"]?.ToString()) ? (int)x["hb_id"]! : 0,
                Season = !string.IsNullOrEmpty(x["season"]?.ToString()) ? (int)x["season"]! : 0,
                Ongoing = !string.IsNullOrEmpty(x["ongoing"]?.ToString()) ? (bool)x["ongoing"]! : false,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt,
                Hidden = !string.IsNullOrEmpty(x["hidden"]?.ToString()) ? (bool)x["hidden"]! : false,
                Mal_Id = !string.IsNullOrEmpty(x["mal_id"]?.ToString()) ? (int)x["mal_id"]! : 0,
                Slug = x["slug"]!["slug"]!.ToString(),
            };
        }).ToList();

        //var document = new HtmlDocument();
        //document.LoadHtml(htmlData);
        //
        //var gg = document.DocumentNode
        //    .SelectNodes(".//nav[@class='series']/ul");
        //
        //var nodes1 = document.DocumentNode
        //    .SelectNodes(".//nav[@class='series']/ul")
        //    .ToList();

        //var filtered = AllAnimes.Where(x => 
        //    Regex.IsMatch(x.Title, StringExtensions.WildCardToRegular("*Ano*"))).ToList();
        //
        //var filtered2 = AllAnimes.Where(x =>
        //    Regex.IsMatch(x.Title, StringExtensions.WildCardToRegular("*A*n*o*h*a*n*a*"))).ToList();
        //
        //var gg = LevenshteinDistance.Compute("aunt", searchQuery);
        //
        //var filtered3 = AllAnimes.Where(x => !string.IsNullOrEmpty(x.Title) && 
        //    LevenshteinDistance.Compute(x.Title, searchQuery) <= 3).ToList();

        return AllAnimes;
    }

    public override async Task<List<Anime>> SearchAsync(string searchQuery,
        SearchFilter searchFilter,
        int page)
    {
        if (AllAnimes.Count <= 0)
        {
            await LoadAllAnimeAsync();
        }

        if (searchFilter == SearchFilter.AllList)
        {
            return AllAnimes;
        }

        var lowerTxt = searchQuery.ToLower();

        return AllAnimes.Where(x => (!string.IsNullOrEmpty(x.Title) &&
            x.Title.ToLower().Contains(lowerTxt)) || 
            (!string.IsNullOrEmpty(x.AltTitle) && x.AltTitle.ToLower().Contains(lowerTxt)))
            .ToList();
    }

    public override async Task<List<Episode>> GetEpisodesAsync(Anime anime)
    {
        var episodes = new List<Episode>();

        var json = await _netHttpClient.SendHttpRequestAsync($"{ApiUrl}/{anime.Slug}", GetDefaultHeaders());
        //var json2 = _netHttpClient.SendHttpRequestAsync($"{ApiUrl}/{anime.Slug}/resources");
        var sources = await _netHttpClient.SendHttpRequestAsync($"{ApiUrl}/{anime.Slug}/sources", GetDefaultHeaders());

        if (string.IsNullOrEmpty(sources))
            return episodes;

        var decryptor = new TwistDecryptor();

        var jsonObj = JObject.Parse(json);
        var jsonSources = JArray.Parse(sources);

        anime.Summary = jsonObj["description"]!.ToString();

        episodes = jsonSources.Select(x => {
            DateTime? createdAt = null;
            DateTime? updatedAt = null;

            if (!string.IsNullOrEmpty(x["created_at"]?.ToString()))
            {
                createdAt = DateTime.Parse(x["created_at"]!.ToString(), System.Globalization.CultureInfo.InvariantCulture);
            }

            if (!string.IsNullOrEmpty(x["updated_at"]?.ToString()))
            {
                createdAt = DateTime.Parse(x["updated_at"]!.ToString(), System.Globalization.CultureInfo.InvariantCulture);
            }

            int epNum = (int)x["number"]!;
            string videoLink = (anime.Ongoing ? ActiveCdnUrl : CdnUrl) + decryptor.Decrypt(x["source"]!.ToString(), AesKey);

            return new Episode()
            {
                Id = (int)x["id"]!,
                EpisodeNumber = epNum,
                EpisodeName = $"{anime.Title} - Episode {epNum}",
                AnimeId = (int)x["anime_id"]!,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt,
                EpisodeLink = videoLink,
            };
        }).ToList();

        return episodes;
    }

    public override Task<List<Quality>> GetEpisodeLinksAsync(Episode episode)
    {
        List<Quality> list = new List<Quality>();

        list.Add(new Quality() 
        {
            IsM3U8 = episode.EpisodeLink.Contains(".m3u8"),
            QualityUrl = episode.EpisodeLink,
            Referer = BaseUrl,
        });

        return Task.FromResult(list);
    }
}