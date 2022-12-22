using System;
using System.Net;
using System.Net.Http;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using AnimeDl.Models;
using AnimeDl.Utils.Extensions;
using AnimeDl.Utils;

namespace AnimeDl.Extractors;

public class StreamSB : VideoExtractor
{
    private readonly char[] hexArray = "0123456789ABCDEF".ToCharArray();

    public StreamSB(HttpClient http,
        VideoServer server) : base(http, server)
    {
    }

    private string BytesToHex(byte[] bytes)
    {
        var hexChars = new char[(bytes.Length * 2)];
        for (int j = 0; j < bytes.Length; j++)
        {
            var v = bytes[j] & 0xFF;

            hexChars[j * 2] = hexArray[v >> 4];
            hexChars[j * 2 + 1] = hexArray[v & 0x0F];
        }
        
        return new string(hexChars);
    }

    public override async Task<List<Video>> Extract()
    {
        var url = _server.Embed.Url;

        var id = url.FindBetween("/e/", ".html");
        if (string.IsNullOrEmpty(id))
            id = url.Split(new[] { "/e/" }, StringSplitOptions.None)[1];

        var bytes = Encoding.ASCII.GetBytes($"||{id}||||streamsb");
        var bytesToHex = BytesToHex(bytes);

        var jsonLink = $"https://streamsss.net/sources49/{bytesToHex}/";

        var headers = new WebHeaderCollection()
        {
            //{ "watchsb", "streamsb" },
            { "watchsb", "sbstream" },
            { "User-Agent", Http.ChromeUserAgent() },
            { "Referer", url },
        };

        var json = await _http.SendHttpRequestAsync(jsonLink, headers);

        var jObj = JObject.Parse(json);
        var masterUrl = jObj["stream_data"]?["file"]?.ToString().Trim('"')!;

        return new List<Video>
        {
            new Video()
            {
                Format = VideoType.M3u8,
                VideoUrl = masterUrl,
                Headers = headers,
                Resolution = "Multi Quality"
            }
        };
    }
}