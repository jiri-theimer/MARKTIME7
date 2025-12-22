using BO;

namespace BL
{
    public interface Ip53VatRateBL
    {
        public BO.p53VatRate Load(int pid);        
        public IEnumerable<BO.p53VatRate> GetList(BO.myQuery mq);
        public int Save(BO.p53VatRate rec, DateTime d1, DateTime d2);
        public double NajdiSazbu(DateTime? d, x15IdEnum x15id, int intJ27ID,string strCountryCode=null);
        public int NajdiX15ID(DateTime? d, double vatrate, int intJ27ID, string strCountryCode = null);
    }
    class p53VatRateBL : BaseBL, Ip53VatRateBL
    {
        public p53VatRateBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,x15.x15Name,j27.j27Code,");
            sb(_db.GetSQL1_Ocas("p53"));
            sb($" FROM p53VatRate a INNER JOIN x15VatRateType x15 ON a.x15ID=x15.x15ID AND a.x01ID={_mother.CurrentUser.x01ID} LEFT OUTER JOIN j27Currency j27 ON a.j27ID=j27.j27ID");

            sb(strAppend);
            return sbret();
        }
        public BO.p53VatRate Load(int pid)
        {
            return _db.Load<BO.p53VatRate>(GetSQL1(" WHERE a.p53ID=@pid"), new { pid = pid });
        }
       
        public IEnumerable<BO.p53VatRate> GetList(BO.myQuery mq)
        {
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.p53VatRate>(fq.FinalSql, fq.Parameters);
        }



        public int Save(BO.p53VatRate rec,DateTime d1,DateTime d2)
        {
            if (!ValidateBeforeSave(rec,d1,d2))
            {
                return 0;
            }
            using (var sc = new System.Transactions.TransactionScope())     //ukládání podléhá transakci{
            {
                var p = new DL.Params4Dapper();
                p.AddInt("pid", rec.pid);
                p.AddInt("j27ID", rec.j27ID, true);
                p.AddEnumInt("x15ID", rec.x15ID, true);
                p.AddInt("x01ID", rec.x01ID == 0 ? _mother.CurrentUser.x01ID : rec.x01ID);
                p.AddDouble("p53Value", rec.p53Value);
                p.AddString("p53CountryCode", rec.p53CountryCode);
                p.AddDateTime("p53ValidFrom", d1);
                p.AddDateTime("p53ValidUntil", d2);

                int intPID = _db.SaveRecord("p53VatRate", p, rec,false);
                if (intPID > 0)
                {
                    sc.Complete();                    
                }
                return intPID;
            }


        }
        private bool ValidateBeforeSave(BO.p53VatRate rec, DateTime d1, DateTime d2)
        {
            
            if (rec.x15ID == BO.x15IdEnum.Nic)
            {
                this.AddMessage("Chybí vyplnit [Hladina DPH]."); return false;
            }
            if (string.IsNullOrEmpty(rec.p53CountryCode))
            {
                this.AddMessage("Chybí vyplnit [ISO kód státu]."); return false;
            }
            var lis = GetList(new BO.myQuery("p53") { IsRecordValid=null});
            if (lis.Any(p=>p.pid !=rec.pid && p.x15ID==rec.x15ID && p.p53CountryCode==rec.p53CountryCode && p.j27ID==rec.j27ID && ((d1>=p.ValidFrom && d1<=p.ValidUntil) || (d2>=p.ValidFrom && d2<=p.ValidUntil))))
            {
                this.AddMessage("Pro zadané období, hladinu DPH, kód země a měnu již existuje záznam DPH sazby."); return false;
            }

            return true;
        }

        public double NajdiSazbu(DateTime? d,x15IdEnum x15id,int intJ27ID, string strCountryCode = null)
        {
            if (d == null) d = DateTime.Now;
            var lis = GetList(new BO.myQuery("p53") { IsRecordValid = null }).Where(p =>p.x15ID==x15id && p.ValidFrom <= d && p.ValidUntil >= d);
            if (intJ27ID > 0)
            {
                lis = lis.Where(p => p.j27ID == intJ27ID || p.j27ID == 0);
            }
            if (!string.IsNullOrEmpty(strCountryCode))
            {
                lis = lis.Where(p => p.p53CountryCode == strCountryCode || p.p53CountryCode == null);
            }
            if (lis.Count() == 0) return 0;
            if (lis.Count() ==1) return lis.First().p53Value;

            return lis.OrderByDescending(p => p.p53CountryCode).ThenByDescending(p=>p.j27ID).First().p53Value;
        }

        public int NajdiX15ID(DateTime? d, double vatrate, int intJ27ID, string strCountryCode = null)
        {
            if (d == null) d = DateTime.Now;
            var lis = GetList(new BO.myQuery("p53") { IsRecordValid = null }).Where(p => p.p53Value == vatrate && p.ValidFrom <= d && p.ValidUntil >= d);
            if (intJ27ID > 0)
            {
                lis = lis.Where(p => p.j27ID == intJ27ID || p.j27ID == 0);
            }
            if (!string.IsNullOrEmpty(strCountryCode))
            {
                lis = lis.Where(p => p.p53CountryCode == strCountryCode || p.p53CountryCode == null);
            }
            if (lis.Count() == 0)
            {
                return 0;
            }
            else
            {
                return (int)lis.First().x15ID;
            }
            

            
        }

    }
}
