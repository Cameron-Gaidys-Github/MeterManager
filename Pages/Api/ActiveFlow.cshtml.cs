using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MeterManager.Data;
using HtmlAgilityPack;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using MeterManager.Models;
using System.Threading;
using System.Net.Mail;

namespace MeterManager.Pages.Api
{
    /// <summary>
    /// Razor PageModel for the /Api/ActiveFlow endpoint.
    /// This endpoint provides live flow readings for all configured meters by scraping their web interfaces in parallel.
    /// </summary>
    public class ActiveFlowModel : PageModel
    {
        private readonly SBCorpInetDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        // Tracks previous online state for each meter to avoid duplicate alerts
        private static readonly Dictionary<int, bool> _meterOnlineState = new();

        public ActiveFlowModel(SBCorpInetDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        // Sends an email alert when a meter goes offline
        public void SendMeterOfflineAlert(EtherMeterProfile meter)
        {
            var smtp = new SmtpClient("smtp-relay.idirectory.itw")
            {
                Port = 587,
                Credentials = new System.Net.NetworkCredential("username", "password"),
                EnableSsl = true
            };
            var mail = new MailMessage("from@yourdomain.com", "to@yourdomain.com")
            {
                Subject = $"Meter {meter.MeterID} Offline",
                Body = $"Meter {meter.MeterID} at {meter.Location} is offline."
            };
            smtp.Send(mail);
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var meters = _context.EtherMeterProfiles.Where(m => m.IsActive).ToList();
            var client = _httpClientFactory.CreateClient();
            var throttler = new SemaphoreSlim(5);

            var tasks = meters.Select(async meter =>
            {
                await throttler.WaitAsync();
                try
                {
                    var url = $"http://{meter.IPAddress}";
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                    var html = await client.GetStringAsync(url, cts.Token);

                    var doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(html);

                    string? flow = null;
                    string? highFlow = null;

                    foreach (var cell in doc.DocumentNode.SelectNodes("//td") ?? Enumerable.Empty<HtmlAgilityPack.HtmlNode>())
                    {
                        var text = cell.InnerText.Trim();
                        if (text.Contains("Meter 1 Flow", System.StringComparison.OrdinalIgnoreCase))
                        {
                            var next = cell.NextSibling;
                            while (next != null && next.NodeType != HtmlAgilityPack.HtmlNodeType.Element)
                                next = next.NextSibling;
                            if (next != null)
                                flow = next.InnerText.Trim();
                        }
                        if (meter.MeterID == 5 && text.Contains("Meter 2 Flow", System.StringComparison.OrdinalIgnoreCase))
                        {
                            var next = cell.NextSibling;
                            while (next != null && next.NodeType != HtmlAgilityPack.HtmlNodeType.Element)
                                next = next.NextSibling;
                            if (next != null)
                                highFlow = next.InnerText.Trim();
                        }
                    }

                    // Meter is online, update state
                    lock (_meterOnlineState)
                    {
                        _meterOnlineState[meter.MeterID] = true;
                    }

                    return new
                    {
                        MeterID = meter.MeterID,
                        FlowRate = flow,
                        HighFlowRate = highFlow
                    };
                }
                catch
                {
                    bool wasOnline;
                    lock (_meterOnlineState)
                    {
                        wasOnline = !_meterOnlineState.ContainsKey(meter.MeterID) || _meterOnlineState[meter.MeterID];
                        _meterOnlineState[meter.MeterID] = false;
                    }
                    // Only send alert if meter just went offline
                    if (wasOnline)
                    {
                        SendMeterOfflineAlert(meter);
                    }
                    return new
                    {
                        MeterID = meter.MeterID,
                        FlowRate = (string?)null,
                        HighFlowRate = (string?)null
                    };
                }
                finally
                {
                    throttler.Release();
                }
            });

            var results = await Task.WhenAll(tasks);

            // Recommended cache and security headers
            Response.Headers["Cache-Control"] = "private, no-cache";
            Response.Headers["X-Content-Type-Options"] = "nosniff";
            Response.ContentType = "application/json; charset=utf-8";

            return new JsonResult(results);
        }
    }
}
