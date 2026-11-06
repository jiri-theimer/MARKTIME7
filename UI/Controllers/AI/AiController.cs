using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace UI.Controllers.AI
{
    /// <summary>
    /// Společné akce všech AI agentů (nezávislé na konkrétním agentovi).
    /// URL: /Ai/Rate (POST), /Ai/History (GET)
    /// </summary>
    public class AiController : BaseController
    {
        /// <summary>Hodnocení odpovědi agenta: rating = 1 / -1, komentář nepovinný. Uživatel hodnotí jen své dotazy.</summary>
        [HttpPost]
        public IActionResult Rate(long logId, int rating, string comment)
        {
            bool ok = BL.AI.Core.AiLog.Rate(Factory, logId, rating, comment);
            return Content(JsonSerializer.Serialize(new { ok }), "application/json");
        }

        /// <summary>Předchozí dotazy přihlášeného uživatele pro danou AI funkci (j98Feature) – nabídka v poli dotazu.</summary>
        [HttpGet]
        public IActionResult History(string feature)
        {
            var list = string.IsNullOrEmpty(feature) ? new List<string>() : BL.AI.Core.AiLog.GetHistory(Factory, feature);
            return Content(JsonSerializer.Serialize(list), "application/json");
        }
    }
}
