using AnimeDl.Extractors;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AnimeDl.Scrapers
{
    public class GogoAnimeScraper : BaseScraper
    {
        //public override string BaseUrl => "https://gogoanime.pe";
        //public override string BaseUrl => "https://www1.gogoanime.cm/";
        public override string BaseUrl => "https://gogoanime.sk/";

        public string CdnUrl => "https://ajax.gogocdn.net/ajax/load-list-episode?ep_start=0&ep_end=10000&id=";

        public override async Task<List<Anime>> SearchAsync(string searchText, 
            SearchType searchType = SearchType.Find, int Page = 1)
        {
            List<Anime> animes = new List<Anime>();

            string htmlData = "";

            switch (searchType)
            {
                case SearchType.Find:
                    htmlData = await Http.GetHtmlAsync($"{BaseUrl}//search.html?keyword=" + searchText);
                    break;
                case SearchType.Popular:
                    htmlData = await Http.GetHtmlAsync($"{BaseUrl}/popular.html?page=" + Page);
                    break;
                case SearchType.NewSeason:
                    htmlData = await Http.GetHtmlAsync($"{BaseUrl}/new-season.html?page=" + Page);
                    break;
                case SearchType.LastUpdated:
                    htmlData = await Http.GetHtmlAsync($"{BaseUrl}/?page=" + Page);
                    break;
                case SearchType.Trending:
                    break;
                case SearchType.AllList:
                    //htmlData = await Http.GetHtmlAsync($"https://animesa.ga/animel.php");
                    htmlData = await Http.GetHtmlAsync($"https://animefrenzy.org/anime");
                    break;
                default:
                    break;
            }

            if (string.IsNullOrEmpty(htmlData))
                return animes;

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(htmlData);

            HtmlNode itemsNode = document.DocumentNode.Descendants()
                .Where(node => node.HasClass("items")).FirstOrDefault();

            if (itemsNode != null)
            {
                List<HtmlNode> nodes3 = itemsNode.Descendants()
                    .Where(node => node.Name == "li").ToList();
                for (int i = 0; i < nodes3.Count; i++)
                {
                    //var currentNode = HtmlNode.CreateNode(nodes[i].OuterHtml);

                    string img = "";
                    string title = "";
                    string category = "";
                    string released = "";
                    string link = "";

                    //HtmlNode imgNode = currentNode.Descendants()
                    //    .Where(node => node.Name == "img").FirstOrDefault();
                    //if (imgNode != null)
                    //{
                    //    var attrImg = imgNode.Attributes.Where(x => x.Name == "src")
                    //        .FirstOrDefault();
                    //    if (attrImg != null)
                    //        img = attrImg.Value;
                    //}

                    HtmlNode imgNode = nodes3[i].SelectSingleNode(".//div[@class='img']/a/img");
                    if (imgNode != null)
                    {
                        img = imgNode.Attributes["src"].Value;
                    }

                    //HtmlNode nameNode = currentNode.Descendants()
                    //    .Where(node => node.HasClass("name")).FirstOrDefault();

                    //var nameNode = currentNode.SelectSingleNode("//p[@class='name']/a[@title='Profile View' and @href");
                    var nameNode = nodes3[i].SelectSingleNode(".//p[@class='name']/a");
                    if (nameNode != null)
                    {
                        category = nameNode.Attributes["href"].Value;
                        title = nameNode.Attributes["title"].Value; //OR name = nameNode.InnerText;
                    }

                    HtmlNode releasedNode = nodes3[i].SelectSingleNode(".//p[@class='released']");
                    if (releasedNode != null)
                    {
                        released = new string(releasedNode.InnerText.Where(char.IsDigit).ToArray());
                    }

                    if (category.Contains("-episode"))
                    {
                        //category = category.Remove(category.LastIndexOf('\\'));
                        category = "/category" + category.Remove(category.LastIndexOf("-episode"));
                    }

                    link = BaseUrl + category;

                    animes.Add(new Anime()
                    {
                        Id = i + 1,
                        Image = img,
                        Title = title,
                        EpisodesNum = 0,
                        Category = category,
                        //Link = "https://animesa.ga/watchanime.html?q=/videos/" + name.RemoveBadChars1(),
                        Released = released,
                        Link = link,
                    });
                }
            }

            return animes;
        }

        public override async Task<List<Episode>> GetEpisodesAsync(Anime anime)
        {
            List<Episode> episodes = new List<Episode>();

            string htmlData = await Http.GetHtmlAsync($"{BaseUrl}" + anime.Category);

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(htmlData);

            string imageLink = "";
            string summary = "";
            string genre = "";
            string type = "";
            string released = "";
            string othernames = "";
            string status = "";

            HtmlNode imgNode = document.DocumentNode.SelectSingleNode(".//div[@class='anime_info_body_bg']/img");
            if (imgNode != null)
            {
                imageLink = imgNode.Attributes["src"].Value;
            }

            List<HtmlNode> animeInfoNodes = document.DocumentNode
                .SelectNodes(".//div[@class='anime_info_body_bg']/p").ToList();

            for (int i = 0; i < animeInfoNodes.Count; i++)
            {
                switch (i)
                {
                    case 0: //Bookmarks
                        break;
                    case 1: //Type (e.g TV Series)
                        type = animeInfoNodes[i].InnerText;
                        type = Regex.Replace(type, @"\t|\n|\r", "");
                        type = new Regex("[ ]{2,}", RegexOptions.None).Replace(type, " ").Trim();
                        break;
                    case 2: //Plot SUmmary
                        summary = animeInfoNodes[i].InnerText.Trim();
                        break;
                    case 3: //Genre
                        genre = animeInfoNodes[i].InnerText.Replace("Genre:", "").Trim();
                        break;
                    case 4: //Released Year
                        released = animeInfoNodes[i].InnerText;
                        released = Regex.Replace(released, @"\t|\n|\r", "");
                        released = new Regex("[ ]{2,}", RegexOptions.None).Replace(released, " ").Trim();
                        break;
                    case 5: //Status
                        status = animeInfoNodes[i].InnerText;
                        status = Regex.Replace(status, @"\t|\n|\r", "");
                        status = new Regex("[ ]{2,}", RegexOptions.None).Replace(status, " ").Trim();
                        break;
                    case 6: //Other Name
                        othernames = animeInfoNodes[i].InnerText;
                        othernames = Regex.Replace(othernames, @"\t|\n|\r", "");
                        othernames = new Regex("[ ]{2,}", RegexOptions.None).Replace(othernames, " ").Trim();
                        break;
                    default:
                        break;
                }
            }

            anime.Type = type.Trim();
            anime.Genre = genre;
            anime.Summary = summary;
            anime.Released = released;
            anime.Status = status.Trim();
            anime.OtherNames = othernames;

            var movieId = document.DocumentNode.Descendants().Where(x => x.Id == "movie_id")
                .FirstOrDefault().Attributes["value"].Value;

            //https://ajax.gogocdn.net/ajax/load-list-episode?ep_start=500&ep_end=500&id=133&default_ep=0&alias=naruto-shippuden

            //string sf = $"https://vidstream.pro/info/{movieId}?domain=gogoanime.be&skey=db04c5540929bebd456b9b16643fc436";

            string url = CdnUrl + movieId;

            htmlData = await Http.GetHtmlAsync(url);

            document = new HtmlDocument();
            document.LoadHtml(htmlData);

            List<HtmlNode> liNodes = document.DocumentNode.Descendants()
                .Where(node => node.Name == "li")
                .ToList();

            for (int i = 0; i < liNodes.Count; i++)
            {
                string name = "";
                string link = "";
                string subOrDub = "";

                HtmlNode hrefNode = liNodes[i].SelectSingleNode(".//a");
                if (hrefNode != null)
                {
                    link = hrefNode.Attributes["href"].Value.Trim();
                }

                var nameNode = liNodes[i].SelectSingleNode(".//div[@class='name']");
                if (nameNode != null)
                {
                    name = nameNode.InnerText;
                }

                var subDubNode = liNodes[i].SelectSingleNode(".//div[@class='cate']");
                if (subDubNode != null)
                {
                    //subOrDub = subDubNode.Attributes["title"].Value; //OR name = nameNode.InnerText;
                    subOrDub = subDubNode.InnerText;
                }

                link = BaseUrl + link;
                int epNumber = 1;

                try
                {
                    epNumber = Convert.ToInt32(link.Split(new char[] { '-' }).LastOrDefault());
                }
                catch (Exception e)
                {
                    continue;
                }

                //string episodeLinkSaga = "https://animesa.ga/watchanime.html?q=/videos" + anime.Category.Replace("/category", "") + "-episode-" + epNumber.ToString();

                episodes.Add(new Episode()
                {
                    EpisodeLink = link,
                    EpisodeNumber = epNumber,
                    EpisodeName = anime.Title + $" - Episode {epNumber}",
                    //EpisodeLinkSaga = episodeLinkSaga
                });
            }

            return episodes;
        }

        public override async Task<List<Quality>> GetEpisodeLinksAsync(Episode episode)
        {
            //string link = anime.Link.Replace("/category", "") + "-episode-" + episode.EpisodeNumber;
            string link = episode.EpisodeLink;

            string htmlData = await Http.GetHtmlAsync(link);

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(htmlData);

            //Exception for fire force season 2 episode 1
            if (htmlData.Contains(@">404</h1>"))
            {
                htmlData = await Http.GetHtmlAsync(link + "-1");
            }

            var vidStreamNode = doc.DocumentNode
                .SelectSingleNode(".//div[@class='play-video']/iframe");
            if (vidStreamNode != null)
            {
                string vidStreamUrl = "https:" + vidStreamNode.Attributes["src"].Value;
                //string vidCdnUrl = vidStreamUrl.Replace("streaming.php", "loadserver.php");
                //string vidCdnUrl = vidStreamUrl.Replace("streaming.php", "download");
                string vidCdnUrl = vidStreamUrl;

                return await new GogoCDN().ExtractQualities(vidCdnUrl);
            }

            return new List<Quality>();
        }

        public override async Task<List<Genre>> GetGenresAsync()
        {
            List<Genre> genres = new List<Genre>();

            string htmlData = await Http.GetHtmlAsync(BaseUrl);

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(htmlData);

            //string nav = "menu_series genre right";

            HtmlNode genresNode = document.DocumentNode.Descendants()
                .Where(node => node.GetClasses().Contains("genre"))
                .FirstOrDefault();

            if (genresNode != null)
            {
                List<HtmlNode> nodes = genresNode.Descendants()
                    .Where(node => node.Name == "li").ToList();
                for (int i = 0; i < nodes.Count; i++)
                {
                    //var currentNode = HtmlNode.CreateNode(nodes[i].OuterHtml);

                    string name = "";
                    string link = "";

                    //HtmlNode nameNode = currentNode.Descendants()
                    //    .Where(node => node.HasClass("name")).FirstOrDefault();

                    //var nameNode = currentNode.SelectSingleNode("//p[@class='name']/a[@title='Profile View' and @href");
                    var nameNode = nodes[i].SelectSingleNode(".//a");
                    if (nameNode != null)
                    {
                        link = nameNode.Attributes["href"].Value;
                        name = nameNode.Attributes["title"].Value; //OR name = nameNode.InnerText;
                    }

                    genres.Add(new Genre()
                    {
                        GenreName = name,
                        GenreLink = link
                    });
                }
            }

            return genres;
        }
    }
}