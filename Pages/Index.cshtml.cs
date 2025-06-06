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

            // --- BEGIN: Adjust Claybrook Meter 5 LowFlow values ---
            // If you want to keep tenths for calculations, do NOT change MeterReading here.
            // Only adjust for display/export below.
            // --- END: Adjust Claybrook Meter 5 LowFlow values ---

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

            // Group by meter for summary and details
            var byMeter = allReadings
                .GroupBy(r => r.MeterID)
                .OrderBy(g => g.Key)
                .ToList();

            var sb = new StringBuilder();

            // First row: skip one cell, then meter names
            sb.AppendLine($",Farmhouse (1),ValleyHouse (2),GateHouse (3),SchoolHouse (4),Claybrook LowFlow (5),Claybrook HighFlow (5)");

            // Second row: skip one cell, then total water use for each meter in timeframe
            var totals = new List<string>();
            foreach (var meter in byMeter)
            {
                var readings = meter.OrderBy(r => r.Timestamp).ToList();
                if (readings.Count >= 2)
                {
                    // For Claybrook (ID 5), output both LowFlow and HighFlow
                    if (meter.Key == "5")
                    {
                        var lowTotal = (readings.Last().MeterReading - readings.First().MeterReading) / 10.0;
                        var highTotal = (readings.Last().HighFlowReading ?? 0) - (readings.First().HighFlowReading ?? 0);
                        totals.Add($"{lowTotal:0.0},{highTotal}");
                    }
                    else
                    {
                        totals.Add((readings.Last().MeterReading - readings.First().MeterReading).ToString());
                    }
                }
                else
                {
                    if (meter.Key == "5")
                        totals.Add("0.0,0");
                    else
                        totals.Add("0");
                }
            }
            // The order in header is: Farmhouse, ValleyHouse, GateHouse, SchoolHouse, Claybrook LowFlow, Claybrook HighFlow
            // But byMeter is ordered by MeterID, so we need to insert the Claybrook HighFlow value after LowFlow
            // We'll flatten the totals list accordingly
            var totalsFlat = new List<string>();
            for (int i = 0; i < totals.Count; i++)
            {
                if (i == 4 && totals[i].Contains(","))
                {
                    var split = totals[i].Split(',');
                    totalsFlat.Add(split[0]); // LowFlow
                    totalsFlat.Add(split[1]); // HighFlow
                }
                else
                {
                    totalsFlat.Add(totals[i]);
                }
            }
            sb.AppendLine($"Total Water Usage over Timeframe,{string.Join(",", totalsFlat)}");

            // Add two more blank rows
            sb.AppendLine();
            sb.AppendLine();

            // Data table header
            sb.AppendLine("Meter ID,Date,Time,Meter Reading (gallons),Hourly Use (gallons),Total Daily Use (gallons),High Flow Reading (gallons),High Flow Hourly Use (gallons),High Flow Daily Use (gallons)");

            // Data rows
            foreach (var meterGroup in byMeter)
            {
                var meterId = meterGroup.Key.ToString();
                var readings = meterGroup.OrderBy(r => r.Timestamp).ToList();

                // Pre-calc total daily use (max−min per day) for normal and high flow
                var dailyTotals = readings
                    .GroupBy(r => r.Timestamp.Date)
                    .ToDictionary(
                        g => g.Key,
                        g => new
                        {
                            Total = g.Max(r => r.MeterReading) - g.Min(r => r.MeterReading),
                            HighTotal = g.Max(r => r.HighFlowReading ?? 0) - g.Min(r => r.HighFlowReading ?? 0)
                        }
                    );

                double? prevReading = null;
                long? prevHigh = null;
                foreach (var r in readings)
                {
                    var date = r.Timestamp.Date;
                    var time = r.Timestamp.ToString("HH:mm");
                    double current = r.MeterID == "5"
                        ? (double)(r.MeterReading / 10m)
                        : r.MeterReading;
                    if (current < 0) current = 0;
                    var highCurrent = r.HighFlowReading ?? 0;
                    var hourlyUse = prevReading.HasValue ? current - prevReading.Value : 0;
                    var highHourlyUse = prevHigh.HasValue ? highCurrent - prevHigh.Value : 0;
                    prevReading = current;
                    prevHigh = highCurrent;
                    var dailyUse = dailyTotals[date].Total;
                    var highDailyUse = dailyTotals[date].HighTotal;

                    sb.AppendLine($"{meterId}," +
                                $"{date:yyyy-MM-dd}," +
                                $"{time}," +
                                $"{current:0.0}," +
                                $"{hourlyUse:0.0}," +
                                $"{dailyUse}," +
                                $"{(r.HighFlowReading.HasValue ? r.HighFlowReading.Value.ToString() : "")}," +
                                $"{highHourlyUse}," +
                                $"{highDailyUse}");
                }
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var fileName = $"WaterUsage_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            return File(bytes, "text/csv", fileName);
        }
    }
}