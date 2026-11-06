using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace UI.Controllers.AI
{
    /// <summary>
    /// Společné akce všech AI agentů (nezávislé na konkrétním agentovi).
    /// URL: /Ai/Rate (POST)
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
    }
}
