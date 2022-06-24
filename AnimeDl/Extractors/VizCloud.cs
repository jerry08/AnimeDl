using System.Net;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using AnimeDl.Scrapers;

namespace AnimeDl.Extractors;

internal class VizCloud : BaseExtractor
{
    NineAnimeScraper NineAnimeScraper;

    private Regex Regex = new("(.+?/)e(?:mbed)?/([a-zA-Z0-9]+)");

    public VizCloud(NineAnimeScraper scraper, NetHttpClient netHttpClient) : base(netHttpClient)
    {
        NineAnimeScraper = scraper;
    }

    public override async Task<List<Quality>> ExtractQualities(string url)
    {
        var group = Regex.Match(url).Groups;
        var host = group[1].Value;

        //string cipherKey = "L0vG3vmGn7q0eMKw";
        //string mainKey = "At5cx7gXHCOw8q9c";

        var cipherKey = "ydmaS0SmigxlYcuo";
        var mainKey = "l3wYE83wu8bkk3xi";

        var id = NineAnimeScraper.encrypt(NineAnimeScraper.cipher(cipherKey, NineAnimeScraper.encrypt(group[2].Value))).Replace("/", "_").Replace("=", "");

        url = $"{host}mediainfo/{id}?key={mainKey}";

        var headers = new WebHeaderCollection()
        {
            { "Referer", $"{NineAnimeScraper.BaseUrl}/" }
        };

        var htmlData = await _netHttpClient.SendHttpRequestAsync(url, headers);

        var doc = new HtmlDocument();
        doc.LoadHtml(htmlData);

        var list = new List<Quality>();

        return list;
    }
}