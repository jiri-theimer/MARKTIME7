using System;
using System.Collections.Generic;
using System.Text;

namespace BL
{
    public interface Ix51HelpCoreBL
    {
        public BO.x51HelpCore Load(int pid);
        public BO.x51HelpCore LoadByViewUrl(string viewurl);
        public BO.x51HelpCore LoadByNearUrl(string nearurl);
        public string LoadHtmlContent(int pid);
        public IEnumerable<BO.x51HelpCore> GetList(BO.myQuery mq);
        public int Save(BO.x51HelpCore rec);
        

    }
    class x51HelpCoreBL : BaseBL, Ix51HelpCoreBL
    {
        public x51HelpCoreBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.x51ID,a.x51Name,a.x51ViewUrl,a.x51ExternalUrl,a.x51Ordinary,a.x51Html,a.x51ParentID,a.x51TreePath,a.x51TreeLevel,a.x51TreeIndex,a.x51TreePrev,a.x51TreeNext,a.x51NearUrls,");
            
            sb(_db.GetSQL1_Ocas("x51"));
            sb(" FROM x51HelpCore a");
            sb(strAppend);
            return sbret();
        }
        public BO.x51HelpCore Load(int pid)
        {
            return _db.Load<BO.x51HelpCore>(GetSQL1(" WHERE a.x51ID=@pid"), new { pid = pid });
        }
        public BO.x51HelpCore LoadByViewUrl(string viewurl)
        {
            return _db.Load<BO.x51HelpCore>(GetSQL1(" WHERE a.[x51ViewUrl] LIKE '%'+@url+'%'"), new { url = viewurl });
        }
        public BO.x51HelpCore LoadByNearUrl(string nearurl)
        {
            return _db.Load<BO.x51HelpCore>(GetSQL1(" WHERE a.[x51NearUrls] LIKE '%'+@url+'%'"), new { url = nearurl });
        }
        public string LoadHtmlContent(int pid)
        {
            return _db.Load<BO.GetString>("SELECT a.x51Html as Value FROM x51HelpCore a WHERE a.x51ID=@pid", new { pid = pid }).Value;
        }
        public IEnumerable<BO.x51HelpCore> GetList(BO.myQuery mq)
        {
            if (mq.explicit_orderby == null) mq.explicit_orderby = "a.x51TreeIndex";
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.x51HelpCore>(fq.FinalSql, fq.Parameters);
        }



        public int Save(BO.x51HelpCore rec)
        {
            if (ValidateBeforeSave(rec) == false)
            {
                return 0;
            }
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddString("x51Name", rec.x51Name);
            p.AddString("x51ViewUrl", rec.x51ViewUrl);
            p.AddString("x51ExternalUrl", rec.x51ExternalUrl);
            p.AddString("x51Html", rec.x51Html);           
            p.AddInt("x51Ordinary", rec.x51Ordinary);
            p.AddInt("x51ParentID", rec.x51ParentID,true);
            p.AddString("x51NearUrls", rec.x51NearUrls);

            int intPID = _db.SaveRecord("x51HelpCore", p, rec);
            if (intPID > 0)
            {
                _db.RunSql("exec dbo.x51_recalc_tree");
            }


            return intPID;
        }

        public bool ValidateBeforeSave(BO.x51HelpCore rec)
        {
            if (string.IsNullOrEmpty(rec.x51Name))
            {
                this.AddMessage("Chybí vyplnit [Název]."); return false;
            }


            return true;
        }

    }
}
