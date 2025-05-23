using System;
using System.Collections.Generic;

namespace MeterManager.Models
{
    public class WaterReadingViewModel
    {
        public DateTime TimeStamp { get; set; }
        public Dictionary<string, long> Readings { get; set; } = new();
        public Dictionary<string, long> Differences { get; set; } = new();
    }
}
