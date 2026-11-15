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


            if (rec.IsMail)
            {
                //e-mail ve FILEBOXu: nový záznam předvyplněný z e-mailu (UI.Code.MailRecordSupport); e-mail se k němu přiloží jako kopie
                bool bolTask = f.CurrentUser.j04IsModule_p56;
                bool bolUkon = f.CurrentUser.j04IsModule_p31;
                if (bolTask || bolUkon) DIV();
                if (bolTask) AMI("Založit úkol z e-mailu", $"javascript: _window_open('/p56/Record?pid=0&o27id={pid}', 3)", "task");
                if (bolUkon) AMI("Vykázat úkon z e-mailu", $"javascript: _window_open('/p31/Record?pid=0&o27id={pid}', 3)", "more_time");
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
