using Microsoft.AspNetCore.Mvc;

namespace UI.Controllers
{
    /// <summary>
    /// Check-list (tabulka p65Checklist) – volání z wwwroot/js/p65checklist.js.
    /// prefix + pid určují, komu check-list patří: "p56" = úkol, "o23" = dokument, "p58" = opakovaný úkol (vzor).
    /// Každá akce vrací aktuální stav celého check-listu, takže prohlížeč vždy ukazuje to, co je v databázi.
    /// Oprávnění hlídá BL.p65ChecklistBL.
    /// </summary>
    public class p65Controller : BaseController
    {
        private IActionResult State(string prefix, int pid, bool ok, string message = null)
        {
            if (!Factory.p65ChecklistBL.CanRead(pid, null, prefix))
            {
                return Json(new { ok = false, message = Factory.tra("K tomuto check-listu nemáte přístup."), canedit = false, items = new List<object>() });
            }
            if (!ok && message == null)
            {
                message = Factory.GetFirstNotifyMessage() ?? Factory.tra("Změnu se nepodařilo uložit.");
            }
            var items = Factory.p65ChecklistBL.GetList(pid, prefix).Select(p => new
            {
                id = p.pid,
                name = p.p65Name,
                done = p.IsDone,
                by = p.DoneBy,
                at = p.DateDone == null ? null : Convert.ToDateTime(p.DateDone).ToString("dd.MM.yyyy HH:mm")
            });
            return Json(new { ok = ok, message = message, canedit = Factory.p65ChecklistBL.CanEdit(pid, null, prefix), items = items });
        }

        private static string Px(string prefix)
        {
            return prefix == "p58" || prefix == "o23" ? prefix : "p56";
        }

        public IActionResult List(string prefix, int pid)
        {
            return State(Px(prefix), pid, true);
        }

        [HttpPost]
        public IActionResult Add(string prefix, int pid, string text)
        {
            prefix = Px(prefix);
            var names = Factory.p65ChecklistBL.ParseLines(text);
            if (names.Count == 0) return State(prefix, pid, true);
            return State(prefix, pid, Factory.p65ChecklistBL.Add(pid, names, prefix) > 0);
        }

        [HttpPost]
        public IActionResult Toggle(int p65id, bool done, string prefix, int pid)
        {
            var rec = Factory.p65ChecklistBL.Load(p65id);
            if (rec == null) return State(Px(prefix), pid, false, Factory.tra("Položka už v check-listu není."));
            return State(rec.OwnerPrefix, rec.OwnerPid, Factory.p65ChecklistBL.SetDone(p65id, done));
        }

        [HttpPost]
        public IActionResult Rename(int p65id, string text, string prefix, int pid)
        {
            var rec = Factory.p65ChecklistBL.Load(p65id);
            if (rec == null) return State(Px(prefix), pid, false, Factory.tra("Položka už v check-listu není."));
            return State(rec.OwnerPrefix, rec.OwnerPid, Factory.p65ChecklistBL.Rename(p65id, text));
        }

        [HttpPost]
        public IActionResult Delete(int p65id, string prefix, int pid)
        {
            var rec = Factory.p65ChecklistBL.Load(p65id);
            if (rec == null) return State(Px(prefix), pid, true);   //už je smazaná
            return State(rec.OwnerPrefix, rec.OwnerPid, Factory.p65ChecklistBL.Delete(p65id));
        }

        [HttpPost]
        public IActionResult Reorder(string prefix, int pid, string ids)
        {
            prefix = Px(prefix);
            var p65ids = BO.Code.Bas.ConvertString2ListInt(ids);
            return State(prefix, pid, Factory.p65ChecklistBL.Reorder(pid, p65ids, prefix));
        }
    }
}
