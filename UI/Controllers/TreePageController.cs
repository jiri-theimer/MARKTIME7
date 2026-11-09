using Microsoft.AspNetCore.Mvc;
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
            mq.IsRecordValid = true;
            if (v.TheGridQueryButton != null && v.TheGridQueryButton.j72id > 0)
            {
                mq.lisJ73 = Factory.j72TheGridTemplateBL.GetList_j73(v.TheGridQueryButton.j72id, "p41", 0);
            }


            if (v.periodinput.PeriodValue > 0)
            {
                mq.period_field = "p31Date";   //strom: projekty s úkony v období (v liště to říká tooltip)
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

            var lisP41 = Factory.p41ProjectBL.GetList(mq).ToList();


            var lisflat = new List<UI.Models.Asi.TreeNode>();
            v.TabName = Factory.tra("Strom");

            switch (v.groupby)
            {
                case "p28":
                    v.TabName = $"{v.TabName}: {Factory.tra("Klient->Projekt")}";
                    AddGrouped(lisflat, lisP41, "p28", p => p.p28ID_Client, p => p.Client, Factory.tra("Bez klienta"), p => p.PrefferedName, null);
                    break;
                case "j18":
                    //i projekty archivovaných (uzavřených) středisek – nesmí ze stromu zmizet
                    v.TabName = $"{v.TabName}: {Factory.tra("Středisko->Projekt")}";
                    AddGrouped(lisflat, lisP41, "j18", p => p.j18ID, p => p.j18Name, Factory.tra("Bez střediska"), p => GetBrachName(p, v), p => p.j18Ordinary);
                    break;
                case "p42":
                    v.TabName = $"{v.TabName}: {Factory.tra("Typ projektu->Projekt")}";
                    AddGrouped(lisflat, lisP41, "p42", p => p.p42ID, p => p.p42Name, Factory.tra("Bez typu"), p => GetBrachName(p, v), null);
                    break;
                case "p51":
                    v.TabName = $"{v.TabName}: {Factory.tra("Ceník sazeb->Projekt")}";
                    AddGrouped(lisflat, lisP41, "p51", p => p.p51ID_Billing, p => p.p51Name_Billing, Factory.tra("Bez ceníku"), p => GetBrachName(p, v), null);
                    break;
                default:    //p41 – strom projektů
                    v.groupby = "p41";
                    v.TabName = $"{v.TabName}: {Factory.tra("Projekty")}";
                    //projekty bez viditelných podřízených (a podřízené, jejichž nadřízený je odfiltrovaný) jsou pohromadě ve skupině na začátku
                    lisflat.Add(new UI.Models.Asi.TreeNode() { Id = NoneGroupId, IdParent = 0, Name = Factory.tra("Samostatné projekty"), Prefix = "none" });
                    var ids = lisP41.Select(p => p.pid).ToHashSet();
                    var parentsWithVisibleChildren = lisP41.Where(p => p.p41ParentID > 0 && ids.Contains(p.p41ParentID)).Select(p => p.p41ParentID).ToHashSet();
                    foreach (var c in lisP41)
                    {
                        int idParent;
                        if (c.p41ParentID > 0 && ids.Contains(c.p41ParentID)) idParent = c.p41ParentID;
                        else if (parentsWithVisibleChildren.Contains(c.pid)) idParent = 0;
                        else idParent = NoneGroupId;
                        lisflat.Add(new UI.Models.Asi.TreeNode() { Id = c.pid, IdParent = idParent, Name = GetBrachName(c, v), Prefix = "p41" });
                    }
                    break;
            }

            if (!lisflat.Any(p => p.IdParent == NoneGroupId))
            {
                lisflat.RemoveAll(p => p.Id == NoneGroupId);
            }

            v.lisTreeNodes = UI.Code.basTree.BuildTree(lisflat);
        }

        /// <summary>Id uzlu skupiny „Bez klienta / Bez střediska / Samostatné projekty“ (nekoliduje s pid projektu ani se zápornými id skupin).</summary>
        private const int NoneGroupId = int.MinValue;

        /// <summary>
        /// Strom Skupina → Projekt (klient, středisko, typ, ceník). Podřízený projekt visí pod nadřízeným, pokud je nadřízený ve stejné skupině
        /// a prošel filtrem; jinak přímo pod skupinou. Projekty bez skupiny (id 0) jdou do skupiny „Bez …“ na konci.
        /// </summary>
        private void AddGrouped(List<UI.Models.Asi.TreeNode> flat, List<BO.p41Project> lisP41, string prefix,
            Func<BO.p41Project, int> groupId, Func<BO.p41Project, string> groupName, string noneName,
            Func<BO.p41Project, string> projectName, Func<BO.p41Project, int> groupOrder)
        {
            var groups = lisP41.GroupBy(groupId).Select(g => new { Id = g.Key, Name = groupName(g.First()), Order = groupOrder == null ? 0 : groupOrder(g.First()), Items = g.ToList() });
            groups = groups.OrderBy(g => g.Id == 0 ? 1 : 0).ThenBy(g => g.Order).ThenBy(g => g.Name, StringComparer.CurrentCultureIgnoreCase).ToList();

            foreach (var g in groups)
            {
                int groupNodeId = g.Id > 0 ? -1 * g.Id : NoneGroupId;
                flat.Add(new UI.Models.Asi.TreeNode() { Id = groupNodeId, IdParent = 0, Name = g.Id > 0 ? g.Name : noneName, Prefix = g.Id > 0 ? prefix : "none" });

                var ids = g.Items.Select(p => p.pid).ToHashSet();
                foreach (var c in g.Items)
                {
                    int idParent = (c.p41ParentID > 0 && ids.Contains(c.p41ParentID)) ? c.p41ParentID : groupNodeId;
                    flat.Add(new UI.Models.Asi.TreeNode() { Id = c.pid, IdParent = idParent, Name = projectName(c), Prefix = "p41" });
                }
            }
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
