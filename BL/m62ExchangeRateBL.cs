

namespace BL
{
    public interface Im62ExchangeRateBL
    {
        public BO.m62ExchangeRate Load(int pid);
        public BO.m62ExchangeRate LoadByQuery(BO.m62RateTypeENUM ratetype, int j27id_slave, int j27id_master, DateTime d, int pid_exclude);
        public IEnumerable<BO.m62ExchangeRate> GetList(BO.myQuery mq);
        public int Save(BO.m62ExchangeRate rec);
        public int ImportOneRate(HttpClient client, DateTime? d, int j27id_slave);

    }
    class m62ExchangeRateBL : BaseBL, Im62ExchangeRateBL
    {

        public m62ExchangeRateBL(BL.Factory mother) : base(mother)
        {

        }

        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,j27master.j27Code as j27Code_Master,j27slave.j27Code as j27Code_Slave," + _db.GetSQL1_Ocas("m62") + " FROM m62ExchangeRate a");
            sb(" INNER JOIN j27Currency j27master ON a.j27ID_Master=j27master.j27ID INNER JOIN j27Currency j27slave ON a.j27ID_Slave=j27slave.j27ID");

            sb(strAppend);
            return sbret();
        }

        public BO.m62ExchangeRate Load(int pid)
        {
            return _db.Load<BO.m62ExchangeRate>(GetSQL1(" WHERE a.m62ID=@pid"), new { pid = pid });
        }
       
        public BO.m62ExchangeRate LoadByQuery(BO.m62RateTypeENUM ratetype,int j27id_slave,int j27id_master,DateTime d, int pid_exclude)
        {
            return _db.Load<BO.m62ExchangeRate>(GetSQL1(" WHERE a.m62RateType=@ratetype AND a.j27ID_Slave=@j27id_slave AND a.j27ID_Master=@j27id_master AND a.m62Date=@d AND a.m62ID<>@pid_exclude AND a.x01ID=@x01id"), new { ratetype = (int) ratetype,j27id_slave=j27id_slave,j27id_master=j27id_master,d=d, pid_exclude = pid_exclude,x01id=_mother.CurrentUser.x01ID });
        }

        public IEnumerable<BO.m62ExchangeRate> GetList(BO.myQuery mq)
        {
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.m62ExchangeRate>(fq.FinalSql, fq.Parameters);
        }


        public int Save(BO.m62ExchangeRate rec)
        {
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }
            using (var sc = new System.Transactions.TransactionScope())
            {
                var p = new DL.Params4Dapper();

                p.AddInt("pid", rec.pid);
                p.AddInt("x01ID", rec.x01ID == 0 ? _mother.CurrentUser.x01ID : rec.x01ID);
                p.AddInt("j27ID_Master", rec.j27ID_Master, true);
                p.AddInt("j27ID_Slave", rec.j27ID_Slave, true);
                p.AddEnumInt("m62RateType", rec.m62RateType);
                p.AddDateTime("m62Date", rec.m62Date);
                p.AddDouble("m62Rate", rec.m62Rate);
                p.AddInt("m62Units", rec.m62Units);
               
                int intPID = _db.SaveRecord("m62ExchangeRate", p, rec);
                if (intPID > 0)
                {
                    _db.RunSql("exec dbo.m62_aftersave @m62id,@j03id_sys", new { m62id = intPID, j03id_sys = _mother.CurrentUser.pid });
                }
                sc.Complete();
                return intPID;
            }

        }


        private bool ValidateBeforeSave(BO.m62ExchangeRate rec)
        {            

            if (rec.j27ID_Master == 0)
            {
                this.AddMessage("Chybí zdrojová měna."); return false;
            }
            if (rec.j27ID_Master == 0)
            {
                this.AddMessage("Chybí cílová měna."); return false;
            }
            if (rec.j27ID_Master==rec.j27ID_Slave)
            {
                this.AddMessage("Zdrojová a cílová měna se musí lišit."); return false;
            }
            if (rec.m62Rate <= 0)
            {
                this.AddMessage("Hodnota kurzu musí být větší než nula."); return false;
            }
            if (rec.m62Units <= 0)
            {
                this.AddMessage("Množství musí být větší než nula."); return false;
            }
            var recExist = LoadByQuery(rec.m62RateType, rec.j27ID_Slave, rec.j27ID_Master, rec.m62Date, rec.pid);
            if (recExist != null)
            {
                this.AddMessageTranslated(_mother.tra("Pro tuto měnu a den již kurz existuje.") + ": " + recExist.Veta + " | " + BO.Code.Bas.ObjectDate2String(recExist.m62Date));
                    return false;
            }

            return true;
        }

        public int ImportOneRate(HttpClient client, DateTime? d, int j27id_slave)
        {            
            var recJ27 = _mother.FBL.LoadCurrencyByID(j27id_slave);
            if (recJ27 == null || d==null)
            {
                this.AddMessage("Na vstupu chybí měna nebo datum.");return 0;
            }
            string url = string.Format("http://www.cnb.cz/cs/financni_trhy/devizovy_trh/kurzy_devizoveho_trhu/denni_kurz.txt?date={0}", BO.Code.Bas.ObjectDate2String(d, "dd.MM.yyyy"));
            string strRaw = Handle_ImportCnb(client, Convert.ToDateTime(d)).Result;
            if (strRaw == null)
            {
                return 0;   //chybový stav
            }
            var rows = BO.Code.Bas.ConvertString2List(strRaw, "\n");

            string strDate = rows[0].Split(" ")[0];


            var qry = rows.Where(p => p.Contains("|" + recJ27.j27Code + "|"));

            if (qry.Count() > 0)
            {
                var pipes = BO.Code.Bas.ConvertString2List(qry.First(), "|");
                var c = new BO.m62ExchangeRate() {m62RateType=BO.m62RateTypeENUM.InvoiceRate, j27ID_Slave = j27id_slave, j27ID_Master = 2 };
                c.m62Date = BO.Code.Bas.String2Date(strDate);
                c.m62Units = BO.Code.Bas.InInt(pipes[2]);
                c.m62Rate = BO.Code.Bas.InDouble(pipes[4]);

                return Save(c);
            }

            return 0;

        }


        private async Task<string> Handle_ImportCnb(HttpClient client,DateTime d)
        {            
            
            string url = string.Format("http://www.cnb.cz/cs/financni_trhy/devizovy_trh/kurzy_devizoveho_trhu/denni_kurz.txt?date={0}", BO.Code.Bas.ObjectDate2String(d, "dd.MM.yyyy"));
            using (var request = new HttpRequestMessage(new HttpMethod("GET"), url))
            {
                try
                {
                    var response = await client.SendAsync(request);

                    var strRet = await response.Content.ReadAsStringAsync();

                    return strRet;
                }
                catch(Exception ex)
                {
                    BO.Code.File.LogError(ex.Message,"robot", "Handle_ImportCnb");
                    return null;
                }
                

            }


        }



    }
}
