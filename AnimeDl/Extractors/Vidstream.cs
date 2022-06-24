using System.Collections.Generic;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace AnimeDl.Extractors;

/*class Vidstream : BaseExtractor
{
    bool ShowAllMirrorLinks;

    public Vidstream(bool showAllMirrorLinks)
    {
        ShowAllMirrorLinks = showAllMirrorLinks;
    }

    public override async Task<List<Quality>> ExtractQualities(string url)
    {
        string htmlData = await _netHttpClient.SendHttpRequestAsync(url);

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
}*/

internal class Vidstream : BaseExtractor
{
    private string MainUrl;

    public Vidstream(string mainUrl, NetHttpClient netHttpClient) : base(netHttpClient)
    {
        MainUrl = mainUrl;
    }

    private string GetExtractorUrl(string id)
    {
        return $"{MainUrl}/streaming.php?id={id}";
    }

    private string GetDownloadUrl(string id)
    {
        return $"{MainUrl}/download?id={id}";
    }

    public override async Task<List<Quality>> ExtractQualities(string id)
    {
        var url = GetDownloadUrl(id);

        var htmlData = await _netHttpClient.SendHttpRequestAsync(url);

        var doc = new HtmlDocument();
        doc.LoadHtml(htmlData);

        var list = new List<Quality>();

        return list;
    }
}