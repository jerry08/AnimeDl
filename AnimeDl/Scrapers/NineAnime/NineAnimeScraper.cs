using System;
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

namespace AnimeDl.Scrapers;

internal class NineAnimeScraper : BaseScraper
{
    //public override string BaseUrl => "https://9anime.center";
    public override string BaseUrl => "https://9anime.pl";

    public NineAnimeScraper(NetHttpClient netHttpClient) : base(netHttpClient)
    {
    }

    public override async Task<List<Anime>> SearchAsync(
        string searchQuery,
        SearchFilter searchFilter,
        int page)
    {
        var animes = new List<Anime>();

        var htmlData = searchFilter switch
        {
            SearchFilter.Find => await _netHttpClient.SendHttpRequestAsync($"{BaseUrl}/filter?sort=title%3Aasc&keyword={searchQuery}"),
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

        foreach (var node in document.DocumentNode.SelectNodes(".//ul[@class='anime-list']/li"))
        {
            string title = node.SelectSingleNode(".//a[@class='name']").InnerText;
            string href = node.SelectSingleNode(".//a[@href]").Attributes["href"].Value;
            string image = node.SelectSingleNode(".//a[@class='poster']/img").Attributes["src"].Value;

            href = new Regex(@"(\\?ep=(\\d+)\$)").Replace(href, "");

            animes.Add(new Anime()
            {
                Title = title,
                EpisodesNum = 0,
                Link = href,
                Image = image
            });
        }

        return animes;
    }

    //Credits to https://github.com/jmir1
    //private string key = "0wMrYU+ixjJ4QdzgfN2HlyIVAt3sBOZnCT9Lm7uFDovkb/EaKpRWhqXS5168ePcG";

    //thanks to @Modder4869 for key
    private string key = "c/aUAorINHBLxWTy3uRiPt8J+vjsOheFG1E0q2X9CYwDZlnmd4Kb5M6gSVzfk7pQ";
    
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

        string output = "";

        for (int i = 0; i < input.Length; i++)
        {
            if (i % 3 != 0)
            {
                continue;
            }

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
            c = (c + f) % 256;
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
        string r = "";
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
        return Uri.EscapeDataString(id);
    }

    private string Decode(string id)
    {
        return Uri.UnescapeDataString(id);
    }

    public override async Task<List<Episode>> GetEpisodesAsync(Anime anime)
    {
        List<Episode> episodes = new List<Episode>();

        string htmlData = await _netHttpClient.SendHttpRequestAsync($"{BaseUrl}" + anime.Link);

        var document = new HtmlDocument();
        document.LoadHtml(htmlData);

        string animeId = document.DocumentNode
            .SelectSingleNode(".//div[@class='player-wrapper watchpage']")
            .Attributes["data-id"].Value;

        string animeidencoded = Encode(GetVrf(animeId));

        //var epsHtml = await _netHttpClient.SendHttpRequestAsync($"{BaseUrl}/ajax/anime/servers?ep=1&id={animeId}&vrf={animeidencoded}&ep=8&episode=&token=");
        var epsHtml = await _netHttpClient.SendHttpRequestAsync($"{BaseUrl}/ajax/anime/servers?id={animeId}&vrf={animeidencoded}");

        document = new HtmlDocument();
        document.LoadHtml(JObject.Parse(epsHtml)["html"]!.ToString());

        var epNodes = document.DocumentNode
            .SelectNodes(".//ul[@class='episodes']/li/a").ToList();

        for (int i = 0; i < epNodes.Count; i++)
        {
            int epNum = Convert.ToInt32(epNodes[i].InnerText);
            var link = epNodes[i].Attributes["href"]?.Value;
            var name = $"Episode {epNum}";

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
        string htmlData = await _netHttpClient.SendHttpRequestAsync(episode.EpisodeLink);

        var document = new HtmlDocument();
        document.LoadHtml(htmlData);

        string animeId = document.DocumentNode
            .SelectSingleNode(".//div[@class='player-wrapper watchpage']")
            .Attributes["data-id"].Value;

        string animeidencoded = Encode(GetVrf(animeId));

        //string ll = $"{BaseUrl}/ajax/anime/servers?ep=1&id={animeId}&vrf={animeidencoded}&ep=8&episode=&token=";

        string epsHtml = await _netHttpClient.SendHttpRequestAsync($"{BaseUrl}/ajax/anime/servers?ep=1&id={animeId}&vrf={animeidencoded}&ep=8&episode=&token=");

        document = new HtmlDocument();
        document.LoadHtml(JObject.Parse(epsHtml)["html"]!.ToString());

        var element = document.DocumentNode
            .SelectSingleNode(".//div[@class='body']");

        //var jsonregex = new Regex(@"(\\{.+\\}.*$data)");
        //var m = jsonregex.Matches(element.InnerHtml);

        //var gs = new Uri(ll).DecodeQueryParameters();

        var sources = document.DocumentNode
            .SelectNodes($".//ul[@class='episodes']/li/a")
            .Where(x => x?.Attributes["data-base"]?.Value == episode.EpisodeNumber.ToString())
            .FirstOrDefault()?.Attributes["data-sources"]?.Value;

        //var sourceId = JObject.Parse(sources)["41"].ToString();
        var sourceId = JObject.Parse(sources!)["28"]!.ToString();

        var epServer = await _netHttpClient.SendHttpRequestAsync($"{BaseUrl}/ajax/anime/episode?id={sourceId}");

        var headers = new WebHeaderCollection()
        {
            { "Referer", $"{BaseUrl}/" }
        };

        var encryptedSourceUrl = JObject.Parse(epServer)["url"]!.ToString().Replace("=", "");
        var embedLink = GetLink(encryptedSourceUrl)?.Replace("/embed/", "/e/")!;

        //await new VizCloud(this).ExtractQualities(embedLink);

        var embedHtml = await _netHttpClient.SendHttpRequestAsync(embedLink, headers);

        document.LoadHtml(embedHtml);

        var skeyScript = document.DocumentNode.Descendants()
            .Where(x => x.Name == "script" && x.InnerHtml.Contains("window.skey = "))
            .FirstOrDefault();

        var skey = skeyScript?.InnerText.SubstringAfter("window.skey = \'").SubstringBefore("\'");

        var sourceObjectLink = GetLink(encryptedSourceUrl)?.Replace("/embed/", "/info/") + $"?skey={skey}";
        var sourceObjectHtml = await _netHttpClient.SendHttpRequestAsync(sourceObjectLink, headers);
        var sourceObject = JObject.Parse(sourceObjectHtml)["media"]?["sources"];

        var masterUrls = sourceObject!.Select(x => x["file"]?.ToString()).ToList();
        var masterUrl = masterUrls!.Where(x => !x!.Contains("/simple/")).FirstOrDefault()!;

        var domainParser = new DomainParser(new WebTldRuleProvider());

        //var domainInfo = domainParser.Parse("sub.test.co.uk");
        //domainInfo.Domain = "test";
        //domainInfo.Hostname = "sub.test.co.uk";
        //domainInfo.RegistrableDomain = "test.co.uk";
        //domainInfo.SubDomain = "sub";
        //domainInfo.TLD = "co.uk";

        var domainInfo = domainParser.Parse(masterUrl);
        var origin = domainInfo.RegistrableDomain;

        var masterPlaylist = await _netHttpClient.SendHttpRequestAsync(masterUrl, headers);
        var playlists = masterPlaylist.SubstringAfter("#EXT-X-STREAM-INF:").Split(new string[] { "#EXT-X-STREAM-INF:" }, StringSplitOptions.None);

        var list = new List<Quality>();

        for (int i = 0; i < playlists.Length; i++)
        {
            string quality = playlists[i].SubstringAfter("RESOLUTION=").SubstringAfter("x").SubstringBefore("\n") + "p";
            var split = masterUrl.Split('/');
            string videoUrl = string.Join("/", split.Take(split.Length - 1)) + "/" + playlists[i].SubstringAfter("\n").SubstringBefore("\n");

            list.Add(new Quality()
            {
                Resolution = quality,
                QualityUrl = videoUrl,
                Headers = new WebHeaderCollection()
                {
                    { "Referer", $"{BaseUrl}/" },
                    { "Origin", "https://" + origin }
                }
            });
        }

        //return new List<Quality>()
        //{
        //    new Quality()
        //    {
        //        QualityUrl = embedLink,
        //        Headers = new WebHeaderCollection()
        //        {
        //            { "Referer", $"{BaseUrl}/" }
        //        }
        //    }
        //};

        return list;
    }
}