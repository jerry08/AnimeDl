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
    class StreamSB1 : StreamSB
    {
        public override string MainUrl => "https://sbplay1.com";
    }

    class StreamSB2 : StreamSB
    {
        public override string MainUrl => "https://sbplay2.com";
    }

    class StreamSB3 : StreamSB
    {
        public override string MainUrl => "https://sbplay.one";
    }
    
    class StreamSB4 : StreamSB
    {
        public override string MainUrl => "https://cloudemb.com";
    }

    class StreamSB5 : StreamSB
    {
        public override string MainUrl => "https://sbplay.org";
    }
    
    class StreamSB6 : StreamSB
    {
        public override string MainUrl => "https://embedsb.com";
    }
    
    class StreamSB7 : StreamSB
    {
        public override string MainUrl => "https://pelistop.co";
    }
    
    class StreamSB8 : StreamSB
    {
        public override string MainUrl => "https://StreamSB.net";
    }

    class StreamSB9 : StreamSB
    {
        public override string MainUrl => "https://sbplay.one";
    }

    // This is a modified version of https://github.com/jmir1/aniyomi-extensions/blob/master/src/en/genoanime/src/eu/kanade/tachiyomi/animeextension/en/genoanime/extractors/StreamSBExtractor.kt
    // The following code is under the Apache License 2.0 https://github.com/jmir1/aniyomi-extensions/blob/master/LICENSE
    class StreamSB : BaseExtractor
    {
        public virtual string MainUrl => "https://sbplay1.com";

        private char[] hexArray = "0123456789ABCDEF".ToCharArray();

        private string BytesToHex(byte[] bytes)
        {
            char[] hexChars = new char[(bytes.Length * 2)];
            for (int j = 0; j < bytes.Length; j++)
            {
                var v = bytes[j] & 0xFF;
                hexChars[j * 2] = hexArray[v >> 4];
                hexChars[j * 2 + 1] = hexArray[v & 0x0F];
            }
            
            return new string(hexChars);
        }

        /*public async Task<List<Quality>> ExtractQualities(string url)
        {
            url = url.Replace("/e/", "/d/");
            string html = await Utils.GetHtmlAsync(url);

            var list = new List<Quality>();

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            var qualityMatch = new Regex(@"\d+x(\d+)");
            var downloadContent = new Regex(@"download_video\('(.+?)','(.+?)','(.+?)'\)");
            var nodes = doc.DocumentNode.SelectNodes(".//table/tr").ToList();
            
            for (int i = 0; i < nodes.Count; i++)
            {
                var linkNode = nodes[i].SelectSingleNode(".//a");
                if (linkNode != null)
                {
                    string link = linkNode.Attributes["onclick"]?.Value;

                    Match match = qualityMatch.Match(html);
                    int quality = Convert.ToInt32(match.Groups[1].Value);
                    
                    var groups = downloadContent.Match(link).Groups;
                    string contentId = groups[1].Value;
                    string mode = groups[2].Value;
                    string contentHash = groups[3].Value;

                    string downloadUrl = $"{MainUrl}/dl?op=download_orig&id={contentId}&mode={mode}&hash={contentHash}";

                    var headers = new WebHeaderCollection
                    {
                        { "Referer", url }
                    };

                    string html2 = await Utils.GetHtmlAsync(downloadUrl, headers);

                    HtmlDocument doc2 = new HtmlDocument();
                    doc2.LoadHtml(html2);

                    var directDownloadNodes = doc2.DocumentNode.SelectNodes(".//a")
                        .Where(x => x.Attributes != null && x.Attributes["href"] != null &&
                        x.Attributes["href"].Value.Contains(".mp4")).ToList();

                    for (int j = 0; j < directDownloadNodes.Count; j++)
                    {
                        list.Add(new Quality()
                        {
                            Resolution = quality.ToString(),
                            QualityUrl = directDownloadNodes[j].Attributes["href"].Value,
                            Referer = url
                        });
                    }
                }
            }

            return list;
        }*/
        
        public override async Task<List<Quality>> ExtractQualities(string url)
        {
            var regexID = new Regex("(embed-[a-zA-Z0-9]{0,8}[a-zA-Z0-9_-]+|\\/e\\/[a-zA-Z0-9]{0,8}[a-zA-Z0-9_-]+)");
            var id = Regex.Replace(regexID.Match(url).Groups[0].Value, "(embed-|\\/e\\/)", "");
            var bytes = Encoding.ASCII.GetBytes(id);
            var bytesToHex = BytesToHex(bytes);
            string master = $"{MainUrl}/sources41/566d337678566f743674494a7c7c{bytesToHex}7c7c346b6767586d6934774855537c7c73747265616d7362/6565417268755339773461447c7c346133383438333436313335376136323337373433383634376337633465366534393338373136643732373736343735373237613763376334363733353737303533366236333463353333363534366137633763373337343732363536313664373336327c7c6b586c3163614468645a47617c7c73747265616d7362";

            string host = url.Split(new string[] { "https://" },
                StringSplitOptions.None)[1].Split('/')[0];

            var headers = new WebHeaderCollection()
            {
                { "Host", host },
                { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; rv:91.0) Gecko/20100101 Firefox/91.0" },
                { "Accept", "application/json, text/plain, */*" },
                { "Accept-Language", "en-US,en;q=0.5" },
                { "Referer", url },
                { "watchsb", "streamsb" },
                { "DNT", "1" },
                { "Connection", "keep-alive" },
                { "Sec-Fetch-Dest", "empty" },
                { "Sec-Fetch-Mode", "no-cors" },
                { "Sec-Fetch-Site", "same-origin" },
                { "TE", "trailers" },
                { "Pragma", "no-cache" },
                { "Cache-Control", "no-cache" },
            };

            string json = await Utils.GetHtmlAsync(master, headers);

            var jObj = JObject.Parse(json);
            var masterUrl = jObj["stream_data"]?["file"]?.ToString().Trim('"');

            var m3u8Streams = new M3u8Helper().M3u8Generation(new M3u8Helper.M3u8Stream()
            {
                StreamUrl = masterUrl,
                Headers = new WebHeaderCollection()
                {
                    { "User-Agent", Utils.UserAgent },
                    { "Accept", "*/*" },
                    { "Accept-Language", "en-US,en;q=0.5" },
                    { "Accept-Encoding", "gzip, deflate, br" },
                    { "Origin", MainUrl },
                    { "DNT", "1" },
                    { "Connection", "keep-alive" },
                    { "Referer", $"{MainUrl}/" },
                    { "Sec-Fetch-Dest", "empty" },
                    { "Sec-Fetch-Mode", "cors" },
                    { "Sec-Fetch-Site", "cross-site" }
                }
            }).ToList();

            //var cleanstreamurl = Regex.Replace(sgs[0].StreamUrl, "https://.*/hls/", $"{urlmain}/hls/");

            //var test = Utils.GetHtml(m3u8Streams[0].StreamUrl);

            var list = new List<Quality>();

            foreach (var m3u8Stream in m3u8Streams)
            {
                string[] split = m3u8Stream.StreamUrl.Replace("/hls/", "/")
                    .Split('/');
                split[split.Length - 1] = "video.mp4";

                string mp4StreamUrl = string.Join("/", split);

                list.Add(new Quality() 
                {
                    QualityUrl = mp4StreamUrl,
                    Resolution = m3u8Stream.Quality
                });
            }

            return list;
        }
    }
}
