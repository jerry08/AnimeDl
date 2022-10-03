using System.Collections.Specialized;
using Newtonsoft.Json;
using AnimeDl.Utils.JsonConverters;

namespace AnimeDl;

/// <summary>
/// The Class which contains all the information about a Video
/// </summary>
public class Video
{
    /// <summary>
    /// Will represent quality to user in form of `"${quality}p"` (1080p).
    /// If quality is null, shows "Unknown Quality".
    /// If isM3U8 is true, shows "Multi Quality"
    /// </summary>
    public string Resolution { get; set; } = default!;

    /// <summary>
    /// If the video is an M3U8 file, set this variable to true,
    /// This makes the app show it as a "Multi Quality" Link
    /// </summary>
    public bool IsM3U8 { get; set; }

    /// <summary>
    /// The direct url to the Video.
    /// Supports mp4, mkv and m3u8 for now, afaik
    /// </summary>
    public string VideoUrl { get; set; } = default!;

    /// <summary>
    /// 
    /// </summary>
    public string FileSize { get; set; } = default!;

    /// <summary>
    /// 
    /// </summary>
    public string FileType { get; set; } = default!;

    /// <summary>
    /// 
    /// </summary>
    public string Referer { get; set; } = default!;

    /// <summary>
    /// 
    /// </summary>
    //[JsonConverter(typeof(WebHeaderCollectionConverter))]
    //public WebHeaderCollection Headers { get; set; } = new();

    //public Dictionary<string, string> Headers { get; set; } = new();

    [JsonConverter(typeof(NameValueCollectionConverter))]
    public NameValueCollection Headers { get; set; } = new();
}