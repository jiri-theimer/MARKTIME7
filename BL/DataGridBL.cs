
using System.Data;
using BO;
using Microsoft.VisualBasic;

namespace BL
{
    public interface IDataGridBL
    {
        public DataTable GetGridTable(BO.baseQuery mq, bool bolGetTotalsRow = false);
        public DataTable GetGroupByList(BO.baseQuery mq, string explicit_groupby_select = null);
        public IEnumerable<BO.GetListOfPids> GetListOfFindPid(BO.baseQuery mq,int intVirtualRowsCount);
        public DataTable GetList4MailMerge(string prefix, int pid);
        public DataTable GetList4MailMerge(int pid, string individual_sql_source);
        public DataTable GetListFromPureSql(string sql);
        public string GetLastFinalSql();
        public List<DL.Param4DT> GetLastFinalGridParameters();


    }
    class DataGridBL : BaseBL, IDataGridBL
    {
        private DL.FinalSqlCommand _q { get; set; }
        public DataGridBL(BL.Factory mother) : base(mother)
        {

        }



        public DataTable GetList4MailMerge(int pid, string individual_sql_source)
        {
            individual_sql_source = individual_sql_source.Replace("@pid", pid.ToString()).Replace("#pid#", pid.ToString());
            return _db.GetDataTable(individual_sql_source);
        }



        public DataTable GetListFromPureSql(string sql)
        {
            sql = BO.Code.Bas.OcistitSQL(sql);
            return _db.GetDataTable(sql);
        }
        public IEnumerable<BO.GetListOfPids> GetListOfFindPid(BO.baseQuery mq,int intVirtualRowsCount)     //pro hledání záznamu v grid stránkách
        {
            BO.TheEntity ce = _mother.EProvider.ByPrefix(mq.Prefix);
            var sb = new System.Text.StringBuilder();
            sb.Append($"SELECT");
            if (intVirtualRowsCount > 25000)
            {
                sb.Append($" TOP 5000");
            }
            sb.Append($" a.{mq.PrefixDb}ID as pid");
            sb.Append($",ROW_NUMBER() OVER(ORDER BY {mq.explicit_orderby}) as rowindex");
            sb.Append(" FROM ");
            sb.Append(ce.SqlFromGrid);    //úvodní FROM klauzule s primární "a" tabulkou

            List<string> relSqls = new List<string>();
            foreach (BO.TheGridColumn col in mq.explicit_columns.Where(x => x.RelName != null && x.RelName != "a"))
            {
                if (col.RelSqlDependOn != null && relSqls.Exists(p => p == col.RelSqlDependOn) == false)
                {
                    relSqls.Add(col.RelSqlDependOn);
                    sb.Append(" ");
                    sb.Append(col.RelSqlDependOn);
                }
                if (relSqls.Exists(p => p == col.RelSql) == false)
                {
                    relSqls.Add(col.RelSql);
                    sb.Append(" ");
                    sb.Append(col.RelSql);
                }

            }
            foreach (BO.TheGridColumn col in mq.explicit_columns.Where(x => x.RelSqlInCol != null))  //sloupce, které mají na míru RelSqlInCol definovanou přímo ve sloupci
            {
                if (!relSqls.Exists(p => p == col.RelSqlInCol))
                {
                    if (col.RelName == null)
                    {
                        relSqls.Add(col.RelSqlInCol);
                        sb.Append(" ");
                        sb.Append(col.RelSqlInCol);
                    }
                    else
                    {   //sloupec s explicitním sql relací a zároveň z jiné relace
                        string upraveno = col.RelSqlInCol.Replace("a.", col.RelName + ".");
                        upraveno = upraveno.Replace("_relname_", col.RelName);
                        if (!relSqls.Exists(p => p == upraveno))
                        {
                            relSqls.Add(upraveno);

                            sb.Append(" ");
                            sb.Append(upraveno);
                        }


                    }

                }

            }

            //List<string> relSqls = new List<string>();

            //foreach (BO.TheGridColumn col in mq.explicit_columns.Where(x => x.RelName != null && x.RelName != "a"))
            //{
            //    if (col.RelSqlDependOn != null && !relSqls.Exists(p => p == col.RelSqlDependOn))
            //    {
            //        relSqls.Add(col.RelSqlDependOn);
            //        sb.Append(" ");
            //        sb.Append(col.RelSqlDependOn);
            //    }
            //    if (col.RelSqlInCol != null && !relSqls.Exists(p => p == col.RelSqlInCol))
            //    {
            //        string upraveno = col.RelSqlInCol;
            //        if (col.RelName != null)
            //        {                        
            //            //sloupec s explicitním sql relací a zároveň z jiné relace
            //            upraveno = col.RelSqlInCol.Replace("a.",$"{col.RelName}.");

            //            upraveno = upraveno.Replace("_relname_", col.RelName);
            //        }

            //        if (!relSqls.Exists(p => p == upraveno))
            //        {
            //            relSqls.Add(upraveno);
            //            sb.Append(" ");
            //            sb.Append(upraveno);
            //        }

            //    }

            //}


            _q = DL.basQuery.GetFinalSql(sb.ToString(), mq, _mother.CurrentUser, true);

            
            Handle_DynamicSql_ByExternalFiles(mq, false);  //zpracování externích sql ze složky dynamicsql
            AddGlobalParams2DynamicViews(mq, false);

            return _db.GetList<BO.GetListOfPids>(_q.FinalSql, _q.Parameters);


        }
        public string GetLastFinalSql()
        {
            if (_q == null) return null;

            return _q.FinalSql;
        }
        public List<DL.Param4DT> GetLastFinalGridParameters()
        {
            return _q.Parameters4DT;
        }
        public DataTable GetGridTable(BO.baseQuery mq, bool bolGetTotalsRow = false)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("SELECT ");            

            if (mq.TopRecordsOnly > 0)
            {
                sb.Append("TOP " + mq.TopRecordsOnly.ToString() + " ");
            }

            if (mq.explicit_columns == null || mq.explicit_columns.Count() == 0)
            {
                mq.explicit_columns = new BL.TheColumnsProvider(_mother.EProvider, _mother.Translator).getDefaultPallete(false, mq, _mother);    //na vstupu není přesný výčet sloupců -> pracovat s default sadou
            }
            if (bolGetTotalsRow)
            {
                if (mq.Prefix == "p31") //test, aby nedocházelo k nesmyslným součtům
                {
                    foreach (var col in mq.explicit_columns.Where(p=>p.Prefix !="p31"))
                    {
                        col.IsShowTotals = false;
                    }
                }
                
                if (mq.explicit_sqlgroupby == null)
                {
                    sb.Append(string.Join(",", mq.explicit_columns.Select(p => p.getFinalSqlSyntax_SUM())));   //součtová řádka gridu
                }
                else
                {
                    sb.Append(string.Join(",", mq.explicit_columns.Select(p => p.getFinalSqlSyntax_GROUPBY_SELECT())));   //souhrny - např. v nástroji GRID-REPORT
                }

            }
            else
            {
                if (mq.explicit_sqlgroupby == null)
                {
                    sb.Append(string.Join(",", mq.explicit_columns.Select(p => p.getFinalSqlSyntax_SELECT())));    //grid sloupce               
                }
                else
                {
                    sb.Append(string.Join(",", mq.explicit_columns.Select(p => p.getFinalSqlSyntax_GROUPBY_SELECT())));    //agregační grid sloupce               
                }
                
            }
            BO.TheEntity ce = _mother.EProvider.ByPrefix(mq.Prefix);

            if (bolGetTotalsRow)
            {
                sb.Append($",COUNT(a.{mq.PrefixDb}ID) as RowsCount");     //sumační dotaz gridu
                switch (mq.Prefix)
                {
                    case "p31": //součty pro hodnoty záložek hodiny/výdaje/odměny
                        sb.Append(",SUM(case when p34x.p33ID=1 then 1 end) as RowsTime");
                        sb.Append(",SUM(case when p34x.p33ID IN (2,5) AND p34x.p34IncomeStatementFlag=1 then 1 end) as RowsExpense");
                        sb.Append(",SUM(case when p34x.p33ID IN (2,5) AND p34x.p34IncomeStatementFlag=2 then 1 end) as RowsFee");
                        sb.Append(",SUM(case when p34x.p33ID=3 then 1 end) as RowsKusovnik");
                        break;
                   
                }
            }
            else
            {
                
                if (mq.explicit_sqlgroupby == null)
                {
                    sb.Append(",");
                    sb.Append(_db.GetSQL1_Ocas(mq.PrefixDb, true, !ce.IsWithoutValidity));    //select dotaz gridu
                }
                
                if (ce.Prefix == "p31" && mq.master_prefix == "app")    //app = approving
                {
                    sb.Append(",a.p31Guid");
                }
            }

            if (mq.explicit_selectsql != null)
            {
                sb.Append("," + mq.explicit_selectsql);
            }

            sb.Append(" FROM ");
                        
            if (ce.Prefix == "p31" && (mq.master_prefix == "app" || mq.IsRecordValid==false))    //app = approving nebo úkon z fyzického archivu
            {
                if (mq.IsRecordValid == false)
                {
                    sb.Append(ce.SqlFromGrid.Replace("p31Worksheet a", "p31Worksheet_Del a"));  //fyzický archiv úkonů je tabulka p31Worksheet_Del
                }
                else
                {
                    sb.Append(ce.SqlFromGrid.Replace("p31Worksheet a", "p31Worksheet_Temp a"));  //temp data pro schvalování
                }
                
            }
            else
            {
                sb.Append(ce.SqlFromGrid);    //úvodní FROM klauzule s primární "a" tabulkou
            }


            List<string> relSqls = new List<string>();
            foreach (BO.TheGridColumn col in mq.explicit_columns.Where(x => x.RelName != null && x.RelName != "a"))
            {
                if (col.RelSqlDependOn != null && relSqls.Exists(p => p == col.RelSqlDependOn) == false)
                {
                    relSqls.Add(col.RelSqlDependOn);
                    sb.Append(" ");
                    sb.Append(col.RelSqlDependOn);
                }
                if (relSqls.Exists(p => p == col.RelSql) == false)
                {
                    relSqls.Add(col.RelSql);
                    sb.Append(" ");
                    sb.Append(col.RelSql);
                }

            }
            foreach (BO.TheGridColumn col in mq.explicit_columns.Where(x => x.RelSqlInCol != null))  //sloupce, které mají na míru RelSqlInCol definovanou přímo ve sloupci
            {
                if (!relSqls.Exists(p => p == col.RelSqlInCol))
                {
                    if (col.RelName == null)
                    {
                        relSqls.Add(col.RelSqlInCol);
                        sb.Append(" ");
                        sb.Append(col.RelSqlInCol);
                    }
                    else
                    {   //sloupec s explicitním sql relací a zároveň z jiné relace
                        string upraveno = col.RelSqlInCol.Replace("a.", col.RelName + ".");
                        upraveno = upraveno.Replace("_relname_", col.RelName);
                        if (!relSqls.Exists(p => p == upraveno))
                        {
                            relSqls.Add(upraveno);

                            sb.Append(" ");
                            sb.Append(upraveno);
                        }


                    }

                }

            }


            //vždy musí být nějaké výchozí třídění v ce.SqlOrderBy!!
            if (!bolGetTotalsRow && String.IsNullOrEmpty(mq.explicit_orderby))
            {
                mq.explicit_orderby = ce.SqlOrderBy;
            }



            //parametrický dotaz s WHERE klauzulí

            if (mq.explicit_orderby_rst != null)
            {
                mq.explicit_orderby = null;
            }
            

            _q = DL.basQuery.GetFinalSql(sb.ToString(), mq, _mother.CurrentUser, true);    //závěrečné vygenerování WHERE a ORDERBY klauzule

            Handle_DynamicSql_ByExternalFiles(mq, true);  //zpracování externích sql ze složky dynamicsql
            AddGlobalParams2DynamicViews(mq, true);


            if (!bolGetTotalsRow && mq.OFFSET_PageSize > 0)
            {
                _q.FinalSql += " OFFSET @pagesize*@pagenum ROWS FETCH NEXT @pagesize ROWS ONLY";
                if (_q.Parameters4DT == null) _q.Parameters4DT = new List<DL.Param4DT>();
                _q.Parameters4DT.Add(new DL.Param4DT() { ParamType = "int", ParName = "pagesize", ParValue = mq.OFFSET_PageSize });
                _q.Parameters4DT.Add(new DL.Param4DT() { ParamType = "int", ParName = "pagenum", ParValue = mq.OFFSET_PageNum });

            }

            if (mq.explicit_orderby_rst != null)
            {
                _q.FinalSql = $"SELECT rst.* FROM ({_q.FinalSql}) rst ORDER BY {mq.explicit_orderby_rst}";  //optimalizace výkonu databáze
                //BO.Code.File.LogInfo(_q.FinalSql);
            }
           
            
            //if (_mother.CurrentUser.j02Login == "hajek@matidal.cz")
            //{
               //BO.Code.File.LogInfo(_q.FinalSql);
            //}



            return _db.GetDataTable(_q.FinalSql, _q.Parameters4DT);

        }
        private void AddGlobalParams2DynamicViews(baseQuery mq, bool bolDataTable)
        {
            
            if (bolDataTable)
            {
                if (_q.Parameters4DT == null) _q.Parameters4DT = new List<DL.Param4DT>();
                if (!_q.Parameters4DT.Any(p => p.ParName == "x01id"))
                {
                    _q.Parameters4DT.Add(new DL.Param4DT() { ParamType = "int", ParName = "x01id", ParValue = _mother.CurrentUser.x01ID });   //kvůli filtrování uvnitř dynamicsql
                }                    
                if (!_q.Parameters4DT.Any(p => p.ParName == "d1"))
                {
                    _q.Parameters4DT.Add(new DL.Param4DT() { ParamType = "datetime", ParName = "d1", ParValue = mq.global_d1_query });  //kvůli filtrování uvnitř dynamicsql
                    _q.Parameters4DT.Add(new DL.Param4DT() { ParamType = "datetime", ParName = "d2", ParValue = mq.global_d2_235959 });    //kvůli filtrování uvnitř dynamicsql
                }
            }
            else
            {
                if (_q.FinalSql.Contains("@d1") || _q.FinalSql.Contains("@x01id"))
                {
                    _q.Parameters.Add("x01id", _mother.CurrentUser.x01ID, DbType.Int32);
                    _q.Parameters.Add("d1", mq.global_d1_query, DbType.DateTime);
                    _q.Parameters.Add("d2", mq.global_d2_235959, DbType.DateTime);
                    _q.Parameters.Add("p31date1", mq.global_d1_query, DbType.DateTime);
                    _q.Parameters.Add("p31date2", mq.global_d2_235959, DbType.DateTime);                                        
                }
                    
                

            }
        }
        private void Handle_DynamicSql_ByExternalFiles(baseQuery mq, bool bolDataTable)    //zpracování SQL views načítaných z externích TXT souborů z wwwroot složky: dynamicsql
        {

            
            
            if (!_q.FinalSql.Contains("[%"))
            {                
                return;    //externí SQL views jsou mergovány značkou [% + %]
            }            

            List<string> sqlfiles = BO.Code.MergeContent.GetAllMergeFieldsInContent(_q.FinalSql);   //test, zda v sql existují relace na dynamická sql view
            foreach (var sqlfile in sqlfiles)
            {
                //nahradit merge výrazy sql-view za skutečné table functions
                string s = $"{sqlfile.Substring(0, sqlfile.Length - 4)}_{(string.IsNullOrEmpty(mq.period_field) ? "p31Date" : mq.period_field)}(@x01id,@d1,@d2)";

                
                if (s == $"{mq.Prefix}_nevyuctovano_p91DateSupply(@x01id,@d1,@d2)" || s == $"{mq.Prefix}_nevyuctovano_p91Date(@x01id,@d1,@d2)" || s == $"{mq.Prefix}_nevyuctovano_{mq.PrefixDb}PlanFrom(@x01id,@d1,@d2)" || s == $"{mq.Prefix}_nevyuctovano_{mq.PrefixDb}PlanUntil(@x01id,@d1,@d2)" || s == $"{mq.Prefix}_nevyuctovano_{mq.PrefixDb}DateInsert(@x01id,@d1,@d2)")
                {
                    s = $"{mq.Prefix}_nevyuctovano_p31Date(@x01id,convert(datetime,'01.01.2000',104),convert(datetime,'01.01.3000',104))";
                }
                if (s == $"{mq.Prefix}_rozpracovano_p91DateSupply(@x01id,@d1,@d2)" || s == $"{mq.Prefix}_rozpracovano_p91Date(@x01id,@d1,@d2)" || s == $"{mq.Prefix}_rozpracovano_{mq.PrefixDb}PlanFrom(@x01id,@d1,@d2)" || s == $"{mq.Prefix}_rozpracovano_{mq.PrefixDb}PlanUntil(@x01id,@d1,@d2)" || s == $"{mq.Prefix}_rozpracovano_{mq.PrefixDb}DateInsert(@x01id,@d1,@d2)")
                {
                    s = $"{mq.Prefix}_rozpracovano_p31Date(@x01id,convert(datetime,'01.01.2000',104),convert(datetime,'01.01.3000',104))";
                }
                if (s == $"{mq.Prefix}_vykazano_p91DateSupply(@x01id,@d1,@d2)" || s == $"{mq.Prefix}_vykazano_p91Date(@x01id,@d1,@d2)" || s == $"{mq.Prefix}_vykazano_{mq.PrefixDb}PlanFrom(@x01id,@d1,@d2)" || s == $"{mq.Prefix}_vykazano_{mq.PrefixDb}PlanUntil(@x01id,@d1,@d2)" || s == $"{mq.Prefix}_vykazano_{mq.PrefixDb}DateInsert(@x01id,@d1,@d2)")
                {
                    s = $"{mq.Prefix}_vykazano_p31Date(@x01id,convert(datetime,'01.01.2000',104),convert(datetime,'01.01.3000',104))";
                }
                if (s == $"{mq.Prefix}_vyuctovano_{mq.PrefixDb}DateInsert(@x01id,@d1,@d2)" || s == $"{mq.Prefix}_vyuctovano_{mq.PrefixDb}PlanFrom(@x01id,@d1,@d2)" || s == $"{mq.Prefix}_vyuctovano_{mq.PrefixDb}PlanUntil(@x01id,@d1,@d2)")
                {
                    s = $"{mq.Prefix}_vyuctovano_p31Date(@x01id,convert(datetime,'01.01.2000',104),convert(datetime,'01.01.3000',104))";
                }

                if (s == "p91_vyuctovano_p91DateMaturity(@x01id,@d1,@d2)")
                {
                    s = $"p91_vyuctovano_p31Date(@x01id,convert(datetime,'01.01.2000',104),convert(datetime,'01.01.3000',104))";
                }



                _q.FinalSql = _q.FinalSql.Replace("[%" + sqlfile + "%]", s);
            }

        }

        public DataTable GetList4MailMerge(string prefix, int pid)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("SELECT ");
            switch (prefix)
            {
                case "j02":
                    sb.Append("a.*,j07.j07Name,j04.*");
                    sb.Append(" FROM j02User a INNER JOIN j04UserRole j04 ON a.j04ID=j04.j04ID LEFT OUTER JOIN j07ContactType j07 on a.j07ID=j07.j07ID");
                    sb.Append(" LEFT OUTER JOIN j02User_FreeField j02free ON a.j02ID=j02free.j02ID");
                    break;

                case "p90":
                    sb.Append("a.*,p89.*,p90free.*,p28.*,j27.*");
                    sb.Append(" FROM p90Proforma a INNER JOIN p89ProformaType p89 ON a.p89ID=p89.p89ID LEFT OUTER JOIN p28Contact p28 ON a.p28ID=p28.p28ID");
                    sb.Append(" LEFT OUTER JOIN j27Currency j27 ON a.j27ID=j27.j27ID");
                    sb.Append(" LEFT OUTER JOIN p90Proforma_FreeField p90free ON a.p90ID=p90free.p90ID");
                    break;
                case "p91":
                    sb.Append("a.*,p92.*,p93.*,p28.*,j27.*,p91free.*");
                    sb.Append(" FROM p91Invoice a INNER JOIN p92InvoiceType p92 ON a.p92ID=p92.p92ID LEFT OUTER JOIN p28Contact p28 ON a.p28ID=p28.p28ID LEFT OUTER JOIN p93InvoiceHeader p93 on p92.p93ID=p93.p93ID");
                    sb.Append(" LEFT OUTER JOIN j27Currency j27 ON a.j27ID=j27.j27ID");
                    sb.Append(" LEFT OUTER JOIN p91Invoice_FreeField p91free ON a.p91ID=p91free.p91ID");
                    break;
                case "p94":
                    sb.Append("a.p94Date,a.p94Amount,a.p94Description,p91.*,p92.*,p93.*,p28.*,j27.*,p91free.*");
                    sb.Append(" FROM p94Invoice_Payment a INNER JOIN p91Invoice p91 ON a.p91ID=p91.p91ID INNER JOIN p92InvoiceType p92 ON p91.p92ID=p92.p92ID LEFT OUTER JOIN p28Contact p28 ON p91.p28ID=p28.p28ID LEFT OUTER JOIN p93InvoiceHeader p93 on p92.p93ID=p93.p93ID");
                    sb.Append(" LEFT OUTER JOIN j27Currency j27 ON p91.j27ID=j27.j27ID");
                    sb.Append(" LEFT OUTER JOIN p91Invoice_FreeField p91free ON p91.p91ID=p91free.p91ID");
                    break;
                case "p84":
                    var recP84 = _mother.p84UpominkaBL.Load(pid);
                    sb.Append("a.*,p91.*,p83.*,p86.*,j27.*");
                    sb.Append(" FROM p84Upominka a LEFT OUTER JOIN p91Invoice p91 ON a.p91ID=p91.p91ID LEFT OUTER JOIN p83UpominkaType p83 ON a.p83ID=p83.p83ID LEFT OUTER JOIN j27Currency j27 ON p91.j27ID=j27.j27ID");
                    sb.Append($" LEFT OUTER JOIN (select {recP84.p91ID} as InvoiceID,* FROM p86BankAccount WHERE p86ID=dbo.p91_get_p86id({recP84.p91ID})) p86 ON a.p91ID=p86.InvoiceID");
                    break;
                case "p28":
                    sb.Append("a.*,p29.*,p28free.*");
                    sb.Append(" FROM p28Contact a INNER JOIN p29ContactType p29 on a.p29ID=p29.p29ID");
                    sb.Append(" LEFT OUTER JOIN p28Contact_FreeField p28free ON a.p28ID=p28free.p28ID");
                    break;
                case "p41":
                    sb.Append("a.*,p42.*,p41free.*");
                    sb.Append(" FROM p41Project a INNER JOIN p42ProjectType p42 on a.p42ID=p42.p42ID");
                    //sb.Append(" LEFT OUTER JOIN p28Contact p28 a.p28ID_Client=p28.p28ID");
                    sb.Append(" LEFT OUTER JOIN p41Project_FreeField p41free ON a.p41ID=p41free.p41ID");
                    break;
                case "p31":
                    sb.Append("a.*,j02.*,p32x.*,p34.*,p28Client.*,p70.*,j27billing_orig.*");
                    sb.Append(" FROM p31Worksheet a");
                    sb.Append(" INNER JOIN j02User j02 ON a.j02ID=j02.j02ID INNER JOIN p32Activity p32x ON a.p32ID=p32x.p32ID");
                    sb.Append(" INNER JOIN p34ActivityGroup p34 ON p32x.p34ID=p34.p34ID INNER JOIN p41Project p41 ON a.p41ID=p41.p41ID");
                    sb.Append(" LEFT OUTER JOIN p28Contact p28Client ON p41.p28ID_Client=p28Client.p28ID");
                    sb.Append(" LEFT OUTER JOIN p70BillingStatus p70 ON a.p70ID=p70.p70ID LEFT OUTER JOIN p71ApproveStatus p71 ON a.p71ID=p71.p71ID LEFT OUTER JOIN p72PreBillingStatus p72trim ON a.p72ID_AfterTrimming=p72trim.p72ID LEFT OUTER JOIN p72PreBillingStatus p72approve ON a.p72ID_AfterApprove=p72approve.p72ID");
                    sb.Append(" LEFT OUTER JOIN j27Currency j27billing_orig ON a.j27ID_Billing_Orig=j27billing_orig.j27ID");

                    break;
            }
            
            sb.Append($" WHERE a.{prefix}ID={pid}");
            return _db.GetDataTable(sb.ToString());
        }



        public DataTable GetGroupByList(BO.baseQuery mq,string explicit_groupby_select=null)
        {
            //v mq musí být definován: explicit_columns a explicit_orderby
            var sb = new System.Text.StringBuilder();
            sb.Append("SELECT ");

            if (explicit_groupby_select == null)
            {
                sb.Append(string.Join(",", mq.explicit_columns.Select(p => p.getFinalSqlSyntax_GROUPBY_SELECT())));    //grid sloupce
            }
            else
            {
                sb.Append(explicit_groupby_select);
            }

            if (mq.explicit_selectsql != null)
            {
                sb.Append("," + mq.explicit_selectsql);
            }

            BO.TheEntity ce = _mother.EProvider.ByPrefix(mq.Prefix);

            sb.Append($",COUNT(a.{mq.PrefixDb}ID) as RowsCount,0 as pid");     //počet záznamů v souhrnu
            switch (mq.Prefix)
            {
                case "p31": //součty pro hodnoty záložek hodiny/výdaje/odměny
                    sb.Append(",SUM(case when p34x.p33ID=1 then 1 end) as RowsTime");
                    sb.Append(",SUM(case when p34x.p33ID IN (2,5) AND p34x.p34IncomeStatementFlag=1 then 1 end) as RowsExpense");
                    sb.Append(",SUM(case when p34x.p33ID IN (2,5) AND p34x.p34IncomeStatementFlag=2 then 1 end) as RowsFee");
                    sb.Append(",SUM(case when p34x.p33ID=3 then 1 end) as RowsKusovnik");
                    break;

            }


            

            sb.Append($",{mq.explicit_sqlgroupby} as GroupByValue"); //sloupec podle které se filtrují řádky souhrnů v gridu

            sb.Append(" FROM ");

            if (ce.Prefix == "p31" && (mq.master_prefix == "app" || mq.IsRecordValid == false))    //app = approving nebo úkon z fyzického archivu
            {
                if (mq.IsRecordValid == false)
                {
                    sb.Append(ce.SqlFromGrid.Replace("p31Worksheet a", "p31Worksheet_Del a"));  //fyzický archiv úkonů je tabulka p31Worksheet_Del
                }
                else
                {
                    sb.Append(ce.SqlFromGrid.Replace("p31Worksheet a", "p31Worksheet_Temp a"));  //temp data pro schvalování
                }

            }
            else
            {
                sb.Append(ce.SqlFromGrid);    //úvodní FROM klauzule s primární "a" tabulkou
            }            
            

            List<string> relSqls = new List<string>();
            foreach (BO.TheGridColumn col in mq.explicit_columns.Where(x => x.RelName != null && x.RelName != "a"))
            {
                if (col.RelSqlDependOn != null && relSqls.Exists(p => p == col.RelSqlDependOn) == false)
                {
                    relSqls.Add(col.RelSqlDependOn);
                    sb.Append(" ");
                    sb.Append(col.RelSqlDependOn);
                }
                if (relSqls.Exists(p => p == col.RelSql) == false)
                {
                    relSqls.Add(col.RelSql);
                    sb.Append(" ");
                    sb.Append(col.RelSql);
                }

            }
            foreach (BO.TheGridColumn col in mq.explicit_columns.Where(x => x.RelSqlInCol != null))  //sloupce, které mají na míru RelSqlInCol definovanou přímo ve sloupci
            {
                if (!relSqls.Exists(p => p == col.RelSqlInCol))
                {
                    if (col.RelName == null)
                    {
                        relSqls.Add(col.RelSqlInCol);
                        sb.Append(" ");
                        sb.Append(col.RelSqlInCol);
                    }
                    else
                    {   //sloupec s explicitním sql relací a zároveň z jiné relace
                        string upraveno = col.RelSqlInCol.Replace("a.", col.RelName + ".");
                        upraveno = upraveno.Replace("_relname_", col.RelName);
                        if (!relSqls.Exists(p => p == upraveno))
                        {
                            relSqls.Add(upraveno);

                            sb.Append(" ");
                            sb.Append(upraveno);
                        }


                    }

                }

            }

            //parametrický dotaz s WHERE klauzulí

            _q = DL.basQuery.GetFinalSql(sb.ToString(), mq, _mother.CurrentUser, true);    //závěrečné vygenerování WHERE a ORDERBY klauzule

            Handle_DynamicSql_ByExternalFiles(mq, true);  //zpracování externích sql ze složky dynamicsql

            AddGlobalParams2DynamicViews(mq, true);

            

            //BO.Code.File.LogInfo(_q.FinalSql);

            return _db.GetDataTable(_q.FinalSql, _q.Parameters4DT);

        }
    }
}
