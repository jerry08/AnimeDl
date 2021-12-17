using AnimeDl.Scrapers.Events;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AnimeDl.Scrapers
{
    public class AnimeTakeScraper : BaseScraper
    {
        public override string BaseUrl => "https://animetake.tv/";
        public string BaseUrl2 => "https://www12.9anime.to/";

        #region AnimeTake
        AnimeEventArgs GetFromAnimeTake(SearchType searchType, string searchTxt)
        {
            AnimeEventArgs animeEventArgs = new AnimeEventArgs();

            switch (searchType)
            {
                case SearchType.Find:
                    animeEventArgs = AnimeTakeFind(searchTxt);
                    break;
                case SearchType.AllList:
                    break;
                case SearchType.Popular:
                    animeEventArgs = AnimeTakePopular();
                    break;
                case SearchType.NewSeason:
                    break;
                case SearchType.LastUpdated:
                    animeEventArgs = AnimeTakeLastUpdated();
                    break;
                case SearchType.Trending:
                    break;
                case SearchType.Ongoing:
                    animeEventArgs = AnimeTakeOngoing();
                    break;
                case SearchType.Movies:
                    animeEventArgs = AnimeTakeMovies();
                    break;
                default:
                    break;
            }

            return animeEventArgs;
        }
        AnimeEventArgs AnimeTakeFind(string searchTxt)
        {
            AnimeEventArgs animeEventArgs = new AnimeEventArgs();

            string htmlData = Utils.GetHtml(BaseUrl + $"search/?key={searchTxt}");

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(htmlData);

            var animeNodes = document.DocumentNode.Descendants()
                .Where(node => node.HasClass("nopadding"))
                .ToList();

            for (int i = 0; i < animeNodes.Count; i++)
            {
                string img = "";
                string title = "";
                string category = "";

                HtmlNode nameNode = animeNodes[i].Descendants().Where(x => x.HasClass("latestep_title"))
                    .FirstOrDefault();
                if (nameNode != null)
                {
                    //title = nameNode.FirstChild.InnerText;
                    title = nameNode.FirstChild.GetDirectInnerText();
                }

                HtmlNode hrefNode = animeNodes[i].Descendants().Where(x => x.Name == "a")
                    .FirstOrDefault();
                if (hrefNode != null)
                {
                    category = hrefNode.Attributes["href"].Value;
                }

                HtmlNode imgNode = animeNodes[i].Descendants().Where(x => x.Name == "img")
                    .FirstOrDefault();
                if (imgNode != null)
                {
                    //img = imgNode.Attributes["src"].Value;
                    img = imgNode.Attributes["data-src"].Value;
                }

                animeEventArgs.Animes.Add(new Anime()
                {
                    Id = i + 1,
                    Image = img,
                    Title = title,
                    EpisodesNum = 0,
                    Category = category
                });
            }

            //var animeNodes2 = document.DocumentNode.Descendants()
            //    .Where(node => node.HasClass("popular"))
            //    .ToList();

            return animeEventArgs;
        }

        AnimeEventArgs AnimeTakePopular()
        {
            AnimeEventArgs animeEventArgs = new AnimeEventArgs();

            string htmlData = Utils.GetHtml(BaseUrl + $"animelist/popular");

            htmlData = Utils.GetHtml(BaseUrl2 + $"search?keyword=Naruto");
            htmlData = Utils.GetHtml("https://www12.9anime.to/watch/naruto.xx8z");

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(htmlData);

            var animeNodes = document.DocumentNode.Descendants()
                .Where(node => node.HasClass("nopadding"))
                .ToList();

            for (int i = 0; i < animeNodes.Count; i++)
            {
                string img = "";
                string title = "";
                string category = "";

                HtmlNode nameNode = animeNodes[i].Descendants().Where(x => x.HasClass("latestep_title"))
                    .FirstOrDefault();
                if (nameNode != null)
                {
                    //title = nameNode.FirstChild.InnerText;
                    title = nameNode.FirstChild.GetDirectInnerText();
                }

                HtmlNode hrefNode = animeNodes[i].Descendants().Where(x => x.Name == "a")
                    .FirstOrDefault();
                if (hrefNode != null)
                {
                    category = hrefNode.Attributes["href"].Value;
                }

                HtmlNode imgNode = animeNodes[i].Descendants().Where(x => x.Name == "img")
                    .FirstOrDefault();
                if (imgNode != null)
                {
                    //img = imgNode.Attributes["src"].Value;
                    img = "https://animetake.tv" + imgNode.Attributes["data-src"].Value;
                }

                animeEventArgs.Animes.Add(new Anime()
                {
                    Id = i + 1,
                    Image = img,
                    Title = title,
                    EpisodesNum = 0,
                    Category = category
                });
            }

            //var animeNodes2 = document.DocumentNode.Descendants()
            //    .Where(node => node.HasClass("popular"))
            //    .ToList();

            return animeEventArgs;
        }
        AnimeEventArgs AnimeTakeLastUpdated()
        {
            AnimeEventArgs animeEventArgs = new AnimeEventArgs();

            string htmlData = Utils.GetHtml(BaseUrl);

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(htmlData);

            var animeNodes = document.DocumentNode.Descendants()
                .Where(node => node.HasClass("latestep_wrapper"))
                .ToList();

            for (int i = 0; i < animeNodes.Count; i++)
            {
                string img = "";
                string title = "";
                string category = "";

                HtmlNode aNode = animeNodes[i].Descendants().Where(x => x.HasClass("latest-parent"))
                    .FirstOrDefault();
                if (aNode != null)
                {
                    category = aNode.Attributes["href"].Value;
                    title = aNode.FirstChild.GetDirectInnerText();
                }

                HtmlNode imgNode = animeNodes[i].Descendants().Where(x => x.Name == "img")
                    .FirstOrDefault();
                if (imgNode != null)
                {
                    //img = imgNode.Attributes["src"].Value;
                    img = "https://animetake.tv" + imgNode.Attributes["data-src"].Value;
                }

                animeEventArgs.Animes.Add(new Anime()
                {
                    Id = i + 1,
                    Image = img,
                    Title = title,
                    EpisodesNum = 0,
                    Category = category
                });
            }

            //var animeNodes2 = document.DocumentNode.Descendants()
            //    .Where(node => node.HasClass("popular"))
            //    .ToList();

            return animeEventArgs;
        }
        AnimeEventArgs AnimeTakeUpComing()
        {
            AnimeEventArgs animeEventArgs = new AnimeEventArgs();

            string htmlData = Utils.GetHtml(BaseUrl);

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(htmlData);

            var animeListNode = document.DocumentNode.Descendants()
                .Where(node => node.HasClass("latest-serie-sidebar"))
                .FirstOrDefault();

            if (animeListNode != null)
            {
                var animeNodes = animeListNode.Descendants().Where(x => x.HasClass("row"))
                    .ToList();

                for (int i = 0; i < animeNodes.Count; i++)
                {
                    string img = "";
                    string title = "";
                    string category = "";

                    HtmlNode aNode = animeNodes[i].Descendants().Where(x => x.Name == "a")
                        .LastOrDefault();
                    if (aNode != null)
                    {
                        category = aNode.Attributes["href"].Value;
                        title = aNode.InnerText.Trim();
                    }

                    HtmlNode imgNode = animeNodes[i].Descendants().Where(x => x.Name == "img")
                        .FirstOrDefault();
                    if (imgNode != null)
                    {
                        //img = imgNode.Attributes["src"].Value;
                        img = "https://animetake.tv" + imgNode.Attributes["data-src"].Value;
                    }

                    animeEventArgs.Animes.Add(new Anime()
                    {
                        Id = i + 1,
                        Image = img,
                        Title = title,
                        EpisodesNum = 0,
                        Category = category
                    });
                }
            }

            //var animeNodes2 = document.DocumentNode.Descendants()
            //    .Where(node => node.HasClass("popular"))
            //    .ToList();

            return animeEventArgs;
        }
        AnimeEventArgs AnimeTakeOngoing()
        {
            AnimeEventArgs animeEventArgs = new AnimeEventArgs();

            string htmlData = Utils.GetHtml(BaseUrl);

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(htmlData);

            var animeListNode = document.DocumentNode.Descendants()
                .Where(node => node.HasClass("latest-serie-sidebar"))
                .LastOrDefault();

            if (animeListNode != null)
            {
                var animeNodes = animeListNode.Descendants().Where(x => x.HasClass("row"))
                    .ToList();

                for (int i = 0; i < animeNodes.Count; i++)
                {
                    string img = "";
                    string title = "";
                    string category = "";

                    HtmlNode aNode = animeNodes[i].Descendants().Where(x => x.Name == "a")
                        .LastOrDefault();
                    if (aNode != null)
                    {
                        category = aNode.Attributes["href"].Value;
                        title = aNode.InnerText.Trim();
                    }

                    HtmlNode imgNode = animeNodes[i].Descendants().Where(x => x.Name == "img")
                        .FirstOrDefault();
                    if (imgNode != null)
                    {
                        //img = imgNode.Attributes["src"].Value;
                        img = "https://animetake.tv" + imgNode.Attributes["data-src"].Value;
                    }

                    animeEventArgs.Animes.Add(new Anime()
                    {
                        Id = i + 1,
                        Image = img,
                        Title = title,
                        EpisodesNum = 0,
                        Category = category
                    });
                }
            }

            //var animeNodes2 = document.DocumentNode.Descendants()
            //    .Where(node => node.HasClass("popular"))
            //    .ToList();

            return animeEventArgs;
        }
        AnimeEventArgs AnimeTakeMovies()
        {
            AnimeEventArgs animeEventArgs = new AnimeEventArgs();

            string htmlData = Utils.GetHtml(BaseUrl + "animelist/movies");

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(htmlData);

            var animeNodes = document.DocumentNode.Descendants()
                .Where(node => node.HasClass("animelist_poster"))
                .ToList();

            for (int i = 0; i < animeNodes.Count; i++)
            {
                string img = "";
                string title = "";
                string category = "";

                HtmlNode lastNode = animeNodes[i].Descendants()
                    .Where(node => node.HasClass("col-xs-12"))
                    .LastOrDefault();

                HtmlNode aNode = animeNodes[i].Descendants().Where(x => x.Name == "a")
                    .LastOrDefault();
                if (aNode != null)
                {
                    category = aNode.Attributes["href"].Value;
                    title = aNode.Attributes["title"].Value;
                }

                HtmlNode imgNode = animeNodes[i].Descendants().Where(x => x.Name == "img")
                    .FirstOrDefault();
                if (imgNode != null)
                {
                    //img = imgNode.Attributes["src"].Value;
                    img = "https://animetake.tv" + imgNode.Attributes["data-src"].Value;
                }

                animeEventArgs.Animes.Add(new Anime()
                {
                    Id = i + 1,
                    Image = img,
                    Title = title,
                    EpisodesNum = 0,
                    Category = category
                });
            }

            //var animeNodes2 = document.DocumentNode.Descendants()
            //    .Where(node => node.HasClass("popular"))
            //    .ToList();

            return animeEventArgs;
        }
        #endregion
    }
}