namespace UI.Code
{
    /// <summary>
    /// Časová stopa záznamu (záložka Poznámky, Views/Record/TabB05.cshtml):
    /// poznámky a posuny workflow (b05), soubory záznamu (o27), u projektu úkoly (p56) a termíny (o22), u kontaktu projekty (p41).
    /// Data se čtou přes BL (platí oprávnění uživatele), jen čtení.
    /// </summary>
    public class TimelineItem
    {
        public string Kind { get; set; }        // rec | note | wrk | task | term | project | file | pay | proforma | dun | mail | invoice
        public DateTime When { get; set; }      // řazení a seskupení
        public bool IsFuture { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }       // barva bubliny (stav / typ), null = podle druhu
        public string Title { get; set; }
        public string Action { get; set; }     // popisek události místo druhu (např. „Založen úkol“)
        public string Author { get; set; }
        public string Prefix { get; set; }      // pro kontextové menu / otevření
        public int Pid { get; set; }
        public BO.b05Workflow_History B05 { get; set; }
        public BO.p56Task P56 { get; set; }
        public BO.o22Milestone O22 { get; set; }
        public BO.p41Project P41 { get; set; }
        public BO.o27Attachment O27 { get; set; }
        public List<BO.o27Attachment> Files { get; set; }   // přílohy poznámky
        public bool HasReminder { get; set; }
        public string SearchText { get; set; }
        public List<TimelineTag> Tags { get; set; }         // obecné štítky
        public string TitleHref { get; set; }               // odkaz titulku (javascript:…), jinak podle druhu
        public bool HideMenu { get; set; }                  // bez kontextového menu (záznam nemá vlastní menu)
    }

    public class TimelineTag
    {
        public string Text { get; set; }
        public string Icon { get; set; }
        public string Css { get; set; }     // např. tl-tag-late
    }

    public static class RecordTimeline
    {
        public const int MaxPerKind = 300;

        private class RecStamp
        {
            public string UserInsert { get; set; }
            public DateTime? DateInsert { get; set; }
            public string UserUpdate { get; set; }
            public DateTime? DateUpdate { get; set; }
            public DateTime? ValidUntil { get; set; }
        }
        private class LoginName { public string Login { get; set; } public string Name { get; set; } }

        /// <summary>Výchozí začátek období podle entity: uživatel má velmi mnoho úkolů → posledních 6 měsíců; ostatní záznamy vše.</summary>
        public static DateTime? DefaultFrom(string prefix)
        {
            return BO.Code.Entity.GetPrefixDb(prefix ?? "") == "j02" ? DateTime.Today.AddMonths(-6) : (DateTime?)null;
        }

        /// <param name="from">začátek období (null = vše); budoucí události se zobrazují vždy</param>
        public static List<TimelineItem> Build(BL.Factory f, string prefix, int pid, DateTime? from = null)
        {
            var ret = new List<TimelineItem>();
            if (string.IsNullOrEmpty(prefix) || pid == 0) return ret;
            string prefixDb = BO.Code.Entity.GetPrefixDb(prefix);

            // poznámky a workflow
            var lisB05 = f.WorkflowBL.GetList_b05(prefix, pid, 0, 0, 0).ToList();
            var withReminder = new HashSet<int>();
            Dictionary<int, List<BO.o27Attachment>> noteFiles = new Dictionary<int, List<BO.o27Attachment>>();
            if (lisB05.Count > 0)
            {
                foreach (var r in f.o24ReminderBL.GetList("b05", string.Join(",", lisB05.Select(p => p.pid))) ?? Enumerable.Empty<BO.o24Reminder>())
                {
                    withReminder.Add(r.o24RecordPid);
                }
                foreach (var a in f.o27AttachmentBL.GetList(new BO.myQueryO27() { b05_recordentity = prefixDb, b05_recordpid = pid })
                    .Where(p => !(p.o27CallerFlag == BO.o27CallerFlagENUM.Notepad && p.o27WwwRootFolder != null)))
                {
                    if (!noteFiles.TryGetValue(a.o27RecordPid, out var l)) noteFiles[a.o27RecordPid] = l = new List<BO.o27Attachment>();
                    l.Add(a);
                }
            }
            foreach (var c in lisB05)
            {
                bool isWrk = c.b06ID > 0 || (c.b02ID_To > 0 && c.b02ID_To != c.b02ID_From);
                string title = c.b05Name;
                if (isWrk)
                {
                    title = !string.IsNullOrWhiteSpace(c.b06Name) ? c.b06Name : (c.b02Name_To ?? f.tra("Změna stavu"));
                }
                ret.Add(new TimelineItem
                {
                    Kind = isWrk ? "wrk" : "note",
                    When = c.DateInsert ?? c.b05Date ?? DateTime.MinValue,
                    Icon = isWrk ? "swap_horiz" : (c.p56ID > 0 ? "task_alt" : (c.o22ID > 0 ? "event" : "sticky_note_2")),
                    Color = isWrk && !string.IsNullOrEmpty(c.b02Color) ? c.b02Color : null,
                    Title = title,
                    Author = c.j02ID_Sys > 0 ? c.Person : c.UserInsert,
                    Prefix = "b05",
                    Pid = c.pid,
                    B05 = c,
                    Files = noteFiles.TryGetValue(c.pid, out var nf) ? nf : null,
                    HasReminder = withReminder.Contains(c.pid),
                    SearchText = $"{c.b05Name} {c.b06Name} {c.b02Name_From} {c.b02Name_To} {c.b05NotepadText200} {c.p56Name} {c.o22Name} {c.Person}"
                });
            }

            // soubory záznamu
            foreach (var a in f.o27AttachmentBL.GetList(new BO.myQueryO27() { entity = prefixDb, recpid = pid }).Take(MaxPerKind))
            {
                ret.Add(new TimelineItem
                {
                    Kind = "file",
                    When = a.DateInsert ?? DateTime.MinValue,
                    Icon = a.IsMail ? "mail" : "attach_file",
                    Title = a.IsMail ? (a.o27MailSubject ?? a.o27OriginalFileName) : a.o27OriginalFileName,
                    Author = a.UserInsert,
                    Prefix = "o27",
                    Pid = a.pid,
                    O27 = a,
                    SearchText = $"{a.o27OriginalFileName} {a.o27Name} {a.o27MailSubject} {a.o27MailSenderName}"
                });
            }

            // projekt: úkoly a termíny
            if (prefixDb == "p41")
            {
                AddTasks(f, ret, Period(new BO.myQueryP56() { p41id = pid, IsRecordValid = null }, from));
                AddTerms(f, ret, new BO.myQueryO22() { p41id = pid, IsRecordValid = null });
                AddInvoices(f, ret, new BO.myQueryP91() { p41id = pid }, 0);
            }

            // vyúčtování: úhrady, spárované zálohy, upomínky, elektronické odeslání
            if (prefixDb == "p91")
            {
                AddInvoiceEvents(f, ret, pid);
            }

            // uživatel: přidělené úkoly a termíny, vlastní projekty
            if (prefixDb == "j02")
            {
                AddUserActivity(f, ret, pid, from);
            }

            // kontakt: projekty
            if (prefixDb == "p28")
            {
                AddInvoices(f, ret, new BO.myQueryP91() { p28id = pid }, pid);     // odběratel faktury nebo klient fakturovaného projektu
                foreach (var p in f.p41ProjectBL.GetList(new BO.myQueryP41("p41") { p28id = pid, IsRecordValid = null, TopRecordsOnly = MaxPerKind }))
                {
                    ret.Add(new TimelineItem
                    {
                        Kind = "project",
                        When = p.DateInsert ?? DateTime.MinValue,
                        Icon = "work_outline",
                        Color = !string.IsNullOrEmpty(p.b02Color) ? p.b02Color : null,
                        Title = p.p41Name,
                        Author = p.Owner,
                        Prefix = "p41",
                        Pid = p.pid,
                        P41 = p,
                        SearchText = $"{p.p41Name} {p.p41Code} {p.p42Name} {p.b02Name} {p.Owner}"
                    });
                }
            }

            // stopa samotného záznamu: založení, poslední úprava, archiv
            ret.AddRange(RecordStamps(f, prefix, prefixDb, pid));

            // loginy → jména (soubory, záznam, poznámky bez osoby)
            var logins = ret.Where(p => !string.IsNullOrEmpty(p.Author) && !p.Author.Contains(' ')).Select(p => p.Author).Distinct().ToList();
            if (logins.Count > 0)
            {
                try
                {
                    var names = f.db.GetList<LoginName>("SELECT j02Login as Login, j02FirstName+' '+j02LastName as Name FROM j02User WHERE j02Login IN @logins", new { logins })
                        .Where(p => p.Login != null).GroupBy(p => p.Login, StringComparer.OrdinalIgnoreCase).ToDictionary(g => g.Key, g => g.First().Name, StringComparer.OrdinalIgnoreCase);
                    foreach (var it in ret)
                    {
                        if (it.Author != null && names.TryGetValue(it.Author, out var n) && !string.IsNullOrWhiteSpace(n)) it.Author = n.Trim();
                    }
                }
                catch { }
            }

            if (from != null)
            {
                ret = ret.Where(p => p.When >= from.Value || p.IsFuture).ToList();
            }
            return ret.OrderByDescending(p => p.When).ThenByDescending(p => p.Kind == "rec" ? 0 : 1).ThenByDescending(p => p.Pid).ToList();
        }

        /// <summary>Stránka uživatele: úkoly a termíny, kde je řešitelem, jeho projekty; Vykázané úkony a přihlášení záměrně ne (příliš mnoho událostí; přihlášení mají vlastní záložky LOGIN/PING log).</summary>
        // úkoly omezit na období už v dotazu (p56TaskBL nepoužívá TopRecordsOnly)
        private static BO.myQueryP56 Period(BO.myQueryP56 q, DateTime? from)
        {
            if (from != null)
            {
                q.period_field = "p56DateInsert";
                q.global_d1 = from;
                q.global_d2 = new DateTime(2999, 12, 31);
            }
            return q;
        }

        private static void AddUserActivity(BL.Factory f, List<TimelineItem> ret, int j02id, DateTime? from)
        {
            // úkoly a termíny, kde je uživatel v roli (řešitel apod.)
            AddTasks(f, ret, Period(new BO.myQueryP56() { j02id = j02id, IsRecordValid = null }, from));
            AddTerms(f, ret, new BO.myQueryO22() { j02id = j02id, IsRecordValid = null });

            // projekty, které vlastní
            foreach (var p in f.p41ProjectBL.GetList(new BO.myQueryP41("p41") { j02id_owner = j02id, IsRecordValid = null, TopRecordsOnly = MaxPerKind }))
            {
                ret.Add(new TimelineItem
                {
                    Kind = "project",
                    Action = f.tra("Založen projekt"),
                    When = p.DateInsert ?? DateTime.MinValue,
                    Icon = "work_outline",
                    Color = !string.IsNullOrEmpty(p.b02Color) ? p.b02Color : null,
                    Title = p.p41Name,
                    Author = p.UserInsert,
                    Prefix = "p41",
                    Pid = p.pid,
                    P41 = p,
                    SearchText = $"{p.p41Name} {p.p41Code} {p.p42Name} {p.b02Name}"
                });
            }
        }

        /// <summary>Vyúčtování (p91): úhrady (p94), spárování s uhrazenou zálohou (p99), upomínky (p84), elektronické odeslání (x40).</summary>
        private static void AddInvoiceEvents(BL.Factory f, List<TimelineItem> ret, int p91id)
        {
            var recP91 = f.p91InvoiceBL.Load(p91id);
            string cur = recP91?.j27Code ?? "";
            string Money(double d) => BO.Code.Bas.Number2String(d) + (cur.Length > 0 ? " " + cur : "");

            foreach (var c in f.p91InvoiceBL.GetList_p94(p91id) ?? Enumerable.Empty<BO.p94Invoice_Payment>())
            {
                var tags = new List<TimelineTag>();
                if (!string.IsNullOrWhiteSpace(c.p94Code)) tags.Add(new TimelineTag { Text = c.p94Code, Icon = "tag" });
                if (!string.IsNullOrWhiteSpace(c.p94Description)) tags.Add(new TimelineTag { Text = c.p94Description, Css = "tl-tag-desc" });
                ret.Add(new TimelineItem
                {
                    Kind = "pay", Action = f.tra("Úhrada faktury"), When = c.p94Date,
                    Icon = "payments", Color = "#198754", Title = Money(c.p94Amount),
                    Author = c.UserInsert, Prefix = "p94", Pid = c.pid, HideMenu = true, Tags = tags,
                    SearchText = $"úhrada platba {c.p94Code} {c.p94Description} {c.p94Amount}"
                });
            }

            foreach (var c in f.p91InvoiceBL.GetList_p99(0, p91id, 0) ?? Enumerable.Empty<BO.p99Invoice_Proforma>())
            {
                var tags = new List<TimelineTag>();
                if (c.p99Amount_Vat != 0) tags.Add(new TimelineTag { Text = f.tra("bez DPH") + " " + Money(c.p99Amount_WithoutVat) + ", " + f.tra("DPH") + " " + Money(c.p99Amount_Vat) });
                if (!string.IsNullOrWhiteSpace(c.p82Code)) tags.Add(new TimelineTag { Text = f.tra("úhrada zálohy") + " " + c.p82Code, Icon = "receipt" });
                ret.Add(new TimelineItem
                {
                    Kind = "proforma", Action = f.tra("Spárováno se zálohou") + (c.p90Code != null ? " " + c.p90Code : ""),
                    When = c.DateInsert ?? DateTime.MinValue, Icon = "link", Color = "#0e7490", Title = Money(c.p99Amount),
                    Author = c.UserInsert, Prefix = "p90", Pid = c.p90ID, HideMenu = c.p90ID == 0, Tags = tags,
                    TitleHref = c.p90ID > 0 ? $"javascript:_edit('p90',{c.p90ID})" : null,
                    SearchText = $"záloha párování {c.p90Code} {c.p82Code} {c.p99Amount}"
                });
            }

            foreach (var c in f.p84UpominkaBL.GetList(new BO.myQueryP84() { p91id = p91id, IsRecordValid = null }) ?? Enumerable.Empty<BO.p84Upominka>())
            {
                var tags = new List<TimelineTag>();
                if (!string.IsNullOrWhiteSpace(c.p83Name)) tags.Add(new TimelineTag { Text = c.p83Name, Icon = "category" });
                if (c.p84AmountDebt != 0) tags.Add(new TimelineTag { Text = f.tra("dluh") + " " + BO.Code.Bas.Number2String(c.p84AmountDebt) + (c.j27Code != null ? " " + c.j27Code : ""), Css = "tl-tag-late" });
                if (!string.IsNullOrWhiteSpace(c.p84Code)) tags.Add(new TimelineTag { Text = c.p84Code, Icon = "tag" });
                ret.Add(new TimelineItem
                {
                    Kind = "dun", Action = f.tra("Vystavena upomínka") + (c.p84Index > 0 ? $" ({c.p84Index}.)" : ""),
                    When = c.p84Date, Icon = "notification_important", Color = "#b02a37",
                    Title = !string.IsNullOrWhiteSpace(c.p84Name) ? c.p84Name : f.tra("Upomínka"),
                    Author = c.Owner ?? c.UserInsert, Prefix = "p84", Pid = c.pid, Tags = tags,
                    SearchText = $"upomínka {c.p84Name} {c.p84Code} {c.p83Name}"
                });
            }

            // x40 nemá sloupec x40ValidUntil → bez filtru časové platnosti (jako _p91RecHeaderBox)
            foreach (var c in f.MailBL.GetList(new BO.myQueryX40() { p91id = p91id, IsRecordValid = null }) ?? Enumerable.Empty<BO.x40MailQueue>())
            {
                var tags = new List<TimelineTag>();
                if (!string.IsNullOrWhiteSpace(c.x40Recipient)) tags.Add(new TimelineTag { Text = c.x40Recipient.Length > 80 ? c.x40Recipient.Substring(0, 78) + "…" : c.x40Recipient, Icon = "person" });
                switch (c.x40Status)
                {
                    case BO.x40StateENUM.IsProceeded: tags.Add(new TimelineTag { Text = f.tra("odesláno"), Icon = "done", Css = "tl-tag-ok" }); break;
                    case BO.x40StateENUM.IsError: tags.Add(new TimelineTag { Text = f.tra("chyba") + (string.IsNullOrWhiteSpace(c.x40ErrorMessage) ? "" : ": " + (c.x40ErrorMessage.Length > 80 ? c.x40ErrorMessage.Substring(0, 78) + "…" : c.x40ErrorMessage)), Icon = "error_outline", Css = "tl-tag-late" }); break;
                    case BO.x40StateENUM.InQueque: tags.Add(new TimelineTag { Text = f.tra("ve frontě"), Icon = "schedule" }); break;
                    case BO.x40StateENUM.WaitOnConfirm: tags.Add(new TimelineTag { Text = f.tra("čeká na potvrzení"), Icon = "hourglass_empty" }); break;
                    case BO.x40StateENUM.IsStopped: tags.Add(new TimelineTag { Text = f.tra("zastaveno"), Icon = "block" }); break;
                }
                if (!string.IsNullOrWhiteSpace(c.x40Attachments)) tags.Add(new TimelineTag { Text = c.x40Attachments.Length > 60 ? c.x40Attachments.Substring(0, 58) + "…" : c.x40Attachments, Icon = "attach_file" });
                ret.Add(new TimelineItem
                {
                    Kind = "mail", Action = f.tra("Odesláno e-mailem"),
                    When = c.x40DatetimeProcessed ?? c.DateInsert ?? DateTime.MinValue,
                    Icon = "forward_to_inbox", Color = c.x40Status == BO.x40StateENUM.IsError ? "#b02a37" : "#4169e1",
                    Title = !string.IsNullOrWhiteSpace(c.x40Subject) ? c.x40Subject : f.tra("(bez předmětu)"),
                    Author = c.UserInsert, Prefix = "x40", Pid = c.pid, HideMenu = true,
                    TitleHref = $"javascript:_window_open('/Mail/Record?pid={c.pid}')", Tags = tags,
                    SearchText = $"email e-mail odesláno {c.x40Subject} {c.x40Recipient} {c.x40CC}"
                });
            }
        }

        /// <summary>Nová vyúčtování: u projektu faktury s úkony projektu, u kontaktu jako odběratel faktury nebo klient fakturovaného projektu.</summary>
        private static void AddInvoices(BL.Factory f, List<TimelineItem> ret, BO.myQueryP91 q, int p28id_contact)
        {
            if (!f.CurrentUser.j04IsModule_p91) return;
            q.IsRecordValid = null;
            foreach (var c in f.p91InvoiceBL.GetList(q) ?? Enumerable.Empty<BO.p91Invoice>())
            {
                var tags = new List<TimelineTag>();
                tags.Add(new TimelineTag { Text = BO.Code.Bas.Number2String(c.p91Amount_WithoutVat) + " " + c.j27Code + " " + f.tra("bez DPH"), Icon = "payments" });
                if (p28id_contact > 0)
                {
                    tags.Add(c.p28ID == p28id_contact
                        ? new TimelineTag { Text = f.tra("odběratel faktury"), Icon = "person" }
                        : new TimelineTag { Text = f.tra("klient projektu") + (string.IsNullOrWhiteSpace(c.p91Client) ? "" : " · " + f.tra("odběratel") + " " + c.p91Client), Icon = "work_outline" });
                }
                else if (!string.IsNullOrWhiteSpace(c.p91Client))
                {
                    tags.Add(new TimelineTag { Text = c.p91Client, Icon = "person" });
                }
                if (c.p91IsDraft) tags.Add(new TimelineTag { Text = f.tra("koncept"), Icon = "edit" });
                else if (c.p91Amount_Debt > 0.009 && c.p91DateMaturity < DateTime.Today) tags.Add(new TimelineTag { Text = f.tra("po splatnosti") + " · " + BO.Code.Bas.Number2String(c.p91Amount_Debt) + " " + c.j27Code, Icon = "schedule", Css = "tl-tag-late" });
                else if (c.p91Amount_Debt > 0.009) tags.Add(new TimelineTag { Text = f.tra("splatnost") + " " + c.p91DateMaturity.ToString("d.M.yyyy"), Icon = "schedule" });
                else tags.Add(new TimelineTag { Text = f.tra("uhrazeno"), Icon = "done", Css = "tl-tag-ok" });
                if (!string.IsNullOrWhiteSpace(c.b02Name)) tags.Add(new TimelineTag { Text = c.b02Name, Icon = "flag" });
                ret.Add(new TimelineItem
                {
                    Kind = "invoice",
                    Action = c.p91IsDraft ? f.tra("Koncept vyúčtování") : f.tra("Vyúčtování"),
                    When = c.DateInsert ?? c.p91Date,
                    Icon = "receipt_long",
                    Title = string.IsNullOrWhiteSpace(c.p91Code) ? f.tra("(bez čísla)") : c.p91Code,
                    Author = c.UserInsert, Prefix = "p91", Pid = c.pid, Tags = tags,
                    SearchText = $"vyúčtování faktura {c.p91Code} {c.p91Client} {c.b02Name}"
                });
            }
        }

        private static void AddTasks(BL.Factory f, List<TimelineItem> ret, BO.myQueryP56 q)
        {
            foreach (var t in f.p56TaskBL.GetList(q))
                {
                    ret.Add(new TimelineItem
                    {
                        Kind = "task",
                        Action = f.tra("Založen úkol") + (t.p57Name != null && !string.Equals(t.p57Name, f.tra("Úkol"), StringComparison.OrdinalIgnoreCase) ? " · " + t.p57Name : ""),
                        When = t.DateInsert ?? t.p56PlanFrom ?? DateTime.MinValue,
                        Icon = "add_task",
                        Color = !string.IsNullOrEmpty(t.b02Color) ? t.b02Color : null,
                        Title = t.p56Name,
                        Author = t.UserInsert,      // kdo úkol založil; řešitel je štítek
                        Prefix = "p56",
                        Pid = t.pid,
                        P56 = t,
                        SearchText = $"{t.p56Name} {t.p56Code} {t.p57Name} {t.b02Name} {t.Owner} založen"
                    });
                    if (t.isclosed && t.ValidUntil != null && t.ValidUntil < DateTime.Now)
                    {
                        ret.Add(new TimelineItem
                        {
                            Kind = "task",
                            Action = f.tra("Úkol uzavřen"),
                            When = t.ValidUntil.Value,
                            Icon = "task_alt",
                            Color = "#6c757d",
                            Title = t.p56Name,
                            Author = t.UserUpdate,
                            Prefix = "p56",
                            Pid = t.pid,
                            SearchText = $"{t.p56Name} {t.p56Code} uzavřen"
                        });
                    }
                }
        }

        private static void AddTerms(BL.Factory f, List<TimelineItem> ret, BO.myQueryO22 q)
        {
            foreach (var m in f.o22MilestoneBL.GetList(q))
                {
                    DateTime when = m.o22PlanFrom ?? m.o22PlanUntil ?? m.DateInsert ?? DateTime.MinValue;
                    ret.Add(new TimelineItem
                    {
                        Kind = "term",
                        When = when,
                        IsFuture = when > DateTime.Now,
                        Icon = m.o21TypeFlag == BO.o21TypeFlagEnum.Lhuta ? "hourglass_bottom" : (m.o21TypeFlag == BO.o21TypeFlagEnum.Milnik ? "flag" : "event"),
                        Color = !string.IsNullOrEmpty(m.o21Color) ? m.o21Color : null,
                        Title = !string.IsNullOrWhiteSpace(m.o22Name) ? m.o22Name : m.o21Name,
                        Author = m.Owner,
                        Prefix = "o22",
                        Pid = m.pid,
                        O22 = m,
                        SearchText = $"{m.o22Name} {m.o21Name} {m.o22Location} {m.Owner}"
                    });
                }
        }

        private static List<TimelineItem> RecordStamps(BL.Factory f, string prefix, string prefixDb, int pid)
        {
            var ret = new List<TimelineItem>();
            RecStamp st = null;
            try
            {
                string tab = BO.Code.Entity.GetEntity(prefixDb);
                var ce = f.EProvider?.ByPrefix(prefixDb);
                bool withValidity = ce == null || !ce.IsWithoutValidity;
                string sql = $"SELECT a.{prefixDb}UserInsert as UserInsert, a.{prefixDb}DateInsert as DateInsert, a.{prefixDb}UserUpdate as UserUpdate, a.{prefixDb}DateUpdate as DateUpdate"
                    + (withValidity ? $", a.{prefixDb}ValidUntil as ValidUntil" : "")
                    + $" FROM {tab} a WHERE a.{prefixDb}ID=@pid";
                st = f.db.Load<RecStamp>(sql, new { pid });
            }
            catch { return ret; }   // tabulka bez standardních sloupců – stopa záznamu se nezobrazí
            if (st == null || st.DateInsert == null) return ret;

            string alias = BO.Code.Entity.GetAlias(prefixDb);
            ret.Add(new TimelineItem
            {
                Kind = "rec", When = st.DateInsert.Value, Icon = "add_circle_outline",
                Title = f.tra("Záznam založen") + (string.IsNullOrEmpty(alias) ? "" : $" ({alias})"),
                Author = st.UserInsert, Prefix = prefixDb, Pid = pid, SearchText = "založen založení"
            });
            bool archived = st.ValidUntil != null && st.ValidUntil < DateTime.Now;
            if (st.DateUpdate != null && (st.DateUpdate.Value - st.DateInsert.Value).TotalMinutes >= 1 && !(archived && Math.Abs((st.DateUpdate.Value - st.ValidUntil.Value).TotalMinutes) < 2))
            {
                ret.Add(new TimelineItem
                {
                    Kind = "rec", When = st.DateUpdate.Value, Icon = "edit_note",
                    Title = f.tra("Naposledy upraveno"), Author = st.UserUpdate, Prefix = prefixDb, Pid = pid, SearchText = "upraveno úprava změna"
                });
            }
            if (archived)
            {
                ret.Add(new TimelineItem
                {
                    Kind = "rec", When = st.ValidUntil.Value, Icon = "inventory_2", Color = "#6c757d",
                    Title = f.tra("Přesunuto do archivu"), Author = st.UserUpdate, Prefix = prefixDb, Pid = pid, SearchText = "archiv archivováno"
                });
            }
            return ret;
        }

        /// <summary>Nadpis skupiny podle data (Nadcházející / Dnes / Včera / Tento týden / měsíc rok).</summary>
        public static string GroupLabel(BL.Factory f, TimelineItem it)
        {
            var d = it.When.Date; var today = DateTime.Today;
            if (it.When > DateTime.Now && d > today) return f.tra("Nadcházející");
            if (d == today) return f.tra("Dnes");
            if (d == today.AddDays(-1)) return f.tra("Včera");
            int dow = ((int)today.DayOfWeek + 6) % 7;
            if (d >= today.AddDays(-dow)) return f.tra("Tento týden");
            string[] months = { "leden", "únor", "březen", "duben", "květen", "červen", "červenec", "srpen", "září", "říjen", "listopad", "prosinec" };
            string m = f.tra(months[d.Month - 1]);
            return d.Year == today.Year ? char.ToUpper(m[0]) + m.Substring(1) : $"{char.ToUpper(m[0]) + m.Substring(1)} {d.Year}";
        }
    }
}
