using System;
using System.Collections.Generic;
using AnimeDl.Models;

namespace AnimeDl.Scrapers.Events;

public class VideoEventArgs : EventArgs
{
    public List<Video> Videos { get; set; } = new();
    
    public VideoServer VideoServer { get; set; } = default!;

    public VideoEventArgs(List<Video> videos, VideoServer videoServer)
    {
        Videos = videos;
        VideoServer = videoServer;
    }
}
