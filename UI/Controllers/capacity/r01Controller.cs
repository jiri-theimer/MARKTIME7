using BO;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Office2013.Excel;
using Microsoft.AspNetCore.Mvc;
using UI.Models;
using UI.Models.capacity;


namespace UI.Controllers.capacity
{

    public class r01Controller : BaseController
    {
        public IActionResult Move(int p41id, int r02id)
        {
            var v = new MoveViewModel() { p41ID = p41id, r02ID = r02id };

            if (v.p41ID == 0)
            {
                return StopPage(true, "Na vstupu chybí projekt.");
            }

            RefreshState_Move(v);
            v.d1 = v.RecP41.p41PlanFrom;
            v.d2 = v.RecP41.p41PlanUntil;
            v.d1_orig = v.RecP41.p41PlanFrom;
            v.d2_orig = v.RecP41.p41PlanUntil;

            return View(v);
        }

        private void RefreshState_Move(MoveViewModel v)
        {
            v.RecP41 = Factory.p41ProjectBL.Load(v.p41ID);


            v.lisR01 = Factory.r01CapacityBL.GetList(new BO.myQueryR01() { p41id = v.p41ID, r02id = v.r02ID });
            if (v.lisItems == null)
            {
                v.lisItems = new List<PlanItem>();
                foreach (var c in v.lisR01)
                {
                    v.lisItems.Add(new PlanItem() { r01id = c.pid, j02id = c.j02ID, p41id = c.p41ID, Person = c.Person, d1 = c.r01Start, d2 = c.r01End, Color = c.r01Color, HoursFa = c.r01HoursFa, HoursNeFa = c.r01HoursNeFa, HoursTotal = c.r01HoursTotal, d1_orig = c.r01Start, d2_orig = c.r01End }); ;
                }

            }
            v.HasOwnerPermissions = Factory.p41ProjectBL.InhaleRecDisposition(v.p41ID).OwnerAccess;




        }
        [HttpPost]
        public IActionResult Move(MoveViewModel v)
        {
            RefreshState_Move(v);
            if (v.IsPostback)
            {

                if (v.PostbackOper == "posunout")
                {
                    var days = (Convert.ToDateTime(v.d1) - Convert.ToDateTime(v.d1_orig)).TotalDays;
                    for (int i = 0; i < v.lisItems.Count(); i++)
                    {
                        v.lisItems[i].d1 = v.lisItems[i].d1_orig.AddDays(days);
                        v.lisItems[i].d2 = v.lisItems[i].d2_orig.AddDays(days);
                    }
                }
                if (v.PostbackOper == "narovnat")
                {                    
                    for (int i = 0; i < v.lisItems.Count(); i++)
                    {
                        v.lisItems[i].d1 = Convert.ToDateTime(v.d1);
                        v.lisItems[i].d2 = Convert.ToDateTime(v.d2);
                    }
                }
                if (v.PostbackOper == "zleva")
                {
                    for (int i = 0; i < v.lisItems.Count(); i++)
                    {
                        v.lisItems[i].d1 = Convert.ToDateTime(v.d1);
                        var days = (Convert.ToDateTime(v.lisItems[i].d2_orig) - Convert.ToDateTime(v.lisItems[i].d1_orig)).TotalDays;
                        v.lisItems[i].d2 = Convert.ToDateTime(v.d1).AddDays(days);
                    }
                }

                return View(v);
            }

            if (ModelState.IsValid)
            {
                var recP41 = Factory.p41ProjectBL.Load(v.p41ID);
                recP41.p41PlanFrom = v.d1;
                recP41.p41PlanUntil = v.d2;
                if (Factory.p41ProjectBL.Save(recP41, null, null, null) == 0)
                {
                    return View(v);
                }

                for (int i = 0; i < v.lisItems.Count(); i++)
                {
                    var rec = Factory.r01CapacityBL.Load(v.lisItems[i].r01id);
                    rec.r01Start = v.lisItems[i].d1;
                    rec.r01End = v.lisItems[i].d2;
                    if (!Factory.r01CapacityBL.ValidateBeforeSave(rec))
                    {
                        recP41.p41PlanFrom = v.d1_orig;
                        recP41.p41PlanUntil = v.d2_orig;
                        Factory.p41ProjectBL.Save(recP41, null, null, null);
                        return View(v);
                    }
                }

                

                for (int i = 0; i < v.lisItems.Count(); i++)
                {
                    var rec = Factory.r01CapacityBL.Load(v.lisItems[i].r01id);
                    rec.r01Start = v.lisItems[i].d1;
                    rec.r01End = v.lisItems[i].d2;
                    rec.r01Color = v.lisItems[i].Color;
                    Factory.r01CapacityBL.Save(rec);                    
                }
                v.SetJavascript_CallOnLoad(0);
                return View(v);

                
            }
            else
            {
                Notify_RecNotSaved();
            }

            return View(v);
        }

        public string PosunoutDatum(string d,string s)
        {
            DateTime d0 = BO.Code.Bas.String2Date(d);
            switch (s)
            {
                case "month_plus":
                    return d0.AddMonths(1).ToString("dd.MM.yyyy");
                case "month_minus":
                    return d0.AddMonths(-1).ToString("dd.MM.yyyy");
            }

            return d;
        }
        public IActionResult Edit(string itemsprefix,string oper, string items, string groupby, int r02id, int r01id, int p41id,int j02id)
        {

            var mq = new myQueryR01();

            if (!string.IsNullOrEmpty(items))
            {

                var prs = BO.Code.Bas.ConvertString2List(items, "|");
                var inputitems = new List<InputPlanItem>();

                foreach (var s in prs)
                {
                    var ss = s.Split(";");
                    var c = new InputPlanItem() { d = ss[1] };
                    c.d1 = BO.Code.Bas.String2Date(c.d);
                    if (itemsprefix == "j02")
                    {
                        c.j02id = Convert.ToInt32(ss[0]);
                    }
                    if (itemsprefix == "p41")
                    {
                        c.p41id = Convert.ToInt32(ss[0]);
                    }
                    inputitems.Add(c);
                }

                
                mq = new myQueryR01() { p41id = p41id,j02id=j02id, r02id = r02id, global_d1 = inputitems.Min(p => p.d1) };
                if (itemsprefix == "j02")
                {
                    var j02ids = inputitems.Select(p => p.j02id).Distinct();
                    mq.j02ids = j02ids.ToList();
                }
                if (itemsprefix == "p41")
                {
                    var p41ids = inputitems.Select(p => p.p41id).Distinct();
                    mq.p41ids = p41ids.ToList();
                }

                switch (groupby)
                {
                    case "Month":
                    case "Year":
                        mq.global_d2 = inputitems.Max(p => p.d1).AddMonths(1).AddDays(-1);
                        break;

                    case "Day":
                        mq.global_d2 = inputitems.Max(p => p.d1);
                        break;
                }
            }
            if (r01id > 0)
            {
                mq.SetPids(r01id.ToString());
            }

            var lis = Factory.r01CapacityBL.GetList(mq);
            if (lis.Count() == 0)
            {
                return StopPage(true, "Na vstupu chybí záznamy kapacitních plánů.");
            }

            var v = new EditViewModel() {ItemsPrefix=itemsprefix, Oper = oper, r02ID = r02id, lisItems = new List<PlanItem>() };


            foreach (var c in lis)
            {
                v.lisItems.Add(new PlanItem() { r01id = c.pid, p41id = c.p41ID, j02id = c.j02ID, d1 = c.r01Start, d2 = c.r01End, HoursFa = c.r01HoursFa, HoursNeFa = c.r01HoursNeFa, HoursTotal = c.r01HoursTotal, Memo = c.r01Text, IsIncludeWeekend = c.r01IsIncludeWeekend, Person = c.Person, Project = c.Project, TempGuid = BO.Code.Bas.GetGuid(), Color = c.r01Color, Client = c.Client });
            }


            RefreshState_Edit(v);

            return View(v);

        }
        private void RefreshState_Edit(EditViewModel v)
        {

            v.IsUseFaNefa = Factory.Lic.x01IsCapacityFaNefa;

            var p41ids = v.lisItems.Select(p => p.p41id).ToList();

            v.lisR04 = Factory.p41ProjectBL.GetList_r04(p41ids, null).OrderBy(p => p.Person);
            v.lisR01 = Factory.r01CapacityBL.GetList(new myQueryR01() { p41ids = p41ids, r02id = v.r02ID });

            foreach (var c in v.lisItems)
            {
                foreach (var recR04 in v.lisR04.Where(p => p.j02ID == c.j02id && p.p41ID == c.p41id))
                {
                    recR04.UserInsert = "rezerva";
                }
                foreach (var recR01 in v.lisR01.Where(p => p.j02ID == c.j02id && p.p41ID == c.p41id))
                {
                    recR01.Rezerva = true;
                }

            }


        }

        [HttpPost]
        public IActionResult Edit(EditViewModel v, string guid)
        {
            RefreshState_Edit(v);

            if (v.IsPostback)
            {
                switch (v.PostbackOper)
                {
                    case "delete_row":
                        v.lisItems.First(p => p.TempGuid == guid).IsTempDeleted = true;
                        break;
                    case "clear_rows":
                        v.lisItems.Clear();
                        break;

                }
                return View(v);
            }

            if (ModelState.IsValid)
            {
                if (v.Oper == "clear")
                {
                    foreach (var c in v.lisItems)
                    {
                        Factory.CBL.DeleteRecord("r01Capacity", c.r01id);
                    }
                    v.SetJavascript_CallOnLoad(0);
                    return View(v);
                }


                int x = 0;
                var lis2save = new List<r01Capacity>();

                foreach (var c in v.lisItems.Where(p => !p.IsTempDeleted))
                {
                    var rec = Factory.r01CapacityBL.Load(c.r01id);
                    rec.r01HoursFa = c.HoursFa;
                    rec.r01Start = c.d1;
                    rec.r01End = c.d2;
                    if (v.IsUseFaNefa)
                    {
                        rec.r01HoursNeFa = c.HoursNeFa;
                    }

                    rec.r01Text = c.Memo;
                    rec.r01Color = c.Color;

                    lis2save.Add(rec);

                    if (!Factory.r01CapacityBL.ValidateBeforeSave(rec))
                    {
                        return View(v);
                    }
                }
                foreach (var c in v.lisItems.Where(p => p.IsTempDeleted))
                {
                    Factory.CBL.DeleteRecord("r01Capacity", c.r01id);
                }
                foreach (var c in lis2save)
                {
                    x = x + Factory.r01CapacityBL.Save(c);

                }
                if (x > 0 || lis2save.Count() == 0)
                {
                    v.SetJavascript_CallOnLoad(0);
                    return View(v);
                }
                Notify_RecNotSaved();
            }
            else
            {
                Notify_RecNotSaved();
            }

            return View(v);


        }

        public IActionResult Create(string itemsprefix, int p41id, string items, string groupby, int r02id, int j02id)
        {
            if (groupby == "Year") groupby = "Month";

            if (string.IsNullOrEmpty(items))
            {
                return StopPage(true, "Na vstupu chybí označená plocha časového období plánu!");
            }
            var v = new CreateViewModel() { ItemsPrefix = itemsprefix, p41ID = p41id, SourceGroupBy = groupby, Items = items, InputCells = new List<InputPlanItem>(), InputIntervals = new List<InputPlanItem>(), r02ID = r02id };
            v.opgScale = Factory.CBL.LoadUserParam("CreatePlan-opgScale", "interval");
            if (v.p41ID == 0)
            {
                v.p41ID = Factory.CBL.LoadUserParamInt("CreatePlan-Last_p41ID", 0, 20);
            }

            v.ProjectCombo = new UI.Models.ProjectComboViewModel() { SelectedP41ID = v.p41ID };
            if (v.p41ID > 0)
            {
                v.RecP41 = Factory.p41ProjectBL.Load(v.p41ID);
            }

            SetupItemsCreate(items, v);




            RefreshState_Create(v);

            return View(v);
        }
        private void SetupItemsCreate(string items, CreateViewModel v)
        {
            if (v.RecP41 == null)
            {
                return;
            }
            bool bolCreateX = false;
            if (items == "all") //vytvořit plán pro celé období projektu
            {
                bolCreateX = true;                
                DateTime d1 = v.RecP41.p41PlanFrom != null ? Convert.ToDateTime(v.RecP41.p41PlanFrom) : new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                DateTime d2 = v.RecP41.p41PlanUntil != null ? Convert.ToDateTime(v.RecP41.p41PlanUntil) : new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(6);
                var lis = new List<string>();int xx = 0;
                for(DateTime d = d1; d <= d2; d=d.AddMonths(1))
                {
                    lis.Add($"{xx};{d.ToString("dd.MM.yyyy")}");
                    xx += 1;
                }
                items = string.Join("|", lis);
            }
            
            var prs = BO.Code.Bas.ConvertString2List(items, "|").OrderBy(p => p);
            bool bolMessage = false;
            DateTime datP1 = (v.RecP41.p41PlanFrom == null ? new DateTime(2000, 1, 1) : Convert.ToDateTime(v.RecP41.p41PlanFrom));
            DateTime datP2 = (v.RecP41.p41PlanUntil == null ? new DateTime(2100, 1, 1) : Convert.ToDateTime(v.RecP41.p41PlanUntil));
            
            IEnumerable<BO.r04CapacityResource> lisR04 = (v.p41ID>0 ? Factory.p41ProjectBL.GetList_r04(v.p41ID, 0): null);
            

            foreach (var s in prs)
            {
                var ss = s.Split(";");
                var c = new InputPlanItem() { d = ss[1], TempGuid = BO.Code.Bas.GetGuid() };
                if (v.ItemsPrefix == "j02")
                {
                    c.j02id = Convert.ToInt32(ss[0]);
                }


                var d0 = BO.Code.Bas.String2Date(c.d);

                switch (v.SourceGroupBy)
                {
                    case "Day":
                        c.d1 = d0; c.d2 = d0; c.dalias = d0.ToString("dd") + " " + BO.Code.Bas.DayOfWeekString(d0).Substring(0, 2);
                        break;
                    case "Month":
                        c.d1 = d0; c.d2 = d0.AddMonths(1).AddDays(-1); c.dalias = d0.ToString("MM") + "/" + d0.Year.ToString();
                        break;
                    case "Year":
                        c.d1 = new DateTime(d0.Year, 1, 1); c.d2 = new DateTime(d0.Year, 12, 31); c.dalias = d0.Year.ToString();
                        break;
                }
                bool bolOK = false;
                if (c.d1 >= datP1 && c.d2 <= datP2)
                {
                    bolOK = true;
                }
                else
                {
                    if (c.d1 < datP1 && c.d2 <= datP2 && c.d2 >= datP1)
                    {
                        c.d1 = datP1; bolOK = true;
                    }
                    if (c.d2 > datP2 && c.d1 >= datP1 && c.d1 <= datP2)
                    {
                        c.d2 = datP2; bolOK = true;
                    }
                    if (datP1 >= c.d1 && c.d2 >= datP2)
                    {
                        c.d1 = datP1; c.d2 = datP2; bolOK = true;
                    }

                    if (!bolMessage) bolMessage = true;

                }
                if (v.ItemsPrefix == "p41" && v.InputCells.Any(p => p.d1 == c.d1 && p.d2 == c.d2))    //vyhodit duplicitní datumy - osoby se doplní z r04 později
                {
                    bolOK = false;
                    this.AddMessageTranslated("Na vstupu může být pouze jeden projekt", "info");
                }
                if (bolOK)
                {
                    if (v.ItemsPrefix == "j02")
                    {
                        c.Person = Factory.j02UserBL.Load(c.j02id).FullnameDesc;
                        v.InputCells.Add(c);
                    }
                    if (v.ItemsPrefix == "p41")
                    {
                        foreach(var recR04 in lisR04)   //vložit všechny uživatele z kapacitních zdrojů
                        {
                            var ipi = new InputPlanItem() { j02id = recR04.j02ID, Person = recR04.Person, d1 = c.d1, d2 = c.d2 };                           
                            v.InputCells.Add(ipi);
                        }
                    }

                    
                }


            }

            if (!bolCreateX)
            {
                if (v.InputCells.Count() == 0)
                {
                    this.AddMessageTranslated("Plán období projektu je mimo vybrané období!");
                    return;
                }

                if (bolMessage)
                {
                    this.AddMessageTranslated("Na vstupu bylo třeba ořezovat některé plochy, aby se vešli do plánovaného období projektu!", "info");
                }
            }
            

          
            int intLastJ02ID = v.InputCells[0].j02id; DateTime datLastD1 = v.InputCells[0].d1; DateTime datLastD2 = v.InputCells[0].d1; string strLastD = v.InputCells[0].d; string strLastPerson = null;
            int x = 0;
            foreach (var c in v.InputCells.OrderBy(p => p.Person).ThenBy(p => p.d1))
            {
                if (x > 0 && (c.d1 != datLastD2.AddDays(1) || c.j02id != intLastJ02ID))
                {
                    var cc = new InputPlanItem() { j02id = intLastJ02ID, d1 = datLastD1, d2 = datLastD2, d = strLastD, Person = strLastPerson, TempGuid = BO.Code.Bas.GetGuid() };
                    v.InputIntervals.Add(cc);
                    datLastD1 = c.d1;
                    strLastD = c.d;
                }

                datLastD2 = c.d2;
                intLastJ02ID = c.j02id;
                strLastPerson = c.Person;
                x += 1;
            }

            var ccc = new InputPlanItem() { j02id = intLastJ02ID, d1 = datLastD1, d2 = datLastD2, d = strLastD, Person = strLastPerson };
            v.InputIntervals.Add(ccc);

        }
        private void RefreshState_Create(CreateViewModel v)
        {
            if (v.InputIntervals == null)
            {
                v.InputIntervals = new List<InputPlanItem>();
            }
            if (v.InputCells == null)
            {
                v.InputCells = new List<InputPlanItem>();
            }

            v.IsUseFaNefa = Factory.Lic.x01IsCapacityFaNefa;
            if (v.ProjectCombo == null)
            {
                v.ProjectCombo = new ProjectComboViewModel() { SelectedP41ID = v.p41ID };
            }
            if (v.RecP41 == null && v.p41ID > 0)
            {
                v.RecP41 = Factory.p41ProjectBL.Load(v.p41ID);
            }
            if (v.p41ID > 0)
            {
                v.FaZastropovan = BO.Code.Bas.bit_compare_or(v.RecP41.p41CapacityStream, 2);
                v.NeFaZastropovan = BO.Code.Bas.bit_compare_or(v.RecP41.p41CapacityStream, 4);

                v.lisR04 = Factory.p41ProjectBL.GetList_r04(v.p41ID, 0).OrderBy(p => p.Person);
                v.lisR01 = Factory.r01CapacityBL.GetList(new myQueryR01() { p41id = v.p41ID, r02id = v.r02ID });

                if (v.lisR04.Count() == 0)
                {

                    this.AddMessageTranslated("V projektu chybí personální zdroje pro plánování.");
                }
            }

        }

        [HttpPost]
        public IActionResult Create(CreateViewModel v, string guid)
        {
            RefreshState_Create(v);

            if (v.IsPostback)
            {
                switch (v.PostbackOper)
                {
                    case "delete_row":
                        if (v.opgScale == "cell")
                        {
                            v.InputCells.First(p => p.TempGuid == guid).IsTempDeleted = true;
                        }
                        else
                        {
                            v.InputIntervals.First(p => p.TempGuid == guid).IsTempDeleted = true;
                        }

                        break;
                    case "clear_rows":
                        if (v.opgScale == "cell")
                        {
                            v.InputCells.Clear();
                        }
                        else
                        {
                            v.InputIntervals.Clear();
                        }

                        break;
                    case "p41id":
                        if (v.ProjectCombo.SelectedP41ID > 0)
                        {
                            
                            return RedirectToAction("Create", new {itemsprefix=v.ItemsPrefix, p41id = v.ProjectCombo.SelectedP41ID, r02id = v.r02ID, items = v.Items, groupby = v.SourceGroupBy });
                        }
                        break;
                }
                return View(v);
            }

            if (ModelState.IsValid)
            {
                if (v.p41ID == 0)
                {
                    this.AddMessage("Chybí projekt.");
                    return View(v);
                }
                Factory.CBL.SetUserParam("CreatePlan-opgScale", v.opgScale);
                var lis = v.InputCells;
                if (v.opgScale == "interval")
                {
                    lis = v.InputIntervals;
                }
                int x = 0;

                foreach (var c in lis.Where(p => !p.IsTempDeleted && (p.HoursFa > 0 || p.HoursNeFa > 0)))
                {
                    var rec = new r01Capacity() { r02ID = v.r02ID, p41ID = v.p41ID, r01Start = c.d1, r01End = c.d2, r01Text = c.Memo, j02ID = c.j02id, r01HoursFa = c.HoursFa, r01HoursNeFa = c.HoursNeFa };

                    if (!Factory.r01CapacityBL.ValidateBeforeSave(rec))
                    {
                        return View(v);
                    }
                }
                foreach (var c in lis.Where(p => !p.IsTempDeleted && (p.HoursFa > 0 || p.HoursNeFa > 0)))
                {
                    var rec = new r01Capacity() { r02ID = v.r02ID, p41ID = v.p41ID, r01Start = c.d1, r01End = c.d2, r01Text = c.Memo, j02ID = c.j02id, r01HoursFa = c.HoursFa, r01HoursNeFa = c.HoursNeFa, r01Color = c.Color };

                    x += Factory.r01CapacityBL.Save(rec);
                }
                if (x > 0)
                {
                    Factory.CBL.SetUserParam("CreatePlan-Last_p41ID", v.p41ID.ToString());
                    v.SetJavascript_CallOnLoad(v.p41ID);
                    return View(v);
                }
                Notify_RecNotSaved();
            }
            else
            {
                Notify_RecNotSaved();
            }

            return View(v);


        }


        public IActionResult ProjectPlan(int p41id)
        {
            var v = new ProjectPlanViewModel() { p41ID = p41id, timeline = new CapacityTimelineJ02ViewModel() };

            if (v.p41ID == 0)
            {
                return StopPage(true, "Na vstupu chybí projekt.");
            }
            v.timeline.p41ID = v.p41ID;
            v.timeline.UserKeyBase = "TabR01";

            v.HasOwnerPermissions = Factory.p41ProjectBL.InhaleRecDisposition(v.p41ID).OwnerAccess;
            v.timeline.IsReadOnly = !v.HasOwnerPermissions;

            v.RecP41 = Factory.p41ProjectBL.Load(v.p41ID);


            v.timeline.RecP41 = v.RecP41;

            v.p41PlanFrom = v.RecP41.p41PlanFrom == null ? new DateTime(DateTime.Now.Year, 1, 1) : Convert.ToDateTime(v.RecP41.p41PlanFrom);
            v.p41PlanUntil = v.RecP41.p41PlanUntil == null ? new DateTime(DateTime.Now.Year + 1, 12, 31) : Convert.ToDateTime(v.RecP41.p41PlanUntil);

            v.timeline.CurMonth = v.p41PlanFrom.Month;
            v.timeline.CurYear = v.p41PlanFrom.Year;




            return View(v);
        }





        



        public IActionResult ProjectResources(int p41id)
        {
            var v = new ProjectResourcesViewModel() { p41ID = p41id, lisR04 = new List<r04Repeater>() };
            if (v.p41ID == 0)
            {
                return StopPage(true, "Na vstupu chybí projekt.");
            }
            var lis = Factory.p41ProjectBL.GetList_r04(v.p41ID, 0);
            foreach (var c in lis)
            {
                v.lisR04.Add(new r04Repeater()
                {
                    TempGuid = BO.Code.Bas.GetGuid(),
                    pid = c.pid,
                    j02ID = c.j02ID,
                    FullName = c.Person,
                    r04Text = c.r04Text,
                    r04HoursFa = c.r04HoursFa,
                    r04HoursNeFa = c.r04HoursNeFa,
                    r04HoursTotal = c.r04HoursTotal,

                    r04WorksheetFlag = c.r04WorksheetFlag
                });
            }
            v.RecP41 = Factory.p41ProjectBL.Load(v.p41ID);
            v.CapacityStream = v.RecP41.p41CapacityStream;

            RefreshState_ProjectResources(v);

            if (v.lisR04.Count() == 0 && v.CapacityStream == 0) v.CapacityStream = 30;

            return View(v);
        }

        private void RefreshState_ProjectResources(ProjectResourcesViewModel v)
        {
            v.IsUseFaNefa = Factory.Lic.x01IsCapacityFaNefa;
            if (v.lisR04 == null)
            {
                v.lisR04 = new List<r04Repeater>();
            }
            v.lisJ11 = Factory.j11TeamBL.GetList(new BO.myQueryJ11());
            v.lisProjectRoles = Factory.x67EntityRoleBL.GetList(new BO.myQuery("x67")).Where(p => p.x67Entity == "p41");
            if (v.RecP41 == null)
            {
                v.RecP41 = Factory.p41ProjectBL.Load(v.p41ID);
            }
            v.lisR01 = Factory.r01CapacityBL.GetList(new BO.myQueryR01() { p41id = v.p41ID, j02ids = v.lisR04.Where(p => !p.IsTempDeleted).Select(p => p.j02ID).ToList() });
            v.FaZastropovan = BO.Code.Bas.bit_compare_or(v.CapacityStream, 2);
            v.NeFaZastropovan = BO.Code.Bas.bit_compare_or(v.CapacityStream, 4);
        }

        [HttpPost]
        public IActionResult ProjectResources(ProjectResourcesViewModel v, string guid)
        {
            RefreshState_ProjectResources(v);

            if (v.IsPostback)
            {
                switch (v.PostbackOper)
                {
                    case "j11id":
                        var lisJ02 = Factory.j02UserBL.GetList(new BO.myQueryJ02() { j11id = v.SelectedJ11ID });
                        foreach(var per in lisJ02)
                        {
                            if (!v.lisR04.Any(p => p.j02ID == per.pid))
                            {
                                v.lisR04.Add(new r04Repeater() { TempGuid = BO.Code.Bas.GetGuid(), j02ID = per.pid, FullName = per.FullnameDesc });
                            }
                        }
                        break;
                    case "x67id":
                        var lis = Factory.x67EntityRoleBL.GetList_X69("p41", v.p41ID);

                        foreach (var ass in lis.Where(p => p.x67ID == v.SelectedX67ID))
                        {
                            if (ass.j02ID > 0 && !v.lisR04.Any(p => p.j02ID == ass.j02ID))
                            {
                                v.lisR04.Add(new r04Repeater() { TempGuid = BO.Code.Bas.GetGuid(), j02ID = ass.j02ID, FullName = ass.Person });
                            }
                            if (ass.j11ID > 0)
                            {
                                foreach (var per in Factory.j02UserBL.GetList(new BO.myQueryJ02() { j11id = ass.j11ID }))
                                {
                                    if (!v.lisR04.Any(p => p.j02ID == per.pid))
                                    {
                                        v.lisR04.Add(new r04Repeater() { TempGuid = BO.Code.Bas.GetGuid(), j02ID = per.pid, FullName = per.FullnameDesc });
                                    }

                                }
                            }
                            if (ass.x69IsAllUsers)
                            {
                                foreach (var per in Factory.j02UserBL.GetList(new BO.myQueryJ02()))
                                {
                                    if (!v.lisR04.Any(p => p.j02ID == per.pid))
                                    {
                                        v.lisR04.Add(new r04Repeater() { TempGuid = BO.Code.Bas.GetGuid(), j02ID = per.pid, FullName = per.FullnameDesc });
                                    }

                                }
                            }

                        }
                        break;

                    case "add_row":
                        var c = new r04Repeater() { TempGuid = BO.Code.Bas.GetGuid() };
                        v.lisR04.Add(c);
                        break;

                    case "delete_row":
                        v.lisR04.First(p => p.TempGuid == guid).IsTempDeleted = true;
                        break;
                    case "clear_rows":
                        v.lisR04.Clear();
                        break;

                }
                return View(v);
            }

            if (ModelState.IsValid)
            {
                var lis = new List<BO.r04CapacityResource>();
                foreach (var c in v.lisR04.Where(p => !p.IsTempDeleted))
                {
                    c.r04HoursFa = Math.Round(c.r04HoursFa, 0);
                    c.r04HoursFa = Math.Round(c.r04HoursFa, 0);
                    if (v.FaZastropovan && c.r04HoursFa <= 0)
                    {
                        this.AddMessageTranslated($"U zdroje [{c.FullName}] chybí vyplnit stop hodin.");
                        return View(v);
                    }
                    double dblPlan = Math.Round(v.lisR01.Where(p => p.j02ID == c.j02ID).Sum(p => p.r01HoursFa), 0);
                    if (v.FaZastropovan && (c.r04HoursFa < dblPlan))
                    {
                        this.AddMessageTranslated($"U zdroje [{c.FullName}] je strop ({c.r04HoursFa}) nižší než součet již naplánovaných hodin ({dblPlan}).");
                        return View(v);
                    }
                    lis.Add(new BO.r04CapacityResource()
                    {
                        j02ID = c.j02ID,
                        r04HoursFa = c.r04HoursFa,
                        r04HoursNeFa = c.r04HoursNeFa,
                        r04Text = c.r04Text,
                        r04WorksheetFlag = c.r04WorksheetFlag
                    });

                }

                if (Factory.p41ProjectBL.Save_r04list(v.p41ID, lis))
                {
                    v.RecP41.p41CapacityStream = v.CapacityStream;
                    Factory.p41ProjectBL.Save(v.RecP41, null, null, null);
                    v.SetJavascript_CallOnLoad(v.p41ID);
                    return View(v);
                }
                this.Notify_RecNotSaved();
            }
            else
            {
                this.Notify_RecNotSaved();
            }

            return View(v);
        }
    }
}
