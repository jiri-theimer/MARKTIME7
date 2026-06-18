using Microsoft.AspNetCore.Mvc;
using UI.Models;
using UI.Models.Kanban;

namespace UI.Controllers
{
    public class KanbanController : BaseController
    {
        public IActionResult p28(int go2pid)
        {

            var v = new p28KanbanViewModel() { go2pid = go2pid };

            v.TheGridQueryButton = new TheGridQueryViewModel() { j72id = Factory.CBL.LoadUserParamInt("kanban-p28-j72id"), paramkey = "kanban-p28-j72id", prefix = "p28" };

            var mq = new BO.myQueryP28();

            v.p29IDs = Factory.CBL.LoadUserParam("kanban-p28-p29ids");
            if (v.p29IDs != null)
            {
                var p29ids = BO.Code.Bas.ConvertString2ListInt(v.p29IDs);
                var lis = Factory.p29ContactTypeBL.GetList(new BO.myQuery("p29") { pids = p29ids });
                v.SelectedP29Names = string.Join(",", lis.Select(p => p.p29Name));
                mq.p29ids = BO.Code.Bas.ConvertString2ListInt(v.p29IDs);
            }

            var lisP28 = Factory.p28ContactBL.GetList(mq);
            v.polozky = new List<KanbanPolozka>();

            foreach(var c in lisP28)
            {
                var polozka = new KanbanPolozka() {prefix="p28",pid=c.pid, nazev = c.p28Name, kod = c.p28Code, b02Name = c.b02Name, b02Color = c.b02Color,sloupec_nazev=c.p29Name };
                
                v.polozky.Add(polozka);
            }




            v.p31statequery = new p31StateQueryViewModel() { UserParamKey = "kanban-p28-p31statequery" };
            v.p31statequery.Value = Factory.CBL.LoadUserParamInt(v.p31statequery.UserParamKey);


            return View(v);
        }
    }
}
