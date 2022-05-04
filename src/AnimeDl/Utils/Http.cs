using BrotliSharpLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AnimeDl
{
    internal static class Http
    {
        public const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.113 Safari/537.36";

        private const int NumberOfRetries = 3;
        private const int DelayOnRetry = 500;

        static Http()
        {
            // Increase maximum concurrent connections
            ServicePointManager.DefaultConnectionLimit = 20;

            //System.Net.WebRequest.DefaultWebProxy = null;
        }

        public static string GetHtml(string url, WebHeaderCollection headers = null)
        {
            var task = GetHtmlAsync(url, headers);
            task.Wait();
            return task.Result;
        }

        public async static Task<string> GetHtmlAsync(string url,
            WebHeaderCollection headers = null, IEnumerable<Cookie> cookies = null)
        {
            url = url.Replace(" ", "%20");

            //Exceptions
            if (url.Contains("https://gogoanime") && url.Contains("/category/hataraku-saibou-2"))
            {
                url = url.Replace("hataraku-saibou-2", "hataraku-saibou");
            }

            string html = "";

            for (int i = 1; i <= NumberOfRetries; ++i)
            {
                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    if (headers != null)
                    {
                        //request.Headers = headers;
                        for (int j = 0; j < headers.Count; j++)
                        {
                            request.SetRawHeader(headers.Keys[j], headers[j]);
                        }
                    }

                    if (cookies != null)
                    {
                        request.CookieContainer = new CookieContainer();

                        foreach (var cookie in cookies)
                        {
                            request.CookieContainer.Add(cookie);
                        }
                    }

                    //request.Proxy = null;
                    request.ServerCertificateValidationCallback += delegate { return true; };

                    HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();
                    Stream receiveStream = response.GetResponseStream();
                    StreamReader streamReader = null;
                    BrotliStream brotli = null;

                    //Headers: "Accept-Encoding" - "gzip, deflate, br"
                    //br (Brotli)
                    if (!string.IsNullOrEmpty(response.ContentEncoding) 
                        && response.ContentEncoding.ToLower().Contains("br"))
                    {
                        //Example: Getting Twist Moe episodes uses "br" encoding ("response" variable above)
                        brotli = new BrotliStream(receiveStream, CompressionMode.Decompress, true);
                        streamReader = new StreamReader(brotli);
                    }
                    else if (string.IsNullOrEmpty(response.CharacterSet))
                    {
                        streamReader = new StreamReader(receiveStream);
                    }
                    else
                    {
                        streamReader = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                    }

                    html = await streamReader.ReadToEndAsync();
                    
                    streamReader.Close();
                    brotli?.Close();
                    response.Close();

                    break;
                }
                //catch (Exception e) when (i < NumberOfRetries)
                catch (Exception e)
                {
                    await Task.Delay(DelayOnRetry);
                }
            }

            return html;
        }
    }
}