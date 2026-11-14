using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.SignalR;

namespace UI.Code
{
    /// <summary>
    /// Most mezi dvěma webovými aplikacemi MARKTIME (UI a mobilní MO), které běží jako samostatné procesy:
    /// SignalR událost chatu vzniklá v jedné aplikaci se přepošle do druhé, aby ji dostali i tam připojení uživatelé.
    ///
    /// Adresa druhé aplikace: appsettings.json, "Chat": { "RelayUrl": "https://..." }. Bez ní se nic nepřeposílá.
    /// Zabezpečení: obsah je zašifrovaný sdíleným Data Protection klíčem (stejný KeyPath + AppName jako u SSO cookie)
    /// a platí 2 minuty - cizí požadavek nejde podvrhnout a není potřeba další heslo.
    /// </summary>
    public static class ChatRelay
    {
        private const string Purpose = "MARKTIME.ChatRelay.v1";
        public const string Path = "/ChatRelay/Event";

        public class Target
        {
            public string login { get; set; }
            public bool muted { get; set; }
        }

        private class Envelope<T>
        {
            public string type { get; set; }
            public int u01id { get; set; }
            public int u02id { get; set; }
            public int actorid { get; set; }
            public List<Target> targets { get; set; }
            public T message { get; set; }
        }

        /// <summary>Přepošle událost do druhé aplikace. Nečeká na výsledek a nikdy nevyhodí výjimku.</summary>
        public static void Send(IServiceProvider sp, List<Target> targets, string strType, int u01id, int u02id, int actorid, object message)
        {
            try
            {
                if (sp == null || targets == null || targets.Count == 0) return;
                var cfg = (IConfiguration)sp.GetService(typeof(IConfiguration));
                string strUrl = cfg?.GetSection("Chat")?["RelayUrl"];
                if (string.IsNullOrWhiteSpace(strUrl)) return;

                var dp = (IDataProtectionProvider)sp.GetService(typeof(IDataProtectionProvider));
                var factory = (IHttpClientFactory)sp.GetService(typeof(IHttpClientFactory));
                if (dp == null || factory == null) return;

                string strJson = JsonSerializer.Serialize(new Envelope<object>() { type = strType, u01id = u01id, u02id = u02id, actorid = actorid, targets = targets, message = message });
                string strToken = dp.CreateProtector(Purpose).ToTimeLimitedDataProtector().Protect(strJson, TimeSpan.FromMinutes(2));
                string strTarget = strUrl.Trim().TrimEnd('/') + Path;

                //služby jsou vyzvednuté předem: požadavek, ze kterého voláme, mezitím skončí
                _ = Task.Run(async () =>
                {
                    try
                    {
                        using (var hc = factory.CreateClient())
                        {
                            hc.Timeout = TimeSpan.FromSeconds(5);
                            using (var content = new StringContent(strToken, System.Text.Encoding.UTF8, "text/plain"))
                            {
                                await hc.PostAsync(strTarget, content);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        BO.Code.File.LogError(e.Message, null, "ChatRelay.Send");
                    }
                });
            }
            catch (Exception e)
            {
                BO.Code.File.LogError(e.Message, null, "ChatRelay.Send");
            }
        }

        /// <summary>Přijme událost z druhé aplikace a doručí ji zdejším prohlížečům. Vrací false, když obsah není pravý nebo vypršel.</summary>
        public static bool Receive(IServiceProvider sp, string strToken)
        {
            if (string.IsNullOrWhiteSpace(strToken)) return false;
            Envelope<JsonElement> c;
            try
            {
                var dp = (IDataProtectionProvider)sp.GetService(typeof(IDataProtectionProvider));
                string strJson = dp.CreateProtector(Purpose).ToTimeLimitedDataProtector().Unprotect(strToken.Trim());
                c = JsonSerializer.Deserialize<Envelope<JsonElement>>(strJson);
            }
            catch
            {
                return false;
            }
            if (c == null || c.targets == null) return false;

            var hub = (IHubContext<ChatHub>)sp.GetService(typeof(IHubContext<ChatHub>));
            if (hub == null) return true;
            object message = c.message.ValueKind == JsonValueKind.Undefined || c.message.ValueKind == JsonValueKind.Null ? null : c.message;
            foreach (var t in c.targets.Where(p => !string.IsNullOrEmpty(p.login)))
            {
                _ = hub.Clients.Group(ChatHub.GroupName(t.login)).SendAsync("chat", new { type = c.type, u01id = c.u01id, u02id = c.u02id, actorid = c.actorid, muted = t.muted, message = message });
            }
            return true;
        }
    }
}
