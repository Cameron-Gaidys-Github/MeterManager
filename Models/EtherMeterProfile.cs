using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Net.NetworkInformation;   

namespace MeterManager.Models
{
    public class EtherMeterProfile
    {
        [Key]
        public int MeterID { get; set; }
        public string IPAddress { get; set; } = string.Empty;
        public string UDPPort { get; set; } = string.Empty;
        public string EMChannel { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public bool IsActive { get; set; }

        [NotMapped]
        public bool IsNetworkOnline { get; set; }

        [NotMapped]
        public bool IsMeterOnline { get; set; }

    }
}
