using AnimeDl.Models;
using System;
using System.Collections.Generic;

namespace AnimeDl.Scrapers.Events;

public class GenreEventArgs : EventArgs
{
    public List<Genre> Genres { get; private set; } = new();

    public GenreEventArgs(List<Genre> genres)
    {
        Genres = genres;
    }
}