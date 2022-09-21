using System.Threading.Tasks;
using System.Collections.Generic;

namespace AnimeDl.Extractors.Interfaces;

internal interface IVideoExtractor
{
    Task<List<Video>> Extract();
}