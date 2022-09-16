using System;
using System.Net;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using AnimeDl.Scrapers;
using AnimeDl.Utils.Extensions;

namespace AnimeDl.Extractors;

internal class RapidCloud : BaseExtractor
{
    private readonly string fallbackKey = "c1d17096f2ca11b7";
    private readonly string consumetApi = "https://consumet-api.herokuapp.com";
    private readonly string enimeApi = "https://api.enime.moe";
    private readonly string host = "https://rapid-cloud.co";

    public RapidCloud(NetHttpClient netHttpClient) : base(netHttpClient)
    {
    }

    public override async Task<List<Quality>> ExtractQualities(string url)
    {
        var id = new Stack<string>(url.Split('/')).Pop().Split('?')[0];
        var sId = await _netHttpClient.SendHttpRequestAsync(consumetApi + "/utils/rapid-cloud");

        var headers = new WebHeaderCollection()
        {
            { "X-Requested-With", "XMLHttpRequest" }
        };

        var res = await _netHttpClient.SendHttpRequestAsync($"{this.host}/ajax/embed-6/getSources?id={id}&sId={sId}", headers);

        var decryptKey = await _netHttpClient.SendHttpRequestAsync("https://raw.githubusercontent.com/consumet/rapidclown/main/key.txt");
        if (string.IsNullOrEmpty(decryptKey))
            decryptKey = fallbackKey;

        var jObj = JObject.Parse(res);

        var sources = jObj["sources"]?.ToString()!;

        var isEncrypted = (bool)jObj["encrypted"]!;
        if (isEncrypted)
        {
            sources = new TwistDecryptor().Decrypt(sources, decryptKey);
        }

        var m3u8File = JArray.Parse(sources)[0]["file"]?.ToString()!;

        var m3u8data = (await _netHttpClient.SendHttpRequestAsync(m3u8File, headers))
            .Split('\n').Where(x => x.Contains(".m3u8") && x.Contains("RESOLUTION="))
            .ToList();

        var list = new List<Quality>();

        for (int i = 0; i < m3u8data.Count; i++)
        {
            var secondHalf = new Regex(@"(?<=RESOLUTION=).*(?<=,C)|(?<=URI=).*");
            var s = secondHalf.Matches(m3u8data[i]);

            var f1 = s[0].Value.Split(new string[] { ",C" }, StringSplitOptions.None)[0];
            //var f2 = s[1].Value.Replace(/ "/g, '');
            var f2 = s[1].Value;

            list.Add(new Quality()
            {
                QualityUrl = $"{m3u8File.Split(new string[] { "master.m3u8" }, StringSplitOptions.None)[0]}${f2.Replace("iframes", "index")}",
                Headers = headers,
                IsM3U8 = sources.Contains(".m3u8"),
                Resolution = "Auto p"
            });
        }

        //list.Add(new Quality()
        //{
        //    QualityUrl = sources,
        //    Headers = headers,
        //    IsM3U8 = sources.Contains(".m3u8"),
        //    Resolution = "Auto p"
        //});

        return list;
    }
}