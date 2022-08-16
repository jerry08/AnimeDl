using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using HtmlAgilityPack;
using AnimeDl.Utils.Extensions;

namespace AnimeDl.Extractors;

internal class RapidCloud : BaseExtractor
{
    public RapidCloud(NetHttpClient netHttpClient) : base(netHttpClient)
    {
    }

    public override async Task<List<Quality>> ExtractQualities(string url)
    {
        var headers = new WebHeaderCollection()
        {
            { "Referer", "https://zoro.to/" }
        };

        var html = await _netHttpClient.SendHttpRequestAsync(url, headers);

        var key = html.FindBetween("var recaptchaSiteKey = '", "',");
        var number = html.FindBetween("recaptchaNumber = '", "';");

        var token = await Captcha(url, key);
        //var jsonLink = $"https://rapid-cloud.ru/ajax/embed-6/getSources?id={url.FindBetween("/embed-6/", "?z=")}&_token=${token}&_number={number}";
        var jsonLink = $"https://rapid-cloud.co/ajax/embed-6/getSources?id={url.FindBetween("/embed-6/", "?z=")}&_token=${token}&_number={number}";

        html = await _netHttpClient.SendHttpRequestAsync(jsonLink, headers);

        var jsonObj = JObject.Parse(html);

        //if (key != null && number != null)
        //{
        //    var test = await Captcha(url, key);
        //}

        var list = new List<Quality>();

        var sources = jsonObj["sources"]!.ToString();
        var array = JArray.Parse(sources)[0];

        list.Add(new Quality()
        {
            QualityUrl = array["file"]!.ToString(),
            Headers = headers,
            Resolution = "Auto p"
        });

        return list;
    }

    private async Task<string> Captcha(string url, string key)
    {
        var uri = new Uri(url);
        var domain = (Convert.ToBase64String(Encoding.ASCII.GetBytes(uri.Scheme + "://" + uri.Host + ":443")) + ".").Replace("\n", "");
        var vToken = (await _netHttpClient.SendHttpRequestAsync($"https://www.google.com/recaptcha/api.js?render={key}", new WebHeaderCollection() 
        {
            { "Referrer", uri.Scheme + "://" + uri.Host }
        })).Replace("\n", "").FindBetween("/releases/", "/recaptcha");
        var recapTokenHtml = await _netHttpClient.SendHttpRequestAsync($"https://www.google.com/recaptcha/api2/anchor?ar=1&hl=en&size=invisible&cb=kr60249sk&k={key}&co={domain}&v={vToken}");

        var doc = new HtmlDocument();
        doc.LoadHtml(recapTokenHtml);

        return vToken;
    }
}
