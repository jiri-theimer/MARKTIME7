namespace UI.Menu
{
    public class p31approveContextMenu: BaseContextMenu
    {
        public p31approveContextMenu(BL.Factory f, int pid,string guid) : base(f, pid)
        {
            var rec = f.p31WorksheetBL.LoadTempRecord(pid,guid);

            AMI("Upravit zdrojový záznam úkonu", $"javascript:_window_open('/p31/Record?pid={pid}&approve_guid={guid}')", "edit_note");
            DIV();
            if (rec.p33ID == BO.p33IdENUM.Cas)
            {
                AMI("Rozdělit úkon na 2 kusy", $"javascript: _window_open('/p31split/Index?pid={pid}&approve_guid={guid}',2)", "arrow_split");
                DIV();
            }
            AMI_Workflow("p31", pid,0,0);
            DIV();
            AMI("Přidat nový úkon", $"javascript:p31_create()", "more_time");
            
        }
    }
}
