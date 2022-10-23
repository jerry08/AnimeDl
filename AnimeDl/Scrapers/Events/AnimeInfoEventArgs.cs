using System;
using AnimeDl.Models;

namespace AnimeDl.Scrapers.Events;

public class AnimeInfoEventArgs : EventArgs
{
    public Anime Anime { get; private set; } = new();

    public AnimeInfoEventArgs(Anime anime)
    {
        Anime = anime;
    }
}