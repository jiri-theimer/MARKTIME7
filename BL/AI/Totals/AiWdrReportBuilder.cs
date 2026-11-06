using System.Text.Json.Nodes;

namespace BL.AI.Totals
{
    /// <summary>
    /// Sestaví JSON report WebDataRocks (j79WebDataRocksReport) z návrhu.
    /// Pole WebDataRocks se jmenují podle hlaviček CSV, které generuje p31totalsController (col.Header z ParseTheGridColumns),
    /// proto se hlavičky berou ze stejné metody.
    /// </summary>
    public static class AiWdrReportBuilder
    {
        public static string Build(BO.AI.AiTotalsProposal p, Dictionary<string, string> headerByKey, string csvUrl)
        {
            string H(string key) => headerByKey.TryGetValue(key, out var h) ? h : key;

            var rows = new JsonArray();
            foreach (var k in p.Rows ?? new()) rows.Add(new JsonObject { ["uniqueName"] = H(k) });

            var columns = new JsonArray();
            foreach (var k in p.Columns ?? new()) columns.Add(new JsonObject { ["uniqueName"] = H(k) });
            columns.Add(new JsonObject { ["uniqueName"] = "Measures" });

            var measures = new JsonArray();
            foreach (var mm in p.Measures ?? new())
                measures.Add(new JsonObject { ["uniqueName"] = H(mm.Key), ["aggregation"] = (mm.Aggregation ?? "sum").ToLowerInvariant() });

            var report = new JsonObject
            {
                ["dataSource"] = new JsonObject { ["filename"] = csvUrl, ["delimiter"] = ";" },
                ["slice"] = new JsonObject { ["rows"] = rows, ["columns"] = columns, ["measures"] = measures },
                ["options"] = new JsonObject
                {
                    ["grid"] = new JsonObject { ["type"] = "compact", ["showTotals"] = "on", ["showGrandTotals"] = "on" }
                },
                ["formats"] = new JsonArray
                {
                    new JsonObject { ["name"] = "", ["decimalSeparator"] = ",", ["thousandsSeparator"] = " ", ["decimalPlaces"] = 2 }
                }
            };
            return report.ToJsonString(_opt);
        }

        // čitelná diakritika v uloženém JSON (místo ř …)
        private static readonly System.Text.Json.JsonSerializerOptions _opt = new()
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            TypeInfoResolver = new System.Text.Json.Serialization.Metadata.DefaultJsonTypeInfoResolver()
        };
    }
}
