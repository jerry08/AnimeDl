using System.Net;

namespace AnimeDl.Models;

public class FileUrl
{
    public string Url { get; set; } = default!;

    public WebHeaderCollection Headers { get; set; } = default!;

    public FileUrl()
    {

    }

    public FileUrl(string url)
    {
        Url = url;
    }

    public FileUrl(string url, WebHeaderCollection headers)
    {
        Url = url;
        Headers = headers;
    }
}