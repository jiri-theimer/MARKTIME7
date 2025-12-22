using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography.Xml;
using UI.Models;
using UI.Models.p31oper;
using UI.Models.p31view;
using UI.Models.Record;

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
            var v = new totalsViewModel() { record_pid = master_pid, record_prefix = master_entity, SelectedJ79ID = j79id, selected_entity = selected_entity, selected_pids = selected_pids };
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

            v.lisJ79 = Factory.j79TotalsTemplateBL.GetList(Factory.CurrentUser.pid, v.record_prefix);

            if (!v.lisJ79.Any(p => p.j02ID == Factory.CurrentUser.pid && p.j79IsSystem))
            {
                Factory.j79TotalsTemplateBL.CreateDefaultSysRecord(Factory.CurrentUser.pid, v.record_prefix);
                v.lisJ79 = Factory.j79TotalsTemplateBL.GetList(Factory.CurrentUser.pid, v.record_prefix);
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

            v.TheGridQueryButton = new TheGridQueryViewModel() { j72id = Factory.CBL.LoadUserParamInt("p31totals-j72id"),paramkey= "p31totals-j72id",prefix="p31" };
            if (v.TheGridQueryButton.j72id > 0)
            {
                v.TheGridQueryButton.j72name = Factory.j72TheGridTemplateBL.LoadName(v.TheGridQueryButton.j72id);
            }
            
            v.p31statequery = new p31StateQueryViewModel() { Value = v.SelectedTemplate.j79StateQuery, javascript_onchange = "change_statequery" };

            v.p31tabquery = new p31TabQueryViewModel() { Value = v.SelectedTemplate.j79TabQuery, javascript_onchange = "change_tabquery" };

            v.GridColumns = v.SelectedTemplate.j79Columns;

            v.GroupField1 = v.SelectedTemplate.j79GroupField1;
            v.GroupField2 = v.SelectedTemplate.j79GroupField2;
            v.GroupField3 = v.SelectedTemplate.j79GroupField3;
            v.PivotField = v.SelectedTemplate.j79PivotField;
            v.PivotValue = v.SelectedTemplate.j79PivotValue;

            v.j02IDs = v.SelectedTemplate.j79Query_j02IDs;
            if (v.j02IDs != null)
            {
                var j02ids = BO.Code.Bas.ConvertString2ListInt(v.j02IDs);
                var lis = Factory.j02UserBL.GetList(new BO.myQueryJ02() { pids = j02ids });
                v.SelectedPersons = string.Join(",", lis.Select(p => p.FullnameDesc));
            }

            v.j07IDs = v.SelectedTemplate.j79Query_j07IDs;
            if (v.j07IDs != null)
            {
                var j07ids = BO.Code.Bas.ConvertString2ListInt(v.j07IDs);
                var lis = Factory.j07PersonPositionBL.GetList(new BO.myQuery("j07") { pids = j07ids });
                v.SelectedPositions = string.Join(",", lis.Select(p => p.j07Name));
            }

            v.j11IDs = v.SelectedTemplate.j79Query_j11IDs;
            if (v.j11IDs != null)
            {
                var j11ids = BO.Code.Bas.ConvertString2ListInt(v.j11IDs);
                var lis = Factory.j11TeamBL.GetList(new BO.myQueryJ11() { pids = j11ids });
                v.SelectedTeams = string.Join(",", lis.Select(p => p.j11Name));
            }

            if (caller == "grid" && !string.IsNullOrEmpty(master_entity) && master_pid > 0)
            {
                Factory.CBL.SaveLastCallingRecPid(master_entity, master_pid, "grid", true, false, null);   //uložit info o naposledy vybraném záznamu v gridu
            }


            GenerateOutput(v);

            return View(v);
        }

        private void GenerateOutput(totalsViewModel v)
        {
            if (string.IsNullOrEmpty(v.GridColumns)) return;

            

            v.lisGridColumns = _colsProvider.ParseTheGridColumns("p31", v.GridColumns, Factory); //.Where(p => !p.IsNotUseP31TOTALS).ToList();
            if (v.lisGridColumns.Any(p => p.Header.Contains("L5")))
            {
                v.lisGridColumns.First(p => p.Header.Contains("L5")).Header = this.Factory.getP07Level(5, true);
            }

            if (!v.IsAllowEditTemplate && (v.lisGridColumns.Count() != BO.Code.Bas.ConvertString2List(v.SelectedTemplate.j79Columns, ",").Count()))
            {
                this.AddMessageTranslated("V této sdílené statistice nevidíte všechny údaje, protože nemáte plné oprávnění k reportovaným úkonům.", "info");
            }
            

            var groups = new List<BO.TheGridColumn>();
            if (!string.IsNullOrEmpty(v.GroupField1) && v.lisGridColumns.Any(p => p.UniqueName == v.GroupField1))
            {
                groups.Add(v.lisGridColumns.First(p => p.UniqueName == v.GroupField1));

            }
            if (!string.IsNullOrEmpty(v.GroupField2) && v.lisGridColumns.Any(p => p.UniqueName == v.GroupField2))
            {
                groups.Add(v.lisGridColumns.First(p => p.UniqueName == v.GroupField2));
            }
            if (!string.IsNullOrEmpty(v.GroupField3) && v.lisGridColumns.Any(p => p.UniqueName == v.GroupField3))
            {
                groups.Add(v.lisGridColumns.First(p => p.UniqueName == v.GroupField3));
            }

            BO.TheGridColumn pf = null; //pivot-field
            if (v.PivotField != null && v.PivotValue != null)
            {
                try
                {
                    pf = v.lisGridColumns.Where(p => p.UniqueName == v.PivotField).First();
                }
                catch
                {

                }

            }

            var finalcols = new List<BO.TheGridColumn>();
            finalcols.AddRange(groups);
            foreach (var col in v.lisGridColumns)
            {
                if (!finalcols.Contains(col))
                {
                    finalcols.Add(col);
                }
            }
            //if (v.PivotField != null && v.PivotValue != null && finalcols.Any(p => p.UniqueName == v.PivotField))
            //{
            //    finalcols.Remove(finalcols.Where(p => p.UniqueName == v.PivotField).First());
            //}

            var wheres = new List<string>();
            var mq = new BO.myQueryP31() { explicit_columns = finalcols, MyRecordsDisponible = true };
            foreach(var col in mq.explicit_columns)
            {
                if (col.Prefix !="p31" && col.IsShowTotals)
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
                    wheres.Add($"p41x.p41ID_P07Level{v.record_prefix.Substring(2,1)}={v.record_pid}"); break;
                    
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
                    wheres.Add($"p41x.p41ID_P07Level{v.selected_entity.Substring(2,1)} IN ({v.selected_pids})"); break;
                    
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
            if (!string.IsNullOrEmpty(v.j02IDs))
            {
                mq.j02ids = BO.Code.Bas.ConvertString2ListInt(v.j02IDs);
            }
            if (!string.IsNullOrEmpty(v.j07IDs))
            {
                mq.j07ids = BO.Code.Bas.ConvertString2ListInt(v.j07IDs);
            }
            if (!string.IsNullOrEmpty(v.j11IDs))
            {
                mq.j11ids = BO.Code.Bas.ConvertString2ListInt(v.j11IDs);
            }

            if (v.SelectedTemplate.j79AddQuery != null)
            {
                var lis = BO.Code.Bas.ConvertString2List(v.SelectedTemplate.j79AddQuery, "|");
                foreach(var s in lis)
                {
                    var arr = s.Split("###");
                    if (!string.IsNullOrEmpty(arr[1]))
                    {
                        var col = v.lisGridColumns.First(p => p.UniqueName == arr[0]);
                        
                        var vals = arr[1].Split(";");
                        var likes = new List<string>();
                        for (int i = 0; i <= vals.Length - 1; i++)
                        {
                            if (!string.IsNullOrEmpty(vals[i]))
                            {
                                likes.Add($"{col.getFinalSqlSyntax_WHERE()} LIKE '%{vals[i].Trim()}%'");
                            }

                        }
                        wheres.Add($"({string.Join(" OR ", likes)})");

                    }
                }
            }
           
            if (wheres.Count() > 0)
            {
                mq.explicit_sqlwhere = String.Join(" AND ", wheres);
            }



            mq.explicit_orderby = string.Join(",", finalcols.Where(p => p.IsShowTotals == false && p != pf).Select(p => p.getFinalSqlSyntax_GROUPBY_ORDERBY()));

            mq.explicit_sqlgroupby = string.Join(",", finalcols.Where(p => p.IsShowTotals == false).Select(p => p.getFinalSqlSyntax_GROUPBY()));

            //mq.explicit_orderby += ",a__p31Worksheet__p31Hours_Orig desc";


            var dt = Factory.gridBL.GetGroupByList(mq);
            

            var def = new BL.Code.HtmlTable() { IsCanExport = true };

            
            def.ColHeaders = string.Join("|", finalcols.Where(p => p != pf).Select(p => p.Header));
            if (Factory.CurrentUser.j02DefaultHoursFormat == "T")
            {
                //hodiny budou v HH:MM
                var lis = new List<string>();
                foreach(var col in finalcols)
                {
                    if (col.IsHours)
                    {
                        lis.Add("HHMM");
                    }
                    else
                    {
                        lis.Add(col.HtmlTableType);
                    }
                }
                def.ColTypes = string.Join("|", lis);
            }
            else
            {
                def.ColTypes = string.Join("|", finalcols.Where(p => p != pf).Select(p => p.HtmlTableType));
            }
            
            def.ColHeaderStyles = string.Join("|", finalcols.Where(p => p != pf).Select(p => p.Tooltip));


            var subtotals = new List<string>();
            foreach (var col in finalcols.Where(p => p != pf))
            {
                if (groups.Contains(col))
                {
                    subtotals.Add("1");
                }
                else
                {
                    if (col.IsShowTotals)
                    {
                        subtotals.Add("11");
                    }
                    else
                    {
                        subtotals.Add("0");
                    }
                }
            }
            def.ColFlexSubtotals = String.Join("|", subtotals);

            BO.TheGridColumn pv = null;
            try
            {
                pv = v.lisGridColumns.Where(p => p.UniqueName == v.PivotValue).First();
            }
            catch
            {

            }

            if (pf != null && pv !=null)
            {
                var pivots_sql = new List<string>();
                try
                {
                    dt.DefaultView.Sort = v.PivotField;
                }
                catch
                {
                    return;
                }
                
                var dtdist = dt.DefaultView.ToTable(true, v.PivotField);

                int xx = 0;
                foreach (System.Data.DataRow dbrow in dtdist.Rows)
                {
                    xx += 1;
                    if (xx > 50)
                    {
                        this.AddMessageTranslated("Maximální počet PIVOT sloupců je 50!", "warning"); break;
                    }
                    
                    def.ColTypes += "|N";
                    def.ColFlexSubtotals += "|11";
                    def.ColHeaderStyles += "|background-color:khaki";
                    if (dbrow[v.PivotField] == System.DBNull.Value)
                    {
                        pivots_sql.Add($"SUM(CASE WHEN {pf.getFinalSqlSyntax_WHERE()} IS NULL THEN {pv.getFinalSqlSyntax_WHERE()} END)");
                        def.ColHeaders += $"|---";
                    }
                    else
                    {
                        switch (pf.FieldType)
                        {
                            case "date":
                            case "datetime":
                                pivots_sql.Add($"SUM(CASE WHEN {pf.getFinalSqlSyntax_WHERE()} = convert(datetime,'{dbrow[v.PivotField]}',104) THEN {pv.getFinalSqlSyntax_WHERE()} END) AS PIVOT{xx}");
                                break;
                            case "string":
                                pivots_sql.Add($"SUM(CASE WHEN {pf.getFinalSqlSyntax_WHERE()} = {BO.Code.Bas.GS(dbrow[v.PivotField].ToString())} THEN {pv.getFinalSqlSyntax_WHERE()} END) AS PIVOT{xx}");
                                break;
                            default:
                                pivots_sql.Add($"SUM(CASE WHEN {pf.getFinalSqlSyntax_WHERE()} = {dbrow[v.PivotField]} THEN {pv.getFinalSqlSyntax_WHERE()} END) AS PIVOT{xx}");
                                break;
                        }
                      
                        def.ColHeaders += $"|{BO.Code.Bas.OM2(dbrow[v.PivotField].ToString(), 20)}";
                    }
                }
                mq.explicit_sqlgroupby = string.Join(",", finalcols.Where(p => p.IsShowTotals == false && p != pf).Select(p => p.getFinalSqlSyntax_GROUPBY()));
                

                var xxx = finalcols.Where(p => p != pf);
                var strFirstCols = string.Join(",", finalcols.Where(p => p != pf).Select(p => p.getFinalSqlSyntax_GROUPBY_SELECT()));
               
                if (pivots_sql.Count()>0)
                {
                    mq.explicit_selectsql = string.Join(",", pivots_sql);
                }
                

                dt = Factory.gridBL.GetGroupByList(mq, strFirstCols);

                
                if (!v.SelectedTemplate.j79IsSystem)
                {
                    def.TableCaption = v.SelectedTemplate.j79Name;
                }                
                if (pf != null && pv != null)
                {
                    def.TableAfterCaption = $"PIVOT: {pf.Header} ({pv.Header})";
                }
            }

            def.Sql = Factory.gridBL.GetLastFinalSql();
            //BO.Code.File.LogInfo(Factory.gridBL.GetLastFinalSql()); //logovat výsledný sql dotaz
            if (v.periodinput.PeriodValue > 0)
            {
                def.Sql = def.Sql.Replace("@d1", BO.Code.Bas.GD(v.periodinput.d1));
                def.Sql = def.Sql.Replace("@d2", BO.Code.Bas.GD(v.periodinput.d2));
            }

            def.IsUseDatatables = false;
            def.IsFullWidth = false;
            def.MailSubject = v.lisJ79.Where(p => p.pid == v.SelectedJ79ID).First().j79Name;
          
            
            var generator = new BL.Code.Datatable2Html(def, Factory);
            try
            {
                v.HtmlVystup = generator.CreateHtmlTable(dt);
            }catch(Exception ex)
            {
                this.AddMessageTranslated(ex.Message);
            }
            
            v.TableClientID = generator.TableClientID;

            if (dt.Rows.Count == 0)
            {
                v.HtmlVystup = $"<span class='badge-def'>{DateTime.Now}</span><code> {Factory.tra("Pro zadaný filtr neexistují úkony.")}</code>";
            }




            //BO.Code.File.LogInfo(Factory.gridBL.GetLastFinalSql());

            
        }
        public int SavePivotField(int j79id, string pivotfield)
        {
            var rec = Factory.j79TotalsTemplateBL.Load(j79id);
            rec.j79PivotField = pivotfield;
            return Factory.j79TotalsTemplateBL.Save(rec,null,null);
        }
        public int SavePivotValue(int j79id, string pivotvalue)
        {
            var rec = Factory.j79TotalsTemplateBL.Load(j79id);
            rec.j79PivotValue = pivotvalue;
            return Factory.j79TotalsTemplateBL.Save(rec,null,null);
        }
        public int SaveGroupField(int j79id, int groupfield_index, string groupfield_value)
        {
            var rec = Factory.j79TotalsTemplateBL.Load(j79id);
            switch (groupfield_index)
            {
                case 1:
                    rec.j79GroupField1 = groupfield_value;
                    break;
                case 2:
                    rec.j79GroupField2 = groupfield_value;
                    break;
                case 3:
                    rec.j79GroupField3 = groupfield_value;
                    break;
            }

            return Factory.j79TotalsTemplateBL.Save(rec,null,null);
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

        public int SaveAs(string j79name, int j79id, string masterprefix)
        {
            var rec = Factory.j79TotalsTemplateBL.Load(j79id);
            rec.pid = 0;
            rec.j02ID = Factory.CurrentUser.pid;
            rec.j79Name = j79name;
            rec.j79IsPublic = false;
            rec.j79IsSystem = false;


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



        //private void Handle_GridOutput(totalsViewModel v,BO.myQueryP31 mq)
        //{
           
        //    v.gridinput = new Views.Shared.Components.myGrid.myGridInput() { is_enable_selecting = false, entity = "p31Worksheet", oncmclick = "", ondblclick = "" };

        //    //mq.explicit_selectsql = string.Join(",", v.lisGridColumns.Select(p => p.getFinalSqlSyntax_GROUPBY_SELECT()));

            
        //    mq.explicit_selectsql = "MIN(a.p31ID) as pid,0 as isclosed,NULL as p72ID_AfterApprove,NULL as p72ID_AfterTrimming,NULL as p70ID,NULL as p71ID,NULL as p91ID,0 as p91IsDraft,0 as p33ID,0 as p34IncomeStatementFlag,0 as p32IsBillable,NULL as p31Rate_Billing_Orig,NULL as p31Amount_WithoutVat_Orig,NULL as p41BillingFlag,NULL as p31ExcludeBillingFlag,NULL as b05ID_Last,NULL as p31RowColorFlag,NULL as o23ID";

        //    v.gridinput.query = mq;

        //    v.gridinput.master_entity = "inform";
            
        //    v.gridinput.fixedcolumns = v.GridColumns;
        //}
    }
}
