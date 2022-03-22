using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Security.Cryptography;
using System.IO;
using AnimeDl.Scrapers;

namespace AnimeDl.Extractors
{
    class GogoCDN : BaseExtractor
    {
        bool ShowAllMirrorLinks;

        public GogoCDN(bool showAllMirrorLinks)
        {
            ShowAllMirrorLinks = showAllMirrorLinks;
        }

        public override async Task<List<Quality>> ExtractQualities(string url)
        {
            string htmlData = await Utils.GetHtmlAsync(url);

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(htmlData);

            //var script = doc.DocumentNode
            //    .SelectSingleNode("script[data-name='crypto'");

            //var tt = doc.DocumentNode.Descendants("script")
            //    .Where(x => x.Name == "script").ToList();

            var cryptoScript = doc.DocumentNode.Descendants()
                .Where(x => x.Name == "script").ToList();

            string dataValue = cryptoScript.Where(x => x.Attributes["data-name"]?.Value == "crypto")
                .FirstOrDefault().Attributes["data-value"].Value;

            var id = CryptoHandler(CryptoHandler(dataValue, false).Split('&')[0]);

            var link = $"https://gogoplay4.com/encrypt-ajax.php?id={id}";

            string encHtmlData = await Utils.GetHtmlAsync(link,
                new WebHeaderCollection()
                {
                    { "X-Requested-With", "XMLHttpRequest" },
                });

            var jsonObj = JObject.Parse(encHtmlData);
            var sources = CryptoHandler(jsonObj["data"].ToString(), false);

            sources = sources.Replace(@"o""<P{#meme"":""", @"e"":[{""file"":""");

            var source = JObject.Parse(sources)["source"].ToString();
            var sourcesList = JArray.Parse(source);

            var list = sourcesList.Select(x =>
            {
                return new Quality()
                {
                    QualityUrl = x["file"].ToString(),
                    Resolution = x["label"].ToString(),
                    FileType = x["type"].ToString()
                };
            }).ToList();

            return list;
        }

        private string CryptoHandler(string dataValue, bool encrypt = true)
        {
            var key = Encoding.UTF8.GetBytes("25716538522938396164662278833288");
            var iv = Encoding.UTF8.GetBytes("1285672985238393");

            var cryptoProvider = new RijndaelManaged();
            cryptoProvider.Mode = CipherMode.CBC;
            cryptoProvider.Padding = PaddingMode.PKCS7;

            if (encrypt)
            {
                // Convert from Base64 to binary
                byte[] bytIn = Encoding.ASCII.GetBytes(dataValue);

                var padding = new byte[] { 0x8, 0xe, 0x3, 0x8, 0x9, 0x3, 0x4, 0x9 };
                bytIn = bytIn.Concat(padding).ToArray();

                // Create a MemoryStream
                MemoryStream ms = new MemoryStream();

                // Create Crypto Stream that encrypts a stream
                CryptoStream cs = new CryptoStream(ms,
                    cryptoProvider.CreateEncryptor(key, iv),
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
                MemoryStream ms = new MemoryStream(bytIn, 0, bytIn.Length);

                // Create a CryptoStream that decrypts the data
                CryptoStream cs = new CryptoStream(ms,
                    cryptoProvider.CreateDecryptor(key, iv),
                    CryptoStreamMode.Read);

                // Read the Crypto Stream
                StreamReader sr = new StreamReader(cs);

                return sr.ReadToEnd();
            }
        }
    }
}