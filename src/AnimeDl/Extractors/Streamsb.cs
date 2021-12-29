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
    class Streamsb
    {
        public async Task<List<Quality>> ExtractQualities(string url)
        {
            url = url.Replace("/e/", "/d/");
            string html = await Utils.GetHtmlAsync(url);

            var list = new List<Quality>();

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            var qualityMatch = new Regex(@"\d+x(\d+)");
            var downloadContent = new Regex(@"download_video\('(.+?)','(.+?)','(.+?)'\)");
            var nodes = doc.DocumentNode.SelectNodes(".//table/tr").ToList();
            
            for (int i = 0; i < nodes.Count; i++)
            {
                var linkNode = nodes[i].SelectSingleNode(".//a");
                if (linkNode != null)
                {
                    string link = linkNode.Attributes["onclick"]?.Value;

                    Match match = qualityMatch.Match(html);
                    int quality = Convert.ToInt32(match.Groups[1].Value);
                    
                    var groups = downloadContent.Match(link).Groups;
                    string contentId = groups[1].Value;
                    string mode = groups[2].Value;
                    string contentHash = groups[3].Value;

                    string downloadUrl = $"https://sbplay1.com/dl?op=download_orig&id={contentId}&mode={mode}&hash={contentHash}";

                    var headers = new WebHeaderCollection
                    {
                        { "Referer", url }
                    };

                    string html2 = await Utils.GetHtmlAsync(downloadUrl, headers);

                    HtmlDocument doc2 = new HtmlDocument();
                    doc2.LoadHtml(html2);

                    var directDownloadNodes = doc2.DocumentNode.SelectNodes(".//a")
                        .Where(x => x.Attributes != null && x.Attributes["href"] != null &&
                        x.Attributes["href"].Value.Contains(".mp4")).ToList();

                    for (int j = 0; j < directDownloadNodes.Count; j++)
                    {
                        list.Add(new Quality()
                        {
                            Resolution = quality.ToString(),
                            QualityUrl = directDownloadNodes[j].Attributes["href"].Value,
                            Referer = url
                        });
                    }
                }
            }

            return list;
        }
    }
}
