

namespace UI.Menu
{
    public class defContextMenu: BaseContextMenu
    {
        public defContextMenu(BL.Factory f, int pid, string source,string prefix) : base(f, pid)
        {
            
           

            if (BL.Code.UserUI.GetFlatViewValue(prefix) > 0)
            {
                if (source != "recpage")
                {

                    AMI_RecPage("Stránka záznamu", prefix, pid);
                }

                if (source != "grid")
                {
                    AMI_RecGrid("Přejít do Tabulky", prefix, pid);
                    
                }
                
                DIV();
            }
            

            AMI("Upravit", $"javascript:_edit('{prefix}',{pid})", "edit_note");
            AMI_Clone(prefix, pid);

            if (_f.CurrentUser.IsAdmin)
            {
                DIV();
                AMI("Odstranit záznam", $"javascript:_delete('{prefix}',{pid})", "delete_forever");
            }
            
            

            DIV();
            AMI("Nový", $"javascript:_edit('{prefix}',0)", "add");

            
        }
    }
}
