using UI.Menu;

namespace UI.Code.Menu
{
    public class p49ContextMenu:BaseContextMenu
    {
        public p49ContextMenu(BL.Factory f, int pid, string source, string device) : base(f, pid)
        {
            this.device = device;
            var rec = f.p49FinancialPlanBL.Load(pid);

            var recP41 = f.p41ProjectBL.Load(rec.p41ID);
            var mydisp = f.p41ProjectBL.InhaleRecDisposition(recP41.pid, recP41);

            
            if (source != "recpage")
            {
                AMI_RecPage("Stránka", "p49", pid,null,null,rec.p49Code);
            }
            if (source != "grid")
            {
                AMI_RecGrid("Přejít do Tabulky", "p49", pid);
            }

            
            if (mydisp.OwnerAccess)
            {
                DIV();
                AMI("Upravit", $"javascript:_edit('p49',{pid})", "edit_note");
                AMI_Clone("p49", pid);

                if (_f.CurrentUser.IsAdmin)
                {                    
                    AMI("Odstranit záznam", $"javascript:_delete('p49',{pid})", "delete_forever");
                }

               
            }


            DIV();
            
            AMI("Realizovat", $"javascript: _window_open('/p31/Record?pid=0&p49id={pid}', 3)", "flight_takeoff");

            AMI_RecPage(recP41.TypePlusName, "p41", recP41.pid, "bind");
            if (recP41.p28ID_Client > 0)
            {
                AMI_RecPage(recP41.Client, "p28", recP41.p28ID_Client, "bind");
            }
            if (rec.p56ID > 0)
            {                
                AMI_RecPage(rec.p56Name, "p56", rec.p56ID, "bind");
            }
            if (rec.p28ID_Supplier > 0)
            {
                var recSupplier = f.p28ContactBL.Load(rec.p28ID_Supplier);
                AMI_RecPage(recSupplier.p28Name, "p28", rec.p28ID_Supplier, "bind");
            }
            AMI("Vazby", null, null, null, "bind");




        }
    }
}
