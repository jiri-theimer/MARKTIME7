using System.Collections.Generic;

namespace BL
{
    public interface Ia59RecPageLayerBL
    {
        public BO.a59RecPageLayer Load(int pid);
        public IEnumerable<BO.a59RecPageLayer> GetList(BO.myQuery mq);
        public int Save(BO.a59RecPageLayer rec,List<int> b02IDs);
        public BO.a59RecPageLayer LoadByWorkflow(int a55id, int b02id);


    }
    class a59RecPageLayerBL : BaseBL, Ia59RecPageLayerBL
    {
        public a59RecPageLayerBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,");
            sb(_db.GetSQL1_Ocas("a59"));
            sb(" FROM a59RecPageLayer a");
            sb(strAppend);
            return sbret();
        }
        public BO.a59RecPageLayer Load(int pid)
        {
            return _db.Load<BO.a59RecPageLayer>(GetSQL1(" WHERE a.a59ID=@pid"), new { pid = pid });
        }

        public BO.a59RecPageLayer LoadByWorkflow(int a55id,int b02id)
        {
            if (b02id > 0)
            {
                return _db.Load<BO.a59RecPageLayer>(GetSQL1(" WHERE a.a55ID=@a55id AND EXISTS (select a56ID FROM a56RecPageLayer_Status WHERE a59ID=a.a59ID AND b02ID=@b02id)"), new { a55id = a55id, b02id = b02id });
            }
            else
            {
                return _db.Load<BO.a59RecPageLayer>(GetSQL1(" WHERE a.a55ID=@a55id"), new { a55id = a55id });
            }
            
        }


        public IEnumerable<BO.a59RecPageLayer> GetList(BO.myQuery mq)
        {
            
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.a59RecPageLayer>(fq.FinalSql, fq.Parameters);
        }



        public int Save(BO.a59RecPageLayer rec, List<int> b02IDs)
        {
            if (ValidateBeforeSave(rec) == false)
            {
                return 0;
            }

            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddString("a59Name", rec.a59Name);
            p.AddInt("a55ID", rec.a55ID, true);
            p.AddEnumInt("a59StructureFlag", rec.a59StructureFlag);
            p.AddInt("a59ColumnsPerPage", rec.a59ColumnsPerPage);
            p.AddString("a59CssClassContainer", rec.a59CssClassContainer);

            p.AddString("a59Boxes", rec.a59Boxes);
            p.AddString("a59DockState", rec.a59DockState);
            p.AddString("a59CustomHtmlStructure", rec.a59CustomHtmlStructure);

            int intPID = _db.SaveRecord("a59RecPageLayer", p, rec);
            if (intPID > 0 && b02IDs !=null)
            {
                if (rec.pid > 0)
                {
                    _db.RunSql("DELETE FROM a56RecPageLayer_Status WHERE a59ID=@pid", new { pid = intPID });
                }
                if (b02IDs.Count > 0)
                {
                    _db.RunSql("INSERT INTO a56RecPageLayer_Status (a59ID,b02ID) SELECT @pid,b02ID FROM b02WorkflowStatus WHERE b02ID IN (" + string.Join(",", b02IDs) + ")", new { pid = intPID });
                }
                
            }


            return intPID;
        }

        public bool ValidateBeforeSave(BO.a59RecPageLayer rec)
        {
            if (rec.a55ID == 0)
            {
                this.AddMessage("Chybí vazba na šablonu web stránky."); return false;
            }
            if (string.IsNullOrEmpty(rec.a59Name))
            {
                this.AddMessage("Chybí vyplnit [Název]."); return false;
            }
           
            return true;
        }

    }
}
