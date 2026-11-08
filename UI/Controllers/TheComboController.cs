
using System.Data;


namespace UI.Controllers
{
    public class TheComboController : BaseController
    {
        private readonly BL.TheColumnsProvider _colsProvider;

        public TheComboController(BL.TheColumnsProvider cp)
        {
            _colsProvider = cp;
        }



        public string GetHtml4TheCombo(string entity, string tableid, string myqueryinline, string pids, string filterflag, string searchstring, string masterprefix, int masterpid, string explicit_columns) //Vrací HTML zdroj tabulky pro MyCombo
        {
            var mq = new BO.InitMyQuery(Factory.CurrentUser).Load(entity, masterprefix, masterpid, myqueryinline);
            if (mq.j02id_query > 0 && mq.j02id_query != mq.CurrentUser.pid)
            {
                mq.CurrentUser = Factory.j02UserBL.LoadRunningUser(mq.j02id_query);
            }

            var ce = Factory.EProvider.ByPrefix(mq.Prefix);
            if (!ce.IsWithoutValidity)
            {
                mq.IsRecordValid = true;    //v combo nabídce pouze časově platné záznamy
            }

            if (!string.IsNullOrEmpty(pids) && pids != "0")
            {
                mq.SetPids(pids);
            }
            else
            {
                if (filterflag != "0" && filterflag != "")
                {
                    mq.SearchString = searchstring; //filtrování na straně serveru
                    mq.TopRecordsOnly = Factory.CBL.LoadUserParamInt("searchbox-toprecs", 50); //maximálně prvních 50 záznamů, které vyhovují podmínce
                }
            }


            List<BO.TheGridColumn> cols = null; string strJ72Columns = null;

            if (string.IsNullOrEmpty(explicit_columns) && (mq.Prefix == "j02" || mq.PrefixDb == "p41" || mq.Prefix == "p28" || mq.Prefix == "p91" || mq.Prefix == "p32" || mq.Prefix == "p56" || mq.Prefix == "p90" || mq.Prefix == "o23" || mq.Prefix == "p31"))
            {

                strJ72Columns = Factory.CBL.LoadUserParam($"searchbox-{mq.Prefix}", Factory.j72TheGridTemplateBL.getDefaultPalleteSearchbox(entity));

            }
            else
            {
                strJ72Columns = explicit_columns;   //kvůli výkonu combo z klienta předává zadání sloupců
            }



            if (strJ72Columns != null)
            {
                cols = _colsProvider.ParseTheGridColumns(mq.Prefix, strJ72Columns, Factory);    //zjt: původně zde bylo mq.PrefixDb
            }
            else
            {
                cols = _colsProvider.getDefaultPallete(true, mq, Factory);   //výchozí paleta sloupců
            }


            mq.explicit_columns = cols;


            mq.explicit_orderby = ce.SqlOrderByCombo;
            if (mq.PrefixDb == "p41")
            {
                mq.explicit_orderby = cols[0].getFinalSqlSyntax_ORDERBY();
                mq.explicit_orderby_rst = cols[0].UniqueName;

                if (cols.Count() > 1)
                {
                    mq.explicit_orderby = $"{mq.explicit_orderby},{cols[1].getFinalSqlSyntax_ORDERBY()}";
                    mq.explicit_orderby_rst = $"{mq.explicit_orderby_rst},{cols[1].UniqueName}";
                }
            }
            if (mq.Prefix == "j02" || mq.Prefix == "p28" || mq.Prefix == "p56" || mq.Prefix == "o23")
            {
                mq.explicit_orderby = cols[0].getFinalSqlSyntax_ORDERBY();
                mq.explicit_orderby_rst = cols[0].UniqueName;
            }

            if (mq.Prefix == "p91" || mq.Prefix == "p31")
            {
                mq.explicit_orderby = $"a.{mq.Prefix}ID DESC";
                mq.explicit_orderby_rst = $"pid DESC";
            }


            var strGroupByField = Factory.CBL.LoadUserParam($"searchbox-groupby-{mq.Prefix}");  //nabídka combo položek je vizualizována podle souhrnů
            //if (strGroupByField != null)
            //{
            //    var colGroupBy = _colsProvider.ByUniqueName(strGroupByField);
                
            //    if (mq.explicit_orderby == null)
            //    {
            //        mq.explicit_orderby = colGroupBy.getFinalSqlSyntax_ORDERBY() ;
            //    }
            //    else
            //    {
            //        if (!mq.explicit_orderby.Contains(colGroupBy.getFinalSqlSyntax_ORDERBY()))
            //        {
            //            mq.explicit_orderby = colGroupBy.getFinalSqlSyntax_ORDERBY() + "," + mq.explicit_orderby;
            //        }
                    
            //    }
                
            //}

            var dt = Factory.gridBL.GetGridTable(mq);
            if (strGroupByField != null)
            {
                dt.DefaultView.Sort = strGroupByField;
                DataTable sorted = dt.DefaultView.ToTable();
                dt.Dispose(); // uvolní původní
                dt = sorted;

                foreach(var col in cols)
                {
                    if (col.UniqueName == strGroupByField)
                    {
                        cols.Remove(col);break;
                    }
                }
            }
            var intRows = dt.Rows.Count;

            var s = new System.Text.StringBuilder();

            //informační lišta nabídky (počet záznamů, filtr sešitu, založit nový) – vzhled mtc-bar v site.css
            s.Append("<div class='mtc-bar'>");
            if (mq.TopRecordsOnly > 0)
            {
                if (intRows >= mq.TopRecordsOnly)
                {
                    s.Append($"<span class='mtc-count'>{Factory.tra("Zobrazeno prvních")} {intRows} {Factory.tra("záznamů")}. {Factory.tra("Zpřesněte filtrovací podmínku.")}</span>");
                }
                else
                {
                    s.Append($"<span class='mtc-count'>{Factory.tra("Počet záznamů")}: {intRows}</span>");
                }
            }

            if (mq.Prefix == "p28" && Factory.CurrentUser.TestPermission(BO.PermValEnum.GR_p28_Creator))
            {
                s.Append($"<a class='mtc-new' tabindex='-1' href=\"javascript: _window_open('/p28/Record?pid=0')\">+ {Factory.tra("Založit nový kontakt")}</a>");
            }
            if (mq.Prefix == "p41")
            {
                var mqP41 = (BO.myQueryP41)mq;
                if (mqP41.p34id_for_p31_entry > 0)
                {
                    var recP34 = Factory.p34ActivityGroupBL.Load(mqP41.p34id_for_p31_entry);
                    if (recP34 != null)
                    {                        
                        if (!Factory.CurrentUser.IsHes(524288))
                        {
                            s.Append($"<span class='mtc-count'>{Factory.tra("Projekty pro sešit")} <span class='badge-light'>{System.Net.WebUtility.HtmlEncode(recP34.p34Name)}</span></span>");
                            s.Append($"<a class='mtc-link' href='javascript:zrusit_p34id_for_p31_entry()' tabindex='-1'>{Factory.tra("Zrušit filtrování podle sešitu")}</a>");
                        }
                        
                    }

                }


            }
            if ((mq.Prefix == "p41" || mq.Prefix == "le5") && Factory.CurrentUser.TestPermission(BO.PermValEnum.GR_p41_Creator))
            {

                s.Append($"<a class='mtc-new' tabindex='-1' href=\"javascript: _window_open('/p41/Record?pid=0')\">+ {Factory.tra("Založit nový")}</a>");
            }
            if (mq.Prefix == "p56" && Factory.CurrentUser.j04IsModule_p56)
            {
                s.Append($"<a class='mtc-new' tabindex='-1' href=\"javascript: _window_open('/p56/Record?pid=0')\">+ {Factory.tra("Založit nový úkol")}</a>");
            }
            s.Append("</div>");



            s.Append(string.Format("<table id='{0}' class='table table-thecombo'>", tableid));

            s.Append("<thead><tr>");
            foreach (var col in cols)
            {
                switch (Factory.CurrentUser.j02LangIndex)
                {
                    case 1:
                        s.Append(string.Format("<th>{0}</th>", col.TranslateLang1));
                        break;
                    case 2:
                        s.Append(string.Format("<th>{0}</th>", col.TranslateLang2));
                        break;
                    default:
                        s.Append(string.Format("<th>{0}</th>", col.Header));
                        break;
                }

            }
            s.Append(string.Format("</tr></thead><tbody id='{0}_tbody'>", tableid));

            
            
            string strTrClass = "";object lastGroupByVal = null;
            for (int i = 0; i < intRows; i++)
            {
                if (strGroupByField !=null && dt.Rows[i][strGroupByField] !=System.DBNull.Value && !Equals(lastGroupByVal, dt.Rows[i][strGroupByField]))
                {
                    s.Append($"<tr class='mtc-group' data-skip='true' data-v='0'><td colspan=10>{dt.Rows[i][strGroupByField]}</td></tr>");
                }
                   

                strTrClass = "txz";
                if (Convert.ToBoolean(dt.Rows[i]["isclosed"]) == true)
                {

                    strTrClass += " isclosed";
                }
                if (mq.Prefix == "p32")
                {
                    if (Convert.ToBoolean(dt.Rows[i]["p32IsBillable"]))
                    {
                        strTrClass += " fa";
                    }
                    else
                    {
                        strTrClass += " nefa";
                    }
                }
                s.Append($"<tr class='{strTrClass}' data-v='{dt.Rows[i]["pid"]}'>");
                
                //s.Append(">");
                foreach (var col in cols)
                {
                    if (col.NormalizedTypeName == "num")
                    {
                        s.Append(string.Format("<td class='mtc-num'>{0}</td>", BO.Code.Bas.ParseCellValueFromDb(dt.Rows[i], col)));
                    }
                    else
                    {
                        s.Append(string.Format("<td>{0}</td>", BO.Code.Bas.ParseCellValueFromDb(dt.Rows[i], col)));
                    }


                }
                s.Append("</tr>");
                if (strGroupByField != null)
                {
                    lastGroupByVal = dt.Rows[i][strGroupByField];
                }
                
            }
            s.Append("</tbody></table>");
            s.Append($"<input type=\"hidden\" id=\"columnsinfo{tableid}\" value=\"{strJ72Columns}\"/>");    //předání používaných sloupců, aby se nemuseli znovu zjišťovat na serveru

            return s.ToString();
        }



        //zdroj checkboxů pro taghelper mycombochecklist:
        public string GetHtml4Checkboxlist(string controlid, string entity, string selectedvalues, string masterprefix, int masterpid, string myqueryinline) //Vrací HTML seznam checkboxů pro taghelper: mycombochecklist
        {
            var mq = new BO.InitMyQuery(Factory.CurrentUser).Load(entity, masterprefix, masterpid, myqueryinline);
            if (mq.j02id_query > 0 && mq.j02id_query != mq.CurrentUser.pid)
            {
                mq.CurrentUser = Factory.j02UserBL.LoadRunningUser(mq.j02id_query);
            }
            mq.explicit_columns = _colsProvider.getDefaultPallete(false, mq, Factory);
            var ce = Factory.EProvider.ByPrefix(mq.Prefix);
            if (ce.IsWithoutValidity == false)
            {
                mq.IsRecordValid = true;    //v combo nabídce pouze časově platné záznamy
            }
            mq.explicit_orderby = ce.SqlOrderByCombo;


            List<int> selpids = null;
            if (String.IsNullOrEmpty(selectedvalues) == false)
            {
                selpids = BO.Code.Bas.ConvertString2ListInt(selectedvalues);
            }

            string strTextField = "a__" + entity + "__" + mq.Prefix + "Name";
            switch (mq.Prefix)
            {
                case "j02":
                    strTextField = "a__j02User__fullname_desc";
                    break;
                case "j27":
                    strTextField = "a__j27Currency__j27code";
                    break;
                case "p32":
                    strTextField = "a__p32Activity__AktivitaPlusSesit";
                    break;
            }

            string strGroupField = null;
            string strLastGroup = null;
            string strGroup = null;
            string strChecked = "";
            int intValue = 0;
            string strText = "";
            var dt = Factory.gridBL.GetGridTable(mq);
            var intRows = dt.Rows.Count;

            var sb = new System.Text.StringBuilder();

            //seznam položek – hledání, Vše/Nic a počet řeší mycombochecklist_init (mycontrols.js), vzhled mcl-* v site.css
            sb.AppendLine("<div class='mcl-list'>");
            for (int i = 0; i < intRows; i++)
            {
                intValue = Convert.ToInt32(dt.Rows[i]["pid"]);
                strText = Convert.ToString(dt.Rows[i][strTextField]);
                if (strGroupField != null)
                {
                    strGroup = dt.Rows[i][strGroupField] == null ? null : Convert.ToString(dt.Rows[i][strGroupField]);
                    if (strGroup != strLastGroup)
                    {
                        sb.AppendLine($"<div class='mcl-group'>{System.Net.WebUtility.HtmlEncode(strGroup)}</div>");
                    }
                    strLastGroup = strGroup;
                }
                strChecked = (selpids != null && selpids.Contains(intValue)) ? " checked" : "";
                sb.Append($"<label class='mcl-item' for='chk{controlid}_{intValue}'><input type='checkbox' id='chk{controlid}_{intValue}' name='chk{controlid}' value='{intValue}'{strChecked} /><span>{System.Net.WebUtility.HtmlEncode(strText)}</span></label>");
            }
            sb.AppendLine("</div>");

            return sb.ToString();
        }



        public string GetAutoCompleteHtmlItems(int o15flag, string tableid) //Vrací options pro datalist v rámci autocomplete pole
        {
            var mq = new BO.myQuery("o15");
            var lis = Factory.o15AutoCompleteBL.GetList(mq).Where(p => (int)p.o15Flag == o15flag);
            return string.Join("|", lis.Select(p => p.o15Value));

        }

        public string GetMySelectHtmlOptions(string entity, string textfield, string orderfield)
        {
            var sb = new System.Text.StringBuilder();
            var mq = new BO.myQuery(entity) { IsRecordValid = true };
            textfield = System.Web.HttpUtility.UrlDecode(textfield).Replace("##", "'");
            mq.explicit_selectsql = textfield + " AS combotext";
            if (!string.IsNullOrEmpty(orderfield))
            {
                orderfield = System.Web.HttpUtility.UrlDecode(orderfield).Replace("##", "'");
                mq.explicit_orderby = orderfield;
            }

            var dt = Factory.gridBL.GetGridTable(mq);
            foreach (DataRow dbRow in dt.Rows)
            {

                sb.Append(string.Format("<option value='{0}'>{1}</option>", dbRow["pid"].ToString(), dbRow["combotext"]));
            }


            return sb.ToString();
        }





    }
}