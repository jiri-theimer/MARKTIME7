using Microsoft.AspNetCore.Mvc;
using UI.Models.p31oper;

namespace UI.Controllers.p31oper
{
    /// <summary>Oprávnění k úkonu: co smí přihlášený uživatel s úkonem dělat a odkud to oprávnění plyne (menu úkonu → Další).</summary>
    public class p31permController : BaseController
    {
        public IActionResult Index(int pid)
        {
            var v = new p31PermViewModel() { Explain = Factory.p31WorksheetBL.ExplainRecDisposition(pid) };
            if (v.Explain == null)
            {
                return this.StopPage(true, "Záznam nebyl nalezen.");
            }
            return View(v);
        }
    }
}
