using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace UI.Code
{
    /// <summary>
    /// SignalR hub interního chatu. Slouží pouze k doručování událostí do prohlížečů:
    /// nic neukládá a nesahá do databáze - veškeré zápisy jdou přes ChatController a BL.ChatBL.
    ///
    /// Každé spojení se zařadí do skupiny podle přihlašovacího jména. Login obsahuje doménu licence,
    /// takže se zprávy nemohou pomíchat mezi zákazníky ani v režimu SharedApp.
    /// </summary>
    [Authorize]
    public class ChatHub : Hub
    {
        public static string GroupName(string strLogin)
        {
            return "chat:" + (strLogin ?? "").Trim().ToLower();
        }

        /// <summary>
        /// Oznámí členům konverzace, že se v ní něco změnilo mimo ChatController (zpráva od systému z workflow).
        /// Prohlížeče si konverzaci a seznam načtou znovu. Registruje se ve Startup jako obsluha BL.ChatEvents.ChannelChanged.
        /// </summary>
        public static void NotifyChannel(IServiceProvider sp, List<string> logins, int u01id)
        {
            try
            {
                var hub = (IHubContext<ChatHub>)sp.GetService(typeof(IHubContext<ChatHub>));
                if (hub == null || logins == null) return;
                foreach (string strLogin in logins)
                {
                    _ = hub.Clients.Group(GroupName(strLogin)).SendAsync("chat", new { type = "channel", u01id = u01id, u02id = 0, actorid = 0, muted = false, message = (object)null });
                }
                //stejnou událost dostanou i uživatelé připojení přes druhou aplikaci (UI <-> mobilní MO)
                ChatRelay.Send(sp, logins.Select(p => new ChatRelay.Target() { login = p }).ToList(), "channel", u01id, 0, 0, null);
            }
            catch (Exception e)
            {
                BO.Code.File.LogError(e.Message, null, "ChatHub.NotifyChannel");
            }
        }

        public override async Task OnConnectedAsync()
        {
            string strLogin = Context.User?.Identity?.Name;
            if (string.IsNullOrEmpty(strLogin))
            {
                Context.Abort();
                return;
            }
            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(strLogin));

            //zprávy se rozesílají podle j02User.j02Login. Kdyby se jméno v přihlašovací cookie lišilo od loginu v databázi,
            //zařadit spojení i do skupiny podle databáze - jinak by uživateli nic nechodilo.
            try
            {
                var http = Context.GetHttpContext();
                if (http != null)
                {
                    var ru = (BO.RunningUser)http.RequestServices.GetService(typeof(BO.RunningUser));
                    if (ru != null && string.IsNullOrEmpty(ru.j02Login)) ru.j02Login = strLogin;
                    var f = (BL.Factory)http.RequestServices.GetService(typeof(BL.Factory));
                    string strDbLogin = f?.CurrentUser?.j02Login;
                    if (!string.IsNullOrEmpty(strDbLogin) && GroupName(strDbLogin) != GroupName(strLogin))
                    {
                        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(strDbLogin));
                    }
                }
            }
            catch (Exception e)
            {
                BO.Code.File.LogError(e.Message, strLogin, "ChatHub.OnConnectedAsync");
            }
            await base.OnConnectedAsync();
        }
    }
}
