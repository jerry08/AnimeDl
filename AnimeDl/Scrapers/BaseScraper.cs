using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AnimeDl.Scrapers
{
    public enum AnimeSites
    {
        GogoAnime,
        TwistMoe,
        Zoro,
        NineAnime,
        Tenshi
    }

    //For Gogoanime
    public enum SearchType
    {
        Find,
        AllList,
        Popular,
        Ongoing,
        NewSeason,
        LastUpdated,
        Trending,
        Movies
    }

    public abstract class BaseScraper
    {
        public virtual string BaseUrl { get { return ""; } }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="searchType"></param>
        /// <param name="Page">Applies to GogoAnime only</param>
        /// <returns></returns>
        public virtual async Task<List<Anime>> SearchAsync(string searchText,
            SearchType searchType = SearchType.Find, int Page = 1)
        {
            return await Task.FromResult(new List<Anime>());
        }

        public virtual async Task<List<Episode>> GetEpisodesAsync(Anime anime)
        {
            return await Task.FromResult(new List<Episode>());
        }

        public virtual async Task<List<Quality>> GetEpisodeLinksAsync(Episode episode)
        {
            return await Task.FromResult(new List<Quality>());
        }

        public virtual async Task<List<Genre>> GetGenresAsync()
        {
            return await Task.FromResult(new List<Genre>());
        }

        public virtual WebHeaderCollection GetDefaultHeaders()
        {
            //var headers = new NameValueCollection();
            var headers = new WebHeaderCollection
            {
                { "accept-encoding", "gzip, deflate, br" }
            };

            return headers;
        }
    }
}