using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace UI
{
    //Překlad tiskových sestav (trdx) z jazyka originálu (x31Report.x31LangCode, výchozí cs) do jiného jazyka.
    //Slovník = soubor reports.lang.json: pole objektů {"cs":"Faktura","en":"Invoice","de":"Rechnung","sk":"Faktúra"}, jazyk bez překladu se vynechá.
    //Řádek říká, že texty znamenají totéž - hledá se ve sloupci jazyka originálu a výsledek se bere ze sloupce cílového jazyka, takže jeden slovník slouží všem směrům.
    //Společný slovník i slovník zákazníka mají stejný formát a jsou vzájemně zaměnitelné. Skládají se v pořadí priority:
    //  1. slovník zákazníka  {Factory.ReportFolder}\reports.lang.json  (složka sestav klienta; spravuje se v administraci: /ReportLang/Index)
    //  2. společný slovník   {RootUpload}\_distribution\trdx\reports.lang.json
    //Třídy v tomto souboru nemají závislost na BL ani na Telerik - dají se zkoušet samostatně.

    public class ReportLangRow
    {
        [System.Text.Json.Serialization.JsonPropertyName("cs")]
        public string Cs { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("en")]
        public string En { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("de")]
        public string De { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("sk")]
        public string Sk { get; set; }

        public string GetByLang(string lang)
        {
            switch (lang)
            {
                case "cs": return Cs;
                case "en": return En;
                case "de": return De;
                case "sk": return Sk;
                default: return null;
            }
        }

        public bool IsEmpty()
        {
            return ReportLang.Langs.All(p => ReportLang.NormalizeKey(GetByLang(p)).Length == 0);
        }
    }

    public class ReportLangDictionary
    {
        //pro každý jazyk: text -> řádek (přesně a bez ohledu na velikost písmen); při duplicitě platí první výskyt
        private readonly Dictionary<string, Dictionary<string, ReportLangRow>> _exact = new Dictionary<string, Dictionary<string, ReportLangRow>>();
        private readonly Dictionary<string, Dictionary<string, ReportLangRow>> _nocase = new Dictionary<string, Dictionary<string, ReportLangRow>>();

        public List<ReportLangRow> Rows { get; } = new List<ReportLangRow>();

        public ReportLangDictionary(IEnumerable<ReportLangRow> rows)
        {
            foreach (string lang in ReportLang.Langs)
            {
                _exact[lang] = new Dictionary<string, ReportLangRow>(StringComparer.Ordinal);
                _nocase[lang] = new Dictionary<string, ReportLangRow>(StringComparer.OrdinalIgnoreCase);
            }
            if (rows == null) return;
            foreach (var row in rows)
            {
                if (row == null || row.IsEmpty()) continue;
                Rows.Add(row);
                foreach (string lang in ReportLang.Langs)
                {
                    string key = ReportLang.NormalizeKey(row.GetByLang(lang));
                    if (key.Length == 0) continue;
                    if (!_exact[lang].ContainsKey(key)) _exact[lang].Add(key, row);
                    if (!_nocase[lang].ContainsKey(key)) _nocase[lang].Add(key, row);
                }
            }
        }

        //řádek, který má v jazyce lang text key
        public ReportLangRow Find(string lang, string key, bool ignoreCase)
        {
            Dictionary<string, ReportLangRow> d;
            if (!(ignoreCase ? _nocase : _exact).TryGetValue(lang, out d)) return null;
            ReportLangRow row;
            return d.TryGetValue(key, out row) ? row : null;
        }
    }

    public static class ReportLang
    {
        public const string FileName = "reports.lang.json";
        public const string DefaultLang = "cs";     //jazyk originálu sestavy, když ho šablona nemá vyplněný (x31LangCode je prázdný)
        public static readonly string[] Langs = new[] { "cs", "en", "de", "sk" };

        public static string GetLangName(string lang)
        {
            switch (lang)
            {
                case "cs": return "Česky";
                case "en": return "English";
                case "de": return "Deutsch";
                case "sk": return "Slovenčina";
                default: return lang;
            }
        }

        //jazyk originálu sestavy: prázdný nebo neznámý kód = čeština
        public static string GetSourceLang(string langCode)
        {
            langCode = (langCode ?? "").Trim().ToLower();
            return Langs.Contains(langCode) ? langCode : DefaultLang;
        }

        //jazyky nabídky překladu nad náhledem sestavy = všechny; první položku (kód "" = nepřekládat) si doplňuje view. Volba jazyka originálu sestavu nezmění.
        public static List<KeyValuePair<string, string>> GetTranslateMenu()
        {
            return Langs.Select(p => new KeyValuePair<string, string>(p, GetLangName(p))).ToList();
        }

        //čtení snese i ruční úpravy souboru: čárku za posledním řádkem a komentáře
        private static readonly System.Text.Json.JsonSerializerOptions _jsonRead = new System.Text.Json.JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip
        };
        //zápis: diakritika čitelně (ne \uXXXX)
        private static readonly System.Text.Json.JsonSerializerOptions _jsonWrite = new System.Text.Json.JsonSerializerOptions()
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        private class CacheItem
        {
            public DateTime Stamp;
            public long Length;
            public ReportLangDictionary Dict;
        }
        private static readonly ConcurrentDictionary<string, CacheItem> _cache = new ConcurrentDictionary<string, CacheItem>(StringComparer.OrdinalIgnoreCase);

        //slovník ze souboru; drží se v paměti a znovu se čte jen po změně souboru. Soubor neexistuje -> null. Neplatný json -> výjimka JsonException.
        public static ReportLangDictionary GetDictionary(string fullPath)
        {
            var fi = new System.IO.FileInfo(fullPath);
            if (!fi.Exists)
            {
                _cache.TryRemove(fullPath, out _);
                return null;
            }
            CacheItem c;
            if (_cache.TryGetValue(fullPath, out c) && c.Stamp == fi.LastWriteTimeUtc && c.Length == fi.Length)
            {
                return c.Dict;
            }
            c = new CacheItem() { Stamp = fi.LastWriteTimeUtc, Length = fi.Length, Dict = new ReportLangDictionary(ParseFile(System.IO.File.ReadAllText(fullPath))) };
            _cache[fullPath] = c;
            return c.Dict;
        }

        public static List<ReportLangRow> ParseFile(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return new List<ReportLangRow>();
            var rows = System.Text.Json.JsonSerializer.Deserialize<List<ReportLangRow>>(content, _jsonRead) ?? new List<ReportLangRow>();
            return rows.Where(p => p != null && !p.IsEmpty()).ToList();
        }

        //jeden výraz = jeden řádek souboru (dobře se čte, upravuje ručně a porovnává)
        public static string SerializeFile(IEnumerable<ReportLangRow> rows)
        {
            var lines = new List<string>();
            foreach (var row in rows)
            {
                if (row == null || row.IsEmpty()) continue;
                var sb = new StringBuilder("{");
                foreach (string lang in Langs) AppendProperty(sb, lang, row.GetByLang(lang));
                lines.Add(sb.Append('}').ToString());
            }
            return "[\r\n" + string.Join(",\r\n", lines) + (lines.Count > 0 ? "\r\n" : "") + "]\r\n";
        }

        private static void AppendProperty(StringBuilder sb, string name, string value)
        {
            value = NormalizeKey(value);
            if (value.Length == 0) return;
            if (sb.Length > 1) sb.Append(',');
            sb.Append('"').Append(name).Append("\":").Append(System.Text.Json.JsonSerializer.Serialize(value, _jsonWrite));
        }

        //klíč pro hledání: bez okrajových mezer, konce řádků sjednocené na \n
        public static string NormalizeKey(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\r\n", "\n").Replace('\r', '\n').Trim();
        }
    }

    public static class ReportTranslator
    {
        //otevírací značka elementu včetně atributů (hodnoty atributů v uvozovkách, takže znak > uvnitř hodnoty nevadí)
        private static readonly Regex _rxTag = new Regex("<(?<name>[A-Za-z_][\\w.\\-]*)(?<attrs>(?:\\s+[\\w.:\\-]+\\s*=\\s*\"[^\"]*\")+)\\s*/?>", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex _rxAttr = new Regex("(?<pre>\\s+(?<n>[\\w.:\\-]+)\\s*=\\s*\")(?<v>[^\"]*)\"", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        //části textu, které se nepřekládají: vložený výraz {…} a (jen u HtmlTextBox) html značka
        private static readonly Regex _rxExpr = new Regex("\\{[^{}]*\\}", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex _rxExprOrHtml = new Regex("\\{[^{}]*\\}|<[^<>]+>", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        //elementy, jejichž atribut Value není zobrazovaný text (podmínky, parametry, obrázky, čárové kódy) - ty se nesmí přeložit
        private static readonly HashSet<string> _noValueElements = new HashSet<string>(StringComparer.Ordinal)
        {
            "Filter", "Parameter", "ReportParameter", "SqlDataSourceParameter", "ObjectDataSourceParameter", "Binding", "PictureBox", "Barcode", "CheckBox", "Sorting", "Grouping"
        };

        //přeloží texty sestavy z jazyka sourceLang do targetLang; slovníky v pořadí priority (první má přednost), null slovníky se přeskočí
        public static string Translate(string reportXml, string sourceLang, string targetLang, params ReportLangDictionary[] dictionaries)
        {
            if (string.IsNullOrEmpty(reportXml) || string.IsNullOrEmpty(targetLang) || dictionaries == null) return reportXml;
            sourceLang = ReportLang.GetSourceLang(sourceLang);
            if (targetLang == sourceLang) return reportXml;
            var dicts = dictionaries.Where(p => p != null).ToArray();
            if (dicts.Length == 0) return reportXml;

            //slovenský originál: výraz, který slovník ve slovenštině nemá, se hledá i mezi českými - řádky bez slovenštiny jsou většinou výrazy v obou jazycích stejné (Aktivita, Projekt, Banka)
            string[] lookupLangs = sourceLang == "sk" ? new[] { "sk", "cs" } : new[] { sourceLang };

            return Walk(reportXml, (text) => Lookup(text, lookupLangs, targetLang, dicts));
        }

        //projde zobrazované texty sestavy; translate vrací překlad nebo null (= ponechat). Nepřeložené části zůstávají v xml beze změny znak po znaku.
        private static string Walk(string reportXml, Func<string, string> translate)
        {
            return _rxTag.Replace(reportXml, (mTag) =>
            {
                string attrs = mTag.Groups["attrs"].Value;
                if (attrs.IndexOf("Value", StringComparison.Ordinal) < 0 && attrs.IndexOf("Text", StringComparison.Ordinal) < 0) return mTag.Value;

                string element = mTag.Groups["name"].Value;
                bool isHtml = element == "HtmlTextBox";
                bool changed = false;

                string newAttrs = _rxAttr.Replace(attrs, (mAttr) =>
                {
                    string name = mAttr.Groups["n"].Value;
                    if (name == "Value")
                    {
                        if (_noValueElements.Contains(element)) return mAttr.Value;
                    }
                    else if (name != "Text")
                    {
                        return mAttr.Value;
                    }

                    string raw = mAttr.Groups["v"].Value;
                    if (raw.Length == 0) return mAttr.Value;

                    string translated = TranslateValue(System.Net.WebUtility.HtmlDecode(raw), isHtml, translate);
                    if (translated == null) return mAttr.Value;

                    changed = true;
                    return mAttr.Groups["pre"].Value + EscapeAttribute(translated) + "\"";
                });

                if (!changed) return mTag.Value;
                return mTag.Value.Substring(0, mTag.Groups["attrs"].Index - mTag.Index) + newAttrs + mTag.Value.Substring(mTag.Groups["attrs"].Index - mTag.Index + attrs.Length);
            });
        }

        //hodnota atributu: nejdřív jako celek, potom po částech mezi vloženými výrazy {…} (a html značkami). Vrací null, pokud se nic nepřeložilo.
        private static string TranslateValue(string value, bool isHtml, Func<string, string> translate)
        {
            string whole = translate(value);
            if (whole != null) return KeepExpressions(value, whole);

            if (value.TrimStart().StartsWith("=")) return null;    //výraz (= Fields.xxx, = IIf(...)) - textové konstanty uvnitř výrazu se nepřekládají, mohou to být porovnávané hodnoty

            Regex rx = isHtml ? _rxExprOrHtml : _rxExpr;
            var matches = rx.Matches(value);
            if (matches.Count == 0) return null;

            var sb = new StringBuilder();
            bool changed = false;
            int pos = 0;
            foreach (Match m in matches)
            {
                changed |= AppendLiteral(sb, value.Substring(pos, m.Index - pos), translate);
                sb.Append(m.Value);
                pos = m.Index + m.Length;
            }
            changed |= AppendLiteral(sb, value.Substring(pos), translate);

            return changed ? sb.ToString() : null;
        }

        //Překlad celého textu z řádku slovníku může mít vložené výrazy zapsané jinak než sestava (velikost písmen v {Fields.xxx}).
        //Když si výrazy v originálu a v překladu odpovídají pořadím, vrátí se do překladu přesně ty ze sestavy.
        private static string KeepExpressions(string value, string translated)
        {
            var orig = _rxExpr.Matches(value);
            if (orig.Count == 0) return translated;
            var trans = _rxExpr.Matches(translated);
            if (trans.Count != orig.Count) return translated;
            for (int i = 0; i < orig.Count; i++)
            {
                if (!string.Equals(orig[i].Value, trans[i].Value, StringComparison.OrdinalIgnoreCase)) return translated;
            }
            var sb = new StringBuilder();
            int pos = 0;
            for (int i = 0; i < trans.Count; i++)
            {
                sb.Append(translated, pos, trans[i].Index - pos).Append(orig[i].Value);
                pos = trans[i].Index + trans[i].Length;
            }
            return sb.Append(translated, pos, translated.Length - pos).ToString();
        }

        private static bool AppendLiteral(StringBuilder sb, string literal, Func<string, string> translate)
        {
            if (literal.Length == 0) return false;
            string s = literal.Any(char.IsLetter) ? translate(literal) : null;
            sb.Append(s ?? literal);
            return s != null;
        }

        //překlad jednoho textu. Okrajové mezery a koncová dvojtečka se zachovávají; prázdný překlad = nepřeloženo.
        private static string Lookup(string text, string[] lookupLangs, string lang, ReportLangDictionary[] dicts)
        {
            string core = ReportLang.NormalizeKey(text);
            if (core.Length == 0) return null;

            string norm = text.Replace("\r\n", "\n").Replace('\r', '\n');
            int start = norm.IndexOf(core, StringComparison.Ordinal);
            string lead = norm.Substring(0, start);
            string trail = norm.Substring(start + core.Length);

            bool colon;
            var row = FindRow(core, lookupLangs, dicts, (r) => !string.IsNullOrWhiteSpace(r.GetByLang(lang)), out colon);
            if (row == null) return null;
            if (!colon) return lead + row.GetByLang(lang) + trail;

            string coreNoColon = core.Substring(0, core.Length - 1).TrimEnd();
            return lead + row.GetByLang(lang) + core.Substring(coreNoColon.Length) + trail;     //dvojtečka včetně případné mezery před ní
        }

        //řádek slovníku pro text (bez okrajových mezer): přesně -> bez koncové dvojtečky -> totéž bez ohledu na velikost písmen;
        //v každém kroku jazyky hledání v daném pořadí a v nich slovníky v pořadí priority.
        //accept: řádek se bere jen když má překlad do cílového jazyka. colon = shoda až po odebrání koncové dvojtečky.
        private static ReportLangRow FindRow(string core, string[] lookupLangs, ReportLangDictionary[] dicts, Func<ReportLangRow, bool> accept, out bool colon)
        {
            colon = false;
            string coreNoColon = core.EndsWith(":") ? core.Substring(0, core.Length - 1).TrimEnd() : null;

            foreach (bool ignoreCase in new[] { false, true })
            {
                foreach (string key in new[] { core, coreNoColon })
                {
                    if (string.IsNullOrEmpty(key)) continue;
                    foreach (string lang in lookupLangs)
                    {
                        foreach (var d in dicts)
                        {
                            var row = d.Find(lang, key, ignoreCase);
                            if (row != null && accept(row)) { colon = !object.ReferenceEquals(key, core); return row; }
                        }
                    }
                }
            }
            return null;
        }

        //zápis hodnoty zpět do atributu: konec řádku a tabulátor jako číselné entity (jinak by je xml parser změnil na mezeru)
        private static string EscapeAttribute(string s)
        {
            var sb = new StringBuilder(s.Length + 16);
            foreach (char ch in s)
            {
                switch (ch)
                {
                    case '&': sb.Append("&amp;"); break;
                    case '<': sb.Append("&lt;"); break;
                    case '>': sb.Append("&gt;"); break;
                    case '"': sb.Append("&quot;"); break;
                    case '\r': break;
                    case '\n': sb.Append("&#xD;&#xA;"); break;     //konec řádku vždy jako CRLF - stejně ho zapisuje návrhář sestav
                    case '\t': sb.Append("&#x9;"); break;
                    default: sb.Append(ch); break;
                }
            }
            return sb.ToString();
        }
    }
}
