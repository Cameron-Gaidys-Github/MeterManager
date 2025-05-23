using Microsoft.EntityFrameworkCore;
using MeterManager.Models;

namespace MeterManager.Data
{
    public class SBCorpInetDbContext : DbContext
    {
        public SBCorpInetDbContext(DbContextOptions<SBCorpInetDbContext> options)
            : base(options)
        {
        }

        public DbSet<EtherMeterProfile> EtherMeterProfiles { get; set; }
        public DbSet<EtherMeterThreshold> EtherMeterThresholds { get; set; }
        public DbSet<EtherMeterReading> EtherMeterReadings { get; set; } 
        public DbSet<EthermeterThresholdOffset> EtherMeterThresholdOffsets { get; set; }
    }
}
