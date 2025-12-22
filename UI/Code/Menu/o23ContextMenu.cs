

namespace UI.Menu
{
    public class o23ContextMenu: BaseContextMenu
    {
        public o23ContextMenu(BL.Factory f, int pid, string source,string device) : base(f, pid)
        {
            this.device = device;
            var rec = f.o23DocBL.Load(pid);
            var perm = f.o23DocBL.InhaleRecDisposition(rec.pid,rec);

            HEADER(rec.o23Name);
            if (source != "recpage")
            {
                AMI_RecPage("Stránka", "o23", pid,null,null,rec.o18Name);
            }
            if (source != "grid")
            {
                AMI_RecGrid("Přejít do Tabulky", "o23", pid,rec.o17ID);
            }

            if (rec.o18TemplateFlag == BO.o18TemplateENUM.Uctenka)
            {
                var lis = f.p31WorksheetBL.GetList(new BO.myQueryP31() { o23id = rec.pid });
                if (lis.Count() == 0)
                {
                    AMI("Překlopit na peněžní výdaj", $"javascript:_window_open('/p31/Record?pid=0&newrec_prefix=o23&newrec_pid={pid}',3)", "paid");
                }
                

            }


            if (perm.OwnerAccess)
            {
                DIV();
                AMI("Upravit kartu záznamu", $"javascript:_edit('o23',{pid})", "edit_note");                
                AMI_Clone("o23", pid);
                AMI("Odstranit", $"javascript:_delete('o23',{pid})", "delete_forever");
                
            }
            else
            {
                AMI_Clone("o23", pid,true);
            }
            DIV();


            if (_f.CurrentUser.j04IsModule_p31 && rec.o18TemplateFlag == BO.o18TemplateENUM.None)
            {
                
                AMI("Vykázat úkon", $"javascript:_window_open('/p31/Record?pid=0&newrec_prefix=o23&newrec_pid={pid}',3)", "more_time");
            }
            
            
            

            AMI_Workflow("o23", pid,rec.b02ID,rec.b01ID);
            DIV();
            AMI("Další", null, null, null, "more");
            if (perm.OwnerAccess)
            {
                AMI_ChangeLog("o23", pid, "more");
            }
            

            AMI_SendMail("o23", pid,"more");            
            AMI_Report("o23", pid,"more");

            AMI_RowColor("o23", rec.pid);

            var lisO19 = _f.o23DocBL.GetList_o19(rec.pid);
            if (lisO19.Count() > 0)
            {
                AMI("Vazby", null, null, null, "bind");
            }
            foreach(var c in lisO19)
            {
                if (c.entity == "p41")
                {
                    var recP41 = _f.p41ProjectBL.Load(c.o19RecordPid);
                    AMI_RecPage(recP41.TypePlusName, "p41", c.o19RecordPid, "bind");
                }
                else
                {
                    AMI_RecPage(_f.CBL.GetObjectAlias(c.o20Entity,c.o19RecordPid), c.o20Entity, c.o19RecordPid, "bind");
                }
                
            }
        }
    }
}
