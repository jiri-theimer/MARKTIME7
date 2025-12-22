using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UI.Models.p31invoice;
using UI.Models;
using DocumentFormat.OpenXml.Bibliography;

namespace UI.Controllers.p31oper
{
    public class p31invoiceController : BaseController
    {
        public IActionResult Append2Invoice(string pids, string guid_pids)
        {
            if (!string.IsNullOrEmpty(guid_pids))
            {
                pids = Factory.p85TempboxBL.LoadByGuid(guid_pids).p85Message; //vstupní pids předány přes p85Tempbox
            }
            if (string.IsNullOrEmpty(pids) || BO.Code.Bas.ConvertString2ListInt(pids).Count() == 0)
            {
                return StopPage(true, "pids missing.");
            }
            var v = new Append2InvoiceViewModel() { pids = pids };

            RefreshState_Append2Invoice(v);
            if (v.lisP31.Count() == 0)
            {
                return StopPage(true, "Na vstupu chybí minimálně jeden nevyúčtovaný úkon.");
            }

            if (v.SelectedInvoiceP91ID == 0)
            {
                var mq = new BO.myQueryP91();
                mq.p41id = v.lisP31.First().p41ID;
                var lisP91 = Factory.p91InvoiceBL.GetList(mq);
                if (lisP91.Count() == 0)
                {
                    if (v.lisP31.First().p28ID_Client > 0)
                    {
                        mq = new BO.myQueryP91() { p28id = v.lisP31.First().p28ID_Client };
                        lisP91 = Factory.p91InvoiceBL.GetList(mq);
                    }
                }
                if (lisP91.Count() > 0)
                {
                    v.SelectedInvoiceP91ID = lisP91.First().pid;
                    v.SelectedInvoiceText = lisP91.First().p91Code;
                }                
            }

            return View(v);
        }

        private void RefreshState_Append2Invoice(Append2InvoiceViewModel v)
        {
            var mq = new BO.myQueryP31() { pids = BO.Code.Bas.ConvertString2ListInt(v.pids) };
            mq.isinvoiced = false;

            v.lisP31 = Factory.p31WorksheetBL.GetList(mq);

            if (v.SelectedInvoiceP91ID > 0)
            {
                v.Rec = Factory.p91InvoiceBL.Load(v.SelectedInvoiceP91ID);
            }
        }

        [HttpPost]
        public IActionResult Append2Invoice(Append2InvoiceViewModel v, string oper)
        {
            RefreshState_Append2Invoice(v);
            if (v.IsPostback)
            {
                return View(v);
            }
            if (v.SelectedInvoiceP91ID == 0)
            {
                AddMessage("Chybí cílové vyúčtování (faktura).");
                return View(v);
            }

            return RedirectToAction("Index", "p31approveinput", new { v.pids, prefix = "p31", p91id = v.SelectedInvoiceP91ID });
        }


        public IActionResult Index(string tempguid)
        {
            if (string.IsNullOrEmpty(tempguid))
            {
                return StopPage(true, "guid missing.");
            }
            var v = new GatewayViewModel() { tempguid = tempguid, p91Date = DateTime.Today, p91DateSupply = DateTime.Today, BillingScale = 0, IsDraft = true };

            v.ShowAfterFinish = Factory.CBL.LoadUserParam("p31invoice-ShowAfterFinish","recpage");
            v.IsRememberDates = Factory.CBL.LoadUserParamBool("p31invoice-isremember", false);
            v.IsRememberMaturity = Factory.CBL.LoadUserParamBool("p31invoice-isremembermaturity", false);
            v.IsRememberInvoiceText = Factory.CBL.LoadUserParamBool("p31invoice-isrememberinvoicetext", false);
            v.IsRememberP92ID = Factory.CBL.LoadUserParamBool("p31invoice-isrememberp92id", false);
            v.IsRememberDohromady = Factory.CBL.LoadUserParamBool("p31invoice-isrememberdohromady", false);
            if (v.IsRememberDohromady)
            {
                v.BillingScale = Factory.CBL.LoadUserParamInt("p31invoice-BillingScale", 0);
                
            }
            if (v.IsRememberDates || v.IsRememberMaturity || v.IsRememberInvoiceText || v.IsRememberP92ID)
            {
                v.p91ID_LastRemember = Factory.CBL.LoadUserParamInt("p31invoice-remember-p91id");
                if (v.p91ID_LastRemember > 0)
                {
                    v.RecLastRemember = Factory.p91InvoiceBL.Load(v.p91ID_LastRemember);
                    if (v.RecLastRemember != null)
                    {
                        v.p91Date = v.RecLastRemember.p91Date; v.p91DateSupply = v.RecLastRemember.p91DateSupply; v.p91Datep31_From = v.RecLastRemember.p91Datep31_From; v.p91Datep31_Until = v.RecLastRemember.p91Datep31_Until;
                    }

                }
            }
            v.IsRememberIsDraft = Factory.CBL.LoadUserParamBool("p31invoice-isrememberdraft", false);
            v.IsDraft = Factory.CBL.LoadUserParamBool("p31invoice-isdraft", true);


            RefreshState_Index(v);



            return View(v);
        }

        private void RefreshState_Index(GatewayViewModel v)
        {
            DateTime datMaturityDef = DateTime.Today.AddDays(Factory.Lic.x01InvoiceMaturityDays);

            var mq = new BO.myQueryP31() { tempguid = v.tempguid };
            v.lisP31 = Factory.p31WorksheetBL.GetList(mq).OrderBy(p => p.Project);


            if (v.lisP31.Count() > 0)
            {
                if (v.p91Datep31_From == null) v.p91Datep31_From = v.lisP31.Min(p => p.p31Date);
                if (v.p91Datep31_Until == null) v.p91Datep31_Until = v.lisP31.Max(p => p.p31Date);

                List<int> p41ids = v.lisP31.Select(p => p.p41ID).ToList();
                List<int> p28ids = v.lisP31.Where(p => p.p28ID_Client > 0).Select(p => p.p28ID_Client).ToList();
                v.lisFakturacniPoznamky = Factory.WorkflowBL.GetList_b05_p41_p28(p41ids, p28ids).Where(p => BO.Code.Bas.bit_compare_or(p.b05Tab1Flag, 4) == true);   //fakturační poznámky
            }
            

            if (v.BillingScale == 0)
            {
                if (v.lisP31.GroupBy(p => p.p28ID_Client).Count() > 1)
                {
                    v.BillingScale = 2;
                }
                else
                {
                    v.BillingScale = 1;
                }
            }
            if (v.BillingScale == 1 && v.lisP91_Scale1 == null)
            {
                v.lisP91_Scale1 = new List<p91CreateItem>();

                var cc = new p91CreateItem() { DateMaturity = datMaturityDef, TempGUID = BO.Code.Bas.GetGuid() };
                if (v.p91DateSupply != null)
                {
                    cc.DateSupply = v.p91DateSupply.Value;
                }
                if (v.lisP31.Where(p => p.p28ID_Client > 0).Count() > 0)
                {
                    InhaleClientSetting(v, cc, v.lisP31.Where(p => p.p28ID_Client > 0).First().p28ID_Client);
                }
                foreach (var dbl in v.lisP31.Where(p => p.p31Amount_WithoutVat_Approved != 0).GroupBy(p => p.j27ID_Billing_Orig))
                {
                    cc.WithoutVat += " " + BO.Code.Bas.Number2String(dbl.Sum(p => p.p31Amount_WithoutVat_Approved)) + dbl.First().j27Code_Billing_Orig;
                }
                cc.p41ID = v.lisP31.First().p41ID; cc.p41Name = v.lisP31.First().p41Name;
                if (cc.p41ID > 0)
                {
                    InhaleProjectSetting(v, cc, cc.p41ID);
                }

                v.lisP91_Scale1.Add(cc);
            }
            if (v.BillingScale == 2 && v.lisP91_Scale2 == null)
            {
                v.lisP91_Scale2 = new List<p91CreateItem>();
                var lis = v.lisP31.GroupBy(p => p.p28ID_Client);
                foreach (var c in lis)
                {
                    var cc = new p91CreateItem() { DateMaturity = datMaturityDef, TempGUID = BO.Code.Bas.GetGuid() };
                    if (v.p91DateSupply != null)
                    {
                        cc.DateSupply = v.p91DateSupply.Value;
                    }
                    if (c.First().p28ID_Client > 0)
                    {
                        InhaleClientSetting(v, cc, c.First().p28ID_Client);
                    }
                    foreach (var dbl in c.Where(p => p.p31Amount_WithoutVat_Approved != 0).GroupBy(p => p.j27ID_Billing_Orig))
                    {
                        cc.WithoutVat += " " + BO.Code.Bas.Number2String(dbl.Sum(p => p.p31Amount_WithoutVat_Approved)) + dbl.First().j27Code_Billing_Orig;
                    }
                    cc.p41ID = c.First().p41ID; cc.p41Name = c.First().p41Name;
                    if (cc.p41ID > 0)
                    {
                        InhaleProjectSetting(v, cc, cc.p41ID);
                    }

                    v.lisP91_Scale2.Add(cc);
                }
            }
            if (v.BillingScale == 3 && v.lisP91_Scale3 == null)
            {
                v.lisP91_Scale3 = new List<p91CreateItem>();
                var lis = v.lisP31.GroupBy(p => p.p41ID);
                foreach (var c in lis)
                {
                    var cc = new p91CreateItem() { DateMaturity = datMaturityDef, p41ID = c.First().p41ID, p41Name = c.First().p41Name, TempGUID = BO.Code.Bas.GetGuid() };
                    if (v.p91DateSupply != null)
                    {
                        cc.DateSupply = v.p91DateSupply.Value;
                    }
                    cc.p28ID = c.First().p28ID_Client;
                    if (cc.p28ID > 0)
                    {
                        InhaleClientSetting(v, cc, cc.p28ID);

                    }
                    foreach (var dbl in c.Where(p => p.p31Amount_WithoutVat_Approved != 0).GroupBy(p => p.j27ID_Billing_Orig))
                    {
                        cc.WithoutVat += " " + BO.Code.Bas.Number2String(dbl.Sum(p => p.p31Amount_WithoutVat_Approved)) + dbl.First().j27Code_Billing_Orig;
                    }
                    cc.p41ID = c.First().p41ID; cc.p41Name = c.First().p41Name;
                    if (cc.p41ID > 0)
                    {
                        InhaleProjectSetting(v, cc, cc.p41ID);
                    }
                    v.lisP91_Scale3.Add(cc);
                }
            }
        }

        private void Handle_SaveRemember(GatewayViewModel v, int p91id)
        {
            Factory.CBL.SetUserParam("p31invoice-isremember", BO.Code.Bas.GB(v.IsRememberDates));
            Factory.CBL.SetUserParam("p31invoice-isremembermaturity", BO.Code.Bas.GB(v.IsRememberMaturity));
            Factory.CBL.SetUserParam("p31invoice-isrememberinvoicetext", BO.Code.Bas.GB(v.IsRememberInvoiceText));
            Factory.CBL.SetUserParam("p31invoice-isdraft", BO.Code.Bas.GB(v.IsDraft));
            Factory.CBL.SetUserParam("p31invoice-isrememberdraft", BO.Code.Bas.GB(v.IsRememberIsDraft));
            Factory.CBL.SetUserParam("p31invoice-isrememberp92id", BO.Code.Bas.GB(v.IsRememberP92ID));
            Factory.CBL.SetUserParam("p31invoice-isrememberdohromady", BO.Code.Bas.GB(v.IsRememberDohromady));
            if (v.IsRememberDohromady)
            {
                Factory.CBL.SetUserParam("p31invoice-BillingScale", v.BillingScale.ToString());
            }
            if (v.IsRememberDates || v.IsRememberMaturity || v.IsRememberInvoiceText)
            {
                Factory.CBL.SetUserParam("p31invoice-remember-p91id", p91id.ToString());
            }
            else
            {
                Factory.CBL.SetUserParam("p31invoice-remember-p91id", null);
            }
        }

        [HttpPost]
        public IActionResult Index(GatewayViewModel v)
        {
            RefreshState_Index(v);

            if (v.IsPostback)
            {
                switch (v.PostbackOper)
                {
                    case "save":
                        var lisP91 = GetInvoiceItems(v);
                        if (lisP91.Count() == 0)
                        {
                            AddMessage("Na vstupu chybí položky vyúčtování.");
                            return View(v);
                        }

                        int intPID = Save(v);
                        if (intPID > 0)
                        {
                            Factory.CBL.SetUserParam("p31invoice-ShowAfterFinish", v.ShowAfterFinish);

                            Handle_SaveRemember(v, intPID);
                            switch (v.ShowAfterFinish)
                            {
                                case "grid":
                                    if (BL.Code.UserUI.IsShowLeftPanel("p91", Factory.CurrentUser.j02UIBitStream))
                                    {
                                        v.SetJavascript_CallOnLoad($"/Record/RecPage?prefix=p91&pid={intPID}");
                                    }
                                    else
                                    {
                                        if (BL.Code.UserUI.IsShowFlatView("p91", Factory.CurrentUser.j02UIBitStream))
                                        {
                                            v.SetJavascript_CallOnLoad($"/TheGrid/FlatView?prefix=p91&go2Pid={intPID}");
                                        }
                                        else
                                        {
                                            v.SetJavascript_CallOnLoad($"/TheGrid/MasterView?prefix=p91&go2pid={intPID}");
                                        }
                                    }                               
                                    break;
                                case "closeandrefresh":
                                    v.SetJavascript_CallOnLoad(1);
                                    break;
                                case "continue":
                                    return RedirectToAction("Index", "p31InvoiceFinish", new { p91id = intPID });
                                    
                                case "recpage":
                                default:
                                    v.SetJavascript_CallOnLoad($"/Record/RecPage?prefix=p91&pid={intPID}");
                                    break;
                            }

                            return View(v);

                            

                        }

                        break;
                    case "append2invoice":
                        if (v.SelectedInvoiceP91ID == 0)
                        {
                            AddMessage("Chybí cílové vyúčtování (faktura).");
                            return View(v);
                        }
                        var recP91 = Factory.p91InvoiceBL.Load(v.SelectedInvoiceP91ID);
                        var dispP91 = Factory.p91InvoiceBL.InhaleRecDisposition(recP91.pid, recP91);
                        if (!dispP91.OwnerAccess)
                        {
                            foreach (var rec in v.lisP31)
                            {
                                var dispP41 = Factory.p41ProjectBL.InhaleRecDisposition(rec.p41ID, null);
                                if (!dispP41.p91_DraftCreate)
                                {
                                    AddMessageTranslated(rec.Project + ": " + Factory.tra("V projektu nemáte oprávnění vytvářet vyúčtování."));
                                }
                                if (!recP91.p91IsDraft && rec.p71ID == BO.p71IdENUM.Schvaleno && rec.p72ID_AfterApprove == BO.p72IdENUM.Fakturovat)
                                {
                                    AddMessageTranslated(rec.Project + ": " + Factory.tra("S vaším oprávněním můžete do tohoto vyúčtování vkládat pouze úkony s nulovou fakturační cenou!"));
                                }
                            }
                        }
                        if (Factory.p31WorksheetBL.Append2Invoice(v.SelectedInvoiceP91ID, v.lisP31.Select(p => p.pid).ToList()))
                        {
                            v.SetJavascript_CallOnLoad(1);
                            return View(v);
                        }
                        return View(v);



                }
                return View(v);
            }



            return View(v);
        }

        private List<p91CreateItem> GetInvoiceItems(GatewayViewModel v)
        {
            var lis = new List<p91CreateItem>();
            switch (v.BillingScale)
            {
                case 1:
                    lis = v.lisP91_Scale1;
                    break;
                case 2:
                    lis = v.lisP91_Scale2;
                    break;
                case 3:
                    lis = v.lisP91_Scale3;
                    break;

            }

            return lis;
        }



        private p91CreateItem InhaleClientSetting(GatewayViewModel v, p91CreateItem ret, int p28id)
        {
            if (p28id == 0)
            {
                return ret;
            }

            var recP28 = Factory.p28ContactBL.Load(p28id);
            if (recP28 == null)
            {
                return ret;
            }

            ret.p28ID = recP28.pid;
            ret.p28Name = recP28.p28Name;
            ret.p28ID_Invoice = ret.p28ID;
            ret.p28Name_Invoice = ret.p28Name;

            if (recP28.p28InvoiceMaturityDays > 0)
            {
                ret.DateMaturity = DateTime.Today.AddDays(recP28.p28InvoiceMaturityDays);
            }
            if (recP28.p92ID > 0)
            {
                var recP92 = Factory.p92InvoiceTypeBL.Load(recP28.p92ID);
                ret.p92ID = recP92.pid;
                ret.p92Name = recP92.p92Name;
                ret.InvoiceText1 = recP92.p92InvoiceDefaultText1;
                ret.InvoiceText2 = recP92.p92InvoiceDefaultText2;
            }
            if (recP28.p28InvoiceDefaultText1 != null)
            {
                ret.InvoiceText1 = recP28.p28InvoiceDefaultText1;
            }
            if (recP28.p28InvoiceDefaultText2 != null)
            {
                ret.InvoiceText2 = recP28.p28InvoiceDefaultText2;
            }


            if (v.IsRememberInvoiceText && v.RecLastRemember != null && ret.InvoiceText1 == null)
            {
                ret.InvoiceText1 = v.RecLastRemember.p91Text1;
            }
            if (v.IsRememberInvoiceText && v.RecLastRemember != null && ret.InvoiceText2 == null)
            {
                ret.InvoiceText2 = v.RecLastRemember.p91Text2;
            }

            //if (ret.InvoiceText1 != null)
            //{
            //    if (v.p91Datep31_From != null && (ret.InvoiceText1.Contains("1]") || ret.InvoiceText1.Contains("2]")))
            //    {
            //        //text faktury se merguje podle p91Datep31_From a p91Datep31_Until
            //        ret.InvoiceText1 = Factory.CBL.MergeTextWithOneDate(ret.InvoiceText1, Convert.ToDateTime(v.p91Datep31_From), Convert.ToDateTime(v.p91Datep31_Until));
            //    }
            //    else
            //    {
            //        //text faktury se merguje podle DUZP
            //        ret.InvoiceText1 = Factory.CBL.MergeTextWithOneDate(ret.InvoiceText1, ret.DateSupply, ret.DateSupply);
            //    }
                    
            //}
            //if (ret.InvoiceText2 != null)
            //{
            //    if (v.p91Datep31_From != null && (ret.InvoiceText2.Contains("1]") || ret.InvoiceText2.Contains("2]")))
            //    {
            //        //tecnický text faktury se merguje podle p91Datep31_From a p91Datep31_Until
            //        ret.InvoiceText2 = Factory.CBL.MergeTextWithOneDate(ret.InvoiceText2, Convert.ToDateTime(v.p91Datep31_From), Convert.ToDateTime(v.p91Datep31_Until));
            //    }
            //    else
            //    {
            //        //technický text faktury se merguje podle DUZP
            //        ret.InvoiceText2 = Factory.CBL.MergeTextWithOneDate(ret.InvoiceText2, ret.DateSupply, ret.DateSupply);
            //    }
                    
            //}

            if (v.IsRememberMaturity && v.RecLastRemember != null)
            {
                ret.DateMaturity = v.RecLastRemember.p91DateMaturity;
            }

            if (v.IsRememberP92ID && v.RecLastRemember != null)
            {
                var recP92 = Factory.p92InvoiceTypeBL.Load(v.RecLastRemember.p92ID);
                ret.p92ID = recP92.pid;
                ret.p92Name = recP92.p92Name;
            }

            return ret;
        }

        private void InhaleProjectSetting(GatewayViewModel v, p91CreateItem ret, int p41id)
        {
            var recP41 = Factory.p41ProjectBL.Load(p41id);
            if (recP41.p28ID_Billing > 0)
            {
                ret.p28ID_Invoice = recP41.p28ID_Billing;
                ret.p28Name_Invoice = Factory.p28ContactBL.Load(recP41.p28ID_Billing).p28Name;
            }
            if (recP41.p41InvoiceMaturityDays > 0)
            {
                ret.DateMaturity = DateTime.Today.AddDays(recP41.p41InvoiceMaturityDays);
            }
            
            if (recP41.p92ID==0 && ret.p92ID == 0)
            {
                var lisP92 = Factory.p92InvoiceTypeBL.GetList(new BO.myQuery("p92"));
                if (v.lisP31.Count() > 0)
                {
                    lisP92 = lisP92.Where(p => p.j27ID == v.lisP31.First().j27ID_Billing_Orig);
                }
                if (lisP92.Count() > 0)
                {
                    recP41.p92ID = lisP92.First().pid;

                }
            }
            
            if (recP41.p92ID == 0 && ret.p92ID > 0)
            {
                recP41.p92ID = ret.p92ID;
            }
            
            if (recP41.p92ID > 0)
            {
                var recP92 = Factory.p92InvoiceTypeBL.Load(recP41.p92ID);
                ret.p92ID = recP92.pid;
                ret.p92Name = recP92.p92Name;
                if (recP92.p92InvoiceDefaultText1 != null)
                {
                    ret.InvoiceText1 = recP92.p92InvoiceDefaultText1;
                }
                if (recP92.p92InvoiceDefaultText2 != null)
                {
                    ret.InvoiceText2 = recP92.p92InvoiceDefaultText2;
                }

            }
            if (recP41.p41InvoiceDefaultText1 != null)
            {
                ret.InvoiceText1 = recP41.p41InvoiceDefaultText1;
            }
            if (recP41.p41InvoiceDefaultText2 != null)
            {
                ret.InvoiceText2 = recP41.p41InvoiceDefaultText2;
            }


            //if (ret.InvoiceText1 != null)
            //{
            //    if (v.p91Datep31_From != null && (ret.InvoiceText1.Contains("1]") || ret.InvoiceText1.Contains("2]")))
            //    {
            //        //text faktury se merguje podle p91Datep31_From a p91Datep31_Until
            //        ret.InvoiceText1 = Factory.CBL.MergeTextWithOneDate(ret.InvoiceText1, Convert.ToDateTime(v.p91Datep31_From), Convert.ToDateTime(v.p91Datep31_Until));
            //    }
            //    else
            //    {
            //        //text faktury se merguje podle DUZP
            //        ret.InvoiceText1 = Factory.CBL.MergeTextWithOneDate(ret.InvoiceText1, ret.DateSupply, ret.DateSupply);

            //    }

            //}
            //if (ret.InvoiceText2 != null)
            //{
            //    if (v.p91Datep31_From != null && (ret.InvoiceText2.Contains("1]") || ret.InvoiceText2.Contains("2]")))
            //    {
            //        //tecnický text faktury se merguje podle p91Datep31_From a p91Datep31_Until
            //        ret.InvoiceText2 = Factory.CBL.MergeTextWithOneDate(ret.InvoiceText2, Convert.ToDateTime(v.p91Datep31_From), Convert.ToDateTime(v.p91Datep31_Until));
            //    }
            //    else
            //    {
            //        //technický text faktury se merguje podle DUZP
            //        ret.InvoiceText2 = Factory.CBL.MergeTextWithOneDate(ret.InvoiceText2, ret.DateSupply, ret.DateSupply);
            //    }

            //}


        }


        private int Save(GatewayViewModel v)
        {
            int intRetP91ID = 0;
            var lis = GetInvoiceItems(v);

            if (v.p91Date == null || v.p91Datep31_From == null || v.p91Datep31_Until == null || v.p91DateSupply == null)
            {
                AddMessage("Datumy [Vystavení], [Plnění], [Začátek] a [Konec] jsou povinná k vyplnění.");
                return 0;
            }
            foreach (var rec in lis)
            {
                if (rec.p28ID_Invoice == 0)
                {
                    AddMessage("Ve vyúčtování chybí vazba na klienta.");
                    return 0;
                }
                if (rec.p92ID == 0)
                {
                    AddMessage("Ve vyúčtování chybí vazba na typ faktury.");
                    return 0;
                }
            }
            int intLastP28ID = 0;
            int intLastP41ID = 0;
            string strGUID = null; string strLastGUID = null;
            IEnumerable<BO.p85Tempbox> lisTemp = null;
            foreach (var c in v.lisP31.OrderBy(p => p.p28ID_Client).ThenBy(p => p.p41ID))
            {
                if (v.BillingScale == 1 && strGUID == null)
                {
                    strGUID = v.lisP91_Scale1.First().TempGUID;
                }
                if (v.BillingScale == 2 && c.p28ID_Client != intLastP28ID)
                {
                    strGUID = lis.Where(p => p.p28ID == c.p28ID_Client).First().TempGUID;
                }
                if (v.BillingScale == 3 && c.p41ID != intLastP41ID)
                {
                    strGUID = lis.Where(p => p.p41ID == c.p41ID).First().TempGUID;
                }
                if (strGUID != strLastGUID)
                {
                    lisTemp = Factory.p85TempboxBL.GetList(strGUID, false, "p31");
                }
                if (lisTemp.Where(p => p.p85DataPID == c.pid).Count() == 0)
                {
                    var rec = new BO.p85Tempbox() { p85GUID = strGUID, p85DataPID = c.pid, p85Prefix = "p31" };
                    Factory.p85TempboxBL.Save(rec);
                }

                intLastP28ID = c.p28ID_Client;
                intLastP41ID = c.p41ID;
                strLastGUID = strGUID;
            }

            var errs = new List<int>();

            foreach (var rec in lis)
            {
                var c = new BO.p91Create() { TempGUID = rec.TempGUID, DateIssue = Convert.ToDateTime(v.p91Date), DateSupply = Convert.ToDateTime(v.p91DateSupply), DateP31_From = Convert.ToDateTime(v.p91Datep31_From), DateP31_Until = Convert.ToDateTime(v.p91Datep31_Until) };
                c.p28ID = rec.p28ID_Invoice;

                c.p92ID = rec.p92ID;
                c.InvoiceText1 = rec.InvoiceText1;
                c.InvoiceText2 = rec.InvoiceText2;
                c.DateMaturity = rec.DateMaturity;
                c.IsDraft = v.IsDraft;
                int intPID = Factory.p91InvoiceBL.Create(c);
                if (intPID == 0)
                {
                    errs.Add(1);
                }
                else
                {
                    intRetP91ID = intPID;
                }
            }

            if (errs.Count() > 0)
            {
                AddMessage("Při generování vyúčtování došlo k chybám.");
                return 0;
            }
            else
            {
                return intRetP91ID;
            }
        }
    }
}
