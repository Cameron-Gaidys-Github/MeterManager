using System;
using System.ComponentModel.DataAnnotations;

namespace MeterManager.Models
{
    public class EtherMeterReading
    {
        [Key]
        public long ID { get; set; }
        public DateTime Timestamp { get; set; }
        public string MeterID { get; set; } = string.Empty;
        public bool NetworkFault { get; set; }
        public bool MeterFault { get; set; }
        public long MeterReading { get; set; }
        public long? HighFlowReading { get; set; }
        public double? FlowRate { get; set; }
    }
}
