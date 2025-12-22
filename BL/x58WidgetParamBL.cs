using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace BL
{
    public interface Ix58WidgetParamBL
    {
        public BO.x58WidgetParam Load(int pid);
        public BO.x58WidgetParam LoadByName(string name, int pid_exclude);
        public IEnumerable<BO.x58WidgetParam> GetList(BO.myQuery mq);
        public int Save(BO.x58WidgetParam rec);

      

    }

    class x58WidgetParamBL : BaseBL, Ix58WidgetParamBL
    {
        public x58WidgetParamBL(BL.Factory mother) : base(mother)
        {

        }

        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,");
            sb(_db.GetSQL1_Ocas("x58"));
            sb(" FROM x58WidgetParam a");
            sb(strAppend);
            return sbret();
        }
        public BO.x58WidgetParam Load(int pid)
        {
            return _db.Load<BO.x58WidgetParam>(GetSQL1(" WHERE a.x58ID=@pid"), new { pid = pid });
        }
        public BO.x58WidgetParam LoadByName(string name, int pid_exclude)
        {
            return _db.Load<BO.x58WidgetParam>(GetSQL1(" WHERE a.x58Name LIKE @name AND a.x58ID<>@pid_exclude"), new { name = name, pid_exclude = pid_exclude });
        }

        public IEnumerable<BO.x58WidgetParam> GetList(BO.myQuery mq)
        {
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.x58WidgetParam>(fq.FinalSql, fq.Parameters);
        }



        public int Save(BO.x58WidgetParam rec)
        {

            if (ValidateBeforeSave(rec) == false)
            {
                return 0;
            }
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.x58ID);
            p.AddInt("x01ID", rec.x01ID == 0 ? _mother.CurrentUser.x01ID : rec.x01ID);
            p.AddString("x58Name", rec.x58Name);
            p.AddString("x58LabelText", rec.x58LabelText);
            p.AddEnumInt("x58FormatFlag", rec.x58FormatFlag);
            p.AddString("x58Datasource", rec.x58Datasource);            
            p.AddInt("x58Ordinal", rec.x58Ordinal);            
            p.AddString("x58Description", rec.x58Description);           
            p.AddBool("x58IsAllowEmptyValue", rec.x58IsAllowEmptyValue);            
           
            int intPID = _db.SaveRecord("x58WidgetParam", p, rec);
            

            return intPID;
        }

        public bool ValidateBeforeSave(BO.x58WidgetParam rec)
        {
            if (string.IsNullOrEmpty(rec.x58Name))
            {
                this.AddMessage("Chybí vyplnit [Název]."); return false;
            }
           
            rec.x58Name = Regex.Replace(rec.x58Name, "[^a-zA-Z0-9]", "_"); //kód raději pouze pro alfanumerické znaky

            if (LoadByName(rec.x58Name, rec.pid) != null)
            {
                this.AddMessageTranslated(string.Format(_mother.tra("V systému již existuje jiný parametr s názvem: {0}."), rec.x58Name)); return false;
            }


        
            return true;
        }
    }
}
