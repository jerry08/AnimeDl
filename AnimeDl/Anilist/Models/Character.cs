using System.Collections.Generic;
using AnimeDl.Anilist.Api;

namespace AnimeDl.Anilist.Models;

public class Character
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Image { get; set; }
    public string? Banner { get; set; }
    public string? Role { get; set; }

    public string? description { get; set; }
    public string? age { get; set; }
    public string? gender { get; set; }
    public FuzzyDate? dateOfBirth { get; set; }
    public List<Media>? Roles { get; set; }
}