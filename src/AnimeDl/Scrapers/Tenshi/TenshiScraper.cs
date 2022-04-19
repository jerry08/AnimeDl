using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimeDl.Scrapers
{
    public class TenshiScraper : BaseScraper
    {
        public override string BaseUrl => "https://tenshi.moe";

        private string ApiUrl => "https://api.twist.moe/api/anime";

        private string ActiveCdnUrl => "https://air-cdn.twist.moe";
        private string CdnUrl => "https://cdn.twist.moe";

        private string AesKey => "267041df55ca2b36f2e322d05ee2c9cf";

        public override async Task<List<Anime>> SearchAsync(string searchText, SearchType searchType = SearchType.Find, int Page = 1)
        {
            string json = await Http.GetHtmlAsync(BaseUrl);

            string lowerTxt = searchText.ToLower();

            return new List<Anime>();
        }

        public override async Task<List<Episode>> GetEpisodesAsync(Anime anime)
        {
            List<Episode> episodes = new List<Episode>();

            string json = await Http.GetHtmlAsync($"{ApiUrl}/{anime.Slug}", GetDefaultHeaders());
            //string json2 = await Http.GetHtmlAsync($"{ApiUrl}/{anime.Slug}/resources");
            string sources = await Http.GetHtmlAsync($"{ApiUrl}/{anime.Slug}/sources", GetDefaultHeaders());

            if (string.IsNullOrEmpty(sources))
                return episodes;

            var decryptor = new TwistDecryptor();

            var jsonObj = JObject.Parse(json);
            var jsonSources = JArray.Parse(sources);

            anime.Summary = jsonObj["description"]?.ToString();

            episodes = jsonSources.Select(x => {
                DateTime? createdAt = null;
                DateTime? updatedAt = null;

                if (!string.IsNullOrEmpty(x["created_at"]?.ToString()))
                {
                    createdAt = DateTime.Parse(x["created_at"].ToString(), System.Globalization.CultureInfo.InvariantCulture);
                }

                if (!string.IsNullOrEmpty(x["updated_at"]?.ToString()))
                {
                    createdAt = DateTime.Parse(x["updated_at"].ToString(), System.Globalization.CultureInfo.InvariantCulture);
                }

                int epNum = (int)x["number"];
                string videoLink = (anime.Ongoing ? ActiveCdnUrl : CdnUrl) + decryptor.Decrypt(x["source"].ToString(), AesKey);

                return new Episode()
                {
                    Id = (int)x["id"],
                    EpisodeNumber = epNum,
                    EpisodeName = $"{anime.Title} - Episode {epNum}",
                    AnimeId = (int)x["anime_id"],
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
                QualityUrl = episode.EpisodeLink,
                Referer = BaseUrl,
            });

            return Task.FromResult(list);
        }
    }
}
