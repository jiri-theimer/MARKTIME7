using Microsoft.AspNetCore.Mvc;

using UI.Models;
using UI.Models.Record;
using UI.Models.Tab1;

namespace UI.Controllers
{
    
    public class p51Controller : BaseController
    {
        public IActionResult Info(int pid)
        {
            var v = new BaseTab1ViewModel() { prefix = "p51", pid = pid };
            return View(v);
        }
        public IActionResult Recalc(int p51id,string flag,bool isafter_p51record)
        {
            var v = new p51RecalcViewModel() {p51id=p51id,flag=flag,isafter_p51record=isafter_p51record};

            RefreshStateRecalc(v);
            if (v.flag == null)
            {
                v.d1 = v.Rec.ValidFrom;
            }
            if (v.isafter_p51record)
            {
                //přesměrováno sem po uložení ceníku
                var lisP31 = Factory.p31WorksheetBL.GetList(new BO.myQueryP31() { p51id=v.p51id,iswip=true});
                if (lisP31.Count() > 0)
                {
                    v.d1 = lisP31.Min(p => p.p31Date);
                    v.d2 = lisP31.Max(p => p.p31Date);
                }
                v.pocetRozpracovanychUkonu = lisP31.Count();
                v.CostRatesRecalcFlag = "drzet_cenik";
            }

            return View(v);
        }

        private void RefreshStateRecalc(p51RecalcViewModel v)
        {
            if (v.Rec == null)
            {
                v.Rec = Factory.p51PriceListBL.Load(v.p51id);
                

            }
        }
        [HttpPost]        
        public IActionResult Recalc(p51RecalcViewModel v)
        {
            RefreshStateRecalc(v);
            if (v.IsPostback)
            {
                if (v.PostbackOper == "recalc")
                {
                    if (v.d1 == null || v.d2 == null)
                    {
                        this.AddMessage("Na vstupu chybí časové období přepočtu.");
                        return View(v);
                    }
                    if (v.flag == "fpr")
                    {
                        Factory.p51PriceListBL.RecalcFPR(v.p51id, v.d1.Value, v.d2.Value);
                        this.AddMessageTranslated($"Přepočet dokončen.", "info");
                        return View(v);
                    }

                    var mq = new BO.myQueryP31() { global_d1 = v.d1, global_d2 = v.d2 };
                    switch (v.Rec.p51TypeFlag)
                    {
                        case BO.p51TypeFlagENUM.BillingRates:
                        case BO.p51TypeFlagENUM.RootBillingRates:
                            mq.p51id_billingrate = v.p51id;
                            mq.iswip_or_excluded = true;
                            
                            break;
                        case BO.p51TypeFlagENUM.CostRates:
                            if (v.CostRatesRecalcFlag== "drzet_cenik")
                            {
                                mq.p51id_costrate = v.p51id;
                            }
                            
                            break;
                        default:

                            break;
                    }

                    var lisP31 = Factory.p31WorksheetBL.GetList(mq);
                    switch (v.Rec.p51TypeFlag)
                    {
                        case BO.p51TypeFlagENUM.BillingRates:
                        case BO.p51TypeFlagENUM.RootBillingRates:
                            Factory.p31WorksheetBL.Recalc(lisP31.Select(p => p.pid).ToList(),1);
                            break;
                        case BO.p51TypeFlagENUM.CostRates:
                            Factory.p31WorksheetBL.Recalc(lisP31.Select(p => p.pid).ToList(), 2);
                            break;
                        case BO.p51TypeFlagENUM.OverheadRates:
                            Factory.p31WorksheetBL.Recalc(lisP31.Select(p => p.pid).ToList(), 3);
                            break;
                    }
                    
                    this.AddMessageTranslated($"Počet přepočtených úkonů: {lisP31.Count()}.", "info");
                    return View(v);
                }
            }

            if (ModelState.IsValid)
            {
                

                v.SetJavascript_CallOnLoad(v.p51id);
                return View(v);

            }


            this.Notify_RecNotSaved();
            return View(v);
        }
        
        public IActionResult Tab1(int pid, string caller)
        {
            var v = new p51Tab1() { Factory = this.Factory,pid = pid, caller = caller };

            v.Rec = Factory.p51PriceListBL.Load(v.pid);
            if (v.Rec != null)
            {
                v.RecSum = Factory.p51PriceListBL.LoadSumRow(v.Rec.pid);
                v.lisP52 = Factory.p51PriceListBL.GetList_p52(v.pid);
               
            }

            return View(v);
        }
        public IActionResult Record(int pid, bool isclone,bool iscustom,string tempguid,int p51typeflag)
        {
            var v = new p51Record() { rec_pid = pid, rec_entity = "p51",TempGuid=tempguid };
            v.Rec = new BO.p51PriceList();
            if (p51typeflag > 0)
            {
                v.Rec.p51TypeFlag = (BO.p51TypeFlagENUM)p51typeflag;
            }
            if (v.TempGuid !=null && v.rec_pid == 0)
            {
                //zjistit, zda již neexistuje rozdělaný temp ceník
                var c = Factory.p51PriceListBL.LoadByTempGuid(v.TempGuid);
                if (c !=null) v.rec_pid = c.pid;

            }
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.p51PriceListBL.Load(v.rec_pid);
                
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }
                v.ComboJ27Code = v.Rec.j27Code;
                v.d1 = v.Rec.ValidFrom;
                v.d2 = v.Rec.ValidUntil;

                var lis= Factory.p51PriceListBL.GetList_p52(v.rec_pid).ToList();
                v.lisP52 = new List<p52Repeater>();
                foreach (var c in lis)
                {
                    var cc = new p52Repeater() {
                        TempGuid = BO.Code.Bas.GetGuid(),j02ID=c.j02ID,j07ID=c.j07ID,p32ID=c.p32ID,p34ID=c.p34ID,p52Rate=c.p52Rate
                        ,ComboPerson=c.Person,ComboJ07Name=c.j07Name,ComboP32Name=c.p32Name
                        ,ComboP34Name=c.p34Name
                    };
                    cc.RowPrefixWho = "all";
                    if (c.j02ID > 0)
                    {
                        cc.RowPrefixWho = "j02";
                    }
                    if (c.j07ID > 0)
                    {
                        cc.RowPrefixWho = "j07";
                    }                    
                    if (c.p32ID > 0)
                    {
                        cc.RowPrefixActivity = "p32";
                    }
                    else
                    {
                        cc.RowPrefixActivity = "p34";
                    }                    
                    if (c.p52IsPlusAllTimeSheets)
                    {
                        cc.RowPrefixActivity = "all";
                    }
                    v.lisP52.Add(cc);                    
                }

            }
            else
            {
                v.Rec.j27ID = Factory.Lic.j27ID;
                v.ComboJ27Code = Factory.FBL.LoadCurrencyByID(Factory.Lic.j27ID).j27Code;
                //nový záznam
                if (iscustom)
                {
                    v.Rec.p51IsCustomTailor = true;
                    
                }
            }
            
           
            RefreshState(v);
            v.Toolbar = new MyToolbarViewModel(v.Rec);
            v.Toolbar.AllowArchive = false;
            if (isclone)
            {
                v.MakeClone();
                v.Rec.p51IsCustomTailor = false;v.Rec.p51IsMasterPriceList = false;v.Rec.p51IsFPR = false;
            }

            return ViewTup(v, BO.PermValEnum.GR_p51_Admin);
        }

        private void RefreshState(p51Record v)
        {
            if (v.lisP52 == null)
            {
                v.lisP52 = new List<p52Repeater>();
            }

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(p51Record v,string guid)
        {
            RefreshState(v);
            if (v.IsPostback)
            {
                switch (v.PostbackOper)
                {
                    case "clear_tailor":
                        v.Rec.p51IsCustomTailor = false;
                        v.TempGuid = null;
                        this.AddMessageTranslated("Změny je třeba potvrdit tlačítkem [Uložit změny].", "info");
                        return View(v);
                        
                    case "add_row":
                        var c = new p52Repeater() { TempGuid = BO.Code.Bas.GetGuid(), RowPrefixWho = "j07", RowPrefixActivity = "p34" };
                        var recP34 = Factory.p34ActivityGroupBL.GetList(new BO.myQueryP34() { ismoneyinput = false }).First();
                        c.p34ID = recP34.pid; c.ComboP34Name = recP34.p34Name;
                        v.lisP52.Add(c);
                        return View(v);
                    case "clone_row":
                        var row = v.lisP52.Where(p => p.TempGuid == guid).First();
                        var d = new p52Repeater()
                        {
                            TempGuid = BO.Code.Bas.GetGuid(),
                            RowPrefixWho = row.RowPrefixWho,
                            RowPrefixActivity = row.RowPrefixActivity,                            
                            j02ID = row.j02ID,
                            j07ID = row.j07ID,
                            p32ID = row.p32ID,
                            p34ID = row.p34ID,
                            p52Rate = row.p52Rate,                            
                            ComboJ07Name = row.ComboJ07Name,
                            ComboP34Name = row.ComboP34Name,
                            ComboPerson = row.ComboPerson,
                            ComboP32Name = row.ComboP32Name
                        };
                        v.lisP52.Add(d);
                        return View(v);
                    case "delete_row":
                        v.lisP52.First(p => p.TempGuid == guid).IsTempDeleted = true;
                        return View(v);
                    case "clear_rows":
                        v.lisP52.Clear();
                        return View(v);
                    default:
                        return View(v);
                }
            }
            
            

            if (ModelState.IsValid)
            {
                BO.p51PriceList c = new BO.p51PriceList();
                if (v.rec_pid > 0) c = Factory.p51PriceListBL.Load(v.rec_pid);

                c.p51TypeFlag = v.Rec.p51TypeFlag;
                c.p51Name = v.Rec.p51Name;
                c.p51IsCustomTailor = v.Rec.p51IsCustomTailor;
                if (c.p51IsCustomTailor && !string.IsNullOrEmpty(v.TempGuid))
                {
                    c.p51Name = v.TempGuid;
                }
                c.j27ID = v.Rec.j27ID;
                c.p51DefaultRateT = v.Rec.p51DefaultRateT;
                c.p51IsFPR = v.Rec.p51IsFPR;

                if (v.d2 == null)
                {
                    v.d2 = new DateTime(3000, 1, 1);
                }
                if (v.d1 == null)
                {
                    v.d1 = new DateTime(DateTime.Today.Year, 1, 1);
                }

                var lis = new List<BO.p52PriceList_Item>();
                int x = 0;
                foreach(var row in v.lisP52.Where(p => p.IsTempDeleted==false))
                {
                    x += 1;
                    string msg = "Row index #" + x.ToString() + ": ";
                    var cc = new BO.p52PriceList_Item() { p52Rate = row.p52Rate };
                    if (row.RowPrefixWho == "j02")
                    {
                        cc.j02ID = row.j02ID;
                        if (cc.j02ID == 0)
                        {
                            this.AddMessageTranslated(msg+Factory.tra("Chybí vyplnit osobu."));return View(v);
                        }
                    }
                    if (row.RowPrefixWho == "j07")
                    {
                        cc.j07ID = row.j07ID;
                        if (cc.j07ID == 0)
                        {
                            this.AddMessageTranslated(msg + Factory.tra("Chybí vyplnit pozici.")); return View(v);
                        }
                    }
                    if (row.RowPrefixActivity == "p32")
                    {
                        cc.p32ID = row.p32ID;
                        if (cc.p32ID == 0)
                        {
                            this.AddMessageTranslated(msg+Factory.tra("Chybí vyplnit aktivitu.")); return View(v);
                        }
                        cc.p34ID = Factory.p32ActivityBL.Load(cc.p32ID).p34ID;
                    }
                    if (row.RowPrefixActivity == "p34")
                    {
                        cc.p34ID = row.p34ID;
                        if (cc.p34ID == 0)
                        {
                            this.AddMessageTranslated(msg+Factory.tra("Chybí vyplnit sešit.")); return View(v);
                        }
                    }
                    if (row.RowPrefixActivity == "all")
                    {
                        cc.p52IsPlusAllTimeSheets = true;
                        cc.p34ID = Factory.p34ActivityGroupBL.GetList(new BO.myQueryP34() { ismoneyinput = false }).First().pid;
                    }
                    
                    lis.Add(cc);
                }
                c.pid = Factory.p51PriceListBL.Save(c, lis, Convert.ToDateTime(v.d1), Convert.ToDateTime(v.d2));

                if (v.rec_pid>0 && c.pid > 0 && (v.Rec.p51TypeFlag==BO.p51TypeFlagENUM.BillingRates || v.Rec.p51TypeFlag == BO.p51TypeFlagENUM.RootBillingRates))
                {
                    var lisP31 = Factory.p31WorksheetBL.GetList(new BO.myQueryP31() { p51id = c.pid, iswip = true });
                    if (lisP31.Count() > 0)
                    {
                        //přesměrovat na přepočet sazeb
                        return RedirectToAction("Recalc", new { p51id = c.pid, isafter_p51record = true });
                    }
                    
                }
                if (c.pid > 0)
                {
                    v.SetJavascript_CallOnLoad(c.pid);
                    return View(v);
                }

            }


            this.Notify_RecNotSaved();
            return View(v);
        }
    }
}
