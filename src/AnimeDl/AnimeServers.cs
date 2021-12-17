using AnimeDl.Scrapers;

namespace AnimeDl
{
    public class AnimeServers
    {
        //public const string Url1 = "https://gogo-stream.com";
        //changed to
        public const string Url1 = "https://streamani.net";

        public const string Url2 = "https://vidstreaming.io";

        public const string Url3 = "https://gogoanime.pe";

        public const string Url4 = "https://animesa.ga";

        public const string Url5 = "https://animefrenzy.org";

        public const string Url6 = "https://animetake.tv/";

        public const string Url7 = "https://www12.9anime.to/";

        #region Memo
        public const string Ajax1 = "https://ajax.gogocdn.net/ajax/load-list-episode";
        public const string Ajax2 = "https://ajax.apimovie.xyz/ajax/load-list-episode";
        
        //https://ajax.gogocdn.net/ajax/page-recent-release-ongoing.html?page=6
        //https://ajax.gogocdn.net/ajax/page-recent-release.html
        //https://ajax.gogocdn.net/ajax/page-recent-release.html?page=1

        //Example ajax
        //https://ajax.apimovie.xyz/ajax/load-list-episode?ep_start=500&ep_end=500&id=133&default_ep=0&alias=naruto-shippuden
        const string Ajax3 = "https://ajax.apimovie.xyz/ajax/load-list-episode?ep_start=1&ep_end=5&id=anohana";
        #endregion
    }
}
