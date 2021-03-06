using System;

namespace AnimeDl;

public class Episode
{
    public int Id { get; set; }

    public int AnimeId { get; set; }

    public string EpisodeName { get; set; } = default!;

    public string Description { get; set; } = default!;

    public float EpisodeNumber { get; set; }

    public string EpisodeLink { get; set; } = default!;

    public string Image { get; set; } = default!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}