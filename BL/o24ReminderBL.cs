using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    public interface Io24ReminderBL
    {
        public BO.o24Reminder Load(int pid);
        public IEnumerable<BO.o24Reminder> GetList(string record_prefix, int record_pid);
        public IEnumerable<BO.o24Reminder> GetList(string record_prefix, string record_pids);
        public IEnumerable<BO.o24Reminder> GetList_ddx(int o23id);
        public IEnumerable<BO.o24Reminder> GetList_ddx_or_classic(int o23id);
        public int Save(BO.o24Reminder rec);
        public IEnumerable<BO.o24Reminder> GetList_Wait_On_Process();

    }
    class o24ReminderBL : BaseBL, Io24ReminderBL
    {
        public o24ReminderBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,j02.j02LastName+' '+j02.j02FirstName as Person,j11.j11Name,x67.x67Name,p28.p28Name,p24.p24Name,");
            sb(_db.GetSQL1_Ocas("o24"));
            sb(" FROM o24Reminder a LEFT OUTER JOIN j02User j02 ON a.j02ID=j02.j02ID LEFT OUTER JOIN j11Team j11 ON a.j11ID=j11.j11ID LEFT OUTER JOIN x67EntityRole x67 ON a.x67ID=x67.x67ID");
            sb(" LEFT OUTER JOIN p28Contact p28 ON a.p28ID=p28.p28ID LEFT OUTER JOIN p24ContactGroup p24 ON a.p24ID=p24.p24ID");
            sb(strAppend);
            return sbret();
        }
        public BO.o24Reminder Load(int pid)
        {
            return _db.Load<BO.o24Reminder>(GetSQL1(" WHERE a.o24ID=@pid"), new { pid = pid });
        }
        public IEnumerable<BO.o24Reminder> GetList_ddx(int o23id)
        {
            var mq = new BO.myQuery("o24") { explicit_sqlwhere = $"a.o24RecordEntity LIKE 'dd%' AND a.o24RecordPid={o23id}" };
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.o24Reminder>(fq.FinalSql, fq.Parameters);
        }
        public IEnumerable<BO.o24Reminder> GetList(string record_prefix,int record_pid)
        {
            var mq = new BO.myQuery("o24") {IsRecordValid=null, explicit_sqlwhere = $"a.o24RecordEntity='{record_prefix}' AND a.o24RecordPid={record_pid}" };
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.o24Reminder>(fq.FinalSql, fq.Parameters);
        }
        public IEnumerable<BO.o24Reminder> GetList_ddx_or_classic(int o23id)
        {
            var mq = new BO.myQuery("o24") { explicit_sqlwhere = $"(a.o24RecordEntity LIKE 'dd%' OR a.o24RecordEntity='o23') AND a.o24RecordPid={o23id}" };
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.o24Reminder>(fq.FinalSql, fq.Parameters);
        }
        public IEnumerable<BO.o24Reminder> GetList(string record_prefix, string record_pids)
        {
            if (string.IsNullOrEmpty(record_pids)) return null;
            var mq = new BO.myQuery("o24") {IsRecordValid=null, explicit_sqlwhere = $"a.o24RecordEntity='{record_prefix}' AND a.o24RecordPid IN ({record_pids})" };
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.o24Reminder>(fq.FinalSql, fq.Parameters);
        }
        public IEnumerable<BO.o24Reminder> GetList_Wait_On_Process()
        {
            var mq = new BO.myQuery("o24") { explicit_sqlwhere = $"a.o24DatetimeProcessed IS NULL AND GETDATE() BETWEEN a.o24ValidFrom AND a.o24ValidUntil AND (a.o24StaticDate IS NOT NULL OR a.o24RecordDate IS NOT NULL) AND a.o24RecordEntity<>'p58' AND a.x01ID={_mother.CurrentUser.x01ID}" };
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.o24Reminder>(fq.FinalSql, fq.Parameters);
        }


        public int Save(BO.o24Reminder rec)
        {
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddInt("x01ID", rec.x01ID == 0 ? _mother.CurrentUser.x01ID : rec.x01ID, true);
            p.AddInt("o24RecordPid", rec.o24RecordPid, true);            
            p.AddString("o24RecordEntity", rec.o24RecordEntity);
            p.AddEnumInt("o24MediumFlag", rec.o24MediumFlag);
            p.AddString("o24Unit", rec.o24Unit);
            p.AddInt("o24Count", rec.o24Count);
            p.AddInt("j02ID", rec.j02ID,true);
            p.AddInt("j11ID", rec.j11ID, true);
            p.AddInt("x67ID", rec.x67ID, true);
            p.AddInt("p24ID", rec.p24ID, true);
            p.AddInt("p28ID", rec.p28ID, true);
            p.AddString("o24Memo", rec.o24Memo);
            p.AddDateTime("o24StaticDate", rec.o24StaticDate);
            p.AddDateTime("o24DatetimeProcessed", rec.o24DatetimeProcessed);
            p.AddDateTime("o24StaticDate", rec.o24StaticDate);
            p.AddDateTime("o24RecordDate", rec.o24RecordDate);

            return _db.SaveRecord("o24Reminder", p, rec);

        }
        private bool ValidateBeforeSave(BO.o24Reminder rec)
        {
            if (string.IsNullOrEmpty(rec.o24RecordEntity))
            {
                this.AddMessage("Chybí vyplnit [Entita]."); return false;
            }
            if (rec.o24RecordPid == 0)
            {
                this.AddMessage("Chybí vyplnit [Záznam]."); return false;
            }


            return true;
        }

    }
}
