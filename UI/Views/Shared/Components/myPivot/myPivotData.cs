using System.Data;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace UI.Views.Shared.Components.myPivot
{
    /// <summary>
    /// Převod agregovaných dat (DataTable z gridBL.GetGroupByList) do kompaktního sloupcového JSON pro MARKTIME pivot (mypivot.js).
    /// Textové, datumové a ano/ne hodnoty jdou přes číselník (každá hodnota jen jednou, řádky nesou index), čísla jako pole čísel.
    /// {
    ///   "n": počet řádků,
    ///   "fields": [{ "name": hlavička, "type": "string|number|date|bool", "measure": true|false, "decimals": 2 }],
    ///   "data": [ { "d": [hodnoty], "i": [indexy, -1 = prázdné] } | [čísla nebo null] ],
    ///   "count": [počet záznamů ve skupině] | null
    /// }
    /// </summary>
    public static class myPivotData
    {
        public static string ToJson(DataTable dt, List<BO.TheGridColumn> cols, string countColumn = null)
        {
            using var ms = new MemoryStream();
            using (var w = new Utf8JsonWriter(ms, new JsonWriterOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }))
            {
                int n = dt.Rows.Count;
                w.WriteStartObject();
                w.WriteNumber("n", n);

                // jedinečné názvy polí (WebDataRocks i pivot je identifikují hlavičkou)
                var names = new List<string>();
                foreach (var c in cols)
                {
                    string name = string.IsNullOrWhiteSpace(c.Header) ? c.UniqueName : c.Header.Trim();
                    string unique = name; int k = 2;
                    while (names.Contains(unique, StringComparer.OrdinalIgnoreCase)) unique = $"{name} ({k++})";
                    names.Add(unique);
                }

                w.WriteStartArray("fields");
                for (int x = 0; x < cols.Count; x++)
                {
                    var c = cols[x];
                    w.WriteStartObject();
                    w.WriteString("name", names[x]);
                    w.WriteString("type", TypeOf(c.FieldType));
                    w.WriteBoolean("measure", c.IsShowTotals);
                    w.WriteNumber("decimals", Decimals(c.FieldType));
                    w.WriteEndObject();
                }
                w.WriteEndArray();

                w.WriteStartArray("data");
                foreach (var c in cols)
                {
                    int ci = dt.Columns.IndexOf(c.UniqueName);
                    string type = TypeOf(c.FieldType);
                    if (type == "number")
                    {
                        w.WriteStartArray();
                        for (int r = 0; r < n; r++)
                        {
                            object o = ci < 0 ? null : dt.Rows[r][ci];
                            if (o == null || o == DBNull.Value) w.WriteNullValue();
                            else w.WriteRawValue(Num(o), true);
                        }
                        w.WriteEndArray();
                    }
                    else
                    {
                        var dict = new Dictionary<string, int>();
                        var idx = new int[n];
                        for (int r = 0; r < n; r++)
                        {
                            object o = ci < 0 ? null : dt.Rows[r][ci];
                            string s = Text(o, type);
                            if (s == null) { idx[r] = -1; continue; }
                            if (!dict.TryGetValue(s, out int i)) { i = dict.Count; dict.Add(s, i); }
                            idx[r] = i;
                        }
                        w.WriteStartObject();
                        w.WriteStartArray("d");
                        foreach (var s in dict.Keys) w.WriteStringValue(s);   // Dictionary zachovává pořadí vložení
                        w.WriteEndArray();
                        w.WriteStartArray("i");
                        for (int r = 0; r < n; r++) w.WriteNumberValue(idx[r]);
                        w.WriteEndArray();
                        w.WriteEndObject();
                    }
                }
                w.WriteEndArray();

                int cc = countColumn == null ? -1 : dt.Columns.IndexOf(countColumn);
                if (cc < 0) w.WriteNull("count");
                else
                {
                    w.WriteStartArray("count");
                    for (int r = 0; r < n; r++)
                    {
                        object o = dt.Rows[r][cc];
                        w.WriteNumberValue(o == DBNull.Value ? 0 : Convert.ToInt64(o));
                    }
                    w.WriteEndArray();
                }
                w.WriteEndObject();
            }
            return Encoding.UTF8.GetString(ms.ToArray());
        }

        private static string TypeOf(string fieldType) => fieldType switch
        {
            "num" or "num0" or "num3" or "num4" or "num5" or "int" => "number",
            "date" or "datetime" or "datetimesec" => "date",
            "bool" => "bool",
            _ => "string"
        };

        private static int Decimals(string fieldType) => fieldType switch
        {
            "num0" or "int" => 0,
            "num3" => 3,
            "num4" => 4,
            "num5" => 5,
            _ => 2
        };

        private static string Num(object o)
        {
            switch (o)
            {
                case decimal d: return d.ToString("0.##########", CultureInfo.InvariantCulture);
                case double db: return double.IsFinite(db) ? db.ToString("0.##########", CultureInfo.InvariantCulture) : "0";
                case float f: return ((double)f).ToString("0.##########", CultureInfo.InvariantCulture);
                case bool b: return b ? "1" : "0";
                default:
                    return double.TryParse(Convert.ToString(o, CultureInfo.InvariantCulture), NumberStyles.Float, CultureInfo.InvariantCulture, out var v)
                        ? v.ToString("0.##########", CultureInfo.InvariantCulture) : "0";
            }
        }

        private static string Text(object o, string type)
        {
            if (o == null || o == DBNull.Value) return null;
            switch (type)
            {
                case "date":
                    if (o is DateTime dtm) return dtm.TimeOfDay == TimeSpan.Zero ? dtm.ToString("yyyy-MM-dd") : dtm.ToString("yyyy-MM-ddTHH:mm");
                    return o.ToString();
                case "bool":
                    if (o is bool b) return b ? "1" : "0";
                    return Convert.ToInt32(o) != 0 ? "1" : "0";
                default:
                    string s = o.ToString().Trim();
                    if (s.Contains('<') && s.Contains('>')) s = BO.Code.Bas.Html2Text(s);
                    return s == "" ? null : s;
            }
        }
    }
}
