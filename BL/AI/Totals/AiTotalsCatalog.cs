using System.Text;
using System.Text.Json;

namespace BL.AI.Totals
{
    /// <summary>
    /// Katalog sloupců nástroje SOUČTY pro přihlášeného uživatele.
    /// Kopíruje logiku palety TheGridDesigneru volaného z /p31totals (relace úkonu, IHRC/VYSL, j04GridColumnsExclude,
    /// uživatelská pole, IsNotUseP31TOTALS) a doplňuje AI popisy/vyřazení z Data/p31-hints.json.
    /// Model vidí místo dlouhých klíčů krátká ID (m01…, d01…) – šetří tokeny a zmenšuje JSON schéma.
    /// </summary>
    public class AiTotalsCatalog
    {
        public List<CatalogEntry> Items { get; } = new();
        private readonly Dictionary<string, CatalogEntry> _byKey = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, CatalogEntry> _byId = new(StringComparer.OrdinalIgnoreCase);

        public IEnumerable<CatalogEntry> Measures => Items.Where(p => p.Role == "measure");
        public IEnumerable<CatalogEntry> Dimensions => Items.Where(p => p.Role == "dimension");

        public CatalogEntry ByKey(string key) => key != null && _byKey.TryGetValue(key, out var e) ? e : null;
        public CatalogEntry ById(string id) => id != null && _byId.TryGetValue(id, out var e) ? e : null;

        // z relací projektových úrovní (le1–le4) nabízet jen tato pole
        private static readonly HashSet<string> _levelFields = new() { "NazevProjektu", "p41Code", "TypProjektu", "KlientProjektu" };

        public class CatalogEntry
        {
            public string Id { get; set; }
            public string Key { get; set; }
            public string Header { get; set; }
            public string Role { get; set; }
            public string Group { get; set; }
            public string Type { get; set; }
            public string Hint { get; set; }
        }

        private class HintRec { public string hint { get; set; } public bool exclude { get; set; } }

        public static AiTotalsCatalog Build(BL.Factory f, BL.TheColumnsProvider cp)
        {
            var hints = JsonSerializer.Deserialize<Dictionary<string, HintRec>>(BL.AI.Core.AiPromptStore.Get("p31-hints.json"));
            var cat = new AiTotalsCatalog();
            var u = f.CurrentUser;

            // relace – stejně jako TheGridDesignerController.Index_RefreshState
            var rels = f.EProvider.getApplicableRelations("p31", f);
            if (!u.IsRatesAccess) rels = rels.Where(p => p.RelName != "p31_p91").ToList();
            rels.Insert(0, new BO.EntityRelation { TableName = "p31Worksheet", RelName = "a", AliasSingular = "Úkon" });

            IEnumerable<BO.TheGridColumn> cols = cp.AllColumns();
            if (!u.IsRatesAccess) cols = cols.Where(p => !p.IHRC);
            if (!u.IsVysledovkyAccess) cols = cols.Where(p => !p.VYSL);
            var all = cols.ToList();
            all.AddRange(new BL.ffColumnsProvider(f, "p31").getColumns());   // uživatelská pole + role
            if (!string.IsNullOrEmpty(u.j04GridColumnsExclude))
            {
                var exc = u.j04GridColumnsExclude.Split(",").ToHashSet();
                all = all.Where(p => !exc.Contains(p.UniqueName)).ToList();
            }
            all = all.Where(p => !p.IsNotUseP31TOTALS && !p.IsTimestamp).ToList();

            int m = 0, d = 0;
            foreach (var rel in rels)
            {
                string alias = rel.AliasSingular;
                bool isLevel = rel.TableName.StartsWith("le");
                if (isLevel && int.TryParse(rel.TableName.Substring(2), out int li)) alias = f.getP07Level(li, true) ?? alias;

                foreach (var c in all.Where(p => p.Entity == rel.TableName))
                {
                    if (isLevel && !_levelFields.Contains(c.Field)) continue;
                    bool numeric = c.FieldType != null && (c.FieldType.StartsWith("num") || c.FieldType == "int");
                    bool isMeasure = rel.RelName == "a" && c.IsShowTotals && numeric;
                    if (!isMeasure && numeric && rel.RelName != "a") continue;          // číselné atributy jiných entit
                    if (!isMeasure && (c.FieldType == "time" || c.FieldType == "datetime")) continue;

                    string key = $"{rel.RelName}__{c.Entity}__{c.Field}";
                    hints.TryGetValue(key, out var h);
                    if (h != null && h.exclude) continue;
                    if (cat._byKey.ContainsKey(key)) continue;   // duplicitní Field v provideru – ponechat první výskyt

                    string header = c.Header == "L5" ? f.getP07Level(5, true) : c.Header;
                    if (rel.RelName != "a" && !c.NotShowRelInHeader) header = $"{header} [{alias}]";

                    var e = new CatalogEntry
                    {
                        Id = isMeasure ? $"m{++m:00}" : $"d{++d:00}",
                        Key = key,
                        Header = header,
                        Role = isMeasure ? "measure" : "dimension",
                        Group = rel.RelName == "a" ? (c.DesignerGroup ?? "Úkon") : alias,
                        Type = c.FieldType,
                        Hint = h?.hint ?? c.Tooltip
                    };
                    cat.Items.Add(e);
                    cat._byKey[key] = e;
                    cat._byId[e.Id] = e;
                }
            }
            return cat;
        }

        /// <summary>Text katalogu pro system prompt.</summary>
        public string ToPromptText()
        {
            var sb = new StringBuilder();
            sb.AppendLine("# KATALOG – MĚŘÍTKA (id | název | skupina | popis)");
            foreach (var e in Measures) sb.AppendLine($"{e.Id} | {e.Header} | {e.Group} | {e.Hint}");
            sb.AppendLine();
            sb.AppendLine("# KATALOG – DIMENZE (id | název | skupina | popis)");
            foreach (var e in Dimensions) sb.AppendLine($"{e.Id} | {e.Header} | {e.Group} | {e.Hint}");
            return sb.ToString();
        }

        /// <summary>Katalog pro UI formulář (klíč → název).</summary>
        public List<BO.AI.AiCatalogItem> ToUiList() => Items.Select(e => new BO.AI.AiCatalogItem
        {
            Key = e.Key, Header = e.Header, Role = e.Role, Group = e.Group, Type = e.Type, Hint = e.Hint
        }).ToList();
    }
}
