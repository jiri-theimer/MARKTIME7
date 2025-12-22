namespace UI.Menu
{
    public class o22ContextMenu: BaseContextMenu
    {
        public o22ContextMenu(BL.Factory f, int pid, string source) : base(f, pid)
        {
            var rec = f.o22MilestoneBL.Load(pid);
            var disp = f.o22MilestoneBL.InhaleRecDisposition(rec.pid, rec);

            if (!disp.ReadAccess)
            {
                AMI_NoDisposition(); return;  //bez oprávnění                
            }

            if (source != "recpage")
            {
                HEADER(rec.o21Name);
                AMI_RecPage("Stránka: "+ rec.o21Name, "o22", pid);
            }
            if (source != "grid")
            {
                AMI_RecGrid("Přejít do Tabulky", "o22", pid);
            }

            
            if (disp.OwnerAccess)
            {
                DIV();
                AMI("Upravit kartu záznamu", $"javascript:_edit('o22',{pid})", "edit_note");
                AMI_Clone("o22", pid);
                AMI("Odstranit", $"javascript:_delete('o22',{pid})", "delete_forever");
            }

            DIV();
            AMI_Workflow("o22", pid,0,0);
            


            AMI("Další", null, null, null, "more");

            
            AMI_SendMail("o22", pid, "more");

            if (rec.b05RecordEntity != null)
            {
                AMI("Vazby", null, null, null, "bind");
            }

           
           
            if (rec.b05RecordEntity != null && rec.b05RecordPid > 0 && rec.b05RecordEntity != "p41")
            {
                AMI_RecPage(BO.Code.Bas.OM2(_f.CBL.GetObjectAlias(rec.b05RecordEntity, rec.b05RecordPid), 35), rec.b05RecordEntity, rec.b05RecordPid, "bind");
            }
            if (rec.p41ID > 0)
            {
                var recP41 = _f.p41ProjectBL.Load(rec.p41ID);
                AMI_RecPage(recP41.p07Name + ": " + rec.ProjectWithClient, "p41", rec.p41ID, "bind");
                if (recP41.p28ID_Client > 0)
                {
                    AMI_RecPage(recP41.Client, "p28", recP41.p28ID_Client, "bind");
                }
                
            }
           
        }
    }
}
