using BL;
using Microsoft.AspNetCore.Mvc;
using UI.Models.batch;
using UI.Models.p31oper;

namespace UI.Controllers.p31oper
{
    public class p31delbatchController : BaseController
    {
        public IActionResult Index(string pids,int j72id,string oper, string guid_pids)
        {
            if (!string.IsNullOrEmpty(guid_pids))
            {
                pids = Factory.p85TempboxBL.LoadByGuid(guid_pids).p85Message; //vstupní pids předány přes p85Tempbox
            }
            if (!Factory.CurrentUser.TestPermission(BO.PermValEnum.GR_P31_Owner))
            {
                return this.StopPage(true, "Pro tuto operaci nemáte dostatečné oprávnění.");
            }
            var v = new p31delbatchViewModel() { pids = pids, j72id = j72id, oper = oper };
            var lispids_all = BO.Code.Bas.ConvertString2ListInt(pids);
            if (lispids_all.Count() == 0)
            {
                return this.StopPage(true, "Na vstupu chybí úkony.");
            }            
            var lispids_valid = get_valid_pids(v);
            v.pids_valid = string.Join(",", lispids_valid);
            if (lispids_valid.Count() == 0)
            {
                return this.StopPage(true, "Na vstupu chybí úkony.");
            }

          
            
            RefreshState(v);

            return View(v);
        }

        private void RefreshState(p31delbatchViewModel v)
        {
            var gridState = Factory.j72TheGridTemplateBL.LoadState(v.j72id, Factory.CurrentUser.pid);
            
            
            v.gridinput = new Views.Shared.Components.myGrid.myGridInput() { j72id = v.j72id, is_enable_selecting = false, entity = gridState.j72Entity , oncmclick = "", ondblclick = "" };
            v.gridinput.myqueryinline = $"pids|list_int|{v.pids_valid}";
            if (v.oper == "restore")
            {
                v.gridinput.myqueryinline += "|IsRecordValid|bool|false";
            }
            else
            {
                v.gridinput.myqueryinline += "|IsRecordValid|bool|true";
            }
            v.gridinput.query = new BO.InitMyQuery(Factory.CurrentUser).Load("p31", null, 0, v.gridinput.myqueryinline);
            
            
        }


        [HttpPost]
        public IActionResult Index(p31delbatchViewModel v)
        {
            RefreshState(v);

            if (v.IsPostback)
            {

                return View(v);
            }

            if (ModelState.IsValid)
            {

                var arr = get_valid_pids(v);            
                string strGUID = BO.Code.Bas.GetGuid();

                foreach (int pid in arr)
                {
                    Factory.p85TempboxBL.Save(new BO.p85Tempbox() { p85GUID = strGUID, p85DataPID = pid });                    

                }

                bool b = false;
                if (v.oper == "archive")
                {
                    b=Factory.p31WorksheetBL.Move_To_Del(strGUID);
                }
                if (v.oper == "restore")
                {
                    b=Factory.p31WorksheetBL.Move_From_Del(strGUID);
                }
                if (b)
                {
                    v.SetJavascript_CallOnLoad(0);
                    return View(v);
                }
               
            }

            return View(v);
        }

        private List<int> get_valid_pids(p31delbatchViewModel v)
        {
            var arr = BO.Code.Bas.ConvertString2ListInt(v.pids);
            var mq = new BO.myQueryP31() { pids = arr, IsRecordValid = (v.oper == "restore" ? false : true) };
            var valids = Factory.p31WorksheetBL.GetList(mq).Select(p => p.pid).ToList();

            

            return valids;
        }
    }


    
}
