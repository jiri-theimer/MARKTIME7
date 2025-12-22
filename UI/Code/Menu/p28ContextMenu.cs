
using UI.Views.Shared.Components.myPeriod;

namespace UI.Menu
{
    public class p28ContextMenu : BaseContextMenu
    {
        public p28ContextMenu(BL.Factory f, int pid, string source,string period,string device) : base(f, pid)
        {
            this.device = device;
            var rec = f.p28ContactBL.Load(pid);
            var perm = f.p28ContactBL.InhaleRecDisposition(rec.pid, rec);
            var recP29 = f.p29ContactTypeBL.Load(rec.p29ID);

            var mq = new BO.myQueryP41("p41") { MyRecordsDisponible = true, p28id = rec.pid };
            var lisP41 = _f.p41ProjectBL.GetList(mq);

            if (source != "recpage")
            {
                //HEADER(rec.p28Name);
                AMI_RecPage("Stránka", "p28", pid,null,null, rec.p28Name);
                
            }
            if (source != "grid")
            {
                AMI_RecGrid("Přejít do Tabulky", "p28", pid);
                
            }
            if (f.CurrentUser.j04IsModule_p41 && (recP29.p29ScopeFlag == BO.p29ScopeFlagENUM.Anyone || recP29.p29ScopeFlag == BO.p29ScopeFlagENUM.Client || recP29.p29ScopeFlag == BO.p29ScopeFlagENUM.ClientOrSupplier))
            {
                string strAfterName = (lisP41.Count() > 0 ? lisP41.Count().ToString() : null);
                
                var cc=AMI("Projekty klienta", null, null, null, "p41",null, strAfterName);
                
                if (f.p42ProjectTypeBL.GetList_ProjectCreate().Count() > 0)
                {
                    AMI("Nový projekt", $"javascript: _window_open('/p41/Record?pid=0&p28id={rec.pid}')", "add", "p41");
                }
                if (lisP41.Count()>0 && lisP41.Count() < 10)
                {
                    DIV(null, "p41");
                    foreach (var c in lisP41)
                    {
                        AMI_RecPage(c.PrefferedName, "p41", c.pid, "p41");
                    }
                    
                }
                

            }

            DIV();

           
            if (perm.OwnerAccess)
            {
                AMI("Upravit kartu kontaktu", $"javascript:_edit('p28',{pid})", "edit_note");
                AMI_Clone("p28", pid);
                AMI("Odstranit", $"javascript:_delete('p28',{pid})", "delete_forever");
            }
            else
            {
                AMI_Clone("p28", pid,true);
            }

            DIV();

            if (lisP41.Count() > 0)
            {
                
                if (lisP41.Count() == 1)
                {
                    AMI_Vykazat(lisP41.First());
                }
                else
                {
                    AMI("Vykázat úkon", $"javascript: _window_open('/p41/SelectProject?source_prefix=p28&source_pid={rec.pid}')", "more_time");
                }

                var mqP31 = new BO.myQueryP31() { isinvoiced = false, p28id = rec.pid,MyRecordsDisponible_Approve=true };
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
                                      
                    AMI("Schválit/Vyúčtovat", $"javascript:tg_approve_contextmenu('p28',{pid})", "approval", null, null, null, this.CompleteText_SchvalitVyuctovat(mqP31.global_d1, mqP31.global_d2, intWIP, intAPP));

                }
            }

            AMI_Workflow("p28", pid, rec.b02ID, rec.b01ID);
            
            AMI_Doc("p28", pid);
            DIV();
            AMI("Další", null, null, null, "more");

            AMI_ChangeLog("p28", pid, "more");
            AMI_Report("p28", pid,"more");

            AMI_SendMail("p28", pid, "more");

            

            if (_f.CurrentUser.j04IsModule_p31 && _f.CurrentUser.IsApprovingPerson)
            {
                DIV(null, "more");
                AMI("Úkony vyloučit z vyúčtování", $"javascript: _window_open('/p31excludebilling/Index?pids={pid}&flag=1&prefix=p28',2)", "delete_sweep","more");
            }

            if (_f.CurrentUser.TestPermission(BO.PermValEnum.GR_GridExports))
            {
                AMI("PIERSTONE Report", $"javascript: _window_open('/Pierstone/Index?prefix=p28&pids={pid}',3)", "flag", "more");
            }
            AMI("SOUČTY", $"javascript:_window_open('/p31totals/Index?selected_entity=p28&selected_pids={rec.pid}&blank=1')", "functions", "more");

            AMI_RowColor("p28", rec.pid);

            if (rec.p28RegID != null && rec.p28CountryCode == "CZ")
            {
                AMI("Rejstříky", null, null, null, "regs");
                AMI("JUSTICE", BO.Code.Rejstriky.Justice(rec.p28RegID), null, "regs", null, "_blank");
                AMI("ARES", BO.Code.Rejstriky.Ares(rec.p28RegID), null, "regs", null, "_blank");
                AMI("INSOLVENCE", BO.Code.Rejstriky.Insolvence(rec.p28RegID), null, "regs", null, "_blank");
            }
            if (rec.p28RegID != null && rec.p28CountryCode == "SK")
            {
                AMI("OBCHODNÝ REGISTER (SK)", BO.Code.Rejstriky.ObchodnyRegister(rec.p28RegID), null, "regs", null, "_blank");
            }

        }
    }
}
