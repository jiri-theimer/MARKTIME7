

using System.Xml.Schema;

namespace BO
{
    public class QueryPeriod
    {
        public DateTime d1;
        private DateTime _d2;

        public QueryPeriod(DateTime d1, DateTime d2)
        {
            this.d1 = d1;
            this.d2 = d2;
        }
        public DateTime d2
        {
            get
            {
                return _d2;
            }
            set
            {
                _d2 = BO.Code.Bas.ConvertDateTo235959(value);     //převést datum-do na čas 23:59:59
            }
        }
    }
    public class QRow
    {
        public string StringWhere { get; set; }
        public string ParName { get; set; }
        public object ParValue { get; set; }

        public string AndOrZleva { get; set; } = "AND";

        public string BracketLeft { get; set; }
        public string BracketRight { get; set; }

        public string Par2Name { get; set; }
        public object Par2Value { get; set; }

    }

    public abstract class baseQuery
    {
        private string _pkfield;
        private string _prefixdb;   //prefix, který pasuje na fyzický datový model tabulky
        private string _prefix; //prefix podle Entity singletonu
        private List<QRow> _lis;

        public string Prefix
        {
            get
            {
                return _prefix;
            }
            set
            {
                _prefix = value;
                _prefixdb = BO.Code.Entity.GetPrefixDb(_prefix);
                _pkfield = "a." + _prefixdb + "ID";

            }
        }
        public string PkField
        {
            get
            {
                return _pkfield;
            }
        }

        public string PrefixDb
        {
            get
            {
                return _prefixdb;
            }
        }
        public List<int> pids { get; set; }
        public void SetPids(string strPids)
        {
            this.pids = Code.Bas.ConvertString2ListInt(strPids);

        }

        public int TopRecordsOnly { get; set; }
        public int OFFSET_PageSize { get; set; }
        public int OFFSET_PageNum { get; set; }

        public IEnumerable<BO.TheGridColumn> explicit_columns { get; set; }
        public string explicit_orderby { get; set; }
        public string explicit_orderby_rst { get; set; }    //pokud vyplněno, pak sql dotaz složit přes rst syntaxi - to dává lepší výkon db serveru!!!
        public string explicit_selectsql { get; set; }
        public string explicit_sqlwhere { get; set; }
        public string explicit_sqlgroupby { get; set; }
        public BO.RunningUser CurrentUser;
        public int j02id_query { get; set; }

        public List<BO.TheGridColumnFilter> TheGridFilter { get; set; }     //sloupcový filtr
        public int TheGridOperatorFlag { get; set; }    //0: and, 1: or
        public bool IsNotUseParams_Sql_InGridFilter { get; set; }           //vnitřně nepoužívat parametry v SQL podmínce sloupcového filtru
        public IEnumerable<BO.j73TheGridQuery> lisJ73 { get; set; }         //uložený filtr z návrháře sloupců
        
        public bool MyRecordsDisponible { get; set; }
        public bool MyRecordsDisponible_Approve { get; set; }   //aplikovatelné pouze u p31/p41/p28/p56
        public bool? IsRecordValid { get; set; } = true;
        public List<int> o51ids { get; set; }

        public int p31statequery { get; set; }  //filtrování podle stavu úkonu v p31/p41/p28/p56/j02, paleta 1 - 17
        public string p31tabquery { get; set; }
        public bool? iswip { get; set; }        //rozpracované úkony

        public bool? isinvoiced { get; set; }   //vyúčtované úkony
        public bool? isapproved_and_wait4invoice { get; set; }  //schváleno a čeká na vyúčtování


        public string period_field { get; set; }
        public DateTime? global_d1 { get; set; }
        public DateTime? global_d2 { get; set; }
        public string master_prefix { get; set; }

        public int get_real_j02id_query()
        {
            if (this.j02id_query > 0)
            {
                return this.j02id_query;
            }
            else
            {
                return this.CurrentUser.pid;
            }
        }
        public DateTime global_d2_235959
        {
            get
            {
                if (this.global_d2 == null) return new DateTime(3000, 1, 1);
                return Code.Bas.ConvertDateTo235959(Convert.ToDateTime(this.global_d2));  //převést datum-do na čas 23:59:59
            }
        }
        public DateTime global_d1_query
        {
            get
            {
                if (this.global_d1 == null) return new DateTime(2000, 1, 1);
                return Convert.ToDateTime(this.global_d1);
            }
        }
        public DateTime global_d2_query
        {
            get
            {
                if (this.global_d2 == null) return new DateTime(3000, 1, 1);
                return Convert.ToDateTime(this.global_d2);
            }
        }

        public string ParseSqlWhere()   //vrací kompletní sql where klauzuly bez parametrů!
        {
            if (_lis == null)
            {
                GetRows();
            }
            if (_lis.Any(p => p.ParName == "x01id_hostingmode"))
            {
                string s = String.Join(" ", _lis.Select(p => p.AndOrZleva + " " + p.BracketLeft + p.StringWhere + p.BracketRight)).Trim();
                return s.Replace("@x01id_hostingmode", this.CurrentUser.x01ID.ToString());
            }
            else
            {
                return String.Join(" ", _lis.Select(p => p.AndOrZleva + " " + p.BracketLeft + p.StringWhere + p.BracketRight)).Trim();
            }


        }

        public bool IsActivePeriodQuery()   //test, zda je v query žádost o filtrování přes časové období
        {
            if (this.global_d1 != null && this.global_d2 != null)
            {
                if (Convert.ToDateTime(this.global_d1).Year > 1900 || Convert.ToDateTime(this.global_d2).Year < 3000)
                {
                    return true;
                }
            }

            return false;
        }

        protected string _searchstring;
        public string SearchString
        {
            get
            {
                return _searchstring;
            }
            set
            {
                _searchstring = value;
                if (!string.IsNullOrEmpty(_searchstring))
                {
                    _searchstring = _searchstring.ToLower().Trim();
                    if (_searchstring.Contains("--") || _searchstring.Contains("drop") || _searchstring.Contains("delete") || _searchstring.Contains("truncate"))
                    {
                        _searchstring = _searchstring.Replace("--", "").Replace("drop", "").Replace("delete", "").Replace("truncate", "").Replace(";", " or ").Replace(",", " or ").Replace("  ", " ");
                    }

                    //_searchstring = _searchstring.Replace("--", "").Replace("drop", "").Replace("delete", "").Replace("truncate", "").Replace(";", " or ").Replace(",", " or ").Replace("  ", " ");
                    //_searchstring = _searchstring.Replace(" or ", "#or#").Replace(" and ", "#and#");
                    //_searchstring = _searchstring.Replace(" ", " and ");
                    //_searchstring = _searchstring.Replace("#or#", " or ").Replace("#and#", " and ");
                }

            }
        }

        public virtual List<QRow> GetRows()
        {
            return InhaleRows();

        }
        public void ClearQRows()
        {
            _lis = new List<QRow>();
        }
        protected List<QRow> InhaleRows()
        {
            if (this.pids != null && this.pids.Any())
            {
                AQ($"{_pkfield} IN ({String.Join(",", this.pids)})", "", null);
                if (this.Prefix != "p31")   //archiv u p31 je externí tabulka p31worksheet_del
                {
                    this.IsRecordValid = null;  //pokud jsou na vstupu konkrétní ID záznamů, filtr časové platnosti nemá význam
                }
                
            }
            if (this.Prefix == "b05")
            {
                this.IsRecordValid = null;
            }


            if (this.IsRecordValid != null && this.Prefix !="p31")  //pro úkon existuje fyzický archiv v tabulce p31worksheet_del
            {
                if (this.IsRecordValid == true)
                {
                    AQ($"a.{_prefixdb}ValidUntil>GETDATE()", "", null);
                }
                if (this.IsRecordValid == false)
                {
                    AQ($"GETDATE() NOT BETWEEN a.{_prefixdb}ValidFrom AND a.{_prefixdb}ValidUntil", "", null);
                }

            }
            if (CurrentUser != null && CurrentUser.IsHostingModeTotalCloud)
            {
                Handle_AppendCloudQuery(this.PrefixDb);

            }

            if (this.o51ids != null && this.o51ids.Count > 0)
            {
                AQ($"a.{_prefixdb}ID IN (select o52RecordPid FROM o52TagBinding WHERE o51ID IN ({string.Join(",", this.o51ids)}))", null, null);
                //AQ("a." + _prefixdb + "ID IN (select o52RecordPid FROM o52TagBinding WHERE o51ID IN (" + string.Join(",", this.o51ids) + "))", null, null);
            }
            if (this.explicit_sqlwhere != null)
            {
                AQ(this.explicit_sqlwhere, "", null);
            }
            if (this.lisJ73 != null)
            {                
                ParseJ73Query();
            }
            if (this.TheGridFilter != null)
            {
                ParseSqlFromTheGridFilter();  //složit filtrovací podmínku ze sloupcového filtru gridu
            }

            return _lis;
        }

        protected void AQ(string strWhere, string strParName, object ParValue, string strAndOrZleva = "AND", string strBracketLeft = null, string strBracketRight = null, string strPar2Name = null, object Par2Value = null)
        {
            if (_lis == null)
            {
                _lis = new List<QRow>();
            }
            if (_lis.Count == 0)
            {
                strAndOrZleva = ""; //první podmínka zleva
            }

            if (!String.IsNullOrEmpty(strParName) && _lis.Where(p => p.ParName == strParName).Count() > 0)
            {
                return; //parametr strParName již byl dříve přidán
            }
            if (strWhere.Contains(" OR ", StringComparison.OrdinalIgnoreCase))
            {
                strWhere = $"({strWhere})";    //v podmínce je operátor OR
            }
            _lis.Add(new QRow() { StringWhere = strWhere, ParName = strParName, ParValue = ParValue, AndOrZleva = strAndOrZleva, BracketLeft = strBracketLeft, BracketRight = strBracketRight, Par2Name = strPar2Name, Par2Value = Par2Value });
        }





        private void ParseSqlFromTheGridFilter()    //zpracování SQL where klauzule pro sloupcový filtr
        {
            int x = 1;
            int intQueryRowsAtStart = _lis.Count();

            foreach (var filterrow in this.TheGridFilter)
            {
                int intQueryRowsBefore = _lis.Count();                
                string parName = $"par{x}";

                var col = filterrow.BoundColumn;
                var strF = col.getFinalSqlSyntax_WHERE();


                string strGlobalAndOrZleva = "AND";
                if (x > 1)
                {
                    if (this.TheGridOperatorFlag == 1) strGlobalAndOrZleva = "OR";  //je zapnutý režim sloupcového filtru: [OR]

                }

                
                int endIndex = 0;
                string[] arr = new string[] { filterrow.value };
                if (filterrow.value.IndexOf(";") > -1)  //v podmnínce sloupcového filtru může být středníkem odděleno více hodnot!
                {
                    arr = filterrow.value.Split(";");
                    endIndex = arr.Count() - 1;
                }

                switch (filterrow.oper)
                {
                    case "1":   //IS NULL                        
                        AQ($"{strF} IS NULL", "", null, strGlobalAndOrZleva);
                        break;
                    case "2":   //IS NOT NULL                        
                        AQ($"{strF} IS NOT NULL", "", null, strGlobalAndOrZleva);
                        break;
                    case "10":   //větší než nula                        
                        AQ($"{strF} > 0", "", null, strGlobalAndOrZleva);
                        break;
                    case "11":   //je nula nebo prázdné                        
                        AQ($"ISNULL({strF},0)=0", "", null, strGlobalAndOrZleva);
                        break;
                    case "8":   //ANO
                        x += 1;
                        AQ($"{strF} = 1", "", null, strGlobalAndOrZleva);
                        break;
                    case "9":   //NE                        
                        AQ($"{strF} = 0", "", null, strGlobalAndOrZleva);
                        break;
                    case "3":   //obsahuje                
                        for (var i = 0; i <= endIndex; i++)
                        {
                            if (arr[i].Trim() != "")
                            {
                                arr[i] = arr[i].Trim();
                                if (this.IsNotUseParams_Sql_InGridFilter)
                                {
                                    AQ(leva_zavorka(i, endIndex) + string.Format(strF + " LIKE '%{0}%' COLLATE Latin1_General_CI_AI", arr[i].Replace("'", "")) + prava_zavorka(i, endIndex), null, null, i == 0 ? strGlobalAndOrZleva : "OR");
                                }
                                else
                                {
                                    AQ(leva_zavorka(i, endIndex) + string.Format(strF + " LIKE '%'+@{0}+'%' COLLATE Latin1_General_CI_AI", parName + "i" + i.ToString()) + prava_zavorka(i, endIndex), parName + "i" + i.ToString(), arr[i], i == 0 ? strGlobalAndOrZleva : "OR");


                                }

                            }

                        }

                        break;
                    case "33":   //neobsahuje                
                        for (var i = 0; i <= endIndex; i++)
                        {
                            if (arr[i].Trim() != "")
                            {
                                arr[i] = arr[i].Trim();
                                if (this.IsNotUseParams_Sql_InGridFilter)
                                {
                                    AQ(leva_zavorka(i, endIndex) + string.Format(strF + " NOT LIKE '%{0}%' COLLATE Latin1_General_CI_AI", arr[i].Replace("'", "")) + prava_zavorka(i, endIndex), null, null, "AND");
                                }
                                else
                                {
                                    AQ(leva_zavorka(i, endIndex) + string.Format(strF + " NOT LIKE '%'+@{0}+'%' COLLATE Latin1_General_CI_AI", parName + "i" + i.ToString()) + prava_zavorka(i, endIndex), parName + "i" + i.ToString(), arr[i], "AND");


                                }

                            }

                        }

                        break;
                    case "5":   //začíná na 
                        for (var i = 0; i <= endIndex; i++)
                        {
                            if (arr[i].Trim() != "")
                            {
                                arr[i] = arr[i].Trim();
                                if (this.IsNotUseParams_Sql_InGridFilter)
                                {
                                    AQ(leva_zavorka(i, endIndex) + string.Format(strF + " LIKE '{0}%' COLLATE Latin1_General_CI_AI", arr[i].Trim().Replace("'", "")) + prava_zavorka(i, endIndex), null, null, i == 0 ? strGlobalAndOrZleva : "OR");
                                }
                                else
                                {
                                    AQ(leva_zavorka(i, endIndex) + string.Format(strF + " LIKE @{0}+'%' COLLATE Latin1_General_CI_AI", parName + "i" + i.ToString()) + prava_zavorka(i, endIndex), parName + "i" + i.ToString(), arr[i].Trim(), i == 0 ? strGlobalAndOrZleva : "OR");
                                }

                            }

                        }

                        break;
                    case "6":   //je rovno
                        for (var i = 0; i <= endIndex; i++)
                        {
                            if (arr[i].Trim() != "")
                            {
                                arr[i] = arr[i].Trim();
                                if (this.IsNotUseParams_Sql_InGridFilter)
                                {
                                    AQ(leva_zavorka(i, endIndex) + string.Format(strF + " = {0}", get_rawsql_value(col.NormalizedTypeName, arr[i])) + prava_zavorka(i, endIndex), null, null, i == 0 ? strGlobalAndOrZleva : "OR");
                                }
                                else
                                {
                                    AQ(leva_zavorka(i, endIndex) + string.Format(strF + " = @{0}", parName + "i" + i.ToString()) + prava_zavorka(i, endIndex), parName + "i" + i.ToString(), get_param_value(col.NormalizedTypeName, arr[i]), i == 0 ? strGlobalAndOrZleva : "OR");
                                }

                            }

                        }

                        break;
                    case "4":   //interval                        
                        if (this.IsNotUseParams_Sql_InGridFilter)
                        {
                            AQ(string.Format(strF + " >= {0}", get_rawsql_value(col.NormalizedTypeName, filterrow.c1value)), null, null, strGlobalAndOrZleva);
                            AQ(string.Format(strF + " <= {0}", get_rawsql_value(col.NormalizedTypeName, filterrow.c2value)), null, null, "AND");
                        }
                        else
                        {
                            AQ(string.Format(strF + " >= @{0}", parName + "c1"), parName + "c1", get_param_value(col.NormalizedTypeName, filterrow.c1value), strGlobalAndOrZleva);
                            AQ(string.Format(strF + " <= @{0}", parName + "c2"), parName + "c2", get_param_value(col.NormalizedTypeName, filterrow.c2value), "AND");
                        }

                        break;
                    case "7":   //není rovno
                        for (var i = 0; i <= endIndex; i++)
                        {                            
                            if (arr[i].Trim() != "")
                            {
                                arr[i] = arr[i].Trim();
                                if (this.IsNotUseParams_Sql_InGridFilter)
                                {
                                    AQ(leva_zavorka(i, endIndex) + string.Format(strF + " <> {0}", get_rawsql_value(col.NormalizedTypeName, arr[i])) + prava_zavorka(i, endIndex), null, null, i == 0 ? strGlobalAndOrZleva : "OR");
                                }
                                else
                                {
                                    AQ(leva_zavorka(i, endIndex) + string.Format(strF + " <> @{0}", parName + "i" + i.ToString()) + prava_zavorka(i, endIndex), parName + "i" + i.ToString(), get_param_value(col.NormalizedTypeName, arr[i]), i == 0 ? strGlobalAndOrZleva : "OR");
                                }

                            }
                        }

                        break;
                }

                if (_lis.Count() > intQueryRowsBefore)
                {
                    x += 1;
                }

            }

            if (_lis.Count() - intQueryRowsAtStart>1)
            {
                _lis[intQueryRowsAtStart].BracketLeft = "(";
                _lis[_lis.Count() - 1].BracketRight = ")";
            }

        }

        private string leva_zavorka(int i, int intEndIndex)
        {
            if (intEndIndex > 0 && i == 0)
            {
                return "(";
            }
            else
            {
                return "";
            }
        }
        private string prava_zavorka(int i, int intEndIndex)
        {
            if (intEndIndex > 0 && i == intEndIndex)
            {
                return ")";
            }
            else
            {
                return "";
            }
        }


        private void ParseJ73Query()    //zpracování vnitřní filtrovací podmínky z návrháře sloupců
        {
            int x = 0; string ss = ""; string strField = ""; string strAndOrZleva = "";BO.j73TheGridQuery recLast = null;
            

            int intFirstApplicableRowIndex = 0;bool bolNegace = false;
            if (_lis != null && _lis.Count()>0)
            {
                intFirstApplicableRowIndex = _lis.Count();
            }
            if (this.lisJ73.Count() > 0)
            {
                bolNegace = this.lisJ73.First().j72IsQueryNegation;
            }
           
            foreach (var c in this.lisJ73)
            {                
                if (recLast  !=null && c.j72ID != recLast.j72ID)    //filtr složený z vnitřní podmínky tabulky a pojmenovaného filtru
                {
                    this.lisJ73.First().j73BracketLeft += "(";
                    recLast.j73BracketRight += ")";
                    
                    c.j73BracketLeft += "(";
                    this.lisJ73.Last().j73BracketRight += ")";
                }
                x += 1;
                ss = x.ToString();
                strField = c.j73Column;
                if (c.FieldSqlSyntax != null)
                {
                    strField = c.FieldSqlSyntax;
                }
                strAndOrZleva = c.j73Op;

                switch (c.j73Operator)
                {
                    case "ISNULL":
                        AQ($"{strField} IS NULL", "", null, strAndOrZleva, c.j73BracketLeft, c.j73BracketRight);
                        break;
                    case "NOT-ISNULL":
                        AQ($"{strField} IS NOT NULL", "", null, strAndOrZleva, c.j73BracketLeft, c.j73BracketRight);
                        break;
                    case "GREATERZERO":
                        AQ($"ISNULL({strField},0)>0", "", null, strAndOrZleva, c.j73BracketLeft, c.j73BracketRight);
                        break;
                    case "ISNULLORZERO":
                        AQ($"ISNULL({strField},0)=0", "", null, strAndOrZleva, c.j73BracketLeft, c.j73BracketRight);
                        break;
                    case "CONTAINS":
                        AQ($"{strField} LIKE '%{Code.Bas.GSS(c.j73Value)}%'", null, null, strAndOrZleva, c.j73BracketLeft, c.j73BracketRight);
                        //AQ( strField + " LIKE '%" + Code.Bas.GSS(c.j73Value) + "+%'", null, null, strAndOrZleva, c.j73BracketLeft, c.j73BracketRight);

                        break;
                    case "STARTS":
                        AQ($"{strField} LIKE {Code.Bas.GSS(c.j73Value)}%'", null, null, strAndOrZleva, c.j73BracketLeft, c.j73BracketRight);
                        //AQ( strField + " LIKE '" + Code.Bas.GSS(c.j73Value) + "+%'", null, null, strAndOrZleva, c.j73BracketLeft, c.j73BracketRight);

                        break;
                    case "INTERVAL":
                        if (c.FieldType == "date")
                        {
                            if (c.j73DatePeriodFlag > 0)
                            {
                                var cPeriods = new BO.Code.Cls.ThePeriodProviderSupport();
                                var lisPeriods = cPeriods.GetPallete();
                                c.j73Date1 = lisPeriods.Where(p => p.pid == c.j73DatePeriodFlag).First().d1;
                                c.j73Date2 = Convert.ToDateTime(lisPeriods.Where(p => p.pid == c.j73DatePeriodFlag).First().d2).AddDays(1).AddMinutes(-1);
                            }
                            if (c.j73Date1 != null && c.j73Date2 != null)
                            {
                                AQ(c.WrapFilter($"{strField} BETWEEN {Code.Bas.GD(c.j73Date1)} AND {Code.Bas.GD(c.j73Date2)}"), null, null, strAndOrZleva, c.j73BracketLeft, c.j73BracketRight);
                                //AQ( c.WrapFilter(strField + " BETWEEN " + Code.Bas.GD(c.j73Date1) + " AND " + Code.Bas.GD(c.j73Date2)), null, null, strAndOrZleva, c.j73BracketLeft, c.j73BracketRight);
                            }
                            else
                            {
                                if (c.j73Date1 != null)
                                {
                                    AQ(c.WrapFilter($"{strField} >= {Code.Bas.GD(c.j73Date1)}"), null, null, strAndOrZleva, c.j73BracketLeft, c.j73BracketRight);
                                    //AQ( c.WrapFilter(strField + ">=" + Code.Bas.GD(c.j73Date1)), null, null, strAndOrZleva, c.j73BracketLeft, c.j73BracketRight);
                                }
                                if (c.j73Date2 != null)
                                {
                                    AQ(c.WrapFilter($"{strField} <= {Code.Bas.GD(c.j73Date2)}"), null, null, strAndOrZleva, c.j73BracketLeft, c.j73BracketRight);
                                    //AQ( c.WrapFilter(strField + "<=" + Code.Bas.GD(c.j73Date2)), null, null, strAndOrZleva, c.j73BracketLeft, c.j73BracketRight);
                                }
                            }

                        }
                        if (c.FieldType == "number")
                        {
                            AQ(c.WrapFilter($"{strField} BETWEEN {Code.Bas.GN(c.j73Num1)} AND {Code.Bas.GN(c.j73Num2)}"), null, null, strAndOrZleva, c.j73BracketLeft, c.j73BracketRight);
                            //AQ( c.WrapFilter(strField + " BETWEEN " + Code.Bas.GN(c.j73Num1) + " AND " + Code.Bas.GN(c.j73Num2)), null, null, strAndOrZleva, c.j73BracketLeft, c.j73BracketRight);
                        }
                        break;
                    case "EQUAL":
                    case "NOT-EQUAL":
                        string strOper = "=";
                        if (c.j73Operator == "NOT-EQUAL")
                        {
                            strOper = "<>";
                        }
                        if (c.FieldType == "bool" || c.FieldType == "bool1")
                        {
                            if (c.SqlWrapper == null)
                            {
                                AQ(c.WrapFilter($"{strField} {strOper} {c.j73Value}"), "", null, strAndOrZleva, c.j73BracketLeft, c.j73BracketRight);
                                //AQ(c.WrapFilter(strField + " " + strOper + " " + c.j73Value), "", null, strAndOrZleva, c.j73BracketLeft, c.j73BracketRight);
                            }
                            else
                            {
                                if (c.j73Value == "1")
                                {
                                    AQ($"{c.FieldSqlSyntax} IN ({c.SqlWrapper})", null, null, strAndOrZleva, c.j73BracketLeft, c.j73BracketRight);

                                }
                                else
                                {
                                    AQ($"{c.FieldSqlSyntax} NOT IN ({c.SqlWrapper})", null, null, strAndOrZleva, c.j73BracketLeft, c.j73BracketRight);

                                }
                            }


                        }
                        if (c.FieldType == "bool1x")
                        {
                            AQ(c.FieldSqlSyntax, null, null,strAndOrZleva,c.j73BracketLeft,c.j73BracketRight);
                        }
                        if (c.FieldType == "string")
                        {                            
                            if (c.j73Value.Contains(";"))
                            {
                                
                                AQ($"{strField} {(c.j73Operator == "NOT-EQUAL" ? "NOT IN": "IN")} ('{Code.Bas.GSS(c.j73Value).Replace(";", "','")}')", null, null, strAndOrZleva, c.j73BracketLeft, c.j73BracketRight);
                            }
                            else
                            {
                                AQ($"{strField} {strOper} '{Code.Bas.GSS(c.j73Value)}'", null, null, strAndOrZleva, c.j73BracketLeft, c.j73BracketRight);
                            }
                            
                            
                        }
                        if (c.FieldType == "combo")
                        {

                            AQ(c.WrapFilter(strField + " " + strOper + " " + c.j73ComboValue.ToString()), null, null, strAndOrZleva, c.j73BracketLeft, c.j73BracketRight);

                        }
                        if (c.FieldType == "multi")
                        {
                            strOper = "IN";
                            if (c.j73Operator == "NOT-EQUAL")
                            {
                                strOper = "NOT IN";
                            }
                            
                            AQ(c.WrapFilter($"{strField} {strOper} ({c.j73Value})",c.j02AddValue,c.j11AddValue), "", null, strAndOrZleva, c.j73BracketLeft, c.j73BracketRight);
                            //AQ( c.WrapFilter(strField + " " + strOper + " (" + c.j73Value + ")"), "", null, strAndOrZleva, c.j73BracketLeft, c.j73BracketRight);
                        }
                        break;
                }
                recLast = c;
            }

            if (this.lisJ73.Count() > 1 && _lis.Count()> intFirstApplicableRowIndex)
            {
                _lis[intFirstApplicableRowIndex].BracketLeft += "(";               
                _lis.Last().BracketRight += ")";
            }
            if (bolNegace && _lis.Count()>0)    //negace podmínky filtru
            {                
                _lis[intFirstApplicableRowIndex].BracketLeft += "NOT (";
                _lis.Last().BracketRight += ")";
            }

        }


        private object get_param_value(string colType, string colValue)
        {
            if (String.IsNullOrEmpty(colValue))
            {
                return null;
            }
            if (colType == "num")
            {
                return Code.Bas.InDouble(colValue);
            }
            if (colType == "date")
            {
                return Convert.ToDateTime(colValue);
            }
            if (colType == "bool")
            {
                if (colValue == "1")
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }

            return colValue;
        }

        private string get_rawsql_value(string colType, string colValue)
        {
            if (String.IsNullOrEmpty(colValue))
            {
                if (colType == "num")
                {
                    return "0";
                }
                return "null";
            }
            if (colType == "num")
            {
                return colValue.Replace(",", ".");
            }
            if (colType == "date")
            {
                return $"convert(datetime,'{BO.Code.Bas.ObjectDate2String(Convert.ToDateTime(colValue), "dd.MM.yyyy")}',104)";
            }
            if (colType == "bool")
            {
                if (colValue == "1")
                {
                    return "1";
                }
                else
                {
                    return "0";
                }

            }

            return "'" + colValue.Replace("'", "") + "'";
        }

        public void Handle_p31TabQuery()
        {
            string s = null; string sf = " INNER JOIN p32Activity p32a ON za.p32ID=p32a.p32ID INNER JOIN p34ActivityGroup p34a ON p32a.p34ID=p34a.p34ID";
            switch (this.p31tabquery)
            {
                case "time":
                    s = "p34a.p33ID=1";
                    break;
                case "kusovnik":
                    s = "p34a.p33ID=3";
                    break;
                case "expense":
                    s = "p34a.p33ID IN (2,5) AND p34a.p34IncomeStatementFlag=1";
                    break;
                case "fee":
                    s = "p34a.p33ID IN (2,5) AND p34a.p34IncomeStatementFlag=2";
                    break;
            }

            if (s != null)
            {
                switch (this.Prefix)
                {
                    case "j02":
                        AQ($"EXISTS (SELECT 1 FROM p31Worksheet za{sf} WHERE za.j02ID=a.j02ID AND {s} AND za.p31Date between @p31date1 AND @p31date2)", null, null);
                        break;
                    case "p41":
                    case "le5":
                    case "le4":
                    case "le3":
                    case "le2":
                    case "le1":

                        AQ($"EXISTS (SELECT 1 FROM p31Worksheet za{sf} WHERE za.p41ID=a.p41ID AND {s} AND za.p31Date between @p31date1 AND @p31date2)", null, null);
                        break;
                    case "p28":

                        AQ($"EXISTS (SELECT 1 FROM p31Worksheet za{sf} INNER JOIN p41Project zx ON za.p41ID=zx.p41ID WHERE zx.p28ID_Client=a.p28ID AND {s} AND za.p31Date between @p31date1 AND @p31date2)", null, null);
                        break;
                    case "p56":

                        AQ($"EXISTS (SELECT 1 FROM p31Worksheet za{sf} WHERE za.p56ID=a.p56ID AND {s} AND za.p56ID IS NOT NULL AND za.p31Date between @p31date1 AND @p31date2)", null, null);
                        break;
                    case "p91":
                        AQ($"EXISTS (SELECT 1 FROM p31Worksheet za{sf} WHERE za.p91ID=a.p91ID AND {s} AND za.p31Date between @p31date1 AND @p31date2)", null, null);
                        break;
                    case "o23":
                        AQ($"EXISTS (SELECT 1 FROM p31Worksheet za{sf} WHERE za.o23ID=a.o23ID AND {s} AND za.p31Date between @p31date1 AND @p31date2)", null, null);
                        break;
                }

            }
        }

        public void Handle_p31StateQuery()
        {
            string s = null; string sf = null;
            switch (this.p31statequery)
            {
                case 1: //Rozpracované
                    this.iswip = true;
                    break;
                case 2://rozpracované s korekcí
                    s = "za.p71ID IS NULL AND za.p91ID IS NULL AND za.p72ID_AfterTrimming IS NOT NULL AND za.p31ExcludeBillingFlag IS NULL AND zb.p41BillingFlag<99";
                    sf = " INNER JOIN p41Project zb ON za.p41ID=zb.p41ID";
                    break;
                case 3://nevyúčtované                    
                    s = "za.p91ID IS NULL AND za.p31ExcludeBillingFlag IS NULL AND zb.p41BillingFlag<99";
                    sf = " INNER JOIN p41Project zb ON za.p41ID=zb.p41ID";
                    break;
                case 4://schválené
                    this.isapproved_and_wait4invoice = true; break;  //AQ("a.p71ID=1 AND a.p91ID IS NULL", null, null); break;
                case 5://schválené jako fakturovat
                    s = "za.p71ID=1 AND za.p72ID_AfterApprove=4 AND za.p91ID IS NULL"; break;
                case 6://schválené jako paušál
                    s = "za.p71ID=1 AND za.p72ID_AfterApprove=6 AND za.p91ID IS NULL"; break;
                case 7://schválené jako odpis
                    s = "za.p71ID=1 AND za.p72ID_AfterApprove IN (2,3) AND za.p91ID IS NULL";
                    break;
                case 8://schválené jako fakturovat později
                    s = "za.p71ID=1 AND za.p72ID_AfterApprove=7 AND za.p91ID IS NULL";
                    break;
                case 9://neschválené
                    s = "za.p71ID=2 AND za.p91ID IS NULL";
                    break;
                case 10://vyúčtované
                    this.isinvoiced = true; break;
                case 11://DRAFT vyúčtování
                    s = "zb.p91IsDraft=1"; sf = " INNER JOIN p91Invoice zb ON za.p91ID=zb.p91ID";
                    break;
                case 12://vyúčtované jako fakturovat
                    s = "za.p70ID=4"; sf = " INNER JOIN p91Invoice zb ON za.p91ID=zb.p91ID";
                    break;
                case 13://vyúčtované jako paušál
                    s = "za.p70ID=6"; sf = " INNER JOIN p91Invoice zb ON za.p91ID = zb.p91ID";
                    break;
                case 14://vyúčtované jako odpis
                    s = "za.p70ID IN (2,3)"; sf = " INNER JOIN p91Invoice zb ON za.p91ID=zb.p91ID";
                    break;
                case 15: //v archivu
                    s = "za.p31ValidUntil<GETDATE()"; break;
                case 16://rozpracované Fa aktivita
                    s = "za.p71ID IS NULL AND za.p91ID IS NULL AND zb.p32IsBillable=1"; sf = " INNER JOIN p32Activity zb ON za.p32ID=zb.p32ID INNER JOIN p41Project zc ON za.p41ID=zc.p41ID";
                    break;
                case 17://rozpracované Fa aktivita
                    s = "za.p71ID IS NULL AND za.p91ID IS NULL AND zb.p32IsBillable=0"; sf = " INNER JOIN p32Activity zb ON za.p32ID=zb.p32ID INNER JOIN p41Project zc ON za.p41ID=zc.p41ID";
                    break;
                case 21:    //100% uhrazené vyúčtování
                    s = "zb.p91Amount_Debt<1"; sf = " INNER JOIN p91Invoice zb ON za.p91ID=zb.p91ID";
                    break;
            }

            if (s != null)
            {
                switch (this.Prefix)
                {
                    case "j02":
                        AQ($"EXISTS (SELECT 1 FROM p31Worksheet za{sf} WHERE za.j02ID=a.j02ID AND {s} AND za.p31Date between @p31date1 AND @p31date2)", null, null);
                        break;
                    case "p41":
                    case "le5":
                    case "le4":
                    case "le3":
                    case "le2":
                    case "le1":

                        AQ($"EXISTS (SELECT 1 FROM p31Worksheet za{sf} WHERE za.p41ID=a.p41ID AND {s} AND za.p31Date between @p31date1 AND @p31date2)", null, null);
                        break;
                    case "p28":

                        AQ($"EXISTS (SELECT 1 FROM p31Worksheet za{sf} INNER JOIN p41Project zx ON za.p41ID=zx.p41ID WHERE zx.p28ID_Client=a.p28ID AND {s} AND za.p31Date between @p31date1 AND @p31date2)", null, null);
                        break;
                    case "p56":

                        AQ($"EXISTS (SELECT 1 FROM p31Worksheet za{sf} WHERE za.p56ID=a.p56ID AND {s} AND za.p56ID IS NOT NULL AND za.p31Date between @p31date1 AND @p31date2)", null, null);
                        break;
                    case "o23":
                        AQ($"EXISTS (SELECT 1 FROM p31Worksheet za{sf} WHERE za.o23ID=a.o23ID AND {s} AND za.p31Date between @p31date1 AND @p31date2)", null, null);
                        break;
                }

            }
        }


        private void Handle_AppendCloudQuery(string prefixdb)
        {
            //zásadní filtrovací podmínka pro TotalCloud hosting dotazy
            string s = null;

            switch (prefixdb)
            {
                case "p31":
                case "p32":
                case "p40":
                    s = "p34x.x01ID=@x01id_hostingmode"; break;

                case "p28":
                    s = "p29x.x01ID=@x01id_hostingmode"; break;
                case "p91":
                    s = "p92x.x01ID=@x01id_hostingmode"; break;
                case "p90":
                    s = "p89x.x01ID=@x01id_hostingmode"; break;
                case "j04":
                case "j02":
                case "p11":
                case "j79":
                case "p55":
                case "j05":
                case "j13":
                case "j92":
                case "b05":
                case "p68":
                case "j06":
                    s = "x67x.x01ID=@x01id_hostingmode"; break;
                case "p56":
                case "p58":
                    s = "p57x.x01ID=@x01id_hostingmode"; break;
                case "o22":
                    s = "o21x.x01ID=@x01id_hostingmode"; break;
                case "p41":
                case "le5":
                case "le4":
                case "le3":
                case "le2":
                case "le1":
                //case "p42":
                    s = "p07x.x01ID=@x01id_hostingmode"; break;
                case "o51":
                    s = "o53x.x01ID=@x01id_hostingmode"; break;
                case "o23":
                    s = "o18x.x01ID=@x01id_hostingmode"; break;
                case "r01":
                    s = "r02.x01ID=@x01id_hostingmode"; break;
                case "o42":
                    s = "j40x.x01ID=@x01id_hostingmode"; break;
                case "b02":
                case "b06":
                    s = "b01x.x01ID=@x01id_hostingmode"; break;
                case "p60":
                    s= "p57x.x01ID=@x01id_hostingmode"; break;
                case "p84":
                    s = "p83x.x01ID=@x01id_hostingmode"; break;
                case "p12":
                    s = "x67x.x01ID=@x01id_hostingmode"; break;
                case "x51":
                case "x97":
                    s = "1=1"; break;
                case "j27":
                case "x15":
                case "j19":
                case "p71":
                case "p70":
                case "p72":
                case "o58":
                    return;
                default:
                    s = "a.x01ID=@x01id_hostingmode"; break;

            }
            AQ(s, "x01id_hostingmode", this.CurrentUser.x01ID);

        }



    }
}
