
using UI.Views.Shared.Components.myPeriod;

namespace UI.Menu
{
    public class p56ContextMenu : BaseContextMenu
    {
        public p56ContextMenu(BL.Factory f, int pid, string source, string period, string device) : base(f, pid)
        {
            this.device = device;
            var rec = f.p56TaskBL.Load(pid);
            var disp = f.p56TaskBL.InhaleRecDisposition(rec.pid, rec);
            if (!disp.ReadAccess)
            {
                if (rec.p41ID > 0)
                {
                    //zjistit zda uživatel není manažerem projektu
                    var dispP41 = f.p41ProjectBL.InhaleRecDisposition(rec.p41ID);
                    disp.ReadAccess = dispP41.ReadAccess;
                }
                if (!disp.ReadAccess)
                {
                    AMI_NoDisposition(); return;  //bez oprávnění
                }

            }

            if (source != "recpage")
            {
               // HEADER($"{rec.p57Name}: <b>{rec.b02Name}</b>");


                AMI_RecPage("Stránka", "p56", pid,null,null, $"{rec.p56Code} | {rec.b02Name}");
            }
            if (source != "grid")
            {
                AMI_RecGrid("Přejít do Tabulky", "p56", pid);
            }


            if (disp.OwnerAccess)
            {
                DIV();
                AMI("Upravit kartu úkolu", $"javascript:_edit('p56',{pid})", "edit_note");
                AMI_Clone("p56", pid);
                AMI("Odstranit", $"javascript:_delete('p56',{pid})", "delete_forever");
            }
            else
            {
                AMI_Clone("p56", pid, true);
            }
            DIV();

            BO.p41Project recP41 = null;
            if (!rec.isclosed && rec.p41ID > 0)
            {
                recP41 = _f.p41ProjectBL.Load(rec.p41ID);
                
                AMI_Vykazat(recP41, rec.pid);
            }

            int intP91ID = 0; string strP91Code = null;
            if (f.CurrentUser.j04IsModule_p91 || f.CurrentUser.IsApprovingPerson)
            {
                var mqP31 = new BO.myQueryP31() { isinvoiced = false, p56id = rec.pid, MyRecordsDisponible_Approve = true };
                if (!string.IsNullOrEmpty(period))
                {
                    var pp = new BL.Singleton.ThePeriodProvider();
                    var per = new myPeriodViewModel();
                    per.LoadFromString(pp, _f, period);
                    mqP31.global_d1 = per.d1; ;
                    mqP31.global_d2 = per.d2;
                    mqP31.period_field = per.PeriodField;
                }

                var lisP31 = _f.p31WorksheetBL.GetList(mqP31);
                if (lisP31.Count() > 0)
                {
                    int intWIP = lisP31.Where(p => p.p71ID == BO.p71IdENUM.Nic).Count();
                    int intAPP = lisP31.Where(p => p.p71ID != BO.p71IdENUM.Nic).Count();
                    AMI("Schválit/Vyúčtovat", $"javascript: tg_approve_contextmenu('p56',{pid})", "approval", null, null, null, "(" + intWIP.ToString() + "x + " + intAPP.ToString() + "x)");
                    if (lisP31.Any(p => p.p91ID > 0))
                    {
                        intP91ID = lisP31.First(p => p.p91ID > 0).p91ID;
                        strP91Code = lisP31.First(p => p.p91ID > 0).p91Code;
                    }
                }
            }


            

            AMI_Workflow("p56", pid, rec.b02ID, rec.b01ID);

            DIV();

            AMI("Další", null, null, null, "more");
            if (disp.OwnerAccess)
            {
                AMI_ChangeLog("p56", pid, "more");
            }
            
            AMI_Report("p56", pid, "more");

            AMI_SendMail("p56", pid, "more");

            if (recP41 != null || rec.b05RecordEntity != null)
            {
                AMI("Vazby", null, null, null, "bind");
            }

            if (rec.b05RecordEntity != null && rec.b05RecordPid > 0 && rec.b05RecordEntity != "p41")
            {
                AMI_RecPage(BO.Code.Bas.OM2(_f.CBL.GetObjectAlias(rec.b05RecordEntity, rec.b05RecordPid), 35), rec.b05RecordEntity, rec.b05RecordPid, "bind");
            }
            if (recP41 != null)
            {
                AMI_RecPage(recP41.p07Name + ": " + rec.ProjectWithClient, "p41", rec.p41ID, "bind");
                if (recP41.p28ID_Client > 0)
                {
                    AMI_RecPage(recP41.Client, "p28", recP41.p28ID_Client, "bind");
                }
                if (intP91ID > 0)
                {
                    AMI_RecPage(strP91Code, "p91", intP91ID, "bind");
                }

                AMI("SOUČTY", $"javascript:_window_open('/p31totals/Index?selected_entity=p56&selected_pids={rec.pid}&blank=1')", "functions", "more");
            }

            

        }
    }
}
