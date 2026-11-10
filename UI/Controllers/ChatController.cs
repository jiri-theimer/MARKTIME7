using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using UI.Models;

namespace UI.Controllers
{
    /// <summary>
    /// Interní chat mezi uživateli. Okno chatu: /Chat/Index (otevírá se z horní lišty přes _chat()).
    ///  /Chat/Index?j02id=15            - otevře přímou konverzaci s uživatelem
    ///  /Chat/Index?entity=p41&pid=123  - otevře konverzaci k záznamu (p41, p56, p28, p91, o23)
    /// Zápisy jdou přes BL.ChatBL, do prohlížečů se změny doručují přes SignalR (UI.Code.ChatHub).
    /// </summary>
    public class ChatController : BaseController
    {
        private const int MaxFiles = 5;
        private const long MaxFileBytes = 20L * 1024 * 1024;
        private static readonly HashSet<string> _allowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".msg", ".eml", ".pdf", ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".webp", ".tif", ".tiff", ".heic",
            ".xls", ".xlsx", ".doc", ".docx", ".txt", ".zip", ".rar", ".7z", ".ppt", ".pptx", ".csv",
            ".mp3", ".mp4", ".odt", ".ods", ".rtf", ".ics", ".isdoc", ".isdocx", ".xml"
        };
        private static readonly HashSet<string> _inlineImages = new(StringComparer.OrdinalIgnoreCase) { ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".webp" };

        // ── převod na JSON ──────────────────────────────────────────────────────────

        private static string D(DateTime? d)
        {
            return d == null ? null : d.Value.ToString("yyyy-MM-ddTHH:mm:ss");
        }

        private static object ToJson(BO.Chat.ChannelItem c)
        {
            return new
            {
                id = c.pid,
                type = c.u01TypeFlag,
                name = c.DisplayName,
                entity = c.u01RecordEntity,
                recpid = c.u01RecordPid,
                muted = c.u03IsMuted,
                pinned = c.u03IsPinned,
                role = c.u03RoleFlag,
                ownerid = c.j02ID_Owner,
                unread = c.UnreadCount,
                members = c.MembersCount,
                partnerid = c.PartnerID,
                partnerread = c.PartnerLastReadID,
                lastread = c.u02ID_LastRead,
                lastdate = D(c.u01DateLastMessage ?? c.u01DateInsert),
                lastsenderid = c.LastSenderID,
                lastsender = c.LastSenderName,
                lasttext = c.LastIsDeleted ? null : c.LastText,
                lastdeleted = c.LastIsDeleted,
                lastfiles = c.LastAttachmentsCount
            };
        }

        private static object ToJson(BO.Chat.MessageItem c)
        {
            return new
            {
                id = c.pid,
                u01id = c.u01ID,
                senderid = c.j02ID_Sender,
                sender = c.SenderName,
                type = c.u02TypeFlag,
                text = c.u02Text,
                replyto = c.u02ID_ReplyTo,
                replytext = c.ReplyText,
                replysender = c.ReplySenderName,
                edited = c.u02IsEdited,
                deleted = c.u02IsDeleted,
                date = D(c.u02DateInsert),
                files = c.Attachments.Select(p => new { id = p.o27ID, name = p.o27OriginalFileName, size = p.o27FileSize, image = _inlineImages.Contains(System.IO.Path.GetExtension(p.o27OriginalFileName ?? "")) }),
                reactions = c.Reactions.Select(p => new { key = p.u04Emoji, j02id = p.j02ID, name = p.PersonName })
            };
        }

        private JsonResult Fail(string strDefaultMessage = null)
        {
            string s = Factory.CurrentUser.GetLastMessageNotify();
            if (string.IsNullOrEmpty(s)) s = strDefaultMessage ?? Factory.tra("Operaci se nepodařilo provést.");
            return Json(new { ok = false, message = s });
        }

        // ── doručení přes SignalR ───────────────────────────────────────────────────

        /// <summary>Pošle událost všem členům konverzace. Každý člen dostává příznak, zda má konverzaci ztlumenou.</summary>
        private void Push(int u01id, string strType, object message = null, List<BO.Chat.MemberItem> members = null, int u02id = 0)
        {
            try
            {
                var hub = (IHubContext<UI.Code.ChatHub>)HttpContext.RequestServices.GetService(typeof(IHubContext<UI.Code.ChatHub>));
                if (hub == null) return;
                if (members == null) members = Factory.ChatBL.GetMembers(u01id, true);

                foreach (var c in members.Where(p => !string.IsNullOrEmpty(p.j02Login)))
                {
                    _ = hub.Clients.Group(UI.Code.ChatHub.GroupName(c.j02Login)).SendAsync("chat", new
                    {
                        type = strType,
                        u01id = u01id,
                        u02id = u02id,
                        actorid = Factory.CurrentUser.pid,
                        muted = c.u03IsMuted,
                        message = message
                    });
                }
            }
            catch (Exception e)
            {
                BO.Code.File.LogError(e.Message, Factory.CurrentUser.j02Login, "ChatController.Push");  //nedoručené oznámení nesmí shodit uložení zprávy
            }
        }

        // ── okno chatu ──────────────────────────────────────────────────────────────

        public IActionResult Index(int u01id, int j02id, string entity, int pid)
        {
            var v = new ChatViewModel() { IsAvailable = Factory.ChatBL.IsAvailable() };
            if (v.IsAvailable)
            {
                if (j02id > 0)
                {
                    u01id = Factory.ChatBL.OpenDirect(j02id);
                }
                else if (!string.IsNullOrEmpty(entity) && pid > 0)
                {
                    u01id = Factory.ChatBL.OpenRecordChannel(entity, pid);
                    if (u01id > 0) Push(u01id, "channel");
                }
                v.InitialU01ID = u01id;
            }
            return View(v);
        }

        public IActionResult Channels()
        {
            if (!Factory.ChatBL.IsAvailable()) return Json(new { ok = false });
            return Json(new { ok = true, channels = Factory.ChatBL.GetMyChannels().Select(p => ToJson(p)) });
        }

        public IActionResult UnreadCount()
        {
            if (!Factory.ChatBL.IsAvailable()) return Json(new { count = 0 });
            return Json(new { count = Factory.ChatBL.GetUnreadTotal() });
        }

        public IActionResult Messages(int u01id, int before)
        {
            var c = Factory.ChatBL.LoadMyChannel(u01id);
            if (c == null) return Fail(Factory.tra("Konverzace neexistuje nebo k ní nemáte přístup."));

            const int TAKE = 50;
            var lis = Factory.ChatBL.GetMessages(u01id, before, TAKE);
            return Json(new
            {
                ok = true,
                channel = ToJson(c),
                members = before > 0 ? null : Factory.ChatBL.GetMembers(u01id).Select(p => new { id = p.j02ID, name = p.PersonName, role = p.u03RoleFlag, lastread = p.u02ID_LastRead }),
                messages = lis.Select(p => ToJson(p)),
                hasmore = lis.Count == TAKE
            });
        }

        public IActionResult Users()
        {
            return Json(new { ok = true, users = Factory.ChatBL.GetUsers().Select(p => new { id = p.pid, name = $"{p.j02FirstName} {p.j02LastName}".Trim(), email = p.j02Email }) });
        }

        // ── zprávy ──────────────────────────────────────────────────────────────────

        [HttpPost]
        [RequestSizeLimit(110L * 1024 * 1024)]
        public IActionResult Send(int u01id, string text, int replyto)
        {
            var files = new List<IFormFile>();
            var rejected = new List<string>();
            if (Request.HasFormContentType && Request.Form.Files != null)
            {
                foreach (var file in Request.Form.Files.Where(p => p.Length > 0))
                {
                    string strExt = System.IO.Path.GetExtension(file.FileName ?? "");
                    if (files.Count >= MaxFiles || string.IsNullOrEmpty(strExt) || !_allowedExtensions.Contains(strExt) || file.Length > MaxFileBytes)
                    {
                        rejected.Add(System.IO.Path.GetFileName(file.FileName ?? ""));
                        continue;
                    }
                    files.Add(file);
                }
            }
            if (rejected.Count > 0 && files.Count == 0 && string.IsNullOrWhiteSpace(text))
            {
                return Json(new { ok = false, message = Factory.tra("Soubor nelze přiložit (nepovolený typ nebo velikost nad 20 MB)") + ": " + string.Join(", ", rejected) });
            }

            int intU02ID = Factory.ChatBL.SendMessage(u01id, text, replyto, files.Count > 0);
            if (intU02ID == 0) return Fail();

            foreach (var file in files)
            {
                string strTempPath = null;
                try
                {
                    if (!System.IO.Directory.Exists(Factory.TempFolder)) System.IO.Directory.CreateDirectory(Factory.TempFolder);
                    strTempPath = System.IO.Path.Combine(Factory.TempFolder, Guid.NewGuid().ToString("N") + System.IO.Path.GetExtension(file.FileName));
                    using (var fs = new System.IO.FileStream(strTempPath, System.IO.FileMode.Create))
                    {
                        file.CopyTo(fs);
                    }
                    if (Factory.ChatBL.AddAttachment(intU02ID, file.FileName, strTempPath, file.ContentType) == 0)
                    {
                        rejected.Add(System.IO.Path.GetFileName(file.FileName));
                    }
                }
                catch (Exception e)
                {
                    BO.Code.File.LogError(e.Message, Factory.CurrentUser.j02Login, "ChatController.Send");
                    rejected.Add(System.IO.Path.GetFileName(file.FileName));
                }
                finally
                {
                    try
                    {
                        if (strTempPath != null && System.IO.File.Exists(strTempPath)) System.IO.File.Delete(strTempPath);
                    }
                    catch
                    {
                        //dočasný soubor uklidí robot
                    }
                }
            }

            var msg = Factory.ChatBL.LoadMessage(intU02ID);
            if (msg == null) return Fail();
            if (string.IsNullOrEmpty(msg.u02Text) && msg.Attachments.Count == 0)
            {
                Factory.ChatBL.DeleteMessage(intU02ID);     //zpráva byla jen z příloh a žádná se neuložila
                return Json(new { ok = false, message = Factory.tra("Přílohu se nepodařilo uložit.") });
            }

            var json = ToJson(msg);
            Push(u01id, "message", json, null, intU02ID);
            return Json(new { ok = true, message = json, rejected = rejected });
        }

        [HttpPost]
        public IActionResult Edit(int u02id, string text)
        {
            if (!Factory.ChatBL.EditMessage(u02id, text)) return Fail();
            var msg = Factory.ChatBL.LoadMessage(u02id);
            var json = ToJson(msg);
            Push(msg.u01ID, "update", json, null, u02id);
            return Json(new { ok = true, message = json });
        }

        [HttpPost]
        public IActionResult Delete(int u02id)
        {
            if (!Factory.ChatBL.DeleteMessage(u02id)) return Fail();
            var msg = Factory.ChatBL.LoadMessage(u02id);
            var json = ToJson(msg);
            Push(msg.u01ID, "update", json, null, u02id);
            return Json(new { ok = true, message = json });
        }

        [HttpPost]
        public IActionResult React(int u02id, string key)
        {
            if (!Factory.ChatBL.ToggleReaction(u02id, key)) return Fail();
            var msg = Factory.ChatBL.LoadMessage(u02id);
            var json = ToJson(msg);
            Push(msg.u01ID, "update", json, null, u02id);
            return Json(new { ok = true, message = json });
        }

        [HttpPost]
        public IActionResult SaveAsNote(int u02id)
        {
            if (Factory.ChatBL.SaveMessageAsNote(u02id) == 0) return Fail();
            return Json(new { ok = true });
        }

        [HttpPost]
        public IActionResult Read(int u01id, int u02id)
        {
            if (Factory.ChatBL.MarkRead(u01id, u02id))
            {
                Push(u01id, "read", null, null, u02id);     //odesílatel uvidí "přečteno", moje ostatní okna si srovnají počet
            }
            return Json(new { ok = true, count = Factory.ChatBL.GetUnreadTotal() });
        }

        // ── diagnostika ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Test doručování: za 3 sekundy pošle přes SignalR zkušební událost stejnou cestou jako skutečné zprávy
        /// (do skupiny podle j02User.j02Login). V okně chatu se spouští kliknutím na reproduktor se stisknutým Shiftem.
        /// </summary>
        [HttpPost]
        public IActionResult Ping()
        {
            string strClaim = User?.Identity?.Name;
            string strDbLogin = Factory.j02UserBL.Load(Factory.CurrentUser.pid)?.j02Login;
            var hub = (IHubContext<UI.Code.ChatHub>)HttpContext.RequestServices.GetService(typeof(IHubContext<UI.Code.ChatHub>));
            string strGroup = UI.Code.ChatHub.GroupName(strDbLogin);
            _ = Task.Run(async () =>
            {
                await Task.Delay(3000);
                await hub.Clients.Group(strGroup).SendAsync("chat", new { type = "ping" });
            });
            return Json(new { ok = true, claim = strClaim, dblogin = strDbLogin, same = UI.Code.ChatHub.GroupName(strClaim) == strGroup });
        }

        // ── konverzace ──────────────────────────────────────────────────────────────

        [HttpPost]
        public IActionResult Direct(int j02id)
        {
            int intU01ID = Factory.ChatBL.OpenDirect(j02id);
            if (intU01ID == 0) return Fail();
            Push(intU01ID, "channel");
            return Json(new { ok = true, u01id = intU01ID });
        }

        [HttpPost]
        public IActionResult Group(string name, string j02ids)
        {
            int intU01ID = Factory.ChatBL.CreateGroup(name, BO.Code.Bas.ConvertString2ListInt(j02ids));
            if (intU01ID == 0) return Fail();
            Push(intU01ID, "channel");
            return Json(new { ok = true, u01id = intU01ID });
        }

        [HttpPost]
        public IActionResult AddMembers(int u01id, string j02ids)
        {
            if (!Factory.ChatBL.AddMembers(u01id, BO.Code.Bas.ConvertString2ListInt(j02ids))) return Fail(Factory.tra("Nikdo nebyl přidán."));
            Push(u01id, "channel");
            return Json(new { ok = true });
        }

        [HttpPost]
        public IActionResult Rename(int u01id, string name)
        {
            if (!Factory.ChatBL.RenameGroup(u01id, name)) return Fail();
            Push(u01id, "channel");
            return Json(new { ok = true });
        }

        [HttpPost]
        public IActionResult Leave(int u01id)
        {
            var members = Factory.ChatBL.GetMembers(u01id);     //včetně mě: i moje okna se mají dozvědět, že konverzace zmizela
            if (!Factory.ChatBL.Leave(u01id)) return Fail();
            Push(u01id, "channel", null, members);
            return Json(new { ok = true });
        }

        [HttpPost]
        public IActionResult RemoveMember(int u01id, int j02id)
        {
            var members = Factory.ChatBL.GetMembers(u01id);     //včetně odebíraného: jeho oknům má konverzace zmizet
            if (!Factory.ChatBL.RemoveMember(u01id, j02id)) return Fail();
            Push(u01id, "channel", null, members);
            return Json(new { ok = true });
        }

        [HttpPost]
        public IActionResult DeleteChannel(int u01id)
        {
            var members = Factory.ChatBL.GetMembers(u01id);     //načíst předem: po smazání už členy zjistit nejde
            if (!Factory.ChatBL.DeleteChannel(u01id)) return Fail();
            Push(u01id, "channel", null, members);
            return Json(new { ok = true });
        }

        [HttpPost]
        public IActionResult Mute(int u01id, bool value)
        {
            if (!Factory.ChatBL.SetMuted(u01id, value)) return Fail();
            return Json(new { ok = true, count = Factory.ChatBL.GetUnreadTotal() });
        }

        [HttpPost]
        public IActionResult Pin(int u01id, bool value)
        {
            if (!Factory.ChatBL.SetPinned(u01id, value)) return Fail();
            return Json(new { ok = true });
        }

        // ── přílohy ─────────────────────────────────────────────────────────────────

        private static string GetImageContentType(string strFileName)
        {
            switch (System.IO.Path.GetExtension(strFileName ?? "").ToLower())
            {
                case ".png": return "image/png";
                case ".gif": return "image/gif";
                case ".bmp": return "image/bmp";
                case ".webp": return "image/webp";
                default: return "image/jpeg";
            }
        }

        public IActionResult Attachment(int id, bool download)
        {
            var c = Factory.ChatBL.LoadAttachment(id);
            if (c == null) return NotFound();

            string strFullPath = Factory.UploadFolder + "\\" + c.o27ArchiveFolder + "\\" + c.o27ArchiveFileName;
            if (!System.IO.File.Exists(strFullPath)) return NotFound();

            bool bolInline = !download && _inlineImages.Contains(System.IO.Path.GetExtension(c.o27OriginalFileName ?? ""));
            Response.Headers["Content-Disposition"] = $"{(bolInline ? "inline" : "attachment")}; filename*=UTF-8''{Uri.EscapeDataString(c.o27OriginalFileName ?? "soubor")}";
            Response.Headers["X-Content-Type-Options"] = "nosniff";
            return PhysicalFile(strFullPath, bolInline ? GetImageContentType(c.o27OriginalFileName) : "application/octet-stream");
        }
    }
}
