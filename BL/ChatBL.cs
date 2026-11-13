using System.Collections.Concurrent;

namespace BL
{
    /// <summary>
    /// Události chatu, které vznikají mimo ChatController (např. zpráva o kroku workflow). UI vrstva si při startu
    /// zaregistruje obsluhu, která je přes SignalR doručí do prohlížečů - BL o SignalR neví.
    /// </summary>
    public static class ChatEvents
    {
        /// <summary>Parametry: přihlašovací jména členů konverzace, u01ID.</summary>
        public static Action<List<string>, int> ChannelChanged { get; set; }
    }

    /// <summary>
    /// Interní chat mezi uživateli: tabulky u01Channel, u02Message, u03ChannelMember, u04MessageReaction.
    /// Přílohy zpráv jsou v o27Attachment (o27Entity='u02').
    /// Každá metoda nejdříve ověřuje, že přihlášený uživatel je členem konverzace.
    /// </summary>
    public interface IChatBL
    {
        public bool IsAvailable();
        public bool HasPermission();
        public IEnumerable<BO.j02User> GetUsers();
        public IEnumerable<BO.Chat.ChannelItem> GetMyChannels();
        public BO.Chat.ChannelItem LoadMyChannel(int u01id);
        public int GetUnreadTotal();

        public int OpenDirect(int j02id_other);
        public int CreateGroup(string strName, List<int> j02ids);
        public int OpenRecordChannel(string entity, int pid);
        public int GetRecordMessagesCount(string entity, int pid);
        public int SaveMessageAsNote(int u02id);
        public void PostWorkflowMessage(BO.b05Workflow_History rec);
        public int PostNotification(List<string> emails, string strText);
        public List<BO.Chat.RoleItem> GetRecordRoles(int u01id);
        public bool RenameGroup(int u01id, string strName);
        public bool AddMembers(int u01id, List<int> j02ids);
        public bool Leave(int u01id);
        public bool RemoveMember(int u01id, int j02id);
        public bool DeleteChannel(int u01id);
        public bool SetMuted(int u01id, bool muted);
        public bool SetPinned(int u01id, bool pinned);
        public List<BO.Chat.MemberItem> GetMembers(int u01id, bool istrusted = false);

        public List<BO.Chat.MessageItem> GetMessages(int u01id, int u02id_before, int take = 50);
        public BO.Chat.MessageItem LoadMessage(int u02id);
        public int SendMessage(int u01id, string strText, int u02id_replyto, bool hasattachments);
        public bool EditMessage(int u02id, string strText);
        public bool DeleteMessage(int u02id);
        public bool ToggleReaction(int u02id, string strReaction);
        public bool MarkRead(int u01id, int u02id);

        public int AddAttachment(int u02id, string strOrigFileName, string strSourceFullPath, string strContentType);
        public BO.o27Attachment LoadAttachment(int o27id);
    }

    class ChatBL : BaseBL, IChatBL
    {
        public const int MaxTextLength = 4000;
        public static readonly string[] Reactions = new[] { "like", "heart", "laugh", "wow", "sad", "thanks", "done" };
        private static readonly string[] _recordEntities = new[] { "p41", "p56", "p28", "p91", "o23" };
        private static readonly ConcurrentDictionary<string, DateTime> _available = new ConcurrentDictionary<string, DateTime>();   //kde už u-tabulky jsou
        private static readonly ConcurrentDictionary<string, DateTime> _unavailable = new ConcurrentDictionary<string, DateTime>(); //kde nejsou (zkusit znovu za 5 minut)

        public ChatBL(BL.Factory mother) : base(mother)
        {

        }

        private int Me
        {
            get
            {
                return _mother.CurrentUser.pid;
            }
        }

        private static string NameSql(string strAlias)
        {
            return $"LTRIM(RTRIM(ISNULL({strAlias}.j02FirstName,'')+' '+ISNULL({strAlias}.j02LastName,'')))";
        }

        private int GetInt(string strSQL, object param)
        {
            var c = _db.Load<BO.GetInteger>(strSQL, param);
            return c == null ? 0 : c.Value;
        }

        /// <summary>Má přihlášený uživatel v aplikační roli oprávnění k chatu (GR_CHAT)?</summary>
        public bool HasPermission()
        {
            try
            {
                return _mother.CurrentUser != null && _mother.CurrentUser.TestPermission(BO.PermValEnum.GR_CHAT);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Je chat pro přihlášeného uživatele k dispozici? Musí mít oprávnění role GR_CHAT a v databázi licence musí být tabulky chatu.
        /// Existence tabulek se drží v paměti, aby se netestovala při každé stránce.
        /// </summary>
        public bool IsAvailable()
        {
            if (_mother.CurrentUser == null || _db == null) return false;
            if (!HasPermission()) return false;
            return TablesExist();
        }

        private bool TablesExist()
        {
            string strKey = $"{_mother.App.HostingMode}|{_mother.CurrentUser.x01LoginDomain}";
            if (_available.ContainsKey(strKey)) return true;
            if (_unavailable.TryGetValue(strKey, out DateTime d) && d > DateTime.Now) return false;

            if (GetInt("SELECT CASE WHEN OBJECT_ID('dbo.u01Channel') IS NULL OR OBJECT_ID('dbo.u02Message') IS NULL OR OBJECT_ID('dbo.u03ChannelMember') IS NULL OR OBJECT_ID('dbo.u04MessageReaction') IS NULL THEN 0 ELSE 1 END as Value", null) == 1)
            {
                _available[strKey] = DateTime.Now;
                return true;
            }
            _unavailable[strKey] = DateTime.Now.AddMinutes(5);
            return false;
        }

        /// <summary>
        /// Uživatelé licence, se kterými lze chatovat: platný účet s přihlášením, bez virtuálních osob a bez sebe sama.
        /// Nabízejí se jen ti, jejichž aplikační role má oprávnění k chatu - jinak by zprávy chodily někomu, kdo je nemůže číst.
        /// </summary>
        public IEnumerable<BO.j02User> GetUsers()
        {
            return ChatUsers().Where(p => p.pid != Me).ToList();
        }

        /// <summary>Všichni uživatelé licence, kteří mohou chat používat - včetně přihlášeného (upozornění systému chodí i jemu).</summary>
        private List<BO.j02User> ChatUsers()
        {
            var lis = _mother.j02UserBL.GetList(new BO.myQueryJ02());
            if (lis == null) return new List<BO.j02User>();

            int intPos = (int)BO.PermValEnum.GR_CHAT;
            var lisAllowed = _db.GetList<BO.GetInteger>($"SELECT a.j02ID as Value FROM j02User a INNER JOIN j04UserRole b ON a.j04ID=b.j04ID INNER JOIN x67EntityRole c ON b.x67ID=c.x67ID WHERE SUBSTRING(c.x67RoleValue,{intPos},1)='1'");
            var allowed = lisAllowed == null ? new HashSet<int>() : new HashSet<int>(lisAllowed.Select(p => p.Value));

            return lis.Where(p => allowed.Contains(p.pid) && !p.isclosed && !string.IsNullOrEmpty(p.j02Login) && !p.j02IsLoginManualLocked && p.j02VirtualParentID == 0)
                .OrderBy(p => p.j02LastName).ThenBy(p => p.j02FirstName).ToList();
        }

        private bool IsMember(int u01id)
        {
            if (u01id <= 0) return false;
            return GetInt("SELECT COUNT(*) as Value FROM u03ChannelMember a INNER JOIN u01Channel b ON a.u01ID=b.u01ID WHERE a.u01ID=@u01id AND a.j02ID=@me AND a.u03DateLeft IS NULL AND b.u01ValidUntil>GETDATE()", new { u01id = u01id, me = Me }) > 0;
        }

        // ── konverzace ──────────────────────────────────────────────────────────────

        private IEnumerable<BO.Chat.ChannelItem> GetChannels(int u01id)
        {
            sb("SELECT a.u01ID as pid,a.u01TypeFlag,a.u01Name,a.u01RecordEntity,a.u01RecordPid,a.j02ID_Owner,a.u01DateLastMessage,a.u01DateInsert");
            sb(",m.u03IsMuted,m.u03IsPinned,m.u03RoleFlag,m.u02ID_LastRead");
            sb(",(SELECT COUNT(*) FROM u02Message x WHERE x.u01ID=a.u01ID AND x.u02ID>ISNULL(m.u02ID_LastRead,0) AND x.u02IsDeleted=0 AND ((x.u02TypeFlag=1 AND x.j02ID_Sender<>@me) OR x.u02TypeFlag=3)) as UnreadCount");
            sb(",(SELECT COUNT(*) FROM u03ChannelMember y WHERE y.u01ID=a.u01ID AND y.u03DateLeft IS NULL) as MembersCount");
            sb($",pm.j02ID as PartnerID,{NameSql("op")} as PartnerName,pm.u02ID_LastRead as PartnerLastReadID");
            sb($",CASE WHEN lm.u02TypeFlag=3 THEN 0 ELSE lm.j02ID_Sender END as LastSenderID,CASE WHEN lm.u02TypeFlag=3 THEN NULL ELSE {NameSql("ls")} END as LastSenderName,lm.u02Text200 as LastText,lm.u02IsDeleted as LastIsDeleted,lm.u02AttachmentsCount as LastAttachmentsCount");
            sb(" FROM u01Channel a INNER JOIN u03ChannelMember m ON a.u01ID=m.u01ID AND m.j02ID=@me AND m.u03DateLeft IS NULL");
            sb(" LEFT OUTER JOIN u02Message lm ON a.u02ID_Last=lm.u02ID");
            sb(" LEFT OUTER JOIN j02User ls ON lm.j02ID_Sender=ls.j02ID");
            sb(" LEFT OUTER JOIN u03ChannelMember pm ON a.u01TypeFlag=1 AND pm.u01ID=a.u01ID AND pm.j02ID<>@me");
            sb(" LEFT OUTER JOIN j02User op ON pm.j02ID=op.j02ID");
            sb(" WHERE a.u01ValidUntil>GETDATE()");
            if (u01id > 0)
            {
                sb(" AND a.u01ID=@u01id");
            }
            sb(" ORDER BY m.u03IsPinned DESC,ISNULL(a.u01DateLastMessage,a.u01DateInsert) DESC");

            var lis = _db.GetList<BO.Chat.ChannelItem>(sbret(), new { me = Me, u01id = u01id });
            return lis ?? new List<BO.Chat.ChannelItem>();
        }

        public IEnumerable<BO.Chat.ChannelItem> GetMyChannels()
        {
            return GetChannels(0);
        }
        public BO.Chat.ChannelItem LoadMyChannel(int u01id)
        {
            if (u01id <= 0) return null;
            return GetChannels(u01id).FirstOrDefault();
        }

        public int GetUnreadTotal()
        {
            sb("SELECT COUNT(*) as Value FROM u02Message x INNER JOIN u03ChannelMember m ON x.u01ID=m.u01ID INNER JOIN u01Channel a ON m.u01ID=a.u01ID");
            sb(" WHERE m.j02ID=@me AND m.u03DateLeft IS NULL AND m.u03IsMuted=0 AND a.u01ValidUntil>GETDATE()");
            sb(" AND x.u02ID>ISNULL(m.u02ID_LastRead,0) AND x.u02IsDeleted=0 AND ((x.u02TypeFlag=1 AND x.j02ID_Sender<>@me) OR x.u02TypeFlag=3)");    //upozornění (typ 3) je nepřečtené pro každého, i když ho robot zapsal pod účtem příjemce
            return GetInt(sbret(), new { me = Me });
        }

        private void AddMember(int u01id, int j02id, int intRoleFlag)
        {
            sb("IF EXISTS(SELECT 1 FROM u03ChannelMember WHERE u01ID=@u01id AND j02ID=@j02id)");
            sb(" UPDATE u03ChannelMember SET u03DateLeft=NULL,u02ID_LastRead=(SELECT u02ID_Last FROM u01Channel WHERE u01ID=@u01id) WHERE u01ID=@u01id AND j02ID=@j02id AND u03DateLeft IS NOT NULL");
            sb(" ELSE");
            sb(" INSERT INTO u03ChannelMember(u01ID,j02ID,u03RoleFlag,u02ID_LastRead,u03UserInsert,u03DateInsert)");
            sb(" SELECT @u01id,@j02id,@role,(SELECT u02ID_Last FROM u01Channel WHERE u01ID=@u01id),@login,GETDATE()");    //nový člen nemá starší zprávy jako nepřečtené
            _db.RunSql(sbret(), new { u01id = u01id, j02id = j02id, role = intRoleFlag, login = _mother.CurrentUser.j02Login });
        }

        private int SaveChannel(BO.u01Channel rec)
        {
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddInt("x01ID", _mother.CurrentUser.x01ID, true);
            p.AddEnumInt("u01TypeFlag", rec.u01TypeFlag);
            p.AddString("u01Name", rec.u01Name);
            p.AddString("u01DirectKey", rec.u01DirectKey);
            p.AddString("u01RecordEntity", rec.u01RecordEntity);
            p.AddInt("u01RecordPid", rec.u01RecordPid, true);
            p.AddInt("j02ID_Owner", Me, true);
            return _db.SaveRecord("u01Channel", p, rec);
        }

        public int OpenDirect(int j02id_other)
        {
            if (j02id_other <= 0 || j02id_other == Me || !GetUsers().Any(p => p.pid == j02id_other))
            {
                this.AddMessage("S tímto uživatelem nelze zahájit konverzaci."); return 0;
            }
            string strKey = $"{Math.Min(Me, j02id_other)}-{Math.Max(Me, j02id_other)}";
            int intU01ID = GetInt("SELECT TOP 1 u01ID as Value FROM u01Channel WHERE x01ID=@x01id AND u01DirectKey=@k", new { x01id = _mother.CurrentUser.x01ID, k = strKey });
            if (intU01ID > 0)
            {
                return intU01ID;
            }

            intU01ID = SaveChannel(new BO.u01Channel() { u01TypeFlag = BO.u01TypeFlagENUM.Direct, u01DirectKey = strKey });
            if (intU01ID == 0)
            {
                //souběh: druhá strana založila stejnou konverzaci ve stejný okamžik
                return GetInt("SELECT TOP 1 u01ID as Value FROM u01Channel WHERE x01ID=@x01id AND u01DirectKey=@k", new { x01id = _mother.CurrentUser.x01ID, k = strKey });
            }
            AddMember(intU01ID, Me, 1);
            AddMember(intU01ID, j02id_other, 1);
            return intU01ID;
        }

        public int CreateGroup(string strName, List<int> j02ids)
        {
            strName = (strName ?? "").Trim();
            if (strName == "")
            {
                this.AddMessage("Chybí název skupiny."); return 0;
            }
            if (strName.Length > 200) strName = strName.Substring(0, 200);

            var users = GetUsers().Select(p => p.pid).ToList();
            var ids = (j02ids ?? new List<int>()).Where(p => users.Contains(p)).Distinct().ToList();
            if (ids.Count == 0)
            {
                this.AddMessage("Vyberte alespoň jednoho člena skupiny."); return 0;
            }

            int intU01ID = SaveChannel(new BO.u01Channel() { u01TypeFlag = BO.u01TypeFlagENUM.Group, u01Name = strName });
            if (intU01ID == 0) return 0;

            AddMember(intU01ID, Me, 2);
            foreach (int j02id in ids)
            {
                AddMember(intU01ID, j02id, 1);
            }
            SaveSystemMessage(intU01ID, $"{MyName()} založil(a) skupinu.");
            return intU01ID;
        }

        private bool HasRecordReadAccess(string entity, int pid)
        {
            switch (entity)
            {
                case "p41": return _mother.p41ProjectBL.InhaleRecDisposition(pid).ReadAccess;
                case "p56": return _mother.p56TaskBL.InhaleRecDisposition(pid).ReadAccess;
                case "p28": return _mother.p28ContactBL.InhaleRecDisposition(pid).ReadAccess;
                case "p91": return _mother.p91InvoiceBL.InhaleRecDisposition(pid).ReadAccess;
                case "o23": return _mother.o23DocBL.InhaleRecDisposition(pid).ReadAccess;
                default: return false;
            }
        }

        /// <summary>Konverzace k záznamu: kdo má právo záznam číst, může se připojit.</summary>
        /// <summary>Projektové úrovně le1-le5 jsou z pohledu chatu projekt (p41).</summary>
        private static string NormalizeEntity(string entity)
        {
            entity = (entity ?? "").ToLower();
            return (entity.Length == 3 && entity.StartsWith("le")) ? "p41" : entity;
        }

        /// <summary>Počet zpráv v konverzaci k záznamu - pro ikonu chatu u záznamu. Vrací 0, pokud konverzace neexistuje.</summary>
        public int GetRecordMessagesCount(string entity, int pid)
        {
            entity = NormalizeEntity(entity);
            if (pid <= 0 || !_recordEntities.Contains(entity)) return 0;
            sb("SELECT COUNT(*) as Value FROM u02Message a INNER JOIN u01Channel b ON a.u01ID=b.u01ID");
            sb(" WHERE b.x01ID=@x01id AND b.u01RecordEntity=@e AND b.u01RecordPid=@pid AND b.u01ValidUntil>GETDATE() AND a.u02TypeFlag=1 AND a.u02IsDeleted=0");
            return GetInt(sbret(), new { x01id = _mother.CurrentUser.x01ID, e = entity, pid = pid });
        }

        /// <summary>Zprávu z konverzace k záznamu uloží jako poznámku záznamu (b05Workflow_History). Vrací b05ID.</summary>
        public int SaveMessageAsNote(int u02id)
        {
            var msg = LoadMessage(u02id);   //ověřuje členství
            if (msg == null || msg.u02IsDeleted || msg.u02TypeFlag != 1 || (string.IsNullOrWhiteSpace(msg.u02Text) && msg.Attachments.Count == 0))
            {
                this.AddMessage("Tuto zprávu nelze uložit jako poznámku."); return 0;
            }
            var rec = LoadChannel(msg.u01ID);
            if (rec == null || rec.u01TypeFlag != BO.u01TypeFlagENUM.Record || rec.u01RecordPid == 0)
            {
                this.AddMessage("Jako poznámku lze uložit pouze zprávu z konverzace k záznamu."); return 0;
            }
            bool bolAccess = false;
            try
            {
                bolAccess = HasRecordReadAccess(rec.u01RecordEntity, rec.u01RecordPid);
            }
            catch
            {
                bolAccess = false;
            }
            if (!bolAccess)
            {
                this.AddMessage("K záznamu nemáte přístup."); return 0;
            }

            string strText = System.Net.WebUtility.HtmlEncode(msg.u02Text ?? "").Replace("\r\n", "<br>").Replace("\n", "<br>");
            string strHtml = $"<p><em>{_mother.tra("Z chatu")} · {System.Net.WebUtility.HtmlEncode(msg.SenderName)} · {msg.u02DateInsert.ToString("d.M.yyyy HH:mm")}</em></p>";
            if (strText != "") strHtml += $"<p>{strText}</p>";
            var recB05 = new BO.b05Workflow_History()
            {
                b05IsCommentOnly = true,
                b05IsManualStep = true,
                b05RecordEntity = rec.u01RecordEntity,
                b05RecordPid = rec.u01RecordPid,
                j02ID_Sys = Me,
                b05Notepad = strHtml,
                x04ID = _mother.Lic == null ? 0 : _mother.Lic.x04ID_Default
            };
            int intB05ID = _mother.WorkflowBL.Save2History(recB05);
            if (intB05ID > 0 && msg.Attachments.Count > 0)
            {
                CopyAttachmentsToNote(u02id, intB05ID);
            }
            return intB05ID;
        }

        /// <summary>Přílohy zprávy zkopíruje k poznámce (o27Entity='b05'). Soubory se kopírují, v chatu zůstávají.</summary>
        private void CopyAttachmentsToNote(int u02id, int b05id)
        {
            var lisO27 = _db.GetList<BO.o27Attachment>($"SELECT a.*,{_db.GetSQL1_Ocas("o27")} FROM o27Attachment a WHERE a.o27Entity='u02' AND a.o27RecordPid=@pid ORDER BY a.o27ID", new { pid = u02id });
            if (lisO27 == null) return;

            foreach (var c in lisO27)
            {
                try
                {
                    string strSource = _mother.UploadFolder + "\\" + c.o27ArchiveFolder + "\\" + c.o27ArchiveFileName;
                    if (!System.IO.File.Exists(strSource))
                    {
                        this.AddMessageWithPars("Přílohu [{0}] se nepodařilo zkopírovat k poznámce.", c.o27OriginalFileName, null, "warning"); continue;
                    }
                    var rec = new BO.o27Attachment() { o27Entity = "b05", o27RecordPid = b05id, o27ContentType = c.o27ContentType };
                    string strArchiveName = BO.Code.Bas.GetGuid() + System.IO.Path.GetExtension(c.o27OriginalFileName ?? "");
                    if (_mother.o27AttachmentBL.UploadAndSaveOneFile(rec, c.o27OriginalFileName, strSource, strArchiveName) == 0)
                    {
                        this.AddMessageWithPars("Přílohu [{0}] se nepodařilo zkopírovat k poznámce.", c.o27OriginalFileName, null, "warning");
                    }
                }
                catch (Exception e)
                {
                    BO.Code.File.LogError(e.Message, _mother.CurrentUser.j02Login, "ChatBL.CopyAttachmentsToNote");
                    this.AddMessageWithPars("Přílohu [{0}] se nepodařilo zkopírovat k poznámce.", c.o27OriginalFileName, null, "warning");
                }
            }
        }

        public int OpenRecordChannel(string entity, int pid)
        {
            entity = NormalizeEntity(entity);
            if (pid <= 0 || !_recordEntities.Contains(entity))
            {
                this.AddMessage("K tomuto typu záznamu nelze vést konverzaci."); return 0;
            }
            bool bolAccess = false;
            try
            {
                bolAccess = HasRecordReadAccess(entity, pid);
            }
            catch
            {
                bolAccess = false;
            }
            if (!bolAccess)
            {
                this.AddMessage("K záznamu nemáte přístup."); return 0;
            }

            int intU01ID = GetInt("SELECT TOP 1 u01ID as Value FROM u01Channel WHERE x01ID=@x01id AND u01RecordEntity=@e AND u01RecordPid=@pid AND u01ValidUntil>GETDATE()", new { x01id = _mother.CurrentUser.x01ID, e = entity, pid = pid });
            if (intU01ID == 0)
            {
                string strName = _mother.CBL.GetObjectAlias(entity, pid);
                if (string.IsNullOrWhiteSpace(strName)) strName = $"{entity.ToUpper()} #{pid}";
                if (strName.Length > 200) strName = strName.Substring(0, 200);
                intU01ID = SaveChannel(new BO.u01Channel() { u01TypeFlag = BO.u01TypeFlagENUM.Record, u01Name = strName, u01RecordEntity = entity, u01RecordPid = pid });
                if (intU01ID == 0) return 0;
            }
            if (!IsMember(intU01ID))
            {
                AddMember(intU01ID, Me, 1);
            }
            return intU01ID;
        }

        private BO.u01Channel LoadChannel(int u01id)
        {
            return _db.Load<BO.u01Channel>($"SELECT a.*,{_db.GetSQL1_Ocas("u01")} FROM u01Channel a WHERE a.u01ID=@pid", new { pid = u01id });
        }

        public bool RenameGroup(int u01id, string strName)
        {
            strName = (strName ?? "").Trim();
            if (strName == "" || !IsMember(u01id)) return false;
            if (strName.Length > 200) strName = strName.Substring(0, 200);
            var rec = LoadChannel(u01id);
            if (rec == null || rec.u01TypeFlag != BO.u01TypeFlagENUM.Group) return false;

            _db.RunSql("UPDATE u01Channel SET u01Name=@name,u01DateUpdate=GETDATE(),u01UserUpdate=@login WHERE u01ID=@pid", new { name = strName, login = _mother.CurrentUser.j02Login, pid = u01id });
            SaveSystemMessage(u01id, $"{MyName()} přejmenoval(a) skupinu na „{strName}“.");
            return true;
        }

        public bool AddMembers(int u01id, List<int> j02ids)
        {
            if (!IsMember(u01id)) return false;
            var rec = LoadChannel(u01id);
            if (rec == null || rec.u01TypeFlag == BO.u01TypeFlagENUM.Direct) return false;

            var users = GetUsers().ToList();
            var current = GetMembers(u01id, true).Select(p => p.j02ID).ToList();
            var added = users.Where(p => j02ids != null && j02ids.Contains(p.pid) && !current.Contains(p.pid)).ToList();
            if (added.Count == 0) return false;

            foreach (var c in added)
            {
                AddMember(u01id, c.pid, 1);
            }
            SaveSystemMessage(u01id, $"{MyName()} přidal(a): {string.Join(", ", added.Select(p => $"{p.j02FirstName} {p.j02LastName}".Trim()))}.");
            return true;
        }

        public bool Leave(int u01id)
        {
            if (!IsMember(u01id)) return false;
            var rec = LoadChannel(u01id);
            if (rec == null || rec.u01TypeFlag == BO.u01TypeFlagENUM.Direct) return false;

            SaveSystemMessage(u01id, $"{MyName()} opustil(a) konverzaci.");
            _db.RunSql("UPDATE u03ChannelMember SET u03DateLeft=GETDATE() WHERE u01ID=@u01id AND j02ID=@me", new { u01id = u01id, me = Me });
            if (GetInt("SELECT COUNT(*) as Value FROM u03ChannelMember WHERE u01ID=@u01id AND u03DateLeft IS NULL", new { u01id = u01id }) == 0)
            {
                _db.RunSql("UPDATE u01Channel SET u01ValidUntil=GETDATE() WHERE u01ID=@u01id", new { u01id = u01id });    //bez členů: konverzaci uzavřít
            }
            return true;
        }

        /// <summary>Odebrat člena smí jen správce skupiny nebo zakladatel konverzace. Sám sebe odebrat nelze - k tomu slouží Leave.</summary>
        public bool RemoveMember(int u01id, int j02id)
        {
            if (j02id <= 0 || j02id == Me || !IsMember(u01id)) return false;
            var rec = LoadChannel(u01id);
            if (rec == null || rec.u01TypeFlag == BO.u01TypeFlagENUM.Direct) return false;

            var members = GetMembers(u01id, true);
            var me = members.FirstOrDefault(p => p.j02ID == Me);
            var target = members.FirstOrDefault(p => p.j02ID == j02id);
            if (target == null) return false;
            if (rec.j02ID_Owner != Me && (me == null || me.u03RoleFlag != 2))
            {
                this.AddMessage("Odebírat členy může pouze správce konverzace."); return false;
            }

            _db.RunSql("UPDATE u03ChannelMember SET u03DateLeft=GETDATE() WHERE u01ID=@u01id AND j02ID=@j02id", new { u01id = u01id, j02id = j02id });
            SaveSystemMessage(u01id, $"{MyName()} odebral(a): {target.PersonName}.");
            return true;
        }

        /// <summary>
        /// Smazat konverzaci smí jen správce skupiny nebo zakladatel konverzace; přímou konverzaci 1:1 smazat nelze.
        /// Konverzace se uzavře (u01ValidUntil) a zmizí všem členům, zprávy v databázi zůstávají. Soubory příloh se přesunou do složky DELETED.
        /// </summary>
        public bool DeleteChannel(int u01id)
        {
            if (!IsMember(u01id)) return false;
            var rec = LoadChannel(u01id);
            if (rec == null || rec.u01TypeFlag == BO.u01TypeFlagENUM.Direct) return false;

            var me = GetMembers(u01id, true).FirstOrDefault(p => p.j02ID == Me);
            if (rec.j02ID_Owner != Me && (me == null || me.u03RoleFlag != 2))
            {
                this.AddMessage("Smazat konverzaci může pouze její správce."); return false;
            }

            var lisO27 = _db.GetList<BO.o27Attachment>($"SELECT a.*,{_db.GetSQL1_Ocas("o27")} FROM o27Attachment a INNER JOIN u02Message b ON a.o27RecordPid=b.u02ID WHERE a.o27Entity='u02' AND b.u01ID=@u01id AND b.u02IsDeleted=0", new { u01id = u01id });
            if (lisO27 != null)
            {
                foreach (var c in lisO27)
                {
                    try
                    {
                        _mother.o27AttachmentBL.Move2Deleted(c);
                    }
                    catch
                    {
                        //soubor na disku už není
                    }
                }
            }

            _db.RunSql("UPDATE u01Channel SET u01ValidUntil=GETDATE(),u01DateUpdate=GETDATE(),u01UserUpdate=@login WHERE u01ID=@u01id", new { login = _mother.CurrentUser.j02Login, u01id = u01id });
            return true;
        }

        public bool SetMuted(int u01id, bool muted)
        {
            if (!IsMember(u01id)) return false;
            _db.RunSql("UPDATE u03ChannelMember SET u03IsMuted=@b WHERE u01ID=@u01id AND j02ID=@me", new { b = muted, u01id = u01id, me = Me });
            return true;
        }
        public bool SetPinned(int u01id, bool pinned)
        {
            if (!IsMember(u01id)) return false;
            _db.RunSql("UPDATE u03ChannelMember SET u03IsPinned=@b WHERE u01ID=@u01id AND j02ID=@me", new { b = pinned, u01id = u01id, me = Me });
            return true;
        }

        /// <summary>istrusted=true pouze pro vnitřní použití po ověřené operaci (např. rozeslání oznámení).</summary>
        public List<BO.Chat.MemberItem> GetMembers(int u01id, bool istrusted = false)
        {
            if (!istrusted && !IsMember(u01id)) return new List<BO.Chat.MemberItem>();

            sb($"SELECT a.j02ID,b.j02Login,{NameSql("b")} as PersonName,a.u03RoleFlag,a.u03IsMuted,a.u02ID_LastRead");
            sb(" FROM u03ChannelMember a INNER JOIN j02User b ON a.j02ID=b.j02ID WHERE a.u01ID=@u01id AND a.u03DateLeft IS NULL ORDER BY b.j02LastName,b.j02FirstName");
            var lis = _db.GetList<BO.Chat.MemberItem>(sbret(), new { u01id = u01id });
            return lis == null ? new List<BO.Chat.MemberItem>() : lis.ToList();
        }

        private string MyName()
        {
            return $"{_mother.CurrentUser.j02FirstName} {_mother.CurrentUser.j02LastName}".Trim();
        }

        // ── zprávy ──────────────────────────────────────────────────────────────────

        private string GetMessageSql()
        {
            sb("a.u02ID as pid,a.u01ID,a.j02ID_Sender,a.u02TypeFlag,CASE WHEN a.u02IsDeleted=1 THEN NULL ELSE a.u02Text END as u02Text,a.u02ID_ReplyTo");
            sb(",CASE WHEN a.u02IsDeleted=1 THEN 0 ELSE a.u02AttachmentsCount END as u02AttachmentsCount,a.u02IsEdited,a.u02IsDeleted,a.u02DateInsert");
            sb($",{NameSql("s")} as SenderName,CASE WHEN r.u02IsDeleted=1 THEN NULL ELSE r.u02Text200 END as ReplyText,{NameSql("rs")} as ReplySenderName");
            sb(" FROM u02Message a LEFT OUTER JOIN j02User s ON a.j02ID_Sender=s.j02ID");
            sb(" LEFT OUTER JOIN u02Message r ON a.u02ID_ReplyTo=r.u02ID LEFT OUTER JOIN j02User rs ON r.j02ID_Sender=rs.j02ID");
            return sbret();
        }

        private void FillDetails(List<BO.Chat.MessageItem> lis)
        {
            if (lis.Count == 0) return;
            string strIDs = string.Join(",", lis.Select(p => p.pid));   //čísla z databáze, bezpečné pro IN

            if (lis.Any(p => p.u02AttachmentsCount > 0))
            {
                var lisO27 = _db.GetList<BO.Chat.AttachmentItem>($"SELECT o27ID,o27RecordPid,o27OriginalFileName,o27FileSize,o27ContentType FROM o27Attachment WHERE o27Entity='u02' AND o27RecordPid IN ({strIDs}) ORDER BY o27ID");
                if (lisO27 != null)
                {
                    foreach (var c in lis.Where(p => p.u02AttachmentsCount > 0))
                    {
                        c.Attachments = lisO27.Where(p => p.o27RecordPid == c.pid).ToList();
                    }
                }
            }

            var lisU04 = _db.GetList<BO.Chat.ReactionItem>($"SELECT a.u02ID,a.j02ID,a.u04Emoji,{NameSql("b")} as PersonName FROM u04MessageReaction a INNER JOIN j02User b ON a.j02ID=b.j02ID WHERE a.u02ID IN ({strIDs}) ORDER BY a.u04ID");
            if (lisU04 != null && lisU04.Any())
            {
                foreach (var c in lis)
                {
                    c.Reactions = lisU04.Where(p => p.u02ID == c.pid).ToList();
                }
            }
        }

        public List<BO.Chat.MessageItem> GetMessages(int u01id, int u02id_before, int take = 50)
        {
            if (!IsMember(u01id)) return new List<BO.Chat.MessageItem>();
            if (take <= 0 || take > 200) take = 50;

            string s = $"SELECT TOP {take} {GetMessageSql()} WHERE a.u01ID=@u01id AND (@before=0 OR a.u02ID<@before) ORDER BY a.u02ID DESC";
            var src = _db.GetList<BO.Chat.MessageItem>(s, new { u01id = u01id, before = u02id_before });
            var lis = src == null ? new List<BO.Chat.MessageItem>() : src.OrderBy(p => p.pid).ToList();
            FillDetails(lis);
            return lis;
        }

        public BO.Chat.MessageItem LoadMessage(int u02id)
        {
            if (u02id <= 0) return null;
            var c = _db.Load<BO.Chat.MessageItem>($"SELECT {GetMessageSql()} WHERE a.u02ID=@pid", new { pid = u02id });
            if (c == null || !IsMember(c.u01ID)) return null;
            FillDetails(new List<BO.Chat.MessageItem>() { c });
            return c;
        }

        private static string Text200(string s)
        {
            if (string.IsNullOrEmpty(s)) return null;
            s = System.Text.RegularExpressions.Regex.Replace(s, @"\s+", " ").Trim();
            return s.Length > 200 ? s.Substring(0, 199) + "…" : s;
        }

        private int SaveMessage(int u01id, string strText, int u02id_replyto, int intTypeFlag)
        {
            var rec = new BO.u02Message();
            var p = new DL.Params4Dapper();
            p.AddInt("pid", 0);
            p.AddInt("u01ID", u01id, true);
            p.AddInt("j02ID_Sender", Me, true);
            p.AddInt("u02TypeFlag", intTypeFlag);
            p.AddString("u02Text", strText);
            p.AddString("u02Text200", Text200(strText));
            p.AddInt("u02ID_ReplyTo", u02id_replyto, true);
            int intU02ID = _db.SaveRecord("u02Message", p, rec, false, true);
            if (intU02ID > 0)
            {
                _db.RunSql("UPDATE u01Channel SET u02ID_Last=@u02id,u01DateLastMessage=GETDATE() WHERE u01ID=@u01id", new { u02id = intU02ID, u01id = u01id });
                _db.RunSql("UPDATE u03ChannelMember SET u02ID_LastRead=@u02id,u03DateLastRead=GETDATE() WHERE u01ID=@u01id AND j02ID=@me", new { u02id = intU02ID, u01id = u01id, me = Me });
            }
            return intU02ID;
        }

        private int SaveSystemMessage(int u01id, string strText)
        {
            return SaveMessage(u01id, strText, 0, 2);
        }

        // ── zprávy od systému: workflow ─────────────────────────────────────────────

        /// <summary>
        /// Krok workflow nebo změnu stavu záznamu oznámí v konverzaci k záznamu, pokud taková konverzace existuje.
        /// Volá se z WorkflowBL.Save2History. Samotné poznámky (komentáře bez kroku) se do chatu nepíší.
        /// Chyba chatu nesmí nikdy shodit workflow.
        /// </summary>
        public void PostWorkflowMessage(BO.b05Workflow_History rec)
        {
            try
            {
                if (rec == null || rec.b05IsCommentOnly || rec.b05RecordPid <= 0 || _mother.CurrentUser == null || _db == null) return;
                if (rec.b06ID == 0 && rec.b02ID_To == 0) return;
                string strEntity = NormalizeEntity(rec.b05RecordEntity);
                if (!_recordEntities.Contains(strEntity) || !TablesExist()) return;

                int intU01ID = GetInt("SELECT TOP 1 u01ID as Value FROM u01Channel WHERE x01ID=@x01id AND u01RecordEntity=@e AND u01RecordPid=@pid AND u01ValidUntil>GETDATE()", new { x01id = _mother.CurrentUser.x01ID, e = strEntity, pid = rec.b05RecordPid });
                if (intU01ID == 0) return;

                string strStep = rec.b06ID > 0 ? GetName("SELECT b06Name as Value FROM b06WorkflowStep WHERE b06ID=@pid", rec.b06ID) : null;
                string strFrom = rec.b02ID_From > 0 ? GetName("SELECT b02Name as Value FROM b02WorkflowStatus WHERE b02ID=@pid", rec.b02ID_From) : null;
                string strTo = rec.b02ID_To > 0 ? GetName("SELECT b02Name as Value FROM b02WorkflowStatus WHERE b02ID=@pid", rec.b02ID_To) : null;

                string strWho = MyName();
                if (strWho == "") strWho = _mother.CurrentUser.j02Login;
                string strText = string.IsNullOrEmpty(strStep) ? $"{strWho} změnil(a) stav" : $"{strWho} provedl(a) krok „{strStep}“";
                if (!string.IsNullOrEmpty(strTo))
                {
                    strText += string.IsNullOrEmpty(strFrom) ? $": {strTo}" : $": {strFrom} → {strTo}";
                }
                strText += ".";
                string strNote = Text200(BO.Code.Bas.Html2Text(rec.b05Notepad));
                if (!string.IsNullOrEmpty(strNote))
                {
                    strText += " " + strNote;
                }

                if (SaveSystemMessage(intU01ID, strText) > 0)
                {
                    var logins = GetMembers(intU01ID, true).Where(p => !string.IsNullOrEmpty(p.j02Login)).Select(p => p.j02Login).ToList();
                    ChatEvents.ChannelChanged?.Invoke(logins, intU01ID);
                }
            }
            catch (Exception e)
            {
                BO.Code.File.LogError(e.Message, _mother.CurrentUser?.j02Login, "ChatBL.PostWorkflowMessage");
            }
        }

        // ── upozornění od systému (hlídače) ─────────────────────────────────────────

        /// <summary>Název osobní konverzace, do které chodí upozornění systému.</summary>
        public const string NotifyChannelName = "MARKTIME upozornění";

        /// <summary>
        /// Upozornění systému (hlídač) do interního chatu: každý příjemce ho dostane do své osobní konverzace „MARKTIME upozornění“
        /// (u01TypeFlag=2, u01DirectKey='notify-{j02ID}', jediný člen), zpráva má u02TypeFlag=3 a počítá se jako nepřečtená.
        /// Příjemci se zadávají e-mailem (stejný seznam jako pro poštu); zprávu dostanou ti, komu e-mail patří (j02Email)
        /// a kdo má v aplikační roli chat (GR_CHAT). Volá se z robota - odesílatel je účet robota, v chatu se nezobrazuje.
        /// Příjemce může upozornění i celou konverzaci smazat (DeleteMessage, DeleteChannel).
        /// Chyba chatu nesmí shodit volajícího. Vrací počet uživatelů, kterým zpráva přišla.
        /// </summary>
        public int PostNotification(List<string> emails, string strText)
        {
            int intCount = 0;
            try
            {
                if (emails == null || string.IsNullOrWhiteSpace(strText) || _mother.CurrentUser == null || _db == null || !TablesExist()) return 0;
                var wanted = new HashSet<string>(emails.Where(p => !string.IsNullOrWhiteSpace(p))
                    .SelectMany(p => p.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)).Select(p => p.Trim().ToLower()).Where(p => p != ""));
                if (wanted.Count == 0) return 0;

                strText = strText.Trim();
                if (strText.Length > MaxTextLength) strText = strText.Substring(0, MaxTextLength - 1) + "…";

                var users = ChatUsers().Where(p => !string.IsNullOrWhiteSpace(p.j02Email) && wanted.Contains(p.j02Email.Trim().ToLower())).ToList();
                foreach (var u in users)
                {
                    int intU01ID = OpenNotifyChannel(u.pid);
                    if (intU01ID == 0) continue;

                    var p = new DL.Params4Dapper();
                    p.AddInt("pid", 0);
                    p.AddInt("u01ID", intU01ID, true);
                    p.AddInt("j02ID_Sender", Me, true);
                    p.AddInt("u02TypeFlag", 3);
                    p.AddString("u02Text", strText);
                    p.AddString("u02Text200", Text200(strText));
                    int intU02ID = _db.SaveRecord("u02Message", p, new BO.u02Message(), false, true);
                    if (intU02ID == 0) continue;

                    //u03ChannelMember.u02ID_LastRead se nemění: zpráva má zůstat nepřečtená i tehdy, když je příjemcem účet robota
                    _db.RunSql("UPDATE u01Channel SET u02ID_Last=@u02id,u01DateLastMessage=GETDATE() WHERE u01ID=@u01id", new { u02id = intU02ID, u01id = intU01ID });
                    intCount += 1;
                    ChatEvents.ChannelChanged?.Invoke(new List<string> { u.j02Login }, intU01ID);
                }
            }
            catch (Exception e)
            {
                BO.Code.File.LogError(e.Message, _mother.CurrentUser?.j02Login, "ChatBL.PostNotification");
            }
            return intCount;
        }

        /// <summary>
        /// Osobní konverzace uživatele pro upozornění: najde ji, nebo založí. Příjemce je jejím správcem (u03RoleFlag=2) - smí ji smazat
        /// i mazat jednotlivá upozornění. Po smazání se při dalším upozornění založí nová, prázdná; opuštěná se znovu připojí.
        /// </summary>
        private int OpenNotifyChannel(int j02id)
        {
            string strKey = $"notify-{j02id}";
            int intU01ID = GetInt("SELECT TOP 1 u01ID as Value FROM u01Channel WHERE x01ID=@x01id AND u01DirectKey=@k AND u01ValidUntil>GETDATE()", new { x01id = _mother.CurrentUser.x01ID, k = strKey });
            if (intU01ID == 0)
            {
                //smazaná konverzace si klíč nesmí držet (jedinečný index x01ID+u01DirectKey) - nová začne prázdná
                _db.RunSql("UPDATE u01Channel SET u01DirectKey=NULL WHERE x01ID=@x01id AND u01DirectKey=@k AND u01ValidUntil<=GETDATE()", new { x01id = _mother.CurrentUser.x01ID, k = strKey });
                intU01ID = SaveChannel(new BO.u01Channel() { u01TypeFlag = BO.u01TypeFlagENUM.Group, u01Name = NotifyChannelName, u01DirectKey = strKey });
                if (intU01ID == 0)
                {
                    //souběh: konverzaci mezitím založil jiný běh
                    intU01ID = GetInt("SELECT TOP 1 u01ID as Value FROM u01Channel WHERE x01ID=@x01id AND u01DirectKey=@k AND u01ValidUntil>GETDATE()", new { x01id = _mother.CurrentUser.x01ID, k = strKey });
                    if (intU01ID == 0) return 0;
                }
            }
            AddMember(intU01ID, j02id, 2);
            //konverzace založené dřív měly příjemce jako běžného člena
            _db.RunSql("UPDATE u03ChannelMember SET u03RoleFlag=2 WHERE u01ID=@u01id AND j02ID=@j02id AND u03RoleFlag<>2", new { u01id = intU01ID, j02id = j02id });
            return intU01ID;
        }

        private string GetName(string strSQL, int pid)
        {
            var c = _db.Load<BO.GetString>(strSQL, new { pid = pid });
            return c == null ? null : c.Value;
        }

        // ── role v záznamu ──────────────────────────────────────────────────────────

        private class TeamPerson
        {
            public int j11ID { get; set; }
            public int j02ID { get; set; }
        }

        /// <summary>
        /// Obsazené role v záznamu, ke kterému konverzace patří - nabídka při přidávání členů.
        /// U každé role jsou jen uživatelé, kteří mohou chat používat a v konverzaci ještě nejsou.
        /// </summary>
        public List<BO.Chat.RoleItem> GetRecordRoles(int u01id)
        {
            var ret = new List<BO.Chat.RoleItem>();
            if (!IsMember(u01id)) return ret;
            var rec = LoadChannel(u01id);
            if (rec == null || rec.u01TypeFlag != BO.u01TypeFlagENUM.Record || rec.u01RecordPid == 0) return ret;

            IEnumerable<BO.x69EntityRole_Assign> lisX69 = null;
            try
            {
                //včetně rolí zděděných z klienta nebo střediska, jak je vrací BL jednotlivých entit
                switch (rec.u01RecordEntity)
                {
                    case "p41":
                        var recP41 = _mother.p41ProjectBL.Load(rec.u01RecordPid);
                        if (recP41 != null) lisX69 = _mother.x67EntityRoleBL.GetList_X69_OneProject(recP41, false);
                        break;
                    case "p56":
                        var recP56 = _mother.p56TaskBL.Load(rec.u01RecordPid);
                        if (recP56 != null) lisX69 = _mother.x67EntityRoleBL.GetList_X69_OneTask(recP56, false);
                        break;
                    case "p28":
                        var recP28 = _mother.p28ContactBL.Load(rec.u01RecordPid);
                        if (recP28 != null) lisX69 = _mother.x67EntityRoleBL.GetList_X69_OneContact(recP28, false);
                        break;
                    case "p91":
                        var recP91 = _mother.p91InvoiceBL.Load(rec.u01RecordPid);
                        if (recP91 != null) lisX69 = _mother.x67EntityRoleBL.GetList_X69_OneInvoice(recP91, false);
                        break;
                    case "o23":
                        var recO23 = _mother.o23DocBL.Load(rec.u01RecordPid);
                        if (recO23 != null) lisX69 = _mother.x67EntityRoleBL.GetList_X69_OneDoc(recO23, false);
                        break;
                }
            }
            catch
            {
                lisX69 = null;
            }
            if (lisX69 == null)
            {
                lisX69 = _mother.x67EntityRoleBL.GetList_X69(rec.u01RecordEntity, rec.u01RecordPid);
            }
            if (lisX69 == null) return ret;

            var users = GetUsers().Select(p => p.pid).ToList();     //kdo může chat používat (bez mě)
            var members = GetMembers(u01id, true).Select(p => p.j02ID).ToList();
            var candidates = users.Where(p => !members.Contains(p)).ToList();
            if (candidates.Count == 0) return ret;

            var j11ids = lisX69.Where(p => p.j11ID > 0).Select(p => p.j11ID).Distinct().ToList();
            var lisJ12 = new List<TeamPerson>();
            if (j11ids.Count > 0)
            {
                var x = _db.GetList<TeamPerson>($"SELECT j11ID,j02ID FROM j12Team_Person WHERE j11ID IN ({string.Join(",", j11ids)})");
                if (x != null) lisJ12 = x.ToList();
            }

            foreach (var grp in lisX69.Where(p => !string.IsNullOrEmpty(p.x67Name)).GroupBy(p => p.x67Name))
            {
                var ids = new List<int>();
                foreach (var c in grp)
                {
                    if (c.x69IsAllUsers) ids.AddRange(candidates);
                    if (c.j02ID > 0) ids.Add(c.j02ID);
                    if (c.j11ID > 0) ids.AddRange(lisJ12.Where(p => p.j11ID == c.j11ID).Select(p => p.j02ID));
                }
                ids = ids.Distinct().Where(p => candidates.Contains(p)).ToList();
                if (ids.Count > 0)
                {
                    ret.Add(new BO.Chat.RoleItem() { Name = grp.Key, j02IDs = ids });
                }
            }
            return ret.OrderBy(p => p.Name).ToList();
        }

        public int SendMessage(int u01id, string strText, int u02id_replyto, bool hasattachments)
        {
            if (!IsMember(u01id))
            {
                this.AddMessage("Nejste členem této konverzace."); return 0;
            }
            strText = (strText ?? "").Trim();
            if (strText == "" && !hasattachments)
            {
                this.AddMessage("Zpráva je prázdná."); return 0;
            }
            if (strText.Length > MaxTextLength)
            {
                this.AddMessageWithPars("Zpráva může mít nejvýše {0} znaků.", MaxTextLength.ToString()); return 0;
            }
            if (u02id_replyto > 0 && GetInt("SELECT COUNT(*) as Value FROM u02Message WHERE u02ID=@pid AND u01ID=@u01id", new { pid = u02id_replyto, u01id = u01id }) == 0)
            {
                u02id_replyto = 0;  //odpovídat lze jen na zprávu ze stejné konverzace
            }
            return SaveMessage(u01id, strText == "" ? null : strText, u02id_replyto, 1);
        }

        /// <param name="fordelete">mazání: kromě vlastní zprávy smí uživatel smazat i upozornění systému (typ 3) ve své konverzaci „MARKTIME upozornění“</param>
        private BO.u02Message LoadMyOwnMessage(int u02id, bool fordelete = false)
        {
            var rec = _db.Load<BO.u02Message>("SELECT a.*,a.u02ID as pid FROM u02Message a WHERE a.u02ID=@pid", new { pid = u02id });
            if (rec == null || rec.u02IsDeleted || !IsMember(rec.u01ID)) return null;
            if (rec.u02TypeFlag == 3)
            {
                return fordelete && IsMyNotifyChannel(rec.u01ID) ? rec : null;
            }
            if (rec.j02ID_Sender != Me || rec.u02TypeFlag != 1)
            {
                return null;
            }
            return rec;
        }

        /// <summary>Je konverzace osobní schránkou upozornění přihlášeného uživatele?</summary>
        private bool IsMyNotifyChannel(int u01id)
        {
            return GetInt("SELECT COUNT(*) as Value FROM u01Channel WHERE u01ID=@u01id AND u01DirectKey=@k", new { u01id = u01id, k = $"notify-{Me}" }) > 0;
        }

        public bool EditMessage(int u02id, string strText)
        {
            strText = (strText ?? "").Trim();
            var rec = LoadMyOwnMessage(u02id);
            if (rec == null) return false;
            if (strText == "" && rec.u02AttachmentsCount == 0)
            {
                this.AddMessage("Zpráva je prázdná."); return false;
            }
            if (strText.Length > MaxTextLength)
            {
                this.AddMessageWithPars("Zpráva může mít nejvýše {0} znaků.", MaxTextLength.ToString()); return false;
            }
            _db.RunSql("UPDATE u02Message SET u02Text=@t,u02Text200=@t200,u02IsEdited=1,u02DateUpdate=GETDATE(),u02UserUpdate=@login WHERE u02ID=@pid", new { t = strText == "" ? null : strText, t200 = Text200(strText), login = _mother.CurrentUser.j02Login, pid = u02id });
            return true;
        }

        public bool DeleteMessage(int u02id)
        {
            var rec = LoadMyOwnMessage(u02id, true);
            if (rec == null) return false;

            if (rec.u02AttachmentsCount > 0)
            {
                var lisO27 = _db.GetList<BO.o27Attachment>($"SELECT a.*,{_db.GetSQL1_Ocas("o27")} FROM o27Attachment a WHERE a.o27Entity='u02' AND a.o27RecordPid=@pid", new { pid = u02id });
                if (lisO27 != null)
                {
                    foreach (var c in lisO27)
                    {
                        try
                        {
                            _mother.o27AttachmentBL.Move2Deleted(c);    //soubor přesunout do složky DELETED
                        }
                        catch
                        {
                            //soubor na disku už není
                        }
                    }
                }
            }
            //zpráva fyzicky zůstává kvůli odpovědím, obsah se maže
            _db.RunSql("UPDATE u02Message SET u02IsDeleted=1,u02Text=NULL,u02Text200=NULL,u02DateUpdate=GETDATE(),u02UserUpdate=@login WHERE u02ID=@pid", new { login = _mother.CurrentUser.j02Login, pid = u02id });
            _db.RunSql("DELETE FROM u04MessageReaction WHERE u02ID=@pid", new { pid = u02id });
            return true;
        }

        public bool ToggleReaction(int u02id, string strReaction)
        {
            if (!Reactions.Contains(strReaction)) return false;
            var rec = _db.Load<BO.u02Message>("SELECT a.*,a.u02ID as pid FROM u02Message a WHERE a.u02ID=@pid", new { pid = u02id });
            if (rec == null || rec.u02IsDeleted || rec.u02TypeFlag != 1 || !IsMember(rec.u01ID)) return false;

            if (GetInt("SELECT COUNT(*) as Value FROM u04MessageReaction WHERE u02ID=@pid AND j02ID=@me AND u04Emoji=@e", new { pid = u02id, me = Me, e = strReaction }) > 0)
            {
                _db.RunSql("DELETE FROM u04MessageReaction WHERE u02ID=@pid AND j02ID=@me AND u04Emoji=@e", new { pid = u02id, me = Me, e = strReaction });
            }
            else
            {
                _db.RunSql("INSERT INTO u04MessageReaction(u02ID,j02ID,u04Emoji) VALUES(@pid,@me,@e)", new { pid = u02id, me = Me, e = strReaction });
            }
            return true;
        }

        public bool MarkRead(int u01id, int u02id)
        {
            if (u02id <= 0 || !IsMember(u01id)) return false;
            sb("UPDATE u03ChannelMember SET u02ID_LastRead=x.maxid,u03DateLastRead=GETDATE()");
            sb(" FROM u03ChannelMember INNER JOIN (SELECT MAX(u02ID) as maxid FROM u02Message WHERE u01ID=@u01id AND u02ID<=@u02id) x ON x.maxid IS NOT NULL");
            sb(" WHERE u03ChannelMember.u01ID=@u01id AND u03ChannelMember.j02ID=@me AND ISNULL(u03ChannelMember.u02ID_LastRead,0)<x.maxid");
            return _db.RunSql(sbret(), new { u01id = u01id, u02id = u02id, me = Me });
        }

        // ── přílohy ─────────────────────────────────────────────────────────────────

        public int AddAttachment(int u02id, string strOrigFileName, string strSourceFullPath, string strContentType)
        {
            var recU02 = LoadMyOwnMessage(u02id);
            if (recU02 == null) return 0;

            strOrigFileName = System.IO.Path.GetFileName(strOrigFileName ?? "");
            if (strOrigFileName == "") strOrigFileName = "soubor";
            if (strOrigFileName.Length > 200) strOrigFileName = strOrigFileName.Substring(strOrigFileName.Length - 200);

            var rec = new BO.o27Attachment() { o27Entity = "u02", o27RecordPid = u02id, o27ContentType = strContentType };
            string strArchiveName = BO.Code.Bas.GetGuid() + System.IO.Path.GetExtension(strOrigFileName);    //unikátní název na disku
            int intO27ID = _mother.o27AttachmentBL.UploadAndSaveOneFile(rec, strOrigFileName, strSourceFullPath, strArchiveName);
            if (intO27ID > 0)
            {
                _db.RunSql("UPDATE u02Message SET u02AttachmentsCount=(SELECT COUNT(*) FROM o27Attachment WHERE o27Entity='u02' AND o27RecordPid=@pid) WHERE u02ID=@pid", new { pid = u02id });
            }
            return intO27ID;
        }

        public BO.o27Attachment LoadAttachment(int o27id)
        {
            var rec = _mother.o27AttachmentBL.Load(o27id);
            if (rec == null || rec.o27Entity != "u02" || rec.o27RecordPid == 0) return null;
            var recU02 = _db.Load<BO.u02Message>("SELECT a.*,a.u02ID as pid FROM u02Message a WHERE a.u02ID=@pid", new { pid = rec.o27RecordPid });
            if (recU02 == null || recU02.u02IsDeleted || !IsMember(recU02.u01ID)) return null;
            return rec;
        }
    }
}
