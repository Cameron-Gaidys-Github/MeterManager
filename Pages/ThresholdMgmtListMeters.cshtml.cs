using Microsoft.AspNetCore.Mvc.RazorPages;
using MeterManager.Data;
using MeterManager.Models;
using MeterManager.Helpers;
using Microsoft.EntityFrameworkCore;

namespace MeterManager.Pages
{
    public class ThresholdMgmtListMetersModel : PageModel
    {
        private readonly SBCorpInetDbContext _context;

        public ThresholdMgmtListMetersModel(SBCorpInetDbContext context)
        {
            _context = context;
        }

        public List<MeterWithStatus> MetersWithStatus { get; set; } = new();

        public async Task OnGetAsync()
        {
            var meters = await _context.EtherMeterProfiles
                .OrderBy(m => m.MeterID)
                .ToListAsync();

            foreach (var meter in meters)
            {
                MetersWithStatus.Add(new MeterWithStatus
                {
                    Meter = meter,
                    IsNetworkOnline = NetworkStatusChecker.IsPingSuccessful(meter.IPAddress),
                    IsMeterOnline = NetworkStatusChecker.IsPingSuccessful(meter.IPAddress) // You can replace with different logic if needed
                });
            }
        }

        public class MeterWithStatus
        {
            public EtherMeterProfile Meter { get; set; } = new();
            public bool IsNetworkOnline { get; set; }
            public bool IsMeterOnline { get; set; }
        }
    }
}