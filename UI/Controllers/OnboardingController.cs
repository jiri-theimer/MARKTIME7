using Microsoft.AspNetCore.Mvc;
using UI.Code;

namespace UI.Controllers
{
    /// <summary>
    /// AJAX k úvodní stránce /Home/Index („Onboarding (začínáme)“, UI.Code.OnboardingSupport).
    /// URL: /Onboarding/Checklist (partial pro obnovení), /Onboarding/Mark (POST)
    /// </summary>
    public class OnboardingController : BaseController
    {
        private readonly BL.AI.Core.ILlmClient _llm;

        public OnboardingController(BL.AI.Core.ILlmClient llm)
        {
            _llm = llm;     //Options se čtou líně až v akci (viz AiConfig.CreateClient)
        }

        /// <summary>Aktuální stav checklistu (po návratu z dialogu / jiné záložky).</summary>
        public IActionResult Checklist()
        {
            return PartialView("_Checklist", OnboardingSupport.Build(Factory, _llm));
        }

        /// <summary>Ruční „Hotovo“ u kroku, který z dat poznat nejde (např. aplikace nainstalovaná dřív).</summary>
        [HttpPost]
        public IActionResult Mark(string code)
        {
            return Json(new { ok = OnboardingSupport.MarkDone(Factory, _llm, code) });
        }
    }
}
