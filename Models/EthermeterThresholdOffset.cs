using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeterManager.Models
{
    [Table("EtherMeterThresholdOffset")]
    public class EthermeterThresholdOffset
    {
        [Key]
        public int MeterID { get; set; }

        [Column("Christmas Week")]
        public double ChristmasWeek { get; set; }

        [Column("MLK Week")]
        public double MLKWeek { get; set; }

        [Column("Presidents Week")]
        public double PresidentsWeek { get; set; }

        public double January { get; set; }
        public double February { get; set; }
        public double March { get; set; }
        public double April { get; set; }
        public double May { get; set; }
        public double June { get; set; }
        public double July { get; set; }
        public double August { get; set; }
        public double September { get; set; }
        public double October { get; set; }
        public double November { get; set; }
        public double December { get; set; }
    }
}