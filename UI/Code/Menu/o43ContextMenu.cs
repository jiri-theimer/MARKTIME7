namespace UI.Menu
{
    public class o43ContextMenu: BaseContextMenu
    {
        public o43ContextMenu(BL.Factory f, int pid, string source,string device) : base(f, pid)
        {
            this.device = device;
            var rec = f.o43InboxBL.Load(pid);
            
            if(source != "recpage")
            {
                HEADER("INBOX záznam");
                AMI_RecPage("Stránka záznamu", "o43", pid);
                DIV();
            }
            
            AMI("Karta INBOX záznamu", $"javascript:_edit('o43',{pid})", "edit_note");
            AMI("Odstranit", $"javascript:_delete('o43',{pid})", "delete_forever");
            DIV();           
            if (rec.o43AttachmentsCount > 0)
            {
                AMI("Přílohy", null, "attachment", null, "atts");
                var lis = BO.Code.Bas.ConvertString2List(rec.o43Attachments, ",");
                foreach(string s in lis)
                {
                    AMI(s, $"/o43/DownloadAsFile?pid={rec.pid}&format={s}",null,"atts",null,"_blank");
                }
            }
            AMI("MS-OUTLOOK (msg)", "/o43/DownloadAsFile?pid=" + rec.pid.ToString() + "&format=msg", "attachment");
            AMI("MIME (eml)", "/o43/DownloadAsFile?pid=" + rec.pid.ToString() + "&format=eml", "attachment");

            if (rec.p41ID>0 || rec.j02ID>0 || rec.p91ID>0 || rec.p28ID>0 || rec.o23ID>0 || rec.p56ID > 0 || rec.b05ID>0)
            {
                DIV();
                AMI("Vazby", null, null, null, "rel");
            }
            else
            {
                DIV();
                AMI("Založit z poštovní zprávy", null, null, null, "create");
                if (f.CurrentUser.j04IsModule_o23)
                {
                    AMI("Dokument", $"javascript:_window_open('/o23/Record?pid=0&o43id_source={pid}',3)", "file_present", "create");
                }
                if (f.CurrentUser.j04IsModule_p56)
                {
                    AMI("Úkol", $"javascript:_window_open('/p56/Record?pid=0&o43id_source={pid}',3)", "task", "create");
                }                
                AMI("Událost/Termín", $"javascript:_window_open('/o22/Record?pid=0&o43id_source={pid}',3)", "today", "create");
                AMI("Notepad", $"javascript:_window_open('/b05/Record?pid=0&o43id_source={pid}',3)", "speaker_notes", "create");
               
            }
            

            if (rec.p41ID > 0)
            {
                var recP41 = _f.p41ProjectBL.Load(rec.p41ID);
                AMI_RecPage(recP41.p42Name + ": " + recP41.FullName, "p41", recP41.pid, "rel");
            }
            if (rec.p28ID > 0)
            {                
                AMI_RecPage(_f.tra("Kontakt") + ": " + rec.p28Name, "p28", rec.p28ID, "rel");
            }
            if (rec.j02ID > 0)
            {                
                AMI_RecPage(_f.tra("Uživatel") + ": " + rec.Person, "j02", rec.j02ID, "rel");
            }
            if (rec.p56ID > 0)
            {
                var recP56 = _f.p56TaskBL.Load(rec.p56ID);
                AMI_RecPage(recP56.p57Name + ": " + recP56.FullName, "p56", rec.p56ID, "rel");
            }
            if (rec.o23ID > 0)
            {
                var recO23 = _f.o23DocBL.Load(rec.o23ID);
                AMI_RecPage(recO23.o18Name + ": " + recO23.o23Name, "o23", rec.o23ID, "rel");
            }
            if (rec.p91ID > 0)
            {
                var recP91 = _f.p91InvoiceBL.Load(rec.p91ID);
                AMI_RecPage(recP91.p92Name + ": " + recP91.p91Code, "p91", rec.p91ID, "rel");
            }
            if (rec.b05ID > 0)
            {
                var recP91 = _f.p91InvoiceBL.Load(rec.p91ID);
                AMI_RecPage("Notepad", "b05", rec.b05ID, "rel");
            }

        }
    }
}
