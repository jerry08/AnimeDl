using Newtonsoft.Json;
using System.Collections.Generic;

namespace AnimeDl.Aniskip;

public class AniSkipResponse
{
    [JsonProperty("found")]
    public bool IsFound { get; set; }

    public List<Stamp>? Results { get; set; }

    public string? Message { get; set; }

    public int StatusCode { get; set; }
}