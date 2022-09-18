using System.Collections.Generic;
using System.Threading.Tasks;
using AnimeDl.Extractors.Interfaces;

namespace AnimeDl.Extractors;

abstract class BaseExtractor : IAnimeExtractor
{
    public readonly NetHttpClient _netHttpClient;

    public BaseExtractor(NetHttpClient netHttpClient)
    {
        _netHttpClient = netHttpClient;
    }

    public abstract Task<List<Quality>> ExtractQualities(string url);
}