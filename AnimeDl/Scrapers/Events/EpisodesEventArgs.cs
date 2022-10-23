using System;
using System.Collections.Generic;
using AnimeDl.Models;

namespace AnimeDl.Scrapers.Events;

public class EpisodesEventArgs : EventArgs
{
    public List<Episode> Episodes { get; private set; } = new();

    public EpisodesEventArgs(List<Episode> episodes)
    {
        Episodes = episodes;
    }
}