using System.Collections.Concurrent;

namespace BL
{
    /// <summary>
    /// Check-list: jednoduché položky (text + hotovo) v tabulce p65Checklist.
    /// - V úkolu (prefix "p56") se položky odškrtávají. Hotová položka je uzavřený záznam: p65ValidUntil = čas odškrtnutí,
    ///   otevřená má p65ValidUntil 1.1.3000.
    /// - V opakovaném úkolu (prefix "p58") jsou položky vzorem: neodškrtávají se a každý vygenerovaný úkol si je zkopíruje.
    /// Seznam vždy vrací otevřené i hotové položky - nesmí jít přes dotazy, které uzavřené záznamy skrývají.
    /// Odškrtnutí, přidání, přejmenování, smazání i pořadí se ukládají hned, nezávisle na uložení karty záznamu.
    /// </summary>
    public interface Ip65ChecklistBL
    {
        public bool IsAvailable();
        public bool CanRead(int pid, BO.p56Task rec = null, string prefix = "p56");
        public bool CanEdit(int pid, BO.p56Task rec = null, string prefix = "p56");
        public IEnumerable<BO.p65Checklist> GetList(int pid, string prefix = "p56");
        public BO.p65Checklist Load(int p65id);
        public int Add(int pid, List<string> names, string prefix = "p56");
        public bool SetDone(int p65id, bool done);
        public bool Rename(int p65id, string name);
        public bool Delete(int p65id);
        public bool Reorder(int pid, List<int> p65ids, string prefix = "p56");
        public int CopyFromRecurrence(int p58id, int p56id);
        public List<string> ParseLines(string text);
    }

    class p65ChecklistBL : BaseBL, Ip65ChecklistBL
    {
        public const int MaxNameLength = 500;
        public const int MaxItems = 200;

        //existence tabulky se drží v paměti, aby se netestovala při každém zobrazení úkolu
        private static readonly ConcurrentDictionary<string, DateTime> _available = new ConcurrentDictionary<string, DateTime>();
        private static readonly ConcurrentDictionary<string, DateTime> _unavailable = new ConcurrentDictionary<string, DateTime>();

        public p65ChecklistBL(BL.Factory mother) : base(mother)
        {

        }

        private int Me => _mother.CurrentUser.pid;

        private int GetInt(string strSQL, object param)
        {
            var c = _db.Load<BO.GetInteger>(strSQL, param);
            return c == null ? 0 : c.Value;
        }

        //sloupec vlastníka položky; jiný prefix než p56/p58 se nepřipouští (název sloupce jde do textu SQL)
        private static string OwnerField(string prefix)
        {
            if (prefix == "p58") return "p58ID";
            if (prefix == "p56" || string.IsNullOrEmpty(prefix)) return "p56ID";
            return null;
        }

        /// <summary>Je v databázi licence tabulka p65Checklist ve verzi s vazbou na opakovaný úkol? Bez ní se check-list v aplikaci nenabízí.</summary>
        public bool IsAvailable()
        {
            if (_mother.CurrentUser == null || _db == null) return false;
            string strKey = $"{_mother.App.HostingMode}|{_mother.CurrentUser.x01LoginDomain}";
            if (_available.ContainsKey(strKey)) return true;
            if (_unavailable.TryGetValue(strKey, out DateTime d) && d > DateTime.Now) return false;

            if (GetInt("SELECT CASE WHEN COL_LENGTH('dbo.p65Checklist','p58ID') IS NULL THEN 0 ELSE 1 END as Value", null) == 1)
            {
                _available[strKey] = DateTime.Now;
                return true;
            }
            _unavailable[strKey] = DateTime.Now.AddMinutes(5);
            return false;
        }

        public bool CanRead(int pid, BO.p56Task rec = null, string prefix = "p56")
        {
            if (pid <= 0 || OwnerField(prefix) == null || !IsAvailable()) return false;
            if (prefix == "p58") return CanEdit(pid, null, prefix);     //vzor je vidět jen v kartě opakovaného úkolu

            if (rec == null) rec = _mother.p56TaskBL.Load(pid);
            if (rec == null) return false;
            if (_mother.p56TaskBL.InhaleRecDisposition(pid, rec).ReadAccess) return true;
            return CanEdit(pid, rec, prefix);
        }

        /// <summary>
        /// Úkol: měnit check-list smí ten, kdo má v úkolu jakoukoli roli (osobně, přes tým nebo roli pro všechny), vlastník úkolu a správce.
        /// Opakovaný úkol: ten, kdo smí upravovat jeho kartu (správce, oprávnění zakládat projekty, vlastník záznamu).
        /// </summary>
        public bool CanEdit(int pid, BO.p56Task rec = null, string prefix = "p56")
        {
            if (pid <= 0 || OwnerField(prefix) == null || !IsAvailable()) return false;

            if (prefix == "p58")
            {
                if (_mother.CurrentUser.IsAdmin || _mother.CurrentUser.TestPermission(BO.PermValEnum.GR_p41_Creator)) return true;
                var recP58 = _mother.p58TaskRecurrenceBL.Load(pid);
                return recP58 != null && recP58.j02ID_Owner == Me;
            }

            if (!_mother.CurrentUser.j04IsModule_p56) return false;     //bez přístupu do modulu ÚKOLY
            if (rec == null) rec = _mother.p56TaskBL.Load(pid);
            if (rec == null) return false;

            if (_mother.p56TaskBL.InhaleRecDisposition(pid, rec).OwnerAccess) return true;

            var lisX69 = _mother.x67EntityRoleBL.GetList_X69_OneTask(rec, true);
            return lisX69 != null && lisX69.Any();
        }

        private string GetSQL1(string strAppend)
        {
            return $"SELECT a.*,{_db.GetSQL1_Ocas("p65")},LTRIM(RTRIM(ISNULL(j02.j02FirstName,'')+' '+ISNULL(j02.j02LastName,''))) as DoneBy FROM p65Checklist a LEFT OUTER JOIN j02User j02 ON a.j02ID_Done=j02.j02ID {strAppend}";
        }

        public IEnumerable<BO.p65Checklist> GetList(int pid, string prefix = "p56")
        {
            string strField = OwnerField(prefix);
            if (pid <= 0 || strField == null || !IsAvailable()) return new List<BO.p65Checklist>();
            var lis = _db.GetList<BO.p65Checklist>(GetSQL1($"WHERE a.{strField}=@pid ORDER BY a.p65Ordinary,a.p65ID"), new { pid = pid });
            return lis ?? new List<BO.p65Checklist>();
        }

        public BO.p65Checklist Load(int p65id)
        {
            if (p65id <= 0 || !IsAvailable()) return null;
            return _db.Load<BO.p65Checklist>(GetSQL1("WHERE a.p65ID=@pid"), new { pid = p65id });
        }

        private static string CleanName(string s)
        {
            if (s == null) return null;
            s = s.Replace("\r", " ").Replace("\n", " ").Replace("\t", " ").Trim();
            if (s.Length > MaxNameLength) s = s.Substring(0, MaxNameLength);
            return s == "" ? null : s;
        }

        /// <summary>
        /// Rozdělí vložený text na položky: co řádek, to položka. Z řádků odstraní odrážky a číslování (- * • 1. 2) [ ] [x]).
        /// </summary>
        public List<string> ParseLines(string text)
        {
            var ret = new List<string>();
            if (string.IsNullOrWhiteSpace(text)) return ret;
            foreach (string line in text.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n'))
            {
                string s = System.Text.RegularExpressions.Regex.Replace(line, @"^\s*(?:[-*•–]\s+|\d{1,3}[.)]\s+)?(?:\[[ xX]?\]\s*)?", "");
                s = CleanName(s);
                if (s != null) ret.Add(s);
            }
            return ret;
        }

        /// <summary>Přidá položky na konec check-listu. Vrací počet přidaných, 0 = nic se nepřidalo.</summary>
        public int Add(int pid, List<string> names, string prefix = "p56")
        {
            string strField = OwnerField(prefix);
            if (names == null || strField == null || !CanEdit(pid, null, prefix)) return 0;
            names = names.Select(p => CleanName(p)).Where(p => p != null).ToList();
            if (names.Count == 0) return 0;

            int intCount = GetInt($"SELECT COUNT(*) as Value FROM p65Checklist WHERE {strField}=@pid", new { pid = pid });
            if (intCount + names.Count > MaxItems)
            {
                _mother.CurrentUser.AddMessage(string.Format(_mother.tra("Check-list může mít nejvýše {0} položek."), MaxItems));
                return 0;
            }
            int intOrdinary = GetInt($"SELECT ISNULL(MAX(p65Ordinary),0) as Value FROM p65Checklist WHERE {strField}=@pid", new { pid = pid });

            int x = 0;
            foreach (string strName in names)
            {
                intOrdinary += 1;
                var p = new DL.Params4Dapper();
                p.AddInt("pid", 0);
                p.AddInt(strField, pid, true);
                p.AddString("p65Name", strName);
                p.AddInt("p65Ordinary", intOrdinary);
                if (_db.SaveRecord("p65Checklist", p, new BO.p65Checklist()) > 0) x += 1;    //ValidUntil = 1.1.3000 → otevřená položka
            }
            return x;
        }

        public bool SetDone(int p65id, bool done)
        {
            var rec = Load(p65id);
            if (rec == null || rec.OwnerPrefix != "p56") return false;      //položky vzoru v opakovaném úkolu se neodškrtávají
            if (!CanEdit(rec.p56ID)) return false;
            if (rec.IsDone == done) return true;     //mezitím už to udělal někdo jiný

            //Do p65ValidUntil se zapisuje jen "teď" (hotovo) nebo 1.1.3000 (otevřeno). Jiné budoucí datum by položku časem odškrtlo samo.
            if (done)
            {
                return _db.RunSql("UPDATE p65Checklist SET p65ValidUntil=GETDATE(),j02ID_Done=@me,p65DateUpdate=GETDATE(),p65UserUpdate=@login WHERE p65ID=@pid", new { pid = p65id, me = Me, login = _mother.CurrentUser.j02Login });
            }
            return _db.RunSql("UPDATE p65Checklist SET p65ValidUntil=convert(datetime,'01.01.3000',104),j02ID_Done=NULL,p65DateUpdate=GETDATE(),p65UserUpdate=@login WHERE p65ID=@pid", new { pid = p65id, login = _mother.CurrentUser.j02Login });
        }

        public bool Rename(int p65id, string name)
        {
            name = CleanName(name);
            if (name == null) return false;
            var rec = Load(p65id);
            if (rec == null || !CanEdit(rec.OwnerPid, null, rec.OwnerPrefix)) return false;
            if (rec.p65Name == name) return true;
            return _db.RunSql("UPDATE p65Checklist SET p65Name=@name,p65DateUpdate=GETDATE(),p65UserUpdate=@login WHERE p65ID=@pid", new { pid = p65id, name = name, login = _mother.CurrentUser.j02Login });
        }

        public bool Delete(int p65id)
        {
            var rec = Load(p65id);
            if (rec == null) return true;       //už je smazaná
            if (!CanEdit(rec.OwnerPid, null, rec.OwnerPrefix)) return false;
            return _db.RunSql("DELETE FROM p65Checklist WHERE p65ID=@pid", new { pid = p65id });
        }

        /// <summary>Uloží nové pořadí položek. V p65ids jsou všechny položky záznamu v pořadí shora dolů; cizí a neznámá ID se přeskočí.</summary>
        public bool Reorder(int pid, List<int> p65ids, string prefix = "p56")
        {
            string strField = OwnerField(prefix);
            if (p65ids == null || p65ids.Count == 0 || strField == null || !CanEdit(pid, null, prefix)) return false;
            var known = GetList(pid, prefix).Select(p => p.pid).ToList();
            int x = 0;
            foreach (int p65id in p65ids.Distinct().Where(p => known.Contains(p)))
            {
                x += 1;
                _db.RunSql($"UPDATE p65Checklist SET p65Ordinary=@ord WHERE p65ID=@p65id AND {strField}=@pid AND p65Ordinary<>@ord", new { p65id = p65id, pid = pid, ord = x });
            }
            return true;
        }

        /// <summary>
        /// Zkopíruje vzor check-listu z opakovaného úkolu do právě vygenerovaného úkolu (položky otevřené).
        /// Volá generátor opakovaných úkolů, který běží i bez přihlášeného uživatele s rolí v úkolu - oprávnění se zde proto netestuje.
        /// Vrací počet zkopírovaných položek.
        /// </summary>
        public int CopyFromRecurrence(int p58id, int p56id)
        {
            if (p58id <= 0 || p56id <= 0 || !IsAvailable()) return 0;
            int intCount = GetInt("SELECT COUNT(*) as Value FROM p65Checklist WHERE p58ID=@p58id", new { p58id = p58id });
            if (intCount == 0) return 0;

            string strLogin = _mother.CurrentUser.j02Login;
            _db.RunSql("INSERT INTO p65Checklist(p56ID,p65Name,p65Ordinary,p65ValidFrom,p65ValidUntil,p65DateInsert,p65UserInsert,p65DateUpdate,p65UserUpdate)"
                + " SELECT @p56id,p65Name,p65Ordinary,GETDATE(),convert(datetime,'01.01.3000',104),GETDATE(),@login,GETDATE(),@login FROM p65Checklist WHERE p58ID=@p58id ORDER BY p65Ordinary,p65ID"
                , new { p56id = p56id, p58id = p58id, login = strLogin });
            return intCount;
        }
    }
}
