using AnimeDl.Models;
using System;
using System.Collections.Generic;

namespace AnimeDl.Scrapers.Events;

public class AnimeEventArgs : EventArgs
{
    public List<Anime> Animes { get; private set; } = new();

    public AnimeEventArgs(List<Anime> animes)
    {
        Animes = animes;
    }
}