using System;
using System.Collections.Generic;

namespace AnimeDl.Scrapers.Events;

public class VideoEventArgs : EventArgs
{
    public List<Video> Videos { get; set; } = new();

    public VideoEventArgs(List<Video> videos)
    {
        Videos = videos;
    }
}
