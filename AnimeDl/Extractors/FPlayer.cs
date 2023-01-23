using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Specialized;
using Newtonsoft.Json.Linq;
using AnimeDl.Models;
using AnimeDl.Utils.Extensions;

namespace AnimeDl.Extractors;

public class FPlayer : VideoExtractor
{
    public FPlayer(HttpClient http,
        VideoServer server) : base(http, server)
    {
    }

    public override async Task<List<Video>> Extract()
    {
        var url = _server.Embed.Url;

        var apiLink = url.Replace("/v/", "/api/source/");

        var list = new List<Video>();

        try
        {
            var headers = new Dictionary<string, string>()
            {
                { "Referer", url }
            };

            var json = await _http.PostAsync(apiLink, headers);
            if (!string.IsNullOrEmpty(json))
            {
                var data = JArray.Parse(JObject.Parse(json)["data"]!.ToString());
                for (int i = 0; i < data.Count; i++)
                {
                    list.Add(new()
                    {
                        VideoUrl = data[i]["file"]!.ToString(),
                        Resolution = data[i]["label"]!.ToString(),
                        Format = VideoType.Container,
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