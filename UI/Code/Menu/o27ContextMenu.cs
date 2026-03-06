using UI.Menu;

namespace UI.Code.Menu
{
    public class o27ContextMenu: BaseContextMenu
    {
        public o27ContextMenu(BL.Factory f, int pid, string source, string device) : base(f, pid)
        {
            this.device = device;
            var rec = f.o27AttachmentBL.Load(pid);

           

            if (source != "recpage")
            {
                AMI_RecPage("FILEBOX stránka", "o27", pid);
            }
            if (source != "grid")
            {
                AMI_RecGrid("Přejít do Tabulky", "o27", pid);
            }


            if (rec.UserInsert==f.CurrentUser.j02Login || f.CurrentUser.IsAdmin)
            {
                DIV();
                AMI("Karta záznamu", $"javascript:_edit('o27',{pid})", "edit_note");

                AMI("Odstranit záznam", $"javascript:_delete('o27',{pid})", "delete_forever");


            }


            DIV();

            switch (rec.o27Entity)
            {
                case "p41":
                    var recP41 = f.p41ProjectBL.Load(rec.o27RecordPid);
                    AMI_RecPage(recP41.TypePlusName, rec.o27Entity, rec.o27RecordPid, "bind");
                    break;
                case "p28":
                    var recP28 = f.p28ContactBL.Load(rec.o27RecordPid);
                    AMI_RecPage(recP28.p28Name, rec.o27Entity, rec.o27RecordPid, "bind");
                    break;
                case "p56":
                    var recP56 = f.p56TaskBL.Load(rec.o27RecordPid);
                    AMI_RecPage(recP56.p56Name, rec.o27Entity, rec.o27RecordPid, "bind");
                    break;
                case "p91":
                    var recP91 = f.p91InvoiceBL.Load(rec.o27RecordPid);
                    AMI_RecPage(recP91.p91Code, rec.o27Entity, rec.o27RecordPid, "bind");
                    break;
                case "o23":
                    var recO23 = f.o23DocBL.Load(rec.o27RecordPid);
                    AMI_RecPage(recO23.o18Name+": "+recO23.o23Name, rec.o27Entity, rec.o27RecordPid, "bind");
                    break;
            }
            
           
            AMI("Vazby", null, null, null, "bind");




        }
    }
}
