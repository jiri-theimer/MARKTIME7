using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    public interface Ij18CostUnitBL
    {
        public BO.j18CostUnit Load(int pid);
        public BO.j18CostUnit LoadByCode(string strCode, int pid_exclude);
        public IEnumerable<BO.j18CostUnit> GetList(BO.myQuery mq);
        public int Save(BO.j18CostUnit rec, List<BO.x69EntityRole_Assign> lisX69);

    }
    class j18CostUnitBL : BaseBL, Ij18CostUnitBL
    {
        public j18CostUnitBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            
            sb("SELECT a.*,");
            sb(_db.GetSQL1_Ocas("j18"));
            sb(" FROM j18CostUnit a");
            sb(strAppend);
            return sbret();
        }
        public BO.j18CostUnit Load(int pid)
        {
            return _db.Load<BO.j18CostUnit>(GetSQL1(" WHERE a.j18ID=@pid"), new { pid = pid });
        }
        public BO.j18CostUnit LoadByCode(string strCode, int pid_exclude)
        {
            if (pid_exclude > 0)
            {
                return _db.Load<BO.j18CostUnit>(GetSQL1(" WHERE a.j18Code LIKE @code AND a.j18ID<>@pid_exclude AND a.x01ID=@x01id"), new { code = strCode, pid_exclude = pid_exclude, x01id = _mother.CurrentUser.x01ID });
            }
            else
            {
                return _db.Load<BO.j18CostUnit>(GetSQL1(" WHERE a.j18Code LIKE @code AND a.x01ID=@x01id"), new { code = strCode,x01id=_mother.CurrentUser.x01ID });
            }
        }

        public IEnumerable<BO.j18CostUnit> GetList(BO.myQuery mq)
        {
            mq.x01id = _mother.CurrentUser.x01ID;
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.j18CostUnit>(fq.FinalSql, fq.Parameters);
        }



        public int Save(BO.j18CostUnit rec, List<BO.x69EntityRole_Assign> lisX69)
        {
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }
            using (var sc = new System.Transactions.TransactionScope())     //ukládání podléhá transakci{
            {
                var p = new DL.Params4Dapper();
                p.AddInt("pid", rec.pid);
                p.AddInt("x01ID", rec.x01ID == 0 ? _mother.CurrentUser.x01ID : rec.x01ID);
                
                p.AddString("j18Name", rec.j18Name);
                p.AddInt("j18Ordinary", rec.j18Ordinary);
                p.AddString("j18Code", rec.j18Code);
                p.AddString("j18CountryCode", rec.j18CountryCode);

                int intPID = _db.SaveRecord("j18CostUnit", p, rec);
                if (intPID > 0)
                {
                    if (lisX69 != null)
                    {
                        DL.BAS.SaveX69(_db, "j18", intPID, lisX69);
                    }

                    
                    //var pars = new Dapper.DynamicParameters();
                    //{
                    //    pars.Add("j18id", intPID, System.Data.DbType.Int32);
                    //    pars.Add("j02id_sys", _mother.CurrentUser.pid, System.Data.DbType.Int32);
                    //}

                    //if (_db.RunSp("j18_aftersave", ref pars, false) == "1")
                    //{
                    //    sc.Complete();
                    //    return intPID;
                    //}
                    //else
                    //{
                    //    return 0;
                    //}

                    sc.Complete();
                }


                return intPID;
            }
            

        }
        private bool ValidateBeforeSave(BO.j18CostUnit rec)
        {
            if (string.IsNullOrEmpty(rec.j18Name))
            {
                this.AddMessage("Chybí vyplnit [Název]."); return false;
            }
            if (LoadByCode(rec.j18Code, rec.pid) != null)
            {
                this.AddMessageTranslated(string.Format(_mother.tra("Kód [{0}] již obsadilo jiné středisko."), rec.j18Code));
                return false;
            }

            return true;
        }

    }
}
