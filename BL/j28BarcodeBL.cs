using BO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    public interface Ij28BarcodeBL
    {
        public BO.j28Barcode Load(int pid);
        public BO.j28Barcode LoadByValue(string strCode, int pid_exclude);
        public IEnumerable<BO.j28Barcode> GetList(string record_prefix, int record_pid);
        public IEnumerable<BO.j28Barcode> GetList(string record_prefix,int record_pid, string tempguid);
        public IEnumerable<BO.j28Barcode> GetList(string tempguid);
        public int Save(BO.j28Barcode rec);

    }
    class j28BarcodeBL : BaseBL, Ij28BarcodeBL
    {
        public j28BarcodeBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,");
            sb(_db.GetSQL1_Ocas("j28"));
            sb(" FROM j28Barcode a");
            sb(strAppend);
            return sbret();
        }
        public BO.j28Barcode Load(int pid)
        {
            return _db.Load<BO.j28Barcode>(GetSQL1(" WHERE a.j28ID=@pid"), new { pid = pid });
        }
        public BO.j28Barcode LoadByValue(string strCode, int pid_exclude)
        {
            if (pid_exclude > 0)
            {
                return _db.Load<BO.j28Barcode>(GetSQL1(" WHERE a.j28Value LIKE @code AND a.j28ID<>@pid_exclude AND a.x01ID=@x01id"), new { code = strCode, pid_exclude = pid_exclude, x01id = _mother.CurrentUser.x01ID });
            }
            else
            {
                return _db.Load<BO.j28Barcode>(GetSQL1(" WHERE a.j28Value LIKE @code AND a.x01ID=@x01id"), new { code = strCode,x01id=_mother.CurrentUser.x01ID });
            }
        }

        public IEnumerable<BO.j28Barcode> GetList(string record_prefix, int record_pid)
        {
            if (_mother.CurrentUser.IsHostingModeTotalCloud)
            {
                return _db.GetList<BO.j28Barcode>(GetSQL1(" WHERE a.x01ID=@x01id AND a.j28RecordEntity=@prefix AND a.j28RecordPid=@pid"), new {x01id=_mother.CurrentUser.x01ID, prefix = record_prefix, pid = record_pid });
            }
            else
            {
                return _db.GetList<BO.j28Barcode>(GetSQL1(" WHERE a.j28RecordEntity=@prefix AND a.j28RecordPid=@pid"), new { prefix = record_prefix, pid = record_pid });
            }
            
        }
        public IEnumerable<BO.j28Barcode> GetList(string record_prefix, int record_pid, string tempguid)
        {
                       
            if (_mother.CurrentUser.IsHostingModeTotalCloud)
            {
                return _db.GetList<BO.j28Barcode>(GetSQL1(" WHERE a.x01ID=@x01id AND ((a.j28RecordEntity=@prefix AND a.j28RecordPid=@pid) OR j28TempGuid=@guid)"), new {x01id=_mother.CurrentUser.x01ID, prefix = record_prefix, pid = record_pid, guid = tempguid });
            }
            else
            {
                return _db.GetList<BO.j28Barcode>(GetSQL1(" WHERE (a.j28RecordEntity=@prefix AND a.j28RecordPid=@pid) OR j28TempGuid=@guid"), new { prefix = record_prefix, pid = record_pid, guid = tempguid });
            }
            
        }
        public IEnumerable<BO.j28Barcode> GetList(string tempguid)
        {
            return _db.GetList<BO.j28Barcode>(GetSQL1(" WHERE a.j28TempGuid=@guid"), new { guid = tempguid });
        }


        public int Save(BO.j28Barcode rec)
        {
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddInt("x01ID", rec.x01ID == 0 ? _mother.CurrentUser.x01ID : rec.x01ID, true);
            p.AddString("j28Value", rec.j28Value);
            p.AddString("j28RecordEntity", rec.j28RecordEntity);
            p.AddInt("j28RecordPid", rec.j28RecordPid);
            p.AddString("j28TempGuid", rec.j28TempGuid);

            return _db.SaveRecord("j28Barcode", p, rec);

        }
        private bool ValidateBeforeSave(BO.j28Barcode rec)
        {
            if (string.IsNullOrEmpty(rec.j28Value))
            {
                this.AddMessage("Chybí vyplnit hodnota čárového kódu."); return false;
            }


            return true;
        }

    }
}
