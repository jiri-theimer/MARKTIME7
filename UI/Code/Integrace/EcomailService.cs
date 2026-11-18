using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UI.Code.Integrace
{
    //Ecomail API v2 (https://docs.ecomail.cz): seznamy kontaktů a hromadné přidání odběratelů
    public class EcomailService
    {
        private const string BASEURL = "https://api2.ecomailapp.cz/";
        private const int CHUNK = 500;      //API bere nejvýše 3000 odběratelů na volání; menší dávky kvůli době odezvy
        private readonly string _token;

        public EcomailService(BO.x07Integration rec)
        {
            _token = rec.x07Token;
        }

        private HttpClient Client()
        {
            var c = new HttpClient { BaseAddress = new Uri(BASEURL), Timeout = TimeSpan.FromSeconds(60) };
            c.DefaultRequestHeaders.TryAddWithoutValidation("key", _token);
            return c;
        }

        //seznamy kontaktů v Ecomailu; při chybě vrací null a text chyby
        public List<BO.StringPair> GetLists(out string strError)
        {
            strError = null;
            if (string.IsNullOrWhiteSpace(_token))
            {
                strError = "V nastavení integrace chybí API klíč (token).";
                return null;
            }
            try
            {
                using (var client = Client())
                {
                    var response = client.GetAsync("lists").Result;
                    string s = response.Content.ReadAsStringAsync().Result;
                    if (!response.IsSuccessStatusCode)
                    {
                        strError = $"Ecomail odpověděl chybou {(int)response.StatusCode}. Zkontrolujte API klíč.";
                        return null;
                    }
                    var ret = new List<BO.StringPair>();
                    foreach (var x in JArray.Parse(s))
                    {
                        ret.Add(new BO.StringPair() { Key = (string)x["id"], Value = (string)x["name"] });
                    }
                    return ret;
                }
            }
            catch (Exception ex)
            {
                strError = "Spojení s Ecomailem se nezdařilo: " + (ex.InnerException ?? ex).Message;
                return null;
            }
        }

        //přidá kontakty do seznamu; existující odběratele aktualizuje, odhlášené znovu nepřihlašuje
        public IntegraceResult Subscribe(string strListID, List<EcomailSubscriber> subscribers)
        {
            var ret = new IntegraceResult();
            try
            {
                using (var client = Client())
                {
                    for (int i = 0; i < subscribers.Count; i += CHUNK)
                    {
                        var chunk = subscribers.Skip(i).Take(CHUNK).ToList();
                        var body = new
                        {
                            subscriber_data = chunk.Select(p => p.ToApi()).ToList(),
                            update_existing = true,
                            resubscribe = false,
                            trigger_autoresponders = false
                        };
                        string json = JsonConvert.SerializeObject(body, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                        using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
                        {
                            var response = client.PostAsync($"lists/{Uri.EscapeDataString(strListID)}/subscribe-bulk", content).Result;
                            string s = response.Content.ReadAsStringAsync().Result;
                            bool ok = response.IsSuccessStatusCode;
                            string strMsg = ok ? null : $"Ecomail odpověděl chybou {(int)response.StatusCode}: {Short(s)}";
                            foreach (var c in chunk)
                            {
                                ret.Rows.Add(new IntegraceRow() { pid = c.p28ID, Name = c.email, IsOk = ok, Message = strMsg });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ret.Error = "Spojení s Ecomailem se nezdařilo: " + (ex.InnerException ?? ex).Message;
            }
            return ret;
        }

        private static string Short(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Length > 300 ? s.Substring(0, 300) + "…" : s;
        }
    }

    public class EcomailSubscriber
    {
        public int p28ID { get; set; }
        public string email { get; set; }
        public string name { get; set; }
        public string surname { get; set; }
        public string pretitle { get; set; }
        public string company { get; set; }
        public string street { get; set; }
        public string city { get; set; }
        public string zip { get; set; }

        public object ToApi()
        {
            return new { email, name, surname, pretitle, company, street, city, zip, source = "MARKTIME" };
        }
    }
}
