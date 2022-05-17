using System;
using System.Collections.Generic;

namespace AnimeDl.Scrapers.Events
{
    public class GenreEventArgs : EventArgs
    {
        public List<Genre> Genres { get; set; } = new List<Genre>();

        public GenreEventArgs(List<Genre> genres)
        {
            Genres = genres;
        }
    }
}
