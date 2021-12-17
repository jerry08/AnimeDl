using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AnimeDl.Scrapers
{
    public class StreamAniScraper : BaseScraper
    {
        public override string BaseUrl => "https://streamani.net";

        public override async Task<List<Anime>> SearchAsync(string searchText, 
            SearchType searchType = SearchType.Find, int Page = 1)
        {
            //Test
            //await Task.Delay(5000);

            List<Anime> animes = new List<Anime>();

            string htmlData = await Utils.GetHtmlAsync($"https://animefrenzy.org/anime", GetDefaultHeaders());

            if (string.IsNullOrEmpty(htmlData))
                return animes;

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(htmlData);

            List<HtmlNode> nodes1 = document.DocumentNode.Descendants()
                        .Where(node => node.HasClass("video-block")).ToList();
            for (int i = 0; i < nodes1.Count; i++)
            {
                string img = "";
                string title = "";
                string category = "";

                HtmlNode imgNode = nodes1[i].SelectSingleNode(".//div[@class='picture']/img");
                if (imgNode != null)
                {
                    img = imgNode.Attributes["src"].Value;
                }

                //HtmlNode nameNode = currentNode.Descendants()
                //    .Where(node => node.HasClass("name")).FirstOrDefault();

                //var nameNode = currentNode.SelectSingleNode("//p[@class='name']/a[@title='Profile View' and @href");
                var nameNode = nodes1[i].SelectSingleNode(".//div[@class='name']");
                if (nameNode != null)
                {
                    title = nameNode.InnerHtml?.Trim();
                }

                animes.Add(new Anime()
                {
                    Id = i + 1,
                    Image = img,
                    Title = title,
                    Category = category,
                });
            }

            return animes;
        }
    }
}