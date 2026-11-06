using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using UI.Models.AI;

namespace UI.Controllers.AI
{
    /// <summary>
    /// AI návrh šablony nástroje SOUČTY. Otevírá se jako modální okno z /p31totals/Index.
    /// URL: /AiTotals/Index, /AiTotals/Ask (POST), /AiTotals/Save (POST)
    /// </summary>
    public class AiTotalsController : BaseController
    {
        private readonly BL.TheColumnsProvider _cp;
        private readonly BL.Singleton.ThePeriodProvider _pp;
        private readonly BL.AI.Core.ILlmClient _llm;

        private static readonly JsonSerializerOptions _json = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        public AiTotalsController(BL.TheColumnsProvider cp, BL.Singleton.ThePeriodProvider pp, BL.AI.Core.ILlmClient llm)
        {
            _cp = cp; _pp = pp; _llm = llm;
        }

        private BL.AI.Totals.AiTotalsService Service() => new(Factory, _cp, _pp, _llm);

        public IActionResult Index()
        {
            var svc = Service();
            if (!svc.IsEnabled)
            {
                string msg = "AI asistent není v této instalaci zapnutý.";
                if (Factory.CurrentUser.IsAdmin) msg += " " + _llm.Options.DisabledReason + " Po úpravě konfigurace je nutné restartovat aplikaci.";
                return this.StopPage(true, msg);
            }
            var v = new AiTotalsViewModel
            {
                PageTitle = Factory.tra("AI návrh šablony Součtů"),
                PageSymbol = "functions",
                CatalogJson = JsonSerializer.Serialize(svc.Catalog.ToUiList(), _json),
                PeriodsJson = JsonSerializer.Serialize(
                    new[] { new { pid = 0, name = Factory.tra("Bez omezení období") }, new { pid = 1, name = Factory.tra("Vlastní rozsah") } }
                    .Concat(svc.Periods.Where(p => p.pid > 1).Select(p => new { pid = p.pid, name = p.Header })), _json),
                StatesJson = JsonSerializer.Serialize(BL.AI.Totals.AiTotalsService.States.Select(p => new { id = p.Key, name = Factory.tra(p.Value) }), _json),
                TabsJson = JsonSerializer.Serialize(BL.AI.Totals.AiTotalsService.TabNames.Select(p => new { id = p.Key, name = Factory.tra(p.Value) }), _json)
            };
            return View(v);
        }

        [HttpPost]
        public async Task<IActionResult> Ask(string payload)
        {
            BO.AI.AiTotalsAskInput input;
            try { input = JsonSerializer.Deserialize<BO.AI.AiTotalsAskInput>(payload ?? ""); }
            catch { return JsonResult(new { ok = false, error = "Neplatný vstup." }); }

            var r = await Service().AskAsync(input, HttpContext.RequestAborted);
            return JsonResult(new { ok = r.Ok, error = r.Error, reply = r.Reply, proposal = r.Proposal, confirm = r.Confirm, warnings = r.Warnings, raw = r.Raw, logId = r.LogId });
        }

        /// <param name="logId">j98ID posledního dotazu – zapíše se k němu uložená šablona</param>
        [HttpPost]
        public IActionResult Save(string payload, long logId)
        {
            BO.AI.AiTotalsProposal p;
            try { p = JsonSerializer.Deserialize<BO.AI.AiTotalsProposal>(payload ?? ""); }
            catch { return JsonResult(new { ok = false, errors = new[] { "Neplatný vstup." } }); }

            var r = Service().Save(p, logId);
            return JsonResult(new { ok = r.Ok, j79id = r.J79ID, errors = r.Errors });
        }

        // System.Text.Json kvůli [JsonPropertyName] v BO.AI (aplikace jinak používá Newtonsoft)
        private ContentResult JsonResult(object o) => Content(JsonSerializer.Serialize(o, _json), "application/json");
    }
}
