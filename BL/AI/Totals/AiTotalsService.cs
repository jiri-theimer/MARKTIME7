using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using BL.AI.Core;
using BO.AI;

namespace BL.AI.Totals
{
    /// <summary>
    /// AI agent pro nástroj SOUČTY: z dotazu uživatele navrhne šablonu j79 (sloupce, stav, formát, období, WebDataRocks).
    /// Návrh pouze vrací – uloží se až po potvrzení uživatelem (Save).
    /// Volání modelu, opravu a log řeší AiAgentBase. Instance na jeden request (Factory je scoped).
    /// </summary>
    public class AiTotalsService : AiAgentBase<AiTotalsAnswer, AiTotalsService.AskResult>
    {
        private readonly BL.TheColumnsProvider _cp;
        private readonly BL.Singleton.ThePeriodProvider _pp;
        private AiTotalsCatalog _catalog;

        public AiTotalsService(BL.Factory f, BL.TheColumnsProvider cp, BL.Singleton.ThePeriodProvider pp, ILlmClient llm) : base(f, llm)
        {
            _cp = cp; _pp = pp;
        }

        protected override string Feature => "ai-totals";

        public AiTotalsCatalog Catalog => _catalog ??= AiTotalsCatalog.Build(F, _cp);
        public List<BO.ThePeriod> Periods => _pp.getPallete();

        /// <summary>Stavy úkonů (j79StateQuery) – myQueryP31.Handle_p31StateQuery_p31.</summary>
        public static readonly Dictionary<int, string> States = new()
        {
            [0] = "Všechny stavy", [1] = "Rozpracované", [2] = "Rozpracované s korekcí", [3] = "Nevyúčtované",
            [4] = "Schválené", [5] = "Schválené jako fakturovat", [6] = "Schválené jako paušál", [7] = "Schválené jako odpis",
            [8] = "Schválené jako fakturovat později", [9] = "Neschválené", [10] = "Vyúčtované", [11] = "DRAFT vyúčtování",
            [12] = "Vyúčtované jako fakturovat", [13] = "Vyúčtované jako paušál", [14] = "Vyúčtované jako odpis",
            [15] = "Vyloučené z vyúčtování", [16] = "Rozpracované Fa aktivita", [17] = "Rozpracované NeFa aktivita",
            [18] = "Vyúčtované s nulovou cenou", [19] = "Bez vyloučených z vyúčtování", [20] = "Nevyúčtované a neschválené",
            [21] = "Uhrazené vyúčtování"
        };
        public static readonly Dictionary<string, string> TabNames = new()
        {
            [""] = "Všechny formáty", ["time"] = "Časové úkony (hodiny)", ["kusovnik"] = "Kusovník",
            ["expense"] = "Peněžní výdaje", ["fee"] = "Pevné odměny"
        };

        public class AskResult : AiAskResult
        {
            public AiTotalsProposal Proposal { get; set; }      // s klíči sloupců
            public List<string> Confirm { get; set; } = new();
        }

        // ------------------------------------------------------------------ Ask

        protected override List<LlmSystemBlock> BuildSystem() => new()
        {
            new LlmSystemBlock { Text = AiPromptStore.Get("totals.system.md") },
            new LlmSystemBlock { Text = Catalog.ToPromptText(), Cache = true },   // cache breakpoint: instrukce + katalog
            new LlmSystemBlock { Text = BuildDynamicContext() }
        };

        protected override string DecorateLastUserMessage(string content, BO.AI.AiAskInput input)
        {
            var current = (input as AiTotalsAskInput)?.Current;
            if (current == null) return content;
            return content + "\n\nAktuální návrh ve formuláři (uživatel jej mohl upravit):\n" + JsonSerializer.Serialize(ToIds(current));
        }

        protected override List<string> ProcessAnswer(AiTotalsAnswer ans, AskResult res)
        {
            res.Reply = ans.Reply;
            res.Confirm = ans.Confirm ?? new();
            res.Proposal = null;
            if (ans.Proposal == null) return new();

            res.Proposal = ToKeys(ans.Proposal, out _);     // neznámá ID zachytí validátor
            var v = AiTotalsValidator.Validate(res.Proposal, Catalog, Periods);
            res.Warnings = v.Warnings;
            return v.Errors;
        }

        private string BuildDynamicContext()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Dnešní datum: {DateTime.Today:yyyy-MM-dd} ({DateTime.Today:dddd}).");
            sb.AppendLine();
            sb.AppendLine("# STAVY ÚKONŮ (state)");
            foreach (var s in States) sb.AppendLine($"{s.Key} | {s.Value}");
            sb.AppendLine();
            sb.AppendLine("# FORMÁTY ÚKONŮ (tab)");
            foreach (var t in TabNames) sb.AppendLine($"\"{t.Key}\" | {t.Value}");
            sb.AppendLine();
            sb.AppendLine("# NABÍDKA OBDOBÍ (period.value = pid)");
            sb.AppendLine("0 | Bez omezení období");
            sb.AppendLine("1 | Vlastní rozsah – vyplnit d1 a d2");
            foreach (var p in Periods.Where(p => p.pid > 1))
                sb.AppendLine($"{p.pid} | {p.PeriodName} | {p.d1:yyyy-MM-dd} – {p.d2:yyyy-MM-dd}");
            return sb.ToString();
        }

        protected override JsonObject BuildSchema()
        {
            var dimIds = Catalog.Dimensions.Select(p => p.Id).ToList();
            var meaIds = Catalog.Measures.Select(p => p.Id).ToList();
            var periodIds = new List<int> { 0, 1 }.Concat(Periods.Select(p => p.pid).Where(p => p > 1)).Distinct();

            var proposal = AiSchema.Obj(new JsonObject
            {
                ["name"] = AiSchema.Str(),
                ["rows"] = AiSchema.Array(AiSchema.StrEnum(dimIds)),
                ["columns"] = AiSchema.Array(AiSchema.StrEnum(dimIds)),
                ["measures"] = AiSchema.Array(AiSchema.Obj(new JsonObject
                {
                    ["key"] = AiSchema.StrEnum(meaIds),
                    ["aggregation"] = AiSchema.StrEnum(AiTotalsValidator.Aggregations)
                })),
                ["state"] = AiSchema.IntEnum(States.Keys),
                ["tab"] = AiSchema.StrEnum(AiTotalsValidator.Tabs),
                ["period"] = AiSchema.Obj(new JsonObject
                {
                    ["value"] = AiSchema.IntEnum(periodIds),
                    ["field"] = AiSchema.StrEnum(AiTotalsValidator.PeriodFields),
                    ["d1"] = AiSchema.Nullable(AiSchema.Date()),
                    ["d2"] = AiSchema.Nullable(AiSchema.Date())
                }),
                ["chart"] = AiSchema.IntEnum(new[] { 0, 1 })
            });

            return AiSchema.Obj(new JsonObject
            {
                ["reply"] = AiSchema.Str(),
                ["proposal"] = AiSchema.Nullable(proposal),
                ["confirm"] = AiSchema.Array(AiSchema.StrEnum(new[] { "period", "state", "tab" }))
            });
        }

        // převod ID (model) <-> klíče (aplikace)
        private AiTotalsProposal ToKeys(AiTotalsProposal p, out List<string> unknown)
        {
            var unk = new List<string>();
            string K(string id) { var e = Catalog.ById(id); if (e == null) { unk.Add(id); return id; } return e.Key; }
            var r = Clone(p);
            r.Rows = p.Rows?.Select(K).ToList() ?? new();
            r.Columns = p.Columns?.Select(K).ToList() ?? new();
            r.Measures = p.Measures?.Select(x => new AiTotalsMeasure { Key = K(x.Key), Aggregation = x.Aggregation }).ToList() ?? new();
            unknown = unk;
            return r;
        }

        private AiTotalsProposal ToIds(AiTotalsProposal p)
        {
            string I(string key) => Catalog.ByKey(key)?.Id ?? key;
            var r = Clone(p);
            r.Rows = p.Rows?.Select(I).ToList() ?? new();
            r.Columns = p.Columns?.Select(I).ToList() ?? new();
            r.Measures = p.Measures?.Select(x => new AiTotalsMeasure { Key = I(x.Key), Aggregation = x.Aggregation }).ToList() ?? new();
            return r;
        }

        private static AiTotalsProposal Clone(AiTotalsProposal p) =>
            JsonSerializer.Deserialize<AiTotalsProposal>(JsonSerializer.Serialize(p));

        // ------------------------------------------------------------------ Save

        public class SaveResult
        {
            public bool Ok { get; set; }
            public int J79ID { get; set; }
            public List<string> Errors { get; set; } = new();
        }

        /// <summary>Uloží potvrzený návrh jako novou osobní šablonu Součtů a předvyplní období.</summary>
        public SaveResult Save(AiTotalsProposal p, long logId = 0)
        {
            var res = new SaveResult();
            if (p != null)
            {
                p.Rows ??= new(); p.Columns ??= new(); p.Measures ??= new(); p.Tab ??= "";
            }
            var v = AiTotalsValidator.Validate(p, Catalog, Periods);
            if (!v.IsValid) { res.Errors = v.Errors; return res; }

            var keys = p.Rows.Concat(p.Columns).Concat(p.Measures.Select(x => x.Key)).ToList();
            var rec = new BO.j79TotalsTemplate
            {
                j02ID = F.CurrentUser.pid,
                j79Entity = null,                     // kořenové Součty (bez kontextu záznamu)
                j79Name = p.Name.Length > 255 ? p.Name.Substring(0, 255) : p.Name,
                j79Columns = string.Join(",", keys),
                j79StateQuery = p.State,
                j79TabQuery = string.IsNullOrEmpty(p.Tab) ? null : p.Tab,
                j79Chart = p.Chart,
                j79IsWebDataRocks = true,
                j79IsSystem = false,
                j79IsPublic = false
            };
            int id = F.j79TotalsTemplateBL.Save(rec, null, null);
            if (id <= 0) { res.Errors.Add("Šablonu se nepodařilo uložit."); return res; }

            // hlavičky stejně jako p31totalsController.GenerateGridColumns → názvy polí v CSV/WebDataRocks
            var cols = _cp.ParseTheGridColumns("p31", rec.j79Columns, F);
            var headerByKey = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var c in cols)
            {
                string h = c.Header.Contains("L5") ? F.getP07Level(5, true) : c.Header;
                headerByKey[c.UniqueName] = h;
            }
            rec = F.j79TotalsTemplateBL.Load(id);
            rec.j79WebDataRocksReport = AiWdrReportBuilder.Build(p, headerByKey,
                $"/p31totals/LoadDatasource?login={F.CurrentUser.j02Login}&j79id={id}");
            F.j79TotalsTemplateBL.Save(rec, null, null);

            // období – stejné klíče jako myPeriodViewModel (UserParamKey "p31totals-period-root")
            const string pk = "p31totals-period-root";
            var per = p.Period ?? new AiTotalsPeriod();
            F.CBL.SetUserParam($"{pk}-value", per.Value.ToString());
            F.CBL.SetUserParam($"{pk}-field", per.Field ?? "p31Date");
            if (per.Value == 1 && AiTotalsValidator.TryDate(per.D1, out var d1) && AiTotalsValidator.TryDate(per.D2, out var d2))
            {
                F.CBL.SetUserParam($"{pk}-d1", d1.ToString("dd.MM.yyyy"));
                F.CBL.SetUserParam($"{pk}-d2", d2.ToString("dd.MM.yyyy"));
            }
            F.CBL.SetUserParam("p31totals--j79id", id.ToString());   // Součty otevřou novou šablonu

            LogResult(logId, "j79", id);
            res.Ok = true;
            res.J79ID = id;
            return res;
        }
    }
}
