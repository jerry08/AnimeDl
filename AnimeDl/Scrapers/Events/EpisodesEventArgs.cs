using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimeDl.Scrapers.Events
{
    public class EpisodesEventArgs : EventArgs
    {
        public Anime Anime { get; set; }
        public List<Episode> Episodes { get; set; } = new List<Episode>();
    }
}
