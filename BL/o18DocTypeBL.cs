using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    public interface Io18DocTypeBL
    {
        public BO.o18DocType Load(int pid);
        public IEnumerable<BO.o18DocType> GetList(BO.myQueryO18 mq);
        public IEnumerable<BO.o18DocType> GetList_DocumentCreate(string prefix = null);     //seznam typů, pro které uživatel může založit nový dokument
        public IEnumerable<BO.o20DocTypeEntity> GetList_o20(int o18id);
        public BO.o20DocTypeEntity LoadO20(int o20id);
        public IEnumerable<BO.o16DocType_FieldSetting> GetList_o16(int o18id);
        public IEnumerable<BO.o16DocType_FieldSetting> GetList_o16();
        public int Save(BO.o18DocType rec, List<BO.o20DocTypeEntity> lisO20, List<BO.o16DocType_FieldSetting> lisO16, List<BO.x69EntityRole_Assign> lisX69, List<BO.j08CreatePermission> lisJ08);

    }
    class o18DocTypeBL : BaseBL, Io18DocTypeBL
    {
        public o18DocTypeBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*");
            sb(",convert(bit,case when a.o18ID IN (SELECT o18ID FROM o20DocTypeEntity WHERE o20Entity='j02') then 1 else 0 end) as Is_j02");
            sb(",convert(bit,case when a.o18ID IN (SELECT o18ID FROM o20DocTypeEntity WHERE o20Entity='p41') then 1 else 0 end) as Is_p41");            
            sb(",convert(bit,case when a.o18ID IN (SELECT o18ID FROM o20DocTypeEntity WHERE o20Entity='p31') then 1 else 0 end) as Is_p31");            
            sb(",convert(bit,case when a.o18ID IN (SELECT o18ID FROM o20DocTypeEntity WHERE o20Entity='p28') then 1 else 0 end) as Is_p28");
            
            sb(","+_db.GetSQL1_Ocas("o18"));
            sb(" FROM o18DocType a");
            sb(strAppend);
            return sbret();
        }
        public BO.o18DocType Load(int pid)
        {
            return _db.Load<BO.o18DocType>(GetSQL1(" WHERE a.o18ID=@pid"), new { pid = pid });
        }
        public BO.o20DocTypeEntity LoadO20(int o20id)
        {
            return _db.Load<BO.o20DocTypeEntity>("SELECT a.*,a.o20ID as pid FROM o20DocTypeEntity a WHERE a.o20ID=@pid", new { pid = o20id });
        }

        public IEnumerable<BO.o18DocType> GetList(BO.myQueryO18 mq)
        {
            if (mq.explicit_orderby == null) mq.explicit_orderby = "a.o18Ordinary";
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(" WHERE 1=1"), mq, _mother.CurrentUser);
            return _db.GetList<BO.o18DocType>(fq.FinalSql, fq.Parameters);
        }

        public IEnumerable<BO.o20DocTypeEntity> GetList_o20(int o18id)
        {
            
            return _db.GetList<BO.o20DocTypeEntity>("SELECT a.*,a.o20ID as pid FROM o20DocTypeEntity a WHERE a.o18ID=@o18id ORDER BY a.o20Ordinary",new { o18id = o18id });
        }
        public IEnumerable<BO.o16DocType_FieldSetting> GetList_o16(int o18id)
        {
            return _db.GetList<BO.o16DocType_FieldSetting>("SELECT * FROM o16DocType_FieldSetting WHERE o18ID=@pid ORDER BY o16Ordinary", new { pid = o18id });
        }
        public IEnumerable<BO.o16DocType_FieldSetting> GetList_o16()
        {
            return _db.GetList<BO.o16DocType_FieldSetting>("SELECT * FROM o16DocType_FieldSetting ORDER BY o16Ordinary");
        }
        public int Save(BO.o18DocType rec,List<BO.o20DocTypeEntity> lisO20,List<BO.o16DocType_FieldSetting> lisO16, List<BO.x69EntityRole_Assign> lisX69, List<BO.j08CreatePermission> lisJ08)
        {
            if (!ValidateBeforeSave(rec,lisO16))
            {
                return 0;
            }
            //using (var sc = new System.Transactions.TransactionScope())
            //{

            //}

            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddInt("x01ID", rec.x01ID == 0 ? _mother.CurrentUser.x01ID : rec.x01ID);
            p.AddString("o18Name", rec.o18Name);
            p.AddString("o18Code", rec.o18Code);
            p.AddInt("o18Ordinary", rec.o18Ordinary);
            p.AddBool("o18IsColors", rec.o18IsColors);
            p.AddByte("o18NotepadTab", rec.o18NotepadTab);
            p.AddByte("o18FilesTab", rec.o18FilesTab);
            p.AddByte("o18RolesTab", rec.o18RolesTab);
            p.AddByte("o18TagsTab", rec.o18TagsTab);
            p.AddInt("b01ID", rec.b01ID, true);            

            p.AddInt("x38ID", rec.x38ID, true);
            p.AddInt("o17ID", rec.o17ID, true);
            p.AddString("o18ReportCodes", rec.o18ReportCodes);
            p.AddEnumInt("o18EntryNameFlag", rec.o18EntryNameFlag);
            p.AddEnumInt("o18EntryCodeFlag", rec.o18EntryCodeFlag);
            p.AddEnumInt("o18EntryOrdinaryFlag", rec.o18EntryOrdinaryFlag);
            p.AddBool("o18IsSeparatedNotepadTab", rec.o18IsSeparatedNotepadTab);
            p.AddInt("o18MaxOneFileSize", rec.o18MaxOneFileSize);
            p.AddString("o18AllowedFileExtensions", rec.o18AllowedFileExtensions);
            p.AddBool("o18IsAllowEncryption", rec.o18IsAllowEncryption);
            p.AddEnumInt("o18GeoFlag", rec.o18GeoFlag);
            p.AddEnumInt("o18BarcodeFlag", rec.o18BarcodeFlag);

            p.AddBool("o18IsAllowTree", rec.o18IsAllowTree);


            p.AddEnumInt("o18TemplateFlag", rec.o18TemplateFlag);
            p.AddInt("p34ID_Uctenka", rec.p34ID_Uctenka, true);
            p.AddInt("p32ID_Uctenka", rec.p32ID_Uctenka, true);


            int intO18ID = _db.SaveRecord("o18DocType", p, rec);
            if (intO18ID > 0)
            {
                var o18ids = new List<int>();
                var lisO20Saved = GetList_o20(intO18ID);
                foreach (var c in lisO20)
                {
                    p = new DL.Params4Dapper();

                    p.AddInt("pid", c.o20ID);
                    p.AddInt("o18ID", intO18ID, true);
                    p.AddString("o20Entity", c.o20Entity);
                    p.AddString("o20Name", c.o20Name);
                    p.AddEnumInt("o20EntryModeFlag", c.o20EntryModeFlag);
                    p.AddEnumInt("o20GridColumnFlag", c.o20GridColumnFlag);
                    p.AddEnumInt("o20EntityPageFlag", c.o20EntityPageFlag);
                    p.AddBool("o20IsMultiSelect", c.o20IsMultiSelect);
                    p.AddBool("o20IsClosed", c.o20IsClosed);
                    p.AddBool("o20IsEntryRequired", c.o20IsEntryRequired);
                    p.AddInt("o20Ordinary", c.o20Ordinary);
                    p.AddInt("o20RecTypePid", c.o20RecTypePid, true);
                    p.AddString("o20RecTypeEntity", c.o20RecTypeEntity);
                    _db.SaveRecord("o20DocTypeEntity", p, c, false, false);
                }
                foreach (var c in lisO20Saved)
                {
                    if (!lisO20.Any(p => p.o20ID == c.o20ID && p.o20ID > 0))
                    {
                        var intO19ID = _db.GetIntegerFromSql("select o19ID FROM o19DocTypeEntity_Binding WHERE o20ID=" + c.o20ID.ToString());
                        if (intO19ID > 0)
                        {
                            this.AddMessage("Vazbu nelze odstranit, protože pro ní již existují záznamy dokumentů."); return 0;
                        }
                        _db.RunSql("DELETE FROM o20DocTypeEntity WHERE o20ID=@pid", new { pid = c.o20ID });
                    }
                }

                if (lisO16 != null)
                {
                    _db.RunSql("UPDATE o16DocType_FieldSetting SET o16FieldGroup=NULL WHERE o18ID=@pid", new { pid = intO18ID });    //příznak, že záznam byl uložen
                    foreach (var c in lisO16)
                    {
                        p = new DL.Params4Dapper();
                        p.AddInt("pid", _db.GetIntegerFromSql($"select TOP 1 o16ID as Value FROM o16DocType_FieldSetting WHERE o18ID={intO18ID} AND o16Field='{c.o16Field}'"));
                        p.AddInt("o18ID", intO18ID, true);
                        p.AddString("o16Field", c.o16Field);
                        p.AddString("o16Name", c.o16Name);
                        p.AddString("o16NameGrid", c.o16NameGrid);
                        p.AddBool("o16IsEntryRequired", c.o16IsEntryRequired);
                        p.AddBool("o16IsGridField", c.o16IsGridField);
                        p.AddBool("o16IsFixedDataSource", c.o16IsFixedDataSource);
                        p.AddString("o16DataSource", c.o16DataSource);
                        p.AddInt("o16TextboxHeight", c.o16TextboxHeight);

                        p.AddInt("o16Ordinary", c.o16Ordinary);
                        p.AddString("o16Format", c.o16Format);
                        p.AddString("o16HelpText", c.o16HelpText);
                        p.AddInt("o16ReminderNotifyBefore", c.o16ReminderNotifyBefore);
                        p.AddString("o16FieldGroup", "1");  //příznak, že záznam byl uložen
                        int into16ID = _db.SaveRecord("o16DocType_FieldSetting", p, c, false, false);
                    }
                    _db.RunSql("DELETE FROM o16DocType_FieldSetting WHERE o16FieldGroup IS NULL AND o18ID=@pid", new { pid = intO18ID });
                }

                if (lisJ08 != null)
                {
                    DL.BAS.SaveJ08(_db, "o18", intO18ID, lisJ08);
                }

                if (lisX69 != null && !DL.BAS.SaveX69(_db, "o18", intO18ID, lisX69))
                {
                    this.AddMessageTranslated("Error: DL.BAS.SaveX69");
                    return 0;
                }

                if (rec.o17ID == 0) //zkontrolovat a případně založit menu dokumentu
                {                    
                    var lisO17 = _mother.o17DocMenuBL.GetList(new BO.myQuery("o17"));
                    if (lisO17.Count() == 0)
                    {
                        var recO17 = new BO.o17DocMenu() { o17Name = "DOC", x01ID = _mother.CurrentUser.x01ID };
                        rec.o17ID = _mother.o17DocMenuBL.Save(recO17, null);
                    }
                    else
                    {
                        rec.o17ID = lisO17.First().pid;
                    }

                    _db.RunSql($"UPDATE o18DocType set o17ID={rec.o17ID} WHERE o18ID={intO18ID}");

                    if (_mother.App.HostingMode == Singleton.HostingModeEnum.SharedApp) //u sdílené aplikace je třeba aktualizovat obsah v o17DocMenu [a7_cloudheader]
                    {
                        new DL.HostingTasks(_db).UpdateCloudHeader_O17(_mother.Lic.x01LoginDomain);
                    }
                    _mother.App.RefreshO17List(); //obnovit singleton seznam o17
                    _mother.j02UserBL.ClearAllUsersCache(); //vyčistit uživatelům j02Cache_TimeStamp

                }

                //sc.Complete();  //potvrzení transakce
            }


            return intO18ID;

        }
        private bool ValidateBeforeSave(BO.o18DocType rec, List<BO.o16DocType_FieldSetting> lisO16)
        {
            if (rec.o18AllowedFileExtensions != null)
            {
                rec.o18AllowedFileExtensions = rec.o18AllowedFileExtensions.Replace(",", "|").Replace(";", "|");
            }
            if (string.IsNullOrEmpty(rec.o18Name))
            {
                this.AddMessage("Chybí vyplnit [Název]."); return false;
            }
           if (lisO16 != null)
            {
                if (lisO16.Any(p => string.IsNullOrEmpty(p.o16Name)))
                {
                    this.AddMessage("V nastavení uživatelských polí chybí vyplnit název.");return false;
                }
                if (lisO16.Any(p => string.IsNullOrEmpty(p.o16Field)))
                {
                    this.AddMessage("V nastavení uživatelských polí chybí vyplnit pole."); return false;
                }
                foreach(var c in lisO16.GroupBy(p => p.o16Field))
                {
                    if (c.Count() > 1)
                    {
                        this.AddMessage("V nastavení uživatelských polí je duplicita."); return false;
                    }
                }
               
            }
           if (rec.o17ID==0 && rec.pid > 0)
            {
                this.AddMessage("Chybí vyplnit [Menu dokumentů].");return false;
            }
            return true;
        }

        public IEnumerable<BO.o18DocType> GetList_DocumentCreate(string prefix=null)
        {
            if (_mother.CurrentUser.TestPermission(BO.PermValEnum.GR_o23_Creator))
            {
                return GetList(new BO.myQueryO18() { entity=prefix});  //všechny typy
            }
            string s = GetSQL1(" WHERE a.o18ID IN (select j08RecordPid FROM j08CreatePermission WHERE j08RecordEntity='o18' AND (j08IsAllUsers=1 OR j02ID=@j02id OR j04ID=@j04id");
            if (_mother.CurrentUser.j11IDs != null)
            {
                s += " OR j11ID IN (" + _mother.CurrentUser.j11IDs + ")";
            }
            s += "))";
            if (prefix != null)
            {
                s += " AND a.o18ID IN (select o18ID FROM o20DocTypeEntity WHERE o20Entity=@prefix)";
            }
            if (_mother.CurrentUser.IsHostingModeTotalCloud)
            {
                s += $" AND a.x01ID={_mother.CurrentUser.x01ID}";
            }

            return _db.GetList<BO.o18DocType>(s, new { j02id = _mother.CurrentUser.pid, j04id = _mother.CurrentUser.j04ID,prefix=prefix });
        }

    }
}
