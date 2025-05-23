using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MeterManager.Data;
using HtmlAgilityPack;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace MeterManager.Pages.Api
{
    public class ActiveFlowModel : PageModel
    {
        private readonly SBCorpInetDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public ActiveFlowModel(SBCorpInetDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var meters = _context.EtherMeterProfiles.ToList();
            var client = _httpClientFactory.CreateClient();
            var results = new List<object>();

            foreach (var meter in meters)
            {
                try
                {
                    var url = $"http://{meter.IPAddress}";
                    var html = await client.GetStringAsync(url);

                    var doc = new HtmlDocument();
                    doc.LoadHtml(html);

                    string? flow = null;
                    string? highFlow = null;

                    foreach (var cell in doc.DocumentNode.SelectNodes("//td") ?? Enumerable.Empty<HtmlNode>())
                    {
                        var text = cell.InnerText.Trim();

                        if (text.Contains("Meter 1 Flow", System.StringComparison.OrdinalIgnoreCase))
                        {
                            var next = cell.NextSibling;
                            while (next != null && next.NodeType != HtmlNodeType.Element)
                                next = next.NextSibling;
                            if (next != null)
                                flow = next.InnerText.Trim();
                        }
                        if (meter.MeterID == 5 && text.Contains("Meter 2 Flow", System.StringComparison.OrdinalIgnoreCase))
                        {
                            var next = cell.NextSibling;
                            while (next != null && next.NodeType != HtmlNodeType.Element)
                                next = next.NextSibling;
                            if (next != null)
                                highFlow = next.InnerText.Trim();
                        }
                    }

                    results.Add(new
                    {
                        MeterID = meter.MeterID,
                        FlowRate = flow,
                        HighFlowRate = highFlow
                    });
                }
                catch
                {
                    results.Add(new
                    {
                        MeterID = meter.MeterID,
                        FlowRate = (string?)null,
                        HighFlowRate = (string?)null
                    });
                }
            }

            return new JsonResult(results);
        }
    }
}