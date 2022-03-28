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
            string html = await Utils.GetHtmlAsync(url, new WebHeaderCollection()
            {
                { "Referer", "https://zoro.to/" }
            });

            string key = html.FindBetween("var recaptchaSiteKey = '", "',");
            string number = html.FindBetween("recaptchaNumber = '", "';");

            if (key != null && number != null)
            {
                var test = await Captcha(url, key);
            }

            var list = new List<Quality>();

            return list;
        }

        private async Task<string> Captcha(string url, string key)
        {
            var uri = new Uri(url);
            string domain = Convert.ToBase64String(Encoding.ASCII.GetBytes(uri.Scheme + "://" + uri.Host + ":443")).Replace("\n", "");
            string vToken = (await Utils.GetHtmlAsync($"https://www.google.com/recaptcha/api.js?render={key}", new WebHeaderCollection() 
            {
                { "Referrer", uri.Scheme + "://" + uri.Host }
            })).Replace("\n", "").FindBetween("/releases/", "/recaptcha");
            string recapTokenHtml = await Utils.GetHtmlAsync($"https://www.google.com/recaptcha/api2/anchor?ar=1&hl=en&size=invisible&cb=kr60249sk&k={key}&co={domain}&v={vToken}");

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(recapTokenHtml);

            return null;
        }
    }
}
