using DocumentFormat.OpenXml.Presentation;
using Microsoft.AspNetCore.Mvc;
using UI.Models;
using UI.Models.Kanban;

namespace UI.Controllers
{
    public class KanbanController : BaseController
    {
        [HttpPost]
        public IActionResult Move(int pid, string prefix, int sloupec_pid)
        {
            // prefix říká, o jakou entitu jde; sloupec_pid je cílový sloupec
            var viewtype = Factory.CBL.LoadUserParam("kanban-p28-viewtype", "p29");
            switch (prefix)
            {
                case "p28":
                    var rec = Factory.p28ContactBL.Load(pid);
                    if (viewtype == "p29")
                    {
                        rec.p29ID = sloupec_pid;
                        Factory.p28ContactBL.Save(rec, null, null, null, null, null, null);
                    }
                    else
                    {
                        rec.b02ID = sloupec_pid;
                        Factory.WorkflowBL.RunWorkflowStatus("p28", pid, sloupec_pid,null,0);
                    }
                    
                    
                    break;
                // další agendy (úlohy, projekty…) dle jejich BL
                default:
                    return BadRequest();
            }
            return Json(new { ok = true });
        }
        public IActionResult p28(string viewtype,int go2pid)
        {

            var v = new p28KanbanViewModel() { go2pid = go2pid,viewtype=viewtype };

            var lisB01 = Factory.b01WorkflowTemplateBL.GetList(new BO.myQuery("b01")).Where(p => p.b01Entity == "p28");
            v.viewtypes = new List<BO.StringPair>();
            v.viewtypes.Add(new BO.StringPair() { Key = "p29", Value = "Typ kontaktu" });
            foreach (var c in lisB01)
            {
                v.viewtypes.Add(new BO.StringPair() { Key = $"b01-{c.pid}", Value = c.b01Name });
            }
            if (string.IsNullOrEmpty(v.viewtype))
            {
                v.viewtype = Factory.CBL.LoadUserParam("kanban-p28-viewtype", "p29");
            }
            v.TheGridQueryButton = new TheGridQueryViewModel() { j72id = Factory.CBL.LoadUserParamInt("kanban-p28-j72id"), paramkey = "kanban-p28-j72id", prefix = "p28" };            
            if (v.TheGridQueryButton.j72id > 0)
            {
                v.TheGridQueryButton.j72name = Factory.j72TheGridTemplateBL.LoadName(v.TheGridQueryButton.j72id);
            }

            var lisX69 = Factory.x67EntityRoleBL.GetList_X69("p28", 0);

            var mq = new BO.myQueryP28();
            
            if (v.TheGridQueryButton.j72id > 0)
            {
                mq.lisJ73 = Factory.j72TheGridTemplateBL.GetList_j73(v.TheGridQueryButton.j72id, "p28", 0);
            }


            v.sloupce = new List<KanbanSloupec>();
            if (v.viewtype == "p29")
            {
                var lisP29 = Factory.p29ContactTypeBL.GetList(new BO.myQuery("p29")).OrderBy(p => p.p29Ordinary);
                foreach (var c in lisP29)
                {
                    v.sloupce.Add(new KanbanSloupec() { pid = c.pid, nazev = c.p29Name });
                }
            }
            else
            {
                mq.b01id= int.Parse(v.viewtype.Replace("b01-", ""));                
                var lisB02 = Factory.b02WorkflowStatusBL.GetList(new BO.myQuery("b02")).Where(p => p.b01ID == mq.b01id).OrderBy(p => p.b02Ordinary);
                foreach(var c in lisB02)
                {
                    v.sloupce.Add(new KanbanSloupec() { pid = c.pid, nazev = c.b02Name,barva=c.b02Color});
                }
            }
            
            v.p31statequery = new p31StateQueryViewModel() { UserParamKey = "kanban-p28-p31statequery" };
            v.p31statequery.Value = Factory.CBL.LoadUserParamInt(v.p31statequery.UserParamKey);
            mq.p31statequery = v.p31statequery.Value;



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
                var polozka = new KanbanPolozka() {prefix="p28",pid=c.pid, nazev = c.p28Name, kod = c.p28Code };
                if (c.p28Street1 !=null || c.p28City1 != null)
                {
                    polozka.nazev_after = $"{c.p28Street1}, {c.p28City1}";
                    if (c.p28CountryCode != Factory.Lic.x01CountryCode)
                    {
                        polozka.nazev_after += ", " + c.p28Country1;
                    }
                }
                if (v.viewtype == "p29")
                {
                    polozka.sloupec_pid = c.p29ID;
                    polozka.b02Name = c.b02Name;
                    polozka.b02Color = c.b02Color;
                }
                else
                {
                    polozka.sloupec_pid = c.b02ID;
                }
                var qryRole = lisX69.Where(p => p.x69RecordPid == c.pid);
                if (qryRole.Count()>0)
                {
                    polozka.role = new List<string>();
                    foreach (var role in qryRole)
                    {
                        polozka.role.Add($"{role.x67Name}: {role.Person} {role.j11Name}");
                    }
                }
                
                v.polozky.Add(polozka);
            }

            var lisHodiny = Factory.p31WorksheetBL.GetList_HodinyKanban("p28");
            foreach(var c in v.polozky)
            {
                var qry = lisHodiny.FirstOrDefault(p => p.pid == c.pid);
                if (qry !=null)
                {
                    c.hodiny_vykazane = qry.hodiny_vykazane;
                    c.hodiny_nevyuctovane = qry.hodiny_nevyuctovane;
                }
            }

           

            

            return View(v);
        }
    }
}
