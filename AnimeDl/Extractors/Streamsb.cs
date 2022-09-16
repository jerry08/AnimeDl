using System;
using System.Net;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace AnimeDl.Extractors;

internal class StreamSB2 : StreamSB
{
    public override string MainUrl => "https://sbplay2.com";

    public StreamSB2(NetHttpClient netHttpClient) : base(netHttpClient)
    {
    }
}

internal class StreamSB : BaseExtractor
{
    public StreamSB(NetHttpClient netHttpClient) : base(netHttpClient)
    {
    }

    //public virtual string MainUrl => "https://sbplay1.com";
    public virtual string MainUrl => "https://watchsb.com";

    private readonly char[] hexArray = "0123456789ABCDEF".ToCharArray();

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

    public override async Task<List<Quality>> ExtractQualities(string url)
    {
        var regexID = new Regex("(embed-[a-zA-Z0-9]{0,8}[a-zA-Z0-9_-]+|\\/e\\/[a-zA-Z0-9]{0,8}[a-zA-Z0-9_-]+)");
        var id = Regex.Replace(regexID.Match(url).Groups[0].Value, "(embed-|\\/e\\/)", "");
        //var bytes = Encoding.ASCII.GetBytes(id);
        var bytes = Encoding.ASCII.GetBytes($"||{id}||||streamsb");
        var bytesToHex = BytesToHex(bytes);
        //var master = $"{MainUrl}/sources41/566d337678566f743674494a7c7c{bytesToHex}7c7c346b6767586d6934774855537c7c73747265616d7362/6565417268755339773461447c7c346133383438333436313335376136323337373433383634376337633465366534393338373136643732373736343735373237613763376334363733353737303533366236333463353333363534366137633763373337343732363536313664373336327c7c6b586c3163614468645a47617c7c73747265616d7362";
        //var jsonLink = $"{MainUrl}/sources41/6d6144797752744a454267617c7c{bytesToHex}7c7c4e61755a56456f34385243727c7c73747265616d7362/6b4a33767968506e4e71374f7c7c343837323439333133333462353935333633373836643638376337633462333634663539343137373761333635313533333835333763376333393636363133393635366136323733343435323332376137633763373337343732363536313664373336327c7c504d754478413835306633797c7c73747265616d7362";
        
        //var jsonLink = $"{MainUrl}/sources41/616e696d646c616e696d646c7c7c{bytesToHex}7c7c616e696d646c616e696d646c7c7c73747265616d7362/616e696d646c616e696d646c7c7c363136653639366436343663363136653639366436343663376337633631366536393664363436633631366536393664363436633763376336313665363936643634366336313665363936643634366337633763373337343732363536313664373336327c7c616e696d646c616e696d646c7c7c73747265616d7362";
        
        var jsonLink = $"{MainUrl}/sources48/" + bytesToHex + "/";

        //var source = "https://sbplay2.com/sources41";
        //var jsonLink = $"{source}/7361696b6f757c7c{bytesToHex}7c7c7361696b6f757c7c73747265616d7362/7361696b6f757c7c363136653639366436343663363136653639366436343663376337633631366536393664363436633631366536393664363436633763376336313665363936643634366336313665363936643634366337633763373337343732363536313664373336327c7c7361696b6f757c7c73747265616d7362";

        var host = url.Split(new string[] { "https://" },
            StringSplitOptions.None)[1].Split('/')[0];

        var headers = new WebHeaderCollection()
        {
            { "Host", host },
            { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; rv:91.0) Gecko/20100101 Firefox/91.0" },
            { "Accept", "application/json, text/plain, */*" },
            { "Accept-Language", "en-US,en;q=0.5" },
            { "Referer", url },
            //{ "watchsb", "streamsb" },
            { "watchsb", "sbstream" },
            { "DNT", "1" },
            { "Connection", "keep-alive" },
            { "Sec-Fetch-Dest", "empty" },
            { "Sec-Fetch-Mode", "no-cors" },
            { "Sec-Fetch-Site", "same-origin" },
            { "TE", "trailers" },
            { "Pragma", "no-cache" },
            { "Cache-Control", "no-cache" }
        };

        var json = await _netHttpClient.SendHttpRequestAsync(jsonLink, headers);

        var jObj = JObject.Parse(json);
        var masterUrl = jObj["stream_data"]?["file"]?.ToString().Trim('"')!;

        //var test = await _netHttpClient.SendHttpRequestAsync(masterUrl, headers);

        headers = new WebHeaderCollection()
        {
            //{ "Host", host },
            { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; rv:91.0) Gecko/20100101 Firefox/91.0" },
            { "Accept", "application/json, text/plain, */*" },
            { "Accept-Language", "en-US,en;q=0.5" },
            //{ "Referer", url },
            //{ "watchsb", "streamsb" },
            { "watchsb", "sbstream" },
            { "DNT", "1" },
            { "Connection", "keep-alive" },
            { "Sec-Fetch-Dest", "empty" },
            { "Sec-Fetch-Mode", "no-cors" },
            { "Sec-Fetch-Site", "same-origin" },
            { "TE", "trailers" },
            { "Pragma", "no-cache" },
            { "Cache-Control", "no-cache" }
        };

        var m3u8Streams = (await new M3u8Helper(_netHttpClient).M3u8Generation(new M3u8Helper.M3u8Stream()
        {
            StreamUrl = masterUrl,
            Headers = headers
        })).ToList();

        //var cleanstreamurl = Regex.Replace(sgs[0].StreamUrl, "https://.*/hls/", $"{urlmain}/hls/");

        //var test = await _netHttpClient.SendHttpRequestAsync(m3u8Streams[0].StreamUrl, m3u8Streams[0].Headers);

        var list = new List<Quality>
        {
            new Quality()
            {
                IsM3U8 = masterUrl.Contains(".m3u8"),
                QualityUrl = masterUrl,
                Headers = headers,
                Resolution = "auto"
            }
        };

        foreach (var m3u8Stream in m3u8Streams)
        {
            var split = m3u8Stream.StreamUrl.Replace("/hls/", "/")
                .Split('/');
            //split[split.Length - 1] = "video.mp4";

            var mp4StreamUrl = string.Join("/", split);
            //var mp4StreamUrl = m3u8Stream.StreamUrl;

            //var test1 = await _netHttpClient.SendHttpRequestAsync(m3u8Stream.StreamUrl, m3u8Stream.Headers);
            //var test2 = await _netHttpClient.SendHttpRequestAsync(mp4StreamUrl, m3u8Stream.Headers);

            list.Add(new Quality() 
            {
                IsM3U8 = mp4StreamUrl.Contains(".m3u8"),
                QualityUrl = mp4StreamUrl,
                Headers = m3u8Stream.Headers,
                Resolution = m3u8Stream.Quality
            });
        }

        return list;
    }
}