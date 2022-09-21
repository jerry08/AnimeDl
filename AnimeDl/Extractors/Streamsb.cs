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

namespace AnimeDl.Extractors;

internal class StreamSB : VideoExtractor
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

        var regexID = new Regex("(embed-[a-zA-Z0-9]{0,8}[a-zA-Z0-9_-]+|\\/e\\/[a-zA-Z0-9]{0,8}[a-zA-Z0-9_-]+)");
        var id = Regex.Replace(regexID.Match(url).Groups[0].Value, "(embed-|\\/e\\/)", "");

        //var id2 = url.FindBetween("/e/", ".html");

        var bytes = Encoding.ASCII.GetBytes($"||{id}||||streamsb");
        var bytesToHex = BytesToHex(bytes);

        var jsonLink = $"https://streamsss.net/sources48/sources48/{bytesToHex}/";

        var headers = new WebHeaderCollection()
        {
            //{ "watchsb", "streamsb" },
            { "watchsb", "sbstream" },
        };

        var json = await _http.SendHttpRequestAsync(jsonLink, headers);

        var jObj = JObject.Parse(json);
        var masterUrl = jObj["stream_data"]?["file"]?.ToString().Trim('"')!;

        var list = new List<Video>
        {
            new Video()
            {
                IsM3U8 = masterUrl.Contains(".m3u8"),
                VideoUrl = masterUrl,
                Headers = headers,
                Resolution = "Multi Quality"
            }
        };

        return list;
    }
}