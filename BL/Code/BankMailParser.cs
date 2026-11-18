using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace BL.Code
{
    /// <summary>Jeden pohyb vyčtený z notifikačního e-mailu banky.</summary>
    public class BankMailMove
    {
        public double Amount { get; set; }          //vždy kladná hodnota
        public int Direction { get; set; }          //1 = příchozí, -1 = odchozí, 0 = nelze určit
        public string VS { get; set; }              //variabilní symbol bez úvodních nul; null = v e-mailu není
        public string Account { get; set; }         //účet protistrany
        public List<string> Codes { get; set; } = new List<string>();   //e-mail bez popisku VS: čísla z textu, která mohou být číslem dokladu
    }

    /// <summary>
    /// Obecné čtení notifikačních e-mailů bank - bez šablon pro jednotlivé banky.
    /// V textu hledá částky, variabilní symboly, účty a směr platby. Ověřeno na e-mailech ČSOB CEB, Raiffeisenbank a Fio (oba formáty).
    /// </summary>
    public static class BankMailParser
    {
        private class Amt
        {
            public double Value; public string Sign; public int Pos; public int End; public bool IsBalance;
        }
        private class Acc
        {
            public string Value; public int Pos; public bool IsCounterparty;
        }

        private static readonly RegexOptions _ro = RegexOptions.CultureInvariant;
        //částka se dvěma desetinnými místy: 2 118,00 | 203.522,00 | 1,234.56 | +851,20
        private static readonly Regex _rxAmt = new Regex(@"(?<![\d.,])([+\-]?) ?(\d{1,3}(?:[ .,]\d{3})+|\d+)([.,]\d{2})(?!\d|[.,]\d)", _ro);
        //částka bez desetinných míst, za kterou stojí měna: 12 100 Kč
        private static readonly Regex _rxAmt2 = new Regex(@"(?<![\d.,])([+\-]?) ?(\d{1,3}(?:[ .]\d{3})+|\d+) ?(?:czk|kc|eur|usd)\b", _ro);
        private static readonly Regex _rxVS = new Regex(@"(?:variabiln\w*\s+symbol|\bvs)\s*[:\-]?\s*(\d{1,10})(?!\d)", _ro);
        private static readonly Regex _rxAcc = new Regex(@"(?<![\d/])(?:\d{1,6}-)?\d{2,10}/\d{4}(?!\d)", _ro);
        private static readonly Regex _rxInt = new Regex(@"(?<![\d.,/\-])\d{3,10}(?![\d/]|[.,]\d)", _ro);
        private static readonly Regex _rxBalance = new Regex(@"(?:zustat|balance|disponibil)[^0-9]*$", _ro);
        private static readonly Regex _rxCounterparty = new Regex(@"(?:protistran|protiuc|z uctu|platce)[^0-9]*$", _ro);
        private static readonly Regex _rxHtmlDrop = new Regex(@"<(style|script|head)\b.*?</\1>|<!--.*?-->", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);
        private static readonly Regex _rxHtmlBreak = new Regex(@"<(br|/p|/td|/tr|/div|/li|/h\d)\b[^>]*>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        private static readonly Regex _rxHtmlTag = new Regex(@"<[^>]+>", _ro);

        //slova bez diakritiky a malými písmeny; hledají se v předmětu i v textu
        private static readonly string[] _wordsIn = { "prijem", "prichozi", "pripsan", "zvysil", "kredit", "incoming", "credited", "received" };
        private static readonly string[] _wordsOut = { "vydej", "odchozi", "odepsan", "snizil", "debet", "outgoing", "debited" };

        /// <summary>Text e-mailu: přednost má textová část, jinak se převede HTML.</summary>
        public static string GetText(string strBodyText, string strBodyHtml)
        {
            if (!string.IsNullOrWhiteSpace(strBodyText)) return strBodyText;
            if (string.IsNullOrWhiteSpace(strBodyHtml)) return "";
            string s = _rxHtmlDrop.Replace(strBodyHtml, " ");
            s = _rxHtmlBreak.Replace(s, "\n");
            s = _rxHtmlTag.Replace(s, " ");
            return System.Net.WebUtility.HtmlDecode(s);
        }

        /// <summary>Pohyby v e-mailu. Prázdný seznam = e-mail neobsahuje žádnou částku (není to oznámení o platbě).</summary>
        public static List<BankMailMove> Parse(string strSubject, string strBody)
        {
            var ret = new List<BankMailMove>();
            string f = Fold(Normalize((strSubject ?? "") + "\n" + (strBody ?? "")));

            var amounts = new List<Amt>();
            CollectAmounts(f, _rxAmt, true, amounts);
            CollectAmounts(f, _rxAmt2, false, amounts);
            amounts = amounts.OrderBy(p => p.Pos).ToList();
            var pay = amounts.Where(p => !p.IsBalance).ToList();    //zůstatek účtu není platba
            if (pay.Count == 0) return ret;

            var accounts = new List<Acc>();
            foreach (Match m in _rxAcc.Matches(f))
            {
                accounts.Add(new Acc() { Value = m.Value, Pos = m.Index, IsCounterparty = _rxCounterparty.IsMatch(Before(f, m.Index, 40)) });
            }

            var lisVS = _rxVS.Matches(f).Cast<Match>().ToList();
            if (lisVS.Count > 0)
            {
                //každý variabilní symbol = jeden pohyb; patří k němu nejbližší částka
                foreach (Match m in lisVS)
                {
                    var a = pay.OrderBy(p => Math.Abs(p.Pos - m.Index)).First();
                    string vs = m.Groups[1].Value.TrimStart('0');
                    if (vs == "") vs = "0";
                    ret.Add(new BankMailMove() { Amount = a.Value, Direction = GetDirection(f, a), VS = vs, Account = GetAccount(accounts, a) });
                }
                return ret;
            }

            //e-mail bez variabilního symbolu: jeden pohyb, čísla z textu jako možná čísla dokladů
            var a1 = pay[0];
            var move = new BankMailMove() { Amount = a1.Value, Direction = GetDirection(f, a1), Account = GetAccount(accounts, a1) };
            foreach (Match m in _rxInt.Matches(f))
            {
                if (amounts.Any(p => p.Pos <= m.Index && m.Index < p.End)) continue;
                if (accounts.Any(p => p.Pos <= m.Index && m.Index < p.Pos + p.Value.Length)) continue;
                string c = m.Value.TrimStart('0');
                if (c != "" && !move.Codes.Contains(c)) move.Codes.Add(c);
                if (move.Codes.Count >= 15) break;
            }
            ret.Add(move);
            return ret;
        }

        private static void CollectAmounts(string f, Regex rx, bool bolDecimals, List<Amt> amounts)
        {
            foreach (Match m in rx.Matches(f))
            {
                if (amounts.Any(p => p.Pos <= m.Index && m.Index < p.End)) continue;
                string s = m.Groups[2].Value.Replace(" ", "").Replace(".", "").Replace(",", "");
                if (bolDecimals) s += "." + m.Groups[3].Value.Substring(1);
                if (!double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out double val) || val == 0) continue;
                amounts.Add(new Amt() { Value = val, Sign = m.Groups[1].Value, Pos = m.Index, End = m.Index + m.Length, IsBalance = _rxBalance.IsMatch(Before(f, m.Index, 60)) });
            }
        }

        private static int GetDirection(string f, Amt a)
        {
            if (a.Sign == "-") return -1;
            if (a.Sign == "+") return 1;
            //částka bez znaménka (Fio): rozhoduje nejbližší slovo, slovo před částkou má přednost
            int intBest = int.MaxValue; int ret = 0;
            foreach (int d in new[] { 1, -1 })
            {
                foreach (string w in (d == 1 ? _wordsIn : _wordsOut))
                {
                    int i = f.IndexOf(w, StringComparison.Ordinal);
                    while (i >= 0)
                    {
                        int dist = (i < a.Pos ? a.Pos - i : (i - a.Pos) + 100000);
                        if (dist < intBest) { intBest = dist; ret = d; }
                        i = f.IndexOf(w, i + 1, StringComparison.Ordinal);
                    }
                }
            }
            return ret;
        }

        private static string GetAccount(List<Acc> accounts, Amt a)
        {
            var lis = accounts.Where(p => p.IsCounterparty).ToList();
            if (lis.Count == 0 && accounts.Count == 1) lis = accounts;
            if (lis.Count == 0) return null;
            return lis.OrderBy(p => Math.Abs(p.Pos - a.Pos)).First().Value;
        }

        private static string Before(string s, int intPos, int intLen)
        {
            int intStart = Math.Max(0, intPos - intLen);
            return s.Substring(intStart, intPos - intStart);
        }

        private static string Normalize(string s)
        {
            return s.Replace(' ', ' ').Replace(' ', ' ').Replace('−', '-').Replace("\r\n", "\n").Replace('\r', '\n');
        }

        /// <summary>Malá písmena bez diakritiky; délka textu se nemění, takže pozice sedí s původním textem.</summary>
        private static string Fold(string s)
        {
            var sb = new StringBuilder(s.Length);
            foreach (char c in s)
            {
                if (c < 128 || char.IsSurrogate(c))
                {
                    sb.Append(char.ToLowerInvariant(c));
                    continue;
                }
                string d = c.ToString().Normalize(NormalizationForm.FormD);
                sb.Append(char.ToLowerInvariant(d.Length > 0 ? d[0] : c));
            }
            return sb.ToString();
        }
    }
}
