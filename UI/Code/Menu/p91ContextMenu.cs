

namespace UI.Menu
{
    public class p91ContextMenu : BaseContextMenu
    {
        public p91ContextMenu(BL.Factory f, int pid, string source,string device) : base(f, pid)
        {
            this.device = device;
            var rec = f.p91InvoiceBL.Load(pid);
            
            var disp = f.p91InvoiceBL.InhaleRecDisposition(rec.pid, rec);
            if (!disp.ReadAccess)
            {
                AMI_NoDisposition(); return;  //bez oprávnění                
            }

            if (source != "recpage")
            {
                //HEADER(rec.p92Name + ": " + rec.p91Code);
                AMI_RecPage("Stránka", "p91", pid,null,null,$"{rec.p91Code}");
            }
            if (source != "grid")
            {
                AMI_RecGrid("Přejít do Tabulky", "p91", pid);
            }

            

            bool bolHasDeletedP31IDs = f.p91InvoiceBL.HasDeletedP31Records(pid);

            if (bolHasDeletedP31IDs)
            {
                DIV("Úkony v archivu");                
            }
            else
            {
                DIV();
                if (disp.OwnerAccess)
                {
                    if (rec.p92TypeFlag == BO.p92TypeFlagENUM.CreditNote)
                    {
                        AMI("Upravit kartu dokladu", $"javascript:_edit('p91',{pid})", "edit_note");
                    }
                    else
                    {
                        AMI("Upravit kartu vyúčtování", $"javascript:_edit('p91',{pid})", "edit_note");
                    }
                    AMI_Clone("p91", pid);

                    AMI("Odstranit", $"javascript: _window_open('/p91oper/p91delete?p91id={pid}')", "delete_forever");
                    DIV();
                }
                if (rec.p92TypeFlag == BO.p92TypeFlagENUM.ClientInvoice)
                {
                    if (rec.p91Amount_Billed > 0)
                    {
                        AMI("Zapsat/Odstranit úhradu", $"javascript: _window_open('/p91oper/p94?p91id={pid}')", "payments");
                    }
                    else
                    {
                        AMI("Zapsat úhradu", $"javascript: _window_open('/p91oper/p94?p91id={pid}')", "payments");
                    }
                }
                
                if (!rec.isclosed && _f.CurrentUser.j04IsModule_p31 && rec.p41ID_First > 0)
                {
                    AMI_Vykazat(f.p41ProjectBL.Load(rec.p41ID_First), 0, rec.pid);
                }
            }


            var recP92 = f.p92InvoiceTypeBL.Load(rec.p92ID);

            AMI_Report("p91", pid, null);
            int j61id = _f.p91InvoiceBL.NajdiVychoziJ61ID(recP92, rec, _f.p28ContactBL.Load(rec.p28ID)) ;
            string strDefJ61Name = null;
            if (j61id > 0)
            {
                strDefJ61Name = _f.j61TextTemplateBL.Load(j61id).j61Name;
            }
            AMI_SendMail("p91", pid, null, strDefJ61Name);

            if (recP92.x31ID_Invoice > 0)
            {
                AMI("Sestava dokladu", $"javascript: _window_open('/ReportsClient/ReportContext?pid={pid}&prefix=p91&x31id={recP92.x31ID_Invoice}')", "print","ami_report");
            }
            if (recP92.x31ID_Attachment > 0)
            {
                AMI("Sestava přílohy", $"javascript: _window_open('/ReportsClient/ReportContext?pid={pid}&prefix=p91&x31id={recP92.x31ID_Attachment}')", "print", "ami_report");
            }
            if (recP92.x31ID_Invoice > 0 && recP92.x31ID_Attachment>0)
            {
                AMI("PDF merge: Doklad+Příloha", $"javascript: _window_open('/ReportsClient/ReportContext?pid={pid}&prefix=p91&x31id={recP92.x31ID_Invoice}&x31id_direct_merge1={recP92.x31ID_Attachment}')", "picture_as_pdf", "ami_report");
            }
            if (rec.p92TypeFlag == BO.p92TypeFlagENUM.CreditNote)
            {
                if (rec.p91ID_CreditNoteBind > 0)
                {
                    var recOD = f.p91InvoiceBL.Load(rec.p91ID_CreditNoteBind);
                    recP92 = f.p92InvoiceTypeBL.Load(recOD.p92ID);
                    AMI("Sestava opravovaného dokladu", $"javascript: _window_open('/ReportsClient/ReportContext?pid={recOD.pid}&prefix=p91&x31id={recP92.x31ID_Invoice}')", "print", "ami_report");
                }
            }
            else
            {
                var recOD = f.p91InvoiceBL.LoadCreditNote(rec.pid);
                if (recOD != null)
                {
                    recP92 = f.p92InvoiceTypeBL.Load(recOD.p92ID);
                    AMI("Sestava opravného dokladu", $"javascript: _window_open('/ReportsClient/ReportContext?pid={recOD.pid}&prefix=p91&x31id={recP92.x31ID_Invoice}')", "print", "ami_report");
                }
            }
            AMI_Doc("p91", pid);

            DIV();
            AMI("Další", null, null, null, "more");
            if (disp.OwnerAccess)
            {
                AMI_ChangeLog("p91", pid, "more");
            }
            

            if (rec.b01ID > 0)
            {
                AMI_Workflow("p91", pid, rec.b02ID, rec.b01ID);
            }
            else
            {
                AMI_Workflow("p91", pid, rec.b02ID, rec.b01ID,"more");
            }
            
            if (!bolHasDeletedP31IDs)
            {
                if (rec.p92TypeFlag == BO.p92TypeFlagENUM.ClientInvoice)
                {
                    AMI("Spárovat fakturu s úhradou zálohy", $"javascript: _window_open('/p91oper/proforma?p91id={pid}')", "receipt", "more");
                    AMI("Vytvořit k faktuře opravný doklad", $"javascript: _window_open('/p91oper/creditnote?p91id={pid}')", "money_off", "more");
                }
                if (rec.p92TypeFlag==BO.p92TypeFlagENUM.ClientInvoice && !BO.Code.Bas.bit_compare_or(rec.p91LockFlag, 2))
                {                    
                    AMI("Převést vyúčtování na jinou měnu", $"javascript: _window_open('/p91oper/j27?p91id={pid}')", "currency_yen", "more");

                    var lisP31 = f.p31WorksheetBL.GetList(new BO.myQueryP31() { p91id = pid });
                    if (rec.p91ExchangeRate != 1 || lisP31.Any(p => p.p31ExchangeRate_Invoice != 1))
                    {
                        AMI("Aktualizovat měnový kurz", $"javascript: _window_open('/p91oper/exupdate?p91id={pid}')", "monetization_on", "more");
                    }
                   
                    AMI("Převést vyúčtování na jinou DPH sazbu", $"javascript: _window_open('/p91oper/vat?p91id={pid}')", "percent", "more");
                }
                if (!rec.p91IsDraft)
                {
                    //DIV(null, "more");
                    AMI("Export do účetního IS", $"javascript: _window_open('/p91export/Index?p91ids={pid}')", "/images/isdoc_icon.png", "more");
                    
                    if (rec.p91Amount_Debt > 1 && DateTime.Today>rec.p91DateMaturity)
                    {
                        var intStupenCeka = _f.p84UpominkaBL.GetFirstFreeUpominkaIndex(rec);
                        if (intStupenCeka > 0 && intStupenCeka<4)
                        {
                            AMI($"Vygenerovat upomínku #{intStupenCeka}", $"javascript: _window_open('/p84/Create?p91ids={pid}')", "gavel", "more");
                        }                                               
                    }
                }
                
                AMI("PDF archiv", $"javascript: _window_open('/p91oper/p96?p91id={pid}',3)", "folder_open", "more");
                AMI("SOUČTY", $"javascript:_window_open('/p31totals/Index?selected_entity=p91&selected_pids={rec.pid}&blank=1')", "functions", "more");
            }




            //DIV(null, "more");

            
            

            AMI_RowColor("p91", rec.pid);

            AMI("Vazby", null, null, null, "bind");
            if (rec.p28ID > 0)
            {
                AMI_RecPage(_f.tra("Klient") + ": " + rec.p28Name, "p28", rec.p28ID, "bind", "contacts");
            }
            if (rec.p41ID_First > 0)
            {
                var recP41 = _f.p41ProjectBL.Load(rec.p41ID_First);
                AMI_RecPage(recP41.TypePlusName, "p41", rec.p41ID_First, "bind");

            }
            if (rec.p91ID_CreditNoteBind > 0)
            {
                AMI_RecPage(_f.tra("Opravovaný doklad"), "p91", rec.p91ID_CreditNoteBind, "bind");
            }
            
        }

        
    }
}
