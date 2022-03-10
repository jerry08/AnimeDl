using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;

namespace AnimeDl.Extractors
{
    class Vidstream : BaseExtractor
    {
        bool ShowAllMirrorLinks;

        public Vidstream(bool showAllMirrorLinks)
        {
            ShowAllMirrorLinks = showAllMirrorLinks;
        }

        public override async Task<List<Quality>> ExtractQualities(string url)
        {
            string htmlData = await Utils.GetHtmlAsync(url);

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(htmlData);

            var aNodes = doc.DocumentNode
                .SelectSingleNode("//div[@class='mirror_link']")
                .SelectNodes(ShowAllMirrorLinks ? "//a" : ".//a");

            var list = new List<Quality>();

            for (int i = 0; i < aNodes.Count; i++)
            {
                list.Add(new Quality()
                {
                    Referer = url,
                    Resolution = aNodes[i].InnerText.Replace("Download", "").Trim(),
                    QualityUrl = aNodes[i].Attributes["href"].Value
                });
            }

            return list;
        }
    }
}
