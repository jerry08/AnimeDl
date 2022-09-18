using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Specialized;
using Newtonsoft.Json.Linq;

namespace AnimeDl.Extractors;

internal class FPlayer : BaseExtractor
{
    public FPlayer(NetHttpClient netHttpClient) : base(netHttpClient)
    {
    }

    public override async Task<List<Quality>> ExtractQualities(string url)
    {
        var apiLink = url.Replace("/v/", "/api/source/");

        var list = new List<Quality>();

        try
        {
            var headers = new NameValueCollection()
            {
                { "Referer", url }
            };

            var json = await _netHttpClient.PostAsync(apiLink, headers);
            if (!string.IsNullOrEmpty(json))
            {
                var data = JArray.Parse(JObject.Parse(json)["data"]!.ToString());
                for (int i = 0; i < data.Count; i++)
                {
                    list.Add(new()
                    {
                        QualityUrl = data[i]["file"]!.ToString(),
                        Resolution = data[i]["label"]!.ToString(),
                        IsM3U8 = data[i]["file"]!.ToString().Contains(".m3u8"),
                        FileType = data[i]["type"]!.ToString(),
                    });
                }

                return list;
            }
        }
        catch { }

        return list;
    }
}