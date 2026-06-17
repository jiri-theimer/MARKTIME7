

using DocumentFormat.OpenXml.Drawing;

namespace BL
{
    public interface Ix01LicenseBL
    {
        public BO.x01License Load(int pid);
        public BO.x01License LoadByGuid(string guid);
        public int Save(BO.x01License rec, int explicit_x01id_newrec = 0);
        public IEnumerable<BO.x01License> GetList(BO.myQuery mq);


    }

    class x01LicenseBL : BaseBL, Ix01LicenseBL
    {
        public x01LicenseBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,");
            sb(_db.GetSQL1_Ocas("x01"));
            sb(" FROM x01License a");
            sb(strAppend);
            return sbret();
        }

        public BO.x01License Load(int pid)
        {
            return _db.Load<BO.x01License>(GetSQL1(" WHERE a.x01ID=@pid"), new { pid = pid });
        }
        public BO.x01License LoadByGuid(string guid)
        {
            return _db.Load<BO.x01License>(GetSQL1(" WHERE a.x01Guid=@guid"), new { guid = guid });
        }

        public IEnumerable<BO.x01License> GetList(BO.myQuery mq)
        {
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.x01License>(fq.FinalSql, fq.Parameters);
        }


        public IEnumerable<BO.x01License> GetList_CloudHeader(BO.myQuery mq)
        {
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            
            return _db.GetList<BO.x01License>(fq.FinalSql, fq.Parameters);
        }




        public int Save(BO.x01License rec, int explicit_x01id_newrec = 0)
        {
            
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }
            int intX01ID = rec.pid;

            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            if (intX01ID == 0)  //x01id není idenity!
            {
                intX01ID = explicit_x01id_newrec;
                if (intX01ID == 0) intX01ID = new Random().Next(1000, 9999999);

                p.AddInt("x01ID", intX01ID);
            }
            p.AddString("x01Name", rec.x01Name);
            p.AddString("x01AppName", rec.x01AppName);
            p.AddString("x01LoginDomain", rec.x01LoginDomain);
            p.AddString("x01AppHost", rec.x01AppHost);

            p.AddString("x01CountryCode", rec.x01CountryCode);

            p.AddString("x01LogoFileName", rec.x01LogoFileName);
            p.AddInt("j27ID", rec.j27ID, true);
            p.AddInt("x04ID_Default", rec.x04ID_Default, true);
            p.AddEnumInt("x15ID", rec.x15ID, true);
            p.AddInt("x01Round2Minutes", rec.x01Round2Minutes);

            p.AddString("x01ContactEmail", rec.x01ContactEmail);
            p.AddString("x01ContactName", rec.x01ContactName);
            p.AddInt("x01LimitUsers", rec.x01LimitUsers);
            p.AddInt("x01Round2Minutes", rec.x01Round2Minutes);

            p.AddString("x01ApiKey", rec.x01ApiKey);
            p.AddString("x01RobotLogin", rec.x01RobotLogin);


            p.AddInt("x01LangIndex", rec.x01LangIndex);

            p.AddString("x01CustomCssFile", rec.x01CustomCssFile);

            p.AddInt("b02ID", rec.b02ID, true);

            p.AddString("x01BillingLang1", rec.x01BillingLang1);
            p.AddString("x01BillingLang2", rec.x01BillingLang2);
            p.AddString("x01BillingLang3", rec.x01BillingLang3);
            p.AddString("x01BillingLang4", rec.x01BillingLang4);
            p.AddString("x01BillingFlag1", rec.x01BillingFlag1);
            p.AddString("x01BillingFlag2", rec.x01BillingFlag2);
            p.AddString("x01BillingFlag3", rec.x01BillingFlag3);
            p.AddString("x01BillingFlag4", rec.x01BillingFlag4);

            p.AddBool("x01IsAllowPasswordRecovery", rec.x01IsAllowPasswordRecovery);
            p.AddString("x01PasswordRecovery_Question", rec.x01PasswordRecovery_Question);
            p.AddString("x01PasswordRecovery_Answer", rec.x01PasswordRecovery_Answer);

            p.AddString("x01ImportCnb_j27Codes", rec.x01ImportCnb_j27Codes);
            p.AddBool("x01IsAllowDuplicity_RegID", rec.x01IsAllowDuplicity_RegID);
            p.AddBool("x01IsAllowDuplicity_VatID", rec.x01IsAllowDuplicity_VatID);
            p.AddInt("x01InvoiceMaturityDays", rec.x01InvoiceMaturityDays);
            p.AddBool("x01IsCapacityFaNefa", rec.x01IsCapacityFaNefa);

            p.AddInt("x01LockFlag", rec.x01LockFlag);
            p.AddBool("x01IsAllowDuplicity_p86", rec.x01IsAllowDuplicity_p86);
            
           
            _db.CurrentUser.j02Login = "guru";

            _db.SaveRecord("x01License", p, rec);   //x01ID není identity
            if (intX01ID > 0)
            {                
                _db.RunSql("exec dbo.x01_aftersave @x01id,@j02id", new { x01id = intX01ID, j02id = _mother.CurrentUser.pid });
                if (rec.pid == 0)
                {
                    rec = Load(intX01ID);
                }                
                Handle_CreateFolders(intX01ID, rec);

                if (_mother.App.HostingMode == Singleton.HostingModeEnum.SharedApp) //u sdílené aplikace je třeba aktualizovat x01 v db [x01License]
                {
                    new DL.HostingTasks(_db).UpdateCloudHeader_X01();
                }


                _mother.App.RefreshX01List();    //obnovit singleton seznam licencí

            }

            return intX01ID;
        }

        private void Handle_CreateFolders(int intX01ID, BO.x01License rec)
        {
            if (rec.pid == 0) rec.pid = intX01ID;
            string s = $"{_mother.App.RootUploadFolder}\\_users";            
            Handle_CreateOneFolder(s, rec, rec.x01Guid + "------" + rec.x01LoginDomain);
            
            s= $"{_mother.App.RootUploadFolder}\\_users\\{rec.x01Guid}\\NOTEPAD";
            Handle_CreateOneFolder(s, rec, null);
            s = $"{_mother.App.RootUploadFolder}\\_users\\{rec.x01Guid}\\PLUGINS";
            Handle_CreateOneFolder(s, rec, null);
            if (!File.Exists($"{s}\\company_logo.png"))
            {
                File.Copy($"{_mother.App.RootUploadFolder}\\_distribution\\plugins\\company_logo.png", $"{s}\\company_logo.png");
            }
            

            s = $"{_mother.App.RootUploadFolder}\\{rec.x01LoginDomain}";
            Handle_CreateOneFolder(s,rec, "_"+rec.x01LoginDomain);
            s = $"{_mother.App.RootUploadFolder}\\{rec.x01LoginDomain}\\TEMP";
            Handle_CreateOneFolder(s, rec, null);
            s = $"{_mother.App.RootUploadFolder}\\{rec.x01LoginDomain}\\DELETED";
            Handle_CreateOneFolder(s, rec, null);
            s = $"{_mother.App.RootUploadFolder}\\{rec.x01LoginDomain}\\X31";
            Handle_CreateOneFolder(s, rec, null);



        }

        private void Handle_CreateOneFolder(string strFolder, BO.x01License rec,string strZnackaFileName)
        {
            if (!Directory.Exists(strFolder))
            {
                Directory.CreateDirectory(strFolder);

            }
            if (strZnackaFileName !=null && !File.Exists(strFolder + "\\" + strZnackaFileName))
            {
                BO.Code.File.WriteText2File(strFolder + "\\" + strZnackaFileName, rec.x01LoginDomain + ": " + rec.x01AppName + ", x01ID: " + rec.pid.ToString() + ", x01GUid: " + rec.x01Guid);
            }

        }
        public bool ValidateBeforeSave(BO.x01License rec)
        {
            if (!string.IsNullOrEmpty(rec.x01AppHost) && BO.Code.Bas.RightString(rec.x01AppHost,1)=="/")
            {
                rec.x01AppHost = BO.Code.Bas.LeftString(rec.x01AppHost, rec.x01AppHost.Length - 1);
            }
            if (string.IsNullOrEmpty(rec.x01ApiKey) || rec.x01ApiKey.Length>8)
            {
                rec.x01ApiKey = BO.Code.Bas.GetGuid().Substring(0, 8).ToUpper();
            }
            if (string.IsNullOrEmpty(rec.x01Name) || string.IsNullOrEmpty(rec.x01AppName) || string.IsNullOrEmpty(rec.x01CountryCode))
            {
                this.AddMessage("Povinná pole: Název, Menu název, ISO kód státu."); return false;
            }
            if (!string.IsNullOrEmpty(rec.x01RobotLogin) && _mother.App.HostingMode==Singleton.HostingModeEnum.SharedApp && !rec.x01RobotLogin.Contains(rec.x01LoginDomain))
            {
                this.AddMessageTranslated("Robot login je chybně zadaný."); return false;
            }

            var lis = _mother.x01LicenseBL.GetList(new BO.myQuery("x01") { IsRecordValid = null });
            if (lis.Any(p => p.pid != rec.pid && (p.x01Name.ToUpper() == rec.x01Name.ToUpper() || p.x01LoginDomain.ToUpper() == rec.x01LoginDomain.ToUpper())))
            {
                this.AddMessage("Název a doména musí být unikátní v rámci všech licencí v databázi."); return false;
            }

            if (string.IsNullOrEmpty(rec.x01RobotLogin))
            {
                this.AddMessage("Chybí vyplnit [Robot loging]."); return false;

            }


            return true;
        }

    }
}
