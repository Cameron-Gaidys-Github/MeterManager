namespace MeterManager.Models
{
    public class MeterWithStatus
    {
        public EtherMeterProfile? Meter { get; set; }
        public bool IsNetworkOnline { get; set; }
        public bool IsMeterOnline { get; set; }
        public bool NetworkFault { get; set; }
        public bool MeterFault { get; set; }
    }
}