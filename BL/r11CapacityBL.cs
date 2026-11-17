using System.Collections.Concurrent;

namespace BL
{
    /// <summary>
    /// Kapacitní plánování:
    /// - r11CapacityLimit = strop hodin osoby na projektu (Fa + Nefa). Hlídá se jen součet, rozdělení Fa/Nefa je orientační.
    /// - r12CapacityPlan  = nepovinné rozplánování stropu do časových bloků. Součet bloků nesmí překročit strop.
    /// Strop bez bloků se bere jako rozložený do plánovaného období projektu (p41PlanFrom - p41PlanUntil).
    /// Rozpad na dny se neukládá, počítá se z pracovního fondu osoby (c21FondCalendar / c22FondCalendar_Date).
    /// Zapisovat smí ten, kdo má k projektu vlastnické oprávnění.
    /// </summary>
    public interface Ir11CapacityBL
    {
        public bool IsAvailable();
        public bool CanEdit(int p41id);
        public BO.r11CapacityLimit Load(int r11id);
        public IEnumerable<BO.r11CapacityLimit> GetList(int p41id, int j02id);
        public IEnumerable<BO.r12CapacityPlan> GetList_r12(int p41id, int j02id);
        public BO.r12CapacityPlan Load_r12(int r12id);
        public IEnumerable<BO.r11Actual> GetList_Actuals(int p41id, int j02id);
        public IEnumerable<BO.r11Absence> GetList_Absences(DateTime d1, DateTime d2);
        public IEnumerable<BO.r11ActualDay> GetList_ActualsPerDay(DateTime d1, DateTime d2);
        public List<int> GetTeamPersonIds(int j11id);
        public int SaveLimit(BO.r11CapacityLimit rec);
        public int SaveWithBlocks(BO.r11CapacityLimit rec, List<BO.r12CapacityPlan> lisBlocks);
        public bool DeleteLimit(int r11id);
        public int SaveBlock(BO.r12CapacityPlan rec);
        public bool DeleteBlock(int r12id);
        public Dictionary<DateTime, double> GetFondPerDay(int c21id, string countrycode, DateTime d1, DateTime d2);
    }

    class r11CapacityBL : BaseBL, Ir11CapacityBL
    {
        //existence tabulek se drží v paměti, aby se netestovala při každém požadavku
        private static readonly ConcurrentDictionary<string, DateTime> _available = new ConcurrentDictionary<string, DateTime>();
        private static readonly ConcurrentDictionary<string, DateTime> _unavailable = new ConcurrentDictionary<string, DateTime>();

        public r11CapacityBL(BL.Factory mother) : base(mother)
        {

        }

        private double GetDouble(string strSQL, object param)
        {
            var c = _db.Load<BO.GetDouble>(strSQL, param);
            return c == null ? 0 : c.Value;
        }

        /// <summary>Jsou v databázi licence tabulky r11CapacityLimit a r12CapacityPlan? Bez nich se kapacitní plánování nenabízí.</summary>
        public bool IsAvailable()
        {
            if (_mother.CurrentUser == null || _db == null) return false;
            string strKey = $"{_mother.App.HostingMode}|{_mother.CurrentUser.x01LoginDomain}";
            if (_available.ContainsKey(strKey)) return true;
            if (_unavailable.TryGetValue(strKey, out DateTime d) && d > DateTime.Now) return false;

            var c = _db.Load<BO.GetInteger>("SELECT CASE WHEN OBJECT_ID('dbo.r11CapacityLimit') IS NULL OR OBJECT_ID('dbo.r12CapacityPlan') IS NULL THEN 0 ELSE 1 END as Value", null);
            if (c != null && c.Value == 1)
            {
                _available[strKey] = DateTime.Now;
                return true;
            }
            _unavailable[strKey] = DateTime.Now.AddMinutes(5);
            return false;
        }

        public bool CanEdit(int p41id)
        {
            if (p41id <= 0 || !_mother.CurrentUser.j04IsModule_r01) return false;
            var disp = _mother.p41ProjectBL.InhaleRecDisposition(p41id);
            return disp != null && disp.OwnerAccess;
        }

        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*," + _db.GetSQL1_Ocas("r11"));
            sb(",j02.j02LastName+' '+j02.j02FirstName as Person,j02.c21ID,j02.j18ID,j02.j02CountryCode");
            sb(",ISNULL(p41.p41NameShort,p41.p41Name) as Project,p41.p41Code,p41.p41PlanFrom,p41.p41PlanUntil,p41.j02ID_Owner as j02ID_ProjectOwner,p28.p28Name as Client");
            sb(" FROM r11CapacityLimit a INNER JOIN p41Project p41 ON a.p41ID=p41.p41ID INNER JOIN p07ProjectLevel p07 ON p41.p07ID=p07.p07ID");
            sb(" INNER JOIN j02User j02 ON a.j02ID=j02.j02ID LEFT OUTER JOIN p28Contact p28 ON p41.p28ID_Client=p28.p28ID");
            sb(" WHERE p07.x01ID=@x01id");
            sb(strAppend);
            return sbret();
        }

        private string GetSQL1_r12(string strAppend = null)
        {
            sb("SELECT a.*," + _db.GetSQL1_Ocas("r12") + ",r11.p41ID,r11.j02ID");
            sb(" FROM r12CapacityPlan a INNER JOIN r11CapacityLimit r11 ON a.r11ID=r11.r11ID INNER JOIN p41Project p41 ON r11.p41ID=p41.p41ID INNER JOIN p07ProjectLevel p07 ON p41.p07ID=p07.p07ID");
            sb(" WHERE p07.x01ID=@x01id");
            sb(strAppend);
            return sbret();
        }

        public BO.r11CapacityLimit Load(int r11id)
        {
            if (r11id <= 0 || !IsAvailable()) return null;
            return _db.Load<BO.r11CapacityLimit>(GetSQL1(" AND a.r11ID=@pid"), new { pid = r11id, x01id = _mother.CurrentUser.x01ID });
        }

        /// <summary>Stropy v licenci; p41id / j02id = 0 znamená bez omezení.</summary>
        public IEnumerable<BO.r11CapacityLimit> GetList(int p41id, int j02id)
        {
            if (!IsAvailable()) return new List<BO.r11CapacityLimit>();
            string s = "";
            if (p41id > 0) s += " AND a.p41ID=@p41id";
            if (j02id > 0) s += " AND a.j02ID=@j02id";
            return _db.GetList<BO.r11CapacityLimit>(GetSQL1(s + " ORDER BY j02.j02LastName,j02.j02FirstName,p41.p41Name"), new { p41id = p41id, j02id = j02id, x01id = _mother.CurrentUser.x01ID });
        }

        public IEnumerable<BO.r12CapacityPlan> GetList_r12(int p41id, int j02id)
        {
            if (!IsAvailable()) return new List<BO.r12CapacityPlan>();
            string s = "";
            if (p41id > 0) s += " AND r11.p41ID=@p41id";
            if (j02id > 0) s += " AND r11.j02ID=@j02id";
            return _db.GetList<BO.r12CapacityPlan>(GetSQL1_r12(s + " ORDER BY a.r12Start"), new { p41id = p41id, j02id = j02id, x01id = _mother.CurrentUser.x01ID });
        }

        public BO.r12CapacityPlan Load_r12(int r12id)
        {
            if (r12id <= 0 || !IsAvailable()) return null;
            return _db.Load<BO.r12CapacityPlan>(GetSQL1_r12(" AND a.r12ID=@pid"), new { pid = r12id, x01id = _mother.CurrentUser.x01ID });
        }

        /// <summary>Skutečnost: vykázané hodiny (časové úkony) pro dvojice osoba + projekt, které mají strop.</summary>
        public IEnumerable<BO.r11Actual> GetList_Actuals(int p41id, int j02id)
        {
            if (!IsAvailable()) return new List<BO.r11Actual>();
            sb("SELECT a.p41ID,a.j02ID,ISNULL(SUM(CASE WHEN p32.p32IsBillable=1 THEN a.p31Hours_Orig END),0) as HoursFa,ISNULL(SUM(CASE WHEN p32.p32IsBillable=0 THEN a.p31Hours_Orig END),0) as HoursNeFa");
            sb(" FROM p31Worksheet a INNER JOIN p32Activity p32 ON a.p32ID=p32.p32ID INNER JOIN p34ActivityGroup p34 ON p32.p34ID=p34.p34ID");
            sb(" INNER JOIN r11CapacityLimit r11 ON a.p41ID=r11.p41ID AND a.j02ID=r11.j02ID");
            sb(" INNER JOIN p41Project p41 ON r11.p41ID=p41.p41ID INNER JOIN p07ProjectLevel p07 ON p41.p07ID=p07.p07ID");
            sb(" WHERE p34.p33ID=1 AND p07.x01ID=@x01id");
            if (p41id > 0) sb(" AND a.p41ID=@p41id");
            if (j02id > 0) sb(" AND a.j02ID=@j02id");
            sb(" GROUP BY a.p41ID,a.j02ID");
            return _db.GetList<BO.r11Actual>(sbret(), new { p41id = p41id, j02id = j02id, x01id = _mother.CurrentUser.x01ID });
        }

        /// <summary>
        /// Absence lidí, kteří mají nějaký strop: hodiny po dnech z časových úkonů na aktivitách s p32AbsenceFlag
        /// (dovolená, nemoc, OČR...). Oběd (6) se nepočítá, není to absence z pracovního fondu.
        /// </summary>
        public IEnumerable<BO.r11Absence> GetList_Absences(DateTime d1, DateTime d2)
        {
            if (!IsAvailable()) return new List<BO.r11Absence>();
            sb("SELECT a.j02ID,a.p31Date as Datum,ISNULL(SUM(a.p31Hours_Orig),0) as Hours");
            sb(" FROM p31Worksheet a INNER JOIN p32Activity p32 ON a.p32ID=p32.p32ID");
            sb(" WHERE p32.p32AbsenceFlag>0 AND p32.p32AbsenceFlag<>6 AND a.p31Date BETWEEN @d1 AND @d2");
            sb(" AND a.j02ID IN (SELECT r11.j02ID FROM r11CapacityLimit r11 INNER JOIN p41Project p41 ON r11.p41ID=p41.p41ID INNER JOIN p07ProjectLevel p07 ON p41.p07ID=p07.p07ID WHERE p07.x01ID=@x01id)");
            sb(" GROUP BY a.j02ID,a.p31Date");
            return _db.GetList<BO.r11Absence>(sbret(), new { d1 = d1.Date, d2 = d2.Date, x01id = _mother.CurrentUser.x01ID });
        }

        /// <summary>Skutečnost po dnech: vykázané hodiny (časové úkony mimo absence) pro dvojice osoba + projekt, které mají strop.</summary>
        public IEnumerable<BO.r11ActualDay> GetList_ActualsPerDay(DateTime d1, DateTime d2)
        {
            if (!IsAvailable()) return new List<BO.r11ActualDay>();
            sb("SELECT a.p41ID,a.j02ID,a.p31Date as Datum,ISNULL(SUM(a.p31Hours_Orig),0) as Hours");
            sb(" FROM p31Worksheet a INNER JOIN p32Activity p32 ON a.p32ID=p32.p32ID INNER JOIN p34ActivityGroup p34 ON p32.p34ID=p34.p34ID");
            sb(" INNER JOIN r11CapacityLimit r11 ON a.p41ID=r11.p41ID AND a.j02ID=r11.j02ID");
            sb(" INNER JOIN p41Project p41 ON r11.p41ID=p41.p41ID INNER JOIN p07ProjectLevel p07 ON p41.p07ID=p07.p07ID");
            sb(" WHERE p34.p33ID=1 AND p07.x01ID=@x01id AND ISNULL(p32.p32AbsenceFlag,0)=0 AND a.p31Date BETWEEN @d1 AND @d2");
            sb(" GROUP BY a.p41ID,a.j02ID,a.p31Date");
            return _db.GetList<BO.r11ActualDay>(sbret(), new { d1 = d1.Date, d2 = d2.Date, x01id = _mother.CurrentUser.x01ID });
        }

        public List<int> GetTeamPersonIds(int j11id)
        {
            return _db.GetList<BO.GetInteger>("SELECT j02ID as Value FROM j12Team_Person WHERE j11ID=@pid AND j02ID IS NOT NULL", new { pid = j11id }).Select(p => p.Value).ToList();
        }

        public int SaveLimit(BO.r11CapacityLimit rec)
        {
            if (!IsAvailable()) return this.ZeroMessage("V databázi chybí tabulky kapacitního plánování.");
            if (rec.p41ID <= 0) return this.ZeroMessage("Chybí vyplnit [Projekt].");
            if (rec.j02ID <= 0) return this.ZeroMessage("Chybí vyplnit [Personální zdroj (uživatel)].");
            if (!CanEdit(rec.p41ID)) return this.ZeroMessage("Kapacitní plán projektu může upravovat jen ten, kdo má k projektu vlastnické oprávnění.");
            if (rec.r11HoursFa < 0 || rec.r11HoursNeFa < 0) return this.ZeroMessage("Hodiny nesmí být záporné.");
            rec.r11HoursTotal = Math.Round(rec.r11HoursFa + rec.r11HoursNeFa, 2);
            if (rec.r11HoursTotal <= 0) return this.ZeroMessage("Chybí vyplnit strop hodin.");
            if (rec.r11HoursTotal > 100000) return this.ZeroMessage("Strop hodin je mimo povolený rozsah.");

            if (rec.pid > 0)
            {
                var recOrig = Load(rec.pid);
                if (recOrig == null) return this.ZeroMessage("Záznam nebyl nalezen.");
                rec.p41ID = recOrig.p41ID; rec.j02ID = recOrig.j02ID;   //projekt a osoba se u existujícího stropu nemění
                double dblPlanned = GetDouble("SELECT ISNULL(SUM(r12Hours),0) as Value FROM r12CapacityPlan WHERE r11ID=@pid", new { pid = rec.pid });
                if (dblPlanned > rec.r11HoursTotal + 0.001)
                {
                    this.AddMessageWithPars("Strop nelze snížit pod již rozplánované hodiny ({0} h).", BO.Code.Bas.Number2String(dblPlanned));
                    return 0;
                }
            }
            else
            {
                var c = _db.Load<BO.GetInteger>("SELECT COUNT(*) as Value FROM r11CapacityLimit WHERE p41ID=@p41id AND j02ID=@j02id", new { p41id = rec.p41ID, j02id = rec.j02ID });
                if (c != null && c.Value > 0) return this.ZeroMessage("Tato osoba už má na projektu strop hodin zadaný.");
            }

            return WriteLimit(rec);
        }

        private int WriteLimit(BO.r11CapacityLimit rec)
        {
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            if (rec.pid == 0)
            {
                p.AddInt("p41ID", rec.p41ID, true);
                p.AddInt("j02ID", rec.j02ID, true);
            }
            p.AddDouble("r11HoursFa", rec.r11HoursFa);
            p.AddDouble("r11HoursNeFa", rec.r11HoursNeFa);
            p.AddDouble("r11HoursTotal", rec.r11HoursTotal);
            p.AddString("r11Text", rec.r11Text);
            return _db.SaveRecord("r11CapacityLimit", p, rec);
        }

        private int WriteBlock(BO.r12CapacityPlan rec)
        {
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            if (rec.pid == 0) p.AddInt("r11ID", rec.r11ID, true);
            p.AddDateTime("r12Start", rec.r12Start);
            p.AddDateTime("r12End", rec.r12End);
            p.AddDouble("r12Hours", rec.r12Hours);
            p.AddString("r12Text", rec.r12Text);
            return _db.SaveRecord("r12CapacityPlan", p, rec);
        }

        /// <summary>
        /// Uloží strop i jeho bloky najednou (karta záznamu r11/Record). lisBlocks je cílový stav: blok s pid = 0 se založí,
        /// existující se přepíše, bloky stropu, které v seznamu nejsou, se smažou. Součet bloků nesmí překročit strop.
        /// </summary>
        public int SaveWithBlocks(BO.r11CapacityLimit rec, List<BO.r12CapacityPlan> lisBlocks)
        {
            if (!IsAvailable()) return this.ZeroMessage("V databázi chybí tabulky kapacitního plánování.");
            if (lisBlocks == null) lisBlocks = new List<BO.r12CapacityPlan>();
            if (rec.pid > 0)
            {
                var recOrig = Load(rec.pid);
                if (recOrig == null) return this.ZeroMessage("Záznam nebyl nalezen.");
                rec.p41ID = recOrig.p41ID; rec.j02ID = recOrig.j02ID;   //projekt a osoba se u existujícího stropu nemění
            }
            if (rec.p41ID <= 0) return this.ZeroMessage("Chybí vyplnit [Projekt].");
            if (rec.j02ID <= 0) return this.ZeroMessage("Chybí vyplnit [Personální zdroj (uživatel)].");
            if (!CanEdit(rec.p41ID)) return this.ZeroMessage("Kapacitní plán projektu může upravovat jen ten, kdo má k projektu vlastnické oprávnění.");
            if (rec.r11HoursFa < 0 || rec.r11HoursNeFa < 0) return this.ZeroMessage("Hodiny nesmí být záporné.");
            rec.r11HoursTotal = Math.Round(rec.r11HoursFa + rec.r11HoursNeFa, 2);
            if (rec.r11HoursTotal <= 0) return this.ZeroMessage("Chybí vyplnit strop hodin.");
            if (rec.r11HoursTotal > 100000) return this.ZeroMessage("Strop hodin je mimo povolený rozsah.");
            if (rec.pid == 0)
            {
                var c = _db.Load<BO.GetInteger>("SELECT COUNT(*) as Value FROM r11CapacityLimit WHERE p41ID=@p41id AND j02ID=@j02id", new { p41id = rec.p41ID, j02id = rec.j02ID });
                if (c != null && c.Value > 0) return this.ZeroMessage("Tato osoba už má na projektu strop hodin zadaný.");
            }

            foreach (var b in lisBlocks)
            {
                b.r12Start = b.r12Start.Date; b.r12End = b.r12End.Date;
                b.r12Hours = Math.Round(b.r12Hours, 2);
                if (b.r12Start.Year < 2000) return this.ZeroMessage("V bloku chybí vyplnit [Od].");
                if (b.r12End < b.r12Start) return this.ZeroMessage("Začátek bloku je větší než jeho konec.");
                if ((b.r12End - b.r12Start).TotalDays > 2000) return this.ZeroMessage("Blok může být dlouhý nejvýše 2000 dní.");
                if (b.r12Hours <= 0) return this.ZeroMessage("V bloku chybí vyplnit hodiny.");
            }
            double dblPlanned = Math.Round(lisBlocks.Sum(p => p.r12Hours), 2);
            if (dblPlanned > rec.r11HoursTotal + 0.001)
            {
                this.AddMessageWithPars("Rozplánované hodiny ({0} h) překračují strop osoby na projektu.", BO.Code.Bas.Number2String(dblPlanned));
                return 0;
            }
            if (rec.pid > 0)
            {
                //bloky s pid musí patřit tomuto stropu
                var ids = _db.GetList<BO.GetInteger>("SELECT r12ID as Value FROM r12CapacityPlan WHERE r11ID=@pid", new { pid = rec.pid }).Select(p => p.Value).ToHashSet();
                if (lisBlocks.Any(p => p.pid > 0 && !ids.Contains(p.pid))) return this.ZeroMessage("Blok nepatří k tomuto stropu.");
            }
            else if (lisBlocks.Any(p => p.pid > 0))
            {
                return this.ZeroMessage("Blok nepatří k tomuto stropu.");
            }

            using (var sc = new System.Transactions.TransactionScope())
            {
                int intPID = WriteLimit(rec);
                if (intPID == 0) return 0;

                var keep = lisBlocks.Where(p => p.pid > 0).Select(p => p.pid).ToList();
                _db.RunSql("DELETE FROM r12CapacityPlan WHERE r11ID=@pid" + (keep.Count > 0 ? " AND r12ID NOT IN (" + string.Join(",", keep) + ")" : ""), new { pid = intPID });
                foreach (var b in lisBlocks)
                {
                    b.r11ID = intPID;
                    if (WriteBlock(b) == 0) return 0;
                }

                sc.Complete();
                return intPID;
            }
        }

        public bool DeleteLimit(int r11id)
        {
            var rec = Load(r11id);
            if (rec == null) return true;   //už je smazaný
            if (!CanEdit(rec.p41ID)) return this.FalsehMessage("Kapacitní plán projektu může upravovat jen ten, kdo má k projektu vlastnické oprávnění.");
            _db.RunSql("DELETE FROM r12CapacityPlan WHERE r11ID=@pid", new { pid = r11id });
            return _db.RunSql("DELETE FROM r11CapacityLimit WHERE r11ID=@pid", new { pid = r11id });
        }

        public int SaveBlock(BO.r12CapacityPlan rec)
        {
            if (!IsAvailable()) return this.ZeroMessage("V databázi chybí tabulky kapacitního plánování.");
            if (rec.pid > 0)
            {
                var recOrig = Load_r12(rec.pid);
                if (recOrig == null) return this.ZeroMessage("Záznam nebyl nalezen.");
                rec.r11ID = recOrig.r11ID;
            }
            var recR11 = Load(rec.r11ID);
            if (recR11 == null) return this.ZeroMessage("Strop hodin nebyl nalezen.");
            if (!CanEdit(recR11.p41ID)) return this.ZeroMessage("Kapacitní plán projektu může upravovat jen ten, kdo má k projektu vlastnické oprávnění.");

            rec.r12Start = rec.r12Start.Date;
            rec.r12End = rec.r12End.Date;
            if (rec.r12End < rec.r12Start) return this.ZeroMessage("Začátek bloku je větší než jeho konec.");
            if ((rec.r12End - rec.r12Start).TotalDays > 2000) return this.ZeroMessage("Blok může být dlouhý nejvýše 2000 dní.");
            rec.r12Hours = Math.Round(rec.r12Hours, 2);
            if (rec.r12Hours <= 0) return this.ZeroMessage("Chybí vyplnit hodiny bloku.");

            //strop se hlídá jako součet: bloky dohromady nesmí překročit r11HoursTotal
            double dblOthers = GetDouble("SELECT ISNULL(SUM(r12Hours),0) as Value FROM r12CapacityPlan WHERE r11ID=@r11id AND r12ID<>@pid", new { r11id = rec.r11ID, pid = rec.pid });
            if (dblOthers + rec.r12Hours > recR11.r11HoursTotal + 0.001)
            {
                this.AddMessageWithPars("Rozplánované hodiny by překročily strop osoby na projektu. K rozplánování zbývá {0} h.", BO.Code.Bas.Number2String(Math.Max(0, recR11.r11HoursTotal - dblOthers)));
                return 0;
            }

            return WriteBlock(rec);
        }

        public bool DeleteBlock(int r12id)
        {
            var rec = Load_r12(r12id);
            if (rec == null) return true;   //už je smazaný
            if (!CanEdit(rec.p41ID)) return this.FalsehMessage("Kapacitní plán projektu může upravovat jen ten, kdo má k projektu vlastnické oprávnění.");
            return _db.RunSql("DELETE FROM r12CapacityPlan WHERE r12ID=@pid", new { pid = r12id });
        }

        /// <summary>
        /// Pracovní fond po dnech. Bere se z c22FondCalendar_Date; pro dny, které tam nejsou,
        /// z hodin dne v týdnu ve fondu c21 (c21Day1_Hours = pondělí). Osoba bez fondu: 8 hodin pondělí až pátek.
        /// Svátky (c26Holiday) státu osoby mají fond vždy nulový.
        /// </summary>
        public Dictionary<DateTime, double> GetFondPerDay(int c21id, string countrycode, DateTime d1, DateTime d2)
        {
            var ret = new Dictionary<DateTime, double>();
            double[] week = new double[] { 8, 8, 8, 8, 8, 0, 0 };  //po..ne
            if (c21id > 0)
            {
                var recC21 = _mother.c21FondCalendarBL.Load(c21id);
                if (recC21 != null)
                {
                    week = new double[] { recC21.c21Day1_Hours, recC21.c21Day2_Hours, recC21.c21Day3_Hours, recC21.c21Day4_Hours, recC21.c21Day5_Hours, recC21.c21Day6_Hours, recC21.c21Day7_Hours };
                    foreach (var c in _mother.c21FondCalendarBL.GetList_c22(c21id, countrycode, d1.Date, d2.Date))
                    {
                        ret[c.c22Date.Date] = c.c22Hours_Work;
                    }
                }
            }
            for (var d = d1.Date; d <= d2.Date; d = d.AddDays(1))
            {
                if (!ret.ContainsKey(d))
                {
                    ret[d] = week[((int)d.DayOfWeek + 6) % 7];
                }
            }
            //svátky (c26Holiday) státu osoby mají vždy nulový fond: c22 je nuluje jen pro dny, které obsahuje,
            //takže bez tohoto by osoba bez fondu nebo den mimo c22 dostal ve svátek běžné hodiny
            string strCountry = string.IsNullOrEmpty(countrycode) ? _mother.CurrentUser.x01CountryCode : countrycode;
            if (string.IsNullOrEmpty(strCountry)) strCountry = "CZ";
            foreach (var c in _mother.c26HolidayBL.GetList(new BO.myQueryC26() { global_d1 = d1.Date, global_d2 = d2.Date }))
            {
                if (string.IsNullOrEmpty(c.c26CountryCode) || string.Equals(c.c26CountryCode, strCountry, StringComparison.OrdinalIgnoreCase))
                {
                    if (ret.ContainsKey(c.c26Date.Date)) ret[c.c26Date.Date] = 0;
                }
            }
            return ret;
        }
    }
}
