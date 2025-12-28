using Markdig.Extensions.Alerts;
using Microsoft.AspNetCore.Mvc;
using SQLitePCL;
using UI.Models;

namespace UI.Controllers
{
    public class TreePageController : BaseController
    {
        private readonly BL.Singleton.ThePeriodProvider _pp;
        public TreePageController(BL.Singleton.ThePeriodProvider pp)
        {
            _pp = pp;
        }
        public IActionResult Index(string groupby, int pid, string tab, string rez, string myqueryinline)
        {

            var v = new TreePageViewModel() { pid = pid, groupby = groupby, DefTab = tab, rez = rez };

            if (string.IsNullOrEmpty(v.groupby))
            {
                v.groupby = Factory.CBL.LoadUserParam("treepage-groupby", "p28");
            }
            if (v.pid == 0)
            {
                v.pid = Factory.CBL.LoadUserParamInt("recpage-p41--pid");   //shodné s gridem
            }

            v.ProjectMask = Factory.CBL.LoadUserParam("treepage-projectmask", "NameWithClient");

            v.periodinput = new Views.Shared.Components.myPeriod.myPeriodViewModel() { prefix = "p41", UserParamKey = "treepage-period" };
            v.periodinput.LoadUserSetting(_pp, Factory);

            v.p31statequery = new p31StateQueryViewModel() { UserParamKey = "treepage-p31statequery" };
            v.p31statequery.Value = Factory.CBL.LoadUserParamInt(v.p31statequery.UserParamKey);


            v.TheGridQueryButton = new UI.Models.TheGridQueryViewModel() { prefix = "p41", paramkey = $"treepage-query-j72id-{v.groupby}-{rez}" };
            v.TheGridQueryButton.j72id = Factory.CBL.LoadUserParamInt(v.TheGridQueryButton.paramkey);
            if (v.TheGridQueryButton.j72id > 0)
            {
                v.TheGridQueryButton.j72name = Factory.j72TheGridTemplateBL.LoadName(v.TheGridQueryButton.j72id);
            }
            v.recordbinquery = new RecordBinQueryViewModel() { Prefix = "p41" };
            v.recordbinquery.UserParamKey = $"treepage-p41-recordbinquery";
            v.recordbinquery.Value = Factory.CBL.LoadUserParamInt(v.recordbinquery.UserParamKey, 1);


            RefreshNavTabs(v);

            RefreshTreeNodes(v);





            return View(v);
        }

        private void RefreshNavTabs(TreePageViewModel v)
        {
            var cTabs = new UI.Code.NavTabsSupport(Factory);
            v.NavTabs = cTabs.getMasterTabs("p41", v.pid, true);

            //v.TabName = Factory.EProvider.ByPrefix(v.prefix).AliasPlural.ToUpper();

            if (string.IsNullOrEmpty(v.DefTab))
            {
                v.DefTab = Factory.CBL.LoadUserParam($"treepage-tab-p41"); //uživatelem naposledy vybraná záložka    
            }

            if (v.NavTabs.Count() > 0)
            {
                var deftab = v.NavTabs[0];


                foreach (var tab in v.NavTabs)
                {
                    tab.Url = tab.Url.Replace("@pid", v.pid.ToString());
                    tab.Url = $"{tab.Url}&master_entity=p41Project&master_pid={v.pid}";
                    if (v.DefTab != null && tab.Entity == v.DefTab)
                    {
                        deftab = tab;
                    }
                }
                deftab.CssClass += " active";
                if (!deftab.Url.Contains("@pid"))
                {
                    v.DefaultNavTabUrl = deftab.Url;
                    v.DefaultNavName = deftab.Name;
                }
            }
        }




        private void RefreshTreeNodes(TreePageViewModel v)
        {
            var mq = new BO.myQueryP41("p41");
            if (v.TheGridQueryButton != null && v.TheGridQueryButton.j72id > 0)
            {
                mq.lisJ73 = Factory.j72TheGridTemplateBL.GetList_j73(v.TheGridQueryButton.j72id, "p41", 0);
            }


            if (v.periodinput.PeriodValue > 0)
            {
                mq.period_field = v.periodinput.PeriodField;
                mq.period_field = "p31Date";
                mq.global_d1 = v.periodinput.d1;
                mq.global_d2 = v.periodinput.d2;

            }

            switch (v.recordbinquery.Value)
            {
                case 1: mq.IsRecordValid = true; break;   //pouze otevřené záznamy
                case 2: mq.IsRecordValid = false; break;  //pouze záznamy v archivu
                default: mq.IsRecordValid = null; break;
            }
            if (v.p31statequery != null)
            {
                mq.p31statequery = v.p31statequery.Value;

            }


            if (v.ProjectMask == "NameWithClient")
            {
                mq.explicit_orderby = "p28client.p28Name,a.p41Name";
            }
            if (v.ProjectMask == "p41Name")
            {
                mq.explicit_orderby = "a.p41NameShort,a.p41Name";
            }

            var lisP41 = Factory.p41ProjectBL.GetList(mq);   //.OrderBy(p => p.p41TreeIndex);
           
           
            var lisflat = new List<UI.Models.Asi.TreeNode>();
            lisflat.Add(new UI.Models.Asi.TreeNode() { Id = 999999, IdParent = 0, Name = "-----------------" });

            v.TabName = Factory.tra("Strom");


            switch (v.groupby)
            {
                case "p41":
                    v.TabName = $"{v.TabName}:{Factory.tra("Projekty")}";
                    foreach (var c in lisP41)
                    {
                        if (c.p41ParentID == 0 && c.p41TreeNext == c.p41TreePrev)
                        {
                            lisflat.Add(new UI.Models.Asi.TreeNode() { Id = c.pid, IdParent = 999999, Name = GetBrachName(c, v), Prefix = "p41" });
                        }
                        else
                        {
                            lisflat.Add(new UI.Models.Asi.TreeNode() { Id = c.pid, IdParent = c.p41ParentID, Name = GetBrachName(c, v), Prefix = "p41" });
                        }

                    }
                    break;
                case "j18":
                    v.TabName = $"{v.TabName}:{Factory.tra("Středisko->Projekt")}";
                    var lisJ18 = lisP41.Select(p => new { p.j18ID, p.j18Name }).Distinct();
                    foreach (var rec in lisJ18)
                    {
                        if (rec.j18ID > 0)
                        {
                            lisflat.Add(new UI.Models.Asi.TreeNode() { Id = -1 * rec.j18ID, IdParent = 0, Name = rec.j18Name, Prefix = "j18" });
                        }

                        var qryP41 = lisP41.Where(p => p.j18ID == rec.j18ID);
                        foreach (var c in qryP41)
                        {

                            if (c.j18ID == 0)
                            {
                                lisflat.Add(new UI.Models.Asi.TreeNode() { Id = c.pid, IdParent = 999999, Name = GetBrachName(c, v) });
                            }
                            else
                            {
                                if (c.p41ParentID == 0 && c.p41TreeNext == c.p41TreePrev)
                                {
                                    lisflat.Add(new UI.Models.Asi.TreeNode() { Id = c.pid, IdParent = -1 * c.j18ID, Name = GetBrachName(c, v) });
                                }
                                else
                                {
                                    var recParent = lisP41.FirstOrDefault(p => p.pid == c.p41ParentID && p.j18ID == rec.j18ID);
                                    if (recParent != null)
                                    {
                                        lisflat.Add(new UI.Models.Asi.TreeNode() { Id = c.pid, IdParent = c.p41ParentID, Name = GetBrachName(c, v) });
                                    }
                                    else
                                    {
                                        lisflat.Add(new UI.Models.Asi.TreeNode() { Id = c.pid, IdParent = -1 * c.j18ID, Name = GetBrachName(c, v) });
                                    }

                                }
                            }



                        }
                    }

                    break;
                case "p42":
                    v.TabName = $"{v.TabName}:{Factory.tra("Typ projektu->Projekt")}";
                    var lisP42 = lisP41.Select(p => new { p.p42ID, p.p42Name }).Distinct();
                    foreach (var rec in lisP42)
                    {
                        if (rec.p42ID > 0)
                        {
                            lisflat.Add(new UI.Models.Asi.TreeNode() { Id = -1 * rec.p42ID, IdParent = 0, Name = rec.p42Name, Prefix = "p42" });
                        }

                        var qryP41 = lisP41.Where(p => p.p42ID == rec.p42ID);
                        foreach (var c in qryP41)
                        {

                            if (c.p42ID == 0)
                            {
                                lisflat.Add(new UI.Models.Asi.TreeNode() { Id = c.pid, IdParent = 999999, Name = GetBrachName(c, v) });
                            }
                            else
                            {
                                if (c.p41ParentID == 0 && c.p41TreeNext == c.p41TreePrev)
                                {
                                    lisflat.Add(new UI.Models.Asi.TreeNode() { Id = c.pid, IdParent = -1 * c.p42ID, Name = GetBrachName(c, v) });
                                }
                                else
                                {
                                    var recParent = lisP41.FirstOrDefault(p => p.pid == c.p41ParentID && p.p42ID == rec.p42ID);
                                    if (recParent != null)
                                    {
                                        lisflat.Add(new UI.Models.Asi.TreeNode() { Id = c.pid, IdParent = c.p41ParentID, Name = GetBrachName(c, v) });
                                    }
                                    else
                                    {
                                        lisflat.Add(new UI.Models.Asi.TreeNode() { Id = c.pid, IdParent = -1 * c.p42ID, Name = GetBrachName(c, v) });
                                    }

                                }
                            }



                        }
                    }

                    break;
                case "p51":
                    v.TabName = $"{v.TabName}:{Factory.tra("Ceník sazeb->Projekt")}";
                    var lisP51 = lisP41.Select(p => new { p.p51ID_Billing, p.p51Name_Billing }).Distinct();
                    foreach (var rec in lisP51)
                    {
                        if (rec.p51ID_Billing > 0)
                        {
                            lisflat.Add(new UI.Models.Asi.TreeNode() { Id = -1 * rec.p51ID_Billing, IdParent = 0, Name = rec.p51Name_Billing, Prefix = "p51" });
                        }

                        var qryP41 = lisP41.Where(p => p.p51ID_Billing == rec.p51ID_Billing);
                        foreach (var c in qryP41)
                        {

                            if (c.p51ID_Billing == 0)
                            {
                                lisflat.Add(new UI.Models.Asi.TreeNode() { Id = c.pid, IdParent = 999999, Name = GetBrachName(c, v) });
                            }
                            else
                            {
                                if (c.p41ParentID == 0 && c.p41TreeNext == c.p41TreePrev)
                                {
                                    lisflat.Add(new UI.Models.Asi.TreeNode() { Id = c.pid, IdParent = -1 * c.p51ID_Billing, Name = GetBrachName(c, v) });
                                }
                                else
                                {
                                    var recParent = lisP41.FirstOrDefault(p => p.pid == c.p41ParentID && p.p51ID_Billing == rec.p51ID_Billing);
                                    if (recParent != null)
                                    {
                                        lisflat.Add(new UI.Models.Asi.TreeNode() { Id = c.pid, IdParent = c.p41ParentID, Name = GetBrachName(c, v) });
                                    }
                                    else
                                    {
                                        lisflat.Add(new UI.Models.Asi.TreeNode() { Id = c.pid, IdParent = -1 * c.p51ID_Billing, Name = GetBrachName(c, v) });
                                    }

                                }
                            }



                        }
                    }

                    break;



                case "p28":
                    v.TabName = $"{v.TabName}:{Factory.tra("Klient->Projekt")}";
                    var lisP28 = lisP41.Select(p => new { p.p28ID_Client, p.Client }).Distinct();


                    foreach (var recP28 in lisP28)
                    {
                        if (recP28.p28ID_Client > 0)
                        {
                            var n = new UI.Models.Asi.TreeNode() { Id = -1 * recP28.p28ID_Client, IdParent = 0, Name = recP28.Client, Prefix = "p28" };
                            n.Url = $"/Record/RecPage?prefix=p28&pid={recP28.p28ID_Client}";
                            lisflat.Add(n);
                        }

                        var qryP41 = lisP41.Where(p => p.p28ID_Client == recP28.p28ID_Client);
                        foreach (var c in qryP41)
                        {

                            if (c.p28ID_Client == 0)
                            {
                                lisflat.Add(new UI.Models.Asi.TreeNode() { Id = c.pid, IdParent = 999999, Name = c.PrefferedName });
                            }
                            else
                            {
                                if (c.p41ParentID == 0 && c.p41TreeNext == c.p41TreePrev)
                                {
                                    lisflat.Add(new UI.Models.Asi.TreeNode() { Id = c.pid, IdParent = -1 * c.p28ID_Client, Name = c.PrefferedName });
                                }
                                else
                                {
                                    var recParent = lisP41.FirstOrDefault(p => p.pid == c.p41ParentID && p.p28ID_Client == recP28.p28ID_Client);
                                    if (recParent != null)
                                    {
                                        lisflat.Add(new UI.Models.Asi.TreeNode() { Id = c.pid, IdParent = c.p41ParentID, Name = c.PrefferedName });
                                    }
                                    else
                                    {
                                        lisflat.Add(new UI.Models.Asi.TreeNode() { Id = c.pid, IdParent = -1 * c.p28ID_Client, Name = c.PrefferedName });
                                    }

                                }
                            }



                        }
                    }

                    break;
            }
            if (!lisflat.Any(p => p.IdParent == 999999))
            {
                lisflat.RemoveAll(p => p.Id == 999999);
            }




            v.lisTreeNodes = UI.Code.basTree.BuildTree(lisflat);

        }


        private string GetBrachName(BO.p41Project rec, TreePageViewModel v)
        {
            if (v.ProjectMask == "p41Name")
            {
                return rec.p41Name;
            }

            return rec.NameWithClient;
        }

    }
}
