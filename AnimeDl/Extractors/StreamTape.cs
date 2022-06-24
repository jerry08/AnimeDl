using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
        var ss = LinkRegex.Matches(text);

        var list = new List<Quality>();

        return list;
    }
}
