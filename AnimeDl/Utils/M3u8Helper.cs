using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using AnimeDl.Utils.Extensions;

namespace AnimeDl;

internal class M3u8Helper
{
    private Regex ENCRYPTION_DETECTION_REGEX = new Regex("#EXT-X-KEY:METHOD=([^,]+),");
    private Regex ENCRYPTION_URL_IV_REGEX = new Regex("#EXT-X-KEY:METHOD=([^,]+),URI=\"([^\"]+)\"(?:,IV=(.*))?");
    private Regex QUALITY_REGEX = new Regex(@"#EXT-X-STREAM-INF:(?:(?:.*?(?:RESOLUTION=\d+x(\d+)).*?\s+(.*))|(?:.*?\s+(.*)))");
    private Regex TS_EXTENSION_REGEX = new Regex(@"(.*\.ts.*|.*\.jpg.*)"); //.jpg here 'case vizcloud uses .jpg instead of .ts

    public class M3u8Stream
    {
        public string StreamUrl { get; set; } = default!;
        public string Resolution { get; set; } = default!;
        public WebHeaderCollection Headers { get; set; } = default!;
    }

    public readonly HttpClient _http;

    public M3u8Helper(HttpClient http)
    {
        _http = http;
    }

    private string? AbsoluteExtensionDetermination(string url)
    {
        var split = url.Split('/');
        var gg = split[split.Length - 1].Split('?')[0];
        if (gg.Contains("."))
        {
            return gg.Split('.')?.LastOrDefault();
        }

        return null;
    }

    private bool IsNotCompleteUrl(string url)
    {
        return !url.Contains("https://") && !url.Contains("http://");
    }

    private string? GetParentLink(string uri)
    {
        var split = uri.Split('/').ToList();
        split.Remove(split.LastOrDefault()!);
        return string.Join("/", split);
    }

    public async Task<IEnumerable<M3u8Stream>> M3u8Generation(M3u8Stream m3u8)
    {
        var m3u8Parent = GetParentLink(m3u8.StreamUrl);
        var response = await _http.SendHttpRequestAsync(m3u8.StreamUrl, m3u8.Headers);
        //var response = Http.GetHtml(m3u8Parent, m3u8.Headers);

        var list = new List<M3u8Stream>();

        foreach (Match? match in QUALITY_REGEX.Matches(response))
        {
            if (match == null)
                continue;

            //var hh = match.ToString().Split(',');
            //for (int i = 0; i < hh.Length; i++)
            //{
            //    var sdv = JToken.FromObject(hh[i]);
            //}
            //var sd = JToken.FromObject(hh);
            //var shh = JObject.FromObject(match.ToString());

            //var token = JToken.FromObject(match.ToString());

            var resolution = match.Groups[1]?.Value!;
            var m3u8Link = match.Groups[2]?.Value!;
            var m3u8Link2 = match.Groups[3]?.Value!;
            if (string.IsNullOrEmpty(m3u8Link))
            {
                m3u8Link = m3u8Link2;
            }

            if (AbsoluteExtensionDetermination(m3u8Link) == "m3u8")
            {
                if (IsNotCompleteUrl(m3u8Link))
                {
                    m3u8Link = $"{m3u8Parent}/{m3u8Link}";
                }
            }

            list.Add(new M3u8Stream()
            {
                Resolution = resolution,
                StreamUrl = m3u8Link,
                Headers = m3u8.Headers
            });
        }

        //yield return new M3u8Stream()
        //{
        //    StreamUrl = m3u8.StreamUrl,
        //    Headers = m3u8.Headers
        //};

        return list;
    }
}