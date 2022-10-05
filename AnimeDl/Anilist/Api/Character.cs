using System.Collections.Generic;
using Newtonsoft.Json;

namespace AnimeDl.Anilist.Api;

public class Character
{
    /// <summary>
    /// The id of the character
    /// </summary>
    [JsonProperty("id")]
    public int Id { get; set; }

    /// <summary>
    /// The names of the character
    /// </summary>
    [JsonProperty("name")]
    public CharacterName? Name { get; set; }

    /// <summary>
    /// Character images
    /// </summary>
    [JsonProperty("image")]
    public CharacterImage? Image { get; set; }

    /// <summary>
    /// A general description of the character
    /// </summary>
    [JsonProperty("description")]
    public string? Description { get; set; }

    /// <summary>
    /// The character's gender. Usually Male, Female, or Non-binary but can be any string.
    /// </summary>
    [JsonProperty("gender")]
    public string? gender { get; set; }
}

public class CharacterConnection
{
    [JsonProperty("edges")]
    public List<CharacterEdge>? Edges { get; set; }

    [JsonProperty("nodes")]
    public List<Character>? Nodes { get; set; }
}

public class CharacterEdge
{
    [JsonProperty("node")]
    public Character? Node { get; set; }

    /// <summary>
    /// The id of the connection
    /// </summary>
    [JsonProperty("id")]
    public int? Id { get; set; }

    /// <summary>
    /// The characters role in the media
    /// </summary>
    [JsonProperty("role")]
    public string? Role { get; set; }

    /// <summary>
    /// Media specific character name
    /// </summary>
    [JsonProperty("name")]
    public string? Name { get; set; }

    /// <summary>
    /// The media the character is in
    /// </summary>
    [JsonProperty("media")]
    public List<Media>? Media { get; set; }

    /// <summary>
    /// The order the character should be displayed from the users favourites
    /// </summary>
    [JsonProperty("favouriteOrder")]
    public int? FavouriteOrder { get; set; }
}

public class CharacterName
{
    /// <summary>
    /// The character's given name
    /// </summary>
    [JsonProperty("first")]
    public string? First { get; set; }

    /// <summary>
    /// The character's middle name
    /// </summary>
    [JsonProperty("middle")]
    public string? Middle { get; set; }

    /// <summary>
    /// The character's surname
    /// </summary>
    [JsonProperty("last")]
    public string? Last { get; set; }

    /// <summary>
    /// The character's first and last name
    /// </summary>
    [JsonProperty("full")]
    public string? Full { get; set; }

    /// <summary>
    /// The character's full name in their native language
    /// </summary>
    [JsonProperty("native")]
    public string? Native { get; set; }

    /// <summary>
    /// Other names the character might be referred to as
    /// </summary>
    [JsonProperty("alternative")]
    public List<string>? Alternative { get; set; }

    /// <summary>
    /// Other names the character might be referred to as but are spoilers
    /// </summary>
    [JsonProperty("alternativeSpoiler")]
    public List<string>? AlternativeSpoiler { get; set; }

    /// <summary>
    /// The currently authenticated users preferred name language. Default romaji for non-authenticated
    /// </summary>
    [JsonProperty("userPreferred")]
    public string? UserPreferred { get; set; }
}

public class CharacterImage
{
    /// <summary>
    /// The character's image of media at its largest size
    /// </summary>
    [JsonProperty("large")]
    public string? Large { get; set; }

    /// <summary>
    /// The character's image of media at medium size
    /// </summary>
    [JsonProperty("medium")]
    public string? Medium { get; set; }
}