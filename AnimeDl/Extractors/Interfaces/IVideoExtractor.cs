using System.Threading.Tasks;
using System.Collections.Generic;

namespace AnimeDl.Extractors.Interfaces;

public interface IVideoExtractor
{
    Task<List<Video>> Extract();
}