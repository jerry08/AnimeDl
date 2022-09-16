using System.Net;
using System.Collections.Specialized;

namespace AnimeDl;

public class Quality
{
    public string Resolution { get; set; } = default!;

    public string QualityUrl { get; set; } = default!;

    public string FileSize { get; set; } = default!;

    public string FileType { get; set; } = default!;

    public string Referer { get; set; } = default!;

    public bool IsM3U8 { get; set; }

    //public NameValueCollection Headers { get; set; }
    public WebHeaderCollection Headers { get; set; } = new WebHeaderCollection();
}