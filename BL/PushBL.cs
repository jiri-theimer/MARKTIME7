using System.Collections.Concurrent;
using System.Security.Cryptography;
using WebPush;

namespace BL
{
    /// <summary>
    /// Web push: upozornění na nové zprávy chatu do mobilní aplikace MO, i když ji uživatel nemá otevřenou.
    /// Odběry prohlížečů jsou v tabulce u05PushSubscription (skript chat_push.sql), posílá se knihovnou WebPush.
    ///
    /// Nastavení v appsettings.json (v UI i v MO stejné hodnoty - zprávy vznikají v obou aplikacích):
    ///   "Push": { "Subject": "mailto:info@firma.cz", "PublicKey": "...", "PrivateKey": "..." }
    /// Klíče vygeneruje admin v MO: /Push/GenerateKeys. Bez klíčů nebo bez tabulky se nic neposílá a nic se nerozbije.
    /// </summary>
    public interface IPushBL
    {
        public bool IsAvailable();
        public string PublicKey { get; }
        public bool Subscribe(string strEndpoint, string strP256dh, string strAuth, string strUserAgent);
        public bool Unsubscribe(string strEndpoint);
        public int CountMine();
        public void ChatMessage(int u02id);
        public int SendTest();
    }

    public class PushKeys
    {
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }

        /// <summary>Nový pár VAPID klíčů (P-256) ve tvaru, který čeká prohlížeč i knihovna WebPush (base64url).</summary>
        public static PushKeys Generate()
        {
            using (var ec = ECDsa.Create(ECCurve.NamedCurves.nistP256))
            {
                var p = ec.ExportParameters(true);
                var pub = new byte[65];
                pub[0] = 0x04;      //nekomprimovaný bod: 0x04 + X + Y
                Buffer.BlockCopy(p.Q.X, 0, pub, 1, 32);
                Buffer.BlockCopy(p.Q.Y, 0, pub, 33, 32);
                return new PushKeys() { PublicKey = B64Url(pub), PrivateKey = B64Url(p.D) };
            }
        }

        private static string B64Url(byte[] b)
        {
            return Convert.ToBase64String(b).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }
    }

    class PushBL : BaseBL, IPushBL
    {
        private static readonly ConcurrentDictionary<string, DateTime> _available = new ConcurrentDictionary<string, DateTime>();   //kde už tabulka je
        private static readonly ConcurrentDictionary<string, DateTime> _unavailable = new ConcurrentDictionary<string, DateTime>(); //kde není (zkusit znovu za 5 minut)
        private static readonly HttpClient _http = new HttpClient() { Timeout = TimeSpan.FromSeconds(15) };

        public PushBL(BL.Factory mother) : base(mother)
        {

        }

        private class Subscription
        {
            public int u05ID { get; set; }
            public int j02ID { get; set; }
            public string u05Endpoint { get; set; }
            public string u05P256dh { get; set; }
            public string u05Auth { get; set; }
        }

        private class MessageInfo
        {
            public int u01ID { get; set; }
            public int j02ID_Sender { get; set; }
            public int u02TypeFlag { get; set; }
            public string u02Text200 { get; set; }
            public bool u02IsDeleted { get; set; }
            public int u01TypeFlag { get; set; }
            public string u01Name { get; set; }
            public string SenderName { get; set; }
        }

        // ── nastavení ───────────────────────────────────────────────────────────────

        private string Cfg(string strKey)
        {
            return _mother.App.Configuration?.GetSection("Push")?[strKey];
        }

        public string PublicKey
        {
            get
            {
                return Cfg("PublicKey");
            }
        }

        private VapidDetails Vapid()
        {
            string strPublic = Cfg("PublicKey"), strPrivate = Cfg("PrivateKey");
            if (string.IsNullOrWhiteSpace(strPublic) || string.IsNullOrWhiteSpace(strPrivate)) return null;
            string strSubject = Cfg("Subject");
            if (string.IsNullOrWhiteSpace(strSubject)) strSubject = "mailto:info@marktime.cz";
            return new VapidDetails(strSubject.Trim(), strPublic.Trim(), strPrivate.Trim());
        }

        private bool TableExists()
        {
            if (_mother.CurrentUser == null || _db == null) return false;
            string strKey = $"{_mother.App.HostingMode}|{_mother.CurrentUser.x01LoginDomain}";
            if (_available.ContainsKey(strKey)) return true;
            if (_unavailable.TryGetValue(strKey, out DateTime d) && d > DateTime.Now) return false;

            var c = _db.Load<BO.GetInteger>("SELECT CASE WHEN OBJECT_ID('dbo.u05PushSubscription') IS NULL THEN 0 ELSE 1 END as Value");
            if (c != null && c.Value == 1)
            {
                _available[strKey] = DateTime.Now;
                return true;
            }
            _unavailable[strKey] = DateTime.Now.AddMinutes(5);
            return false;
        }

        /// <summary>Jsou nastavené klíče a v databázi je tabulka odběrů?</summary>
        public bool IsAvailable()
        {
            return !string.IsNullOrWhiteSpace(Cfg("PublicKey")) && !string.IsNullOrWhiteSpace(Cfg("PrivateKey")) && TableExists();
        }

        // ── odběry ──────────────────────────────────────────────────────────────────

        /// <summary>
        /// Zapíše odběr prohlížeče pro přihlášeného uživatele. Jeden prohlížeč (endpoint) = jeden řádek:
        /// když se na stejném telefonu přihlásí někdo jiný, odběr přejde na něj.
        /// </summary>
        public bool Subscribe(string strEndpoint, string strP256dh, string strAuth, string strUserAgent)
        {
            if (!TableExists()) return false;
            strEndpoint = (strEndpoint ?? "").Trim(); strP256dh = (strP256dh ?? "").Trim(); strAuth = (strAuth ?? "").Trim();
            if (strEndpoint.Length < 10 || strEndpoint.Length > 1000 || strP256dh == "" || strAuth == "" || strP256dh.Length > 200 || strAuth.Length > 100) return false;
            if (!strEndpoint.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) return false;     //adresa služby prohlížeče (Google, Apple, Mozilla)
            if (strUserAgent != null && strUserAgent.Length > 300) strUserAgent = strUserAgent.Substring(0, 300);

            var pars = new { j02id = _mother.CurrentUser.pid, endpoint = strEndpoint, p256dh = strP256dh, auth = strAuth, ua = strUserAgent };
            _db.RunSql2("UPDATE u05PushSubscription SET j02ID=@j02id,u05P256dh=@p256dh,u05Auth=@auth,u05UserAgent=@ua,u05DateLastUse=GETDATE() WHERE u05Endpoint=@endpoint", pars);
            _db.RunSql2("INSERT INTO u05PushSubscription(j02ID,u05Endpoint,u05P256dh,u05Auth,u05UserAgent,u05DateInsert,u05DateLastUse) SELECT @j02id,@endpoint,@p256dh,@auth,@ua,GETDATE(),GETDATE() WHERE NOT EXISTS (SELECT 1 FROM u05PushSubscription WHERE u05Endpoint=@endpoint)", pars);
            return true;
        }

        public bool Unsubscribe(string strEndpoint)
        {
            if (!TableExists() || string.IsNullOrWhiteSpace(strEndpoint)) return false;
            _db.RunSql2("DELETE FROM u05PushSubscription WHERE u05Endpoint=@endpoint AND j02ID=@j02id", new { endpoint = strEndpoint.Trim(), j02id = _mother.CurrentUser.pid });
            return true;
        }

        public int CountMine()
        {
            if (!TableExists()) return 0;
            var c = _db.Load<BO.GetInteger>("SELECT COUNT(*) as Value FROM u05PushSubscription WHERE j02ID=@j02id", new { j02id = _mother.CurrentUser.pid });
            return c == null ? 0 : c.Value;
        }

        // ── odeslání ────────────────────────────────────────────────────────────────

        /// <summary>
        /// Upozorní na zprávu chatu členy konverzace, kteří ji nemají ztlumenou (kromě odesílatele; upozornění systému
        /// typu 3 dostane každý člen). Volat až po uložení příloh. Neblokuje: odesílá se na pozadí, chyba nic neshodí.
        /// </summary>
        public void ChatMessage(int u02id)
        {
            try
            {
                if (u02id <= 0 || !IsAvailable()) return;

                var msg = _db.Load<MessageInfo>("SELECT a.u01ID,a.j02ID_Sender,a.u02TypeFlag,a.u02Text200,a.u02IsDeleted,c.u01TypeFlag,c.u01Name"
                    + ",LTRIM(RTRIM(ISNULL(s.j02FirstName,'')+' '+ISNULL(s.j02LastName,''))) as SenderName"
                    + " FROM u02Message a INNER JOIN u01Channel c ON a.u01ID=c.u01ID LEFT OUTER JOIN j02User s ON a.j02ID_Sender=s.j02ID WHERE a.u02ID=@pid", new { pid = u02id });
                if (msg == null || msg.u02IsDeleted || msg.u02TypeFlag == 2) return;    //systémové zprávy (workflow, založení skupiny) se neposílají

                var subs = _db.GetList<Subscription>("SELECT s.u05ID,s.j02ID,s.u05Endpoint,s.u05P256dh,s.u05Auth FROM u05PushSubscription s"
                    + " INNER JOIN u03ChannelMember m ON s.j02ID=m.j02ID"
                    + " WHERE m.u01ID=@u01id AND m.u03DateLeft IS NULL AND m.u03IsMuted=0 AND (m.j02ID<>@sender OR @type=3)",
                    new { u01id = msg.u01ID, sender = msg.j02ID_Sender, type = msg.u02TypeFlag });
                if (subs == null || !subs.Any()) return;

                string strBody = string.IsNullOrWhiteSpace(msg.u02Text200) ? _mother.tra("Příloha") : msg.u02Text200;
                string strTitle;
                if (msg.u02TypeFlag == 3)
                {
                    strTitle = "MARKTIME";
                }
                else if (msg.u01TypeFlag == 1)
                {
                    strTitle = string.IsNullOrWhiteSpace(msg.SenderName) ? "MARKTIME" : msg.SenderName;
                }
                else
                {
                    strTitle = string.IsNullOrWhiteSpace(msg.u01Name) ? "MARKTIME" : msg.u01Name;
                    if (!string.IsNullOrWhiteSpace(msg.SenderName)) strBody = msg.SenderName.Split(' ')[0] + ": " + strBody;
                }

                string strPayload = System.Text.Json.JsonSerializer.Serialize(new { title = strTitle, body = strBody, url = "Chat/Thread/" + msg.u01ID, tag = "chat-" + msg.u01ID, u01id = msg.u01ID });
                Send(subs.ToList(), strPayload);
            }
            catch (Exception e)
            {
                BO.Code.File.LogError(e.Message, _mother.CurrentUser?.j02Login, "PushBL.ChatMessage");
            }
        }

        /// <summary>Zkušební upozornění do všech prohlížečů přihlášeného uživatele. Vrací počet odběrů, na které se posílá.</summary>
        public int SendTest()
        {
            if (!IsAvailable()) return 0;
            var subs = _db.GetList<Subscription>("SELECT s.u05ID,s.j02ID,s.u05Endpoint,s.u05P256dh,s.u05Auth FROM u05PushSubscription s WHERE s.j02ID=@j02id", new { j02id = _mother.CurrentUser.pid });
            if (subs == null || !subs.Any()) return 0;
            string strPayload = System.Text.Json.JsonSerializer.Serialize(new { title = "MARKTIME", body = _mother.tra("Zkušební upozornění. Funguje to."), url = "Chat/Index", tag = "chat-test", u01id = 0 });
            Send(subs.ToList(), strPayload);
            return subs.Count();
        }

        private void Send(List<Subscription> subs, string strPayload)
        {
            var vapid = Vapid();
            if (vapid == null) return;
            var db = _db;       //DbHandler drží jen connect string; smí se použít i po skončení požadavku
            string strLogin = _mother.CurrentUser?.j02Login;

            _ = Task.Run(async () =>
            {
                var client = new WebPushClient(_http);
                var options = new Dictionary<string, object>
                {
                    ["vapidDetails"] = vapid,
                    ["TTL"] = 24 * 3600,      //nedoručené do 24 hodin (vypnutý telefon) se zahodí
                    ["headers"] = new Dictionary<string, object> { ["Urgency"] = "high" }
                };
                foreach (var s in subs)
                {
                    try
                    {
                        await client.SendNotificationAsync(new PushSubscription(s.u05Endpoint, s.u05P256dh, s.u05Auth), strPayload, options);
                    }
                    catch (WebPushException e)
                    {
                        if (e.StatusCode == System.Net.HttpStatusCode.NotFound || e.StatusCode == System.Net.HttpStatusCode.Gone)
                        {
                            //odběr už neplatí (odinstalovaná aplikace, zrušené povolení) - smazat
                            try { db.RunSql2("DELETE FROM u05PushSubscription WHERE u05ID=@pid", new { pid = s.u05ID }); } catch { }
                        }
                        else
                        {
                            BO.Code.File.LogError($"{(int)e.StatusCode} {e.Message}", strLogin, "PushBL.Send");
                        }
                    }
                    catch (Exception e)
                    {
                        BO.Code.File.LogError(e.Message, strLogin, "PushBL.Send");
                    }
                }
            });
        }
    }
}
