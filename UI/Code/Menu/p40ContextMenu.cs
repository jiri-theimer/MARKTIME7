using BL;
using UI.Menu;

namespace UI.Code.Menu
{
    public class p40ContextMenu:BaseContextMenu
    {
        public p40ContextMenu(BL.Factory f, int pid, string source, string device) : base(f, pid)
        {
            this.device = device;
            var rec = f.p40WorkSheet_RecurrenceBL.Load(pid);

            var recP41 = f.p41ProjectBL.Load(rec.p41ID);
            var mydisp = f.p41ProjectBL.InhaleRecDisposition(recP41.pid,recP41);

           
            if (source != "recpage")
            {
                AMI_RecPage("Stránka", "p40", pid,null,null, rec.p40Name);
            }
            if (source != "grid")
            {
                AMI_RecGrid("Přejít do Tabulky", "p40", pid);
            }

            DIV();
            if (mydisp.OwnerAccess)
            {
                AMI("Upravit", $"javascript:_edit('p40',{pid})", "edit_note");
                AMI_Clone("p40", pid);

                if (_f.CurrentUser.IsAdmin)
                {
                    DIV();
                    AMI("Odstranit záznam", $"javascript:_delete('p40',{pid})", "delete_forever");
                }

                //DIV();
                //AMI("Nový", $"javascript:_edit('p40',0)", "add");
            }


            DIV();
            if (!recP41.isclosed)
            {
                AMI_Vykazat(recP41);
            }
            AMI_RecPage(recP41.TypePlusName, "p41", recP41.pid,"bind");
            if (recP41.p28ID_Client > 0)
            {
                AMI_RecPage(recP41.Client, "p28", recP41.p28ID_Client, "bind");
            }
            AMI("Vazby", null, null, null, "bind");


            

        }
    }
}
