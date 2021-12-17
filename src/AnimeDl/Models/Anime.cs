using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnimeDl
{
    public class Anime
    {
        public int Id { get; set; }

        public string Title { get; set; }
        public string AltTitle { get; set; }
        public int Season { get; set; }
        public bool Ongoing { get; set; }
        public int Hb_Id { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int Mal_Id { get; set; }
        public bool Hidden { get; set; }
        public string Slug { get; set; }

        public string Released { get; set; }
        public int EpisodesNum { get; set; }
        public string Category { get; set; }
        public string Link { get; set; }
        public string Image { get; set; }
        public int LastWatchedEp { get; set; }

        public string Type { get; set; }
        public string Status { get; set; }
        public string OtherNames { get; set; }
        public string Summary { get; set; }
        public string Genre { get; set; }
    }
}