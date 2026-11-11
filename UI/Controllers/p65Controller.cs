using Microsoft.AspNetCore.Mvc;

namespace UI.Controllers
{
    /// <summary>
    /// Check-list úkolu (tabulka p65Checklist) – volání z wwwroot/js/p65checklist.js.
    /// Každá akce vrací aktuální stav celého check-listu, takže prohlížeč vždy ukazuje to, co je v databázi.
    /// Oprávnění hlídá BL.p65ChecklistBL: měnit smí, kdo má v úkolu nějakou roli, vlastník úkolu a správce.
    /// </summary>
    public class p65Controller : BaseController
    {
        private IActionResult State(int p56id, bool ok, string message = null)
        {
            if (!Factory.p65ChecklistBL.CanRead(p56id))
            {
                return Json(new { ok = false, message = Factory.tra("K check-listu tohoto úkolu nemáte přístup."), canedit = false, items = new List<object>() });
            }
            if (!ok && message == null)
            {
                message = Factory.GetFirstNotifyMessage() ?? Factory.tra("Změnu se nepodařilo uložit.");
            }
            var items = Factory.p65ChecklistBL.GetList(p56id).Select(p => new
            {
                id = p.pid,
                name = p.p65Name,
                done = p.IsDone,
                by = p.DoneBy,
                at = p.DateDone == null ? null : Convert.ToDateTime(p.DateDone).ToString("dd.MM.yyyy HH:mm")
            });
            return Json(new { ok = ok, message = message, canedit = Factory.p65ChecklistBL.CanEdit(p56id), items = items });
        }

        private int TaskOf(int p65id)
        {
            var rec = Factory.p65ChecklistBL.Load(p65id);
            return rec == null ? 0 : rec.p56ID;
        }

        public IActionResult List(int p56id)
        {
            return State(p56id, true);
        }

        [HttpPost]
        public IActionResult Add(int p56id, string text)
        {
            var names = Factory.p65ChecklistBL.ParseLines(text);
            if (names.Count == 0) return State(p56id, true);
            return State(p56id, Factory.p65ChecklistBL.Add(p56id, names) > 0);
        }

        [HttpPost]
        public IActionResult Toggle(int p65id, bool done, int p56id)
        {
            int intP56ID = TaskOf(p65id);
            if (intP56ID == 0) return State(p56id, false, Factory.tra("Položka už v check-listu není."));
            return State(intP56ID, Factory.p65ChecklistBL.SetDone(p65id, done));
        }

        [HttpPost]
        public IActionResult Rename(int p65id, string text, int p56id)
        {
            int intP56ID = TaskOf(p65id);
            if (intP56ID == 0) return State(p56id, false, Factory.tra("Položka už v check-listu není."));
            return State(intP56ID, Factory.p65ChecklistBL.Rename(p65id, text));
        }

        [HttpPost]
        public IActionResult Delete(int p65id, int p56id)
        {
            int intP56ID = TaskOf(p65id);
            if (intP56ID == 0) return State(p56id, true);   //už je smazaná
            return State(intP56ID, Factory.p65ChecklistBL.Delete(p65id));
        }

        [HttpPost]
        public IActionResult Reorder(int p56id, string ids)
        {
            var p65ids = BO.Code.Bas.ConvertString2ListInt(ids);
            return State(p56id, Factory.p65ChecklistBL.Reorder(p56id, p65ids));
        }
    }
}
