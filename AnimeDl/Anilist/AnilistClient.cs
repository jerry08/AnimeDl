using AnimeDl.Utils;
using AnimeDl.Utils.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AnimeDl.Anilist.Api;

namespace AnimeDl.Anilist;

public class AnilistClient
{
    private readonly HttpClient _http;

    public bool IsAdult { get; set; } = false;

    public bool ListOnly { get; set; } = false;

    public AnilistClient(HttpClient httpClient)
    {
        _http = httpClient;
    }

    public AnilistClient() : this(Http.Client)
    {
    }

    public async ValueTask<T?> ExecuteQueryAsync<T>(string query,
        string variables = "")
    {
        var data = new
        {
            query,
            variables
        };

        var headers = new NameValueCollection()
        {
            { "Content-Type", "application/json" },
            { "Accept", "application/json" }
        };

        var serialized = JsonConvert.SerializeObject(data);

        //var content = new StringContent(query, Encoding.UTF8, "application/json");
        var content = new StringContent(serialized, Encoding.UTF8, "application/json");

        var json = await _http.PostAsync("https://graphql.anilist.co/", headers, content);
        if (!json.StartsWith("{"))
            throw new Exception("Seems like Anilist is down, maybe try using a VPN or you can wait for it to comeback.");

        return JsonConvert.DeserializeObject<T>(json);
    }

    public async Task<SearchResults?> SearchAsync(
        string type,
        int? page = null,
        int? perPage = null,
        string? search = null,
        string? sort = null,
        string[]? genres = null,
        string[]? tags = null,
        string? format = null,
        bool isAdult = false,
        bool? onList = null,
        int? id = null,
        bool hd = false)
    {
        var query = @"query ($page: Int = 1, $id: Int, $type: MediaType, $isAdult: Boolean = false, $search: String, $format: [MediaFormat], $status: MediaStatus, $countryOfOrigin: CountryCode, $source: MediaSource, $season: MediaSeason, $seasonYear: Int, $year: String, $onList: Boolean, $yearLesser: FuzzyDateInt, $yearGreater: FuzzyDateInt, $episodeLesser: Int, $episodeGreater: Int, $durationLesser: Int, $durationGreater: Int, $chapterLesser: Int, $chapterGreater: Int, $volumeLesser: Int, $volumeGreater: Int, $licensedBy: [String], $isLicensed: Boolean, $genres: [String], $excludedGenres: [String], $tags: [String], $excludedTags: [String], $minimumTagRank: Int, $sort: [MediaSort] = [POPULARITY_DESC, SCORE_DESC]) {
              Page(page: $page, perPage: " + $"{perPage ?? 50}" + @") {
                pageInfo {
                  total
                  perPage
                  currentPage
                  lastPage
                  hasNextPage
                }
                media(id: $id, type: $type, season: $season, format_in: $format, status: $status, countryOfOrigin: $countryOfOrigin, source: $source, search: $search, onList: $onList, seasonYear: $seasonYear, startDate_like: $year, startDate_lesser: $yearLesser, startDate_greater: $yearGreater, episodes_lesser: $episodeLesser, episodes_greater: $episodeGreater, duration_lesser: $durationLesser, duration_greater: $durationGreater, chapters_lesser: $chapterLesser, chapters_greater: $chapterGreater, volumes_lesser: $volumeLesser, volumes_greater: $volumeGreater, licensedBy_in: $licensedBy, isLicensed: $isLicensed, genre_in: $genres, genre_not_in: $excludedGenres, tag_in: $tags, tag_not_in: $excludedTags, minimumTagRank: $minimumTagRank, sort: $sort, isAdult: $isAdult) {
                  id
                  idMal
                  isAdult
                  status
                  chapters
                  episodes
                  nextAiringEpisode {
                    episode
                  }
                  type
                  genres
                  meanScore
                  isFavourite
                  bannerImage
                  coverImage {
                    large
                    extraLarge
                  }
                  title {
                    english
                    romaji
                    userPreferred
                  }
                  mediaListEntry {
                    progress
                    private
                    score(format: POINT_100)
                    status
                  }
                }
              }
            }".Replace("\n", " ").Replace(@"""  """, "");

        /*query = @"query ($id: Int) { # Define which variables will be used in the query (id)
              Media (id: $id, type: ANIME) { # Insert our variables into the query arguments (id) (type: ANIME is hard-coded in the query)
                id
                title {
                  romaji
                  english
                  native
                }
              }
            }";*/

        //var variables = @"{""id"": 1}";
        //variables = @"{""id"": 1, ""perPage"": 50}";

        var variables = $@"""type"": ""{type}"", ""isAdult"": {isAdult.ToString()!.ToLower()}";
        variables += onList is not null ? @$",""onList"":{onList.ToString()!.ToLower()}" : "";
        variables += page is not null ? @$",""page"":{page}" : "";
        variables += id is not null ? @$",""id"":{id}" : "";
        variables += search is not null ? @$",""search"":""{search}""" : "";
        variables += format is not null ? @$",""format"":{format}" : "";
        variables += genres is not null && genres!.Length > 0 ? @$",""genres"":{genres[0]}" : "";
        variables += tags is not null && tags!.Length > 0 ? @$",""tags"":{tags[0]}" : "";

        variables = "{" + variables.Replace("\n", " ").Replace(@"""  """, "") + "}";
        
        var response = (await ExecuteQueryAsync<Query.Page>(query, variables))?.Data?.Page;
        var responseArray = new List<Media>();

        if (response is null || response.Media is null)
            return null;

        response.Media.ForEach(x =>
        {
            var userStatus = x.MediaListEntry?.Status.ToString();
            var genresArr = new List<string>();
            if (x.Genres is not null)
            {

            }
        });

        var pageInfo = response?.PageInfo;

        return new SearchResults()
        {
            Type = type,
            PerPage = perPage,
            Search = search,
            Sort = sort,
            IsAdult = isAdult,
            OnList = onList,
            //Genres = genres,
            //Tags = tags,
            Format = format,
            //Results = responseArray,
            Results = response!.Media,
            Page =  pageInfo?.CurrentPage ?? 0,
            HasNextPage = pageInfo?.HasNextPage == true,
        };
    }

    public async ValueTask<List<Media>> GetRecentlyUpdatedAsync()
    {
        var query = @"{
            Page(page:1,perPage:50) {
                pageInfo {
                    hasNextPage
                    total
                }
                airingSchedules(
                    airingAt_greater: 0
                    airingAt_lesser: ${airingAt_lesser}
                    sort:TIME_DESC
                ) {
                    media {
                        id
                        idMal
                        status
                        chapters
                        episodes
                        nextAiringEpisode { episode }
                        isAdult
                        type
                        meanScore
                        isFavourite
                        bannerImage
                        countryOfOrigin
                        coverImage { large }
                        title {
                            english
                            romaji
                            userPreferred
                        }
                        mediaListEntry {
                            progress
                            private
                            score(format: POINT_100)
                            status
                        }
                    }
                }
            }
        }".Replace("${airingAt_lesser}", $"{DateTime.UtcNow.ToUnixTimeMilliseconds() / 1000 - 10000}")
        .Replace("\n", " ");

        var response = (await ExecuteQueryAsync<Query.Page>(query))?.Data?.Page?.AiringSchedules;

        var responseArray = new List<Media>();
        var idArr = new List<int>();

        response?.ForEach(x =>
        {
            if (x.Media is not null && !idArr.Contains(x.Media.Id))
            {
                if (!ListOnly && (x.Media.CountryOfOrigin == "JP" &&
                    (x.Media.IsAdult == IsAdult || (ListOnly && x.Media.MediaListEntry is not null))))
                {
                    idArr.Add(x.Media.Id);
                    responseArray.Add(x.Media);
                }
            }
        });

        return responseArray;
    }
}