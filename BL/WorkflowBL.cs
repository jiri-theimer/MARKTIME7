
using BO;
using BO.TimeApi;
using System.Data.SqlTypes;

namespace BL
{
    public interface IWorkflowBL
    {

        
        public int RunWorkflowStep(bool bolManualStep, int record_pid, string record_prefix, string notepad, int x04id, BO.b06WorkflowStep recB06, List<BO.x69EntityRole_Assign> lisNominee, double current_user_lon = 0, double current_user_lat = 0,DateTime? b05date=null,string b05name=null, int portalflag=0, int tab1flag = 0);

        public List<int> GetRecordPidsFromFrameworkSQL(BO.b06WorkflowStep rec); //vrací sadu vyhovujích záznamů dotazu b06FrameworkSql pro běh robotického workflow kroku
        public bool ValidateRunWorkflowStep(BO.b06WorkflowStep recB06, int record_pid); //kontroluje, zda u workflow kroku je splněna podmínka přes sql dotaz: b06ValidateBeforeRunSQL
        public int Save2History(BO.b05Workflow_History rec);
        public IEnumerable<BO.b05Workflow_History> GetList_b05(string record_prefix, int record_pid, int p56id, int b05id,int j02id_portal);
        public IEnumerable<BO.b05Workflow_History> GetList_b05_p41_p28(List<int> p41ids, List<int> p28ids);
        public int InitWorkflowStatus(int record_pid, string record_prefix);
        public bool IsStepAvailable4Me(BO.b06WorkflowStep rec, int record_pid, string record_prefix);
        /// <summary>
        /// Vrací: b01id,b02id,j02id_owner,userinsert
        /// </summary>
        /// <param name="record_prefix"></param>
        /// <param name="record_pid"></param>
        /// <returns></returns>
        public BO.WorkflowEntityAttribute LoadEntityAttributes(string record_prefix, int record_pid);
        public bool SaveChangesInNotepad(int b05id, string notepad, int x04id, DateTime? b05date, string b05name = null,int portalflag=0, int tab1flag=0);
        
        public int GetB05CommentsCount(string record_prefix, int record_pid);
        
        public int RunWorkflowStatus(string record_prefix, int record_pid, int b02id_target, string notepad, int x04id, DateTime? b05date = null, string b05name = null, int portalflag = 0, int tab1flag = 0);   //posun bez krokového mechanismu
        
    }

    class WorkflowBL : BaseBL, IWorkflowBL
    {

        public WorkflowBL(BL.Factory mother) : base(mother)
        {

        }


        public int InitWorkflowStatus(int record_pid, string record_prefix) //nahodit výchozí workflow stav, vrací b02ID
        {
            var tuple = LoadEntityAttributes(record_prefix, record_pid);
           
            if (tuple==null || tuple.b01ID == 0) return 0;

            var lisB02 = _mother.b02WorkflowStatusBL.GetList(new BO.myQuery("b02") { IsRecordValid = true }).Where(p =>p.b01ID== tuple.b01ID && p.b02AutoRunFlag == BO.b02AutoRunFlagEnum.Startovaci);
            if (lisB02.Count() == 0) return 0;
            int intB02ID = lisB02.First().pid;

            _db.RunSql($"UPDATE {BO.Code.Entity.GetEntity(record_prefix)} SET b02ID={intB02ID} WHERE {record_prefix}ID={record_pid}");
            var recB05 = new BO.b05Workflow_History() {b02ID_To= intB02ID, b05IsManualStep = false,b05RecordEntity = record_prefix, b05RecordPid = record_pid, j02ID_Sys = _mother.CurrentUser.pid };
            Save2History(recB05);

            var lisB06 = _mother.b06WorkflowStepBL.GetList(new BO.myQuery("b06")).Where(p => p.b02ID == intB02ID && p.b06AutoRunFlag==BO.b06AutoRunFlagEnum.AutomatickySpustenySeStavem);
            foreach(var recB06 in lisB06)
            {
                RunWorkflowStep(false, record_pid, record_prefix, null, 0, recB06, null);
            }
            return intB02ID;
        }
        public int RunWorkflowStep(bool bolManualStep,int record_pid, string record_prefix, string notepad, int x04id, BO.b06WorkflowStep recB06, List<BO.x69EntityRole_Assign> lisNominee,double current_user_lon=0,double current_user_lat= 0,DateTime? b05date=null, string b05name = null,int portalflag=0, int tab1flag = 0)
        {            
            if (!ValidateWorkflowStepBeforeRun(record_pid, record_prefix, notepad, recB06, lisNominee, b05name, x04id))
            {
                return 0;
            }
            var recB05 = new BO.b05Workflow_History() { b05IsManualStep = bolManualStep, b06ID = recB06.pid, b05RecordEntity = record_prefix, b05RecordPid = record_pid, j02ID_Sys = _mother.CurrentUser.pid, b05Notepad = notepad, x04ID = x04id,b05Date=b05date,b05Name=b05name, b05PortalFlag= portalflag,b05Tab1Flag=tab1flag };

            if (recB06.pid == 0)    //pouze notepad
            {
                recB05.b05IsCommentOnly = true;
                return Save2History(recB05);
            }

            if (recB06.b06GeoFlag == BO.b06GeoFlagEnum.LoadFromP15) //načíst polohu+počasí z projektu/úkolu záznamu
            {               
                var retGeo=_mother.p15LocationBL.AppendWeatherLog(record_prefix, record_pid);
                if (!retGeo.issuccess)
                {
                    this.AddMessage("Nelze načíst polohu.");
                    return 0;
                }
                recB05.j95ID = retGeo.pid;
            }
            if (recB06.b06GeoFlag == BO.b06GeoFlagEnum.LoadFromCurrentUser && current_user_lon>0 && current_user_lat>0) //načíst polohu+počasí z aktuální polohy uživatele
            {
                var retGeo = _mother.p15LocationBL.AppendWeatherLog(record_prefix, record_pid, current_user_lon, current_user_lat);
                recB05.j95ID = retGeo.pid;
            }

            if (recB06.b06NomineeFlag == BO.b06NomineeFlagENum.Pevna)   //automatická změna řešitele
            {
                if (recB06.j11ID_Direct > 0)    //řešitelem bude explicitně určený tým
                {
                    lisNominee = new List<BO.x69EntityRole_Assign>();
                    lisNominee.Add(new BO.x69EntityRole_Assign() {x67ID=recB06.x67ID_Direct, j11ID = recB06.j11ID_Direct });
                }
                if (recB06.b02ID_LastReceiver_ReturnTo > 0) //vrátit řešiteli, který byl poslední ve statusu b02ID_LastReceiver_ReturnTo
                {

                }
            }

            if (lisNominee != null && lisNominee.Count() > 0)
            {
                if (!DL.BAS.SaveX69(_db, record_prefix, record_pid, lisNominee))    //změna obsazení role v záznamu
                {
                    this.AddMessageTranslated("Error: DL.BAS.SaveX69");
                    return 0;
                }

            }

            var tuple = LoadEntityAttributes(record_prefix, record_pid);
            

            if (recB06.b02ID_Target > 0)
            {
                _db.RunSql($"UPDATE {BO.Code.Entity.GetEntity(record_prefix)} SET b02ID={recB06.b02ID_Target} WHERE {record_prefix}ID={record_pid}");

                if (record_prefix=="p41" || record_prefix=="p56" || record_prefix=="p91" || record_prefix == "p28" || record_prefix == "o23")
                {
                    _db.RunSql($"exec dbo.{record_prefix}_append_log @pid,@login", new { pid = record_pid,login=_mother.CurrentUser.j02Login });
                }
                
            }

            
            BO.b02WorkflowStatus recB02_Target = null;

            if (recB06.b02ID_Target > 0)
            {
                recB05.b02ID_From = tuple.b02ID;recB05.b02ID_To = recB06.b02ID_Target;
                recB02_Target = _mother.b02WorkflowStatusBL.Load(recB06.b02ID_Target);

                if (recB02_Target.b02RecordFlag == BO.b02RecordFlagEnum.ZaznamVArchivu) //záznam přesunout do archivu
                {
                    _db.RunSql($"UPDATE {BO.Code.Entity.GetEntity(record_prefix)} SET {record_prefix}ValidUntil=GETDATE() WHERE {record_prefix}ID={record_pid}");
                }
                if (recB02_Target.b02RecordFlag == BO.b02RecordFlagEnum.ZaznamOtevreny) //záznam otevřít z archivu
                {
                    _db.RunSql($"UPDATE {BO.Code.Entity.GetEntity(record_prefix)} SET {record_prefix}ValidUntil=convert(datetime,'01.01.3000',104) WHERE {record_prefix}ID={record_pid}");
                }

            }
            int intB05ID=Save2History(recB05);
            if (recB06.p60ID > 0)
            {
                //automaticky založit úkol na pozadí
            }
            if (recB06.p83ID > 0)
            {
                //automaticky založit upomínku na pozadí
                _mother.p84UpominkaBL.TryCreate(record_pid, recB06.p83ID);
            }
            if (recB06.b06RunSQL != null)
            {
                string strSQL = _db.ParseMergeSQL(recB06.b06RunSQL, record_pid.ToString(),null,null,_mother.CurrentUser.pid);
                _db.RunSql(strSQL);
            }

        
            var lisB11 = _mother.b06WorkflowStepBL.GetListB11(recB06.pid);  //email notifikace            
            if (lisB11.Count() > 0)
            {
                var cp = new BL.TheColumnsProvider(_mother.EProvider, _mother.Translator);
                foreach (var recB11 in lisB11)
                {
                    var recJ61 = _mother.j61TextTemplateBL.Load(recB11.j61ID);
                    var recX40 = new BO.x40MailQueue()
                    {
                        x40RecordEntity = record_prefix,
                        x40RecordPid = record_pid,
                        x40IsRecordLink = (recJ61.j61RecordLinkFlag==BO.j61UserFlagEnum.Yes ? true: false),
                        x40IsHtmlBody = true,
                        x40GridColumns = recJ61.j61GridColumns,
                        x40Subject = recJ61.j61MailSubject,
                        x40Body = recJ61.j61MailBody,                       
                    };
                    if (recB11.b11Subject != null)
                    {
                        recX40.x40Subject = recB11.b11Subject;  //předmět zprávy je vyplněný ve workflow kroku
                    }

                    if (record_pid>0 && record_prefix != null)
                    {
                        
                        if ((recX40.x40Subject !=null && BO.Code.MergeContent.GetAllMergeFieldsInContent(recX40.x40Subject).Count() > 0) || (recX40.x40Body != null && BO.Code.MergeContent.GetAllMergeFieldsInContent(recX40.x40Body).Count() > 0))
                        {
                            var dt = _mother.gridBL.GetList4MailMerge(record_prefix, record_pid);
                            recX40.x40Body = BO.Code.MergeContent.GetMergedContent(recX40.x40Body, dt);
                            recX40.x40Subject = BO.Code.MergeContent.GetMergedContent(recX40.x40Subject, dt);
                        }
                        
                        
                    }
                    var receivers = _mother.MailBL.GetMailList(0, recB11.j11ID, recB11.x67ID, 0, 0, record_pid, record_prefix,recB11.j04ID,recB11.b11IsRecordOwner,recB11.b11IsRecordCreator);
                    
                    if (receivers == null || receivers.Count() == 0 || receivers.Count()>50)
                    {
                        //BO.Code.File.LogInfo("RunWorkflowStep: Nenašli se příjemci notifikační zprávy.",null, "RunWorkflowStep");
                        continue;   //nedělat nic, pokud nejsou příjemci zprávy nebo jich je víc než 50
                    }
                    recX40.x40Recipient = string.Join(";", receivers);
                    _mother.MailBL.SendMessage(recX40, false, cp);
                }
            }
            

            if (tuple.IsStopNotify)
            {
                if (recB06.b06AutoRunFlag == BO.b06AutoRunFlagEnum.NeUzivatelskyKrok)
                {
                    tuple.IsStopNotify = false;    //automatické kroky musí mít vždy zapnutou notifikaci                    
                }
                if (recB02_Target !=null && recB02_Target.b02AutoRunFlag == BO.b02AutoRunFlagEnum.Startovaci)
                {
                    tuple.IsStopNotify = true;
                }                
            }



            if (!tuple.IsStopNotify)   //odeslat notifikační zprávy
            {

            }

            return intB05ID;
        }

        //public bool UpdateExpenseBinding(int p31id,int b05id)
        //{
        //    _db.RunSql("UPDATE p31Worksheet set b05ID_Last=@b05id WHERE p31ID=@pid", new { pid = p31id, b05id = b05id });
        //    return _db.RunSql("UPDATE b05Workflow_History set p31ID_Expense=@p31id WHERE b05ID=@pid", new { p31id = p31id, pid = b05id });
        //}
        
        
        
        public bool SaveChangesInNotepad(int b05id, string notepad, int x04id, DateTime? b05date,string b05name=null, int portalflag = 0,int tab1flag=0)
        {
            if (b05id == 0)
            {
                return false;
            }
            string str200 = BO.Code.Bas.Html2Text(notepad);
            if (str200 !=null && str200.Length > 200)
            {
                str200 = $"{str200.Substring(0,197)}...";
            }

            
            return _db.RunSql("UPDATE b05Workflow_History set b05Notepad=@notepad,x04ID=@x04id,b05NotepadText200=@s,b05Date=@d,b05Name=@strname,b05PortalFlag=@portalflag,b05Tab1Flag=@tab1flag,b05DateUpdate=GETDATE(),b05UserUpdate=@login WHERE b05ID=@b05id", new {notepad=notepad,x04id=x04id,b05id=b05id,d=b05date,login=_mother.CurrentUser.j02Login,s= str200, strname =b05name, portalflag = portalflag,tab1flag=tab1flag });
            
        }
        
        public List<int> GetRecordPidsFromFrameworkSQL(BO.b06WorkflowStep rec)
        {
            rec.b06FrameworkSql = rec.b06FrameworkSql.Replace("@b02id", rec.b02ID.ToString());
            rec.b06FrameworkSql = rec.b06FrameworkSql.Replace("#b02id#", rec.b02ID.ToString());
            var dt = _db.GetDataTable(BO.Code.Bas.OcistitSQL(rec.b06FrameworkSql));
            var ret = new List<int>();
            foreach (System.Data.DataRow dbrow in dt.Rows)
            {
                ret.Add(Convert.ToInt32(dbrow[0]));
            }

            return ret;

        }

        public bool ValidateRunWorkflowStep(BO.b06WorkflowStep recB06, int record_pid)
        {
            if (string.IsNullOrEmpty(recB06.b06ValidateBeforeRunSQL))
            {
                return true;
            }
            string strSQL = _db.ParseMergeSQL(recB06.b06ValidateBeforeRunSQL, record_pid.ToString());
            if (_db.GetIntegerFromSql(strSQL) != 1)
            {
                return false;
            }
            else
            {
                return true;
            }
        }


        public bool ValidateWorkflowStepBeforeRun(int record_pid, string record_prefix, string notepad, BO.b06WorkflowStep recB06, List<BO.x69EntityRole_Assign> lisNominee,string b05name,int x04id)
        {
            if (recB06.pid==0 || recB06.b06NotepadFlag == BO.b06NotepadFlagEnum.NotepadIsRequired)
            {
                if (string.IsNullOrEmpty(notepad) && string.IsNullOrEmpty(b05name))
                {
                    this.AddMessage("Chybí vyplnit [Notepad] nebo [Název]."); return false;
                }
                if (string.IsNullOrEmpty(b05name) && (notepad.Length < 10 && x04id>0) || (notepad.Length<5 && x04id==0))
                {
                    this.AddMessage("[Notepad] je příliš krátký."); return false;
                }
            }
            if (recB06.pid == 0)
            {                
                return true;    //na zápis notepadu to stačí             
            }
            if (recB06.pid == 0)
            {
                this.AddMessage("Musíte zvolit z nabídky konkrétní krok!"); return false;
            }
            if (lisNominee != null && lisNominee.Count() > 0)
            {
                if (lisNominee.Any(p => p.j02ID == 0 && p.j11ID == 0 && !p.x69IsAllUsers))
                {
                    this.AddMessage("V nominaci chybí specifikace osoby nebo týmu osob."); return false;
                }
            }
           

            if (recB06.b06NomineeFlag == BO.b06NomineeFlagENum.RucniPovinna && (lisNominee == null || lisNominee.Count() == 0))
            {
                this.AddMessageWithPars("Krok {0} vyžaduje nominaci!", recB06.b06Name); return false;
            }
            if (recB06.b06ValidateBeforeRunSQL != null && !ValidateRunWorkflowStep(recB06, record_pid)) //spuštění kroku je podmíněno splněním SQL dotazu
            {
                this.AddMessage(recB06.b06ValidateBeforeErrorMessage); return false;

            }

            return true;
        }

        public int GetB05CommentsCount(string record_prefix, int record_pid)
        {
            return _db.GetList<BO.GetInteger>("SELECT COUNT(a.b05ID) as Value FROM b05Workflow_History a WHERE a.b05RecordEntity=@prefix AND a.b05RecordPid=@pid AND a.b05Notepad IS NOT NULL", new { prefix = record_prefix, pid = record_pid }).First().Value;
            
        }

        private void Init_SqlListB05()
        {
            sb("select a.*,b06.b06Name,b02to.b02Name as b02Name_To,b02from.b02Name as b02Name_From,j02x.j02LastName+' '+j02x.j02FirstName as Person,b02to.b02Color");
            sb(",p56.p56Name,o22.o22Name,o21.o21Name");
            sb(",j95.j95Longitude,j95.j95Latitude,j95.j95Temp,j95.j95TempFeelsLike,j95.j95Humidity,j95.j95WindSpeed,j95.j95Location,j95.j95Icon,j95.j95Description,");
            sb(_db.GetSQL1_Ocas("b05", false, false, true));
            sb(" FROM b05Workflow_History a ");
            sb(" INNER JOIN j02User j02x ON a.j02ID_Sys=j02x.j02ID INNER JOIN j04UserRole j04x ON j02x.j04ID=j04x.j04ID INNER JOIN x67EntityRole x67x ON j04x.x67ID=x67x.x67ID");
            sb(" LEFT OUTER JOIN b06WorkflowStep b06 ON a.b06ID=b06.b06ID");
            sb(" LEFT OUTER JOIN b02WorkflowStatus b02to ON a.b02ID_To=b02to.b02ID");
            sb(" LEFT OUTER JOIN b02WorkflowStatus b02from ON a.b02ID_From=b02from.b02ID");
            sb(" LEFT OUTER JOIN p56Task p56 ON a.p56ID=p56.p56ID LEFT OUTER JOIN o22Milestone o22 ON a.o22ID=o22.o22ID LEFT OUTER JOIN o21MilestoneType o21 ON o22.o21ID=o21.o21ID");
            sb(" LEFT OUTER JOIN j95GeoWeatherLog j95 ON a.j95ID=j95.j95ID");
        }
        public IEnumerable<BO.b05Workflow_History> GetList_b05_p41_p28(List<int> p41ids, List<int> p28ids)
        {
            Init_SqlListB05();

            if (p41ids.Count() == 0)
            {
                p41ids.Add(-1);
            }
            if (p28ids.Count() == 0)
            {
                p28ids.Add(-1);
            }

            sb($" WHERE (a.b05RecordEntity='p41' AND a.b05RecordPid IN ({string.Join(",", p41ids)})) OR (a.b05RecordEntity='p28' AND a.b05RecordPid IN ({string.Join(",", p28ids)}))");
            if (!_mother.CurrentUser.IsRatesAccess)
            {
                sb(" AND a.b05Tab1Flag & 4 <> 4");    //nemá přístup k fakturačním poznámkám
            }
            sb(" ORDER BY a.b05ID DESC");
            return _db.GetList<BO.b05Workflow_History>(sbret());
        }
        public IEnumerable<BO.b05Workflow_History> GetList_b05(string record_prefix,int record_pid,int p56id,int b05id, int j02id_portal)
        {
            Init_SqlListB05();
            
            if (b05id > 0)
            {
                sb($" WHERE a.b05ID={b05id}");
            }
            else
            {
                sb($" WHERE x67x.x01ID={_mother.CurrentUser.x01ID}");
                if (record_prefix=="p31" && record_pid == 0)
                {
                    sb($" AND a.b05RecordEntity='j02' AND a.b05RecordPid={_mother.CurrentUser.pid}");   //nově vykazovaný úkon je dočasně pod prefixem j02
                }
                else
                {
                    if (!string.IsNullOrEmpty(record_prefix) && record_pid > 0)
                    {
                        if (record_prefix == "p91")
                        {
                            
                            sb($" AND (a.b05RecordEntity='{record_prefix}' AND a.b05RecordPid={record_pid})");
                            sb($" OR (a.b05RecordEntity='p84' AND a.b05RecordPid IN (select p84ID FROM p84Upominka WHERE p91ID={record_pid}))");
                            sb($" OR (a.b05Tab1Flag IN (4,6) AND a.b05RecordEntity='p28' AND a.b05RecordPid IN (select p28ID FROM p91Invoice WHERE p91ID={record_pid}))");
                            sb($" OR (a.b05Tab1Flag IN (4,6) AND a.b05RecordEntity='p41' AND a.b05RecordPid IN (select p41ID FROM p31Worksheet WHERE p91ID={record_pid}))");

                        }
                        else
                        {
                            sb($" AND a.b05RecordEntity='{record_prefix}' AND a.b05RecordPid={record_pid}");
                        }
                        

                    }
                }
                
                if (p56id > 0)
                {
                    sb($" AND a.p56ID={p56id}");
                }
                if (j02id_portal > 0)
                {
                    sb($" AND (a.j02ID_Sys={j02id_portal} OR a.b05PortalFlag=1)");
                }

                if (!_mother.CurrentUser.IsRatesAccess)
                {
                    sb(" AND a.b05Tab1Flag & 4 <> 4");    //nemá přístup k fakturačním poznámkám
                }
                sb(" ORDER BY a.b05ID DESC");
            }
            

            return _db.GetList<BO.b05Workflow_History>(sbret());
        }


        public int Save2History(BO.b05Workflow_History rec)
        {
            var p = new DL.Params4Dapper();
            p.AddString("b05RecordEntity", rec.b05RecordEntity);
            p.AddInt("b05RecordPid", rec.b05RecordPid, true);
            if (rec.j02ID_Sys == 0) { rec.j02ID_Sys = _db.CurrentUser.pid; };
            p.AddInt("j02ID_Sys", rec.j02ID_Sys, true);
            p.AddInt("b06ID", rec.b06ID, true);
            p.AddInt("b02ID_From", rec.b02ID_From, true);
            p.AddInt("b02ID_To", rec.b02ID_To, true);
            p.AddInt("j02ID_Nominee", rec.j02ID_Nominee, true);
            p.AddInt("j11ID_Nominee", rec.j11ID_Nominee, true);
            p.AddInt("x67ID_Nominee", rec.x67ID_Nominee, true);

            p.AddBool("b05IsManualStep", rec.b05IsManualStep);
            p.AddBool("b05IsCommentOnly", rec.b05IsCommentOnly);

            p.AddString("b05ErrorMessage", rec.b05ErrorMessage);
            p.AddDateTime("b05Date", rec.b05Date);

            p.AddString("b05Name", rec.b05Name);
            

            p.AddString("b05Notepad", rec.b05Notepad);
            p.AddInt("x04ID", rec.x04ID, true);
            rec.b05NotepadText200 = BO.Code.Bas.Html2Text(rec.b05Notepad);
            if (rec.b05NotepadText200 != null && rec.b05NotepadText200.Length > 200)
            {
                rec.b05NotepadText200 = $"{rec.b05NotepadText200.Substring(0, 197)}...";
            }
            p.AddString("b05NotepadText200", rec.b05NotepadText200);
            p.AddString("b05SQL", rec.b05SQL);

            p.AddInt("p56ID", rec.p56ID, true);
            p.AddInt("o22ID", rec.o22ID, true);
            p.AddInt("p58ID", rec.p58ID, true);
            p.AddInt("j95ID", rec.j95ID, true);
            p.AddInt("b05PortalFlag", rec.b05PortalFlag);
            p.AddInt("b05Tab1Flag", rec.b05Tab1Flag);

            int intB05ID = _db.SaveRecord("b05Workflow_History", p, rec, false);

            if (intB05ID>0 && rec.b05RecordEntity == "p31" && rec.b05RecordPid>0)
            {
                _db.RunSql("UPDATE p31Worksheet set b05ID_Last=@b05id WHERE p31ID=@p31id", new { b05id = intB05ID, p31id = rec.b05RecordPid });
            }
            return intB05ID;
        }

        public bool IsStepAvailable4Me(BO.b06WorkflowStep rec, int record_pid, string record_prefix)
        {
            int intJ02ID_Owner = _db.GetIntegerFromSql($"select j02ID_Owner FROM {BO.Code.Entity.GetEntity(record_prefix)} WHERE {record_prefix}ID={record_pid}");
            var xx = _db.Load<BO.GetString>($"select {record_prefix}UserInsert as Value FROM {BO.Code.Entity.GetEntity(record_prefix)} WHERE {record_prefix}ID={record_pid}");
            string strUserInsert = null;
            if (xx != null)
            {
                strUserInsert = xx.Value;
            }



            var lisB08 = _mother.b06WorkflowStepBL.GetListB08(rec.pid);
            var j11ids_my = BO.Code.Bas.ConvertString2ListInt(_mother.CurrentUser.j11IDs);

            foreach(var c in lisB08)
            {
                if (c.b08IsRecordOwner && intJ02ID_Owner == _mother.CurrentUser.j02ID)
                {
                    return true;    //vlastník záznamu
                }
                if (c.b08IsRecordCreator && strUserInsert == _mother.CurrentUser.j02Login)
                {
                    return true;    //zakladatel záznamu
                }
                if (c.j04ID == _mother.CurrentUser.j04ID)
                {
                    return true;    //aplikační role
                }
                if (c.j11ID > 0 && j11ids_my.Contains(c.j11ID))
                {
                    return true;    //tým
                }
                if (c.x67ID > 0)
                {
                    var recX67 = _mother.x67EntityRoleBL.Load(c.x67ID);
                    int intRelevantRecordPid = record_pid;
                    if (recX67 !=null && recX67.x67Entity != record_prefix)
                    {
                        if (record_prefix=="p56" && recX67.x67Entity == "p41")
                        {
                            intRelevantRecordPid = _mother.p56TaskBL.Load(record_pid).p41ID;    //projekt v úkolu
                        }
                        if (record_prefix == "o23")
                        {
                            var lisO19 = _mother.o23DocBL.GetList_o19(record_pid).Where(p => p.o20Entity == recX67.x67Entity);
                            if (lisO19.Count() > 0)
                            {
                                intRelevantRecordPid = lisO19.First().o19RecordPid;
                            }
                            

                        }
                        if (record_prefix == "p91" && recX67.x67Entity == "p41")
                        {
                            intRelevantRecordPid = _mother.p91InvoiceBL.Load(record_pid).p41ID_First;    //první projekt ve vyúčtování
                        }

                    }

                    if (intRelevantRecordPid > 0 && recX67 !=null)
                    {
                        var lisX69 = _mother.x67EntityRoleBL.GetList_X69(recX67.x67Entity, intRelevantRecordPid);
                        if (lisX69.Where(p => p.x67ID == c.x67ID && p.j02ID==_mother.CurrentUser.pid).Count() > 0)
                        {
                            return true;
                        }
                        if (j11ids_my.Count() > 0)
                        {
                            foreach(int intJ11ID in j11ids_my)
                            {
                                if (lisX69.Where(p => p.x67ID == c.x67ID && p.j11ID== intJ11ID).Count() > 0)
                                {
                                    return true;
                                }
                            }
                        }
                        
                    }
                    
                    

                }
            }

            return false;
               
        }


        public BO.WorkflowEntityAttribute LoadEntityAttributes(string record_prefix, int record_pid)    //vrací: b01id,b02id,j02id_owner,userinsert
        {
            var ret = new BO.WorkflowEntityAttribute();
            switch (record_prefix)
            {
                case "p56":
                    var recP56 = _mother.p56TaskBL.Load(record_pid); ret.b01ID = recP56.b01ID; ret.b02ID = recP56.b02ID; ret.j02ID_Owner = recP56.j02ID_Owner; ret.UserInsert = recP56.UserInsert;ret.IsStopNotify = recP56.p56IsStopNotify;
                    break;
                case "p41":
                    var recP41 = _mother.p41ProjectBL.Load(record_pid); ret.b01ID = recP41.b01ID; ret.b02ID = recP41.b02ID; ret.j02ID_Owner = recP41.j02ID_Owner; ret.UserInsert = recP41.UserInsert;ret.IsStopNotify = recP41.p41IsStopNotify;
                    break;
                case "o23":
                    var recO23 = _mother.o23DocBL.Load(record_pid); ret.b01ID = recO23.b01ID; ret.b02ID = recO23.b02ID; ret.j02ID_Owner = recO23.j02ID_Owner; ret.UserInsert = recO23.UserInsert;
                    break;
                case "p91":
                    var recP91 = _mother.p91InvoiceBL.Load(record_pid); ret.b01ID = recP91.b01ID; ret.b02ID = recP91.b02ID; ret.j02ID_Owner = recP91.j02ID_Owner; ret.UserInsert = recP91.UserInsert;
                    break;
                case "p28":
                    var recP28 = _mother.p28ContactBL.Load(record_pid); ret.b01ID = recP28.b01ID; ret.b02ID = recP28.b02ID; ret.j02ID_Owner = recP28.j02ID_Owner; ret.UserInsert = recP28.UserInsert;
                    break;
                default:
                    return null;                    
            }

            return ret;
        }

        public int RunWorkflowStatus(string record_prefix,int record_pid,int b02id_target, string notepad, int x04id, DateTime? b05date = null, string b05name = null,int portalflag = 0, int tab1flag = 0)
        {
            //posun stavu bez krokového mechanismu
            var tuple = LoadEntityAttributes(record_prefix, record_pid);

            var recB02_Target = _mother.b02WorkflowStatusBL.Load(b02id_target);

            if (!ValidateWorkflowStatusBeforeRun(recB02_Target, notepad, b05name,x04id))
            {
                return 0;
            }

            var recB05 = new BO.b05Workflow_History() { b05IsManualStep = true, b06ID = 0, b05RecordEntity = record_prefix, b05RecordPid = record_pid, j02ID_Sys = _mother.CurrentUser.pid, b05Notepad = notepad, x04ID = x04id, b05Date = b05date, b05Name = b05name,b05PortalFlag= portalflag, b05Tab1Flag=tab1flag };


            if (recB02_Target != null)
            {
                
                _db.RunSql($"UPDATE {BO.Code.Entity.GetEntity(record_prefix)} SET b02ID={recB02_Target.pid} WHERE {record_prefix}ID={record_pid}");

                if (record_prefix == "p41" || record_prefix == "p56" || record_prefix == "p91" || record_prefix == "p28")
                {
                    _db.RunSql($"exec dbo.{record_prefix}_append_log @pid", new { pid = record_pid });
                }

                
                recB05.b02ID_From = tuple.b02ID; recB05.b02ID_To = recB02_Target.pid;
                
                if (recB02_Target.b02RecordFlag == BO.b02RecordFlagEnum.ZaznamVArchivu) //záznam přesunout do archivu
                {
                    _db.RunSql($"UPDATE {BO.Code.Entity.GetEntity(record_prefix)} SET {record_prefix}ValidUntil=GETDATE() WHERE {record_prefix}ID={record_pid}");
                }
                if (recB02_Target.b02RecordFlag == BO.b02RecordFlagEnum.ZaznamOtevreny) //záznam otevřít z archivu
                {
                    _db.RunSql($"UPDATE {BO.Code.Entity.GetEntity(record_prefix)} SET {record_prefix}ValidUntil=convert(datetime,'01.01.3000',104) WHERE {record_prefix}ID={record_pid}");
                }

               
            }

            return Save2History(recB05);            



        }


        public bool ValidateWorkflowStatusBeforeRun(BO.b02WorkflowStatus recB02, string notepad, string b05name, int x04id)
        {
            if (recB02==null || recB02.pid == 0)
            {
                if (string.IsNullOrEmpty(notepad) && string.IsNullOrEmpty(b05name))
                {
                    this.AddMessage("Chybí vyplnit [Notepad] nebo [Název]."); return false;
                }
                if (string.IsNullOrEmpty(b05name) && (notepad.Length < 20 && x04id > 0) || (notepad.Length < 5 && x04id == 0))
                {
                    this.AddMessage("[Notepad] je příliš krátký."); return false;
                }
            }
           
           
            return true;
        }

    }
}
