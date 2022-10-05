using System.Collections.Generic;
using AnimeDl.Anilist.Models;

namespace AnimeDl.Anilist;

public class SearchResults
{
    public string Type { get; set; } = default!;

    public bool IsAdult { get; set; }

    public bool? OnList { get; set; }

    public int? PerPage { get; set; }

    public string? Search { get; set; }

    public string? Sort { get; set; }

    public List<string>? Genres { get; set; }

    public List<string>? Tags { get; set; }

    public string? Format { get; set; }

    public int Page { get; set; } = 1;

    public List<Media> Results { get; set; } = default!;

    public bool HasNextPage { get; set; }
}