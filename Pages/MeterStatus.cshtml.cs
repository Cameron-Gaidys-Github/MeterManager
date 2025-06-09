using MeterManager.Data;
using MeterManager.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.NetworkInformation;

namespace MeterManager.Pages
{
    public class MeterStatusModel : PageModel
    {
        private readonly SBCorpInetDbContext _context;

        public MeterStatusModel(SBCorpInetDbContext context)
        {
            _context = context;
        }

        public List<MeterWithStatus> MetersWithStatus { get; set; } = new();

        public async Task OnGetAsync()
        {
            var meterProfiles = await _context.EtherMeterProfiles.ToListAsync();
            var ping = new Ping();
            var metersWithStatus = new List<MeterWithStatus>();

            foreach (var m in meterProfiles)
            {
                bool isOnline = false;
                try
                {
                    var reply = await ping.SendPingAsync(m.IPAddress, 1000); // 1 second timeout
                    isOnline = reply.Status == IPStatus.Success;
                }
                catch
                {
                    isOnline = false;
                }

                // Get the latest reading for this meter
                var latestReading = await _context.EtherMeterReadings
                    .Where(r => r.MeterID.ToString() == m.MeterID.ToString())
                    .OrderByDescending(r => r.Timestamp)
                    .FirstOrDefaultAsync();

                // Default to fault if no reading exists
                bool networkFault = latestReading?.NetworkFault == true;
                bool meterFault = latestReading?.MeterFault == true;

                metersWithStatus.Add(new MeterWithStatus
                {
                    Meter = m,
                    IsNetworkOnline = isOnline && !networkFault,
                    IsMeterOnline = isOnline && !meterFault,
                    NetworkFault = networkFault,
                    MeterFault = meterFault
                });
            }

            MetersWithStatus = metersWithStatus;
        }
    }
}