using Microsoft.AspNetCore.Mvc;
using UI.Models;
using UI.Models.Record;
using UI.Models.Tab1;

namespace UI.Controllers
{
    public class p49Controller : BaseController
    {

        public IActionResult Info(int pid)
        {
            return Tab1(pid, "info");
        }
        public IActionResult Tab1(int pid, string caller)
        {
            var v = new p49Tab1() { Factory = this.Factory, pid = pid, caller = caller };
           
            v.Rec = Factory.p49FinancialPlanBL.Load(v.pid);
            if (v.Rec != null)
            {
                
                v.RecP41 = Factory.p41ProjectBL.Load(v.Rec.p41ID);

            }

            return View(v);
        }

        public IActionResult Record(int pid, bool isclone, int p41id,int p56id)
        {            
            var v = new p49Record() { rec_pid = pid, rec_entity = "p49", ComboJ02 = Factory.CurrentUser.FullnameDesc };
            if (p56id > 0 && pid == 0)
            {
                var recP56=Factory.p56TaskBL.Load(p56id);
                p41id = recP56.p41ID;v.ComboP56 = recP56.p56Name;
            }
            v.ProjectCombo = new ProjectComboViewModel() { SelectedP41ID = p41id };
            v.Rec = new BO.p49FinancialPlan() { j02ID = Factory.CurrentUser.pid,p56ID=p56id,p41ID=p41id, p49Date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1) };

            if (v.rec_pid > 0)
            {
                v.Rec = Factory.p49FinancialPlanBL.Load(v.rec_pid);
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }
                v.ComboJ02 = v.Rec.j02Name;
                v.ComboP32 = v.Rec.p32Name;
                v.ComboP34 = v.Rec.p34Name;
                v.ComboJ27Code = v.Rec.j27Code;
                BO.p41Project recP41 = Factory.p41ProjectBL.Load(v.Rec.p41ID);
                v.ProjectCombo.SelectedP41ID = v.Rec.p41ID;
                if (v.Rec.p28ID_Supplier > 0)
                {
                    v.ComboSupplier = Factory.p28ContactBL.Load(v.Rec.p28ID_Supplier).p28Name;
                }
                if (v.Rec.p56ID > 0)
                {                    
                    v.ComboP56 = v.Rec.p56Name;
                }
                
            }
            else
            {
                //nový záznam                
               
                if (v.rec_pid == 0 && !isclone) //výchozí hodnoty nového kontaktu natáhnout z naposledy mnou vytvořeného
                {                   
                    var qry = Factory.p49FinancialPlanBL.GetList(new BO.myQueryP49() { j02id = Factory.CurrentUser.pid, TopRecordsOnly = 1, explicit_orderby = "a.p49ID DESC" });
                    if (qry.Count() > 0)
                    {
                        v.Rec.p34ID = qry.First().p34ID;
                        v.ComboP34 = qry.First().p34Name;
                        v.Rec.p32ID = qry.First().p32ID;
                        v.ComboP32 = qry.First().p32Name;
                        v.Rec.j27ID = qry.First().j27ID;
                        v.ComboJ27Code = qry.First().j27Code;
                        
                        v.Rec.p49Text = qry.First().p49Text;
                    }

                }

                
               
                if (v.Rec.j27ID == 0)
                {
                    v.Rec.j27ID = Factory.Lic.j27ID;
                    v.ComboJ27Code = Factory.FBL.LoadCurrencyByID(v.Rec.j27ID).j27Code;
                }

            }

            if (!TestNeededPermissions(v.Rec.p41ID))
            {
                return this.StopPage(true, "Nedisponujete vlastnickým oprávněním k projektu.");
            }


            RefreshState(v);
            v.Toolbar = new MyToolbarViewModel(v.Rec);
            if (isclone)
            {
                v.MakeClone();
            }

            return View(v);
        }

        private bool TestNeededPermissions(int p41id)
        {
            if (p41id == 0) return true;
            var mydisp = Factory.p41ProjectBL.InhaleRecDisposition(p41id);
            return mydisp.OwnerAccess;
        }



        private void RefreshState(p49Record v)
        {

            v.ProjectCombo.CssClassDiv = "col-sm-10 col-md-9";
            if (v.Rec.p34ID > 0)
            {
                v.RecP34 = Factory.p34ActivityGroupBL.Load(v.Rec.p34ID);
            }
            if (v.ProjectCombo.SelectedP41ID > 0)
            {
                v.lisP56 = Factory.p56TaskBL.GetList(new BO.myQueryP56() {p41id=v.ProjectCombo.SelectedP41ID, IsRecordValid = null });
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(p49Record v)
        {
            RefreshState(v);
            if (v.IsPostback)
            {
                if (v.PostbackOper == "p34id")
                {
                    v.Rec.p32ID = 0;
                    v.ComboP32 = null;
                }
                if (v.PostbackOper == "m+1")
                {
                    v.Rec.p49Date=v.Rec.p49Date.Value.AddMonths(1);
                }
                if (v.PostbackOper == "m-1")
                {
                    v.Rec.p49Date=v.Rec.p49Date.Value.AddMonths(-1);
                }
                if (v.PostbackOper == "y+1")
                {
                    v.Rec.p49Date = v.Rec.p49Date.Value.AddYears(1);
                }
                if (v.PostbackOper == "y-1")
                {
                    v.Rec.p49Date = v.Rec.p49Date.Value.AddYears(-1);
                }

                return View(v);
            }

            if (ModelState.IsValid)
            {

                BO.p49FinancialPlan c = new BO.p49FinancialPlan();
                if (v.rec_pid > 0) c = Factory.p49FinancialPlanBL.Load(v.rec_pid);
                
                c.p41ID = v.ProjectCombo.SelectedP41ID;
                c.j02ID = v.Rec.j02ID;
                c.p56ID = v.Rec.p56ID;
                c.p34ID = v.Rec.p34ID;
                c.p32ID = v.Rec.p32ID;
                c.j27ID = v.Rec.j27ID;
                c.p49Text = v.Rec.p49Text;
                c.p49Amount = v.Rec.p49Amount;
                c.p49Date = v.Rec.p49Date;
               
                c.p28ID_Supplier = v.Rec.p28ID_Supplier;
                c.p49PieceAmount = v.Rec.p49PieceAmount;
                c.p49Pieces = v.Rec.p49Pieces;
                c.p49MarginHidden = v.Rec.p49MarginHidden;
                c.p49MarginTransparent = v.Rec.p49MarginTransparent;
                c.p49StatusFlag = v.Rec.p49StatusFlag;


                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);


                if (!TestNeededPermissions(c.p41ID))
                {
                    return this.StopPage(true, "Nedisponujete vlastnickým oprávněním k projektu.");
                }


                c.pid = Factory.p49FinancialPlanBL.Save(c);
                if (c.pid > 0)
                {
                    v.SetJavascript_CallOnLoad(c.pid);
                    return View(v);
                }





            }


            this.Notify_RecNotSaved();
            return View(v);
        }

        // ---------------------------------------------------------------------------------------
        // Rozpočet projektu: mřížka aktivita × měsíc nad položkami p49. Bez p41id přehled rozpočtů.
        // ---------------------------------------------------------------------------------------
        public IActionResult Budget(int p41id)
        {
            if (!Factory.CurrentUser.j04IsModule_p49)
            {
                return this.StopPage(false, "Nemáte oprávnění pro tento Modul.");
            }
            var v = new UI.Models.p49BudgetViewModel() { p41id = p41id, Year = DateTime.Today.Year };
            if (v.p41id > 0)
            {
                v.RecP41 = Factory.p41ProjectBL.Load(v.p41id);
                if (v.RecP41 == null || !Factory.p41ProjectBL.InhaleRecDisposition(v.p41id, v.RecP41).ReadAccess)
                {
                    return this.StopPage(false, "Projekt nebyl nalezen nebo k němu nemáte přístup.");
                }
                v.CanEdit = Factory.p49FinancialPlanBL.CanEditBudget(v.p41id);
            }
            return View(v);
        }

        private IActionResult BudgetFail(string strMessage)
        {
            return Json(new { ok = false, message = strMessage });
        }
        private static double R2(double d) { return Math.Round(d, 2); }

        public IActionResult BudgetData(int p41id, int year)
        {
            if (!Factory.CurrentUser.j04IsModule_p49) return BudgetFail(Factory.tra("Nemáte oprávnění pro tento Modul."));
            if (p41id == 0) return BudgetOverviewData();

            var recP41 = Factory.p41ProjectBL.Load(p41id);
            if (recP41 == null || !Factory.p41ProjectBL.InhaleRecDisposition(p41id, recP41).ReadAccess)
            {
                return BudgetFail(Factory.tra("Projekt nebyl nalezen nebo k němu nemáte přístup."));
            }
            if (year < 1990 || year > 2200) year = DateTime.Today.Year;

            var lisPlan = Factory.p49FinancialPlanBL.GetList(new BO.myQueryP49() { p41id = p41id }).Where(p => p.p49Date != null && (p.p33ID == 2 || p.p33ID == 5)).ToList();
            var lisAct = Factory.p49FinancialPlanBL.GetBudgetActuals(p41id).ToList();
            var lisP32 = Factory.p49FinancialPlanBL.GetBudgetActivities().ToList();
            int intJ27ID_Default = Factory.Lic.j27ID;
            string strJ27Default = intJ27ID_Default > 0 ? Factory.FBL.LoadCurrencyByID(intJ27ID_Default)?.j27Code : null;

            //řádek = aktivita + měna
            var rows = new Dictionary<string, BudgetRow>();
            BudgetRow GetRow(int p32id, string p32name, int p34id, string p34name, int p34ord, int strana, int j27id, string j27code)
            {
                string key = p32id.ToString() + "-" + j27id.ToString();
                if (!rows.TryGetValue(key, out BudgetRow r))
                {
                    r = new BudgetRow() { key = key, p32id = p32id, name = p32name, p34id = p34id, p34name = p34name, p34ord = p34ord, side = strana, j27id = j27id, currency = j27code };
                    rows.Add(key, r);
                }
                return r;
            }
            foreach (var c in lisPlan)
            {
                var a = lisP32.FirstOrDefault(p => p.p32ID == c.p32ID);
                var r = GetRow(c.p32ID, c.p32Name, c.p34ID, c.p34Name, a == null ? 0 : a.p34Ordinary, c.strana == "P" ? 2 : 1, c.j27ID, c.j27Code);
                r.planTotal += c.p49Amount;
                if (c.p49Date.Value.Year == year)
                {
                    int m = c.p49Date.Value.Month - 1;
                    r.plan[m] += c.p49Amount;
                    r.items[m].Add(new { id = c.pid, amount = R2(c.p49Amount), text = c.p49Text, date = c.p49Date.Value.ToString("dd.MM.yyyy"), done = c.p49StatusFlag == 1 });
                }
            }
            foreach (var c in lisAct)
            {
                int j27id = c.j27ID == 0 ? intJ27ID_Default : c.j27ID;
                var r = GetRow(c.p32ID, c.p32Name, c.p34ID, c.p34Name, c.p34Ordinary, c.Strana, j27id, c.j27ID == 0 ? strJ27Default : c.j27Code);
                r.actTotal += c.Amount;
                if (c.Rok == year) r.act[c.Mesic - 1] += c.Amount;
            }

            var years = lisPlan.Select(p => p.p49Date.Value.Year).Concat(lisAct.Select(p => p.Rok)).Append(DateTime.Today.Year).Append(year).Distinct().OrderBy(p => p).ToList();

            return Json(new
            {
                ok = true,
                mode = "project",
                year = year,
                years = years,
                canEdit = Factory.p49FinancialPlanBL.CanEditBudget(p41id),
                currency = strJ27Default,
                j27id = intJ27ID_Default,
                rows = rows.Values.OrderBy(p => p.side).ThenBy(p => p.p34ord).ThenBy(p => p.p34name).ThenBy(p => p.name).ThenBy(p => p.currency).Select(p => new
                {
                    p.key, p.p32id, p.name, p.p34id, p.p34name, p.side, p.j27id, p.currency,
                    plan = p.plan.Select(x => R2(x)), act = p.act.Select(x => R2(x)), p.items,
                    planTotal = R2(p.planTotal), actTotal = R2(p.actTotal)
                }),
                activities = lisP32.Select(p => new { id = p.p32ID, name = p.p32Name, p34name = p.p34Name, side = p.Strana })
            });
        }

        private class BudgetRow
        {
            public string key; public int p32id; public string name; public int p34id; public string p34name; public int p34ord; public int side; public int j27id; public string currency;
            public double[] plan = new double[12]; public double[] act = new double[12];
            public List<object>[] items = Enumerable.Range(0, 12).Select(p => new List<object>()).ToArray();
            public double planTotal; public double actTotal;
        }

        private IActionResult BudgetOverviewData()
        {
            var lis = Factory.p49FinancialPlanBL.GetBudgetOverview().ToList();
            //jen projekty, které přihlášený smí vidět
            var pids = lis.Select(p => p.p41ID).Distinct().ToList();
            var visible = new HashSet<int>();
            if (pids.Count > 0)
            {
                visible = Factory.p41ProjectBL.GetList(new BO.myQueryP41("p41") { pids = pids, IsRecordValid = null }).Select(p => p.pid).ToHashSet();
            }
            return Json(new
            {
                ok = true,
                mode = "overview",
                rows = lis.Where(p => visible.Contains(p.p41ID)).Select(p => new
                {
                    p41id = p.p41ID, project = p.Project, code = p.p41Code, client = p.Client, currency = p.j27Code,
                    planExp = R2(p.PlanVydaje), actExp = R2(p.SkutVydaje), planRev = R2(p.PlanPrijmy), actRev = R2(p.SkutPrijmy)
                })
            });
        }

        //nastaví plán buňky (aktivita + měna + měsíc) na zadanou částku
        [HttpPost]
        public IActionResult BudgetSaveCell(int p41id, int p32id, int j27id, int year, int month, string amount)
        {
            if (!Factory.p49FinancialPlanBL.CanEditBudget(p41id)) return BudgetFail(Factory.tra("Rozpočet tohoto projektu nemůžete upravovat."));
            if (month < 1 || month > 12 || year < 1990 || year > 2200) return BudgetFail(Factory.tra("Neplatné období."));
            double dbl = 0;
            if (!string.IsNullOrWhiteSpace(amount) && !double.TryParse(amount.Trim().Replace(" ", "").Replace(" ", "").Replace(",", "."), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out dbl))
            {
                return BudgetFail(Factory.tra("Částka není číslo."));
            }
            if (dbl < 0) return BudgetFail(Factory.tra("Částka nesmí být záporná."));

            var recP32 = Factory.p32ActivityBL.Load(p32id);
            if (recP32 == null) return BudgetFail(Factory.tra("Aktivita nebyla nalezena."));
            var recP34 = Factory.p34ActivityGroupBL.Load(recP32.p34ID);
            if (recP34 == null || (recP34.p33ID != BO.p33IdENUM.PenizeBezDPH && recP34.p33ID != BO.p33IdENUM.PenizeVcDPHRozpisu))
            {
                return BudgetFail(Factory.tra("Do rozpočtu patří jen aktivity peněžních výdajů a příjmů. Hodiny se plánují v kapacitním plánu, kusovník do rozpočtu nepatří."));
            }
            if (j27id == 0) j27id = Factory.Lic.j27ID;

            DateTime d1 = new DateTime(year, month, 1); DateTime d2 = d1.AddMonths(1).AddDays(-1);
            var lis = Factory.p49FinancialPlanBL.GetList(new BO.myQueryP49() { p41id = p41id }).Where(p => p.p32ID == p32id && p.j27ID == j27id && p.p49Date != null && p.p49Date.Value.Date >= d1 && p.p49Date.Value.Date <= d2).ToList();
            if (lis.Count > 1)
            {
                return BudgetFail(Factory.tra("V tomto měsíci je víc položek rozpočtu. Upravte je jednotlivě v detailu buňky."));
            }
            Factory.CurrentUser.Messages4Notify = null;
            if (lis.Count == 1)
            {
                var rec = lis[0];
                if (dbl == 0)
                {
                    string strRet = Factory.CBL.DeleteRecord("p49", rec.pid);
                    if (strRet != "1") return BudgetFail(string.IsNullOrEmpty(strRet) ? Factory.tra("Položku se nepodařilo odstranit.") : strRet);
                    return Json(new { ok = true });
                }
                rec.p49Amount = dbl;
                if (rec.p49Pieces <= 1) { rec.p49Pieces = 1; rec.p49PieceAmount = dbl; }    //položka s kusy si rozpad drží, mění se jen částka
                if (Factory.p49FinancialPlanBL.Save(rec) == 0) return BudgetFail(BudgetTakeMessage());
                return Json(new { ok = true });
            }
            if (dbl == 0) return Json(new { ok = true });

            var recP41 = Factory.p41ProjectBL.Load(p41id);
            var c = new BO.p49FinancialPlan()
            {
                p41ID = p41id, p32ID = p32id, p34ID = recP32.p34ID, j27ID = j27id,
                j02ID = recP41.j02ID_Owner > 0 ? recP41.j02ID_Owner : Factory.CurrentUser.pid,
                p49Date = d2, p49Amount = dbl, p49Pieces = 1, p49PieceAmount = dbl
            };
            if (Factory.p49FinancialPlanBL.Save(c) == 0) return BudgetFail(BudgetTakeMessage());
            return Json(new { ok = true });
        }

        private string BudgetTakeMessage()
        {
            string s = Factory.GetFirstNotifyMessage();
            Factory.CurrentUser.Messages4Notify = null;
            return string.IsNullOrEmpty(s) ? Factory.tra("Záznam se nepodařilo uložit.") : s;
        }
    }
}
