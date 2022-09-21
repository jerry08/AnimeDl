using System;

namespace AnimeDl;

/// <summary>
/// The Class which contains all the information about an Episode
/// </summary>
public class Episode
{
    public int Id { get; set; }

    public int AnimeId { get; set; }

    public string Name { get; set; } = default!;

    public string Description { get; set; } = default!;

    public float Number { get; set; }

    public string Link { get; set; } = default!;

    public string Image { get; set; } = default!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}