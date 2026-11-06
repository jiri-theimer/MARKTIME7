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
    /// Instance na jeden request (Factory je scoped).
    /// </summary>
    public class AiTotalsService
    {
        private const string Feature = "ai-totals";
        private readonly BL.Factory _f;
        private readonly BL.TheColumnsProvider _cp;
        private readonly BL.Singleton.ThePeriodProvider _pp;
        private readonly ILlmClient _llm;
        private AiTotalsCatalog _catalog;

        public AiTotalsService(BL.Factory f, BL.TheColumnsProvider cp, BL.Singleton.ThePeriodProvider pp, ILlmClient llm)
        {
            _f = f; _cp = cp; _pp = pp; _llm = llm;
        }

        public AiTotalsCatalog Catalog => _catalog ??= AiTotalsCatalog.Build(_f, _cp);
        public List<BO.ThePeriod> Periods => _pp.getPallete();
        public bool IsEnabled => _llm.Options.Enabled;

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

        public class AskResult
        {
            public bool Ok { get; set; }
            public string Error { get; set; }
            public string Reply { get; set; }
            public AiTotalsProposal Proposal { get; set; }      // s klíči sloupců
            public List<string> Confirm { get; set; } = new();
            public List<string> Warnings { get; set; } = new();
            public string Raw { get; set; }                     // odpověď modelu (s ID) – klient ji vrací v historii
            public long LogId { get; set; }                     // j99ID – pro hodnocení a vazbu na uloženou šablonu
        }

        // ------------------------------------------------------------------ Ask

        public async Task<AskResult> AskAsync(AiTotalsAskInput input, CancellationToken ct = default)
        {
            var res = new AskResult();
            if (!IsEnabled) { res.Error = "AI není zapnuté."; return res; }
            if (input?.Messages == null || !input.Messages.Any(p => p.Role == "user")) { res.Error = "Chybí dotaz."; return res; }

            var req = new LlmRequest
            {
                Schema = BuildSchema(),
                System = new()
                {
                    new LlmSystemBlock { Text = AiPromptStore.Get("totals.system.md") },
                    new LlmSystemBlock { Text = Catalog.ToPromptText(), Cache = true },   // cache breakpoint: instrukce + katalog
                    new LlmSystemBlock { Text = BuildDynamicContext() }
                }
            };

            var msgs = input.Messages.TakeLast(20).ToList();
            for (int i = 0; i < msgs.Count; i++)
            {
                string content = msgs[i].Content;
                if (i == msgs.Count - 1 && msgs[i].Role == "user" && input.Current != null)
                {
                    content += "\n\nAktuální návrh ve formuláři (uživatel jej mohl upravit):\n" + JsonSerializer.Serialize(ToIds(input.Current));
                }
                req.Messages.Add(new LlmMessage { Role = msgs[i].Role, Content = content });
            }

            var log = new AiLogEntry
            {
                Feature = Feature,
                ContextPage = input.ContextPage,
                ConversationGuid = input.ConversationGuid,
                TurnIndex = input.Messages.Count(p => p.Role == "user"),
                QueryText = msgs.Last(p => p.Role == "user").Content
            };

            await RunAsync(req, res, log, ct);

            log.AnswerText = res.Reply;
            if (!res.Ok) { log.Status = 'E'; log.ErrorText = res.Error; }
            res.LogId = AiLog.Insert(_f, log);
            return res;
        }

        private async Task RunAsync(LlmRequest req, AskResult res, AiLogEntry log, CancellationToken ct)
        {
            for (int attempt = 0; attempt < 2; attempt++)
            {
                var r = await _llm.CompleteJsonAsync(req, ct);
                log.Add(r);
                log.AnswerJson = r.Json;
                if (!r.Ok) { res.Error = r.Error; return; }

                AiTotalsAnswer ans;
                try { ans = JsonSerializer.Deserialize<AiTotalsAnswer>(r.Json); }
                catch (Exception ex) { res.Error = "Nelze zpracovat odpověď AI: " + ex.Message; return; }

                res.Raw = r.Json;
                res.Reply = ans.Reply;
                res.Confirm = ans.Confirm ?? new();
                if (ans.Proposal == null) { res.Ok = true; return; }

                var proposal = ToKeys(ans.Proposal, out _);     // neznámá ID zachytí validátor
                var v = AiTotalsValidator.Validate(proposal, Catalog, Periods);

                if (v.IsValid)
                {
                    res.Ok = true;
                    res.Proposal = proposal;
                    res.Warnings = v.Warnings;
                    return;
                }

                if (attempt == 1)
                {
                    res.Ok = true;           // vrátit aspoň částečný návrh, uživatel jej opraví ve formuláři
                    res.Proposal = proposal;
                    res.Warnings = v.Errors.Concat(v.Warnings).ToList();
                    log.Status = 'V';
                    log.ErrorText = string.Join("; ", v.Errors);
                    return;
                }

                // jeden pokus o opravu
                log.RepairCount++;
                req.Messages.Add(new LlmMessage { Role = "assistant", Content = r.Json });
                req.Messages.Add(new LlmMessage { Role = "user", Content = "Návrh obsahuje chyby, oprav je a vrať celý návrh znovu:\n- " + string.Join("\n- ", v.Errors) });
            }
        }

        /// <summary>Hodnocení odpovědi palcem nahoru/dolů.</summary>
        public bool Rate(long logId, int rating, string comment) => AiLog.Rate(_f, logId, rating, comment);

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

        private JsonObject BuildSchema()
        {
            JsonArray Arr(IEnumerable<string> v) { var a = new JsonArray(); foreach (var x in v) a.Add(x); return a; }
            JsonArray ArrI(IEnumerable<int> v) { var a = new JsonArray(); foreach (var x in v) a.Add(x); return a; }
            JsonObject Obj(JsonObject props) => new JsonObject
            {
                ["type"] = "object", ["properties"] = props,
                ["required"] = Arr(props.Select(p => p.Key)), ["additionalProperties"] = false
            };
            JsonObject StrEnum(IEnumerable<string> v) => new JsonObject { ["type"] = "string", ["enum"] = Arr(v) };
            JsonObject NullableDate() => new JsonObject
            {
                ["anyOf"] = new JsonArray { new JsonObject { ["type"] = "string", ["format"] = "date" }, new JsonObject { ["type"] = "null" } }
            };

            var dimIds = Catalog.Dimensions.Select(p => p.Id).ToList();
            var meaIds = Catalog.Measures.Select(p => p.Id).ToList();
            var periodIds = new List<int> { 0, 1 }.Concat(Periods.Select(p => p.pid).Where(p => p > 1)).Distinct();

            var proposal = Obj(new JsonObject
            {
                ["name"] = new JsonObject { ["type"] = "string" },
                ["rows"] = new JsonObject { ["type"] = "array", ["items"] = StrEnum(dimIds) },
                ["columns"] = new JsonObject { ["type"] = "array", ["items"] = StrEnum(dimIds) },
                ["measures"] = new JsonObject
                {
                    ["type"] = "array",
                    ["items"] = Obj(new JsonObject { ["key"] = StrEnum(meaIds), ["aggregation"] = StrEnum(AiTotalsValidator.Aggregations) })
                },
                ["state"] = new JsonObject { ["type"] = "integer", ["enum"] = ArrI(States.Keys) },
                ["tab"] = StrEnum(AiTotalsValidator.Tabs),
                ["period"] = Obj(new JsonObject
                {
                    ["value"] = new JsonObject { ["type"] = "integer", ["enum"] = ArrI(periodIds) },
                    ["field"] = StrEnum(AiTotalsValidator.PeriodFields),
                    ["d1"] = NullableDate(),
                    ["d2"] = NullableDate()
                }),
                ["chart"] = new JsonObject { ["type"] = "integer", ["enum"] = ArrI(new[] { 0, 1 }) }
            });

            return Obj(new JsonObject
            {
                ["reply"] = new JsonObject { ["type"] = "string" },
                ["proposal"] = new JsonObject { ["anyOf"] = new JsonArray { proposal, new JsonObject { ["type"] = "null" } } },
                ["confirm"] = new JsonObject { ["type"] = "array", ["items"] = StrEnum(new[] { "period", "state", "tab" }) }
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
                j02ID = _f.CurrentUser.pid,
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
            int id = _f.j79TotalsTemplateBL.Save(rec, null, null);
            if (id <= 0) { res.Errors.Add("Šablonu se nepodařilo uložit."); return res; }

            // hlavičky stejně jako p31totalsController.GenerateGridColumns → názvy polí v CSV/WebDataRocks
            var cols = _cp.ParseTheGridColumns("p31", rec.j79Columns, _f);
            var headerByKey = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var c in cols)
            {
                string h = c.Header.Contains("L5") ? _f.getP07Level(5, true) : c.Header;
                headerByKey[c.UniqueName] = h;
            }
            rec = _f.j79TotalsTemplateBL.Load(id);
            rec.j79WebDataRocksReport = AiWdrReportBuilder.Build(p, headerByKey,
                $"/p31totals/LoadDatasource?login={_f.CurrentUser.j02Login}&j79id={id}");
            _f.j79TotalsTemplateBL.Save(rec, null, null);

            // období – stejné klíče jako myPeriodViewModel (UserParamKey "p31totals-period-root")
            const string pk = "p31totals-period-root";
            var per = p.Period ?? new AiTotalsPeriod();
            _f.CBL.SetUserParam($"{pk}-value", per.Value.ToString());
            _f.CBL.SetUserParam($"{pk}-field", per.Field ?? "p31Date");
            if (per.Value == 1 && AiTotalsValidator.TryDate(per.D1, out var d1) && AiTotalsValidator.TryDate(per.D2, out var d2))
            {
                _f.CBL.SetUserParam($"{pk}-d1", d1.ToString("dd.MM.yyyy"));
                _f.CBL.SetUserParam($"{pk}-d2", d2.ToString("dd.MM.yyyy"));
            }
            _f.CBL.SetUserParam("p31totals--j79id", id.ToString());   // Součty otevřou novou šablonu

            AiLog.SetResult(_f, logId, "j79", id);
            res.Ok = true;
            res.J79ID = id;
            return res;
        }
    }
}
