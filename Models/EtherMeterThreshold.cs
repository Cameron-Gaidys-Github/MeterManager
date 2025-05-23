using System.ComponentModel.DataAnnotations;

namespace MeterManager.Models
{
    public class EtherMeterThreshold
    {
        [Key]
        public int ThresholdID { get; set; }
        public int MeterID { get; set; }
        public string MeterName { get; set; } = string.Empty;

        public string Hour1 { get; set; } = string.Empty;
        public string Hour2 { get; set; } = string.Empty;
        public string Hour3 { get; set; } = string.Empty;
        public string Hour4 { get; set; } = string.Empty;
        public string Hour5 { get; set; } = string.Empty;
        public string Hour6 { get; set; } = string.Empty;
        public string Hour7 { get; set; } = string.Empty;
        public string Hour8 { get; set; } = string.Empty;
        public string Hour9 { get; set; } = string.Empty;
        public string Hour10 { get; set; } = string.Empty;
        public string Hour11 { get; set; } = string.Empty;
        public string Hour12 { get; set; } = string.Empty;
        public string Hour13 { get; set; } = string.Empty;
        public string Hour14 { get; set; } = string.Empty;
        public string Hour15 { get; set; } = string.Empty;
        public string Hour16 { get; set; } = string.Empty;
        public string Hour17 { get; set; } = string.Empty;
        public string Hour18 { get; set; } = string.Empty;
        public string Hour19 { get; set; } = string.Empty;
        public string Hour20 { get; set; } = string.Empty;
        public string Hour21 { get; set; } = string.Empty;
        public string Hour22 { get; set; } = string.Empty;
        public string Hour23 { get; set; } = string.Empty;
        public string Hour24 { get; set; } = string.Empty;
    }

}
