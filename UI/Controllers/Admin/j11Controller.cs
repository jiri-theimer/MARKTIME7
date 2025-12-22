using Microsoft.AspNetCore.Mvc;
using UI.Models;
using UI.Models.Record;

namespace UI.Controllers
{
    public class j11Controller : BaseController
    {
        public IActionResult Record(int pid, bool isclone)
        {
            var v = new j11Record() { rec_pid = pid, rec_entity = "j11" };
            v.Rec = new BO.j11Team();
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.j11TeamBL.Load(v.rec_pid);
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }
                
                
                v.j02IDs = string.Join(",", Factory.j02UserBL.GetList(new BO.myQueryJ02() { j11id = v.rec_pid }).Select(p => p.pid));
            }
            v.Toolbar = new MyToolbarViewModel(v.Rec);
            if (isclone)
            {
                v.MakeClone();
            }

            RefreshState(v);

            return ViewTup(v, BO.PermValEnum.GR_Admin);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(Models.Record.j11Record v,string pids,string prefix)
        {
            var lisJ02IDs = BO.Code.Bas.ConvertString2ListInt(v.j02IDs);
            if (v.PostbackOper=="add" && prefix == "j02")
            {
                lisJ02IDs.AddRange(BO.Code.Bas.ConvertString2ListInt(pids));
            }
            if (v.PostbackOper == "add" && prefix == "j07")
            {
                var lis=Factory.j02UserBL.GetList(new BO.myQueryJ02() {IsRecordValid=true, j07id = BO.Code.Bas.InInt(pids) });
                lisJ02IDs.AddRange(lis.Select(p=>p.pid).ToList());
            }
            if (v.PostbackOper == "remove" && prefix == "j02")
            {               
                foreach(int x in BO.Code.Bas.ConvertString2ListInt(pids))
                {
                    lisJ02IDs.Remove(x);

                }
            }            

            if (v.IsPostback)
            {
                v.j02IDs = string.Join(",", lisJ02IDs);
                RefreshState(v);
                return View(v);
            }

            RefreshState(v);

            if (ModelState.IsValid)
            {
                BO.j11Team c = new BO.j11Team();
                if (v.rec_pid > 0) c = Factory.j11TeamBL.Load(v.rec_pid);
                c.j11Name = v.Rec.j11Name;
                c.j11Email = v.Rec.j11Email;
               
                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);
                List<int> j02ids = BO.Code.Bas.ConvertString2ListInt(v.j02IDs);
                c.pid = Factory.j11TeamBL.Save(c,j02ids);
                if (c.pid > 0)
                {
                   
                    v.SetJavascript_CallOnLoad(c.pid);
                    return View(v);
                }
            }
            this.Notify_RecNotSaved();
            return View(v);

        }

        private void RefreshState(Models.Record.j11Record v)
        {
            string strMyQuery = "pids|list_int|-1";
            if (!string.IsNullOrEmpty(v.j02IDs))           
            {
                strMyQuery = "pids|list_int|" + v.j02IDs;                
            }
            
            v.gridinput = new Views.Shared.Components.myGrid.myGridInput() { entity = "j02User", master_entity="inform",myqueryinline= strMyQuery,oncmclick="",ondblclick="" };
            v.gridinput.query = new BO.InitMyQuery(Factory.CurrentUser).Load("j02", null, 0, strMyQuery);
            

        }

        public List<int> GetJ02IDsByPosition(int j07id)
        {
            if (j07id == 0) { return null; };

            //var mq = new BO.myQueryJ02() { a05id = a05id,IsRecordValid=true };           
            //return Factory.j07.GetList(mq).Select(p => p.pid).ToList();

            return null;
        }
    }
}