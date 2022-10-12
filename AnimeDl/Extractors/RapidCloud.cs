using System;
using System.Net;
using System.Net.Http;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using AnimeDl.Models;
using AnimeDl.Scrapers;
using AnimeDl.Utils.Extensions;

namespace AnimeDl.Extractors;

public class RapidCloud : VideoExtractor
{
    private readonly string fallbackKey = "c1d17096f2ca11b7";
    //private readonly string consumetApi = "https://consumet-api.herokuapp.com";
    private readonly string consumetApi = "https://api.consumet.org";
    private readonly string enimeApi = "https://api.enime.moe";
    private readonly string host = "https://rapid-cloud.co";

    public RapidCloud(HttpClient http,
        VideoServer server) : base(http, server)
    {
    }

    public override async Task<List<Video>> Extract()
    {
        var url = _server.Embed.Url;

        var id = new Stack<string>(url.Split('/')).Pop().Split('?')[0];
        //var sId = await _http.SendHttpRequestAsync(consumetApi + "/utils/rapid-cloud");
        var sId = await _http.SendHttpRequestAsync($"{enimeApi}/tool/rapid-cloud/server-id",
            new WebHeaderCollection()
            {
                { "User-Agent", "Saikou" }
            }
        );

        if (string.IsNullOrEmpty(sId))
        {
            sId = await _http.SendHttpRequestAsync($"{enimeApi}/tool/rapid-cloud/server-id");
        }

        var headers = new WebHeaderCollection()
        {
            { "X-Requested-With", "XMLHttpRequest" }
        };

        var res = await _http.SendHttpRequestAsync($"{this.host}/ajax/embed-6/getSources?id={id}&sId={sId}", headers);

        var decryptKey = await _http.SendHttpRequestAsync("https://raw.githubusercontent.com/consumet/rapidclown/main/key.txt");
        if (string.IsNullOrEmpty(decryptKey))
            decryptKey = fallbackKey;

        var jObj = JObject.Parse(res);

        var sources = jObj["sources"]?.ToString()!;

        var isEncrypted = (bool)jObj["encrypted"]!;
        if (isEncrypted)
            sources = new RapidCloudDecryptor().Decrypt(sources, decryptKey);

        var m3u8File = JArray.Parse(sources)[0]["file"]?.ToString()!;

        var videoList = new List<Video>();

        videoList.Add(new Video()
        {
            VideoUrl = m3u8File,
            Headers = headers,
            Format = VideoType.M3u8,
            Resolution = "Multi Quality"
        });

        return videoList;
    }
}