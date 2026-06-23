

namespace BL
{
    public interface Io60SpisBL
    {
        public BO.o60Spis Load(int pid);
        public IEnumerable<BO.o60Spis> GetList(BO.myQuery mq);
        public int Save(BO.o60Spis rec);

    }
    class o60SpisBL : BaseBL, Io60SpisBL
    {
        public o60SpisBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,p41.p41Name,p41.p41Code,");
            sb(_db.GetSQL1_Ocas("o60"));
            sb(" FROM o60Spis a INNER JOIN p41Project p41 ON a.p41ID=p41.p41ID");
            sb(strAppend);
            return sbret();
        }
        public BO.o60Spis Load(int pid)
        {
            return _db.Load<BO.o60Spis>(GetSQL1(" WHERE a.o60ID=@pid"), new { pid = pid });
        }

        public IEnumerable<BO.o60Spis> GetList(int p41id)
        {            
            return _db.GetList<BO.o60Spis>(GetSQL1(" WHERE a.p41ID=@p41id"), new { p41id = p41id });
        }



        public int Save(BO.o60Spis rec)
        {
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddInt("p41ID", rec.p41ID, true);
            p.AddString("o60Name", rec.o60Name);
            p.AddString("o60SpisCode", rec.o60SpisCode);
            p.AddString("o60SoudCode", rec.o60SoudCode);
            p.AddString("o60Prijemce", rec.o60Prijemce);

            return _db.SaveRecord("o60Spis", p, rec);
        }
        private bool ValidateBeforeSave(BO.o60Spis rec)
        {
            if (rec.p41ID == 0)
            {
                this.AddMessage("Chybí vyplnit [Projekt]."); return false;
            }
            if (string.IsNullOrEmpty(rec.o60SpisCode))
            {
                this.AddMessage("Chybí vyplnit [Spisová značka]."); return false;
            }
            if (string.IsNullOrEmpty(rec.o60SoudCode))
            {
                this.AddMessage("Chybí vyplnit [Kód soudu]."); return false;
            }
            return true;
        }

    }
}
