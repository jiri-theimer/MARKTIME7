

using BO.TimeApi;

namespace BL
{
    public interface Io23DocBL
    {
        public BO.o23Doc Load(int pid);
        public IEnumerable<BO.o23Doc> GetList(BO.myQueryO23 mq,bool ischangelog=false);
        public int Save(BO.o23Doc rec, List<BO.o19DocTypeEntity_Binding> lisO19, List<BO.x69EntityRole_Assign> lisX69);
        public IEnumerable<BO.o19DocTypeEntity_Binding> GetList_o19(int o23id);
        public BO.o23RecDisposition InhaleRecDisposition(int pid, BO.o23Doc rec = null);
        public void UpdateGeo(int o23id, int j95id);        
        public BO.o23DocSum LoadSumRow(int pid);
        public void SavePassword(int o23id, string pwd);
        public void ClearPassword(int o23id);
        public string LoadPassword(int o23id);
        public string EncryptDescryptNotepad(string notepad, bool bolEncrypt);
        public void UpdateNotepad(int o23id, string notepad);
        public int SaveUctenka(BO.o23Doc rec);
    }
    class o23DocBL : BaseBL, Io23DocBL
    {
        public o23DocBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null, bool ischangelog = false)
        {
            sb("SELECT a.*,o18x.o18Name,o18x.o18TemplateFlag,j02owner.j02Name as Owner,o18x.b01ID,b02.b02Name,b02.b02Color,o18x.o17ID,o18x.x38ID,");
            sb(_db.GetSQL1_Ocas("o23"));
            if (ischangelog)
            {
                sb(" FROM o23Doc_Log a");
            }
            else
            {
                sb(" FROM o23Doc a");
            }
            
            sb(" INNER JOIN o18DocType o18x ON a.o18ID=o18x.o18ID LEFT OUTER JOIN j02User j02owner ON a.j02ID_Owner=j02owner.j02ID LEFT OUTER JOIN b02WorkflowStatus b02 ON a.b02ID=b02.b02ID");
            sb(strAppend);
            return sbret();
        }
        public BO.o23Doc Load(int pid)
        {
            return _db.Load<BO.o23Doc>(GetSQL1(" WHERE a.o23ID=@pid"), new { pid = pid });
        }

        public IEnumerable<BO.o23Doc> GetList(BO.myQueryO23 mq, bool ischangelog = false)
        {
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(null,ischangelog), mq, _mother.CurrentUser);
            return _db.GetList<BO.o23Doc>(fq.FinalSql, fq.Parameters);
        }

        public BO.o23DocSum LoadSumRow(int pid)
        {
            return _db.Load<BO.o23DocSum>("EXEC dbo.o23_inhale_sumrow @j03id_sys,@pid", new { j03id_sys = _mother.CurrentUser.pid, pid = pid });
        }

        private string GetSQL1_o19(string strAppend = null)
        {
            sb("SELECT a.*,o23.o23Name,o20.o18ID,o18.o18Name,o20.o20Ordinary,");
            sb("o23.o23ForeColor,o23.o23BackColor,o20.o20Entity,o20.o20Name,o20.o20IsMultiselect,o20.o20IsMultiselect,");
            sb(_db.GetSQL1_Ocas("o19"));            
            sb(" from o19DocTypeEntity_Binding a INNER JOIN o20DocTypeEntity o20 ON a.o20ID=o20.o20ID INNER JOIN o23Doc o23 ON a.o23ID=o23.o23ID INNER JOIN o18DocType o18 ON o20.o18ID=o18.o18ID");
            
            sb(strAppend);
            return sbret();
        }

        public IEnumerable<BO.o19DocTypeEntity_Binding> GetList_o19(int o23id)
        {
            return _db.GetList<BO.o19DocTypeEntity_Binding>(GetSQL1_o19(" WHERE a.o23ID=@o23id ORDER BY o20.o20Ordinary,o20.o20Entity"),new { o23id = o23id });
        }

        public void UpdateGeo(int o23id,int j95id)
        {
            _db.RunSql("UPDATE o23Doc SET j95ID=@j95id WHERE o23ID=@pid", new { j95id = j95id, pid = o23id });
        }
        public void SavePassword(int o23id, string pwd)
        {
            string s = new BO.Code.Cls.Crypto().Encrypt(pwd,"o23Doc");
            _db.RunSql("UPDATE o23Doc SET o23Password=@pwd WHERE o23ID=@pid", new { pwd = s, pid = o23id });
        }
        public void ClearPassword(int o23id)
        {            
            _db.RunSql("UPDATE o23Doc SET o23Password=null WHERE o23ID=@pid", new { pid = o23id });
        }
        public string LoadPassword(int o23id)
        {
            var rec = Load(o23id);
            if (rec == null || rec.o23Password==null) return null;

            return new BO.Code.Cls.Crypto().Decrypt(rec.o23Password, "o23Doc");
        }
        public string EncryptDescryptNotepad(string notepad,bool bolEncrypt)
        {
            var c = new BO.Code.Cls.Crypto();
            if (bolEncrypt)
            {
                return c.Encrypt(notepad, "o23Doc");
            }
            else
            {
                return c.Decrypt(notepad, "o23Doc");
            }
        }
        public void UpdateNotepad(int o23id, string notepad)
        {            
            _db.RunSql("UPDATE o23Doc SET o23Notepad=@s WHERE o23ID=@pid", new { s = notepad, pid = o23id });
        }

        public int SaveUctenka(BO.o23Doc rec)
        {            
            var recO18 = _mother.o18DocTypeBL.Load(rec.o18ID);
            if (rec.o18ID == 0 || recO18 == null)
            {
                this.AddMessage("Chybí vyplnit [Typ dokumentu]."); return 0;
            }
            if (string.IsNullOrEmpty(rec.o23Name))
            {
                this.AddMessage("Chybí vyplnit [Název]."); return 0;
            }
            if (rec.o23FreeNumber01==0)
            {
                this.AddMessage("Chybí vyplnit [Částka vč. DPH]."); return 0;
            }

            int intPID = 0;
            using (var sc = new System.Transactions.TransactionScope())
            {
                var p = new DL.Params4Dapper();
                p.AddInt("pid", rec.pid);
                p.AddInt("o18ID", rec.o18ID, true);   
                if (rec.j02ID_Owner == 0)
                {
                    rec.j02ID_Owner = _mother.CurrentUser.pid;
                }
                p.AddInt("j02ID_Owner", rec.j02ID_Owner, true);
                if (rec.o23Guid == Guid.Empty)
                {
                    rec.o23Guid = Guid.NewGuid();
                }
                p.AddString("o23Guid", rec.o23Guid.ToString());
                p.AddString("o23Name", rec.o23Name);    //text výdaje             
                p.AddString("o23Notepad", rec.o23Notepad);
                p.AddInt("x04ID", rec.x04ID, true);
                p.AddString("o23Code", rec.o23Code);    //kód dokladu
               

                p.AddString("o23FreeText01", rec.o23FreeText01);    //dodavatel
            

                p.AddDouble("o23FreeNumber01", rec.o23FreeNumber01);    //částka vč. dph
                p.AddDouble("o23FreeNumber02", rec.o23FreeNumber02);    //sazba dph
                p.AddDouble("o23FreeNumber03", rec.o23FreeNumber03);    //částka bez dph
                p.AddDouble("o23FreeNumber04", rec.o23FreeNumber04);    //částka dph
             
                p.AddDateTime("o23FreeDate01", rec.o23FreeDate01);  //datum výdaje

                p.AddInt("p34ID_Expense", rec.p34ID_Expense, true);
                p.AddInt("p41ID_Expense", rec.p41ID_Expense, true);
                p.AddInt("p32ID_Expense", rec.p32ID_Expense, true);
                p.AddInt("j27ID_Expense", rec.j27ID_Expense, true);


                string str200 = BO.Code.Bas.Html2Text(rec.o23Notepad);
                if (str200 != null && str200.Length > 200)
                {
                    str200 = $"{str200.Substring(0, 197)}...";
                }
                p.AddString("o23NotepadText200", str200);



                intPID = _db.SaveRecord("o23Doc", p, rec);
                if (intPID > 0)
                {
                   
                    var pars = new Dapper.DynamicParameters();
                    pars.Add("o23id", intPID);
                    pars.Add("j02id_sys", _mother.CurrentUser.pid);
                    if (_db.RunSp("o23_aftersave", ref pars, false) == "1")
                    {

                        if (recO18.b01ID > 0 && rec.b02ID == 0)
                        {
                            _mother.WorkflowBL.InitWorkflowStatus(intPID, "o23");   //nahodit úvodní workflow stav záznamu
                        }

                        sc.Complete();

                        return intPID;
                    }


                }
            }

            return intPID;
        }
        public int Save(BO.o23Doc rec,List<BO.o19DocTypeEntity_Binding> lisO19,List<BO.x69EntityRole_Assign> lisX69)
        {
            var recO18 = _mother.o18DocTypeBL.Load(rec.o18ID);
            var lisO20 = _mother.o18DocTypeBL.GetList_o20(rec.o18ID);

            
           
            if (!ValidateBeforeSave(rec, recO18, lisO19, lisO20))
            {
                return 0;
            }
            if (!_mother.x67EntityRoleBL.Validate_lisX69_BeforeAssign(lisX69))
            {
                return 0;
            }
            using (var sc = new System.Transactions.TransactionScope())
            {
                var p = new DL.Params4Dapper();
                p.AddInt("pid", rec.pid);
                p.AddInt("o18ID", rec.o18ID, true);
                p.AddInt("j95ID", rec.j95ID, true);
                p.AddInt("j02ID_Owner", rec.j02ID_Owner, true);
                if (rec.o23Guid == Guid.Empty)
                {
                    rec.o23Guid = Guid.NewGuid();
                }
                p.AddString("o23Guid", rec.o23Guid.ToString());
                p.AddString("o23Name", rec.o23Name);
                p.AddInt("o23BitStream", rec.o23BitStream);
                p.AddString("o23Notepad", rec.o23Notepad);
                p.AddInt("x04ID", rec.x04ID, true);
                p.AddString("o23Code", rec.o23Code);
                p.AddInt("o23Ordinary", rec.o23Ordinary);
                p.AddString("o23ArabicCode", rec.o23ArabicCode);
                p.AddString("o23ForeColor", rec.o23ForeColor);
                p.AddString("o23BackColor", rec.o23BackColor);
                p.AddBool("o23IsDraft", rec.o23IsDraft);
                p.AddBool("o23IsEncrypted", rec.o23IsEncrypted);
                
                p.AddString("o23ExternalCode", rec.o23ExternalCode);
                

                p.AddString("o23FreeText01", rec.o23FreeText01);
                p.AddString("o23FreeText02", rec.o23FreeText02);
                p.AddString("o23FreeText03", rec.o23FreeText03);
                p.AddString("o23FreeText04", rec.o23FreeText04);
                p.AddString("o23FreeText05", rec.o23FreeText05);
                p.AddString("o23FreeText06", rec.o23FreeText06);
                p.AddString("o23FreeText07", rec.o23FreeText07);
                p.AddString("o23FreeText08", rec.o23FreeText08);
                p.AddString("o23FreeText09", rec.o23FreeText09);
                p.AddString("o23FreeText10", rec.o23FreeText10);
                p.AddString("o23FreeText11", rec.o23FreeText11);
                p.AddString("o23FreeText12", rec.o23FreeText12);
                p.AddString("o23FreeText13", rec.o23FreeText13);
                p.AddString("o23FreeText14", rec.o23FreeText14);
                p.AddString("o23FreeText15", rec.o23FreeText15);                

                p.AddDouble("o23FreeNumber01", rec.o23FreeNumber01);
                p.AddDouble("o23FreeNumber02", rec.o23FreeNumber02);
                p.AddDouble("o23FreeNumber03", rec.o23FreeNumber03);
                p.AddDouble("o23FreeNumber04", rec.o23FreeNumber04);
                p.AddDouble("o23FreeNumber05", rec.o23FreeNumber05);

                p.AddDateTime("o23FreeDate01", rec.o23FreeDate01);
                p.AddDateTime("o23FreeDate02", rec.o23FreeDate02);
                p.AddDateTime("o23FreeDate03", rec.o23FreeDate03);
                p.AddDateTime("o23FreeDate04", rec.o23FreeDate04);
                p.AddDateTime("o23FreeDate05", rec.o23FreeDate05);

                p.AddBool("o23FreeBoolean01", rec.o23FreeBoolean01);
                p.AddBool("o23FreeBoolean02", rec.o23FreeBoolean02);
                p.AddBool("o23FreeBoolean03", rec.o23FreeBoolean03);
                p.AddBool("o23FreeBoolean04", rec.o23FreeBoolean04);
                p.AddBool("o23FreeBoolean05", rec.o23FreeBoolean05);

                string str200 = BO.Code.Bas.Html2Text(rec.o23Notepad);
                if (str200 != null && str200.Length > 200)
                {
                    str200 = $"{str200.Substring(0, 197)}...";
                }
                p.AddString("o23NotepadText200", str200);

                int intPID = _db.SaveRecord("o23Doc", p, rec);
                if (intPID > 0)
                {
                    SaveO19Binding(intPID, lisO19, lisO20);

                    if (lisX69 != null && !DL.BAS.SaveX69(_db, "o23", intPID, lisX69))
                    {
                        this.AddMessageTranslated("Error: DL.BAS.SaveX69");
                        return 0;
                    }
                   
                   
                    var pars = new Dapper.DynamicParameters();
                    pars.Add("o23id", intPID);
                    pars.Add("j02id_sys", _mother.CurrentUser.pid);
                    if (_db.RunSp("o23_aftersave",ref pars,false) == "1")
                    {
                        

                        if (recO18.b01ID > 0 && rec.b02ID == 0)
                        {
                            _mother.WorkflowBL.InitWorkflowStatus(intPID, "o23");   //nahodit úvodní workflow stav záznamu
                        }

                        sc.Complete();

                        return intPID;
                    }

                   
                    
                     
                }
            }

            return 0;
                
        }

        private void SaveO19Binding(int o23id, List<BO.o19DocTypeEntity_Binding> lisO19, IEnumerable<BO.o20DocTypeEntity> lisO20)
        {
            var lisSaved = GetList_o19(o23id);
            foreach(var c in lisO19)
            {                
                var rec = new BO.o19DocTypeEntity_Binding() { o23ID = o23id, o20ID = c.o20ID,o19RecordPid=c.o19RecordPid };
                var recO20 = _mother.o18DocTypeBL.LoadO20(c.o20ID);
                if (recO20.o20IsMultiSelect)
                {
                    if (lisSaved.Any(p => p.o20ID == c.o20ID && p.o19RecordPid == c.o19RecordPid))
                    {
                        rec = lisSaved.Where(p => p.o20ID == c.o20ID && p.o19RecordPid == c.o19RecordPid).First();
                    }
                }
                else
                {
                    if (lisSaved.Any(p => p.o20ID == c.o20ID))
                    {
                        rec = lisSaved.Where(p => p.o20ID == c.o20ID).First();
                        rec.o19RecordPid = c.o19RecordPid;
                    }                    
                }
                
                if (c.IsSetAsDeleted)
                {
                    if (c.pid > 0)
                    {
                        _db.RunSql("DELETE FROM o19DocTypeEntity_Binding WHERE o19ID=@pid", new { pid = c.pid });
                    }
                }
                else
                {
                    var p = new DL.Params4Dapper();
                    p.AddInt("pid", rec.pid);
                    p.AddInt("o23ID", o23id, true);
                    p.AddInt("o20ID", rec.o20ID, true);
                    p.AddInt("o19RecordPid", rec.o19RecordPid, true);

                    _db.SaveRecord("o19DocTypeEntity_Binding", p, rec);
                }
                
            }
           
        }
        private bool ValidateBeforeSave(BO.o23Doc rec,BO.o18DocType recO18, List<BO.o19DocTypeEntity_Binding> lisO19,IEnumerable<BO.o20DocTypeEntity> lisO20)
        {
            if (rec.j02ID_Owner == 0) rec.j02ID_Owner = _mother.CurrentUser.pid;
            
            if (rec.o18ID == 0 || recO18==null)
            {
                this.AddMessage("Chybí vyplnit [Typ dokumentu]."); return false;
            }
            if (string.IsNullOrEmpty(rec.o23Name) && recO18.o18EntryNameFlag == BO.o18EntryNameENUM.Manual)
            {
                this.AddMessage("Chybí vyplnit [Název]."); return false;
            }
            foreach(var c in lisO20.Where(p => p.o20IsEntryRequired))
            {
                if (lisO19.Where(p => p.o20ID == c.o20ID).Count() == 0)
                {
                    var cx = _mother.o18DocTypeBL.Load(c.o18ID);
                    this.AddMessageTranslated($"V záznamu [{cx.o18Name}] chybí povinná vazba.");
                    return false;
                }
            }
            if (rec.o23IsEncrypted && string.IsNullOrEmpty(rec.o23Notepad))
            {
                this.AddMessageTranslated("Pro šifrování chybí vyplnit [Notepad].");return false;
            }

            if (rec.pid==0 && _mother.CBL.TestIfAllowCreateRecord("o23", rec.o18ID).Flag == BO.ResultEnum.Failed)
            {
                return this.FalsehMessage("Nemáte oprávnění zakládat záznamy tohoto typu.");
            }


            return true;
        }

        public BO.o23RecDisposition InhaleRecDisposition(int pid,BO.o23Doc rec=null)
        {
            var c = new BO.o23RecDisposition() { a55ID = _mother.CurrentUser.a55ID_o23 };
            if (!_mother.CurrentUser.j04IsModule_o23) return c; //bez přístupu do modulu DOKUMENTY
            if (_mother.CurrentUser.IsAdmin || _mother.CurrentUser.TestPermission(BO.PermValEnum.GR_o23_Owner))
            {
                c.OwnerAccess = true; c.ReadAccess = true;
                return c;
            }
            if (rec == null) rec = Load(pid);
            if (rec == null)
            {
                return null;
            }
            if (rec.j02ID_Owner == _mother.CurrentUser.pid)
            {
                c.OwnerAccess = true; c.ReadAccess = true;  //vlastník záznamu
                return c;
            }

            var lisX69 = _mother.x67EntityRoleBL.GetList_X69_OneDoc(rec,true);
            foreach (var role in lisX69)
            {
                if (BO.Code.Bas.TestPermissionInRoleValue(role.x67RoleValue, BO.PermValEnum.o23_Owner))
                {
                    c.OwnerAccess = true; c.ReadAccess = true;  //vlastník
                    if (role.a55ID > 0) c.a55ID = role.a55ID;
                    return c;
                }
                if (BO.Code.Bas.TestPermissionInRoleValue(role.x67RoleValue, BO.PermValEnum.o23_Reader))   //čtenář
                {
                    c.ReadAccess = true;
                    if (role.a55ID > 0) c.a55ID = role.a55ID;
                }
            }

            if (!c.ReadAccess)
            {
                c.ReadAccess = _mother.CurrentUser.TestPermission(BO.PermValEnum.GR_o23_Reader);    //čtenář všech dokumentů
            }
            


            return c;
        }

        

    }
}
