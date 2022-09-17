﻿using System;
using System.Linq;
using System.Net;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using Nager.PublicSuffix;
using AnimeDl.Extractors;
using AnimeDl.Utils.Extensions;
using AnimeDl.Exceptions;
using System.Text;
using System.Diagnostics;
using System.Collections;

namespace AnimeDl.Scrapers;

internal class NineAnimeScraper : BaseScraper
{
    public override string Name { get; set; } = "9anime";

    public override bool IsDubAvailableSeparately { get; set; } = true;

    //public override string BaseUrl => "https://9anime.center";
    //public override string BaseUrl => "https://9anime.pl";
    public override string BaseUrl => "https://9anime.id";

    public NineAnimeScraper(NetHttpClient netHttpClient) : base(netHttpClient)
    {
    }

    public override async Task<List<Anime>> SearchAsync(
        string query,
        SearchFilter searchFilter,
        int page,
        bool selectDub)
    {
        var animes = new List<Anime>();

        var htmlData = searchFilter switch
        {
            SearchFilter.Find => await _netHttpClient.SendHttpRequestAsync($"{BaseUrl}/filter?sort=title%3Aasc&keyword={query}"),
            SearchFilter.Popular => await _netHttpClient.SendHttpRequestAsync($"{BaseUrl}/popular.html?page=" + page),
            SearchFilter.NewSeason => await _netHttpClient.SendHttpRequestAsync($"{BaseUrl}/new-season.html?page=" + page),
            SearchFilter.LastUpdated => await _netHttpClient.SendHttpRequestAsync($"{BaseUrl}/ajax/home/widget?name=updated_all"),
            SearchFilter.Trending => await _netHttpClient.SendHttpRequestAsync($"{BaseUrl}/ajax/home/widget?name=trending"),
            SearchFilter.AllList => await _netHttpClient.SendHttpRequestAsync($"https://animefrenzy.org/anime"),
            //SearchFilter.AllList => await _netHttpClient.SendHttpRequestAsync($"https://animesa.ga/animel.php");
            _ => throw new SearchFilterNotSupportedException("Search filter not supported"),
        };

        if (htmlData is null)
        {
            return animes;
        }

        var document = new HtmlDocument();
        document.LoadHtml(htmlData);

        var listNode = document.DocumentNode.SelectSingleNode(".//div[@id='list-items']");

        foreach (var node in listNode.SelectNodes(".//div[@class='ani poster tip']/a"))
        {
            var image = node.SelectSingleNode(".//img");

            var title = image.Attributes["alt"].Value;
            var href = BaseUrl + node.Attributes["href"].Value;
            var cover = image.Attributes["src"].Value;

            href = new Regex(@"(\\?ep=(\\d+)\$)").Replace(href, "");

            animes.Add(new Anime()
            {
                Site = AnimeSites.NineAnime,
                Title = title,
                EpisodesNum = 0,
                Link = href,
                Image = cover
            });
        }

        return animes;
    }

    //private string key = "c/aUAorINHBLxWTy3uRiPt8J+vjsOheFG1E0q2X9CYwDZlnmd4Kb5M6gSVzfk7pQ";
    private string key = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

    private string cipherKey = "kMXzgyNzT3k5dYab";

    private string EncodeVrf(string text)
    {
        //return Encode(GetVrf(text));
        //return Encode(encrypt(cipher(cipherKey, Encode(text))));
        return encrypt(cipher(Encode(text), cipherKey));
    }
    
    private string DecodeVrf(string text)
    {
        //return Decode(GetVrf(text));
        return Decode(cipher(cipherKey, decrypt(text)));
        //return GetLink(text);
    }

    private string GetVrf(string id)
    {
        string reversed = new string(encrypt(Encode(id) + "0000000").Take(6).Reverse().ToArray());
        reversed += Regex.Replace(encrypt(cipher(reversed, Encode(id))), @"""=+$""", "");
        return reversed;
    }

    private string GetLink(string url)
    {
        var i = url.Substring(0, 6);
        var n = url.Substring(6);
        return Decode(cipher(i, decrypt(n)));
    }

    //private string ue(string input)
    public string encrypt(string input)
    {
        if (input.Any(x => x >= 256))
            throw new Exception("illegal characters!");

        var output = "";

        for (int i = 0; i < input.Length; i++)
        {
            if (i % 3 != 0)
                continue;

            var a = new int[] { -1, -1, -1, -1 };
            a[0] = input[i] >> 2;
            a[1] = (3 & input[i]) << 4;
            if (input.Length > i + 1)
            {
                a[1] = a[1] ^ (input[i + 1] >> 4);
                a[2] = (15 & input[i + 1]) << 2;
            }
            if (input.Length > i + 2) 
            {
                a[2] = a[2] ^ (input[i + 2] >> 6);
                a[3] = 63 & input[i + 2];
            }
            foreach (int n in a)
            {
                if (n == -1)
                    output += "=";
                else
                {
                    if (n >= 0 && n <= 63)
                        output += key[n];
                }
            }
        }

        return output;
    }

    //private string je(string input1, string input2)
    public string cipher(string input1, string input2)
    {
        var arr = new int[256];
        for (int i = 0; i < arr.Length; i++)
        {
            arr[i] = i;
        }

        string output = "";
        var u = 0;
        int r;

        for (int a = 0; a < arr.Length; a++)
        {
            u = (u + arr[a] + input1[a % input1.Length]) % 256;
            r = arr[a];
            arr[a] = arr[u];
            arr[u] = r;
        }

        u = 0;
        var c = 0;

        for (int f = 0; f < input2.Length; f++)
        {
            c = (c + 1) % 256;
            u = (u + arr[c]) % 256;
            r = arr[c];
            arr[c] = arr[u];
            arr[u] = r;
            output += (char)(input2[f] ^ arr[(arr[c] + arr[u]) % 256]);
        }

        return output;
    }

    //private string ze(string input)
    public string decrypt(string input)
    {
        string t;

        if (Regex.Replace(input, @"""[\t\n\f\r]""", "").Length % 4 == 0)
        {
            t = Regex.Replace(input, @"""/==?$/""", "");
        }
        else
        {
            t = input;
        }

        if (t.Length % 4 == 1 || Regex.Matches(t, @"""[^+/0-9A-Za-z]""").Count > 0)
        {
            throw new Exception("bad input");
        }

        int i;
        var r = "";
        var e = 0;
        var u = 0;

        for (int o = 0; o < input.Length; o++)
        {
            e = e << 6;
            i = key.IndexOf(t[o]);
            e = e ^ i;
            u += 6;
            if (24 == u)
            {
                r += (char)((16711680 & e) >> 16);
                r += (char)((65280 & e) >> 8);
                r += (char)(255 & e);
                e = 0;
                u = 0;
            }
        }

        if (12 == u)
        {
            e = e >> 4;
            return r + (char)e;
        }
        else if (18 == u)
        {
            e = e >> 2;
            r += (char)((65280 & e) >> 8);
            r += (char)(255 & e);
        }

        return r;
    }

    private string Encode(string id)
    {
        //return Uri.EscapeDataString(id);
        byte[] bytes = Encoding.Default.GetBytes(id);
        return Encoding.UTF8.GetString(bytes).Replace("+", "%20");
    }

    private string Decode(string id)
    {
        //return Uri.UnescapeDataString(id);

        byte[] bytes = Encoding.Default.GetBytes(id);
        return Encoding.UTF8.GetString(bytes);
    }

    public override async Task<List<Episode>> GetEpisodesAsync(Anime anime)
    {
        var episodes = new List<Episode>();

        var htmlData = await _netHttpClient.SendHttpRequestAsync(anime.Link);

        var document = new HtmlDocument();
        document.LoadHtml(htmlData);

        var animeId = document.DocumentNode
            .SelectSingleNode(".//div[@id='watch-main']")
            .Attributes["data-id"].Value;

        //var epsHtml = await _netHttpClient.SendHttpRequestAsync($"{BaseUrl}/ajax/anime/servers?ep=1&id={animeId}&vrf={animeidencoded}&ep=8&episode=&token=");
        var epsHtml = await _netHttpClient.SendHttpRequestAsync($"{BaseUrl}/ajax/episode/list/{animeId}?vrf={EncodeVrf(animeId)}");

        document = new HtmlDocument();
        //document.LoadHtml(epsHtml);
        document.LoadHtml(JObject.Parse(epsHtml)["result"]!.ToString());

        var epNodes = document.DocumentNode
            .SelectNodes(".//ul/li/a").ToList();

        for (int i = 0; i < epNodes.Count; i++)
        {
            var id = epNodes[i].Attributes["data-ids"]?.Value.Split(',')[0];

            var epNum = Convert.ToInt32(epNodes[i].Attributes["data-num"].Value);
            var link = $"{BaseUrl}/ajax/server/list/{id}?vrf={id}";
            var title = epNodes[i].SelectNodes(".//span[@class='d-title']")
                .FirstOrDefault()?.InnerText;
            var name = $"Episode {epNum} - {title}";

            episodes.Add(new Episode() 
            {
                EpisodeLink = link!,
                EpisodeName = name,
                EpisodeNumber = epNum
            });
        }

        return episodes;
    }

    public override async Task<List<Quality>> GetEpisodeLinksAsync(Episode episode)
    {
        var content = await _netHttpClient.SendHttpRequestAsync(episode.EpisodeLink);

        var document = new HtmlDocument();
        document.LoadHtml(JObject.Parse(content)["result"]!.ToString());

        var dataLinksIdNodes = document.DocumentNode
            .SelectNodes(".//li").ToList();

        var list = new List<Quality>();

        for (int i = 0; i < dataLinksIdNodes.Count; i++)
        {
            var id = dataLinksIdNodes[i].Attributes["data-link-id"].Value;
            var link = $"{BaseUrl}/ajax/server/{id}?vrf={EncodeVrf(id)}";

            var content2 = await _netHttpClient.SendHttpRequestAsync(link);

            var encodedStreamUrl = JObject.Parse(content2)["result"]?["url"]?.ToString();

            var realLink = DecodeVrf(encodedStreamUrl!);
        }

        return list;
    }
}