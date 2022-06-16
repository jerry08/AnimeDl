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
using AnimeDl.Scrapers;

namespace AnimeDl.Extractors
{
    internal class VizCloud : BaseExtractor
    {
        NineAnimeScraper NineAnimeScraper;

        private Regex Regex = new Regex("(.+?/)e(?:mbed)?/([a-zA-Z0-9]+)");

        public VizCloud(NineAnimeScraper scraper)
        {
            NineAnimeScraper = scraper;
        }

        public override async Task<List<Quality>> ExtractQualities(string url)
        {
            var group = Regex.Match(url).Groups;
            var host = group[1].Value;

            //string cipherKey = "L0vG3vmGn7q0eMKw";
            //string mainKey = "At5cx7gXHCOw8q9c";

            string cipherKey = "ydmaS0SmigxlYcuo";
            string mainKey = "l3wYE83wu8bkk3xi";

            var id = NineAnimeScraper.encrypt(NineAnimeScraper.cipher(cipherKey, NineAnimeScraper.encrypt(group[2].Value))).Replace("/", "_").Replace("=", "");

            url = $"{host}mediainfo/{id}?key={mainKey}";

            var headers = new WebHeaderCollection()
            {
                { "Referer", $"{NineAnimeScraper.BaseUrl}/" }
            };

            string htmlData = await Http.GetHtmlAsync(url, headers);

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(htmlData);

            var list = new List<Quality>();

            return list;
        }
    }
}