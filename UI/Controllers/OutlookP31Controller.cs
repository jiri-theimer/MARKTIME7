using Microsoft.AspNetCore.Mvc;
using UI.Code;
using UI.Models;

namespace UI.Controllers
{
    /// <summary>
    /// Průvodce „Vykázat z Outlooku“: uživatel přetáhne události z Outlooku (.msg, z nového Outlooku .ics),
    /// průvodce navrhne projekt, aktivitu a text, uživatel zkontroluje a zapíše. Bez AI, bez změn v DB.
    /// Soubory se čtou jen v paměti, nikam se neukládají.
    /// </summary>
    public class OutlookP31Controller : BaseController
    {
        private const int MaxFiles = 200;
        private const int MaxRows = 300;    // celkem v seznamu (exportovaný celý kalendář by jinak neměl konec)
        private static readonly string[] AllowedExt = { ".msg", ".ics", ".ical", ".vcs" };

        public IActionResult Index()
        {
            var v = new OutlookP31ViewModel();
            RefreshState(v);
            return View(v);
        }

        [HttpPost]
        [RequestSizeLimit(100 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 100 * 1024 * 1024, ValueCountLimit = 20000)]
        public IActionResult Index(OutlookP31ViewModel v, List<IFormFile> files)
        {
            v.Rows ??= new List<OutlookP31Row>();
            switch (v.PostbackOper)
            {
                case "upload":
                    Upload(v, files);
                    break;
                case "save":
                    if (v.CountSelected == 0)
                    {
                        AddMessageTranslated("Není vybrán žádný úkon k zapsání.", "info");
                        break;
                    }
                    int saved = OutlookP31Support.Save(Factory, v.Rows);
                    int errs = v.Rows.Count(p => p.State == OutlookP31State.Error);
                    if (saved > 0) AddMessageTranslated($"Zapsáno úkonů: {saved}.", "info");
                    if (errs > 0) AddMessageTranslated($"Nezapsáno: {errs} – důvod je u řádku.", "error");
                    break;
                case "clear":
                    v.Rows.Clear();
                    break;
                case "clear_done":
                    v.Rows.RemoveAll(p => p.State == OutlookP31State.Saved || p.State == OutlookP31State.Duplicate);
                    break;
            }
            RefreshState(v);
            return View(v);
        }

        private void Upload(OutlookP31ViewModel v, List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
            {
                AddMessageTranslated("Nebyl předán žádný soubor. Události přetáhněte z kalendáře Outlooku, nebo vyberte soubory .msg / .ics.", "info");
                return;
            }
            var events = new List<CalEvent>();
            var notRead = new List<string>();
            foreach (var file in files.Take(MaxFiles))
            {
                string ext = System.IO.Path.GetExtension(file.FileName ?? "").ToLowerInvariant();
                if (!AllowedExt.Contains(ext) || file.Length == 0 || file.Length > OutlookEventReader.MaxFileBytes)
                {
                    notRead.Add(file.FileName); continue;
                }
                byte[] data;
                using (var ms = new System.IO.MemoryStream())
                {
                    file.CopyTo(ms); data = ms.ToArray();
                }
                List<CalEvent> lis;
                try { lis = OutlookEventReader.Read(file.FileName, data); }
                catch { lis = new List<CalEvent>(); }
                if (lis.Count == 0) notRead.Add(file.FileName);
                events.AddRange(lis);
            }
            if (files.Count > MaxFiles) AddMessageTranslated($"Najednou lze načíst nejvýše {MaxFiles} souborů.", "info");

            int free = MaxRows - v.Rows.Count;
            if (events.Count > free)
            {
                AddMessageTranslated($"Seznam pojme nejvýše {MaxRows} událostí, načteno prvních {Math.Max(free, 0)}. Nejdřív zapište nebo vyčistěte stávající.", "warning");
                events = events.OrderBy(p => p.Start).Take(Math.Max(free, 0)).ToList();
            }
            var rows = OutlookP31Support.BuildRows(Factory, events, v.Rows.Select(p => p.Key));
            v.Rows.AddRange(rows);

            if (notRead.Count > 0)
            {
                string names = string.Join(", ", notRead.Take(5)) + (notRead.Count > 5 ? $" a další ({notRead.Count - 5})" : "");
                AddMessageTranslated($"Nejde o událost kalendáře: {names}.", "info");
            }
            if (events.Count > 0 && rows.Count == 0) AddMessageTranslated("Tyto události už v seznamu jsou.", "info");
        }

        private void RefreshState(OutlookP31ViewModel v)
        {
            v.PageTitle = Factory.tra("Vykázat z Outlooku");
            v.PageSymbol = "event_available";
            v.Rows = v.Rows.OrderBy(p => p.Date).ThenBy(p => p.TimeFrom).ToList();
            foreach (var r in v.Rows)
            {
                if (r.State == OutlookP31State.Duplicate || r.State == OutlookP31State.Saved) r.IsSelected = false;
            }
        }
    }
}
