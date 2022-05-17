using AnimeDl.Extractors;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nager.PublicSuffix;

namespace AnimeDl.Scrapers
{
    public class NineAnimeScraper : BaseScraper
    {
        public override string BaseUrl => "https://9anime.center";

        public override async Task<List<Anime>> SearchAsync(string query, 
            SearchType searchType = SearchType.Find, int Page = 1)
        {
            List<Anime> animes = new List<Anime>();

            string htmlData = "";

            switch (searchType)
            {
                case SearchType.Find:
                    htmlData = await Http.GetHtmlAsync($"{BaseUrl}/filter?sort=title%3Aasc&keyword={query}");
                    break;
                case SearchType.Popular:
                    htmlData = await Http.GetHtmlAsync($"{BaseUrl}/popular.html?page=" + Page);
                    break;
                case SearchType.NewSeason:
                    htmlData = await Http.GetHtmlAsync($"{BaseUrl}/new-season.html?page=" + Page);
                    break;
                case SearchType.LastUpdated:
                    htmlData = await Http.GetHtmlAsync($"{BaseUrl}/ajax/home/widget?name=updated_all");
                    break;
                case SearchType.Trending:
                    htmlData = await Http.GetHtmlAsync($"{BaseUrl}/ajax/home/widget?name=trending");
                    break;
                case SearchType.AllList:
                    //htmlData = await Http.GetHtmlAsync($"https://animesa.ga/animel.php");
                    htmlData = await Http.GetHtmlAsync($"https://animefrenzy.org/anime");
                    break;
                default:
                    break;
            }

            if (string.IsNullOrEmpty(htmlData))
                return animes;

            HtmlDocument document = new HtmlDocument();
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
        private string key = "0wMrYU+ixjJ4QdzgfN2HlyIVAt3sBOZnCT9Lm7uFDovkb/EaKpRWhqXS5168ePcG";
        private string GetVrf(string id)
        {
            string reversed = new string(ue(Encode(id) + "0000000").Take(6).Reverse().ToArray());
            reversed += Regex.Replace(ue(je(reversed, Encode(id))), @"""=+$""", "");
            return reversed;
        }

        private string GetLink(string url)
        {
            var i = url.Substring(0, 6);
            var n = url.Substring(6);
            return Decode(je(i, ze(n)));
        }

        private string ue(string input)
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

        private string je(string input1, string input2)
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

        private string ze(string input)
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

            string htmlData = await Http.GetHtmlAsync($"{BaseUrl}" + anime.Link);

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(htmlData);

            string animeId = document.DocumentNode
                .SelectSingleNode(".//div[@class='player-wrapper watchpage']")
                .Attributes["data-id"].Value;

            string animeidencoded = Encode(GetVrf(animeId));

            var epsHtml = await Http.GetHtmlAsync($"{BaseUrl}/ajax/anime/servers?ep=1&id={animeId}&vrf={animeidencoded}&ep=8&episode=&token=");

            document = new HtmlDocument();
            document.LoadHtml(JObject.Parse(epsHtml)["html"].ToString());

            var epNodes = document.DocumentNode
                .SelectNodes(".//ul[@class='episodes']/li/a").ToList();

            for (int i = 0; i < epNodes.Count; i++)
            {
                int epNum = Convert.ToInt32(epNodes[i].InnerText);
                string link = epNodes[i].Attributes["href"]?.Value;
                string name = $"Episode {epNum}";

                episodes.Add(new Episode() 
                {
                    EpisodeLink = link,
                    EpisodeName = name,
                    EpisodeNumber = epNum
                });
            }

            return episodes;
        }

        public override async Task<List<Quality>> GetEpisodeLinksAsync(Episode episode)
        {
            string htmlData = await Http.GetHtmlAsync(episode.EpisodeLink);

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(htmlData);

            string animeId = document.DocumentNode
                .SelectSingleNode(".//div[@class='player-wrapper watchpage']")
                .Attributes["data-id"].Value;

            string animeidencoded = Encode(GetVrf(animeId));

            //string ll = $"{BaseUrl}/ajax/anime/servers?ep=1&id={animeId}&vrf={animeidencoded}&ep=8&episode=&token=";

            string epsHtml = await Http.GetHtmlAsync($"{BaseUrl}/ajax/anime/servers?ep=1&id={animeId}&vrf={animeidencoded}&ep=8&episode=&token=");

            document = new HtmlDocument();
            document.LoadHtml(JObject.Parse(epsHtml)["html"].ToString());

            var element = document.DocumentNode
                .SelectSingleNode(".//div[@class='body']");

            //var jsonregex = new Regex(@"(\\{.+\\}.*$data)");
            //var m = jsonregex.Matches(element.InnerHtml);

            //var gs = new Uri(ll).DecodeQueryParameters();

            string sources = document.DocumentNode
                .SelectNodes($".//ul[@class='episodes']/li/a")
                .Where(x => x?.Attributes["data-base"]?.Value == episode.EpisodeNumber.ToString())
                .FirstOrDefault()?.Attributes["data-sources"]?.Value;

            string sourceId = JObject.Parse(sources)["41"].ToString();

            var epServer = await Http.GetHtmlAsync($"{BaseUrl}/ajax/anime/episode?id={sourceId}");

            var headers = new WebHeaderCollection()
            {
                { "Referer", $"{BaseUrl}/" }
            };

            string encryptedSourceUrl = JObject.Parse(epServer)["url"].ToString().Replace("=", "");
            string embedLink = GetLink(encryptedSourceUrl)?.Replace("/embed/", "/e/");

            string embedHtml = await Http.GetHtmlAsync(embedLink, headers);

            document.LoadHtml(embedHtml);

            var skeyScript = document.DocumentNode.Descendants()
                .Where(x => x.Name == "script" && x.InnerHtml.Contains("window.skey = "))
                .FirstOrDefault();

            string skey = skeyScript.InnerText.SubstringAfter("window.skey = \'").SubstringBefore("\'");

            string sourceObjectLink = GetLink(encryptedSourceUrl)?.Replace("/embed/", "/info/") + $"?skey={skey}";
            var sourceObjectHtml = await Http.GetHtmlAsync(sourceObjectLink, headers);
            var sourceObject = JObject.Parse(sourceObjectHtml)["media"]?["sources"];

            var masterUrls = sourceObject.Select(x => x["file"]?.ToString()).ToList();
            string masterUrl = masterUrls.Where(x => !x.Contains("/simple/")).FirstOrDefault();

            var domainParser = new DomainParser(new WebTldRuleProvider());

            //var domainInfo = domainParser.Parse("sub.test.co.uk");
            //domainInfo.Domain = "test";
            //domainInfo.Hostname = "sub.test.co.uk";
            //domainInfo.RegistrableDomain = "test.co.uk";
            //domainInfo.SubDomain = "sub";
            //domainInfo.TLD = "co.uk";

            var domainInfo = domainParser.Parse(masterUrl);
            string origin = domainInfo.RegistrableDomain;

            string masterPlaylist = await Http.GetHtmlAsync(masterUrl, headers);
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
}