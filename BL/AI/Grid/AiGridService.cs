using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using BL.AI.Core;
using BO.AI;

namespace BL.AI.Grid
{
    /// <summary>
    /// AI agent pro návrhář sloupců tabulky (j72TheGridTemplate): z dotazu uživatele navrhne sloupce, jejich pořadí
    /// a sloupcový filtr (stav gridu j75Filter / j75OperatorFlag) a pevné filtry nad gridem (období, stav úkonů, formát, archiv –
    /// uživatelské parametry, viz AiGridFixedFilters).
    /// Entitu určuje tabulka, ze které bylo okno otevřeno (j72Entity) – AI vybírá jen ze sloupců této entity a jejích relací.
    /// Uložení: přepsat tabulku (stejná oprávnění jako TheGridDesigner), uložit jako novou pojmenovanou tabulku,
    /// nebo jen použít filtr na tabulku, ze které bylo okno otevřeno.
    /// </summary>
    public class AiGridService : AiAgentBase<AiGridAnswer, AiGridService.AskResult>
    {
        public const string FeatureCode = "ai-grid";
        /// <summary>Kód do j98Feature včetně entity (např. "ai-grid-p41") – historie dotazů a statistiky zvlášť pro každou entitu.</summary>
        public string FeatureName => $"{FeatureCode}-{Prefix}";
        protected override string Feature => FeatureName;

        private readonly BL.TheColumnsProvider _cp;
        private readonly BL.Singleton.ThePeriodProvider _pp;
        private AiColumnCatalog _catalog;
        private AiGridFixedFilters _fixed;

        public AiGridService(BL.Factory f, BL.TheColumnsProvider cp, BL.Singleton.ThePeriodProvider pp, ILlmClient llm, int j72id) : base(f, llm)
        {
            _cp = cp; _pp = pp;
            Template = j72id > 0 ? F.j72TheGridTemplateBL.Load(j72id) : null;
        }

        /// <summary>Tabulka, ze které bylo okno otevřeno.</summary>
        public BO.j72TheGridTemplate Template { get; }
        public string Prefix => Template?.j72Entity?.Substring(0, 3);
        public string EntityName => Prefix == null ? null : F.EProvider.ByPrefix(Prefix).AliasSingular;
        public AiColumnCatalog Catalog => _catalog ??= AiColumnCatalog.Build(F, _cp, Prefix, AiColumnCatalog.CatalogMode.Grid);
        /// <summary>Pevné filtry, které grid této tabulky nabízí.</summary>
        public AiGridFixedFilters FixedFilters => _fixed ??= new AiGridFixedFilters(F, _pp, Template);

        /// <summary>Stejné pravidlo jako TheGridDesignerController (HasOwnerPermissions).</summary>
        public bool CanOverwrite => Template != null &&
            ((Template.j72SystemFlag == BO.j72SystemFlagEnum.User && (Template.j02ID == F.CurrentUser.pid || F.CurrentUser.IsAdmin)) ||
             (Template.j72SystemFlag == BO.j72SystemFlagEnum.Grid && Template.j02ID == F.CurrentUser.pid));

        public class AskResult : AiAskResult
        {
            public AiGridProposal Proposal { get; set; }   // s klíči sloupců
        }

        // ------------------------------------------------------------------ výchozí stav

        /// <summary>Aktuální sloupce tabulky a sloupcový filtr uživatele jako návrh pro formulář.</summary>
        public AiGridProposal CurrentProposal(List<string> warnings)
        {
            var p = new AiGridProposal { Name = Template.j72SystemFlag == BO.j72SystemFlagEnum.User ? Template.j72Name : null };
            if (!string.IsNullOrEmpty(Template.j72Columns))
            {
                var skipped = new List<string>();
                foreach (var c in _cp.ParseTheGridColumns(Prefix, Template.j72Columns, F))
                {
                    var e = Catalog.ByKey(c.UniqueName);
                    if (e == null) skipped.Add(c.Header ?? c.UniqueName);
                    else if (!p.Columns.Contains(e.Key)) p.Columns.Add(e.Key);
                }
                if (skipped.Count > 0) warnings.Add("Sloupce, které AI návrh nepodporuje a byly vynechány: " + string.Join(", ", skipped));
            }
            var state = F.j72TheGridTemplateBL.LoadState(Template.pid, F.CurrentUser.pid);
            if (state != null)
            {
                var skippedF = new List<string>();
                p.Filter = AiGridFilterCodec.FromJ75(state.j75Filter, Catalog, skippedF).Where(f => p.Columns.Contains(f.Column)).ToList();
                p.MatchAny = state.j75OperatorFlag == 1 && p.Filter.Count > 1;
                if (skippedF.Count > 0) warnings.Add("Podmínky filtru, které AI návrh nepodporuje a byly vynechány: " + string.Join(", ", skippedF));
            }
            p.Fixed = FixedFilters.Load();
            return p;
        }

        // ------------------------------------------------------------------ Ask

        private static readonly string[] _days = { "neděle", "pondělí", "úterý", "středa", "čtvrtek", "pátek", "sobota" };

        protected override List<LlmSystemBlock> BuildSystem() => new()
        {
            new LlmSystemBlock { Text = AiPromptStore.Get("grid.system.md") },
            new LlmSystemBlock { Text = Catalog.ToPromptText(), Cache = true },   // cache breakpoint: instrukce + katalog entity
            new LlmSystemBlock { Text = FixedFilters.ToPromptText() },
            new LlmSystemBlock { Text = $"Entita tabulky: {EntityName} ({Prefix}).\nTabulka: {Template?.GetName()}.\nDnešní datum: {DateTime.Today:yyyy-MM-dd} ({_days[(int)DateTime.Today.DayOfWeek]}).\nPřihlášený uživatel: {F.CurrentUser.j02FirstName} {F.CurrentUser.j02LastName}." }
        };

        protected override string DecorateLastUserMessage(string content, BO.AI.AiAskInput input)
        {
            var current = (input as AiGridAskInput)?.Current;
            if (current == null) return content;
            return content + "\n\nAktuální návrh ve formuláři (sloupce v pořadí, sloupcový filtr a pevné filtry; uživatel je mohl upravit):\n" + JsonSerializer.Serialize(ToIds(current));
        }

        protected override JsonObject BuildSchema()
        {
            var ids = Catalog.Items.Select(p => p.Id).ToList();
            var filterIds = Catalog.Items.Where(p => p.FilterType != null).Select(p => p.Id).ToList();
            var filter = AiSchema.Obj(new JsonObject
            {
                ["column"] = AiSchema.StrEnum(filterIds),
                ["op"] = AiSchema.StrEnum(AiGridFilterCodec.AllOps),
                ["values"] = AiSchema.Array(AiSchema.Str()),
                ["from"] = AiSchema.Nullable(AiSchema.Str()),
                ["to"] = AiSchema.Nullable(AiSchema.Str())
            });
            var proposal = AiSchema.Obj(new JsonObject
            {
                ["name"] = AiSchema.Str(),
                ["columns"] = AiSchema.Array(AiSchema.StrEnum(ids)),
                ["filter"] = AiSchema.Array(filter),
                ["matchAny"] = new JsonObject { ["type"] = "boolean" }
            });
            var fixedSchema = FixedFilters.BuildSchema();
            if (fixedSchema != null)
            {
                ((JsonObject)proposal["properties"])["fixed"] = fixedSchema;
                ((JsonArray)proposal["required"]).Add("fixed");
            }
            return AiSchema.Obj(new JsonObject
            {
                ["reply"] = AiSchema.Str(),
                ["proposal"] = AiSchema.Nullable(proposal)
            });
        }

        protected override List<string> ProcessAnswer(AiGridAnswer ans, AskResult res)
        {
            res.Reply = ans.Reply;
            res.Proposal = null;
            if (ans.Proposal == null) return new();
            res.Proposal = ToKeys(ans.Proposal);
            var v = Validate(res.Proposal, false);
            if (v.errors.Count == 0) v.warnings.InsertRange(0, AddFilterColumns(res.Proposal));
            res.Warnings = v.warnings;
            return v.errors;
        }

        private (List<string> errors, List<string> warnings) Validate(AiGridProposal p, bool requireName)
        {
            var errors = new List<string>(); var warnings = new List<string>();
            if (p == null) { errors.Add("Návrh je prázdný."); return (errors, warnings); }
            if (p.Columns == null || p.Columns.Count == 0) errors.Add("Tabulka musí mít alespoň jeden sloupec.");
            var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var k in p.Columns ?? new())
            {
                if (Catalog.ByKey(k) == null) errors.Add($"Neznámý sloupec: {k}");
                if (!used.Add(k)) errors.Add($"Sloupec '{Catalog.ByKey(k)?.Header ?? k}' je použit vícekrát.");
            }
            if (requireName && string.IsNullOrWhiteSpace(p.Name)) errors.Add("Chybí název nové tabulky.");
            AiGridFilterCodec.Validate(p.Filter, Catalog, errors);
            FixedFilters.Validate(p.Fixed, errors);
            if ((p.Columns?.Count ?? 0) > 40) warnings.Add("Tabulka má přes 40 sloupců – může být nepřehledná a pomalejší.");
            return (errors, warnings);
        }

        /// <summary>Grid filtruje jen podle zobrazených sloupců – chybějící sloupce z filtru doplní na konec.</summary>
        private List<string> AddFilterColumns(AiGridProposal p)
        {
            var added = new List<string>();
            foreach (var f in p.Filter ?? new())
            {
                if (f.Column == null || p.Columns.Contains(f.Column, StringComparer.OrdinalIgnoreCase)) continue;
                p.Columns.Add(f.Column);
                added.Add(Catalog.ByKey(f.Column)?.Header ?? f.Column);
            }
            return added.Count == 0 ? new() : new() { "Do tabulky byly doplněny sloupce, podle kterých se filtruje: " + string.Join(", ", added) };
        }

        private AiGridProposal ToKeys(AiGridProposal p) => Map(p, id => Catalog.ById(id)?.Key ?? id);   // neznámá ID zachytí validace
        private AiGridProposal ToIds(AiGridProposal p) => Map(p, k => Catalog.ByKey(k)?.Id ?? k);

        private static AiGridProposal Map(AiGridProposal p, Func<string, string> conv) => new()
        {
            Name = p.Name,
            Columns = (p.Columns ?? new()).Select(conv).ToList(),
            Filter = (p.Filter ?? new()).Select(f => new AiGridFilter { Column = conv(f.Column), Op = f.Op, Values = f.Values ?? new(), From = f.From, To = f.To }).ToList(),
            MatchAny = p.MatchAny,
            Fixed = p.Fixed
        };

        // ------------------------------------------------------------------ Save

        public class SaveResult
        {
            public bool Ok { get; set; }
            public int J72ID { get; set; }
            public List<string> Errors { get; set; } = new();
        }

        /// <summary>Režim uložení: přepsat tabulku, uložit jako novou, nebo jen použít filtr.</summary>
        public enum SaveMode { Overwrite, New, Filter }

        /// <summary>
        /// Overwrite → přepíše sloupce tabulky, ze které bylo okno otevřeno (jen s oprávněním);
        /// New → uloží jako novou pojmenovanou tabulku (kopie včetně filtru j73) a přepne na ni grid volající stránky;
        /// Filter → nemění tabulku, jen nastaví sloupcový filtr (všechny jeho sloupce musí v tabulce už být).
        /// Pevné filtry (období, stav, formát, archiv) se ukládají ve všech režimech.
        /// Sloupcový filtr se ukládá do stavu gridu uživatele (j75Filter) – stejně jako filtr zadaný v hlavičce gridu.
        /// </summary>
        public SaveResult Save(AiGridProposal p, long logId, SaveMode mode, string callerPathName)
        {
            var res = new SaveResult();
            if (Template == null) { res.Errors.Add("Tabulka nebyla nalezena."); return res; }
            p.Filter ??= new();
            if (mode == SaveMode.Filter)
            {
                var errs = new List<string>();
                AiGridFilterCodec.Validate(p.Filter, Catalog, errs);
                FixedFilters.Validate(p.Fixed, errs);
                var tableCols = (Template.j72Columns ?? "").Split(",").ToHashSet(StringComparer.OrdinalIgnoreCase);
                var missing = p.Filter.Where(f => !tableCols.Contains(f.Column ?? "")).Select(f => Catalog.ByKey(f.Column)?.Header ?? f.Column).Distinct().ToList();
                if (missing.Count > 0) errs.Add("Filtr používá sloupce, které v uložené tabulce nejsou: " + string.Join(", ", missing) + ". Uložte nejdřív tabulku (Přepsat / Uložit jako novou).");
                if (errs.Count > 0) { res.Errors = errs; return res; }
                SaveState(Template.pid, Template.j72Columns, p);
                FixedFilters.Save(p.Fixed);
                LogResult(logId, "j72", Template.pid);
                res.Ok = true; res.J72ID = Template.pid;
                return res;
            }

            AddFilterColumns(p);
            var v = Validate(p, mode == SaveMode.New);
            if (v.errors.Count > 0) { res.Errors = v.errors; return res; }
            string columns = string.Join(",", p.Columns);

            int id;
            if (mode == SaveMode.Overwrite)
            {
                if (!CanOverwrite) { res.Errors.Add("Nemáte oprávnění upravovat tuto tabulku."); return res; }
                var rec = F.j72TheGridTemplateBL.Load(Template.pid);
                rec.j72Columns = columns;
                id = F.j72TheGridTemplateBL.Save(rec, null, null, null);      // null = filtr j73 a sdílení beze změny
            }
            else
            {
                var rec = F.j72TheGridTemplateBL.Load(Template.pid);
                var lisJ73 = F.j72TheGridTemplateBL.GetList_j73(rec.pid, Prefix, 0).ToList();   // stejně jako „Uložit jako“ v návrháři
                rec.j72SystemFlag = BO.j72SystemFlagEnum.User; rec.j72ID = 0; rec.pid = 0;
                rec.j72Name = p.Name.Length > 100 ? p.Name.Substring(0, 100) : p.Name;
                rec.j02ID = F.CurrentUser.pid;
                rec.j72IsPublic = false;
                rec.j72Columns = columns;
                id = F.j72TheGridTemplateBL.Save(rec, lisJ73, new List<int>(), new List<int>());
            }
            if (id <= 0) { res.Errors.Add("Tabulku se nepodařilo uložit."); return res; }

            SaveState(id, columns, p);
            FixedFilters.Save(p.Fixed);

            // přepnout grid volající stránky na uloženou tabulku (stejné klíče jako návrhář)
            string px = Template.j72Entity.Substring(0, 3), rez = Template.j72Rez;
            if (callerPathName != null && callerPathName.Contains("MasterView")) F.CBL.SetUserParam($"masterview-j72id-{px}-{rez}", id.ToString());
            if (callerPathName != null && callerPathName.Contains("FlatView")) F.CBL.SetUserParam($"flatview-j72id-{px}-{rez}", id.ToString());
            if (callerPathName != null && callerPathName.Contains("SlaveView") && Template.j72MasterEntity != null)
                F.CBL.SetUserParam($"slaveview-j72id-{px}-{Template.j72MasterEntity}-{rez}", id.ToString());

            LogResult(logId, "j72", id);
            res.Ok = true;
            res.J72ID = id;
            return res;
        }

        /// <summary>Stav gridu uživatele – sloupcový filtr z návrhu, stránkování a řazení jako TheGridDesignerController / Event_HandleTheGridFilter.</summary>
        private void SaveState(int j72id, string columns, AiGridProposal p)
        {
            var state = F.j72TheGridTemplateBL.LoadState(j72id, F.CurrentUser.pid);
            if (state == null) return;
            state.j75Filter = AiGridFilterCodec.ToJ75(p.Filter, Catalog);
            state.j75OperatorFlag = p.MatchAny && !string.IsNullOrEmpty(state.j75Filter) ? 1 : 0;
            state.j75CurrentPagerIndex = 0;
            state.j75CurrentRecordPid = 0;
            if (!string.IsNullOrEmpty(state.j75SortDataField) && (columns ?? "").IndexOf(state.j75SortDataField) == -1)
            {
                state.j75SortDataField = ""; state.j75SortOrder = "";
            }
            F.j72TheGridTemplateBL.SaveState(state, F.CurrentUser.pid);
        }
    }
}
