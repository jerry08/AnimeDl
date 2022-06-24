using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnimeDl.Extractors.Interfaces;

internal interface IAnimeExtractor
{
    Task<List<Quality>> ExtractQualities(string url);
}