using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    public interface Ix04NotepadConfigBL
    {



        public BO.x04NotepadConfig Load(int pid);

        public int Save(BO.x04NotepadConfig rec);
        public IEnumerable<BO.x04NotepadConfig> GetList(BO.myQuery mq);


    }

    class x04NotepadConfigBL : BaseBL, Ix04NotepadConfigBL
    {
        public x04NotepadConfigBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,");
            sb(_db.GetSQL1_Ocas("x04"));
            sb(" FROM x04NotepadConfig a");
            sb(strAppend);
            return sbret();
        }

        public BO.x04NotepadConfig Load(int pid)
        {
            return _db.Load<BO.x04NotepadConfig>(GetSQL1(" WHERE a.x04ID=@pid"), new { pid = pid });
        }

        public IEnumerable<BO.x04NotepadConfig> GetList(BO.myQuery mq)
        {
            if (mq.explicit_orderby == null) mq.explicit_orderby = "a.x04Ordinary";
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.x04NotepadConfig>(fq.FinalSql, fq.Parameters);
        }







        public int Save(BO.x04NotepadConfig rec)
        {
            if (ValidateBeforeSave(rec) == false)
            {
                return 0;
            }

            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddInt("x01ID", rec.x01ID == 0 ? _mother.CurrentUser.x01ID : rec.x01ID, true);
            p.AddString("x04Name", rec.x04Name);
            p.AddInt("x04Ordinary", rec.x04Ordinary);
            p.AddBool("x04IsToolbarInline", rec.x04IsToolbarInline);
            p.AddBool("x04IsToolbarSticky", rec.x04IsToolbarSticky);
            p.AddBool("x04IsTrackChanges", rec.x04IsTrackChanges);
            p.AddInt("x04ImageMaxSize", rec.x04ImageMaxSize);
            p.AddInt("x04FileMaxSize", rec.x04FileMaxSize);

            p.AddString("x04ToolbarButtons", rec.x04ToolbarButtons);
            p.AddString("x04ToolbarButtonsXS", rec.x04ToolbarButtonsXS);
            p.AddString("x04PlaceHolder", rec.x04PlaceHolder);
            p.AddString("x04FileAllowedTypes", rec.x04FileAllowedTypes);
            p.AddString("x04ImageAllowedTypes", rec.x04ImageAllowedTypes);

           

            return _db.SaveRecord("x04NotepadConfig", p, rec);
        }



        public bool ValidateBeforeSave(BO.x04NotepadConfig rec)
        {
            
            if (string.IsNullOrEmpty(rec.x04Name))
            {
                this.AddMessage("Chybí název konfigurace."); return false;
            }


            return true;
        }

    }
}
