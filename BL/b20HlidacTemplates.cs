namespace BL
{
    /// <summary>
    /// Hotová pravidla hlídače za celou databázi (b20Entity = "x01", SQL vrací seznam záznamů).
    /// Na kartě nového hlídače se šablona jedním kliknutím předvyplní; administrátor doplní tým příjemců a uloží.
    /// Limity (počty hodin a dní) jsou přímo v SQL, kde je lze upravit. @pid = ID licence (x01ID).
    /// </summary>
    public class b20HlidacTemplate
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string ListEntity { get; set; }
        public string TestSql { get; set; }
        public string NotifyMessage { get; set; }
    }

    public static class b20HlidacTemplates
    {
        /// <summary>Jak často se šablony testují: jednou denně, ráno.</summary>
        public const int NextRunAfterMinutes = 1440;
        public const string TestTimeFrom = "06:00";
        public const string TestTimeUntil = "09:00";

        private static readonly List<b20HlidacTemplate> _lis = new List<b20HlidacTemplate>
        {
            new b20HlidacTemplate
            {
                Key = "wip-hours", Icon = "hourglass_top", ListEntity = "p41",
                Name = "Projekty s rozpracovaností nad 100 hodin",
                Description = "Otevřené projekty, na kterých je víc než 100 nevyúčtovaných fakturovatelných hodin. Limit změníte v SQL (číslo 100 na konci).",
                NotifyMessage = "Na těchto projektech přesáhly nevyúčtované fakturovatelné hodiny hlídaný limit:",
                TestSql =
@"SELECT a.p41ID, convert(varchar(20),convert(decimal(18,1),sum(w.p31Hours_Orig)))+' h'
FROM p41Project a
INNER JOIN p07ProjectLevel p07x ON a.p07ID=p07x.p07ID
INNER JOIN p31Worksheet w ON a.p41ID=w.p41ID
INNER JOIN p32Activity p32x ON w.p32ID=p32x.p32ID
WHERE p07x.x01ID=@pid AND a.p41ValidUntil>GETDATE() AND a.p41BillingFlag<99
AND w.p91ID IS NULL AND w.p31ExcludeBillingFlag IS NULL AND p32x.p32IsBillable=1
GROUP BY a.p41ID
HAVING sum(w.p31Hours_Orig)>=100"
            },
            new b20HlidacTemplate
            {
                Key = "wip-old", Icon = "history", ListEntity = "p41",
                Name = "Projekty s nevyúčtovanou prací starší než 60 dní",
                Description = "Otevřené projekty, kde nejstarší nevyúčtovaný fakturovatelný úkon čeká déle než 60 dní. Počet dní změníte v SQL (číslo 60 na konci).",
                NotifyMessage = "Na těchto projektech čeká na vyúčtování práce starší, než je hlídaný počet dní:",
                TestSql =
@"SELECT a.p41ID, 'od '+convert(varchar(10),min(w.p31Date),104)
FROM p41Project a
INNER JOIN p07ProjectLevel p07x ON a.p07ID=p07x.p07ID
INNER JOIN p31Worksheet w ON a.p41ID=w.p41ID
INNER JOIN p32Activity p32x ON w.p32ID=p32x.p32ID
WHERE p07x.x01ID=@pid AND a.p41ValidUntil>GETDATE() AND a.p41BillingFlag<99
AND w.p91ID IS NULL AND w.p31ExcludeBillingFlag IS NULL AND p32x.p32IsBillable=1
GROUP BY a.p41ID
HAVING min(w.p31Date)<DATEADD(day,-60,GETDATE())"
            },
            new b20HlidacTemplate
            {
                Key = "invoice-overdue", Icon = "request_quote", ListEntity = "p91",
                Name = "Vyúčtování více než 14 dní po splatnosti",
                Description = "Vystavená vyúčtování s neuhrazeným dluhem, která jsou po splatnosti déle než 14 dní. Počet dní změníte v SQL (číslo 14).",
                NotifyMessage = "Tato vyúčtování jsou po splatnosti a nejsou uhrazená:",
                TestSql =
@"SELECT a.p91ID, 'splatnost '+convert(varchar(10),a.p91DateMaturity,104)
FROM p91Invoice a
INNER JOIN p92InvoiceType p92x ON a.p92ID=p92x.p92ID
WHERE p92x.x01ID=@pid AND a.p91ValidUntil>GETDATE() AND a.p91IsDraft=0
AND a.p91Amount_Debt>0.1 AND a.p91DateMaturity<DATEADD(day,-14,GETDATE())"
            },
            new b20HlidacTemplate
            {
                Key = "invoice-draft", Icon = "edit_note", ListEntity = "p91",
                Name = "Koncepty vyúčtování starší než 14 dní",
                Description = "Vyúčtování, která zůstala ve stavu konceptu déle než 14 dní od založení. Počet dní změníte v SQL (číslo 14).",
                NotifyMessage = "Tato vyúčtování jsou stále jen koncept:",
                TestSql =
@"SELECT a.p91ID, 'založeno '+convert(varchar(10),a.p91DateInsert,104)
FROM p91Invoice a
INNER JOIN p92InvoiceType p92x ON a.p92ID=p92x.p92ID
WHERE p92x.x01ID=@pid AND a.p91ValidUntil>GETDATE() AND a.p91IsDraft=1
AND a.p91DateInsert<DATEADD(day,-14,GETDATE())"
            },
            new b20HlidacTemplate
            {
                Key = "task-overdue", Icon = "event_busy", ListEntity = "p56",
                Name = "Úkoly po termínu",
                Description = "Otevřené úkoly, kterým uplynul termín dokončení.",
                NotifyMessage = "Těmto úkolům uplynul termín a nejsou uzavřené:",
                TestSql =
@"SELECT a.p56ID, 'termín '+convert(varchar(10),a.p56PlanUntil,104)
FROM p56Task a
INNER JOIN p57TaskType p57x ON a.p57ID=p57x.p57ID
WHERE p57x.x01ID=@pid AND a.p56ValidUntil>GETDATE() AND a.p56PlanUntil<GETDATE()"
            },
            new b20HlidacTemplate
            {
                Key = "no-timesheet", Icon = "person_off", ListEntity = "j02",
                Name = "Uživatelé, kteří 5 dní nic nevykázali",
                Description = "Aktivní uživatelé, kteří v posledních 90 dnech vykazovali, ale za posledních 5 dní nemají žádný úkon (dovolená zapsaná jako úkon se počítá). Počet dní změníte v SQL (číslo 5).",
                NotifyMessage = "Tito uživatelé v posledních dnech nevykázali žádný úkon:",
                TestSql =
@"SELECT a.j02ID, 'naposledy '+convert(varchar(10),max(w.p31Date),104)
FROM j02User a
INNER JOIN j04UserRole j04x ON a.j04ID=j04x.j04ID
INNER JOIN x67EntityRole x67x ON j04x.x67ID=x67x.x67ID
INNER JOIN p31Worksheet w ON a.j02ID=w.j02ID
WHERE x67x.x01ID=@pid AND a.j02ValidUntil>GETDATE()
AND w.p31Date>=DATEADD(day,-90,GETDATE()) AND w.p31Date<=GETDATE()
GROUP BY a.j02ID
HAVING max(w.p31Date)<DATEADD(day,-5,convert(date,GETDATE()))"
            },
            new b20HlidacTemplate
            {
                Key = "cap-limit-80", Icon = "online_prediction", ListEntity = "p41",
                Name = "Kapacitní plán: strop hodin vyčerpaný z 80 %",
                Description = "Otevřené projekty, na kterých některá osoba vykázala aspoň 80 % svého stropu hodin z kapacitního plánu. Hodnota je počet takových osob na projektu. Podíl změníte v SQL (číslo 0.8).",
                NotifyMessage = "Na těchto projektech se blíží vyčerpání stropu hodin z kapacitního plánu:",
                TestSql =
@"SELECT q.p41ID, convert(varchar(10),count(*))+' os.'
FROM (
 SELECT r.p41ID, r.j02ID, r.r11HoursTotal, isnull(sum(w.p31Hours_Orig),0) as Vykazano
 FROM r11CapacityLimit r
 INNER JOIN p41Project a ON r.p41ID=a.p41ID
 INNER JOIN p07ProjectLevel p07x ON a.p07ID=p07x.p07ID
 LEFT OUTER JOIN (p31Worksheet w INNER JOIN p32Activity p32x ON w.p32ID=p32x.p32ID INNER JOIN p34ActivityGroup p34x ON p32x.p34ID=p34x.p34ID AND p34x.p33ID=1) ON r.p41ID=w.p41ID AND r.j02ID=w.j02ID
 WHERE p07x.x01ID=@pid AND a.p41ValidUntil>GETDATE() AND r.r11HoursTotal>0
 GROUP BY r.p41ID, r.j02ID, r.r11HoursTotal
) q
WHERE q.Vykazano>=q.r11HoursTotal*0.8
GROUP BY q.p41ID"
            },
            new b20HlidacTemplate
            {
                Key = "cap-over", Icon = "error_outline", ListEntity = "p41",
                Name = "Kapacitní plán: vykázáno nad strop hodin",
                Description = "Otevřené projekty, na kterých některá osoba vykázala víc hodin, než je její strop v kapacitním plánu. Hodnota je největší překročení na projektu.",
                NotifyMessage = "Na těchto projektech bylo vykázáno víc hodin, než povoluje strop z kapacitního plánu:",
                TestSql =
@"SELECT q.p41ID, '+'+convert(varchar(20),convert(decimal(18,1),max(q.Vykazano-q.r11HoursTotal)))+' h'
FROM (
 SELECT r.p41ID, r.j02ID, r.r11HoursTotal, isnull(sum(w.p31Hours_Orig),0) as Vykazano
 FROM r11CapacityLimit r
 INNER JOIN p41Project a ON r.p41ID=a.p41ID
 INNER JOIN p07ProjectLevel p07x ON a.p07ID=p07x.p07ID
 LEFT OUTER JOIN (p31Worksheet w INNER JOIN p32Activity p32x ON w.p32ID=p32x.p32ID INNER JOIN p34ActivityGroup p34x ON p32x.p34ID=p34x.p34ID AND p34x.p33ID=1) ON r.p41ID=w.p41ID AND r.j02ID=w.j02ID
 WHERE p07x.x01ID=@pid AND a.p41ValidUntil>GETDATE() AND r.r11HoursTotal>0
 GROUP BY r.p41ID, r.j02ID, r.r11HoursTotal
) q
WHERE q.Vykazano>q.r11HoursTotal+0.05
GROUP BY q.p41ID"
            },
            new b20HlidacTemplate
            {
                Key = "cap-overload", Icon = "groups", ListEntity = "j02",
                Name = "Kapacitní plán: přetížení lidé v příštích 14 dnech",
                Description = "Uživatelé, kteří mají v příštích 14 dnech v blocích kapacitního plánu víc hodin, než je jejich pracovní fond. Hodnota je plán / fond. Počítají se jen bloky (ne stropy rozložené do období projektu) a absence se neodečítají; přesný stav je na stránce Kapacitní plány. Počet dní změníte v SQL (číslo 13 = dnešek + 13 dní).",
                NotifyMessage = "Tito lidé mají v příštích dnech naplánováno víc hodin, než je jejich pracovní fond:",
                TestSql =
@"SELECT q.j02ID, convert(varchar(20),convert(decimal(18,0),q.PlanHodin))+' / '+convert(varchar(20),convert(decimal(18,0),q.Fond))+' h'
FROM (
 SELECT u.j02ID
 ,(SELECT isnull(sum(c.c22Hours_Work),0) FROM c22FondCalendar_Date c
   WHERE c.c21ID=u.c21ID AND isnull(c.c22CountryCode,'CZ')=isnull(u.j02CountryCode,'CZ')
   AND c.c22Date BETWEEN convert(date,GETDATE()) AND DATEADD(day,13,convert(date,GETDATE()))) as Fond
 ,(SELECT isnull(sum(b.r12Hours*x.FondOkno/x.FondBlok),0)
   FROM r12CapacityPlan b INNER JOIN r11CapacityLimit r ON b.r11ID=r.r11ID
   CROSS APPLY (SELECT
     (SELECT isnull(sum(c.c22Hours_Work),0) FROM c22FondCalendar_Date c
      WHERE c.c21ID=u.c21ID AND isnull(c.c22CountryCode,'CZ')=isnull(u.j02CountryCode,'CZ')
      AND c.c22Date BETWEEN b.r12Start AND b.r12End) as FondBlok
    ,(SELECT isnull(sum(c.c22Hours_Work),0) FROM c22FondCalendar_Date c
      WHERE c.c21ID=u.c21ID AND isnull(c.c22CountryCode,'CZ')=isnull(u.j02CountryCode,'CZ')
      AND c.c22Date BETWEEN b.r12Start AND b.r12End
      AND c.c22Date BETWEEN convert(date,GETDATE()) AND DATEADD(day,13,convert(date,GETDATE()))) as FondOkno
   ) x
   WHERE r.j02ID=u.j02ID AND x.FondBlok>0
   AND b.r12End>=convert(date,GETDATE()) AND b.r12Start<=DATEADD(day,13,convert(date,GETDATE()))) as PlanHodin
 FROM j02User u
 WHERE u.j02ValidUntil>GETDATE() AND u.c21ID IS NOT NULL
 AND u.j02ID IN (SELECT r.j02ID FROM r11CapacityLimit r INNER JOIN p41Project a ON r.p41ID=a.p41ID INNER JOIN p07ProjectLevel p07x ON a.p07ID=p07x.p07ID WHERE p07x.x01ID=@pid)
) q
WHERE q.PlanHodin>q.Fond+0.05"
            }
        };

        public static IEnumerable<b20HlidacTemplate> GetList()
        {
            return _lis;
        }

        public static b20HlidacTemplate Find(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return null;
            return _lis.FirstOrDefault(p => p.Key == key.Trim());
        }
    }
}
