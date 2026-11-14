using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using MO.Models;

namespace MO.Controllers
{
    /// <summary>
    /// AI asistent v mobilní aplikaci: stejný agent jako „Zeptej se MARKTIME“ v UI (BL.AI.Ask.AiAskService).
    /// Odpovídá na otázky nad daty a na pokyn připraví vykázání hodin; úkon se zapíše až po potvrzení kartou.
    /// URL: /Ai (stránka), /Ai/Ask, /Ai/UkonConfirm, /Ai/UkonCancel (POST s antiforgery tokenem).
    /// Nastavení: sekce "AI" v appsettings.json MO (stejná jako v UI); API klíč se čte ze společného appsecrets.json.
    /// </summary>
    public class AiController : BaseController
    {
        private readonly BL.TheColumnsProvider _cp;
        private readonly BL.AI.Core.ILlmClient _llm;
        private static readonly JsonSerializerOptions _json = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public AiController(BL.TheColumnsProvider cp, BL.AI.Core.ILlmClient llm)
        {
            _cp = cp; _llm = llm;
        }

        private BL.AI.Ask.AiAskService Service() => new(Factory, _cp, _llm);

        // modely BO.AI mají názvy pro System.Text.Json - výchozí Json() v MO používá Newtonsoft, proto vlastní serializace
        private ContentResult JsonResult(object o) => Content(JsonSerializer.Serialize(o, _json), "application/json");

        public IActionResult Index()
        {
            var v = new AiViewModel
            {
                PageTitle = Factory.tra("AI asistent"),
                HideHeaderTitle = true,
                IsEnabled = Service().IsEnabled,
                MyID = Factory.CurrentUser.pid,
                CanWriteHours = Factory.CurrentUser.j04IsModule_p31
            };
            if (!v.IsEnabled)
            {
                v.DisabledReason = Factory.tra("AI asistent není v této instalaci zapnutý.");
                if (_llm.Options.DeniedByRole) v.DisabledReason = _llm.Options.DisabledReason;
                else if (Factory.CurrentUser.IsAdmin) v.DisabledReason += " " + _llm.Options.DisabledReason;
            }
            return View(v);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ask(string payload)
        {
            BO.AI.AiToolAskInput input;
            try { input = JsonSerializer.Deserialize<BO.AI.AiToolAskInput>(payload ?? ""); }
            catch { return JsonResult(new { ok = false, error = Factory.tra("Neplatný vstup.") }); }
            if (input == null) return JsonResult(new { ok = false, error = Factory.tra("Neplatný vstup.") });
            input.ContextPage = "mo-ai";

            var r = await Service().AskAsync(input, HttpContext.RequestAborted);
            return JsonResult(new { ok = r.Ok, error = r.Error, reply = r.Reply, tables = r.Tables, proposals = r.Proposals, warnings = r.Warnings, thread = r.Ok ? r.Thread : null, logId = r.LogId });
        }

        /// <summary>
        /// Uživatel potvrdil návrh úkonu tlačítkem „Zapsat“. Návrh připravil agent (p85TempBox, jen vlastník, jedno použití);
        /// zápis jde přes p31WorksheetBL.SaveOrigRecord se všemi kontrolami znovu.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UkonConfirm(string guid, long logId = 0)
        {
            var o = BL.AI.Tools.UkonZapisTool.Confirm(Factory, guid);
            if (o.IsError) return JsonResult(new { ok = false, error = o.Content });
            if (logId > 0) BL.AI.Core.AiLog.SetResult(Factory, logId, o.SavedEntity, o.SavedPid);
            return JsonResult(new { ok = true, ukon = o.Data as BL.AI.Tools.UkonCard });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UkonCancel(string guid)
        {
            BL.AI.Tools.UkonZapisTool.Cancel(Factory, guid);
            return JsonResult(new { ok = true });
        }
    }
}
