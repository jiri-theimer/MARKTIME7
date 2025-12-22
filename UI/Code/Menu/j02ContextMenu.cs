
namespace UI.Menu
{
    public class j02ContextMenu:BaseContextMenu
    {
        public j02ContextMenu(BL.Factory f,int pid, string source,string period,string device) : base(f,pid)
        {
            this.device = device;
            var rec = f.j02UserBL.Load(pid);
            //HEADER(rec.FullNameAsc);
            if (source != "recpage")
            {
                AMI_RecPage("Stránka", "j02", pid,null,null, rec.FullNameAsc);
            }
            if (source != "grid")
            {
                AMI_RecGrid("Přejít do Tabulky", "j02", pid);
            }
            
            DIV();
            
            if (f.CurrentUser.IsAdmin)
            {
                if (rec.j02VirtualParentID == 0)
                {
                    AMI("Upravit kartu uživatele", $"javascript:_edit('j02',{pid})", "edit_note");
                    AMI_Clone("j02", pid);
                    AMI("Odstranit", $"javascript:_delete('j02',{pid})", "delete_forever");
                }
                else
                {
                    AMI("Nastavení virtuálního uživatele", $"javascript: _window_open('/j02/VirtualRecord?pid={pid}')", "domino_mask");
                }
               

            }

            if (_f.CurrentUser.j04IsModule_p31 && _f.CurrentUser.IsApprovingPerson)
            {
                DIV();
                AMI("Schválit/Vyúčtovat", $"javascript:tg_approve_contextmenu('j02',{pid})", "domino_mask");

            }

            
            AMI_Report("j02", pid);
           
            DIV();
            AMI("Další", null, null,null, "more");
            
            

            AMI_SendMail("j02", pid,"more");

            if (_f.CurrentUser.j04IsModule_p31 && _f.CurrentUser.IsApprovingPerson)
            {
                DIV(null, "more");
                AMI("Úkony vyloučit z vyúčtování", $"javascript: _window_open('/p31excludebilling/Index?pids={pid}&flag=1&prefix=j02',2)", "delete_sweep","more");
            }
            if (_f.CurrentUser.TestPermission(BO.PermValEnum.GR_GridExports))
            {
                AMI("PIERSTONE Report", $"javascript: _window_open('/Pierstone/Index?prefix=j02&pids={pid}',3)", "flag", "more");
            }
            AMI("SOUČTY", $"javascript:_window_open('/p31totals/Index?selected_entity=j02&selected_pids={rec.pid}&blank=1')", "functions", "more");

            if (rec.j02VirtualParentID == 0)
            {
                AMI("Vygenerovat virtuálního uživatele", $"javascript: _window_open('/j02/VirtualRecord?parent_pid={pid}&pid=0',3)", "domino_mask", "more");
            }
            if (f.CurrentUser.IsAdmin)
            {
                AMI("Simulace přístupových oprávnění", $"javascript: _window_open('/j02/PermSimulation?j02id={pid}',3)", "flag", "more");
                
            }


        }
    }
}
