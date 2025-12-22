using UI.Menu;

namespace UI.Code.Menu
{
    public class b05ContextMenu: BaseContextMenu
    {
        public b05ContextMenu(BL.Factory f, int pid, string source, string device) : base(f, pid)
        {
            var rec = f.WorkflowBL.GetList_b05(null, 0, 0, pid,0).First();

           
            if (source != "recpage")
            {
                HEADER(rec.o21Name);
                AMI_RecPage("Stránka poznámky", "b05", pid);
            }
            if (source != "grid")
            {
                AMI_RecGrid("Přejít do Tabulky", "b05", pid);
            }

            DIV();
            
            if (rec.j02ID_Sys==_f.CurrentUser.pid || _f.CurrentUser.IsAdmin || _f.CurrentUser.TestPermission(BO.PermValEnum.GR_b05OwnerAll))
            {
                AMI("Upravit", $"javascript:_edit('b05',{pid})", "edit_note");
                
                AMI("Odstranit", $"javascript:_delete('b05',{pid})", "delete_forever");
               
            }

            DIV();
            AMI("Odeslat e-mail", $"javascript: _window_open('/Mail/SendMail?record_entity={rec.b05RecordEntity}&record_pid={rec.b05RecordPid}&b05id={rec.pid}',2)", "email");

            

            if (rec.b05RecordEntity != null)
            {
                AMI("Vazby", null, null, null, "bind");

             

                if (rec.b05RecordEntity == "p41")
                {
                    var recP41 = _f.p41ProjectBL.Load(rec.b05RecordPid);
                    AMI_RecPage(recP41.FullName, "p41", recP41.pid, "bind");
                    if (recP41.p28ID_Client > 0)
                    {
                        AMI_RecPage(recP41.Client, "p28", recP41.p28ID_Client, "bind");
                    }
                }
                if (rec.b05RecordEntity == "p91")
                {
                    var recP91 = _f.p91InvoiceBL.Load(rec.b05RecordPid);
                    if (recP91.p28ID > 0)
                    {
                        AMI_RecPage(recP91.p28Name, "p28", recP91.p28ID, "bind");
                    }
                    AMI_RecPage($"{recP91.p92Name}: {recP91.p91Code}", "p91", recP91.pid, "bind");
                }
                if (rec.b05RecordEntity == "p28")
                {
                    var recP28 = _f.p28ContactBL.Load(rec.b05RecordPid);
                    if (recP28 !=null)
                    {
                        AMI_RecPage(recP28.p28Name, "p28", recP28.pid, "bind");
                    }
                }
                if (rec.b05RecordEntity == "p31")
                {
                    var recP31 = _f.p31WorksheetBL.Load(rec.b05RecordPid);   
                    if (recP31 != null)
                    {
                        AMI("Úkon", $"/TheGrid/FlatView?prefix=p31&myqueryinline=pids|list_int|{recP31.pid}", "more_time", "bind");
                        AMI_RecPage(recP31.Project, "p41", recP31.p41ID, "bind");
                        if (recP31.p28ID_Client > 0)
                        {
                            AMI_RecPage(recP31.ClientName, "p28", recP31.p28ID_Client, "bind");
                        }
                    }
                    
                }
            }

            
            

        }
    }
}
