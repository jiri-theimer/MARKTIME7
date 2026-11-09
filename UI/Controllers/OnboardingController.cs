using Microsoft.AspNetCore.Mvc;
using UI.Code;

namespace UI.Controllers
{
    /// <summary>
    /// „Začínáme“ – checklist prvních kroků uživatele (UI.Code.OnboardingSupport).
    /// URL: /Onboarding (stránka), /Onboarding/Checklist?mode=page|home (partial pro obnovení), /Onboarding/Mark (POST), /Onboarding/SetVisible (POST)
    /// </summary>
    public class OnboardingController : BaseController
    {
        private readonly BL.AI.Core.ILlmClient _llm;

        public OnboardingController(BL.AI.Core.ILlmClient llm)
        {
            _llm = llm;     //Options se čtou líně až v akci (viz AiConfig.CreateClient)
        }

        public IActionResult Index()
        {
            var v = OnboardingSupport.Build(Factory, _llm, "page");
            v.PageTitle = Factory.tra("Začínáme");
            return View(v);
        }

        /// <summary>Aktuální stav checklistu (po návratu z dialogu / jiné záložky).</summary>
        public IActionResult Checklist(string mode)
        {
            if (mode != "home") mode = "page";
            var v = OnboardingSupport.Build(Factory, _llm, mode);
            return PartialView("_Checklist", v);
        }

        /// <summary>Ruční „Hotovo“ u kroku, který z dat poznat nejde (např. aplikace nainstalovaná dřív).</summary>
        [HttpPost]
        public IActionResult Mark(string code)
        {
            return Json(new { ok = OnboardingSupport.MarkDone(Factory, _llm, code) });
        }

        /// <summary>Zobrazovat checklist na úvodní stránce (uživatelský parametr "onboarding").</summary>
        [HttpPost]
        public IActionResult SetVisible(bool visible)
        {
            OnboardingSupport.SetVisibleOnHome(Factory, visible);
            return Json(new { ok = true });
        }
    }
}
