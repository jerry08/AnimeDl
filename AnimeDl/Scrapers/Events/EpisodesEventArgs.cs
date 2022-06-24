using System;
using System.Collections.Generic;

namespace AnimeDl.Scrapers.Events;

public class EpisodesEventArgs : EventArgs
{
    public Anime Anime { get; private set; } = default!;

    public List<Episode> Episodes { get; private set; } = new();

    public EpisodesEventArgs(Anime anime, List<Episode> episodes)
    {
        Anime = anime;
        Episodes = episodes;
    }
}