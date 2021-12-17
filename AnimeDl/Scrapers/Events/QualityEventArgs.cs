using System;
using System.Collections.Generic;

namespace AnimeDl.Scrapers.Events
{
    public class QualityEventArgs : EventArgs
    {
        public List<Quality> Qualities { get; set; } = new List<Quality>();

        public QualityEventArgs(List<Quality> qualities)
        {
            Qualities = qualities;
        }
    }
}
