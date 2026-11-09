namespace UI.Code
{
    /// <summary>
    /// „Onboarding (začínáme)“ – úvodní stránka /Home/Index: checklist prvních kroků, klávesové zkratky, videonávody.
    /// Kroky podle modulů a oprávnění role; splnění se pozná z dat (žádné ruční odškrtávání, kromě volby „Hotovo“ u kroků,
    /// které z dat poznat nejde). Jednou splněný krok zůstává splněný (uživatelský parametr <see cref="ParamDone"/>),
    /// takže se dotazuje jen na nesplněné kroky. Bez změn v DB.
    /// </summary>
    public static class OnboardingSupport
    {
        /// <summary>Kódy splněných kroků (čárkami). Zapisuje se při zjištění splnění z dat i při ručním „Hotovo“.</summary>
        public const string ParamDone = "onboarding-done";
        public const string MobileUrl = "https://mobile.marktime.net";

        private class StepDef
        {
            public string Code, Icon, Title, Text, ActionLabel, Href;
            public Func<bool> IsAvailable;
            /// <summary>SELECT … vracející 1, je-li krok splněný (parametr @j02id); null = kontrola v C# (<see cref="CheckCs"/>) nebo jen ručně.</summary>
            public string SqlExists;
            public Func<bool> CheckCs;
        }

        private static List<StepDef> Definitions(BL.Factory f, BL.AI.Core.ILlmClient llm)
        {
            var u = f.CurrentUser;
            var menu = new UI.Menu.TheMenuSupport(f);
            string gridPrefix = u.j04IsModule_p31 ? "p31" : (u.j04IsModule_p41 ? "p41" : (u.j04IsModule_p28 ? "p28" : (u.j04IsModule_p56 ? "p56" : null)));

            return new List<StepDef>
            {
                new StepDef
                {
                    Code = "profile", Icon = "person",
                    Title = "Nastavte si svůj profil",
                    Text = "Doplňte mobil a podpis do e-mailů, zvolte si barvy a velikost písma.",
                    ActionLabel = "Můj profil", Href = "/Home/MyProfile",
                    IsAvailable = () => u.TestPermission(BO.PermValEnum.GR_MyProfile),
                    SqlExists = "SELECT 1 FROM j02User WHERE j02ID=@j02id AND j02UserUpdate=j02Login"     //profil uložil sám uživatel
                },
                new StepDef
                {
                    Code = "p31", Icon = "more_time",
                    Title = "Vykažte první úkon",
                    Text = "Zapište, na čem jste pracovali – projekt, aktivitu, čas a popis.",
                    ActionLabel = "Nový úkon", Href = "javascript:_window_open('/p31/Record?pid=0',3)",
                    IsAvailable = () => u.j04IsModule_p31,
                    SqlExists = "SELECT 1 FROM p31Worksheet WHERE j02ID=@j02id"
                },
                new StepDef
                {
                    Code = "p68", Icon = "timer",
                    Title = "Vyzkoušejte stopky",
                    Text = "Spusťte stopky u rozdělané práce – z naměřeného času pak vytvoříte úkon.",
                    ActionLabel = "Stopky", Href = "javascript:_window_open('/p68/Index',3)",
                    IsAvailable = () => u.j04IsModule_p31,
                    SqlExists = "SELECT 1 FROM p68StopWatch WHERE j02ID=@j02id"
                },
                new StepDef
                {
                    Code = "mo", Icon = "smartphone",
                    Title = "Vykazujte i z telefonu",
                    Text = "Na telefonu otevřete mobile.marktime.net, přihlaste se a přidejte si aplikaci na plochu.",
                    ActionLabel = "mobile.marktime.net", Href = MobileUrl,
                    IsAvailable = () => u.j04IsModule_p31,
                    SqlExists = "SELECT 1 FROM j90LoginAccessLog WHERE j02ID=@j02id AND j90AppClient LIKE '%-MO'"     //přihlášení z MO (MO/LoginController)
                },
                new StepDef
                {
                    Code = "grid", Icon = "view_column",
                    Title = "Upravte si tabulku podle sebe",
                    Text = "V přehledu otevřete Návrhář sloupců, vyberte sloupce a filtr a uložte si je jako pojmenovanou tabulku.",
                    ActionLabel = "Otevřít přehled", Href = gridPrefix == null ? null : menu.GetMainmenuEntityUrl(gridPrefix),
                    IsAvailable = () => gridPrefix != null,
                    SqlExists = "SELECT 1 FROM j72TheGridTemplate WHERE j02ID=@j02id AND (j72SystemFlag IN (0,2) OR (j72SystemFlag=1 AND j72DateUpdate>DATEADD(SECOND,10,j72DateInsert)))"
                },
                new StepDef
                {
                    Code = "widgets", Icon = "dashboard",
                    Title = "Přizpůsobte si plochu",
                    Text = "Na plochu si přidejte přehledy, které chcete mít po ruce, a uspořádejte je.",
                    ActionLabel = "Plocha", Href = "/Widgets/Index",
                    IsAvailable = () => u.j04IsModule_Widgets,
                    SqlExists = "SELECT 1 FROM x56WidgetBinding WHERE j02ID=@j02id AND x56DockState IS NOT NULL UNION ALL SELECT 1 FROM x55Widget WHERE j02ID_Owner=@j02id"
                },
                new StepDef
                {
                    Code = "ical", Icon = "calendar_add_on",
                    Title = "Propojte MARKTIME s kalendářem",
                    Text = "Úkony, úkoly a termíny uvidíte v Google nebo Outlook kalendáři.",
                    ActionLabel = "Nastavit kalendář", Href = "javascript:_window_open('/icalgen/Index',2)",
                    IsAvailable = () => u.TestPermission(BO.PermValEnum.GR_MyProfile),
                    CheckCs = () => f.CBL.LoadUserParam("ical-p31") != null     //uloží icalgen/BuildUrl
                },
                new StepDef
                {
                    Code = "ai", Icon = "auto_awesome",
                    Title = "Zeptejte se MARKTIME",
                    Text = "Napište dotaz vlastními slovy – třeba „Kolik hodin jsem vykázal minulý měsíc?“",
                    ActionLabel = "Zeptat se", Href = "/AiAsk",
                    IsAvailable = () => llm != null && llm.Options.Enabled,     //licence + oprávnění GR_AI
                    SqlExists = "SELECT 1 FROM j98OpenAILog WHERE j02ID=@j02id AND ISNULL(j98Status,'')<>'E'"
                }
            };
        }

        /// <summary>Checklist pro /Home/Index (a jeho obnovení přes /Onboarding/Checklist).</summary>
        public static OnboardingViewModel Build(BL.Factory f, BL.AI.Core.ILlmClient llm)
        {
            var defs = Definitions(f, llm).Where(p => p.IsAvailable()).ToList();
            var done = LoadDone(f);
            var pending = defs.Where(p => !done.Contains(p.Code)).ToList();

            if (pending.Count > 0)
            {
                var newlyDone = Detect(f, pending);
                if (newlyDone.Count > 0)
                {
                    done.UnionWith(newlyDone);
                    SaveDone(f, done);
                }
            }

            var v = new OnboardingViewModel
            {
                Steps = defs.Select(p => new OnboardingStep
                {
                    Code = p.Code,
                    Icon = p.Icon,
                    Title = f.tra(p.Title),
                    Text = f.tra(p.Text),
                    ActionLabel = f.tra(p.ActionLabel),
                    Href = p.Href,
                    IsExternal = p.Href != null && p.Href.StartsWith("https://"),
                    IsDone = done.Contains(p.Code)
                }).ToList()
            };
            return v;
        }

        /// <summary>Ruční „Hotovo“ (jen kód dostupného kroku).</summary>
        public static bool MarkDone(BL.Factory f, BL.AI.Core.ILlmClient llm, string code)
        {
            if (!Definitions(f, llm).Any(p => p.Code == code && p.IsAvailable())) return false;
            var done = LoadDone(f);
            if (done.Add(code)) SaveDone(f, done);
            return true;
        }

        /// <summary>Jeden dotaz do DB pro všechny nesplněné kroky s SQL kontrolou; chyba dotazu (např. chybí tabulka) = nic nesplněno.</summary>
        private static HashSet<string> Detect(BL.Factory f, List<StepDef> pending)
        {
            var res = new HashSet<string>();
            foreach (var p in pending.Where(p => p.CheckCs != null))
            {
                if (p.CheckCs()) res.Add(p.Code);
            }

            var sqlSteps = pending.Where(p => p.SqlExists != null).ToList();
            if (sqlSteps.Count == 0) return res;

            string sql = "SELECT " + string.Join(",", sqlSteps.Select(p => $"CASE WHEN EXISTS({p.SqlExists}) THEN 1 ELSE 0 END AS [{p.Code}]"));
            object loaded = f.db.Load<dynamic>(sql, new { j02id = f.CurrentUser.pid });
            var row = loaded as IDictionary<string, object>;     //Dapper DapperRow
            if (row == null)
            {
                //např. starší DB bez j98OpenAILog – zkusit každý krok zvlášť, ať jeden chybějící nezablokuje ostatní
                if (sqlSteps.Count == 1) return res;
                foreach (var p in sqlSteps)
                {
                    foreach (var c in Detect(f, new List<StepDef> { p })) res.Add(c);
                }
                return res;
            }
            foreach (var p in sqlSteps)
            {
                if (row.TryGetValue(p.Code, out object val) && Convert.ToInt32(val) == 1) res.Add(p.Code);
            }
            return res;
        }

        private static HashSet<string> LoadDone(BL.Factory f)
        {
            string s = f.CBL.LoadUserParam(ParamDone);
            return string.IsNullOrEmpty(s) ? new HashSet<string>() : s.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToHashSet();
        }

        private static void SaveDone(BL.Factory f, HashSet<string> done) => f.CBL.SetUserParam(ParamDone, string.Join(",", done.OrderBy(p => p)));

        /// <summary>
        /// Záložka „Klávesové zkratky“. Musí odpovídat document.onkeydown v wwwroot/js/site.js (Alt+…, Shift+Alt+…)
        /// a hledání „/“ v _Layout – při změně zkratky upravit i tady. (Skupina, klávesy, popis.)
        /// </summary>
        public static readonly (string Group, string Keys, string Text)[] Shortcuts =
        {
            ("Kdekoli", "Alt+W", "Vykázat nový úkon – v tabulce úkonů, projektů, úkolů a kontaktů se předvyplní vybraný záznam"),
            ("Kdekoli", "Alt+Q", "Uložit změny v otevřeném okně"),
            ("Kdekoli", "Alt+1", "Hlavní menu"),
            ("Kdekoli", "Alt+2", "Nabídka Nový"),
            ("Kdekoli", "Alt+3", "Stopky"),
            ("Kdekoli", "/", "Hledat"),
            ("Kdekoli", "Esc", "Zavřít okno"),
            ("V tabulce", "Dvojklik", "Otevřít záznam"),
            ("V tabulce", "Alt+A", "Kontextové menu vybraného řádku"),
            ("V tabulce", "Alt+S nebo Alt+R", "Schvalování a vyúčtování (úkony, projekty, kontakty, úkoly, uživatelé)"),
            ("V tabulce", "Alt+T", "Tisková sestava vybraného záznamu"),
            ("V tabulce", "Shift+Alt+1 až 5", "Obarvit zaškrtnuté záznamy (úkony, projekty, kontakty, vyúčtování, dokumenty)"),
            ("V tabulce", "Shift+Alt+6", "Zrušit barvu zaškrtnutých záznamů")
        };

        /// <summary>
        /// Záložka „Videonávody“ (skupina, název, URL). Nahrazuje widget skupinu „MARKTIME Akademie“ (x54 academy),
        /// jejíž obsah je v DB každého klienta. Prázdné = záložka ukáže jen informaci, že se připravuje.
        /// </summary>
        public static readonly (string Group, string Title, string Url)[] Videos =
        {
        };
    }

    public class OnboardingStep
    {
        public string Code { get; set; }
        public string Icon { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public string ActionLabel { get; set; }
        /// <summary>URL, nebo javascript:_window_open(…); null = bez tlačítka.</summary>
        public string Href { get; set; }
        public bool IsExternal { get; set; }
        public bool IsDone { get; set; }
    }

    public class OnboardingViewModel : UI.Models.BaseViewModel
    {
        public List<OnboardingStep> Steps { get; set; } = new();
        public int DoneCount => Steps.Count(p => p.IsDone);
    }
}
