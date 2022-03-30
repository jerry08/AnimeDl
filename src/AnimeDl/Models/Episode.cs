using System;

namespace AnimeDl
{
    public class Episode
    {
        public int Id { get; set; }
        public int AnimeId { get; set; }
        public string EpisodeName { get; set; }
        public int EpisodeNumber { get; set; }
        public string EpisodeLink { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}