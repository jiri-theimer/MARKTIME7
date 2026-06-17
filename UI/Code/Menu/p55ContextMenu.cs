namespace UI.Menu
{
    public class p55ContextMenu: BaseContextMenu
    {
        public p55ContextMenu(BL.Factory f, int pid, string source, string device) : base(f, pid)
        {
            this.device = device;
            var rec = f.p56TaskBL.LoadToDoList(pid);
            
            
            if (source != "grid")
            {
                AMI_RecGrid("Přejít do Tabulky", "p55", pid);
            }

            var lis = _f.p56TaskBL.GetList(new BO.myQueryP56() {IsRecordValid=null, p55id = pid }).OrderBy(p => p.p56Ordinary);
            if (lis.Count() > 0)
            {
                AMI("Nový podle tohoto vzoru", $"javascript: _window_open('/p56/ToDoList?wrk_record_prefix=le5&wrk_record_pid={lis.First().p41ID}&p55id={pid}',3)", "content_copy");
            }

            if (f.CurrentUser.IsAdmin || rec.j02ID_Owner == f.CurrentUser.pid)
            {
                DIV();
                AMI("Přejmenovat", $"javascript: _p55_record_rename({pid})", "edit");
            }
            



        }
    }
}
