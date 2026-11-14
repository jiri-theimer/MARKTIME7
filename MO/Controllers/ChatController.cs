using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MO.Models;

namespace MO.Controllers
{
    /// <summary>
    /// Interní chat v mobilní aplikaci. Stejná data a stejná BL vrstva (BL.ChatBL) jako okno chatu v UI.
    ///  /Chat/Index            - seznam konverzací
    ///  /Chat/Thread/123       - jedna konverzace
    /// Živé události jdou přes SignalR (MO.Code.ChatHub) a přes MO.Code.ChatRelay i uživatelům připojeným v UI.
    /// První verze umí: číst, psát, přikládat soubory, mazat vlastní zprávy a upozornění, založit přímou konverzaci.
    /// Skupiny, reakce a úpravy zpráv se zakládají a dělají v UI - tady se jen zobrazují.
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

        // ── oprávnění ───────────────────────────────────────────────────────────────

        /// <summary>Bez oprávnění GR_CHAT (nebo bez tabulek chatu) vrací datové akce JSON s chybou; stránky zobrazí vysvětlení.</summary>
        public override void OnActionExecuting(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            if (context.Result != null || Factory == null || Factory.CurrentUser == null) return;

            string strAction = context.RouteData.Values["action"]?.ToString();
            if (string.Equals(strAction, "Index", StringComparison.OrdinalIgnoreCase) || string.Equals(strAction, "Thread", StringComparison.OrdinalIgnoreCase)) return;
            if (!Factory.ChatBL.IsAvailable())
            {
                context.Result = Json(new { ok = false, count = 0, message = Factory.tra(Factory.ChatBL.HasPermission() ? "Chat není v této databázi k dispozici." : "K chatu nemáte oprávnění.") });
            }
        }

        // ── převod na JSON (stejné názvy jako v UI) ─────────────────────────────────

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

        /// <summary>Pošle událost všem členům konverzace: připojeným zde i (přes ChatRelay) v UI.</summary>
        private void Push(int u01id, string strType, object message = null, int u02id = 0)
        {
            try
            {
                var members = Factory.ChatBL.GetMembers(u01id, true).Where(p => !string.IsNullOrEmpty(p.j02Login)).ToList();
                var hub = (IHubContext<MO.Code.ChatHub>)HttpContext.RequestServices.GetService(typeof(IHubContext<MO.Code.ChatHub>));
                if (hub != null)
                {
                    foreach (var c in members)
                    {
                        _ = hub.Clients.Group(MO.Code.ChatHub.GroupName(c.j02Login)).SendAsync("chat", new
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

                MO.Code.ChatRelay.Send(HttpContext.RequestServices, members.Select(p => new MO.Code.ChatRelay.Target() { login = p.j02Login, muted = p.u03IsMuted }).ToList(), strType, u01id, u02id, Factory.CurrentUser.pid, message);
            }
            catch (Exception e)
            {
                BO.Code.File.LogError(e.Message, Factory.CurrentUser.j02Login, "MO.ChatController.Push");  // nedoručené oznámení nesmí shodit uložení zprávy
            }
        }

        // ── stránky ─────────────────────────────────────────────────────────────────

        public IActionResult Index()
        {
            var v = new ChatViewModel
            {
                PageTitle = Factory.tra("Chat"),
                HideHeaderTitle = true,
                IsAvailable = Factory.ChatBL.IsAvailable(),
                IsDenied = !Factory.ChatBL.HasPermission(),
                MyID = Factory.CurrentUser.pid
            };
            return View(v);
        }

        public IActionResult Thread(int id, int j02id)
        {
            if (!Factory.ChatBL.IsAvailable()) return RedirectToAction("Index");

            if (id == 0 && j02id > 0)
            {
                id = Factory.ChatBL.OpenDirect(j02id);
                if (id > 0) Push(id, "channel");
            }
            var c = id > 0 ? Factory.ChatBL.LoadMyChannel(id) : null;
            if (c == null)
            {
                SetMessage(Factory.tra("Konverzace neexistuje nebo k ní nemáte přístup."));
                return RedirectToAction("Index");
            }

            var v = new ChatViewModel
            {
                PageTitle = c.DisplayName,
                HideHeaderTitle = true,
                IsAvailable = true,
                MyID = Factory.CurrentUser.pid,
                ChannelID = c.pid,
                ChannelName = c.DisplayName,
                ChannelType = (int)c.u01TypeFlag,
                MembersCount = c.MembersCount
            };
            return View(v);
        }

        // ── data ────────────────────────────────────────────────────────────────────

        public IActionResult Channels()
        {
            return Json(new { ok = true, channels = Factory.ChatBL.GetMyChannels().Select(p => ToJson(p)) });
        }

        public IActionResult UnreadCount()
        {
            return Json(new { ok = true, count = Factory.ChatBL.GetUnreadTotal() });
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
                messages = lis.Select(p => ToJson(p)),
                hasmore = lis.Count == TAKE
            });
        }

        public IActionResult Users()
        {
            return Json(new { ok = true, users = Factory.ChatBL.GetUsers().Select(p => new { id = p.pid, name = $"{p.j02FirstName} {p.j02LastName}".Trim(), email = p.j02Email }) });
        }

        // ── zápisy ──────────────────────────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
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
                    BO.Code.File.LogError(e.Message, Factory.CurrentUser.j02Login, "MO.ChatController.Send");
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
                        // dočasný soubor uklidí robot
                    }
                }
            }

            var msg = Factory.ChatBL.LoadMessage(intU02ID);
            if (msg == null) return Fail();
            if (string.IsNullOrEmpty(msg.u02Text) && msg.Attachments.Count == 0)
            {
                Factory.ChatBL.DeleteMessage(intU02ID);     // zpráva byla jen z příloh a žádná se neuložila
                return Json(new { ok = false, message = Factory.tra("Přílohu se nepodařilo uložit.") });
            }

            var json = ToJson(msg);
            Push(u01id, "message", json, intU02ID);
            return Json(new { ok = true, message = json, rejected = rejected });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int u02id)
        {
            if (!Factory.ChatBL.DeleteMessage(u02id)) return Fail();
            var msg = Factory.ChatBL.LoadMessage(u02id);
            if (msg == null) return Json(new { ok = true });
            var json = ToJson(msg);
            Push(msg.u01ID, "update", json, u02id);
            return Json(new { ok = true, message = json });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Read(int u01id, int u02id)
        {
            if (Factory.ChatBL.MarkRead(u01id, u02id))
            {
                Push(u01id, "read", null, u02id);     // odesílatel uvidí "přečteno", moje ostatní okna si srovnají počet
            }
            return Json(new { ok = true, count = Factory.ChatBL.GetUnreadTotal() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Direct(int j02id)
        {
            int intU01ID = Factory.ChatBL.OpenDirect(j02id);
            if (intU01ID == 0) return Fail();
            Push(intU01ID, "channel");
            return Json(new { ok = true, u01id = intU01ID });
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
            var c = Factory.ChatBL.LoadAttachment(id);      // ověřuje členství v konverzaci
            if (c == null) return NotFound();

            string strFullPath = System.IO.Path.Combine(Factory.UploadFolder, c.o27ArchiveFolder, c.o27ArchiveFileName);
            if (!System.IO.File.Exists(strFullPath)) return NotFound();

            bool bolInline = !download && _inlineImages.Contains(System.IO.Path.GetExtension(c.o27OriginalFileName ?? ""));
            Response.Headers["Content-Disposition"] = $"{(bolInline ? "inline" : "attachment")}; filename*=UTF-8''{Uri.EscapeDataString(c.o27OriginalFileName ?? "soubor")}";
            Response.Headers["X-Content-Type-Options"] = "nosniff";
            return PhysicalFile(strFullPath, bolInline ? GetImageContentType(c.o27OriginalFileName) : "application/octet-stream");
        }
    }
}
