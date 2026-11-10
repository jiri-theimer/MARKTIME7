using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UI.Models;

namespace UI.Code
{
    /// <summary>
    /// Průvodce „Vykázat z Outlooku“: události (CalEvent) → návrhy časových úkonů (OutlookP31Row).
    /// Párování bez AI: e-maily účastníků → kontakt (o32 / p30) → klient → projekt (p26, spisová značka, kód, název, historie uživatele) → aktivita z historie.
    /// Jen čtení z DB; ukládá se standardně přes p31WorksheetBL.SaveOrigRecord.
    /// </summary>
    public static class OutlookP31Support
    {
        public const string CodePrefix = "ol:";

        private static readonly HashSet<string> FreeMail = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "gmail.com","seznam.cz","email.cz","post.cz","centrum.cz","atlas.cz","volny.cz","tiscali.cz","outlook.com","hotmail.com","live.com","yahoo.com","icloud.com","me.com","azet.sk","zoznam.sk","protonmail.com","proton.me"
        };

        private class P41Info
        {
            public int p41ID { get; set; }
            public string p41Name { get; set; }
            public string p41NameShort { get; set; }
            public string p41Code { get; set; }
            public int p28ID_Client { get; set; }
            public string Client { get; set; }
            public string Combo { get; set; }
        }
        private class Item { public OutlookP31Row Row; public List<CalPerson> External; public string Text; }
        private class EmailHit { public string Email { get; set; } public int p28ID { get; set; } public int p28ID_Company { get; set; } public string p28Name { get; set; } }
        private class P26Hit { public int p28ID { get; set; } public int p41ID { get; set; } }
        private class HistP41 { public int p41ID { get; set; } public int Cnt { get; set; } public DateTime Last { get; set; } }
        private class HistP32 { public int p41ID { get; set; } public int p32ID { get; set; } public string p32Name { get; set; } public int Cnt { get; set; } }
        private class SpisHit { public int p41ID { get; set; } public string o60SpisCode { get; set; } public string SpisKey { get; set; } }
        private class P31Exist { public int p31ID { get; set; } public string p31ExternalCode { get; set; } public DateTime? p31DateTimeFrom_Orig { get; set; } public DateTime? p31DateTimeUntil_Orig { get; set; } public string Project { get; set; } }

        /// <summary>Kód pro p31ExternalCode: stejná událost (i stejný výskyt opakované) = stejný kód.</summary>
        public static string MakeKey(CalEvent ev)
        {
            string basis = (ev.Uid ?? (ev.Subject ?? "")) + "|" + (ev.Start?.ToString("yyyyMMddHHmm") ?? "");
            var h = SHA1.HashData(Encoding.UTF8.GetBytes(basis));
            return CodePrefix + Convert.ToHexString(h, 0, 10).ToLowerInvariant();
        }

        public static List<OutlookP31Row> BuildRows(BL.Factory f, List<CalEvent> events, IEnumerable<string> existingKeys)
        {
            var items = new List<Item>();
            var have = new HashSet<string>(existingKeys ?? Enumerable.Empty<string>());
            int j02id = f.CurrentUser.pid;

            // interní adresy: e-maily uživatelů a jejich firemní domény
            var internalMails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var internalDomains = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var m in f.db.GetList<BO.GetString>("SELECT DISTINCT LOWER(LTRIM(RTRIM(j02Email))) as Value FROM j02User WHERE j02Email LIKE '%@%'") ?? Enumerable.Empty<BO.GetString>())
            {
                internalMails.Add(m.Value);
                string d = Domain(m.Value);
                if (d != null && !FreeMail.Contains(d)) internalDomains.Add(d);
            }
            string myMail = (f.CurrentUser.j02Email ?? "").Trim().ToLowerInvariant();

            foreach (var ev in events.Where(p => p.Start != null))
            {
                string key = MakeKey(ev);
                if (!have.Add(key)) continue;   // stejná událost přetažená znovu

                var row = new OutlookP31Row
                {
                    Key = key,
                    FileName = ev.FileName,
                    Subject = string.IsNullOrWhiteSpace(ev.Subject) ? "(bez předmětu)" : ev.Subject.Trim(),
                    Date = ev.Start.Value.Date,
                    TimeFrom = ev.Start.Value.ToString("HH:mm"),
                    TimeUntil = (ev.End ?? ev.Start).Value.ToString("HH:mm"),
                };
                var dur = (ev.End ?? ev.Start).Value - ev.Start.Value;
                row.Hours = $"{(int)dur.TotalHours:00}:{dur.Minutes:00}";

                var people = new List<CalPerson>();
                if (ev.OrganizerEmail != null || ev.OrganizerName != null) people.Add(new CalPerson { Name = ev.OrganizerName, Email = ev.OrganizerEmail });
                people.AddRange(ev.Attendees);
                var external = people.Where(p => p.Email != null && !internalMails.Contains(p.Email) && !internalDomains.Contains(Domain(p.Email) ?? "")).GroupBy(p => p.Email).Select(g => g.First()).ToList();
                row.Attendees = string.Join(", ", people.Where(p => p.Email == null || p.Email != myMail).Select(p => p.Name ?? p.Email).Distinct().Take(12));

                var me = ev.Attendees.FirstOrDefault(p => p.Email != null && p.Email == myMail);
                bool declined = ev.Declined || me?.PartStat == "DECLINED";

                // přeskočit (uživatel může přesto vybrat v sekci Přeskočené)
                string skip = null;
                if (ev.Cancelled) skip = "Zrušená událost";
                else if (declined) skip = "Odmítnutá účast";
                else if (ev.AllDay) skip = "Celodenní událost";
                else if (ev.Uid != null && Regex.IsMatch(ev.Uid, @"^(p31|p56|o22)-\d+$")) skip = "Událost exportovaná z MARKTIME";
                else if (dur.TotalMinutes < 1) skip = "Nulová délka";
                else if (dur.TotalHours > 16) skip = "Delší než 16 hodin";
                else if (ev.IsFree) skip = "Zobrazeno jako volno";
                if (skip != null) { row.State = OutlookP31State.Skipped; row.Message = skip; }

                // text úkonu: soukromé události bez textu
                if (ev.IsPrivate)
                {
                    row.p31Text = "";
                    row.Warning = "Soukromá událost – text nepřevzat.";
                }
                else
                {
                    row.p31Text = row.Subject;
                    string body = OutlookEventReader.CleanBody(ev.Body, 600);
                    if (body != null)
                    {
                        string first = body.Split('\n').FirstOrDefault(l => l.Trim().Length > 0)?.Trim();
                        if (first != null && first.Length > 2 && !first.Equals(row.Subject, StringComparison.OrdinalIgnoreCase))
                            row.p31Text = $"{row.Subject}: {first}";
                    }
                    var intern = new List<string>();
                    if (!string.IsNullOrWhiteSpace(row.Attendees)) intern.Add("Účastníci: " + row.Attendees);
                    if (!string.IsNullOrWhiteSpace(ev.Location)) intern.Add("Místo: " + ev.Location.Trim());
                    row.p31TextInternal = intern.Count > 0 ? string.Join("\n", intern) : null;
                }
                if (ev.Recurring) row.Warning = Join(row.Warning, "Opakovaná událost – zkontrolujte datum výskytu.");

                row.IsSelected = row.State == OutlookP31State.New;
                items.Add(new Item { Row = row, External = external, Text = $"{ev.Subject} {ev.Location}" });
            }

            Match(f, items);
            var rows = items.Select(p => p.Row).ToList();
            FindDuplicatesAndOverlaps(f, rows);
            return rows;
        }

        private static void Match(BL.Factory f, List<Item> items)
        {
            if (items.Count == 0) return;
            int j02id = f.CurrentUser.pid;

            // 1) e-maily → kontakt (vlastní médium i kontaktní osoba firmy)
            var emails = items.SelectMany(r => r.External).Select(p => p.Email).Distinct().Take(300).ToList();
            var hits = new List<EmailHit>();
            if (emails.Count > 0)
            {
                hits = f.db.GetList<EmailHit>(@"SELECT LOWER(LTRIM(RTRIM(o32.o32Value))) as Email, p28.p28ID, ISNULL(p30.p28ID, 0) as p28ID_Company, p28.p28Name
                    FROM o32Contact_Medium o32 INNER JOIN p28Contact p28 ON o32.p28ID=p28.p28ID
                    LEFT OUTER JOIN p30ContactPerson p30 ON p30.p28ID_Person=p28.p28ID
                    WHERE o32.o33ID IN (2,21,22) AND LOWER(LTRIM(RTRIM(o32.o32Value))) IN @emails AND p28.p28ValidUntil>GETDATE()", new { emails })?.ToList() ?? new List<EmailHit>();
            }

            // 2) projekty, kde je kontakt projektovou osobou
            var p28ids = hits.Select(p => p.p28ID).Distinct().ToList();
            var p26 = p28ids.Count == 0 ? new List<P26Hit>() : (f.db.GetList<P26Hit>("SELECT a.p28ID, a.p41ID FROM p26ProjectContact a INNER JOIN p41Project p41 ON a.p41ID=p41.p41ID WHERE a.p28ID IN @p28ids AND p41.p41ValidUntil>GETDATE()", new { p28ids })?.ToList() ?? new List<P26Hit>());

            // 3) historie uživatele (posledních 180 dní)
            var hist = f.db.GetList<HistP41>("SELECT p41ID, COUNT(*) as Cnt, MAX(p31Date) as Last FROM p31Worksheet WHERE j02ID=@j02id AND p31Date>DATEADD(day,-180,GETDATE()) GROUP BY p41ID", new { j02id })?.ToList() ?? new List<HistP41>();
            var histP32 = f.db.GetList<HistP32>(@"SELECT a.p41ID, a.p32ID, p32.p32Name, COUNT(*) as Cnt FROM p31Worksheet a INNER JOIN p32Activity p32 ON a.p32ID=p32.p32ID INNER JOIN p34ActivityGroup p34 ON p32.p34ID=p34.p34ID
                WHERE a.j02ID=@j02id AND p34.p33ID=1 AND a.p31Date>DATEADD(day,-365,GETDATE()) GROUP BY a.p41ID, a.p32ID, p32.p32Name", new { j02id })?.ToList() ?? new List<HistP32>();

            // klienti z e-mailů
            var clientIds = hits.Select(p => p.p28ID).Concat(hits.Where(p => p.p28ID_Company > 0).Select(p => p.p28ID_Company)).Distinct().ToList();

            // 4) spisové značky v předmětu
            var spisy = new List<SpisHit>();
            var spisCodes = items.SelectMany(r => SpisCandidates(r.Text)).Distinct().Take(100).ToList();
            if (spisCodes.Count > 0)
            {
                spisy = f.db.GetList<SpisHit>("SELECT a.p41ID, a.o60SpisCode, REPLACE(a.o60SpisCode,' ','') as SpisKey FROM o60Spis a INNER JOIN p41Project p41 ON a.p41ID=p41.p41ID WHERE REPLACE(a.o60SpisCode,' ','') IN @spisCodes AND p41.p41ValidUntil>GETDATE()", new { spisCodes })?.ToList() ?? new List<SpisHit>();
            }

            // kandidátní projekty: klienti, p26, historie, spisy
            var p41ids = p26.Select(p => p.p41ID).Concat(hist.Select(p => p.p41ID)).Concat(spisy.Select(p => p.p41ID)).Distinct().ToList();
            var projects = f.db.GetList<P41Info>(@"SELECT a.p41ID, a.p41Name, a.p41NameShort, a.p41Code, a.p28ID_Client, p28.p28Name as Client,
                    ISNULL(p28.p28Name+' - ','')+ISNULL(a.p41NameShort,a.p41Name)+ISNULL(' ('+a.p41Code+')','') as Combo
                FROM p41Project a INNER JOIN p07ProjectLevel p07 ON a.p07ID=p07.p07ID LEFT OUTER JOIN p28Contact p28 ON a.p28ID_Client=p28.p28ID
                WHERE a.p41ValidUntil>GETDATE() AND p07.p07Level=5 AND (a.p41ID IN @p41ids" + (clientIds.Count > 0 ? " OR a.p28ID_Client IN @clientIds" : "") + ")",
                new { p41ids = p41ids.Count == 0 ? new List<int> { 0 } : p41ids, clientIds })?.ToList() ?? new List<P41Info>();
            var byId = projects.ToDictionary(p => p.p41ID);

            foreach (var it in items)
            {
                var row = it.Row;
                var ext = it.External;
                var rowHits = hits.Where(h => ext.Any(e => e.Email == h.Email)).ToList();
                var rowClients = rowHits.Select(p => p.p28ID).Concat(rowHits.Where(p => p.p28ID_Company > 0).Select(p => p.p28ID_Company)).Distinct().ToList();
                string text = Norm(it.Text);
                string textNoSpace = text.Replace(" ", "");

                var score = new Dictionary<int, (int pts, string why)>();
                void Add(int p41id, int pts, string why)
                {
                    if (!byId.ContainsKey(p41id)) return;
                    score[p41id] = score.TryGetValue(p41id, out var s) ? (s.pts + pts, s.why) : (pts, why);
                }

                foreach (var sp in spisy.Where(s => !string.IsNullOrEmpty(s.SpisKey) && textNoSpace.Contains(Norm(s.SpisKey)))) Add(sp.p41ID, 100, $"spisová značka {sp.o60SpisCode}");
                foreach (var p in projects)
                {
                    if (!string.IsNullOrWhiteSpace(p.p41Code) && p.p41Code.Length >= 3 && Regex.IsMatch(text, @"(^|\W)" + Regex.Escape(Norm(p.p41Code)) + @"($|\W)")) Add(p.p41ID, 80, $"kód projektu {p.p41Code}");
                }
                foreach (var h in p26.Where(p => rowHits.Any(x => x.p28ID == p.p28ID)))
                {
                    var who = rowHits.First(x => x.p28ID == h.p28ID);
                    Add(h.p41ID, 50, $"{who.p28Name} je kontaktní osoba projektu");
                }
                foreach (var p in projects.Where(p => rowClients.Contains(p.p28ID_Client)))
                {
                    var who = rowHits.FirstOrDefault(x => x.p28ID == p.p28ID_Client || x.p28ID_Company == p.p28ID_Client);
                    Add(p.p41ID, 30, $"účastník {who?.Email} patří ke klientovi {p.Client}");
                }
                // shoda slov předmětu s názvem projektu (jen mezi kandidáty)
                var words = Words(text);
                foreach (int id in score.Keys.ToList().Concat(hist.Select(h => h.p41ID)).Distinct())
                {
                    if (!byId.TryGetValue(id, out var p)) continue;
                    int common = Words(Norm(p.p41NameShort ?? p.p41Name)).Count(w => words.Contains(w));
                    if (common > 0 && score.ContainsKey(id)) Add(id, Math.Min(common, 3) * 8, null);
                    else if (common >= 2) Add(id, 20, "shoda názvu projektu s předmětem");
                }
                // historie: kam uživatel zapisuje
                foreach (var h in hist.Where(h => score.ContainsKey(h.p41ID)))
                {
                    Add(h.p41ID, Math.Min(h.Cnt, 20) + ((DateTime.Today - h.Last).TotalDays < 30 ? 10 : 0), null);
                }

                if (score.Count > 0)
                {
                    var best = score.OrderByDescending(p => p.Value.pts).ThenByDescending(p => hist.FirstOrDefault(h => h.p41ID == p.Key)?.Last).First();
                    var second = score.Where(p => p.Key != best.Key).Select(p => p.Value.pts).DefaultIfEmpty(0).Max();
                    var pr = byId[best.Key];
                    row.p41ID = pr.p41ID; row.ComboP41 = pr.Combo; row.Client = pr.Client;
                    row.Reason = best.Value.why;
                    int pts = best.Value.pts;
                    bool clear = pts - second >= 25;
                    row.Confidence = pts >= 80 ? 3 : (pts >= 50 && clear) ? 3 : (pts >= 30 && clear) ? 2 : 1;
                    if (!clear && second > 0) row.Reason = Join(row.Reason, $"další možné projekty: {score.Count - 1}", "; ");
                }
                else if (rowHits.Count > 0)
                {
                    row.Reason = $"kontakt {rowHits[0].p28Name} nemá otevřený projekt";
                }
                else if (ext.Count > 0)
                {
                    row.Reason = "e-mail účastníka není v kontaktech";
                }
                else
                {
                    row.Reason = "bez externích účastníků";
                }

                // aktivita: nejčastější v projektu, jinak nejčastější vůbec
                var act = histP32.Where(p => p.p41ID == row.p41ID).OrderByDescending(p => p.Cnt).FirstOrDefault()
                       ?? histP32.GroupBy(p => new { p.p32ID, p.p32Name }).Select(g => new HistP32 { p32ID = g.Key.p32ID, p32Name = g.Key.p32Name, Cnt = g.Sum(x => x.Cnt) }).OrderByDescending(p => p.Cnt).FirstOrDefault();
                if (act != null) { row.p32ID = act.p32ID; row.ComboP32 = act.p32Name; }

                // předvybrat jen řádky s nalezeným projektem
                if (row.State == OutlookP31State.New) row.IsSelected = row.p41ID > 0;
            }
        }

        private static void FindDuplicatesAndOverlaps(BL.Factory f, List<OutlookP31Row> rows)
        {
            if (rows.Count == 0) return;
            var keys = rows.Select(r => r.Key).ToList();
            var d1 = rows.Min(r => r.Date); var d2 = rows.Max(r => r.Date).AddDays(1);
            var exist = f.db.GetList<P31Exist>(@"SELECT a.p31ID, a.p31ExternalCode, a.p31DateTimeFrom_Orig, a.p31DateTimeUntil_Orig, ISNULL(a.p31Text,'') as Project
                FROM p31Worksheet a WHERE a.j02ID=@j02id AND (a.p31ExternalCode IN @keys OR (a.p31Date>=@d1 AND a.p31Date<@d2 AND a.p31DateTimeFrom_Orig IS NOT NULL))",
                new { j02id = f.CurrentUser.pid, keys, d1, d2 })?.ToList() ?? new List<P31Exist>();

            foreach (var row in rows)
            {
                var dup = exist.FirstOrDefault(p => p.p31ExternalCode == row.Key);
                if (dup != null)
                {
                    row.State = OutlookP31State.Duplicate; row.IsSelected = false; row.p31ID = dup.p31ID;
                    row.Message = "Už zapsáno";
                    continue;
                }
                if (row.State != OutlookP31State.New) continue;
                DateTime from = row.Date + TimeSpan.Parse(row.TimeFrom), until = row.Date + TimeSpan.Parse(row.TimeUntil);
                if (until <= from) until = until.AddDays(1);
                var ov = exist.FirstOrDefault(p => p.p31DateTimeFrom_Orig < until && p.p31DateTimeUntil_Orig > from);
                if (ov != null) row.Warning = Join(row.Warning, $"V tomto čase už máte úkon {ov.p31DateTimeFrom_Orig:HH:mm}–{ov.p31DateTimeUntil_Orig:HH:mm}.");
            }
        }

        /// <summary>Uloží vybrané řádky jako časové úkony. Vrací počet uložených.</summary>
        public static int Save(BL.Factory f, List<OutlookP31Row> rows)
        {
            int saved = 0;
            foreach (var row in rows.Where(p => p.IsSelected && (p.State == OutlookP31State.New || p.State == OutlookP31State.Error || p.State == OutlookP31State.Skipped)))
            {
                string err = null;
                if (row.p41ID == 0) err = "Vyberte projekt.";
                else if (row.p32ID == 0) err = "Vyberte aktivitu.";
                BO.p32Activity recP32 = null;
                if (err == null)
                {
                    recP32 = f.p32ActivityBL.Load(row.p32ID);
                    if (recP32 == null) err = "Aktivita neexistuje.";
                }
                if (err == null)
                {
                    var rec = new BO.p31WorksheetEntryInput
                    {
                        j02ID = f.CurrentUser.pid,
                        p41ID = row.p41ID,
                        p32ID = row.p32ID,
                        p34ID = recP32.p34ID,
                        p31Text = row.p31Text,
                        p31TextInternal = row.p31TextInternal,
                        p31ExternalCode = row.Key,
                        p31HoursEntryflag = BO.p31HoursEntryFlagENUM.Hodiny,
                        TimeFrom = row.TimeFrom,
                        TimeUntil = row.TimeUntil,
                        Value_Orig = row.Hours
                    };
                    rec.Addp31Date(row.Date);
                    f.CurrentUser.Messages4Notify = null;
                    int pid = f.p31WorksheetBL.SaveOrigRecord(rec, BO.p33IdENUM.Cas, null);
                    if (pid > 0)
                    {
                        row.State = OutlookP31State.Saved; row.p31ID = pid; row.IsSelected = false; row.Message = null; saved++;
                    }
                    else
                    {
                        err = f.CurrentUser.Messages4Notify?.LastOrDefault()?.Value ?? "Úkon se nepodařilo uložit.";
                    }
                    f.CurrentUser.Messages4Notify = null;
                }
                if (err != null) { row.State = OutlookP31State.Error; row.Message = err; }
            }
            return saved;
        }

        // ---------------- pomocné ----------------

        private static string Domain(string mail)
        {
            int i = mail?.LastIndexOf('@') ?? -1;
            return i > 0 ? mail.Substring(i + 1).ToLowerInvariant() : null;
        }

        private static string Join(string a, string b, string sep = " ") => string.IsNullOrEmpty(a) ? b : (string.IsNullOrEmpty(b) ? a : a + sep + b);

        /// <summary>Malá písmena, bez diakritiky, jednoduché mezery.</summary>
        private static string Norm(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            var d = s.ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(d.Length);
            foreach (char c in d)
            {
                if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark) sb.Append(c);
            }
            return Regex.Replace(sb.ToString(), @"\s+", " ").Trim();
        }

        private static readonly HashSet<string> StopWords = new HashSet<string> { "a", "i", "o", "u", "v", "ve", "s", "se", "z", "ze", "k", "ke", "na", "do", "od", "po", "pro", "za", "pri", "the", "and", "of", "re", "fw", "fwd", "sro", "as", "spol", "schuzka", "jednani", "call", "meeting", "porada", "teams" };
        private static HashSet<string> Words(string normalized)
        {
            return new HashSet<string>(Regex.Split(normalized ?? "", @"[^\p{L}\p{N}]+").Where(w => w.Length >= 3 && !StopWords.Contains(w)));
        }

        /// <summary>Kandidáti na spisovou značku v textu, bez mezer: „21 C 123/2025“, „KSPH 39 INS 1234/2024“, „Nc 55/2026“.</summary>
        private static IEnumerable<string> SpisCandidates(string text)
        {
            if (string.IsNullOrEmpty(text)) yield break;
            foreach (Match m in Regex.Matches(text, @"(?:[A-Z]{2,5}\s*)?\d{0,4}\s*[A-Za-z]{1,5}\s*\d{1,6}\s*/\s*\d{2,4}(?:-\d+)?"))
            {
                yield return Regex.Replace(m.Value, @"\s+", "");
            }
        }
    }
}
