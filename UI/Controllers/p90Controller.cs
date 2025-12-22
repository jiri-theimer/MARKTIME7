using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UI.Models;
using UI.Models.Record;
using UI.Models.Tab1;


namespace UI.Controllers
{
    public class p90Controller : BaseController
    {
        public IActionResult Info(int pid)
        {
            return Tab1(pid, "info");
        }
        public IActionResult Tab1(int pid,string caller)
        {
            var v = new p90Tab1() { Factory = this.Factory, pid = pid,caller=caller };
            v.Rec = Factory.p90ProformaBL.Load(v.pid);
            if (v.Rec != null)
            {
                
                v.SetTagging();
                v.lisP82 = Factory.p82Proforma_PaymentBL.GetList(v.pid);
                v.lisP99 = Factory.p91InvoiceBL.GetList_p99(v.pid, 0, 0);
                v.SetFreeFields(0);
            }
            return View(v);
        }

        public IActionResult Dashboard(int pid)
        {
            var v = new p90Tab1() { Factory = this.Factory, pid = pid, caller = "Dashboard" };
            v.Rec = Factory.p90ProformaBL.Load(v.pid);
            if (v.Rec != null)
            {

                v.SetTagging();
                v.lisP82 = Factory.p82Proforma_PaymentBL.GetList(v.pid);
                v.lisP99 = Factory.p91InvoiceBL.GetList_p99(v.pid, 0, 0);
                v.SetFreeFields(0);
            }
            return View(v);
        }


        public IActionResult Record(int pid, bool isclone,int p28id)
        {
            var v = new p90Record() { rec_pid = pid, rec_entity = "p90", UploadGuid = BO.Code.Bas.GetGuid(), ComboOwner = Factory.CurrentUser.FullnameDesc };
            v.ComboJ27Code = Factory.FBL.LoadCurrencyByID(Factory.Lic.j27ID).j27Code;
            v.Rec = new BO.p90Proforma() { j27ID = Factory.Lic.j27ID };
            
            v.disp = new DispoziceViewModel();
            v.disp.InitItems("p90", Factory);

            if (v.rec_pid > 0)
            {
                v.Rec = Factory.p90ProformaBL.Load(v.rec_pid);
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }
                if (!InhalePermissions(v))
                {
                    return this.StopPage(true, "Nemáte oprávnění k editaci karty záznamu.");
                }
                v.RecP89 = Factory.p89ProformaTypeBL.Load(v.Rec.p89ID);

                v.SetTagging(Factory.o51TagBL.GetTagging("p90", v.rec_pid));

                v.ComboP89Name = v.Rec.p89Name;
                v.ComboJ27Code = v.Rec.j27Code;
                v.ComboP28Name = v.Rec.p28Name;
                v.ComboOwner = v.Rec.Owner;
                if (v.Rec.j19ID > 0)
                {                    
                    v.ComboJ19Name = Factory.FBL.LoadJ19(v.Rec.j19ID).j19Name;
                }

                InhaleDisp(v, v.Rec.p90BitStream);
                

               

            }
            else
            {
                if (!Factory.CurrentUser.TestPermission(BO.PermValEnum.GR_p90_Creator))
                {
                    if (Factory.p89ProformaTypeBL.GetList_ProformaCreate().Count() == 0)
                    {
                        return this.StopPage(true, "Nemáte oprávnění zakládat nové zálohy.");
                    }
                }

                v.Rec.p90Date = DateTime.Today;v.Rec.j02ID_Owner = Factory.CurrentUser.pid;v.ComboOwner = Factory.CurrentUser.FullnameDesc;
                var recLast = Factory.p90ProformaBL.LoadMyLastCreated();
                if (recLast != null)
                {
                    
                    v.RecP89 = Factory.p89ProformaTypeBL.Load(recLast.p89ID);
                    v.Rec.j27ID = recLast.j27ID;v.ComboJ27Code = recLast.j27Code;v.Rec.p89ID = recLast.p89ID;v.ComboP89Name = recLast.p89Name;v.Rec.p90VatRate = recLast.p90VatRate;

                    InhaleDisp(v, 0);
                    

                }
                if (v.Rec.p89ID == 0)
                {
                    var lisP89 = Factory.p89ProformaTypeBL.GetList(new BO.myQuery("p89"));
                    if (lisP89.Count() > 0)
                    {
                        v.Rec.p89ID = lisP89.First().pid;
                        v.ComboP89Name = lisP89.First().p89Name;
                        v.RecP89 = lisP89.First();
                    }
                }
                if (p28id > 0)
                {
                    v.Rec.p28ID = p28id;
                    v.ComboP28Name = Factory.p28ContactBL.Load(p28id).p28Name;
                }
            }

            RefreshStateRecord(v);
            v.Toolbar = new MyToolbarViewModel(v.Rec);
            if (isclone)
            {
                v.MakeClone();
                v.Rec.p90Code = null;
            }

            return View(v);
        }

        private void RefreshStateRecord(p90Record v)
        {
            if (v.Rec.p89ID>0 && v.RecP89 == null)
            {
                v.RecP89 = Factory.p89ProformaTypeBL.Load(v.Rec.p89ID);
            }
            if (!v.disp.IsInhaled)
            {
                InhaleDisp(v, 0);                
            }
            

            InhaleRoles(v);
            

            

            if (v.ff1 == null)
            {
                v.ff1 = new FreeFieldsViewModel();
                v.ff1.InhaleFreeFieldsView(Factory, v.rec_pid, "p90");
            }
            v.ff1.RefreshInputsVisibility(Factory, v.rec_pid, "p90", v.Rec.p89ID);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(p90Record v)
        {
            RefreshStateRecord(v);

            if (v.IsPostback)
            {
                if (v.PostbackOper == "p89id")
                {
                    if (v.Rec.p89ID > 0)
                    {
                        v.RecP89 = Factory.p89ProformaTypeBL.Load(v.Rec.p89ID);
                        InhaleDisp(v, v.disp.GetBitStream());
                        if (v.rec_pid == 0) v.disp.RecoveryDefaultCheckedStates();
                        InhaleRoles(v);
                        
                    }
                    

                }
                if (v.PostbackOper == "recalc1" && v.Rec.p90VatRate > 0)  //dopočítat z částky bez DPH
                {
                    v.Rec.p90Amount_Vat = Math.Round(v.Rec.p90Amount_WithoutVat * v.Rec.p90VatRate / 100, 2);
                    v.Rec.p90Amount = Math.Round(v.Rec.p90Amount_WithoutVat + v.Rec.p90Amount_Vat, 2);
                }
                if (v.PostbackOper == "recalc2" && v.Rec.p90VatRate > 0)  //dopočítat z celkové částky
                {
                    v.Rec.p90Amount_WithoutVat = Math.Round(v.Rec.p90Amount / (1 + v.Rec.p90VatRate / 100), 2);
                    v.Rec.p90Amount_Vat = Math.Round(v.Rec.p90Amount - v.Rec.p90Amount_WithoutVat, 2);
                }

                return View(v);
            }
            
            
            if (ModelState.IsValid)
            {
                BO.p90Proforma c = new BO.p90Proforma();
                if (v.rec_pid > 0) c = Factory.p90ProformaBL.Load(v.rec_pid);
                c.p28ID = v.Rec.p28ID;
                c.j27ID = v.Rec.j27ID;
                c.p89ID = v.Rec.p89ID;
                c.p90Date = v.Rec.p90Date;
                c.p90DateMaturity = v.Rec.p90DateMaturity;
                c.j19ID = v.Rec.j19ID;

                c.p90Amount = v.Rec.p90Amount;
                c.p90Amount_WithoutVat = v.Rec.p90Amount_WithoutVat;
                c.p90Amount_Vat = v.Rec.p90Amount_Vat;
                c.p90VatRate = v.Rec.p90VatRate;
                
                c.p90Text1 = v.Rec.p90Text1;
                c.p90Text2 = v.Rec.p90Text2;

                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);
                c.j02ID_Owner = v.Rec.j02ID_Owner;

                c.p90BitStream = v.disp.GetBitStream();

               

                c.pid = Factory.p90ProformaBL.Save(c, v.ff1.inputs,v.disp.IsRoles? v.roles.getList4Save(Factory): null);
                if (c.pid > 0)
                {
                    Factory.o51TagBL.SaveTagging("p90", c.pid, v.TagPids);
                    
                    if (v.disp.IsFiles)
                    {
                        Factory.o27AttachmentBL.SaveChangesAndUpload(v.UploadGuid, "p90", c.pid);
                    }
                    


                    v.SetJavascript_CallOnLoad(c.pid);
                    return View(v);
                }

            }


            this.Notify_RecNotSaved();
            return View(v);
        }


        private bool InhalePermissions(p90Record v)
        {
            var mydisp = Factory.p90ProformaBL.InhaleRecDisposition(v.Rec.pid,v.Rec);
            if (!mydisp.OwnerAccess)
            {
                return false;
            }
            if (v.Rec.x38ID > 0)
            {
                v.CanEditRecordCode = Factory.x38CodeLogicBL.CanEditRecordCode(v.Rec.x38ID, mydisp);
            }
            else
            {
                v.CanEditRecordCode = mydisp.OwnerAccess;
            }
            return true;
        }


        private void InhaleDisp(p90Record v, int bitstream)
        {

            if (v.RecP89 == null) return;
            //int intCache = (v.rec_pid == 0 ? Factory.j02UserBL.LoadBitstreamFromUserCache( "p90", v.RecP89.pid) : 0);    //pro nový záznam načíst uložená rozšíření z cache
            int intCache = 0;   //cache nepoužívat
            
            v.disp.SetVal(PosEnum.Files, v.RecP89.p89FilesTab, bitstream, v.rec_pid, intCache);            
            v.disp.SetVal(PosEnum.Roles, v.RecP89.p89RolesTab, bitstream, v.rec_pid, intCache);

        }

        private void InhaleRoles(p90Record v)
        {
            if (v.disp.IsRoles && v.roles == null)
            {
                v.roles = new RoleAssignViewModel() { RecPid = v.rec_pid, RecPrefix = "p90", RolePrefix = "p90" };
            }

        }
        
    }
}
