
using BL;
using UI.Views.Shared.Components.myPeriod;

namespace UI.Menu
{
    public class p41ContextMenu: BaseContextMenu
    {
        public p41ContextMenu(BL.Factory f, int pid, string source,string period,string device) : base(f, pid)
        {
            this.device = device;
            var rec = f.p41ProjectBL.Load(pid);
            var disp = f.p41ProjectBL.InhaleRecDisposition(rec.pid,rec);
            
            if (source != "recpage")
            {
                //HEADER(rec.PrefferedName);
                AMI_RecPage("Stránka", "p41", pid,null,null, rec.PrefferedName);

            }
            if (source != "grid")
            {
                AMI_RecGrid("Přejít do Tabulky", "p41", pid);
                if (f.p07LevelsCount>1)
                {
                    AMI_RecGrid($"{f.getP07Level(rec.p07Level, false)} (L{rec.p07Level})", $"le{rec.p07Level}", pid,0,false);
                }

            }
            
            if (disp.OwnerAccess)
            {
                DIV();
                AMI($"Upravit kartu {_f.getP07Level_Inflection(rec.p07Level)}", $"javascript:_edit('p41',{pid})", "edit_note");
                AMI_Clone("p41", pid);
                AMI("Odstranit", $"javascript:_delete('p41',{pid})", "delete_forever");
                

            }
            else
            {
                AMI_Clone("p41", pid,true);
            }
            DIV();

            if (!rec.isclosed && _f.CurrentUser.j04IsModule_p31)
            {
                
                AMI_Vykazat(rec);
                
                if (_f.CurrentUser.IsApprovingPerson && rec.p41BillingFlag != BO.p41BillingFlagEnum.NuloveSazbyBezWip)
                {
                    var mq = new BO.myQueryP31() { isinvoiced = false,MyRecordsDisponible_Approve=true };
                    
                    if (!string.IsNullOrEmpty(period))
                    {
                        var pp = new BL.Singleton.ThePeriodProvider();
                        var per = new myPeriodViewModel();
                        per.LoadFromString(pp, _f, period);
                        mq.global_d1 = per.d1; ;
                        mq.global_d2 = per.d2;
                        mq.period_field = per.PeriodField;
                    }
                    if (rec.p07Level < 5)
                    {
                        mq.leindex = rec.p07Level;
                        mq.lepid = rec.pid;
                    }
                    else
                    {
                        mq.p41id = rec.pid;
                    }

                    var lisP31 = _f.p31WorksheetBL.GetList(mq);
                    if (lisP31.Count() > 0)
                    {
                        int intWIP = lisP31.Where(p => p.p71ID == BO.p71IdENUM.Nic).Count();
                        int intAPP = lisP31.Where(p => p.p71ID != BO.p71IdENUM.Nic).Count();

                        AMI("Schválit/Vyúčtovat", $"javascript:tg_approve_contextmenu('p41',{pid})", "approval", null, null, null, this.CompleteText_SchvalitVyuctovat(mq.global_d1, mq.global_d2, intWIP, intAPP));
                    }

                }
                                

            }
                  
            
            AMI_Workflow("p41", pid,rec.b02ID,rec.b01ID);

            if (_f.CurrentUser.j04IsModule_p56)
            {
                AMI("Úkoly/Kalendář", null, null, null, "p56");
                var lisP60 = f.p60TaskTemplateBL.GetList(new BO.myQuery("p60")).Where(p => p.p60IsPublic || p.j02ID_Owner == _f.CurrentUser.pid);
                if (lisP60.Count()==0)
                {
                    AMI("Úkol", $"javascript: _window_open('/p56/Record?wrk_record_prefix=p41&wrk_record_pid={pid}', 3)", "task", "p56");
                }
                else
                {
                    foreach(var c in lisP60)
                    {
                        AMI(c.p60Name, $"javascript: _window_open('/p56/Record?wrk_record_prefix=p41&wrk_record_pid={pid}&p60id={c.pid}', 3)", "task", "p56");
                    }
                }
                
                AMI("Todo List", $"javascript: _window_open('/p56/TodoList?wrk_record_prefix=p41&wrk_record_pid={pid}', 3)", "task", "p56");
                AMI("Opakovaný úkol", $"javascript: _window_open('/p58/Record?pid=0&wrk_record_prefix=p41&p41id={pid}')", "published_with_changes", "p56");
                DIV(null, "p56");
                AMI("Termín/Událost", $"javascript: _window_open('/o22/Record?wrk_record_prefix=p41&wrk_record_pid={pid}', 3)", "event", "p56");
            }

            AMI_Doc($"p41", pid);

            DIV();
            AMI("Další", null, null, null, "more");
            if (disp.OwnerAccess)
            {
                AMI_ChangeLog("p41", pid, "more");
            }
            
            
            AMI_Report("p41", pid, "more");
           if (f.CurrentUser.IsRatesAccess)
            {
                AMI("Opakovaná/paušální odměna", $"javascript: _window_open('/p40/Record?pid=0&p41id={pid}', 2)", "repeat_on", "more");

            }

            if (_f.CurrentUser.j04IsModule_r01)
            {
                AMI("Kapacitní plán projektu", $"javascript: _window_open('/r01/ProjectPlan?p41id={pid}', 2)", "online_prediction", "more");

            }


            DIV(null, "more");
            
            AMI_SendMail("p41", pid, "more");

            if (_f.CurrentUser.j04IsModule_p41 && _f.CurrentUser.IsApprovingPerson)
            {
                
                AMI("Úkony vyloučit z vyúčtování", $"javascript: _window_open('/p31excludebilling/Index?pids={pid}&flag=1&prefix=p41',2)", "delete_sweep", "more");
            }

            if (_f.App.Implementation=="pierstone" && _f.CurrentUser.TestPermission(BO.PermValEnum.GR_GridExports))
            {
                AMI("PIERSTONE Report", $"javascript: _window_open('/Pierstone/Index?prefix=p41&pids={pid}',3)", "flag", "more");
            }
            AMI("SOUČTY", $"javascript:_window_open('/p31totals/Index?selected_entity=p41&selected_pids={rec.pid}&blank=1')", "functions", "more");
            AMI("PROXY výsledovky", $"javascript:_window_open('/Vysledovka/Index?master_entity=p41&master_pid={rec.pid}')", "balance", "more");

            
            AMI_RowColor("p41", rec.pid);

            bool bolVazby = false;
            if (rec.p28ID_Client > 0)
            {
                AMI_RecPage(_f.tra("Klient") + ": " + rec.Client, "p28", rec.p28ID_Client, "bind"); bolVazby = true;
            }
            if (rec.p41ID_P07Level1 > 0 && rec.p41ID_P07Level1 != rec.pid)
            {
                var rec1 = _f.p41ProjectBL.Load(rec.p41ID_P07Level1);
                if (rec1 !=null) AMI_RecPage(rec1.TypePlusName, "le1", rec.p41ID_P07Level1, "bind"); bolVazby = true;

            }
            if (rec.p41ID_P07Level2 > 0 && rec.p41ID_P07Level2 != rec.pid)
            {
                var rec2 = _f.p41ProjectBL.Load(rec.p41ID_P07Level2);
                if (rec2 !=null) AMI_RecPage(rec2.TypePlusName, "le2", rec.p41ID_P07Level2, "bind"); bolVazby = true;
            }
            if (rec.p41ID_P07Level3 > 0 && rec.p41ID_P07Level3 != rec.pid)
            {
                var rec3 = _f.p41ProjectBL.Load(rec.p41ID_P07Level3);
                if (rec3 !=null) AMI_RecPage(rec3.TypePlusName, "le3", rec.p41ID_P07Level3, "bind"); bolVazby = true;
            }
            if (rec.p41ID_P07Level4 > 0 && rec.p41ID_P07Level4 != rec.pid)
            {
                var rec4 = _f.p41ProjectBL.Load(rec.p41ID_P07Level4);
                if (rec4 !=null) AMI_RecPage(rec4.TypePlusName, "le4", rec.p41ID_P07Level4, "bind"); bolVazby = true;
            }
            if (bolVazby)
            {
                AMI("Vazby", null, null, null, "bind");
            }

        }
    }
}
