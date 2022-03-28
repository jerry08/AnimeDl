using System.Collections.Specialized;
using System.Net;

namespace AnimeDl
{
    public class Quality
    {
        public string Resolution { get; set; }

        public string QualityUrl { get; set; }

        public string FileSize { get; set; }

        public string FileType { get; set; }

        public string Referer { get; set; }

        //public NameValueCollection Headers { get; set; }
        public WebHeaderCollection Headers { get; set; }
    }
}