using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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
        public Dictionary<DateTime, Dictionary<string, EtherMeterReading>> HourlyReadings { get; set; }
            = new();

        public async Task OnGetAsync(DateTime? startDate, DateTime? endDate)
        {
            Meters = await _context.EtherMeterProfiles
                                   .OrderBy(m => m.MeterID)
                                   .ToListAsync();

            var query = _context.EtherMeterReadings.AsQueryable();
            if (startDate.HasValue)
                query = query.Where(r => r.Timestamp >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(r => r.Timestamp <= endDate.Value);

            var readings = await query.ToListAsync();

            var readingsByHour = readings
                .GroupBy(r => new DateTime(r.Timestamp.Year,
                                          r.Timestamp.Month,
                                          r.Timestamp.Day,
                                          r.Timestamp.Hour,
                                          0, 0))
                .OrderByDescending(g => g.Key)
                .Take(100)
                .ToList();

            HourTimestamps = readingsByHour.Select(g => g.Key).ToList();
            HourlyReadings = readingsByHour.ToDictionary(
                g => g.Key,
                g => g.GroupBy(r => r.MeterID.ToString())
                      .ToDictionary(gg => gg.Key, gg => gg.First()));
        }

        public async Task<IActionResult> OnGetExportCsvAsync(DateTime? startDate, DateTime? endDate)
        {
            var query = _context.EtherMeterReadings.AsQueryable();
            if (startDate.HasValue)
                query = query.Where(r => r.Timestamp.Date >= startDate.Value.Date);
            if (endDate.HasValue)
                query = query.Where(r => r.Timestamp.Date <= endDate.Value.Date);

            var allReadings = await query
                .OrderBy(r => r.MeterID)
                .ThenBy(r => r.Timestamp)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Meter ID,Date,Time,Meter Reading (gallons),Hourly Use (gallons),Total Daily Use (gallons)");

            // Group by meter to compute hourly and daily totals
            var byMeter = allReadings
                .GroupBy(r => r.MeterID)
                .OrderBy(g => g.Key);

            foreach (var meterGroup in byMeter)
            {
                var meterId = meterGroup.Key.ToString();
                var readings = meterGroup.OrderBy(r => r.Timestamp).ToList();

                // Pre-calc total daily use (max−min per day)
                var dailyTotals = readings
                    .GroupBy(r => r.Timestamp.Date)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Max(r => r.MeterReading) - g.Min(r => r.MeterReading)
                    );

                long? prevReading = null;
                foreach (var r in readings)
                {
                    var date = r.Timestamp.Date;
                    var time = r.Timestamp.ToString("HH:mm");
                    var current = r.MeterReading;
                    var hourlyUse = prevReading.HasValue ? current - prevReading.Value : 0;
                    prevReading = current;
                    var dailyUse = dailyTotals[date];

                    sb.AppendLine($"{meterId}," +
                                  $"{date:yyyy-MM-dd}," +
                                  $"{time}," +
                                  $"{current}," +
                                  $"{hourlyUse}," +
                                  $"{dailyUse}");
                }
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var fileName = $"WaterUsage_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            return File(bytes, "text/csv", fileName);
        }
    }
}
