namespace UI.Menu
{
    public class x40ContextMenu:BaseContextMenu
    {
        public x40ContextMenu(BL.Factory f, int pid, string source) : base(f, pid)
        {
            var rec = f.MailBL.LoadMessageByPid(pid);
            
            HEADER("Odeslaná zpráva");

            AMI("Detail zprávy", $"javascript: _window_open('/Mail/Record?pid={rec.pid}')", "email");
            DIV();
            AMI("Zkopírovat do nové zprávy", $"javascript: _window_open('/Mail/SendMail?x40id={rec.pid}')", "content_copy");
           
            
            bool b = false;
            
            if (_f.CurrentUser.j04IsModule_p56 && rec.x40RecordEntity == "p56" && rec.x40RecordPid > 0)
            {
                b = true;
                var recP56 = _f.p56TaskBL.Load(rec.x40RecordPid);
                AMI_RecPage(recP56.p57Name + ": " + recP56.p56Name, "p56", rec.x40RecordPid, "bind");
            }
            if (rec.x40RecordEntity == "o22" && rec.x40RecordPid > 0)
            {
                b = true;
                var recO22 = _f.o22MilestoneBL.Load(rec.x40RecordPid);
                AMI_RecPage(recO22.FullName, "o22", rec.x40RecordPid, "bind");
            }
            if (_f.CurrentUser.j04IsModule_o23 && rec.x40RecordEntity == "o23" && rec.x40RecordPid > 0)
            {
                b = true;
                var recO23 = _f.o23DocBL.Load(rec.x40RecordPid);
                AMI_RecPage(recO23.o18Name + ": " + recO23.o23Name, "o23", rec.x40RecordPid, "bind");
            }
            if (_f.CurrentUser.j04IsModule_p91 && rec.x40RecordEntity=="p91" && rec.x40RecordPid>0)
            {
                b = true;
                var recP91 = _f.p91InvoiceBL.Load(rec.x40RecordPid);
                AMI_RecPage($"{recP91.p91Code}: {recP91.p92Name}", "p91", rec.x40RecordPid,"bind");
            }
            
            if (_f.CurrentUser.j04IsModule_p28 && rec.x40RecordEntity == "p28" && rec.x40RecordPid > 0)
            {
                b = true;
                var recJ02 = _f.p28ContactBL.Load(rec.x40RecordPid);
                AMI_RecPage(recJ02.FullNameAsc, "p20", rec.x40RecordPid, "bind");
            }
            if (_f.CurrentUser.j04IsModule_p41 && rec.x40RecordEntity == "p41" && rec.x40RecordPid > 0)
            {
                b = true;
                var recP41 = _f.p41ProjectBL.Load(rec.x40RecordPid);
                if (f.p07LevelsCount > 1)
                {
                    AMI_RecPage(recP41.p42Name + ": " + recP41.FullName, "le" + recP41.p07Level.ToString(), recP41.pid, "bind");
                }
                else
                {
                    AMI_RecPage(recP41.p42Name + ": " + recP41.FullName, "p41", recP41.pid, "bind");

                }
            }
            if (b)
            {
                AMI("Vazby", null, null, null, "bind");
            }
        }
    }
}
