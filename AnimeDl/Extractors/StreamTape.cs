using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AnimeDl.Extractors;

internal class StreamTape : BaseExtractor
{
    private readonly Regex LinkRegex = new(@"'robotlink'\)\.innerHTML = '(.+?)'\+ \('(.+?)'\)");
    
    public virtual string MainUrl => "https://streamtape.com";

    public StreamTape(NetHttpClient netHttpClient) : base(netHttpClient)
    {
    }

    public override async Task<List<Quality>> ExtractQualities(string url)
    {
        var text = await _netHttpClient.SendHttpRequestAsync(url);
        var reg = LinkRegex.Match(text);

        var vidUrl = $"https:{reg.Groups[1]!.Value + reg.Groups[2]!.Value.Substring(3)}";

        var list = new List<Quality>
        {
            new Quality()
            {
                IsM3U8 = vidUrl.Contains(".m3u8"),
                QualityUrl = vidUrl,
                Resolution = "auto",
            }
        };

        return list;
    }
}