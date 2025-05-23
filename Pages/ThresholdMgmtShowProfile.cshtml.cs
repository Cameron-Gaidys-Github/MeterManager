using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MeterManager.Data;
using MeterManager.Models;
using System.Threading.Tasks;

namespace MeterManager.Pages
{
    public class ThresholdMgmtShowProfileModel : PageModel
    {
        private readonly SBCorpInetDbContext _context;

        public ThresholdMgmtShowProfileModel(SBCorpInetDbContext context)
        {
            _context = context;
        }

        public EtherMeterProfile? Meter { get; set; }

        [BindProperty]
        public EtherMeterThreshold? Threshold { get; set; }

        [BindProperty]
        public EthermeterThresholdOffset? Offset { get; set; }

        public async Task<IActionResult> OnGetAsync(int MeterID)
        {
            Meter = await _context.EtherMeterProfiles.FirstOrDefaultAsync(m => m.MeterID == MeterID);
            if (Meter == null)
                return NotFound();

            Threshold = await _context.EtherMeterThresholds
                .FirstOrDefaultAsync(t => t.MeterID == MeterID);

            Offset = await _context.EtherMeterThresholdOffsets
                .FirstOrDefaultAsync(o => o.MeterID == MeterID);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int MeterID)
        {
            if (Offset == null)
                return Page();

            var offsetInDb = await _context.EtherMeterThresholdOffsets
                .FirstOrDefaultAsync(o => o.MeterID == MeterID);

            if (offsetInDb == null)
                return NotFound();

            // Update Offset properties
            offsetInDb.ChristmasWeek = Offset.ChristmasWeek;
            offsetInDb.MLKWeek = Offset.MLKWeek;
            offsetInDb.PresidentsWeek = Offset.PresidentsWeek;
            offsetInDb.January = Offset.January;
            offsetInDb.February = Offset.February;
            offsetInDb.March = Offset.March;
            offsetInDb.April = Offset.April;
            offsetInDb.May = Offset.May;
            offsetInDb.June = Offset.June;
            offsetInDb.July = Offset.July;
            offsetInDb.August = Offset.August;
            offsetInDb.September = Offset.September;
            offsetInDb.October = Offset.October;
            offsetInDb.November = Offset.November;
            offsetInDb.December = Offset.December;

            await _context.SaveChangesAsync();

            return RedirectToPage(new { MeterID });
        }

        public async Task<IActionResult> OnPostThresholdAsync(int MeterID)
        {
            if (Threshold == null)
                return Page();

            var thresholdInDb = await _context.EtherMeterThresholds
                .FirstOrDefaultAsync(t => t.MeterID == MeterID);

            if (thresholdInDb == null)
                return NotFound();

            // Update Threshold properties (add all your hourly properties here)
            // Example:
            // thresholdInDb.Hour1 = Threshold.Hour1;
            // thresholdInDb.Hour2 = Threshold.Hour2;
            // ... up to Hour24 or as many as you have

            await _context.SaveChangesAsync();

            return RedirectToPage(new { MeterID });
        }
    }
}