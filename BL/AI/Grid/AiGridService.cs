using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using BL.AI.Core;
using BO.AI;

namespace BL.AI.Grid
{
    /// <summary>
    /// AI agent pro návrhář sloupců tabulky (j72TheGridTemplate): z dotazu uživatele navrhne sloupce a jejich pořadí.
    /// Entitu určuje tabulka, ze které bylo okno otevřeno (j72Entity) – AI vybírá jen ze sloupců této entity a jejích relací.
    /// Uložení: přepsat tabulku (stejná oprávnění jako TheGridDesigner) nebo uložit jako novou pojmenovanou tabulku.
    /// </summary>
    public class AiGridService : AiAgentBase<AiGridAnswer, AiGridService.AskResult>
    {
        public const string FeatureCode = "ai-grid";
        /// <summary>Kód do j98Feature včetně entity (např. "ai-grid-p41") – historie dotazů a statistiky zvlášť pro každou entitu.</summary>
        public string FeatureName => $"{FeatureCode}-{Prefix}";
        protected override string Feature => FeatureName;

        private readonly BL.TheColumnsProvider _cp;
        private AiColumnCatalog _catalog;

        public AiGridService(BL.Factory f, BL.TheColumnsProvider cp, ILlmClient llm, int j72id) : base(f, llm)
        {
            _cp = cp;
            Template = j72id > 0 ? F.j72TheGridTemplateBL.Load(j72id) : null;
        }

        /// <summary>Tabulka, ze které bylo okno otevřeno.</summary>
        public BO.j72TheGridTemplate Template { get; }
        public string Prefix => Template?.j72Entity?.Substring(0, 3);
        public string EntityName => Prefix == null ? null : F.EProvider.ByPrefix(Prefix).AliasSingular;
        public AiColumnCatalog Catalog => _catalog ??= AiColumnCatalog.Build(F, _cp, Prefix, AiColumnCatalog.CatalogMode.Grid);

        /// <summary>Stejné pravidlo jako TheGridDesignerController (HasOwnerPermissions).</summary>
        public bool CanOverwrite => Template != null &&
            ((Template.j72SystemFlag == BO.j72SystemFlagEnum.User && (Template.j02ID == F.CurrentUser.pid || F.CurrentUser.IsAdmin)) ||
             (Template.j72SystemFlag == BO.j72SystemFlagEnum.Grid && Template.j02ID == F.CurrentUser.pid));

        public class AskResult : AiAskResult
        {
            public AiGridProposal Proposal { get; set; }   // s klíči sloupců
        }

        // ------------------------------------------------------------------ výchozí stav

        /// <summary>Aktuální sloupce tabulky jako návrh pro formulář.</summary>
        public AiGridProposal CurrentProposal(List<string> warnings)
        {
            var p = new AiGridProposal { Name = Template.j72SystemFlag == BO.j72SystemFlagEnum.User ? Template.j72Name : null };
            if (string.IsNullOrEmpty(Template.j72Columns)) return p;
            var skipped = new List<string>();
            foreach (var c in _cp.ParseTheGridColumns(Prefix, Template.j72Columns, F))
            {
                var e = Catalog.ByKey(c.UniqueName);
                if (e == null) skipped.Add(c.Header ?? c.UniqueName);
                else if (!p.Columns.Contains(e.Key)) p.Columns.Add(e.Key);
            }
            if (skipped.Count > 0) warnings.Add("Sloupce, které AI návrh nepodporuje a byly vynechány: " + string.Join(", ", skipped));
            return p;
        }

        // ------------------------------------------------------------------ Ask

        protected override List<LlmSystemBlock> BuildSystem() => new()
        {
            new LlmSystemBlock { Text = AiPromptStore.Get("grid.system.md") },
            new LlmSystemBlock { Text = Catalog.ToPromptText(), Cache = true },   // cache breakpoint: instrukce + katalog entity
            new LlmSystemBlock { Text = $"Entita tabulky: {EntityName} ({Prefix}).\nTabulka: {Template?.GetName()}.\nDnešní datum: {DateTime.Today:yyyy-MM-dd}." }
        };

        protected override string DecorateLastUserMessage(string content, BO.AI.AiAskInput input)
        {
            var current = (input as AiGridAskInput)?.Current;
            if (current == null) return content;
            return content + "\n\nAktuální sloupce ve formuláři (v pořadí; uživatel je mohl upravit):\n" + JsonSerializer.Serialize(ToIds(current));
        }

        protected override JsonObject BuildSchema()
        {
            var ids = Catalog.Items.Select(p => p.Id).ToList();
            var proposal = AiSchema.Obj(new JsonObject
            {
                ["name"] = AiSchema.Str(),
                ["columns"] = AiSchema.Array(AiSchema.StrEnum(ids))
            });
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
            if ((p.Columns?.Count ?? 0) > 40) warnings.Add("Tabulka má přes 40 sloupců – může být nepřehledná a pomalejší.");
            return (errors, warnings);
        }

        private AiGridProposal ToKeys(AiGridProposal p) => new()
        {
            Name = p.Name,
            Columns = (p.Columns ?? new()).Select(id => Catalog.ById(id)?.Key ?? id).ToList()   // neznámá ID zachytí validace
        };

        private AiGridProposal ToIds(AiGridProposal p) => new()
        {
            Name = p.Name,
            Columns = (p.Columns ?? new()).Select(k => Catalog.ByKey(k)?.Id ?? k).ToList()
        };

        // ------------------------------------------------------------------ Save

        public class SaveResult
        {
            public bool Ok { get; set; }
            public int J72ID { get; set; }
            public List<string> Errors { get; set; } = new();
        }

        /// <summary>
        /// overwrite = true → přepíše sloupce tabulky, ze které bylo okno otevřeno (jen s oprávněním);
        /// false → uloží jako novou pojmenovanou tabulku (kopie včetně filtru j73) a přepne na ni grid volající stránky.
        /// Stav gridu (filtr sloupců, stránkování, řazení) se upraví stejně jako v TheGridDesigneru.
        /// </summary>
        public SaveResult Save(AiGridProposal p, long logId, bool overwrite, string callerPathName)
        {
            var res = new SaveResult();
            if (Template == null) { res.Errors.Add("Tabulka nebyla nalezena."); return res; }
            var v = Validate(p, !overwrite);
            if (v.errors.Count > 0) { res.Errors = v.errors; return res; }
            string columns = string.Join(",", p.Columns);

            int id;
            if (overwrite)
            {
                if (!CanOverwrite) { res.Errors.Add("Nemáte oprávnění upravovat tuto tabulku."); return res; }
                var rec = F.j72TheGridTemplateBL.Load(Template.pid);
                rec.j72Columns = columns;
                id = F.j72TheGridTemplateBL.Save(rec, null, null, null);      // null = filtr a sdílení beze změny
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

            // stav gridu – stejně jako TheGridDesignerController
            var state = F.j72TheGridTemplateBL.LoadState(id, F.CurrentUser.pid);
            if (state != null)
            {
                state.j75Filter = "";
                state.j75CurrentPagerIndex = 0;
                state.j75CurrentRecordPid = 0;
                if (!string.IsNullOrEmpty(state.j75SortDataField) && columns.IndexOf(state.j75SortDataField) == -1)
                {
                    state.j75SortDataField = ""; state.j75SortOrder = "";
                }
                F.j72TheGridTemplateBL.SaveState(state, F.CurrentUser.pid);
            }

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
    }
}
