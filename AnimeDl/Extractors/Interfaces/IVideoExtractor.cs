using System.Threading.Tasks;
using System.Collections.Generic;
using AnimeDl.Models;

namespace AnimeDl.Extractors.Interfaces;

public interface IVideoExtractor
{
    Task<List<Video>> Extract();
}