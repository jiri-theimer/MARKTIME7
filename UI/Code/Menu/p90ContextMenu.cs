


namespace UI.Menu
{
    public class p90ContextMenu: BaseContextMenu
    {
        public p90ContextMenu(BL.Factory f, int pid, string source,string device) : base(f, pid)
        {
            this.device = device;
            var rec = f.p90ProformaBL.Load(pid);
            var disp = f.p90ProformaBL.InhaleRecDisposition(rec.pid,rec);
            if (!disp.ReadAccess)
            {
                AMI_NoDisposition(); return;  //bez oprávnění                
            }
            var recP89 = _f.p89ProformaTypeBL.Load(rec.p89ID);

            if (source != "recpage")
            {
                //HEADER(rec.p89Name + ": " + rec.p90Code);
                AMI_RecPage("Stránka", "p90", pid,null,null, rec.p90Code);
            }
            if (source != "grid")
            {
                AMI_RecGrid("Přejít do Tabulky", "p90", pid);
            }

            DIV();
            if (disp.OwnerAccess)
            {
                AMI("Upravit kartu zálohy", $"javascript:_edit('p90',{pid})", "edit_note");
                AMI_Clone("p90", pid);
                AMI("Odstranit", $"javascript:_delete('p90',{pid})", "delete_forever");
                DIV();
            }
            AMI("Zapsat úhradu zálohy", $"javascript: _window_open('/p82/Record?pid=0&p90id={pid}',3)", "payments",null,"menuUhrady");

            var lisP82 = _f.p82Proforma_PaymentBL.GetList(pid);
            if (lisP82.Count() > 0)
            {
                
                foreach(var c in lisP82)
                {
                    
                    AMI($"Úhrada {BO.Code.Bas.Number2String(c.p82Amount)}", $"javascript: _window_open('/ReportsClient/ReportContext?pid={c.pid}&prefix=p82&x31id={recP89.x31ID_Payment}')", "print", "menuUhrady");
                }
            }


            AMI_Report("p90", pid);

            if (recP89.j61ID > 0)
            {
                AMI_SendMail("p90", pid,null, _f.j61TextTemplateBL.Load(recP89.j61ID).j61Name);
            }
            else
            {
                AMI_SendMail("p90", pid, null);
            }
            
          
            
            AMI_Doc("p90", pid);


            DIV();
            AMI("Další", null, null, null, "more");

            if (!rec.p90IsDraft)
            {
                //DIV(null, "more");
                AMI("Export do účetního IS", $"javascript: _window_open('/p90export/Index?p90ids={pid}')", "cloud_upload", "more");
            }

            AMI_Workflow("p90", pid, 0, 0,"more");

            
            

            AMI("Vazby", null, null, null, "bind");
            if (rec.p28ID > 0)
            {
                AMI_RecPage(_f.tra("Klient") + ": " + rec.p28Name, "p28", rec.p28ID, "bind");
            }

        }
    }
}
