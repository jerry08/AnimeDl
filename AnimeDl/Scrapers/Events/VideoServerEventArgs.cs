using System;
using System.Collections.Generic;
using AnimeDl.Models;

namespace AnimeDl.Scrapers.Events;

public class VideoServerEventArgs : EventArgs
{
    public List<VideoServer> VideoServers { get; set; } = new();

    public VideoServerEventArgs(List<VideoServer> servers)
    {
        VideoServers = servers;
    }
}