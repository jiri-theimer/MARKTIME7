using System.Text;
using System.Text.Json;

namespace BL.AI.Core
{
    /// <summary>
    /// Katalog sloupců entity pro AI agenty – stejná paleta jako TheGridDesignerController.Index_RefreshState:
    /// primární tabulka + relace (EProvider.getApplicableRelations), oprávnění IHRC/VYSL, j04GridColumnsExclude, uživatelská pole.
    /// Model vidí místo dlouhých klíčů krátká ID (m01…, d01…, c001…) – šetří tokeny a zmenšuje JSON schéma.
    /// Popisy sloupců z volitelného embedded souboru "{prefix}-hints.json".
    /// </summary>
    public class AiColumnCatalog
    {
        /// <summary>Totals = Součty (měřítka/dimenze, jen p31 sčítané hodnoty), Grid = návrhář sloupců tabulky (všechny sloupce).</summary>
        public enum CatalogMode { Totals, Grid }

        public string Prefix { get; private set; }
        public CatalogMode Mode { get; private set; }
        public List<CatalogEntry> Items { get; } = new();
        private readonly Dictionary<string, CatalogEntry> _byKey = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, CatalogEntry> _byId = new(StringComparer.OrdinalIgnoreCase);

        public IEnumerable<CatalogEntry> Measures => Items.Where(p => p.Role == "measure");
        public IEnumerable<CatalogEntry> Dimensions => Items.Where(p => p.Role == "dimension");

        public CatalogEntry ByKey(string key) => key != null && _byKey.TryGetValue(key, out var e) ? e : null;
        public CatalogEntry ById(string id) => id != null && _byId.TryGetValue(id, out var e) ? e : null;

        // z relací projektových úrovní (le1–le4) nabízet jen tato pole (jinak by se celá paleta projektu opakovala 4×)
        private static readonly HashSet<string> _levelFields = new() { "NazevProjektu", "p41Code", "TypProjektu", "KlientProjektu" };

        public class CatalogEntry
        {
            public string Id { get; set; }
            public string Key { get; set; }
            public string Header { get; set; }
            /// <summary>Totals: measure | dimension; Grid: column</summary>
            public string Role { get; set; }
            public string Group { get; set; }
            public string Type { get; set; }
            public string Hint { get; set; }
            /// <summary>Typ sloupcového filtru gridu: string | number | date | bool; null = podle sloupce nelze filtrovat.</summary>
            public string FilterType { get; set; }
        }

        /// <summary>Typ sloupcového filtru podle FieldType – stejně jako TheGridColumn.NormalizedTypeName a thegrid.js (normalize_coltype_name).</summary>
        public static string FilterTypeOf(string fieldType) => fieldType switch
        {
            "string" => "string",
            "bool" => "bool",
            "num" or "num0" or "num3" or "num4" or "num5" or "int" => "number",
            "date" or "datetime" or "datetimesec" => "date",
            _ => null      // time, filesize… – sloupcový filtr je nenabízí
        };

        private class HintRec { public string hint { get; set; } public bool exclude { get; set; } }

        public static AiColumnCatalog Build(BL.Factory f, BL.TheColumnsProvider cp, string prefix, CatalogMode mode)
        {
            var cat = new AiColumnCatalog { Prefix = prefix, Mode = mode };
            var hints = LoadHints(prefix);
            var u = f.CurrentUser;

            // relace – stejně jako TheGridDesignerController.Index_RefreshState
            var ce = f.EProvider.ByPrefix(prefix);
            var rels = f.EProvider.getApplicableRelations(prefix, f);
            if (!u.IsRatesAccess && prefix == "p31") rels = rels.Where(p => p.RelName != "p31_p91").ToList();
            rels.Insert(0, new BO.EntityRelation { TableName = ce.TableName, RelName = "a", AliasSingular = ce.AliasSingular });

            IEnumerable<BO.TheGridColumn> cols = cp.AllColumns();
            if (!u.IsRatesAccess) cols = cols.Where(p => !p.IHRC);
            if (!u.IsVysledovkyAccess) cols = cols.Where(p => !p.VYSL);
            var all = cols.ToList();
            all.AddRange(new BL.ffColumnsProvider(f, BO.Code.Entity.GetPrefixDb(prefix)).getColumns());   // uživatelská pole + role
            if (!string.IsNullOrEmpty(u.j04GridColumnsExclude))
            {
                var exc = u.j04GridColumnsExclude.Split(",").ToHashSet();
                all = all.Where(p => !exc.Contains(p.UniqueName)).ToList();
            }
            if (mode == CatalogMode.Totals) all = all.Where(p => !p.IsNotUseP31TOTALS && !p.IsTimestamp).ToList();

            int m = 0, d = 0, c = 0;
            foreach (var rel in rels)
            {
                string alias = rel.AliasSingular;
                bool isLevel = rel.TableName.StartsWith("le");
                if (isLevel && int.TryParse(rel.TableName.Substring(2), out int li)) alias = f.getP07Level(li, true) ?? alias;

                foreach (var col in all.Where(p => p.Entity == rel.TableName))
                {
                    if (isLevel && !_levelFields.Contains(col.Field)) continue;
                    bool numeric = col.FieldType != null && (col.FieldType.StartsWith("num") || col.FieldType == "int");
                    string role;
                    if (mode == CatalogMode.Totals)
                    {
                        bool isMeasure = rel.RelName == "a" && col.IsShowTotals && numeric;
                        if (!isMeasure && numeric && rel.RelName != "a") continue;          // číselné atributy jiných entit
                        if (!isMeasure && (col.FieldType == "time" || col.FieldType == "datetime")) continue;
                        role = isMeasure ? "measure" : "dimension";
                    }
                    else role = "column";

                    string key = $"{rel.RelName}__{col.Entity}__{col.Field}";
                    hints.TryGetValue(key, out var h);
                    if (h != null && h.exclude && mode == CatalogMode.Totals) continue;   // vyřazení platí jen pro Součty
                    if (cat._byKey.ContainsKey(key)) continue;   // duplicitní Field v provideru – ponechat první výskyt

                    string header = col.Header == "L5" ? f.getP07Level(5, true) : col.Header;
                    if (rel.RelName != "a" && !col.NotShowRelInHeader) header = $"{header} [{alias}]";

                    var e = new CatalogEntry
                    {
                        Id = role == "measure" ? $"m{++m:00}" : role == "dimension" ? $"d{++d:00}" : $"c{++c:000}",
                        Key = key,
                        Header = header,
                        Role = role,
                        Group = rel.RelName == "a" ? (col.DesignerGroup ?? alias) : alias,
                        Type = col.FieldType,
                        Hint = h?.hint ?? col.Tooltip,
                        FilterType = col.IsFilterable ? FilterTypeOf(col.FieldType) : null
                    };
                    cat.Items.Add(e);
                    cat._byKey[key] = e;
                    cat._byId[e.Id] = e;
                }
            }
            return cat;
        }

        private static Dictionary<string, HintRec> LoadHints(string prefix)
        {
            var json = AiPromptStore.TryGet($"{prefix}-hints.json");
            if (string.IsNullOrEmpty(json)) return new();
            return JsonSerializer.Deserialize<Dictionary<string, HintRec>>(json) ?? new();
        }

        /// <summary>Text katalogu pro system prompt.</summary>
        public string ToPromptText()
        {
            var sb = new StringBuilder();
            if (Mode == CatalogMode.Totals)
            {
                sb.AppendLine("# KATALOG – MĚŘÍTKA (id | název | skupina | popis)");
                foreach (var e in Measures) sb.AppendLine($"{e.Id} | {e.Header} | {e.Group} | {e.Hint}");
                sb.AppendLine();
                sb.AppendLine("# KATALOG – DIMENZE (id | název | skupina | popis)");
                foreach (var e in Dimensions) sb.AppendLine($"{e.Id} | {e.Header} | {e.Group} | {e.Hint}");
            }
            else
            {
                sb.AppendLine("# KATALOG SLOUPCŮ (id | název | skupina | typ filtru | popis)");
                sb.AppendLine("typ filtru: string = text, number = číslo, date = datum, bool = ano/ne, - = podle sloupce nelze filtrovat");
                foreach (var e in Items) sb.AppendLine($"{e.Id} | {e.Header} | {e.Group} | {e.FilterType ?? "-"} | {e.Hint}");
            }
            return sb.ToString();
        }

        /// <summary>Katalog pro UI formulář (klíč → název).</summary>
        public List<BO.AI.AiCatalogItem> ToUiList() => Items.Select(e => new BO.AI.AiCatalogItem
        {
            Key = e.Key, Header = e.Header, Role = e.Role, Group = e.Group, Type = e.Type, Hint = e.Hint, Filter = e.FilterType
        }).ToList();
    }
}
