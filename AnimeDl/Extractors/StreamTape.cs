using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using AnimeDl.Models;
using AnimeDl.Utils.Extensions;

namespace AnimeDl.Extractors;

internal class StreamTape : VideoExtractor
{
    private readonly Regex LinkRegex = new(@"'robotlink'\)\.innerHTML = '(.+?)'\+ \('(.+?)'\)");
    
    public virtual string MainUrl => "https://streamtape.com";

    public StreamTape(HttpClient http,
        VideoServer server) : base(http, server)
    {
    }

    public override async Task<List<Video>> Extract()
    {
        var url = _server.Embed.Url;

        var text = await _http.SendHttpRequestAsync(url);
        var reg = LinkRegex.Match(text);

        var vidUrl = $"https:{reg.Groups[1]!.Value + reg.Groups[2]!.Value.Substring(3)}";

        var list = new List<Video>
        {
            new Video()
            {
                IsM3U8 = vidUrl.Contains(".m3u8"),
                VideoUrl = vidUrl,
                Resolution = "auto",
            }
        };

        return list;
    }
}