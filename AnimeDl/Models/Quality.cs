using System.Collections.Specialized;
using System.Net;

namespace AnimeDl;

public class Quality
{
    public string Resolution { get; set; } = default!;

    public string QualityUrl { get; set; } = default!;

    public string FileSize { get; set; } = default!;

    public string FileType { get; set; } = default!;

    public string Referer { get; set; } = default!;

    //public NameValueCollection Headers { get; set; }
    public WebHeaderCollection Headers { get; set; } = new WebHeaderCollection();
}