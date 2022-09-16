using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using AnimeDl.Utils.Extensions;

namespace AnimeDl.Extractors;

internal class GogoCDN : BaseExtractor
{
    public GogoCDN(NetHttpClient netHttpClient) : base(netHttpClient)
    {
    }

    public override async Task<List<Quality>> ExtractQualities(string url)
    {
        var list = new List<Quality>();

        var htmlData = await _netHttpClient.SendHttpRequestAsync(url);

        var doc = new HtmlDocument();
        doc.LoadHtml(htmlData);

        //var script = doc.DocumentNode
        //    .SelectSingleNode("script[data-name='crypto'");

        //var tt = doc.DocumentNode.Descendants("script")
        //    .Where(x => x.Name == "script").ToList();

        var scripts = doc.DocumentNode.Descendants()
            .Where(x => x.Name == "script").ToList();

        var cryptoScript = scripts.Where(x => x.Attributes["data-name"]?.Value == "episode")
            .FirstOrDefault()!;

        //string dataValue = scripts.Where(x => x.Attributes["data-name"]?.Value == "crypto")
        //  .FirstOrDefault().Attributes["data-value"].Value;

        var dataValue = cryptoScript.Attributes["data-value"].Value;

        var keys = KeysAndIv();

        var decrypted = CryptoHandler(dataValue, keys.Item1, keys.Item3, false).Replace("\t", "");
        var id = decrypted.FindBetween("", "&");
        var end = decrypted.SubstringAfter(id);

        var link = $"https://{new Uri(url).Host}/encrypt-ajax.php?id={CryptoHandler(id, keys.Item1, keys.Item3, true)}{end}&alias={id}";

        //var host = new GogoAnimeScraper().BaseUrl;

        var encHtmlData = await _netHttpClient.SendHttpRequestAsync(link,
            new WebHeaderCollection()
            {
                { "X-Requested-With", "XMLHttpRequest" },
                //{ "Referer", host },
            });

        if (string.IsNullOrEmpty(encHtmlData))
        {
            return list;
        }

        var jsonObj = JObject.Parse(encHtmlData);
        var sources = CryptoHandler(jsonObj["data"]!.ToString(), keys.Item2, keys.Item3, false);
        sources = sources.Replace(@"o""<P{#meme"":""", @"e"":[{""file"":""");

        var source = JObject.Parse(sources)["source"]!.ToString();
        var array = JArray.Parse(source);

        if (array.Count == 1 && array[0]["type"]?.ToString() == "hls")
        {
            string fileURL = array[0]["file"]!.ToString().Trim('"');
            string masterPlaylist = await _netHttpClient.SendHttpRequestAsync(fileURL);
            var masterSplit = masterPlaylist.Split(new string[] { "#EXT-X-STREAM-INF:" }, StringSplitOptions.None).ToList();
            masterSplit.Remove(masterSplit[0]);

            for (int i = 0; i < masterSplit.Count; i++)
            {
                var videoUrlSplit = fileURL.Split('/').ToList();
                videoUrlSplit.RemoveAt(videoUrlSplit.Count - 1);
                var itSplit = masterSplit[i].Split(new string[] { "\n" }, StringSplitOptions.None).ToList();
                itSplit.RemoveAll(x => string.IsNullOrEmpty(x));

                var quality = itSplit[0].SubstringAfter("RESOLUTION=").SubstringBefore("x") + " p";
                var videoUrl = string.Join("/", videoUrlSplit) + "/" + itSplit.LastOrDefault();

                list.Add(new Quality()
                {
                    FileType = "m3u8",
                    IsM3U8 = true,
                    QualityUrl = videoUrl,
                    Resolution = quality,
                    Headers = new WebHeaderCollection()
                    {
                        { "Referer", url },
                    }
                });
            }

            return list;
        }

        list = array.Select(x =>
        {
            return new Quality()
            {
                IsM3U8 = x["file"]!.ToString().Contains(".m3u8"),
                QualityUrl = x["file"]!.ToString(),
                Resolution = x["label"]!.ToString(),
                FileType = x["type"]!.ToString(),
                Headers = new WebHeaderCollection()
                {
                    { "Referer", url },
                }
            };
        }).ToList();

        return list;
    }

    private Tuple<string, string, string> KeysAndIv()
    {
        return new Tuple<string, string, string>
            ("37911490979715163134003223491201", "54674138327930866480207815084989", "3134003223491201");
    }

    private static string CryptoHandler(string dataValue, string key, string iv, bool encrypt = true)
    {
        //var key = Encoding.UTF8.GetBytes("63976882873559819639988080820907");
        //var iv = Encoding.UTF8.GetBytes("4770478969418267");

        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        byte[] ivBytes = Encoding.UTF8.GetBytes(iv);

        var cryptoProvider = new RijndaelManaged();
        cryptoProvider.Mode = CipherMode.CBC;
        cryptoProvider.Padding = PaddingMode.PKCS7;

        if (encrypt)
        {
            // Convert from Base64 to binary
            byte[] bytIn = Encoding.ASCII.GetBytes(dataValue);

            //var padding = new byte[] { 0x8, 0xe, 0x3, 0x8, 0x9, 0x3, 0x4, 0x9 };
            //bytIn = bytIn.Concat(padding).ToArray();

            // Create a MemoryStream
            var ms = new MemoryStream();

            // Create Crypto Stream that encrypts a stream
            var cs = new CryptoStream(ms,
                cryptoProvider.CreateEncryptor(keyBytes, ivBytes),
                CryptoStreamMode.Write);

            // Write content into MemoryStream
            cs.Write(bytIn, 0, bytIn.Length);
            cs.FlushFinalBlock();

            byte[] bytOut = ms.ToArray();
            return Convert.ToBase64String(bytOut);
        }
        else
        {
            // Convert from Base64 to binary
            byte[] bytIn = Convert.FromBase64String(dataValue);

            // Create a MemoryStream
            var ms = new MemoryStream(bytIn, 0, bytIn.Length);

            // Create a CryptoStream that decrypts the data
            var cs = new CryptoStream(ms,
                cryptoProvider.CreateDecryptor(keyBytes, ivBytes),
                CryptoStreamMode.Read);

            // Read the Crypto Stream
            var sr = new StreamReader(cs);

            return sr.ReadToEnd();
        }
    }
}