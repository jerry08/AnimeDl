using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimeDl.Scrapers.Events
{
    public class AnimeEventArgs : EventArgs
    {
        public List<Anime> Animes { get; set; } = new List<Anime>();
        //public List<Download> Downloads { get; set; } = new List<Download>();
        //public List<Genre> Genres { get; set; } = new List<Genre>();

        public AnimeEventArgs()
        {

        }

        public AnimeEventArgs(List<Anime> animes)
        {
            Animes = animes;
        }
    }
}
