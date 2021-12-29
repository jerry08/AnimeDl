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
    class Streamsb
    {
        public async Task<string> ExtractUrl(string url)
        {
            url = url.Replace("/e/", "/d/");
            string html = await Utils.GetHtmlAsync(url);

            var test = "https://sbplay1.com/dl?op=download_orig&";

            return "";
        }
    }
}
