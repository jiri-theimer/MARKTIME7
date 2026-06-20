using BO;
using ceTe.DynamicPDF;
using DocumentFormat.OpenXml.Office2013.PowerPoint.Roaming;
using DocumentFormat.OpenXml.Presentation;
using Microsoft.AspNetCore.Mvc;
using UI.Models;
using UI.Models.Kanban;

namespace UI.Controllers
{

    public class KanbanController : BaseController
    {
        // Per-sloupcové stránkování: pager ve view reloadne board s ?p{sloupec_pid}={stránka}.
        // Tady to přečteme a nastavíme pagenum (0-based) na příslušný sloupec.
        private void ApplyKanbanPageNum(List<KanbanSloupec> sloupce)
        {
            if (sloupce == null) return;
            foreach (var sl in sloupce)
            {
                var raw = Request.Query["p" + sl.pid];
                if (int.TryParse(raw, out var n) && n >= 0)
                    sl.pagenum = n;
            }
        }

        [HttpPost]
        public IActionResult Move(int pid, string prefix, int sloupec_pid)
        {
            var viewtype = Factory.CBL.LoadUserParam("kanban-p28-viewtype");
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
                        Factory.WorkflowBL.RunWorkflowStatus("p28", pid, sloupec_pid, null, 0);
                    }
                    break;
                case "p41":
                    Factory.WorkflowBL.RunWorkflowStatus("p41", pid, sloupec_pid, null, 0);
                    break;
                case "p56":
                    Factory.WorkflowBL.RunWorkflowStatus("p56", pid, sloupec_pid, null, 0);
                    break;
                default:
                    return BadRequest();
            }
            return Json(new { ok = true });
        }


        public IActionResult p28(string viewtype, int go2pid)
        {
            if (!Factory.CurrentUser.j04IsModule_p28)
            {
                return this.StopPage(false, "Nemáte oprávnění pro tento Modul.");
            }
            var v = new p28KanbanViewModel() { go2pid = go2pid, viewtype = viewtype };

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
                mq.b01id = int.Parse(v.viewtype.Replace("b01-", ""));
                var lisB02 = Factory.b02WorkflowStatusBL.GetList(new BO.myQuery("b02")).Where(p => p.b01ID == mq.b01id).OrderBy(p => p.b02Ordinary);
                foreach (var c in lisB02)
                {
                    v.sloupce.Add(new KanbanSloupec() { pid = c.pid, nazev = c.b02Name, barva = c.b02Color });
                }
            }
            ApplyKanbanPageNum(v.sloupce);

            v.p31statequery = new p31StateQueryViewModel() { UserParamKey = "kanban-p28-p31statequery" };
            v.p31statequery.Value = Factory.CBL.LoadUserParamInt(v.p31statequery.UserParamKey);
            mq.p31statequery = v.p31statequery.Value;


            var lisP28 = Factory.p28ContactBL.GetList(mq);
            v.polozky = new List<KanbanPolozka>();

            foreach (var c in lisP28)
            {
                var polozka = new KanbanPolozka() { prefix = "p28", pid = c.pid, nazev = c.p28Name, kod = c.p28Code };
                if (c.p28Street1 != null || c.p28City1 != null)
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
                if (qryRole.Count() > 0)
                {
                    polozka.role = new List<BO.StringPair>();
                    foreach (var role in qryRole)
                    {
                        polozka.role.Add(new BO.StringPair() { Key = role.x67Name, Value = $"{role.Person} {role.j11Name}" });
                    }
                }

                v.polozky.Add(polozka);
            }

            var lisHodiny = Factory.p31WorksheetBL.GetList_HodinyKanban("p28");
            foreach (var c in v.polozky)
            {
                var qry = lisHodiny.FirstOrDefault(p => p.pid == c.pid);
                if (qry != null)
                {
                    c.hodiny_vykazane = qry.hodiny_vykazane;
                    c.hodiny_nevyuctovane = qry.hodiny_nevyuctovane;
                }
            }

            return View(v);
        }

        public IActionResult p41(string viewtype, int go2pid)
        {
            if (!Factory.CurrentUser.j04IsModule_p41)
            {
                return this.StopPage(false, "Nemáte oprávnění pro tento Modul.");
            }
            var v = new p41KanbanViewModel() { go2pid = go2pid, viewtype = viewtype };

            var lisB01 = Factory.b01WorkflowTemplateBL.GetList(new BO.myQuery("b01")).Where(p => p.b01Entity == "p41");
            if (lisB01.Count() == 0)
            {
                return this.StopPage(false, "Neexistuje ani jedna workflow šablona.");
            }
            v.viewtypes = new List<BO.StringPair>();
            foreach (var c in lisB01)
            {
                v.viewtypes.Add(new BO.StringPair() { Key = $"b01-{c.pid}", Value = c.b01Name });
            }
            if (string.IsNullOrEmpty(v.viewtype))
            {
                v.viewtype = Factory.CBL.LoadUserParam("kanban-p41-viewtype", $"b01-{lisB01.First().pid}");
            }
            v.TheGridQueryButton = new TheGridQueryViewModel() { j72id = Factory.CBL.LoadUserParamInt("kanban-p41-j72id"), paramkey = "kanban-p41-j72id", prefix = "p41" };
            if (v.TheGridQueryButton.j72id > 0)
            {
                v.TheGridQueryButton.j72name = Factory.j72TheGridTemplateBL.LoadName(v.TheGridQueryButton.j72id);
            }

            var lisX69 = Factory.x67EntityRoleBL.GetList_X69("p41", 0);

            var mq = new BO.myQueryP41("p41") { IsRecordValid = null };

            if (v.TheGridQueryButton.j72id > 0)
            {
                mq.lisJ73 = Factory.j72TheGridTemplateBL.GetList_j73(v.TheGridQueryButton.j72id, "p41", 0);
            }

            v.sloupce = new List<KanbanSloupec>();
            mq.b01id = int.Parse(v.viewtype.Replace("b01-", ""));
            var lisB02 = Factory.b02WorkflowStatusBL.GetList(new BO.myQuery("b02")).Where(p => p.b01ID == mq.b01id).OrderBy(p => p.b02Ordinary);
            foreach (var c in lisB02)
            {
                v.sloupce.Add(new KanbanSloupec() { pid = c.pid, nazev = c.b02Name, barva = c.b02Color });
            }
            ApplyKanbanPageNum(v.sloupce);

            v.p31statequery = new p31StateQueryViewModel() { UserParamKey = "kanban-p41-p31statequery" };
            v.p31statequery.Value = Factory.CBL.LoadUserParamInt(v.p31statequery.UserParamKey);
            mq.p31statequery = v.p31statequery.Value;



            var lisP41 = Factory.p41ProjectBL.GetList(mq);
            v.polozky = new List<KanbanPolozka>();

            foreach (var c in lisP41)
            {
                var polozka = new KanbanPolozka() { prefix = "p41", pid = c.pid, nazev = c.p41Name, kod = c.p41Code };
                polozka.nazev_after = c.Client;
                polozka.sloupec_pid = c.b02ID;

                var qryRole = lisX69.Where(p => p.x69RecordPid == c.pid);
                if (qryRole.Count() > 0)
                {
                    polozka.role = new List<BO.StringPair>();
                    foreach (var role in qryRole)
                    {
                        polozka.role.Add(new BO.StringPair() { Key = role.x67Name, Value = $"{role.Person} {role.j11Name}" });
                    }
                }

                v.polozky.Add(polozka);
            }

            var lisHodiny = Factory.p31WorksheetBL.GetList_HodinyKanban("p41");
            foreach (var c in v.polozky)
            {
                var qry = lisHodiny.FirstOrDefault(p => p.pid == c.pid);
                if (qry != null)
                {
                    c.hodiny_vykazane = qry.hodiny_vykazane;
                    c.hodiny_nevyuctovane = qry.hodiny_nevyuctovane;
                }
            }

            return View(v);
        }

        public IActionResult p56(string viewtype, int go2pid)
        {
            if (!Factory.CurrentUser.j04IsModule_p56)
            {
                return this.StopPage(false, "Nemáte oprávnění pro tento Modul.");
            }
            var v = new p56KanbanViewModel() { go2pid = go2pid, viewtype = viewtype };

            var lisB01 = Factory.b01WorkflowTemplateBL.GetList(new BO.myQuery("b01")).Where(p => p.b01Entity == "p56");
            if (lisB01.Count() == 0)
            {
                return this.StopPage(false, "Neexistuje ani jedna workflow šablona.");
            }
            v.viewtypes = new List<BO.StringPair>();
            foreach (var c in lisB01)
            {
                v.viewtypes.Add(new BO.StringPair() { Key = $"b01-{c.pid}", Value = c.b01Name });
            }
            if (string.IsNullOrEmpty(v.viewtype))
            {
                v.viewtype = Factory.CBL.LoadUserParam("kanban-p56-viewtype", $"b01-{lisB01.First().pid}");
            }
            v.TheGridQueryButton = new TheGridQueryViewModel() { j72id = Factory.CBL.LoadUserParamInt("kanban-p56-j72id"), paramkey = "kanban-p56-j72id", prefix = "p56" };
            if (v.TheGridQueryButton.j72id > 0)
            {
                v.TheGridQueryButton.j72name = Factory.j72TheGridTemplateBL.LoadName(v.TheGridQueryButton.j72id);
            }

            var lisX69 = Factory.x67EntityRoleBL.GetList_X69("p56", 0);

            var mq = new BO.myQueryP56() { IsRecordValid = null };

            if (v.TheGridQueryButton.j72id > 0)
            {
                mq.lisJ73 = Factory.j72TheGridTemplateBL.GetList_j73(v.TheGridQueryButton.j72id, "p56", 0);
            }

            v.sloupce = new List<KanbanSloupec>();
            mq.b01id = int.Parse(v.viewtype.Replace("b01-", ""));
            var lisB02 = Factory.b02WorkflowStatusBL.GetList(new BO.myQuery("b02")).Where(p => p.b01ID == mq.b01id).OrderBy(p => p.b02Ordinary);
            foreach (var c in lisB02)
            {
                v.sloupce.Add(new KanbanSloupec() { pid = c.pid, nazev = c.b02Name, barva = c.b02Color });
            }
            ApplyKanbanPageNum(v.sloupce);

            v.p31statequery = new p31StateQueryViewModel() { UserParamKey = "kanban-p56-p31statequery" };
            v.p31statequery.Value = Factory.CBL.LoadUserParamInt(v.p31statequery.UserParamKey);
            mq.p31statequery = v.p31statequery.Value;


            var lisP56 = Factory.p56TaskBL.GetList(mq);
            v.polozky = new List<KanbanPolozka>();

            foreach (var c in lisP56)
            {
                var polozka = new KanbanPolozka() { prefix = "p56", pid = c.pid, nazev = $"{c.p56Name} ({c.p56Code})", kod = c.p56Code };
                polozka.nazev_after = c.ProjectWithClient;
                polozka.sloupec_pid = c.b02ID;

                var qryRole = lisX69.Where(p => p.x69RecordPid == c.pid);
                if (qryRole.Count() > 0)
                {
                    polozka.role = new List<BO.StringPair>();
                    foreach (var role in qryRole)
                    {
                        polozka.role.Add(new BO.StringPair() { Key = role.x67Name, Value = $"{role.Person} {role.j11Name}" });
                    }
                }

                v.polozky.Add(polozka);
            }

            var lisHodiny = Factory.p31WorksheetBL.GetList_HodinyKanban("p56");
            foreach (var c in v.polozky)
            {
                var qry = lisHodiny.FirstOrDefault(p => p.pid == c.pid);
                if (qry != null)
                {
                    c.hodiny_vykazane = qry.hodiny_vykazane;
                    c.hodiny_nevyuctovane = qry.hodiny_nevyuctovane;
                }
            }

            return View(v);
        }
    }
}