namespace UI.Menu
{
    public class p84ContextMenu: BaseContextMenu
    {
        public p84ContextMenu(BL.Factory f, int pid, string source, string device) : base(f, pid)
        {
            this.device = device;
            var rec = f.p84UpominkaBL.Load(pid);

            var recP91 = f.p91InvoiceBL.Load(rec.p91ID);            
            var disp = f.p91InvoiceBL.InhaleRecDisposition(rec.p91ID, recP91);

            if (source != "recpage")
            {
                AMI_RecPage("Stránka upomínky", "p84", pid, null, null, rec.p84Code);
            }
            if (source != "grid")
            {
                AMI_RecGrid("Přejít do Tabulky", "p84", pid);
            }


            if (disp.OwnerAccess)
            {
                DIV();
                AMI("Upravit", $"javascript:_edit('p84',{pid})", "edit_note");

                AMI("Odstranit záznam", $"javascript:_delete('p84',{pid})", "delete_forever");


            }


            DIV();

            var recP83 = f.p83UpominkaTypeBL.Load(rec.p83ID);


            AMI_Report("p91", pid);

            if (rec.p84Index==1 && recP83.x31ID_Index1 > 0)
            {
                AMI("Sestava upomínky", $"javascript: _window_open('/ReportsClient/ReportContext?pid={recP91.pid}&prefix=p91&x31id={recP83.x31ID_Index1}')", "print","ami_report");
            }
            if (rec.p84Index == 2 && recP83.x31ID_Index2 > 0)
            {
                AMI("Sestava upomínky", $"javascript: _window_open('/ReportsClient/ReportContext?pid={recP91.pid}&prefix=p91&x31id={recP83.x31ID_Index2}')", "print", "ami_report");
            }
            if (rec.p84Index == 3 && recP83.x31ID_Index3 > 0)
            {
                AMI("Sestava upomínky", $"javascript: _window_open('/ReportsClient/ReportContext?pid={recP91.pid}&prefix=p91&x31id={recP83.x31ID_Index3}')", "print", "ami_report");
            }

            var recP92 = f.p92InvoiceTypeBL.Load(recP91.p92ID);            
            if (recP92.x31ID_Invoice > 0)
            {
                AMI("Sestava faktury", $"javascript: _window_open('/ReportsClient/ReportContext?pid={recP91.pid}&prefix=p91&x31id={recP92.x31ID_Invoice}')", "print", "ami_report");
            }
            if (recP92.x31ID_Attachment > 0)
            {
                AMI("Sestava přílohy faktury", $"javascript: _window_open('/ReportsClient/ReportContext?pid={recP91.pid}&prefix=p91&x31id={recP92.x31ID_Attachment}')", "print", "ami_report");
            }
            int j61id = _f.p84UpominkaBL.NajdiVychoziJ61ID(recP83, rec);
            string strDefJ61Name = null;
            if (j61id > 0)
            {
                strDefJ61Name = _f.j61TextTemplateBL.Load(j61id).j61Name;
            }
            AMI_SendMail("p84", pid, null, strDefJ61Name);

            AMI_Workflow("p84", pid, 0, 0, null);

            

            

            AMI("Vazby", null, null, null, "bind");

            AMI_RecPage(recP91.p92Name+": "+rec.p91Code, "p91", rec.p91ID, "bind");
            if (recP91.p28ID > 0)
            {
                AMI_RecPage(_f.tra("Klient") + ": " + recP91.p91Client, "p28", recP91.p28ID, "bind", "contacts");
            }

        }
    }
}
