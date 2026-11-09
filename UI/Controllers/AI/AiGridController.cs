using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using UI.Models.AI;

namespace UI.Controllers.AI
{
    /// <summary>
    /// AI návrh sloupců tabulky a sloupcového filtru. Otevírá se z menu „Tabulka“ v gridu a z návrháře sloupců.
    /// URL: /AiGrid/Index?j72id=…&amp;pathname=…, /AiGrid/Ask (POST), /AiGrid/Save (POST)
    /// </summary>
    public class AiGridController : BaseController
    {
        private readonly BL.TheColumnsProvider _cp;
        private readonly BL.Singleton.ThePeriodProvider _pp;
        private readonly BL.AI.Core.ILlmClient _llm;
        private static readonly JsonSerializerOptions _json = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        public AiGridController(BL.TheColumnsProvider cp, BL.Singleton.ThePeriodProvider pp, BL.AI.Core.ILlmClient llm)
        {
            _cp = cp; _pp = pp; _llm = llm;
        }

        private BL.AI.Grid.AiGridService Service(int j72id) => new(Factory, _cp, _pp, _llm, j72id);

        /// <param name="j72id">tabulka gridu, ze které bylo okno otevřeno (určuje entitu)</param>
        /// <param name="pathname">stránka gridu (FlatView/MasterView/SlaveView) – pro přepnutí na novou tabulku</param>
        public IActionResult Index(int j72id, string pathname)
        {
            var svc = Service(j72id);
            if (!svc.IsEnabled)
            {
                string msg = "AI asistent není v této instalaci zapnutý.";
                if (_llm.Options.DeniedByRole) msg = _llm.Options.DisabledReason;
                else if (Factory.CurrentUser.IsAdmin) msg += " " + _llm.Options.DisabledReason;
                return this.StopPage(true, msg);
            }
            if (svc.Template == null) return this.StopPage(true, "Tabulka nebyla nalezena.");

            var warnings = new List<string>();
            var v = new AiGridViewModel
            {
                PageTitle = Factory.tra("AI návrh sloupců tabulky"),
                PageSymbol = "grid_view",
                J72ID = j72id,
                CallerPathName = pathname,
                TableName = svc.Template.GetName(),
                EntityName = svc.EntityName,
                CanOverwrite = svc.CanOverwrite,
                Feature = svc.FeatureName,
                CatalogJson = JsonSerializer.Serialize(svc.Catalog.ToUiList(), _json),
                FixedJson = JsonSerializer.Serialize(svc.FixedFilters.ToUi(), _json),
                InitialJson = JsonSerializer.Serialize(svc.CurrentProposal(warnings), _json)
            };
            v.InitialWarningsJson = JsonSerializer.Serialize(warnings, _json);
            return View(v);
        }

        [HttpPost]
        public async Task<IActionResult> Ask(string payload)
        {
            BO.AI.AiGridAskInput input;
            try { input = JsonSerializer.Deserialize<BO.AI.AiGridAskInput>(payload ?? ""); }
            catch { return JsonResult(new { ok = false, error = "Neplatný vstup." }); }

            var r = await Service(input?.J72ID ?? 0).AskAsync(input, HttpContext.RequestAborted);
            return JsonResult(new { ok = r.Ok, error = r.Error, reply = r.Reply, proposal = r.Proposal, warnings = r.Warnings, raw = r.Raw, logId = r.LogId });
        }

        /// <param name="mode">overwrite = přepsat tabulku j72id (jen s oprávněním), new = uložit jako novou pojmenovanou tabulku, filter = jen použít sloupcový filtr</param>
        [HttpPost]
        public IActionResult Save(string payload, int j72id, long logId, string mode, string pathname)
        {
            BO.AI.AiGridProposal p;
            try { p = JsonSerializer.Deserialize<BO.AI.AiGridProposal>(payload ?? ""); }
            catch { return JsonResult(new { ok = false, errors = new[] { "Neplatný vstup." } }); }
            if (p == null || !Enum.TryParse<BL.AI.Grid.AiGridService.SaveMode>(mode ?? "", true, out var m))
                return JsonResult(new { ok = false, errors = new[] { "Neplatný vstup." } });

            var r = Service(j72id).Save(p, logId, m, pathname);
            return JsonResult(new { ok = r.Ok, j72id = r.J72ID, errors = r.Errors });
        }

        private ContentResult JsonResult(object o) => Content(JsonSerializer.Serialize(o, _json), "application/json");
    }
}
