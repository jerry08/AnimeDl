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
    class RapidCloud : BaseExtractor
    {        
        public override async Task<List<Quality>> ExtractQualities(string url)
        {
            var headers = new WebHeaderCollection()
            {
                { "Referer", "https://zoro.to/" }
            };

            string html = await Http.GetHtmlAsync(url, headers);

            string key = html.FindBetween("var recaptchaSiteKey = '", "',");
            string number = html.FindBetween("recaptchaNumber = '", "';");

            var token = await Captcha(url, key);
            var sg = $"https://rapid-cloud.ru/ajax/embed-6/getSources?id={url.FindBetween("/embed-6/", "?z=")}&_token=${token}&_number={number}";

            html = await Http.GetHtmlAsync(sg, headers);

            var jsonObj = JObject.Parse(html);

            //if (key != null && number != null)
            //{
            //    var test = await Captcha(url, key);
            //}

            var list = new List<Quality>();

            var test = jsonObj["sources"].ToString();
            var array = JArray.Parse(test)[0];
            var tt = array["file"].ToString();

            list.Add(new Quality()
            {
                QualityUrl = array["file"].ToString(),
                Headers = headers,
                Resolution = "Auto p"
            });

            return list;
        }

        private async Task<string> Captcha(string url, string key)
        {
            var uri = new Uri(url);
            string domain = (Convert.ToBase64String(Encoding.ASCII.GetBytes(uri.Scheme + "://" + uri.Host + ":443")) + ".").Replace("\n", "");
            string vToken = (await Http.GetHtmlAsync($"https://www.google.com/recaptcha/api.js?render={key}", new WebHeaderCollection() 
            {
                { "Referrer", uri.Scheme + "://" + uri.Host }
            })).Replace("\n", "").FindBetween("/releases/", "/recaptcha");
            string recapTokenHtml = await Http.GetHtmlAsync($"https://www.google.com/recaptcha/api2/anchor?ar=1&hl=en&size=invisible&cb=kr60249sk&k={key}&co={domain}&v={vToken}");

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(recapTokenHtml);

            return vToken;
        }
    }
}
