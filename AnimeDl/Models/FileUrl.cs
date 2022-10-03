using System.Collections.Specialized;

namespace AnimeDl.Models;

public class FileUrl
{
    public string Url { get; set; } = default!;

    public NameValueCollection Headers { get; set; } = default!;

    public FileUrl()
    {

    }

    public FileUrl(string url)
    {
        Url = url;
    }

    public FileUrl(string url, NameValueCollection headers)
    {
        Url = url;
        Headers = headers;
    }
}