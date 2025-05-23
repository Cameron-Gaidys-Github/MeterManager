using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MeterManager.Data;
using MeterManager.Models;

namespace MeterManager.Pages
{
    public class IndexModel : PageModel
    {
        private readonly SBCorpInetDbContext _context;

        public IndexModel(SBCorpInetDbContext context)
        {
            _context = context;
        }

        public List<EtherMeterProfile> Meters { get; set; } = new();
        public List<DateTime> HourTimestamps { get; set; } = new();
        public Dictionary<DateTime, Dictionary<string, EtherMeterReading>> HourlyReadings { get; set; } = new();

        public async Task OnGetAsync(DateTime? startDate, DateTime? endDate)
        {
            Meters = await _context.EtherMeterProfiles.OrderBy(m => m.MeterID).ToListAsync();

            var readingsQuery = _context.EtherMeterReadings.AsQueryable();

            if (startDate.HasValue)
                readingsQuery = readingsQuery.Where(r => r.Timestamp >= startDate.Value);
            if (endDate.HasValue)
                readingsQuery = readingsQuery.Where(r => r.Timestamp <= endDate.Value);

            var readings = await readingsQuery.ToListAsync();

            // After filtering by date, get the most recent 100 timestamps
            HourTimestamps = readings
                .Select(r => r.Timestamp)
                .Distinct()
                .OrderByDescending(t => t)
                .Take(100)
                .ToList();

            // Group readings by hour (truncate minutes/seconds)
            var readingsByHour = readings
                .GroupBy(r => new DateTime(r.Timestamp.Year, r.Timestamp.Month, r.Timestamp.Day, r.Timestamp.Hour, 0, 0))
                .OrderByDescending(g => g.Key)
                .Take(100)
                .ToList();

            HourTimestamps = readingsByHour.Select(g => g.Key).ToList();

            HourlyReadings = readingsByHour.ToDictionary(
                g => g.Key,
                g => g
                    .GroupBy(r => r.MeterID.ToString())
                    .ToDictionary(
                        gg => gg.Key,
                        gg => gg.First()
                    )
            );
        }
    }
}