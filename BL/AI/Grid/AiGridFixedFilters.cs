using System.Text;
using BL.AI.Totals;
using BO.AI;

namespace BL.AI.Grid
{
    /// <summary>
    /// Pevné filtry nad gridem (rozbalovací tlačítka): časové období, stav úkonů, formát úkonů, archiv.
    /// Nejsou součástí šablony tabulky – ukládají se do uživatelských parametrů podle entity (a master entity),
    /// stejně jako v TheGridController.LoadFsmViewModel / myPeriodViewModel.SaveUserSetting.
    /// </summary>
    public class AiGridFixedFilters
    {
        private readonly BL.Factory _f;
        private readonly string _prefix;
        private readonly string _master;       // j72MasterEntity (SlaveView, approve_aio…), null = FlatView/MasterView
        private readonly bool _aio;

        public List<BO.ThePeriod> Periods { get; }
        /// <summary>Datumová pole období pro entitu (id, název) – stejně jako Views/myPeriod/selector.cshtml; prázdné = grid nemá filtr období.</summary>
        public List<(string Id, string Name)> PeriodFields { get; } = new();
        public bool HasPeriod => PeriodFields.Count > 0;
        public bool HasState { get; }
        public bool HasTab { get; }
        public bool HasArchive { get; }
        public bool Any => HasPeriod || HasState || HasTab || HasArchive;

        public static readonly Dictionary<int, string> ArchiveNames = new() { [0] = "Otevřené i v archivu", [1] = "Pouze otevřené záznamy", [2] = "Pouze záznamy v archivu" };
        public IEnumerable<int> ArchiveValues => _prefix == "p31" ? new[] { 1, 2 } : ArchiveNames.Keys;

        public AiGridFixedFilters(BL.Factory f, BL.Singleton.ThePeriodProvider pp, BO.j72TheGridTemplate template)
        {
            _f = f;
            _prefix = template.j72Entity.Substring(0, 3);
            _master = string.IsNullOrEmpty(template.j72MasterEntity) ? null : template.j72MasterEntity;
            _aio = _master == "approve_aio";
            var u = f.CurrentUser;

            Periods = pp.getPallete().Where(p => p.pid > 1).ToList();
            foreach (var x in f.FBL.GetListX21(u.pid) ?? Enumerable.Empty<BO.x21DatePeriod>())   // uživatelská pojmenovaná období
                Periods.Add(new BO.ThePeriod { pid = x.pid, PeriodName = x.x21Name, d1 = x.x21ValidFrom, d2 = x.x21ValidUntil, IsUserPeriod = true });

            // TheGridController.TestIfPeriodOverGrid (approve/fpview mají období vždy)
            bool periodOverGrid = _aio || _master == "fpview_aio" || (u.j04IsModule_p31 && _prefix != "p51" && _prefix != "p44" && _prefix != "p15");
            if (periodOverGrid) PeriodFields.AddRange(FieldsOf(_prefix));

            HasState = u.j04IsModule_p31 && _master != "p91Invoice" && new[] { "p31", "p41", "p28", "o23", "p56", "j02" }.Contains(_prefix);
            HasTab = u.j04IsModule_p31 && (_prefix == "p91" || HasState) && (_master == null || _aio);
            HasArchive = _prefix != "j92" && _prefix != "j90" && _prefix != "x40";
        }

        // ------------------------------------------------------------------ klíče uživatelských parametrů

        private string PeriodKey => _master == null ? $"grid-period-{_prefix}" : (_aio ? "approve-index-period" : $"grid-period-{_prefix}-{_master}");
        private string Key(string name, string aioKey) => _master == null ? $"grid-{_prefix}-{name}" : (_aio ? aioKey : $"grid-{_prefix}-{_master}-{name}");
        private string StateKey => Key("p31statequery", "approve-p31statequery");
        private string TabKey => Key("p31formatquery", "approve-p31tabquery");
        private string ArchiveKey => Key("recordbinquery", "approve-recordbinquery");

        // ------------------------------------------------------------------ načtení / uložení

        /// <summary>Aktuální nastavení uživatele (stejné výchozí hodnoty jako TheGridController).</summary>
        public AiGridFixed Load()
        {
            var r = new AiGridFixed();
            if (HasPeriod)
            {
                string deffield = _prefix == "p91" ? "p91DateSupply" : null;
                var per = new AiTotalsPeriod
                {
                    Value = _f.CBL.LoadUserParamInt($"{PeriodKey}-value", 0),
                    Field = _f.CBL.LoadUserParam($"{PeriodKey}-field", deffield)
                };
                if (!PeriodFields.Any(p => p.Id == per.Field)) per.Field = PeriodFields[0].Id;
                if (per.Value == 1)
                {
                    per.D1 = _f.CBL.LoadUserParamDate($"{PeriodKey}-d1")?.ToString("yyyy-MM-dd");
                    per.D2 = _f.CBL.LoadUserParamDate($"{PeriodKey}-d2")?.ToString("yyyy-MM-dd");
                }
                else if (per.Value != 0 && !Periods.Any(p => p.pid == per.Value)) per.Value = 0;
                r.Period = per;
            }
            if (HasState)
            {
                r.State = _f.CBL.LoadUserParamInt(StateKey, _aio ? 3 : 0);
                if (!AiTotalsService.States.ContainsKey(r.State.Value)) r.State = 0;
            }
            if (HasTab)
            {
                r.Tab = _f.CBL.LoadUserParam(TabKey) ?? "";
                if (!AiTotalsValidator.Tabs.Contains(r.Tab)) r.Tab = "";
            }
            if (HasArchive)
            {
                r.Archive = _f.CBL.LoadUserParamInt(ArchiveKey, 1);
                if (!ArchiveValues.Contains(r.Archive.Value)) r.Archive = 1;
            }
            return r;
        }

        public void Validate(AiGridFixed x, List<string> errors)
        {
            if (x == null) return;
            if (HasPeriod && x.Period != null)
            {
                var per = x.Period;
                if (!PeriodFields.Any(p => p.Id == per.Field)) errors.Add($"Období: neplatné datumové pole '{per.Field}' (povoleno: {string.Join(", ", PeriodFields.Select(p => p.Id))}).");
                if (per.Value == 1)
                {
                    if (!AiTotalsValidator.TryDate(per.D1, out var d1) || !AiTotalsValidator.TryDate(per.D2, out var d2)) errors.Add("Období: vlastní rozsah musí mít obě data od–do (yyyy-MM-dd).");
                    else if (d1 > d2) errors.Add("Období: datum od je větší než datum do.");
                }
                else if (per.Value != 0 && !Periods.Any(p => p.pid == per.Value)) errors.Add($"Období: neznámé pojmenované období {per.Value}.");
            }
            if (HasState && x.State != null && !AiTotalsService.States.ContainsKey(x.State.Value)) errors.Add($"Neplatný stav úkonů: {x.State}");
            if (HasTab && x.Tab != null && !AiTotalsValidator.Tabs.Contains(x.Tab)) errors.Add($"Neplatný formát úkonů: {x.Tab}");
            if (HasArchive && x.Archive != null && !ArchiveValues.Contains(x.Archive.Value)) errors.Add($"Neplatná hodnota archivu: {x.Archive}");
        }

        /// <summary>Uloží do uživatelských parametrů – jen filtry, které grid má a návrh obsahuje.</summary>
        public void Save(AiGridFixed x)
        {
            if (x == null) return;
            if (HasPeriod && x.Period != null)
            {
                var per = x.Period;
                _f.CBL.SetUserParam($"{PeriodKey}-field", per.Field);
                _f.CBL.SetUserParam($"{PeriodKey}-value", per.Value.ToString());
                if (per.Value == 1 && AiTotalsValidator.TryDate(per.D1, out var d1) && AiTotalsValidator.TryDate(per.D2, out var d2))
                {
                    _f.CBL.SetUserParam($"{PeriodKey}-d1", d1.ToString("dd.MM.yyyy"));
                    _f.CBL.SetUserParam($"{PeriodKey}-d2", d2.ToString("dd.MM.yyyy"));
                }
            }
            if (HasState && x.State != null) _f.CBL.SetUserParam(StateKey, x.State.Value.ToString());
            if (HasTab && x.Tab != null) _f.CBL.SetUserParam(TabKey, x.Tab);
            if (HasArchive && x.Archive != null) _f.CBL.SetUserParam(ArchiveKey, x.Archive.Value.ToString());
        }

        // ------------------------------------------------------------------ pro model a UI

        /// <summary>Nabídka pevných filtrů pro system prompt.</summary>
        public string ToPromptText()
        {
            var sb = new StringBuilder();
            sb.AppendLine("# PEVNÉ FILTRY TÉTO TABULKY (fixed)");
            if (!Any) { sb.AppendLine("Tato tabulka nemá žádné pevné filtry – fixed vynech."); return sb.ToString(); }
            if (HasPeriod)
            {
                sb.AppendLine("## Časové období (fixed.period: value, field, d1, d2)");
                sb.AppendLine("value: 0 = bez omezení období, 1 = vlastní rozsah d1–d2 (yyyy-MM-dd), jinak id pojmenovaného období:");
                foreach (var p in Periods) sb.AppendLine($"{p.pid} | {p.PeriodName} | {p.d1:yyyy-MM-dd} – {p.d2:yyyy-MM-dd}");
                sb.AppendLine("field = datumové pole, ke kterému se období vztahuje:");
                foreach (var p in PeriodFields) sb.AppendLine($"{p.Id} | {p.Name}");
            }
            if (HasState)
            {
                sb.AppendLine("## Stav úkonů (fixed.state)");
                if (_prefix != "p31") sb.AppendLine("U této entity znamená: záznamy, které mají úkony v daném stavu.");
                foreach (var s in AiTotalsService.States) sb.AppendLine($"{s.Key} | {s.Value}");
            }
            if (HasTab)
            {
                sb.AppendLine("## Formát úkonů (fixed.tab)");
                foreach (var t in AiTotalsService.TabNames) sb.AppendLine($"\"{t.Key}\" | {t.Value}");
            }
            if (HasArchive)
            {
                sb.AppendLine("## Archiv (fixed.archive)");
                foreach (var a in ArchiveValues) sb.AppendLine($"{a} | {ArchiveNames[a]}");
            }
            return sb.ToString();
        }

        /// <summary>JSON schéma objektu fixed – jen filtry, které grid má; null = žádné.</summary>
        public System.Text.Json.Nodes.JsonObject BuildSchema()
        {
            if (!Any) return null;
            var props = new System.Text.Json.Nodes.JsonObject();
            if (HasPeriod)
                props["period"] = BL.AI.Core.AiSchema.Obj(new System.Text.Json.Nodes.JsonObject
                {
                    ["value"] = BL.AI.Core.AiSchema.IntEnum(new[] { 0, 1 }.Concat(Periods.Select(p => p.pid)).Distinct()),
                    ["field"] = BL.AI.Core.AiSchema.StrEnum(PeriodFields.Select(p => p.Id)),
                    ["d1"] = BL.AI.Core.AiSchema.Nullable(BL.AI.Core.AiSchema.Str()),
                    ["d2"] = BL.AI.Core.AiSchema.Nullable(BL.AI.Core.AiSchema.Str())
                });
            if (HasState) props["state"] = BL.AI.Core.AiSchema.IntEnum(AiTotalsService.States.Keys);
            if (HasTab) props["tab"] = BL.AI.Core.AiSchema.StrEnum(AiTotalsValidator.Tabs);
            if (HasArchive) props["archive"] = BL.AI.Core.AiSchema.IntEnum(ArchiveValues);
            return BL.AI.Core.AiSchema.Obj(props);
        }

        /// <summary>Nabídky pro formulář (null = filtr v gridu není).</summary>
        public object ToUi() => new
        {
            periods = HasPeriod ? new[] { new { id = 0, name = _f.tra("Bez omezení období") }, new { id = 1, name = _f.tra("Vlastní rozsah") } }
                .Concat(Periods.Select(p => new { id = p.pid, name = p.IsUserPeriod ? p.PeriodName : _f.tra(p.PeriodName) })).ToList() : null,
            periodFields = HasPeriod ? PeriodFields.Select(p => new { id = p.Id, name = _f.tra(p.Name) }).ToList() : null,
            states = HasState ? AiTotalsService.States.Select(p => new { id = p.Key, name = _f.tra(p.Value) }).ToList() : null,
            tabs = HasTab ? AiTotalsService.TabNames.Select(p => new { id = p.Key, name = _f.tra(p.Value) }).ToList() : null,
            archive = HasArchive ? ArchiveValues.Select(p => new { id = p, name = _f.tra(ArchiveNames[p]) }).ToList() : null
        };

        /// <summary>Datumová pole období podle entity – kopie nabídky z Views/myPeriod/selector.cshtml. Používá i nástroj agregace.</summary>
        public static List<(string Id, string Name)> FieldsOf(string prefix) => prefix switch
        {
            "p11" => new() { ("p11Date", "Datum docházky") },
            "b05" => new() { ("b05DateInsert", "Čas záznamu"), ("b05Date", "Datum") },
            "j02" => new() { ("p31Date", "Datum úkonu"), ("p91Date", "Datum vyúčtování"), ("p91DateSupply", "DUZP vyúčtování"), ("j02DateInsert", "Čas založení uživatele") },
            "p31" => new() { ("p31Date", "Datum úkonu"), ("p91Date", "Datum vyúčtování"), ("p91DateSupply", "DUZP vyúčtování"), ("p91DateBilled", "Úhrada vyúčtování"), ("p31Approved_When", "Čas schválení úkonu"), ("p31DateInsert", "Čas založení úkonu") },
            "p28" => new() { ("p31Date", "Datum úkonu"), ("p91Date", "Datum vyúčtování"), ("p91DateSupply", "DUZP vyúčtování"), ("p28DateInsert", "Čas založení kontaktu") },
            "p41" or "le1" or "le2" or "le3" or "le4" or "le5" => new() { ("p31Date", "Datum úkonu"), ("p41PlanFrom", "Plán zahájení"), ("p41PlanUntil", "Plán dokončení"), ("p91Date", "Datum vyúčtování"), ("p91DateSupply", "DUZP vyúčtování"), ("p41DateInsert", "Čas založení projektu") },
            "p56" => new() { ("p31Date", "Datum úkonu"), ("p56PlanFrom", "Plán zahájení úkolu"), ("p56PlanUntil", "Plán dokončení úkolu"), ("p91Date", "Datum vyúčtování"), ("p91DateSupply", "DUZP vyúčtování"), ("p56DateInsert", "Čas založení úkolu") },
            "o22" => new() { ("o22PlanFrom", "Začátek"), ("o22PlanUntil", "Konec"), ("o22DateInsert", "Čas založení termínu") },
            "p90" => new() { ("p90Date", "Datum zálohy"), ("p90DateMaturity", "Datum splatnosti"), ("p90DateInsert", "Čas založení zálohy") },
            "p91" => new() { ("p91Date", "Datum vyúčtování"), ("p91DateSupply", "Datum zdanitelného plnění"), ("p91DateMaturity", "Datum splatnosti"), ("p91DateInsert", "Čas vygenerování vyúčtování") },
            "p84" => new() { ("p84Date", "Datum upomínky"), ("p91DateMaturity", "Splatnost faktury"), ("p91DateSupply", "DUZP faktury") },
            "p78" => new() { ("p78Date", "Datum upomínky") },
            "o23" => new() { ("o23DateInsert", "Čas založení dokumentu") },
            "p39" or "p40" => new() { ("p39Date", "Datum úkonu"), ("p39DateCreate", "Datum generování") },
            "o43" => new() { ("o43DateMessage", "Datum zprávy"), ("o43DateInsert", "Datum importu") },
            "p49" or "fp1" or "fp2" or "fp3" => new() { ("p49Date", "Datum plánu") },
            _ => new()
        };
    }
}
