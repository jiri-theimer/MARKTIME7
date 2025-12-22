
using BO;
using BO.Model.Grid;

using System.Data;
using System.Threading.Tasks.Dataflow;


namespace UI.Views.Shared.Components.myGrid
{
    public class myGridSupport
    {
        private System.Text.StringBuilder _s;
        private readonly BL.TheColumnsProvider _colsProvider;
        private BL.Factory _Factory { get; set; }
        private myGridViewModel _grid;
        private List<string> _columnswidth { get; set; }

        public myGridInput gridinput { get; set; }

        public myGridSupport(myGridInput input, BL.Factory f, BL.TheColumnsProvider cp)
        {
            _Factory = f;
            _colsProvider = cp;
            gridinput = input;
            gridinput.tablelayout = new HtmlTableLayout(f.CurrentUser.j02GridCssBitStream, f.CurrentUser.j02FontSizeFlag, true);
        }

        public myGridOutput GetFirstData(TheGridState gridState) //vrátí grid html pro úvodní načtení na stránku
        {
            if (gridState == null)
            {
                return render_thegrid_error("gridState is null!");
            }

            if (!string.IsNullOrEmpty(this.gridinput.fixedcolumns))
            {
                gridState.j72Columns = this.gridinput.fixedcolumns;
            }

            return render_thegrid_html(gridState);
        }



        public myGridOutput Event_HandleTheGridOper(myGridUIContext tgi)     //grid událost: třídění, změna stránky a pagesize
        {

            var gridState = _Factory.j72TheGridTemplateBL.LoadState(tgi.j72id, _Factory.CurrentUser.pid);   //načtení naposledy uloženého grid stavu uživatele

            if (!string.IsNullOrEmpty(this.gridinput.fixedcolumns))
            {
                gridState.j72Columns = this.gridinput.fixedcolumns;
            }
            switch (tgi.key)
            {
                case "pagerindex":
                    gridState.j75CurrentPagerIndex = BO.Code.Bas.InInt(tgi.value);
                    break;
                case "pagesize":
                    gridState.j75PageSize = BO.Code.Bas.InInt(tgi.value);
                    break;
                case "operatorflag":
                    gridState.j75OperatorFlag = BO.Code.Bas.InInt(tgi.value);
                    break;
                case "sortfield":
                    gridState.j75CurrentRecordPid = tgi.currentpid; //záznam, na které uživatel v gridu zrovna stojí
                    if (gridState.j75SortDataField != tgi.value)
                    {
                        gridState.j75SortOrder = "asc";
                        gridState.j75SortDataField = tgi.value;
                    }
                    else
                    {
                        if (gridState.j75SortOrder == "desc")
                        {
                            gridState.j75SortDataField = "";//vyčisitt třídění, třetí stav
                            gridState.j75SortOrder = "";
                        }
                        else
                        {
                            if (gridState.j75SortOrder == "asc")
                            {
                                gridState.j75SortOrder = "desc";
                            }
                        }
                    }


                    break;
                case "groupfield":
                    if (gridState.j75GroupDataField != null && tgi.value == gridState.j75GroupDataField)
                    {
                        //vyčištění souhrnu
                        gridState.j75GroupDataField = null;
                    }
                    else
                    {
                        gridState.j75GroupDataField = tgi.value;
                    }
                    gridState.j75GroupLastValue = null; //pro jistotu vyčistit hodnotu filtru groupby
                    gridState.j75CurrentPagerIndex = 0; //pro jistotu vyčistit
                    break;
                case "groupvalue":
                    if (gridState.j75GroupLastValue != null && tgi.value == gridState.j75GroupLastValue)
                    {
                        //vyčištění hodnoty filtrovaného souhrnu
                        gridState.j75GroupLastValue = null;
                    }
                    else
                    {
                        gridState.j75GroupLastValue = tgi.value;
                    }
                    gridState.j75CurrentPagerIndex = 0; //pro jistotu vyčistit
                    break;
                case "filter":
                    break;  //samostatná událost Event_HandleTheGridFilter
                case "reload":
                case "reload_hardrefresh":
                    break;  //nic
            }

            if (_Factory.j72TheGridTemplateBL.SaveState(gridState, _Factory.CurrentUser.pid) > 0)   //uložení změny grid stavu
            {
                if (tgi.oper == "operatorflag")
                {
                    gridState = _Factory.j72TheGridTemplateBL.LoadState(gridState.j72ID, _Factory.CurrentUser.pid);
                }
                return render_thegrid_html(gridState);
            }
            else
            {
                return render_thegrid_error("Nepodařilo se uložit GRIDSTATE");
            }


        }

        public myGridOutput Event_HandleTheGridFilter(myGridUIContext tgi, List<BO.TheGridColumnFilter> filter)    //grid událost: Změna sloupcového filtru
        {

            var gridState = _Factory.j72TheGridTemplateBL.LoadState(tgi.j72id, _Factory.CurrentUser.pid);


            if (string.IsNullOrEmpty(this.gridinput.fixedcolumns) == false)
            {
                gridState.j72Columns = this.gridinput.fixedcolumns;
            }
            var lis = new List<string>();
            foreach (var c in filter)
            {
                lis.Add(c.field + "###" + c.oper + "###" + c.value);

            }
            gridState.j75CurrentPagerIndex = 0; //po změně filtrovací podmínky je nutné vyčistit paměť stránky
            gridState.j75CurrentRecordPid = 0;

            gridState.j75Filter = string.Join("$$$", lis);

            if (_Factory.j72TheGridTemplateBL.SaveState(gridState, _Factory.CurrentUser.pid) > 0)
            {
                return render_thegrid_html(gridState);
            }
            else
            {
                return render_thegrid_error("Nepodařilo se zpracovat filtrovací podmínku.");
            }
        }


        private myGridOutput render_thegrid_html(BO.TheGridState gridState) //vrací kompletní html gridu: header+body+footer+message
        {
            var ret = new myGridOutput();
            _grid = new myGridViewModel();
            _grid.GridState = gridState;


            if (gridinput.tablelayout.Resize)
            {
                _columnswidth = BO.Code.Bas.ConvertString2List(_grid.GridState.j75ColumnsGridWidth);    //uživatel si nastavuje šířky sloupců
            }
            ret.operatorflag = gridState.j75OperatorFlag;

            BO.baseQuery mq = this.gridinput.query;

            ret.sortfield = gridState.j75SortDataField;
            ret.sortdir = gridState.j75SortOrder;

            _grid.Columns = _colsProvider.ParseTheGridColumns(mq.Prefix, gridState.j72Columns, _Factory);
            if (gridState.j72MasterEntity != null) mq.master_prefix = gridState.j72MasterEntity.Substring(0, 3);


            mq.explicit_columns = _grid.Columns.ToList();   //sloupce v přehledu



            if (!this.gridinput.isrecpagegrid)    //v gridu je vypnuté filtrování
            {
                mq.TheGridOperatorFlag = gridState.j75OperatorFlag;

                if (!String.IsNullOrEmpty(gridState.j75Filter))
                {
                    mq.TheGridFilter = _colsProvider.ParseAdhocFilterFromString(gridState.j75Filter, mq.explicit_columns);
                }

                if (gridState.j72HashJ73Query || gridinput.j72id_query > 0)   //v gridu je filtrovací podmínka nebo zapnutý dodatečný pojmenovaný filtr
                {

                    mq.lisJ73 = _Factory.j72TheGridTemplateBL.GetList_j73(gridState.j72ID, gridState.j72Entity.Substring(0, 3), gridinput.j72id_query);

                    _grid.GridMessage = _Factory.j72TheGridTemplateBL.getFiltrAlias(gridState.j72Entity.Substring(0, 3), mq);
                }

            }



            System.Data.DataTable dtGroupBy = null; //případný dataset souhrnů
            //zjistit, zda se vyskytuje souhrn groupby
            BO.TheGridColumn TheGridGroupByCol = null;  //groupby souhrn sloupec
            if (!this.gridinput.isrecpagegrid && _grid.GridState.j75GroupDataField != null && _grid.Columns.Any(p => p.UniqueName == gridState.j75GroupDataField))
            {
                //je nastavený groupby sloupec - je třeba ho přesunout na první místo
                TheGridGroupByCol = _grid.Columns.Where(p => p.UniqueName == gridState.j75GroupDataField).First();

                mq.explicit_sqlgroupby = TheGridGroupByCol.getFinalSqlSyntax_GROUPBY();
                string strSaveExplicitOrderBy = mq.explicit_orderby;
                mq.explicit_orderby = TheGridGroupByCol.getFinalSqlSyntax_GROUPBY_ORDERBY();

                var ss = new List<string>();
                foreach (var col in mq.explicit_columns)
                {

                    if (col == TheGridGroupByCol)
                    {
                        ss.Add(col.getFinalSqlSyntax_GROUPBY_SELECT());
                    }
                    else
                    {
                        ss.Add(col.getFinalSqlSyntax_SUM());
                    }
                }
                dtGroupBy = _Factory.gridBL.GetGroupByList(mq, string.Join(",", ss));

                mq.explicit_sqlgroupby = null;  //nutno vyčistit
                mq.explicit_orderby = strSaveExplicitOrderBy;   //vrátit třídění do původního stavu

                if (_grid.GridState.j75GroupLastValue != null)
                {
                    //je třeba do hlavního přehledu přidat filtr podle hodnoty souhrnu j75GroupLastValue
                    new BO.InitMyQuery(_Factory.CurrentUser).Handle_GroupBy_ExplicitSqlWhere(_grid.GridState, TheGridGroupByCol, mq);

                }

            }

            System.Data.DataTable dtFooter = null;
            int intVirtualRowsCount = 0; bool bolSqlFooterFailed = false;


            if (this.gridinput.isrecpagegrid)
            {
                intVirtualRowsCount = 1;    //u recpage gridu není třeba součtová řádka
            }
            else
            {
                if (dtGroupBy != null && _grid.GridState.j75GroupLastValue == null)  //celkový počet záznamů spočítat ze souhrnů
                {

                    intVirtualRowsCount = dtGroupBy.Rows.Cast<DataRow>().Sum(x => x.Field<int>("RowsCount"));

                }
                else
                {

                    dtFooter = _Factory.gridBL.GetGridTable(mq, true);
                    if (dtFooter.Columns.Count > 0)
                    {
                        intVirtualRowsCount = Convert.ToInt32(dtFooter.Rows[0]["RowsCount"]);
                    }
                    else
                    {
                        bolSqlFooterFailed = true;
                        _Factory.CurrentUser.AddMessage("GRID Error: Dynamic SQL failed.");
                    }
                }

            }



            if (intVirtualRowsCount > 500 || bolSqlFooterFailed)
            {   //dotazy nad 500 záznamů budou mít zapnutý OFFSET režim stránkování
                mq.OFFSET_PageSize = gridState.j75PageSize;
                mq.OFFSET_PageNum = gridState.j75CurrentPagerIndex / gridState.j75PageSize;
            }

            if (!this.gridinput.isrecpagegrid)  //v recpage gridu se netřídí
            {
                //třídění řešit až po spuštění FOOTER summary DOTAZu
                if (!bolSqlFooterFailed && String.IsNullOrEmpty(gridState.j75SortDataField) == false && _grid.Columns.Where(p => p.UniqueName == gridState.j75SortDataField).Count() > 0)
                {
                    var c = _grid.Columns.Where(p => p.UniqueName == gridState.j75SortDataField).First();
                    mq.explicit_orderby = c.getFinalSqlSyntax_ORDERBY() + " " + gridState.j75SortOrder;
                }
            }


            var dt = _Factory.gridBL.GetGridTable(mq);   //hlavní dotaz pro aktuální stránku j75CurrentPagerIndex

            if (_grid.GridState.j75CurrentRecordPid > 0 && intVirtualRowsCount > gridState.j75PageSize) //má se skočit na konkrétní záznam a záznamů je více než na 1 stránku
            {
                System.Data.DataRow[] recs = dt.Select($"pid={_grid.GridState.j75CurrentRecordPid}");
                if (recs.Count() > 0)
                {
                    //záznam existuje v dt: buď je na první stránce nebo v úvodních 500 záznamech, kde se nepoužívá OFFSET
                    var intIndex = dt.Rows.IndexOf(recs[0]);
                    _grid.GridState.j75CurrentPagerIndex = intIndex - (intIndex % _grid.GridState.j75PageSize);
                }
                else
                {
                    if (intVirtualRowsCount > dt.Rows.Count)
                    {
                        //záznam není v dt a celkový počet záznamů je větší než v dt (tedy více než 500). Je třeba najít odpovídající stránku a pro ní vygenerovat datatable                        

                        try
                        {
                            var lisPIDs = _Factory.gridBL.GetListOfFindPid(mq, intVirtualRowsCount);
                            var qryPID = lisPIDs.Where(p => p.pid == _grid.GridState.j75CurrentRecordPid);
                            if (qryPID.Count() > 0)
                            {
                                var intIndex = qryPID.First().rowindex;
                                _grid.GridState.j75CurrentPagerIndex = intIndex - (intIndex % _grid.GridState.j75PageSize);
                                mq.OFFSET_PageNum = gridState.j75CurrentPagerIndex / gridState.j75PageSize;
                                dt = _Factory.gridBL.GetGridTable(mq);   //znovu načíst dotaz kvůli dohledanému záznamu na jiné stránce j75CurrentPagerIndex
                            }
                        }
                        catch
                        {
                            BO.Code.File.LogError($"Selhalo SQL v dohledání záznamu na druhé nebo další stránce tabulky,pid: {_grid.GridState.j75CurrentRecordPid},j75CurrentPagerIndex: {_grid.GridState.j75CurrentPagerIndex}, mq.OFFSET_PageNum: {mq.OFFSET_PageNum}, intVirtualRowsCount: {intVirtualRowsCount}", _Factory.CurrentUser.j02Login, "render_thegrid_html");
                        }


                    }
                }

            }

            _s = new System.Text.StringBuilder();

            if (TheGridGroupByCol != null)   //souhrny groupby
            {
                Render_GROUPS(dtGroupBy, mq, TheGridGroupByCol, dt, _grid.GridState.j75GroupLastValue);
            }
            else
            {
                Render_DATAROWS(dt, mq);
            }



            ret.body = _s.ToString();


            if (!this.gridinput.isrecpagegrid)
            {
                _s = new System.Text.StringBuilder();
                if (dtGroupBy != null && dtFooter == null)
                {
                    Render_TOTALS(dtGroupBy, true);   //součty podle součtů z gridu souhrnů                    
                }
                else
                {
                    Render_TOTALS(dtFooter, false);    //součty klasicky
                }

                ret.foot = _s.ToString();

                ret.virtualrowscount = intVirtualRowsCount;

                _s = new System.Text.StringBuilder();
                if (dtFooter != null)
                {
                    RENDER_PAGER(intVirtualRowsCount);  //pager nezobrazovat, když jsou vidět pouze souhrny
                }

                ret.pager = _s.ToString();

            }
            return ret;
        }



        private void Render_GROUPS(System.Data.DataTable dt, BO.baseQuery mq, BO.TheGridColumn groupcol, System.Data.DataTable dtMain, string strFilterGroupValue)
        {
            int intRows = dt.Rows.Count;
            int intStartIndex = 0;
            int intEndIndex = intRows - 1;
            if (intEndIndex > 1000) intEndIndex = 1000; //omezovač záznamů


            bool bolGroupHasFilter = (strFilterGroupValue != null ? true : false);
            string strBaseClass = "trgroup";

            if (gridinput.tablelayout.Lineheight != null) strBaseClass = $"{strBaseClass} {gridinput.tablelayout.Lineheight}";    //výška řádku line-height, default je 1


            string strGroupVal = null;

            for (int i = intStartIndex; i <= intEndIndex; i++)
            {
                System.Data.DataRow dbRow = dt.Rows[i];
                string strRowClass = strBaseClass;
                strGroupVal = null;
                if (dbRow["GroupByValue"] != System.DBNull.Value)
                {
                    strGroupVal = dbRow["GroupByValue"].ToString();
                }
                if (bolGroupHasFilter)
                {
                    if (strGroupVal == strFilterGroupValue || (strFilterGroupValue == "null" && strGroupVal == null))
                    {
                        strRowClass = "trgroup group_row_active";

                    }
                }


                _s.Append($"<tr id='rg{i}' class='{strRowClass}'>");



                _s.Append("<td class='td0'></td>");


                _s.Append("<td class='td1' style='width:20px;font-size:80%;'>∑</td>");




                _s.Append($"<td class='td2' style='width:20px;'></td>");  //bez hamburger menu



                foreach (TheGridColumn col in _grid.Columns)
                {
                    _s.Append("<td");
                    if (col == groupcol)
                    {
                        if (col.CssClass == null)
                        {
                            _s.Append(" class='group_content'");
                        }
                        else
                        {
                            _s.Append($" class='{col.CssClass} group_content'");
                        }
                    }
                    else
                    {
                        if (col.CssClass != null) _s.Append($" class='{col.CssClass}'");
                    }


                    if (i == intStartIndex)   //první řádek musí mít explicitně šířky, aby to z něj zdědili další řádky
                    {
                        _s.Append(string.Format(" style='width:{0}'", GetColumnWidth(col)));
                    }

                    if (col == groupcol)
                    {
                        _s.Append($"><a class=\"groupvallink\" onclick=\"tg_groupbyval_click(this,{i})\" data-val=\"{strGroupVal}\">{BO.Code.Bas.ParseCellValueFromDb(dbRow, col, "---")}</a><span class='groupscount'>{dbRow["RowsCount"]}</span>");
                    }
                    else
                    {
                        _s.Append($">{BO.Code.Bas.ParseCellValueFromDb(dbRow, col, null, _Factory.CurrentUser.j02DefaultHoursFormat)}");
                    }
                    _s.Append("</td>");

                }

                _s.Append("</tr>");

                if (strRowClass == "trgroup group_row_active")
                {
                    Render_DATAROWS(dtMain, mq);    //grid pro záznamy z filtrovaného souhrnu strFilterGroupValue
                }



            }
            if (intEndIndex >= 1000)
            {
                _s.Append($"<tr><td colspan='{_grid.Columns.Count()}'>Omezovač maximálního počtu řádků: 1000</td></tr>");
            }
        }


        private void Render_DATAROWS(System.Data.DataTable dt, BO.baseQuery mq)
        {
            int intRows = dt.Rows.Count;
            int intStartIndex = 0;
            int intEndIndex = 0;


            if (mq.OFFSET_PageSize > 0)
            {   //Zapnutý OFFSET - pouze jedna stránka díky OFFSET
                intStartIndex = 0;
                intEndIndex = intRows - 1;
            }
            else
            {   //bez OFFSET               
                intStartIndex = _grid.GridState.j75CurrentPagerIndex;
                intEndIndex = intStartIndex + _grid.GridState.j75PageSize - 1;
                if (intEndIndex + 1 > intRows) intEndIndex = intRows - 1;
            }

            string strBaseClass = "selectable"; bool bolStripIsNeeded = gridinput.tablelayout.Stripped; bool bolStripped = false; string strStrippedClass = "stripped";bool bolCanbeTecka = false;
            if (bolStripIsNeeded && mq.Prefix == "p41" || mq.Prefix == "p28" || mq.Prefix == "p56" || mq.Prefix == "j02" || mq.Prefix == "p91" || mq.Prefix == "p90" || mq.Prefix == "o23" || mq.Prefix == "p40" || mq.Prefix == "p84")
            {
                strStrippedClass = $"bg{BO.Code.Entity.GetPrefixDb(mq.Prefix)}";
            }
            if (gridinput.tablelayout.Lineheight != null) strBaseClass = $"{strBaseClass} {gridinput.tablelayout.Lineheight}";    //výška řádku line-height, default je 1

            for (int i = intStartIndex; i <= intEndIndex; i++)
            {
                System.Data.DataRow dbRow = dt.Rows[i];
                string strRowClass = strBaseClass;
                if (Convert.ToBoolean(dbRow["isclosed"]) == true) strRowClass = $"{strRowClass} trbin";



                if (bolStripIsNeeded && !bolStripped) strRowClass = $"{strRowClass} {strStrippedClass}";
                bolStripped = !bolStripped;

                switch (mq.Prefix)
                {
                    case "p31":
                        switch (Convert.ToInt32(dbRow["p33ID"]))
                        {
                            case 2:
                            case 5:
                                if (Convert.ToInt32(dbRow["p34IncomeStatementFlag"]) == 2)
                                {
                                    strRowClass = $"{strRowClass} trfee";   //peněžní/paušální odměna                                    
                                }
                                else
                                {
                                    strRowClass = $"{strRowClass} trexp"; //peněžní výdaj

                                }
                                break;
                            case 3:
                                strRowClass = $"{strRowClass} trpc";    //kusovník                                
                                break;
                        }
                        if (dbRow["p31RowColorFlag"] != System.DBNull.Value)
                        {
                            strRowClass = $"{strRowClass} trbg{dbRow["p31RowColorFlag"]}";   //záznam má nahozenou barvu
                        }




                        break;
                    case "p41":
                        if (dbRow["p41RowColorFlag"] != System.DBNull.Value)
                        {
                            strRowClass = $"{strRowClass} trbg{dbRow["p41RowColorFlag"]}";   //záznam má nahozenou barvu
                        }
                        bolCanbeTecka = true;
                        break;
                    case "p28":
                        if (dbRow["p28RowColorFlag"] != System.DBNull.Value)
                        {
                            strRowClass = $"{strRowClass} trbg{dbRow["p28RowColorFlag"]}";   //záznam má nahozenou barvu
                        }
                        
                        break;
                    case "p91":
                        if (dbRow["p91RowColorFlag"] != System.DBNull.Value)
                        {
                            strRowClass = $"{strRowClass} trbg{dbRow["p91RowColorFlag"]}";   //záznam má nahozenou barvu
                        }
                        bolCanbeTecka = true;
                        break;
                    case "o23":
                        if (dbRow["o23RowColorFlag"] != System.DBNull.Value)
                        {
                            strRowClass = $"{strRowClass} trbg{dbRow["o23RowColorFlag"]}";   //záznam má nahozenou barvu
                        }
                        bolCanbeTecka = true;

                        break;
                    case "o43":
                        if (!Convert.ToBoolean(dbRow["IsSeen"]))
                        {
                            strRowClass = $"{strRowClass} row_is_bold";
                        }
                        if (Convert.ToBoolean(dbRow["IsFlagged"]))
                        {
                            strRowClass = $"{strRowClass} row_is_red";
                        }
                        if (Convert.ToBoolean(dbRow["IsDraft"]))
                        {
                            strRowClass = $"{strRowClass} row_is_dashed";
                        }
                        break;
                    case "p40":

                        if (dbRow["p39DateCreate_MimoDosah"] != System.DBNull.Value && dbRow["ProjektUzavren"] == System.DBNull.Value)
                        {
                            strRowClass = $"{strRowClass} row_is_bgred";
                        }

                        break;
                    case "p56":
                    case "le1":
                    case "le2":
                    case "le3":
                    case "le4":
                    case "le5":
                    
                        bolCanbeTecka = true;
                        break;

                }

                if (string.IsNullOrEmpty(this.gridinput.ondblclick))
                {
                    _s.Append($"<tr id='r{dbRow["pid"]}' class='{strRowClass}'>");
                }
                else
                {
                    _s.Append($"<tr id='r{dbRow["pid"]}' class='{strRowClass}' ondblclick='{this.gridinput.ondblclick}(this)'>");
                }



                if (this.gridinput.is_enable_selecting)
                {
                    _s.Append(string.Format("<td class='td0'><input type='checkbox' style='padding:0px;' id='chk{0}'/></td>", dbRow["pid"]));
                }
                else
                {
                    _s.Append("<td class='td0'></td>");
                }

                if (bolCanbeTecka && dbRow["b02Color"] != System.DBNull.Value)
                {
                    _s.Append("<td class='td1 tecka'");
                }
                else
                {
                    _s.Append("<td class='td1'");
                }


                switch (mq.Prefix)
                {
                    case "p31":
                        _s.Append($" style='width:20px;{UI.Code.TheGridRowSymbol.p31_td_style(dbRow)}'>");
                        _s.Append(UI.Code.TheGridRowSymbol.p31_td_inner(dbRow));
                        if (dbRow["b05ID_Last"] != System.DBNull.Value)
                        {
                            _s.Append($"<span class='material-icons-outlined-nosize'>speaker_notes</span>");
                        }
                        if (dbRow["o23ID"] != System.DBNull.Value)
                        {
                            _s.Append($"<span class='material-icons-outlined-nosize'>file_present</span>");
                        }
                        break;
                    case "p41":
                    case "le5":
                    case "le4":
                    case "le3":
                    case "le2":
                    case "le1":
                        
                        switch ((byte)dbRow["p41BillingFlag"])
                        {
                            case 99:
                                _s.Append(" style='width:20px;'>");
                                _s.Append("<span class='material-icons-outlined-nosize'>local_cafe</span>");
                                break;
                            case 6:
                                _s.Append(" style='width:20px;'>");
                                _s.Append("<span class='material-icons-outlined-nosize'>local_bar</span>");
                                break;
                            default:
                                //if (dbRow["b02Color"] != System.DBNull.Value)
                                //{
                                //    _s.Append($" style='width:20px;background-color:{dbRow["b02Color"]}'");
                                //}
                                _s.Append(">");


                                if ((int)dbRow["p07Level"] < 5)
                                {
                                    _s.Append($"<kbd class='le{dbRow["p07Level"]}'>L{dbRow["p07Level"]}</kbd>");
                                }

                                break;

                        }




                        break;

                    case "p91":
                        _s.Append($" style='width:20px;{UI.Code.TheGridRowSymbol.p91_td_style(dbRow)}'");
                        //if (dbRow["b02Color"] == System.DBNull.Value)
                        //{
                        //    _s.Append($" style='width:20px;{UI.Code.TheGridRowSymbol.p91_td_style(dbRow)}'");
                        //}
                        //else
                        //{
                        //    _s.Append($" style='width:20px;background-color:{dbRow["b02Color"]}'");
                        //}
                        //_s.Append($" style='width:20px'");
                        _s.Append(">");
                        
                        _s.Append(UI.Code.TheGridRowSymbol.p91_td_inner(dbRow));
                        
                        break;
                    case "p56":
                        //if (dbRow["b02Color"] != System.DBNull.Value)
                        //{
                        //    _s.Append($" style='width:20px;background-color:{dbRow["b02Color"]}'");
                        //}
                        _s.Append(">");
                        
                        break;
                    case "o23":
                        _s.Append(" style='width:20px");
                        //if (dbRow["b02Color"] != System.DBNull.Value)
                        //{
                        //    _s.Append($";background-color:{dbRow["b02Color"]}");
                        //}
                        _s.Append("'>");
                        if (dbRow["o27ID_Last"] != System.DBNull.Value)
                        {

                            _s.Append("<span class='material-icons-outlined-nosize'>attachment</span>");
                        }
                        

                        break;
                    case "o22":
                        if (dbRow["o21Color"] != System.DBNull.Value)
                        {
                            _s.Append($" style='width:20px;background-color:{dbRow["o21Color"]}'");
                        }
                        _s.Append(">");
                        break;
                    case "o43":
                        _s.Append(" style='width:20px;'>");
                        if (Convert.ToInt32(dbRow["AttachmentsCount"]) > 0)
                        {

                            _s.Append("<span class='material-icons-outlined-nosize'>attachment</span>");
                        }

                        break;
                    case "x31":
                        _s.Append(">");
                        if (Convert.ToBoolean(dbRow["IsScheduling"]))
                        {
                            _s.Append("<span class='material-icons-outlined-nosize' style='color:red;'>notifications</span>");
                        }

                        break;
                    case "p40":
                        _s.Append(" style='width:20px");
                        if (dbRow["Vyprsel"] != System.DBNull.Value || dbRow["ProjektUzavren"] != System.DBNull.Value)
                        {

                            if (dbRow["ProjektUzavren"] != System.DBNull.Value)
                            {
                                _s.Append(";background-color:black;");
                            }
                            _s.Append("'>");
                            if (dbRow["Vyprsel"] != System.DBNull.Value)
                            {
                                _s.Append("<span class='material-icons-outlined-nosize' style='color:#FFC107;'>warning</span>");
                            }


                        }
                        else
                        {
                            _s.Append("'>");
                        }



                        break;
                    case "j02":
                        _s.Append(" style='width:20px;'>");

                        if (Convert.ToBoolean(dbRow["IsLoginManualLocked"]))
                        {

                            _s.Append("<span class='material-icons-outlined-nosize' style='color:red;'>block</span>");
                        }
                        else
                        {
                            if (Convert.ToBoolean(dbRow["IsLoginAutoLocked"]))
                            {

                                _s.Append("<span class='material-icons-outlined-nosize' style='color:red;'>warning</span>");
                            }
                        }
                        if (dbRow["j02VirtualParentID"] != System.DBNull.Value)
                        {

                            _s.Append("<span class='material-icons-outlined-nosize'>domino_mask</span>");

                        }

                        break;
                    case "b05":
                        if (dbRow["tab1flag4"] != System.DBNull.Value && Convert.ToInt32(dbRow["tab1flag4"]) == 4)     //fakturační poznámka
                        {
                            _s.Append($" style='width:20px;background-color:#DFFFE9'");
                        }
                        _s.Append(">");
                        break;
                    case "p49":
                        if (dbRow["strana"].ToString() == "P")
                        {
                            _s.Append($" style='width:20px;background-color:#DFFFE9'>");    //příjem
                        }
                        else
                        {
                            _s.Append($" style='width:20px;background-color:#FFCCCB'>");    //výdej
                        }
                        if (Convert.ToInt32(dbRow["p49StatusFlag"]) == 1)
                        {
                            _s.Append("<span class='material-icons-outlined-nosize'>flight</span>");
                        }


                        break;
                    default:
                        _s.Append(" style='width:20px;'>");
                        break;
                }

                if (bolCanbeTecka && dbRow["b02Color"] != System.DBNull.Value)
                {
                    _s.Append($"<span class='dot' style='background-color:{dbRow["b02Color"]}'></span>");
                }


                _s.Append("</td>");

                strRowClass = "td2";
                if (mq.Prefix == "p31")
                {
                    if (!Convert.ToBoolean(dbRow["p32IsBillable"]))
                    {
                        strRowClass += " nefa"; //nefakturovatelný úkon podle aktivity
                    }
                    else
                    {
                        switch ((byte)dbRow["p41BillingFlag"])
                        {
                            case 6:
                                strRowClass += " nusa6"; //projekt nastavený na p41BillingFlag=6
                                break;
                            case 99:
                                strRowClass += " nusa99"; //projekt nastavený na p41BillingFlag=99
                                break;
                            default:
                                if (Convert.ToInt32(dbRow["p33ID"]) == 1 && dbRow["p71ID"] == System.DBNull.Value && Convert.ToDouble(dbRow["p31Amount_WithoutVat_Orig"]) == 0)
                                {
                                    strRowClass += " nusa"; //fakturovatelný čas s nulovou hodinovou sazbou

                                }
                                break;
                        }


                    }

                }
                if (!string.IsNullOrEmpty(this.gridinput.oncmclick))
                {
                    _s.Append($"<td class='{strRowClass}' style='width:20px;'><a class='cm' onclick='{this.gridinput.oncmclick}'>&#9776;</a></td>");      //hamburger menu
                }
                else
                {
                    _s.Append($"<td class='{strRowClass}' style='width:20px;'></td>");  //bez hamburger menu
                }
                switch (this.gridinput.reczoomflag)
                {
                    case 1:
                    case 2:
                        _s.Append($"<td style='width:20px;'><a class='reczoom' data-pos='top left' data-rel='/p31approve/Record?p31id={dbRow["pid"]}&p31guid={dbRow["p31Guid"]}' data-height='215' data-width='1000px' data-visibility='visible'>ℹ</a></td>");
                        break;
                    default:
                        break;
                }

                foreach (TheGridColumn col in _grid.Columns)
                {

                    _s.Append("<td");
                    if (col.CssClass != null)
                    {
                        _s.Append(string.Format(" class='{0}'", col.CssClass));
                    }

                    if (i == intStartIndex)   //první řádek musí mít explicitně šířky, aby to z něj zdědili další řádky
                    {
                        _s.Append(string.Format(" style='width:{0}'", GetColumnWidth(col)));
                    }
                    _s.Append(string.Format(">{0}</td>", BO.Code.Bas.ParseCellValueFromDb(dbRow, col, null, _Factory.CurrentUser.j02DefaultHoursFormat)));


                }

                _s.Append("</tr>");

            }
        }

        private void Render_TOTALS(System.Data.DataTable dt, bool bolGroupBy)
        {
            if (dt.Columns.Count == 0 || dt.Rows.Count == 0)
            {
                return;
            }
            string strVal;

            if (bolGroupBy)
            {
                strVal = string.Format("{0:#,0}", dt.Rows.Cast<DataRow>().Sum(x => x.Field<int>("RowsCount")));
            }
            else
            {
                strVal = string.Format("{0:#,0}", dt.Rows[0]["RowsCount"]);
            }

            _s.Append("<tr id='tabgrid1_tr_totals'>");
            _s.Append($"<th class='th0' title='{_Factory.tra("Celkový počet záznamů")}' style='width:60px;'><span id='tabgrid1_rows' class='badge bg-primary'>{strVal}</span></th>");
            if (this.gridinput.reczoomflag > 0)
            {
                _s.Append("<th></th>");
            }
            switch (this.gridinput.query.Prefix)
            {
                case "p31":
                    _s.Append($"<input type='hidden' id='tabgrid1_rows_time' value='{dt.Rows[0]["RowsTime"]}'/>");
                    _s.Append($"<input type='hidden' id='tabgrid1_rows_expense' value='{dt.Rows[0]["RowsExpense"]}'/>");
                    _s.Append($"<input type='hidden' id='tabgrid1_rows_fee' value='{dt.Rows[0]["RowsFee"]}'/>");
                    _s.Append($"<input type='hidden' id='tabgrid1_rows_kusovnik' value='{dt.Rows[0]["RowsKusovnik"]}'/>");
                    break;

            }


            foreach (var col in _grid.Columns)
            {
                _s.Append("<th");
                if (col.CssClass != null)
                {
                    _s.Append(string.Format(" class='{0}'", col.CssClass));
                }

                strVal = "&nbsp;";
                if (col.IsShowTotals)
                {
                    if (bolGroupBy)
                    {
                        double nn = 0;
                        foreach (DataRow dbrow in dt.Rows)
                        {
                            if (dbrow[col.UniqueName] != System.DBNull.Value)
                            {
                                try
                                {
                                    nn += (double)dbrow[col.UniqueName];
                                }
                                catch
                                {
                                    try
                                    {
                                        nn += (double)(Int32)dbrow[col.UniqueName];
                                    }
                                    catch
                                    {
                                        //nic
                                    }
                                    
                                }
                                
                            }
                        }
                        strVal = string.Format("{0:#,0.00}", nn);

                    }
                    else
                    {
                        if (dt.Rows[0][col.UniqueName] != System.DBNull.Value)
                        {
                            strVal = BO.Code.Bas.ParseCellValueFromDb(dt.Rows[0], col, null, _Factory.CurrentUser.j02DefaultHoursFormat);
                        }
                    }
                }


                _s.Append($" style='width:{GetColumnWidth(col)}'>{strVal}</th>");



            }
            _s.Append("</tr>");
        }



        private string GetColumnWidth(BO.TheGridColumn col)
        {

            if (_columnswidth != null && _columnswidth.Count() > 0)
            {
                var qry = _columnswidth.Where(p => p.Contains(col.UniqueName));
                if (qry.Count() > 0)
                {
                    return $"{qry.First().Substring(qry.First().Length - 3, 3)}px";
                }
            }
            return gridinput.tablelayout.getGridColumnDefaultWidth(col);

        }


        private void render_select_option(string strValue, string strText, string strSelValue)
        {
            if (strSelValue == strValue)
            {
                _s.Append(string.Format("<option selected value='{0}'>{1}</option>", strValue, strText));
            }
            else
            {
                _s.Append(string.Format("<option value='{0}'>{1}</option>", strValue, strText));
            }

        }

        private void RENDER_PAGER(int intRowsCount) //pager má maximálně 10 čísel, j72PageNum začíná od 0
        {
            int intPageSize = _grid.GridState.j75PageSize;

            _s.Append("<select title='" + _Factory.tra("Stránkování záznamů") + "' style='margin-top:2px;' onchange='tg_pagesize(this)'>");
            render_select_option("50", "50", intPageSize.ToString());
            render_select_option("100", "100", intPageSize.ToString());
            render_select_option("200", "200", intPageSize.ToString());
            render_select_option("500", "500", intPageSize.ToString());
            render_select_option("1000", "1000", intPageSize.ToString());
            _s.Append("</select>");
            if (intRowsCount < 0)
            {
                RenderButtonMore();
                Render_ExtendPagerHtml();
                RenderGridMessage();
                return;
            }

            if (intRowsCount <= intPageSize)
            {
                RenderButtonMore();
                Render_ExtendPagerHtml();
                RenderGridMessage();
                return;
            }

            _s.Append("<button title='" + _Factory.tra("První") + "' class='btn btn-light tgp' style='margin-left:6px;' onclick='tg_pager(\n0\n)'>&lt;&lt;</button>");

            int intCurIndex = _grid.GridState.j75CurrentPagerIndex;
            int intPrevIndex = intCurIndex - intPageSize;
            if (intPrevIndex < 0) intPrevIndex = 0;
            _s.Append(string.Format("<button title='" + _Factory.tra("Předchozí") + "' class='btn btn-light tgp' style='margin-right:10px;' onclick='tg_pager(\n{0}\n)'>&lt;</button>", intPrevIndex));

            if (intCurIndex >= intPageSize * 10)
            {
                intPrevIndex = intCurIndex - 10 * intPageSize;
                _s.Append(string.Format("<button class='btn btn-light tgp' onclick='tg_pager(\n{0}\n)'>...</button>", intPrevIndex));
            }


            int intStartIndex = 0;
            for (int i = 0; i <= intRowsCount; i += intPageSize * 10)
            {
                if (intCurIndex >= i && intCurIndex < i + intPageSize * 10)
                {
                    intStartIndex = i;
                    break;
                }
            }

            int intEndIndex = intStartIndex + (9 * intPageSize);
            if (intEndIndex + 1 > intRowsCount) intEndIndex = intRowsCount - 1;


            int intPageNum = intStartIndex / intPageSize; string strClass;

            for (var i = intStartIndex; i <= intEndIndex; i += intPageSize)
            {
                intPageNum += 1;

                if (intCurIndex >= i && intCurIndex < i + intPageSize)
                {
                    strClass = "btn btn-secondary tgp";
                }
                else
                {
                    strClass = "btn btn-light tgp";
                }

                _s.Append(string.Format("<button type='button' class='{0}' onclick='tg_pager(\n{1}\n)'>{2}</button>", strClass, i, intPageNum));

            }
            if (intEndIndex + 1 < intRowsCount)
            {
                intEndIndex += intPageSize;
                if (intEndIndex + 1 > intRowsCount) intEndIndex = intRowsCount - intPageSize;
                _s.Append(string.Format("<button type='button' class='btn btn-light tgp' onclick='tg_pager(\n{0}\n)'>...</button>", intEndIndex));
            }

            int intNextIndex = intCurIndex + intPageSize;
            if (intNextIndex + 1 > intRowsCount) intNextIndex = intRowsCount - intPageSize;
            _s.Append(string.Format("<button type='button' title='" + _Factory.tra("Další") + "' class='btn btn-light tgp' style='margin-left:10px;' onclick='tg_pager(\n{0}\n)'>&gt;</button>", intNextIndex));

            int intLastIndex = intRowsCount - (intRowsCount % intPageSize);  //% je zbytek po celočíselném dělení
            _s.Append(string.Format("<button type='button' title='" + _Factory.tra("Poslední") + "' class='btn btn-light tgp' onclick='tg_pager(\n{0}\n)'>&gt;&gt;</button>", intLastIndex));

            RenderButtonMore();
            Render_ExtendPagerHtml();
            RenderGridMessage();
        }

        private void RenderButtonMore()
        {
            //_s.Append("<button type='button' class='btn btn-outline-secondary' title='GRID records' style='margin-left:6px;padding-top:0px;padding-bottom:0px;' onclick='tg_button_more(this)'><i class='material-icons-outlined-btn'>library_add_check</i></button>");
            //_s.Append("<a title='Zaškrtnout hromadně' style='margin-left:6px;' href='#' onclick='tg_button_more(this)'><i class='material-icons-outlined-btn'>library_add_check</i></a>");
            _s.Append("<span id='gridcurrow'></span>");
        }

        private void RenderGridMessage()
        {
            if (_grid.GridState.j72SystemFlag == BO.j72SystemFlagEnum.User)
            {
                _s.Append("<span id='gridname'>" + _grid.GridState.j72Name + "</span>");
            }

            if (_grid.GridMessage != null)
            {
                // _s.Append("<div class='text-nowrap bd-highlight'>" + _grid.GridMessage + "</div>");
                _s.Append("<span id='gridmessage'>" + _grid.GridMessage + "</span>");
            }
        }
        private void Render_ExtendPagerHtml()
        {
            if (string.IsNullOrEmpty(this.gridinput.extendpagerhtml))
            {
                return;
            }
            _s.Append(this.gridinput.extendpagerhtml);


        }


        private myGridOutput render_thegrid_error(string strError)
        {
            var ret = new myGridOutput();
            ret.message = strError;
            if (_Factory.CurrentUser.Messages4Notify.Count > 0)
            {
                ret.message += " | " + string.Join(",", _Factory.CurrentUser.Messages4Notify.Select(p => p.Value));
            }
            return ret;
        }



        public string Event_HandleTheGridMenu(int j72id)
        {
            var sb = new System.Text.StringBuilder();
            var recJ72 = _Factory.j72TheGridTemplateBL.LoadState(j72id, _Factory.CurrentUser.pid);
            sb.Append("<div style='background-color:white;padding-bottom:20px;'>");
            sb.AppendLine($"<div style='font-weight:bold;text-align:center;'>{_Factory.tra("Seznam pojmenovaných Tabulek")}</div>");

            var lis = _Factory.j72TheGridTemplateBL.GetList(recJ72.j72Entity, recJ72.j02ID, recJ72.j72MasterEntity);
            sb.AppendLine("<table style='width:100%;'>");
            string strGridNavrhar = _Factory.tra("Návrhář sloupců");
            foreach (var c in lis)
            {
                sb.AppendLine("<tr>");
                if (c.j72HashJ73Query)
                {
                    sb.Append("<td style='width:20px;'><span class='material-icons-outlined-nosize text-danger'>filter_alt</span></td>");
                }
                else
                {
                    sb.Append("<td style='width:20px;'><span class='material-icons-outlined-nosize'>grid_on</span></td>");
                }

                switch (c.j72SystemFlag)
                {
                    case j72SystemFlagEnum.Grid:
                        c.j72Name = _Factory.tra("Výchozí Tabulka"); break;
                    default:
                        break;
                }

                if (c.pid == recJ72.pid)
                {
                    c.j72Name += " ✔";
                }
                sb.Append($"<td><a class='nav-link py-0' href='javascript:change_grid({c.pid})'>{c.j72Name}</a></td>");

                sb.AppendLine($"<td style='width:100px;'><a title='{strGridNavrhar}' class='btn btn-sm btn-outline-secondary py-0' href='javascript:_window_open(\"/TheGridDesigner/Index?j72id={c.pid}\",2);'>{_Factory.tra("Návrhář sloupců")}</a></td>");
                sb.AppendLine("</tr>");
            }
            sb.AppendLine("</table>");
            sb.Append("</div>");

            sb.AppendLine("<ul style='border:0px;list-style-type: none;border-top:solid 1px silver;'>");

            sb.AppendLine(string.Format("<li><a class='dropdown-item px-0' href='javascript:tg_gridreport()'><span class='material-icons-outlined' style='width:30px;'>design_services</span>GRID-REPORT</a></li>", j72id));
            sb.AppendLine(string.Format("<li><a class='dropdown-item px-0' href='javascript:tg_export(\"xlsx\")'><span class='material-icons-outlined' style='width:30px;'>cloud_download</span>" + _Factory.tra("MS-EXCEL Export (vše)") + "</a></li>", j72id));
            sb.AppendLine(string.Format("<li><a class='dropdown-item px-0' href='javascript:tg_export(\"csv\")'><span class='material-icons-outlined' style='width:30px;'>cloud_download</span>" + _Factory.tra("CSV Export (vše)") + "</a></li>", j72id));



            sb.AppendLine("</ul>");

            return sb.ToString();
        }


        public myGridExportedFile Event_HandleTheGridExport(string format, int j72id, string pids)
        {
            var gridState = this._Factory.j72TheGridTemplateBL.LoadState(j72id, _Factory.CurrentUser.pid);

            if (!String.IsNullOrEmpty(pids))
            {
                this.gridinput.query.SetPids(pids);
            }


            System.Data.DataTable dt = prepare_datatable_4export(gridState);
            string strTempFileName = BO.Code.Bas.GetGuid();
            string filepath = _Factory.TempFolder + "\\" + strTempFileName + "." + format;

            var cExport = new UI.Code.dataExport();
            string strFileClientName = "gridexport_" + this.gridinput.query.Prefix + "." + format;

            if (format == "csv")
            {
                if (cExport.ToCSV(dt, filepath, this.gridinput.query, ";", true))
                {
                    //return File(System.IO.File.ReadAllBytes(filepath), "application/CSV", strFileClientName);
                    return new myGridExportedFile() { contenttype = "application/CSV", downloadfilename = strFileClientName, tempfilename = strTempFileName + "." + format };


                }
            }
            if (format == "xlsx")
            {
                if (cExport.ToXLSX(dt, filepath, this.gridinput.query))
                {

                    //return File(System.IO.File.ReadAllBytes(filepath), "application/vnd.ms-excel", strFileClientName);
                    return new myGridExportedFile() { contenttype = "application/vnd.ms-excel", downloadfilename = strFileClientName, tempfilename = strTempFileName + "." + format };
                }
            }

            if (format == "clipboard_tabs")
            {
                string s = cExport.ToClipboardTabs(dt, filepath, this.gridinput.query, true);
                return new myGridExportedFile() { contenttype = "clipboard", clipboardcontent = s };
            }
            if (format == "clipboard_html" || format == "email")
            {
                string s = cExport.ToClipboardHtml(dt, filepath, this.gridinput.query, true);
                return new myGridExportedFile() { contenttype = "clipboard", clipboardcontent = s };
            }


            return null;

        }

        private System.Data.DataTable prepare_datatable_4export(BO.TheGridState gridState)
        {
            var mq = this.gridinput.query;

            mq.explicit_columns = _colsProvider.ParseTheGridColumns(mq.Prefix, gridState.j72Columns, _Factory);



            if (!string.IsNullOrEmpty(gridState.j75SortDataField))
            {
                if (mq.explicit_columns.Any(p => p.UniqueName == gridState.j75SortDataField))
                {
                    mq.explicit_orderby = mq.explicit_columns.Where(p => p.UniqueName == gridState.j75SortDataField).First().getFinalSqlSyntax_ORDERBY() + " " + gridState.j75SortOrder;
                }

            }
            if (!this.gridinput.isrecpagegrid)   //v gridu je vypnuté filtrování
            {
                if (!String.IsNullOrEmpty(gridState.j75Filter))
                {
                    mq.TheGridFilter = _colsProvider.ParseAdhocFilterFromString(gridState.j75Filter, mq.explicit_columns);
                }
                if (gridState.j72HashJ73Query || gridinput.j72id_query > 0)
                {

                    mq.lisJ73 = _Factory.j72TheGridTemplateBL.GetList_j73(gridState.j72ID, gridState.j72Entity.Substring(0, 3), gridinput.j72id_query);
                }
            }
            if (gridState.j72MasterEntity != null) mq.master_prefix = gridState.j72MasterEntity.Substring(0, 3);

            if (gridState.j75GroupDataField != null && gridState.j75GroupLastValue != null)
            {
                //ve zdrojovém gridu je nastavený souhrn podle sloupce
                var thegridcol = mq.explicit_columns.Where(p => p.UniqueName == gridState.j75GroupDataField).First();
                new BO.InitMyQuery(_Factory.CurrentUser).Handle_GroupBy_ExplicitSqlWhere(gridState, thegridcol, mq);

            }

            return _Factory.gridBL.GetGridTable(mq);
        }

    }
}
