﻿using Newtonsoft.Json;

namespace AnimeDl.Anilist.Api;

public class FuzzyDate
{
    [JsonProperty("year")]
    public int? Year { get; set; }

    [JsonProperty("month")]
    public int? Month { get; set; }

    [JsonProperty("day")]
    public int? Day { get; set; }
}