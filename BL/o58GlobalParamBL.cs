

using DocumentFormat.OpenXml.Drawing;

namespace BL
{
    public interface Io58GlobalParamBL
    {
        public BO.o58GlobalParam Load(int pid);
        public IEnumerable<BO.o58GlobalParam> GetList(BO.myQuery mq);
        public int Save(BO.o58GlobalParam rec);
        public IEnumerable<BO.o59GlobalParamBinding> GetList_o59(string prefix, int pid);
        public int SaveParamBinding(BO.o59GlobalParamBinding rec);
        public string GetGlobalParamValue(string o58key, int pid);

    }
    class o58GlobalParamBL : BaseBL, Io58GlobalParamBL
    {
        public o58GlobalParamBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,");
            sb(_db.GetSQL1_Ocas("o58"));
            sb(" FROM o58GlobalParam a");
            sb(strAppend);
            return sbret();
        }
        

        public BO.o58GlobalParam Load(int pid)
        {
            return _db.Load<BO.o58GlobalParam>(GetSQL1(" WHERE a.o58ID=@pid"), new { pid = pid });
        }

        public IEnumerable<BO.o58GlobalParam> GetList(BO.myQuery mq)
        {
            if (mq.explicit_orderby == null) { mq.explicit_orderby = "a.o58Ordinary"; }

            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.o58GlobalParam>(fq.FinalSql, fq.Parameters);
        }



        public int Save(BO.o58GlobalParam rec)
        {
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }
            var p = new DL.Params4Dapper();            
            p.AddInt("pid", rec.pid);
            p.AddInt("x01ID", rec.x01ID == 0 ? _mother.CurrentUser.x01ID : rec.x01ID, true);
            p.AddString("o58Entity", rec.o58Entity);
            p.AddEnumInt("x24ID", rec.x24ID,true);
            p.AddString("o58Key", rec.o58Key);
            p.AddString("o58Name", rec.o58Name);
            p.AddInt("o58Ordinary", rec.o58Ordinary);
            p.AddBool("o58IsPerUser", rec.o58IsPerUser);

            if (rec.pid == 0)
            {
                var intPID = _db.GetIntegerFromSql("select max(o58ID) FROM o58GlobalParam") + 1;
                return _db.SaveRecord("o58GlobalParam", p, rec,true,true, intPID);
            }
            else
            {
                return _db.SaveRecord("o58GlobalParam", p, rec);
            }

            
        }
        private bool ValidateBeforeSave(BO.o58GlobalParam rec)
        {
            if (string.IsNullOrEmpty(rec.o58Name))
            {
                this.AddMessage("Chybí vyplnit [Název]."); return false;
            }
            if (string.IsNullOrEmpty(rec.o58Key))
            {
                this.AddMessage("Chybí vyplnit [Kód]."); return false;
            }
            if (string.IsNullOrEmpty(rec.o58Entity))
            {
                this.AddMessage("Chybí vyplnit [Entita]."); return false;
            }

            if (_db.GetIntegerFromSql($"select o58ID FROM o58GlobalParam WHERE o58Key like '{rec.o58Key}' AND o58ID<>{rec.pid}") > 0)
            {
                this.AddMessage("Kód nemůže být duplicitní."); return false;
            }
            return true;
        }


        private string GetSQL1_o59(string strAppend = null)
        {
            sb("SELECT a.*,o58.o58Name,o58.o58IsPerUser,j02.j02Name,o58.x24ID,");
            sb(_db.GetSQL1_Ocas("o59",false,false,false));
            sb(" FROM o59GlobalParamBinding a INNER JOIN o58GlobalParam o58 ON a.o58ID=o58.o58ID");
            sb(" LEFT OUTER JOIN j02User j02 ON a.j02ID=j02.j02ID");
            sb(strAppend);
            
            return sbret();
        }

        public IEnumerable<BO.o59GlobalParamBinding> GetList_o59(string prefix, int pid)
        {
            var s = GetSQL1_o59(" WHERE o58.o58Entity=@prefix AND a.o59RecordPid=@pid ORDER BY o58.o58Ordinary,a.o58ID");
            return _db.GetList<BO.o59GlobalParamBinding>(s, new { prefix = prefix, pid = pid });
        }

        public int SaveParamBinding(BO.o59GlobalParamBinding rec)
        {
            if (rec.IsForDelete)
            {
                if (rec.pid > 0)
                {
                    _db.RunSql("DELETE FROM o59GlobalParamBinding WHERE o59ID=@pid", new { pid = rec.pid });
                }
                return rec.pid;
            }
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddInt("o58ID", rec.o58ID, true);
            p.AddInt("j02ID", rec.j02ID, true);
            p.AddInt("o59RecordPid", rec.o59RecordPid, true);

            p.AddString("o59ValueString", rec.o59ValueString);
            p.AddDouble("o59ValueNum", rec.o59ValueNum);
            p.AddDateTime("o59ValueDate", rec.o59ValueDate);
            p.AddBool("o59ValueBoolean", rec.o59ValueBoolean);
            p.AddString("o59Memo", rec.o59Memo);



            return _db.SaveRecord("o59GlobalParamBinding", p, rec,false,false);

            
        }


        public string GetGlobalParamValue(string o58key, int pid)
        {
            BO.GetString val = _db.Load<BO.GetString>("SELECT a.o59Value as Value FROM o59GlobalParamBinding a INNER JOIN o58GlobalParam b ON a.o58ID=b.o58ID WHERE b.o58Key LIKE @klic AND o59RecordPid=@record_pid", new { klic = o58key, record_pid = pid });
            return (val == null ? null : val.Value);
        }

        public string GetGlobalParamValue(int o58id, int pid)
        {
            BO.GetString val = _db.Load<BO.GetString>("SELECT o59Value as Value FROM o59GlobalParamBinding WHERE o58ID=@o58id AND o59RecordPid=@record_pid", new { o58id = o58id, record_pid = pid });
            return (val == null ? null : val.Value);
        }
    }
}
