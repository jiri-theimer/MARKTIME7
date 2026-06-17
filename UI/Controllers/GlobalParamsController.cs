using Microsoft.AspNetCore.Mvc;
using UI.Models;
using UI.Models.Record;

namespace UI.Controllers
{
    public class GlobalParamsController : BaseController
    {
        public IActionResult Index(string prefix, int pid)
        {
            var v = new GlobalParamsViewModel() { prefix = prefix, pid = pid };
            if (v.pid == 0)
            {
                return this.StopPage(true, "Doplňující parametry je možné vyplnit až po uložení záznamu.");
            }

            var lis = Factory.o58GlobalParamBL.GetList_o59(v.prefix, v.pid).ToList();
            v.lisO59 = new List<o59Repeater>();
            foreach (var c in lis)
            {

                v.lisO59.Add(new o59Repeater()
                {
                    pid=c.pid,
                    TempGuid = BO.Code.Bas.GetGuid(),
                    o58ID=c.o58ID,
                    o58Name = c.o58Name,
                    x24ID = c.x24ID,
                    o58IsPerUser=c.o58IsPerUser,
                    j02ID = c.j02ID,
                    ComboPerson = c.j02Name,
                    o59Memo = c.o59Memo,
                    o59Value = c.o59Value,
                    o59ValueNum = c.o59ValueNum,
                    o59ValueDate = c.o59ValueDate,
                    o59ValueBoolean = c.o59ValueBoolean,
                    o59ValueString = c.o59ValueString

                });
            }

            RefreshState(v);

            foreach (var c in v.lisO58.Where(p => p.o58Entity == v.prefix && !p.o58IsPerUser))
            {
                if (!v.lisO59.Any(p => p.o58ID == c.pid))
                {
                    v.lisO59.Add(new o59Repeater() { o58ID = c.pid, x24ID = c.x24ID, o58Name = c.o58Name, TempGuid = BO.Code.Bas.GetGuid() });
                }
            }

            return View(v);
        }

        private void RefreshState(GlobalParamsViewModel v)
        {
            v.lisO58 = Factory.o58GlobalParamBL.GetList(new BO.myQuery("o58")).Where(p =>p.o58Entity==v.prefix && p.o58IsEditable);
            if (v.lisO59 == null)
            {
                v.lisO59 = new List<o59Repeater>();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(GlobalParamsViewModel v, string guid, int o58id)
        {
            RefreshState(v);

            if (v.IsPostback)
            {
                if (v.PostbackOper == "add_row")
                {
                    var recO58 = Factory.o58GlobalParamBL.Load(o58id);
                    var c = new o59Repeater() { TempGuid = BO.Code.Bas.GetGuid(), o58ID = o58id, o58Name = recO58.o58Name, o58IsPerUser = recO58.o58IsPerUser, x24ID = recO58.x24ID };
                    v.lisO59.Add(c);

                }

                if (v.PostbackOper == "delete_row")
                {
                    v.lisO59.First(p => p.TempGuid == guid).IsTempDeleted = true;

                }

                return View(v);
            }


            if (ModelState.IsValid)
            {
                //for (int i = 0; i < v.lisParams.Count(); i++)
                //{
                //    Factory.CBL.SaveGlobalParam(v.lisParams[i].o58ID, v.pid, v.lisParams[i].o59Value);
                //}

                for (int i = 0; i < v.lisO59.Count(); i++)
                {
                    var c = new BO.o59GlobalParamBinding() { pid = v.lisO59[i].pid, o59RecordPid = v.pid, o58ID = v.lisO59[i].o58ID , o59Memo = v.lisO59[i].o59Memo };
                    var recO58 = Factory.o58GlobalParamBL.Load(v.lisO59[i].o58ID);
                    if (recO58.o58IsPerUser)
                    {
                        c.j02ID = v.lisO59[i].j02ID;
                    }
                    switch (v.lisO59[i].x24ID)
                    {
                        case BO.x24IdENUM.tBoolean:
                            c.o59ValueBoolean = v.lisO59[i].o59ValueBoolean;
                            break;
                        case BO.x24IdENUM.tDate:
                        case BO.x24IdENUM.tDateTime:
                            c.o59ValueDate = v.lisO59[i].o59ValueDate;
                            break;
                        case BO.x24IdENUM.tDecimal:
                        case BO.x24IdENUM.tInteger:
                            c.o59ValueNum = v.lisO59[i].o59ValueNum;
                            break;
                        default:
                            c.o59ValueString = v.lisO59[i].o59ValueString;
                            break;
                    }

                    if (v.lisO59[i].IsTempDeleted)
                    {
                        c.IsForDelete = true;
                    }
                    
                    Factory.o58GlobalParamBL.SaveParamBinding(c);
                    //Factory.CBL.SaveGlobalParam(v.lisParams[i].o58ID, v.pid, v.lisParams[i].o59Value);
                }

                v.SetJavascript_CallOnLoad(0, null, "window.parent._window_close()");
                return View(v);

            }

            this.Notify_RecNotSaved();
            return View(v);
        }
    }
}
