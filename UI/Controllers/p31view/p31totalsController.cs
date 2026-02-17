using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Text;
using UI.Models;

using UI.Models.p31view;


namespace UI.Controllers.p31view
{
    public class p31totalsController : BaseController
    {
        private readonly BL.Singleton.ThePeriodProvider _pp;
        private readonly BL.TheColumnsProvider _colsProvider;
        public p31totalsController(BL.Singleton.ThePeriodProvider pp, BL.TheColumnsProvider cp)
        {
            _pp = pp;
            _colsProvider = cp;
        }
        public IActionResult Index(string master_entity, int master_pid, string caller, int j79id, string selected_entity, string selected_pids,string guid_pids)
        {
            if (Factory.CBL.LoadUserParamBool("p31totals-useoldversion",false))
            {
                //přesměrovat na starou verzi statistik
                return RedirectToAction("Index", "p31totalsOld", new { master_entity = master_entity, master_pid = master_pid, caller= caller, j79id= j79id, selected_entity= selected_entity, selected_pids= selected_pids, guid_pids= guid_pids });
            }
            

            var v = new p31TotalsViewModel() { record_pid = master_pid, record_prefix = master_entity, SelectedJ79ID = j79id, selected_entity = selected_entity, selected_pids = selected_pids };
            if (!string.IsNullOrEmpty(guid_pids))
            {
                v.selected_pids = Factory.p85TempboxBL.LoadByGuid(guid_pids).p85Message; //vstupní pids předány přes p85Tempbox
            }
            v.periodinput = new Views.Shared.Components.myPeriod.myPeriodViewModel() { prefix = "p31", UserParamKey = "p31totals-period" };

            if (!string.IsNullOrEmpty(v.record_prefix))
            {
                v.record_prefix = v.record_prefix.Substring(0, 3);
            }
            else
            {
                v.periodinput.UserParamKey = "p31totals-period-root";
            }

            v.periodinput.LoadUserSetting(_pp, Factory);

            

            if (v.SelectedJ79ID == 0)
            {
                v.SelectedJ79ID = Factory.CBL.LoadUserParamInt($"p31totals-{v.record_prefix}-j79id");
            }
            

            v.lisJ79 = Factory.j79TotalsTemplateBL.GetList(Factory.CurrentUser.pid, v.record_prefix, true);

            if (!v.lisJ79.Any(p => p.j02ID == Factory.CurrentUser.pid && p.j79IsSystem))
            {
                Factory.j79TotalsTemplateBL.CreateDefaultSysRecord(Factory.CurrentUser.pid, v.record_prefix, true);
                v.lisJ79 = Factory.j79TotalsTemplateBL.GetList(Factory.CurrentUser.pid, v.record_prefix, true);
            }
            if (v.SelectedJ79ID == 0 || !v.lisJ79.Any(p => p.pid == v.SelectedJ79ID))
            {
                v.SelectedJ79ID = v.lisJ79.First().pid;
            }
            if (v.SelectedJ79ID == 0)
            {
                return this.StopPage(false, "Nepodařilo se založit výchozí šablonu statistiky.");
            }
            v.SelectedTemplate = v.lisJ79.First(p => p.pid == v.SelectedJ79ID);

            if (Factory.CurrentUser.IsAdmin || v.SelectedTemplate.j02ID == Factory.CurrentUser.pid)
            {
                v.IsAllowEditTemplate = true;
            }
            if (v.SelectedTemplate.j79IsPublic || Factory.j04UserRoleBL.GetList(new BO.myQueryJ04() { j79id = v.SelectedJ79ID }).Count() > 0)
            {
                v.IsShared = true;
            }


            v.ChartType = v.SelectedTemplate.j79Chart;

            v.TheGridQueryButton = new TheGridQueryViewModel() { j72id = Factory.CBL.LoadUserParamInt("p31totals-j72id"),paramkey= "p31totals-j72id",prefix="p31" };
            if (v.TheGridQueryButton.j72id > 0)
            {
                v.TheGridQueryButton.j72name = Factory.j72TheGridTemplateBL.LoadName(v.TheGridQueryButton.j72id);
            }
            
            v.p31statequery = new p31StateQueryViewModel() { Value = v.SelectedTemplate.j79StateQuery, javascript_onchange = "change_statequery" };

            v.p31tabquery = new p31TabQueryViewModel() { Value = v.SelectedTemplate.j79TabQuery, javascript_onchange = "change_tabquery" };

            v.GridColumns = v.SelectedTemplate.j79Columns;

            


            if (caller == "grid" && !string.IsNullOrEmpty(master_entity) && master_pid > 0)
            {
                Factory.CBL.SaveLastCallingRecPid(master_entity, master_pid, "grid", true, false, null);   //uložit info o naposledy vybraném záznamu v gridu
            }


            GenerateGridColumns(v);
            GenerateDataSource(v);

            return View(v);
        }

        private void GenerateGridColumns(p31TotalsViewModel v)
        {
            if (string.IsNullOrEmpty(v.GridColumns)) return;

            v.lisGridColumns = _colsProvider.ParseTheGridColumns("p31", v.GridColumns, Factory);
            if (v.lisGridColumns.Any(p => p.Header.Contains("L5")))
            {
                v.lisGridColumns.First(p => p.Header.Contains("L5")).Header = this.Factory.getP07Level(5, true);
            }

         
            
        }        

        private void GenerateDataSource(p31TotalsViewModel v)
        {
            if (v.lisGridColumns == null)
            {
                return;
            }
            var finalcols = new List<BO.TheGridColumn>();

            foreach (var col in v.lisGridColumns)
            {
                if (!finalcols.Contains(col))
                {
                    finalcols.Add(col);
                }
            }

            var wheres = new List<string>();
            var mq = new BO.myQueryP31() { explicit_columns = finalcols, MyRecordsDisponible = true };
            foreach (var col in mq.explicit_columns)
            {
                if (col.Prefix != "p31" && col.IsShowTotals)
                {
                    col.IsShowTotals = false;   //aby se nesčítali veličiny vyúčtování/projektů apod.
                }
            }
            switch (v.record_prefix)
            {
                case "p41":
                case "le5":
                    //mq.leindex = 5;mq.lepid = v.record_pid;break;
                    wheres.Add($"a.p41ID={v.record_pid}"); break;
                case "le4":
                case "le3":
                case "le2":
                case "le1":
                    wheres.Add($"p41x.p41ID_P07Level{v.record_prefix.Substring(2, 1)}={v.record_pid}"); break;

                case "p28":
                    wheres.Add($"a.p41ID IN (select p41ID FROM p41Project WHERE p28ID_Client={v.record_pid})");
                    break;
                case "j02":
                    wheres.Add($"a.j02ID={v.record_pid}"); break;
                case "o23":
                    wheres.Add($"a.o23ID={v.record_pid}"); break;
                case "p91":
                    wheres.Add($"a.p91ID={v.record_pid}"); break;
                case "p56":
                    wheres.Add($"a.p56ID={v.record_pid}"); break;

            }
            switch (v.selected_entity)
            {
                case "p41":
                case "le5":
                    wheres.Add($"a.p41ID IN ({v.selected_pids})"); break;
                case "le4":
                case "le3":
                case "le2":
                case "le1":
                    wheres.Add($"p41x.p41ID_P07Level{v.selected_entity.Substring(2, 1)} IN ({v.selected_pids})"); break;

                case "p28":
                    wheres.Add($"a.p41ID IN (select p41ID FROM p41Project WHERE p28ID_Client IN ({v.selected_pids}))");
                    break;
                case "j02":
                    wheres.Add($"a.j02ID IN ({v.selected_pids})"); break;
                case "o23":
                    wheres.Add($"a.o23ID IN ({v.selected_pids})"); break;
                case "p91":
                    wheres.Add($"a.p91ID IN ({v.selected_pids})"); break;
                case "p56":
                    wheres.Add($"a.p56ID IN ({v.selected_pids})"); break;
                case "p31":
                    wheres.Add($"a.p31ID IN ({v.selected_pids})"); break;

            }

          

            if (v.TheGridQueryButton.j72id > 0)
            {
                mq.lisJ73 = Factory.j72TheGridTemplateBL.GetList_j73(v.TheGridQueryButton.j72id, "p31", 0);
            }
            mq.p31statequery = v.p31statequery.Value;
            mq.p31tabquery = v.p31tabquery.Value;
            if (v.periodinput.PeriodValue > 0)
            {
                mq.period_field = v.periodinput.PeriodField;
                mq.global_d1 = v.periodinput.d1;
                mq.global_d2 = v.periodinput.d2;
            }


            if (wheres.Count() > 0)
            {
                mq.explicit_sqlwhere = String.Join(" AND ", wheres);
            }

            mq.explicit_sqlgroupby = string.Join(",", finalcols.Where(p => p.IsShowTotals == false).Select(p => p.getFinalSqlSyntax_GROUPBY()));

            var dt = Factory.gridBL.GetGroupByList(mq);
            
            foreach(var col in mq.explicit_columns.Where(p => p.FieldType == "num"))
            {
                int intDataRows = dt.Rows.Count;
                for (int i= 0;i < intDataRows; i++)
                {
                    if (dt.Rows[i][col.UniqueName] == System.DBNull.Value)
                    {
                        dt.Rows[i][col.UniqueName] = 0.00;
                    }
                }
            }
            
            var intRows = dt.Rows.Count;
            
            var basDataExport = new Code.dataExport();
            var strFullPath = $"{Factory.TempFolder}\\WebDataRocks-{Factory.CurrentUser.j02Login}-{v.SelectedJ79ID}.csv";
            basDataExport.ToCSV(dt, strFullPath, mq, ";", true);

            v.DataSourceLengthBytes = (int)BO.Code.File.GetFileInfo(strFullPath).Length;
            v.DataSourceLengthMegaBytes = BO.Code.File.GetFileInfo(strFullPath).Length / 1024d / 1024d;
            if (v.DataSourceLengthMegaBytes > 1d)
            {
                
                this.AddMessageTranslated($"Maximální limit datového zdroje je 1MB.<hr>Nyní je jeho velikost: {BO.Code.Bas.FormatFileSize(v.DataSourceLengthBytes)}.<hr>Velikost datového zdroje snížíte přes filtrování dat (především časové období) a změnou okruhu sloupců.");
            } 
            
            
        }

      
        
        public IActionResult LoadDatasource(string login,int j79id)
        {
            var strFullPath = $"{Factory.TempFolder}\\WebDataRocks-{login}-{j79id}.csv";

            if (!System.IO.File.Exists(strFullPath))
            {
                return null;
            }

            //var bytes = System.IO.File.ReadAllBytes(v.DataSourceCsvPath);
            //var s = System.IO.File.ReadAllText(strFullPath, Encoding.UTF8);

            return Content(System.IO.File.ReadAllText(strFullPath, Encoding.UTF8), "text/csv", Encoding.UTF8);

            
            //return File(bytes, "text/csv; charset=utf-8");
        }

        public int SaveTabQuery(int j79id, string tabquery)
        {
            var rec = Factory.j79TotalsTemplateBL.Load(j79id);
            rec.j79TabQuery = tabquery;
            return Factory.j79TotalsTemplateBL.Save(rec,null,null);
        }
        public int SaveStateQuery(int j79id, int statequery)
        {
            var rec = Factory.j79TotalsTemplateBL.Load(j79id);
            rec.j79StateQuery = statequery;
            return Factory.j79TotalsTemplateBL.Save(rec,null,null);
        }
        public int ChangeChart(int j79id,int charttype)
        {
            var rec = Factory.j79TotalsTemplateBL.Load(j79id);
            rec.j79Chart = charttype;
          
            return Factory.j79TotalsTemplateBL.Save(rec, null, null);

        }
        public int SaveSettings(int j79id, string j02ids, string j07ids, string j11ids, int j72id,string addquery)
        {
            Factory.CBL.SetUserParam($"p31totals-j72id", j72id.ToString());

            var rec = Factory.j79TotalsTemplateBL.Load(j79id);
            rec.j79Query_j02IDs = j02ids;
            rec.j79Query_j11IDs = j11ids;
            rec.j79Query_j07IDs = j07ids;
            rec.j79AddQuery = addquery;
            
            return Factory.j79TotalsTemplateBL.Save(rec,null,null);
        }

        public int SaveColumns(int j79id, string cols)
        {
            var rec = Factory.j79TotalsTemplateBL.Load(j79id);
            rec.j79Columns = cols;
            return Factory.j79TotalsTemplateBL.Save(rec,null,null);
        }

        public int SaveReportSetting(int j79id, string setting)
        {
            var rec = Factory.j79TotalsTemplateBL.Load(j79id);
            rec.j79WebDataRocksReport = setting;
            return Factory.j79TotalsTemplateBL.Save(rec, null, null);
        }

        public int SaveAs(string j79name, int j79id, string masterprefix)
        {
            var rec = Factory.j79TotalsTemplateBL.Load(j79id);
            rec.pid = 0;
            rec.j02ID = Factory.CurrentUser.pid;
            rec.j79Name = j79name;
            rec.j79IsPublic = false;
            rec.j79IsSystem = false;
            rec.j79IsWebDataRocks = true;

            int intJ79ID = Factory.j79TotalsTemplateBL.Save(rec,null,null);
            if (intJ79ID > 0)
            {
                Factory.CBL.SetUserParam($"p31totals-{masterprefix}-j79id", intJ79ID.ToString());
            }
            return intJ79ID;
        }
        public int Rename(string j79name, int j79id)
        {
            var rec = Factory.j79TotalsTemplateBL.Load(j79id);
            rec.j79Name = j79name;

            return Factory.j79TotalsTemplateBL.Save(rec,null,null);
        }
        public int Delete(int j79id)
        {
            Factory.CBL.DeleteRecord("j79TotalsTemplate", j79id);


            return 1;
        }



        
    }
}
