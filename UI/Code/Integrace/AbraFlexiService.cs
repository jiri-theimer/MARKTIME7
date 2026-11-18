using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UI.Code.Integrace
{
    //ABRA Flexi REST API (https://podpora.flexibee.eu, kolekce Dokumentace REST API):
    //odeslání hotovostních výdajů (evidence pokladni-pohyb) a dodavatelských faktur (evidence faktura-prijata) bez položek, volitelně s přílohami.
    public class AbraFlexiService
    {
        public const string EV_POKLADNA = "pokladni-pohyb";
        public const string EV_FAKTURA = "faktura-prijata";

        private readonly BO.x07Integration _rec;
        private readonly BO.x07Config _cfg;

        public AbraFlexiService(BO.x07Integration rec, BO.x07Config cfg)
        {
            _rec = rec;
            _cfg = cfg ?? new BO.x07Config();
        }

        //co v nastavení chybí; null = lze volat
        public string ValidateSetting()
        {
            if (string.IsNullOrWhiteSpace(_rec.x07Url)) return "V nastavení integrace chybí adresa serveru.";
            if (!_rec.x07Url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) return "Adresa serveru musí začínat https://.";
            if (string.IsNullOrWhiteSpace(_cfg.Company)) return "V nastavení integrace chybí identifikátor firmy.";
            if (string.IsNullOrWhiteSpace(_rec.x07Login) || string.IsNullOrWhiteSpace(_rec.x07Password)) return "V nastavení integrace chybí přihlašovací jméno nebo heslo.";
            return null;
        }

        private string Root()
        {
            return $"{_rec.x07Url.TrimEnd('/')}/c/{Uri.EscapeDataString(_cfg.Company.Trim())}/";
        }
        private HttpClient Client()
        {
            var c = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
            c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_rec.x07Login}:{_rec.x07Password}")));
            c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return c;
        }

        //ověření adresy, firmy a přihlášení; null = v pořádku
        public string TestConnection()
        {
            string strErr = ValidateSetting();
            if (strErr != null) return strErr;
            try
            {
                using (var client = Client())
                {
                    var response = client.GetAsync(Root() + EV_FAKTURA + ".json?limit=1&detail=id").Result;
                    if (response.IsSuccessStatusCode) return null;
                    int code = (int)response.StatusCode;
                    if (code == 401 || code == 403) return "ABRA Flexi odmítla přihlášení (jméno, heslo nebo oprávnění k REST API).";
                    if (code == 404) return "Firma s tímto identifikátorem na serveru není.";
                    return $"ABRA Flexi odpověděla chybou {code}.";
                }
            }
            catch (Exception ex)
            {
                return "Spojení s ABRA Flexi se nezdařilo: " + (ex.InnerException ?? ex).Message;
            }
        }

        //tělo požadavku pro jeden doklad (veřejné kvůli testům a ladění)
        public JObject BuildDoc(AbraFlexiDoc d, BO.x07ConfigDocMap map)
        {
            var o = new JObject();
            o["id"] = $"ext:MARKTIME:o23-{d.pid}";     //externí identifikátor: opakované odeslání doklad aktualizuje, nezaloží druhý
            if (!string.IsNullOrWhiteSpace(map.TypDokl)) o["typDokl"] = "code:" + map.TypDokl.Trim();
            o["datVyst"] = d.Date.ToString("yyyy-MM-dd");
            o["popis"] = Cut(d.Text, 255);
            if (!string.IsNullOrWhiteSpace(d.Supplier)) o["nazFirmy"] = Cut(d.Supplier, 255);
            o["mena"] = "code:" + d.Currency;
            o["bezPolozek"] = true;

            double dblBase = Math.Round(d.AmountWithoutVat, 2), dblVat = Math.Round(d.AmountVat, 2);
            if (d.VatRate <= 0 || dblVat == 0)
            {
                o["sumOsv"] = Math.Round(d.AmountWithVat, 2);          //bez DPH: celá částka jako osvobozená
            }
            else if (d.VatRate >= 20)
            {
                o["sumZklZakl"] = dblBase; o["sumDphZakl"] = dblVat;    //základní sazba
            }
            else
            {
                o["sumZklSniz"] = dblBase; o["sumDphSniz"] = dblVat;    //snížená sazba
            }

            if (map.Evidence == EV_FAKTURA)
            {
                o["cisDosle"] = string.IsNullOrWhiteSpace(d.Code) ? "MT-" + d.pid : Cut(d.Code, 30);
                o["datSplat"] = d.Date.ToString("yyyy-MM-dd");
            }
            else
            {
                o["typPohybuK"] = "typPohybu.vydej";
                if (!string.IsNullOrWhiteSpace(map.Pokladna)) o["pokladna"] = "code:" + map.Pokladna.Trim();
                if (!string.IsNullOrWhiteSpace(d.Code)) o["cisDosle"] = Cut(d.Code, 30);
            }
            return o;
        }

        //odešle jeden doklad; vrací id dokladu v ABRA Flexi nebo chybu
        public IntegraceRow Send(AbraFlexiDoc d, BO.x07ConfigDocMap map)
        {
            var row = new IntegraceRow() { pid = d.pid, Name = d.Text };
            if (!string.Equals(d.Currency, _cfg.HomeCurrency ?? "CZK", StringComparison.OrdinalIgnoreCase))
            {
                row.Message = $"Doklad je v měně {d.Currency}; odesílají se jen doklady v měně účetnictví ({_cfg.HomeCurrency ?? "CZK"}).";
                return row;
            }
            try
            {
                var body = new JObject { ["winstrom"] = new JObject { ["@version"] = "1.0", [map.Evidence] = new JArray(BuildDoc(d, map)) } };
                using (var client = Client())
                {
                    using (var content = new StringContent(body.ToString(Formatting.None), Encoding.UTF8, "application/json"))
                    {
                        var response = client.PostAsync(Root() + map.Evidence + ".json", content).Result;
                        string s = response.Content.ReadAsStringAsync().Result;
                        string strID = ParseResult(s, out string strErr);
                        if (strID == null)
                        {
                            row.Message = strErr ?? $"ABRA Flexi odpověděla chybou {(int)response.StatusCode}.";
                            return row;
                        }
                        row.ExternalPid = strID;
                        row.IsOk = true;
                    }

                    if (_cfg.SendFiles)
                    {
                        var errs = new List<string>();
                        foreach (var f in d.Files)
                        {
                            if (!System.IO.File.Exists(f.FullPath)) { errs.Add(f.FileName + ": soubor nenalezen"); continue; }
                            using (var fc = new ByteArrayContent(System.IO.File.ReadAllBytes(f.FullPath)))
                            {
                                fc.Headers.ContentType = new MediaTypeHeaderValue(string.IsNullOrWhiteSpace(f.ContentType) ? "application/octet-stream" : f.ContentType);
                                var r = client.PutAsync($"{Root()}{map.Evidence}/{row.ExternalPid}/prilohy/new/{Uri.EscapeDataString(f.FileName)}", fc).Result;
                                if (!r.IsSuccessStatusCode) errs.Add($"{f.FileName}: chyba {(int)r.StatusCode}");
                            }
                        }
                        if (errs.Count > 0) row.Message = "Doklad odeslán, přílohy ne: " + string.Join("; ", errs);
                    }
                }
            }
            catch (Exception ex)
            {
                row.IsOk = row.ExternalPid != null;
                row.Message = "Spojení s ABRA Flexi se nezdařilo: " + (ex.InnerException ?? ex).Message;
            }
            return row;
        }

        //odpověď: {"winstrom":{"success":"true","results":[{"id":"123",...}]}}; při chybě results[].errors[].message
        public static string ParseResult(string json, out string strError)
        {
            strError = null;
            try
            {
                var w = JObject.Parse(json)["winstrom"];
                var results = w?["results"] as JArray;
                bool ok = string.Equals((string)w?["success"], "true", StringComparison.OrdinalIgnoreCase);
                if (ok && results != null && results.Count > 0 && results[0]["id"] != null)
                {
                    return (string)results[0]["id"];
                }
                var msgs = new List<string>();
                if (results != null)
                {
                    foreach (var r in results)
                    {
                        if (r["errors"] is JArray errs)
                        {
                            foreach (var e in errs)
                            {
                                string m = (string)e["message"];
                                if (!string.IsNullOrEmpty(m)) msgs.Add(((string)e["for"] != null ? (string)e["for"] + ": " : "") + m);
                            }
                        }
                    }
                }
                if (msgs.Count == 0 && w?["message"] != null) msgs.Add((string)w["message"]);
                strError = msgs.Count > 0 ? string.Join("; ", msgs) : "ABRA Flexi doklad nepřijala.";
            }
            catch
            {
                strError = "ABRA Flexi vrátila odpověď, které nerozumím.";
            }
            return null;
        }

        private static string Cut(string s, int len)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return s.Length > len ? s.Substring(0, len) : s;
        }
    }

    //doklad připravený k odeslání (z dokumentu typu Účtenka)
    public class AbraFlexiDoc
    {
        public int pid { get; set; }
        public string Text { get; set; }
        public string Code { get; set; }
        public string Supplier { get; set; }
        public DateTime Date { get; set; }
        public string Currency { get; set; }
        public double AmountWithVat { get; set; }
        public double AmountWithoutVat { get; set; }
        public double AmountVat { get; set; }
        public double VatRate { get; set; }
        public List<AbraFlexiFile> Files { get; set; } = new List<AbraFlexiFile>();
    }
    public class AbraFlexiFile
    {
        public string FileName { get; set; }
        public string FullPath { get; set; }
        public string ContentType { get; set; }
    }
}
