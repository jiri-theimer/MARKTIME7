using System.Data;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using UI.Models.AI;
using UI.Views.Shared.Components.myPivot;

namespace UI.Controllers.AI
{
    /// <summary>
    /// Obecný AI agent „Zeptej se MARKTIME“ (BL.AI.Ask.AiAskService).
    /// URL: /AiAsk (stránka), /AiAsk/Ask (POST), /AiAsk/Pivot?guid=…, /AiAsk/PivotData?guid=…, /AiAsk/DrillDown?guid=… (POST)
    /// guid = zadání tabulky uložené v p85TempBox (jen vlastník).
    /// </summary>
    public class AiAskController : BaseController
    {
        /// <summary>Max. počet skupin pro Pivot-Explorer – stejně jako p31totalsController.PivotMaxRows.</summary>
        public const int PivotMaxRows = 50000;
        public const int DrillMaxRows = 20000;

        private readonly BL.TheColumnsProvider _cp;
        private readonly BL.AI.Core.ILlmClient _llm;
        private static readonly JsonSerializerOptions _json = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public AiAskController(BL.TheColumnsProvider cp, BL.AI.Core.ILlmClient llm)
        {
            _cp = cp; _llm = llm;
        }

        private BL.AI.Ask.AiAskService Service() => new(Factory, _cp, _llm);

        /// <summary>Samostatná stránka (hlavní layout); konverzace přežije reload díky sessionStorage (aiask.js).</summary>
        public IActionResult Index()
        {
            var svc = Service();
            if (!svc.IsEnabled)
            {
                string msg = "AI asistent není v této instalaci zapnutý.";
                if (Factory.CurrentUser.IsAdmin) msg += " " + _llm.Options.DisabledReason;
                return this.StopPage(true, msg);
            }
            var v = new AiAskViewModel
            {
                PageTitle = Factory.tra("Zeptej se MARKTIME"),
                PageSymbol = "auto_awesome",
                Feature = BL.AI.Ask.AiAskService.FeatureCode
            };
            return View(v);
        }

        [HttpPost]
        public async Task<IActionResult> Ask(string payload)
        {
            BO.AI.AiToolAskInput input;
            try { input = JsonSerializer.Deserialize<BO.AI.AiToolAskInput>(payload ?? ""); }
            catch { return JsonResult(new { ok = false, error = "Neplatný vstup." }); }
            if (input == null) return JsonResult(new { ok = false, error = "Neplatný vstup." });

            var r = await Service().AskAsync(input, HttpContext.RequestAborted);
            return JsonResult(new { ok = r.Ok, error = r.Error, reply = r.Reply, raw = r.Reply, tables = r.Tables, warnings = r.Warnings, thread = r.Ok ? r.Thread : null, logId = r.LogId });
        }

        // ------------------------------------------------------------------ Pivot-Explorer nad výsledkem

        public IActionResult Pivot(string guid)
        {
            var spec = BL.AI.Ask.AiAskService.LoadSpec(Factory, guid) as BO.AI.AiAggSpec;
            if (spec == null) return this.StopPage(true, "Výsledek nebyl nalezen (platí jen pro uživatele, který se ptal).");

            var agg = new BL.AI.Tools.AiAggregate(Factory, _cp);
            BL.AI.Tools.AiAggregate.Plan plan;
            try { plan = agg.Build(spec, agg.Catalog(spec.Entity)); }
            catch (Exception ex) { return this.StopPage(true, ex.Message); }
            var names = myPivotData.FieldNames(plan.Display);

            // rozložení: dimenze do řádků, měřítka do hodnot (součet), počet záznamů jako count nad první dimenzí
            var rows = new JsonArray();
            for (int i = 0; i < plan.Dims.Count; i++) rows.Add(new JsonObject { ["uniqueName"] = names[i] });
            var measures = new JsonArray();
            for (int i = 0; i < plan.Measures.Count; i++) measures.Add(new JsonObject { ["uniqueName"] = names[plan.Dims.Count + i], ["aggregation"] = "sum" });
            if (plan.HasCount && plan.Dims.Count > 0) measures.Add(new JsonObject { ["uniqueName"] = names[0], ["aggregation"] = "count" });
            var report = new JsonObject
            {
                ["slice"] = new JsonObject { ["rows"] = rows, ["columns"] = new JsonArray { new JsonObject { ["uniqueName"] = "Measures" } }, ["measures"] = measures },
                ["options"] = new JsonObject { ["grid"] = new JsonObject { ["type"] = "compact", ["showTotals"] = "on", ["showGrandTotals"] = "on" } }
            };

            var v = new AiAskPivotViewModel
            {
                Guid = guid,
                Title = string.IsNullOrEmpty(spec.Title) ? "Pivot-Explorer" : spec.Title,
                Description = agg.Describe(spec),
                ReportJson = report.ToJsonString(_json),
                CanDrill = spec.Entity == "p31"
            };
            return View(v);
        }

        public IActionResult PivotData(string guid)
        {
            var spec = BL.AI.Ask.AiAskService.LoadSpec(Factory, guid) as BO.AI.AiAggSpec;
            if (spec == null) return Json(new { error = Factory.tra("Výsledek nebyl nalezen.") });

            var agg = new BL.AI.Tools.AiAggregate(Factory, _cp);
            DataTable dt;
            BL.AI.Tools.AiAggregate.Plan plan;
            try
            {
                plan = agg.Build(spec, agg.Catalog(spec.Entity));
                dt = agg.Run(plan, PivotMaxRows);
            }
            catch (Exception ex)
            {
                return Json(new { error = Factory.tra("Data se nepodařilo načíst.") + " " + ex.Message });
            }
            if (dt.Rows.Count > PivotMaxRows)
                return Json(new { error = string.Format(Factory.tra("Výsledek má více než {0} řádků (kombinací hodnot)."), PivotMaxRows.ToString("N0")) });

            return Content(myPivotData.ToJson(dt, plan.Display, "RowsCount"), "application/json", Encoding.UTF8);
        }

        // ------------------------------------------------------------------ zdrojové úkony

        /// <summary>
        /// Zdrojové úkony výsledku, řádku tabulky v chatu nebo buňky Pivot-Exploreru: zadání + podmínky na hodnoty dimenzí
        /// (stejný formát jako myPivotDrill). ID úkonů uloží do p85TempBox a vrátí guid pro okno /p31totals/Drill.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DrillDown(string guid)
        {
            // tělo čteme sami – aplikace má pro [FromBody] Newtonsoft, který JsonElement neumí
            myPivotDrill.Input d;
            try
            {
                using var reader = new StreamReader(Request.Body, Encoding.UTF8);
                string body = await reader.ReadToEndAsync();
                d = string.IsNullOrWhiteSpace(body) ? new myPivotDrill.Input() : JsonSerializer.Deserialize<myPivotDrill.Input>(body);
            }
            catch
            {
                return Json(new { error = "Neplatný vstup." });
            }

            var loaded = BL.AI.Ask.AiAskService.LoadSpec(Factory, guid);
            if (loaded == null || loaded.Entity != "p31") return Json(new { error = Factory.tra("Výsledek nebyl nalezen.") });
            var spec = BL.AI.Tools.AiAggregate.ForDrill(loaded);     // výpis → stejné filtry bez dimenzí

            var agg = new BL.AI.Tools.AiAggregate(Factory, _cp);
            BL.AI.Tools.AiAggregate.Plan plan;
            try { plan = agg.Build(spec, agg.Catalog(spec.Entity)); }
            catch (Exception ex) { return Json(new { error = ex.Message }); }
            var mq = plan.Query;
            string where = myPivotDrill.BuildWhere(d?.conds, plan.Display, out string err);
            if (err != null) return Json(new { error = err });
            if (!string.IsNullOrEmpty(where))
                mq.explicit_sqlwhere = string.IsNullOrEmpty(mq.explicit_sqlwhere) ? where : mq.explicit_sqlwhere + " AND " + where;

            // jeden řádek = jeden úkon
            mq.explicit_sqlgroupby = "a.p31ID";
            var dt = Factory.gridBL.GetGroupByList(mq, $"TOP ({DrillMaxRows + 1}) a.p31ID AS drillpid");
            if (dt.Rows.Count == 0) return Json(new { error = Factory.tra("Výsledek neobsahuje žádné úkony.") });
            if (dt.Rows.Count > DrillMaxRows)
                return Json(new { error = string.Format(Factory.tra("Výsledek obsahuje více než {0} úkonů. Zvolte konkrétní řádek nebo zužte filtr."), DrillMaxRows.ToString("N0")) });

            var pids = dt.AsEnumerable().Select(r => Convert.ToInt32(r["drillpid"])).ToList();
            string g = BO.Code.Bas.GetGuid();
            foreach (var chunk in pids.Chunk(2000))
            {
                Factory.db.RunSql("INSERT INTO p85TempBox(p85GUID,p85Prefix,p85DataPID) SELECT @guid,'p31',p31ID FROM p31Worksheet WHERE p31ID IN (" + string.Join(",", chunk) + ")", new { guid = g });
            }
            return Json(new { guid = g, count = pids.Count });
        }

        private ContentResult JsonResult(object o) => Content(JsonSerializer.Serialize(o, _json), "application/json");
    }
}
