

namespace BL
{
    public interface Ix31ReportBL
    {
        public BO.x31Report Load(int pid);
        public BO.x31Report LoadByCode(string code, int pid_exclude);
        public IEnumerable<BO.x31Report> GetList(BO.myQueryX31 mq);
        public int Save(BO.x31Report rec, List<BO.x69EntityRole_Assign> lisX69);
        public BO.o27Attachment LoadReportDoc(int x31id);
        public bool IsReportWaiting4Generate(DateTime dNow, BO.x31Report rec);
        //public BO.ThePeriod InhalePeriodFilter*/(BL.ThePeriodProvider pp);
        public string ParseExportFileNameMask(string strExportFileNameMask, string prefix, int pid);
        public void Clear_x31LastScheduledRun(int x31id);
    }
    class x31ReportBL : BaseBL, Ix31ReportBL
    {
        public x31ReportBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null,bool istestcloud=false)
        {
            sb("SELECT a.*,j25.j25Name,j25.j25Ordinary,j25.j25Code,o27x.o27ArchiveFileName as ReportFileName,o27x.o27ArchiveFolder as ReportFolder,");
            sb(_db.GetSQL1_Ocas("x31"));
            sb(" FROM x31Report a LEFT OUTER JOIN j25ReportCategory j25 ON a.j25ID=j25.j25ID");
            //if (_mother.CurrentUser.IsHostingModeTotalCloud)
            //{
            //    sb($" LEFT OUTER JOIN (select * from o27Attachment WHERE x01ID={_mother.CurrentUser.x01ID} AND o27Entity='x31' AND o27RecordPid IS NOT NULL) o27 ON a.x31ID=o27.o27RecordPid");
            //}
            //else
            //{
            //    sb(" LEFT OUTER JOIN (select * from o27Attachment WHERE o27Entity='x31' AND o27RecordPid IS NOT NULL) o27 ON a.x31ID=o27.o27RecordPid");
            //}
            sb(" LEFT OUTER JOIN o27Attachment o27x ON a.x31ID=o27x.o27RecordPid AND o27x.o27Entity='x31' AND o27x.o27RecordPid IS NOT NULL");
            
            
            if (istestcloud)
            {
                sb(AppendCloudQuery(strAppend));
            }
            else
            {
                sb(strAppend);
            }
            
            
            return sbret();
        }
        public BO.x31Report Load(int pid)
        {
            return _db.Load<BO.x31Report>(GetSQL1(" WHERE a.x31ID=@pid"), new { pid = pid });
        }
        public BO.x31Report LoadByCode(string code, int pid_exclude)
        {
            
            return _db.Load<BO.x31Report>(GetSQL1(" WHERE a.x31Code LIKE @code AND a.x31ID<>@pid_exclude",_mother.CurrentUser.IsHostingModeTotalCloud), new { code = code, pid_exclude = pid_exclude });
        }


        public IEnumerable<BO.x31Report> GetList(BO.myQueryX31 mq)
        {
            if (mq.explicit_orderby == null) mq.explicit_orderby = "j25.j25Ordinary,j25.j25Name,a.x31Ordinary,a.x31Name";
            
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.x31Report>(fq.FinalSql, fq.Parameters);            
        }



        public int Save(BO.x31Report rec, List<BO.x69EntityRole_Assign> lisX69)
        {
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }

            using (var sc = new System.Transactions.TransactionScope())
            {
                var p = new DL.Params4Dapper();
                p.AddInt("pid", rec.pid);
                p.AddInt("x01ID", rec.x01ID == 0 ? _mother.CurrentUser.x01ID : rec.x01ID, true);
                p.AddString("x31Entity", rec.x31Entity);
                p.AddInt("j25ID", rec.j25ID, true);
                p.AddString("x31Name", rec.x31Name);
                p.AddString("x31Code", rec.x31Code);
                p.AddEnumInt("x31FormatFlag", rec.x31FormatFlag);
                p.AddString("x31Description", rec.x31Description);
                //p.AddBool("x31IsUsableAsPersonalPage", rec.x31IsUsableAsPersonalPage);
                p.AddBool("x31IsScheduling", rec.x31IsScheduling);
                p.AddInt("x31Ordinary", rec.x31Ordinary);
                p.AddEnumInt("x31QueryFlag", rec.x31QueryFlag);

                p.AddBool("x31IsRunInDay1", rec.x31IsRunInDay1);
                p.AddBool("x31IsRunInDay2", rec.x31IsRunInDay2);
                p.AddBool("x31IsRunInDay3", rec.x31IsRunInDay3);
                p.AddBool("x31IsRunInDay4", rec.x31IsRunInDay4);
                p.AddBool("x31IsRunInDay5", rec.x31IsRunInDay5);
                p.AddBool("x31IsRunInDay6", rec.x31IsRunInDay6);
                p.AddBool("x31IsRunInDay7", rec.x31IsRunInDay7);
                p.AddString("x31RunInTime", rec.x31RunInTime);
                p.AddString("x31SchedulingReceivers", rec.x31SchedulingReceivers);
                p.AddInt("x21ID_Scheduling", rec.x21ID_Scheduling, true);

                p.AddString("x31DocSqlSource", rec.x31DocSqlSource);
                p.AddString("x31DocSqlSourceTabs", rec.x31DocSqlSourceTabs);

                p.AddString("x31ExportFileNameMask", rec.x31ExportFileNameMask);

                p.AddBool("x31IsPeriodRequired", rec.x31IsPeriodRequired);
                p.AddBool("x31IsAllowPfx", rec.x31IsAllowPfx);

                //p.AddInt("x31LangIndex", rec.x31LangIndex);
                p.AddDateTime("x31LastScheduledRun", rec.x31LastScheduledRun);

                int intPID = _db.SaveRecord("x31Report", p, rec);
                if (intPID > 0)
                {
                    if (lisX69 != null && !DL.BAS.SaveX69(_db, "x31", intPID, lisX69))
                    {
                        this.AddMessageTranslated("Error: DL.BAS.SaveX69");
                        return 0;
                    }
                }

                sc.Complete();

                return intPID;

            }


        }

        public bool ValidateBeforeSave(BO.x31Report rec)
        {
            if (string.IsNullOrEmpty(rec.x31Name))
            {
                this.AddMessage("Chybí vyplnit [Název sestavy]."); return false;
            }
            if (string.IsNullOrEmpty(rec.x31Code) && rec.x31FileName != null)
            {
                rec.x31Code = rec.x31FileName.Replace(".trdx", "");
            }
            if (string.IsNullOrEmpty(rec.x31Code))
            {
                this.AddMessage("Chybí vyplnit [Kód sestavy]."); return false;
            }
            
            if (rec.x31IsScheduling && rec.x31RunInTime != null)
            {
                int secs = BO.Code.Time.ConvertTimeToSeconds(rec.x31RunInTime);
                if (secs > (23 * 60 * 60))
                {
                    this.AddMessage("Čas notikace reportu může být technicky maximálně 23:00!");return false;
                }
            }
           
            if (LoadByCode(rec.x31Code, rec.pid) != null)
            {
                this.AddMessageTranslated(string.Format(_mother.tra("V systému existuje jiná sestava s kódem: {0}."), rec.x31Code)); return false;
            }


            return true;
        }

        public void Clear_x31LastScheduledRun(int x31id)
        {
            _db.RunSql("UPDATE x31Report SET x31LastScheduledRun=null WHERE x31ID=@pid", new { pid = x31id });    //vyčistit časovou stopu notifikačního reportu
        }

        public BO.o27Attachment LoadReportDoc(int x31id)
        {
            var mq = new BO.myQueryO27() { x31id=x31id };            
            var lisO27 = _mother.o27AttachmentBL.GetList(mq);

            if (lisO27.Count() > 0)
            {
                return lisO27.First();
            }

            return null;
        }

        public bool IsReportWaiting4Generate(DateTime dNow,BO.x31Report rec)
        {
            if (!rec.x31IsScheduling) return false;
            bool b = false;
            if (rec.x31IsRunInDay1 && dNow.DayOfWeek == DayOfWeek.Monday) b = true;
            if (rec.x31IsRunInDay2 && dNow.DayOfWeek == DayOfWeek.Tuesday) b = true;
            if (rec.x31IsRunInDay3 && dNow.DayOfWeek == DayOfWeek.Wednesday) b = true;
            if (rec.x31IsRunInDay4 && dNow.DayOfWeek == DayOfWeek.Thursday) b = true;
            if (rec.x31IsRunInDay5 && dNow.DayOfWeek == DayOfWeek.Friday) b = true;
            if (rec.x31IsRunInDay6 && dNow.DayOfWeek == DayOfWeek.Saturday) b = true;
            if (rec.x31IsRunInDay7 && dNow.DayOfWeek == DayOfWeek.Sunday) b = true;
            if (!b) return false;

           
            int secsNow = dNow.Hour * 60 * 60 + dNow.Minute * 60 + dNow.Second;
            if (secsNow >= BO.Code.Time.ConvertTimeToSeconds(rec.x31RunInTime))
            {
                if (rec.x31LastScheduledRun == null)
                {
                    return true;//sestava ještě nikdy nebyla generována
                }
                var d = Convert.ToDateTime(rec.x31LastScheduledRun);
                if (d.Day == dNow.Day && d.Month == dNow.Month && d.Year == dNow.Year)
                {
                    return false;   //dnes již byla generována
                }
                return true;
            }
            
            return false;

        }


        public string ParseExportFileNameMask(string strExportFileNameMask,string prefix,int pid)
        {
            if (pid==0 || prefix == null)
            {
                return strExportFileNameMask;
            }

            string strTab = BO.Code.Entity.GetEntity(prefix);
            if (strExportFileNameMask.Contains("|"))
            {
                //v názvu masky je i FROM klauzule
                var arr = strExportFileNameMask.Split("|");
                strExportFileNameMask = arr[0];
                strTab = arr[1];
            }
            
            string s = $"SELECT {strExportFileNameMask} as Value FROM {strTab} WHERE {prefix}ID = {pid}";
            s = DL.BAS.ParseMergeSQL(s, pid.ToString());

            var ret = _db.Load<BO.GetString>(s);
            if (ret != null)
            {
                ret.Value = ret.Value.Replace(".", "").Replace(",", "_");
                ret.Value = BO.Code.File.ConvertToSafeFileName(ret.Value);
                return ret.Value;
            }

            return null;
        }

    }
}
