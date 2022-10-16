using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Nager.PublicSuffix;
using Newtonsoft.Json.Linq;
using AnimeDl.Models;
using AnimeDl.Extractors;
using AnimeDl.Exceptions;
using AnimeDl.Utils.Extensions;
using AnimeDl.Extractors.Interfaces;

namespace AnimeDl.Scrapers;

/// <summary>
/// Scraper for interacting with Jutsu.
/// </summary>

public class JutsuScraper : BaseScraper
{
    public override string Name { get; set; } = "Gogo";

    public override bool IsDubAvailableSeparately { get; set; } = true;

    private string _baseUrl { get; set; } = default!;

    public override string BaseUrl => _baseUrl;

    public string CdnUrl { get; private set; } = default!;

    public JutsuScraper(HttpClient http) : base(http)
    {
        SetData();
    }

    public void SetData()
    {
        var json = _http.Get("https://raw.githubusercontent.com/jerry08/AnimeDl/master/AnimeDl/Data/jutsu.json");

        if (!string.IsNullOrEmpty(json))
        {
            var jObj = JObject.Parse(json);

            _baseUrl = jObj[BaseUrl]!.ToString();
        }
    }
}

