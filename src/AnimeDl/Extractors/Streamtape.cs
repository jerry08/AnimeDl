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
    class StreamTape : BaseExtractor
    {
        private Regex LinkRegex = new Regex(@"'robotlink'\)\.innerHTML = '(.+?)'\+ \('(.+?)'\)");
        public virtual string MainUrl => "https://streamtape.com";
        
        public override async Task<List<Quality>> ExtractQualities(string url)
        {
            string text = await Http.GetHtmlAsync(url);
            var ss = LinkRegex.Matches(text);

            var list = new List<Quality>();

            return list;
        }
    }
}
