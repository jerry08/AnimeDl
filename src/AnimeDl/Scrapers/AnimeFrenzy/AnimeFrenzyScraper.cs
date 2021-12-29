using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AnimeDl.Scrapers
{
    public class AnimeFrenzyScraper : BaseScraper
    {
        public override string BaseUrl => "https://animefrenzy.org";

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

            HtmlNode accordionNode = document.DocumentNode.Descendants()
                        .Where(node => node.HasClass("listbox")).FirstOrDefault();

            if (accordionNode != null)
            {
                List<HtmlNode> nodes3 = accordionNode.Descendants()
                    .Where(node => node.Name == "a").ToList();
                for (int i = 0; i < nodes3.Count; i++)
                {
                    string title = "";
                    string category = "";

                    //var nameNode = currentNode.SelectSingleNode("//p[@class='name']/a[@title='Profile View' and @href");
                    var nameNode = nodes3[i].SelectSingleNode(".//a");
                    if (nameNode != null)
                    {
                        category = nameNode.Attributes["href"].Value;
                        title = nameNode.SelectSingleNode(".//div").Attributes["title"].Value; //OR name = nameNode.InnerText;
                    }

                    if (category.Contains("-episode"))
                    {
                        //category = category.Remove(category.LastIndexOf('\\'));
                        category = "/category" + category.Remove(category.LastIndexOf("-episode"));
                    }

                    animes.Add(new Anime()
                    {
                        Id = i + 1,
                        Title = title,
                        EpisodesNum = 0,
                        Category = category,
                        //Link = "https://animesa.ga/watchanime.html?q=/videos/" + name.RemoveBadChars1(),
                    });
                }
            }

            return animes;
        }
    }
}