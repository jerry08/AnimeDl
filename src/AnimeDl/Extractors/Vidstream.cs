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
    class Vidstream
    {
        public async Task<List<Quality>> ExtractQualities(string url, bool showAllMirrorLinks)
        {
            string htmlData = await Utils.GetHtmlAsync(url);

            HtmlDocument doc2 = new HtmlDocument();
            doc2.LoadHtml(htmlData);

            var aNodes = doc2.DocumentNode
                .SelectSingleNode("//div[@class='mirror_link']")
                .SelectNodes(showAllMirrorLinks ? "//a" : ".//a");

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
