

using BL.Singleton.ColumnsProvider;
using DocumentFormat.OpenXml.Drawing;

namespace BL
{

    public class TheColumnsProvider
    {
        //private readonly BL.RunningApp _app;
        private readonly Singleton.TheEntitiesProvider _ep;
        private readonly Singleton.TheTranslator _tt;
        private List<BO.TheGridColumn> _lis;


        public TheColumnsProvider(Singleton.TheEntitiesProvider ep, Singleton.TheTranslator tt)
        {
            //_app = runningapp;
            _ep = ep;
            _tt = tt;
            _lis = new List<BO.TheGridColumn>();
            SetupPallete();
            Handle_Translate();

        }

        public void Refresh()
        {
            _lis = new List<BO.TheGridColumn>();
            SetupPallete();
            Handle_Translate();
        }



        private void Handle_Translate()
        {
            //Překlad do ostatních jazyků
            foreach (var col in _lis.Where(p => p.Header.Length > 2))
            {
                bool b = true;
                if (col.Header.Length > 3 && col.Header.Substring(0, 3) == "Col")
                {
                    b = false;
                }
                if (b)
                {
                    col.TranslateLang1 = _tt.DoTranslate(col.Header, 1, "TheColumnsProvider");
                    col.TranslateLang2 = _tt.DoTranslate(col.Header, 2, "TheColumnsProvider");
                }

            }




        }
        private void SetupPallete()
        {
            _lis.InsertRange(0, new defColumnsProvider().getColumns());
            _lis.InsertRange(0, new j02ColumnsProvider().getColumns());
            _lis.InsertRange(0, new p28ColumnsProvider().getColumns());
            _lis.InsertRange(0, new p41ColumnsProvider().getColumns());

            _lis.InsertRange(0, new p31ColumnsProvider().getColumns());
            _lis.InsertRange(0, new o23ColumnsProvider().getColumns());

            _lis.InsertRange(0, new le1ColumnsProvider().getColumns());
            _lis.InsertRange(0, new le2ColumnsProvider().getColumns());
            _lis.InsertRange(0, new le3ColumnsProvider().getColumns());
            _lis.InsertRange(0, new le4ColumnsProvider().getColumns());
            _lis.InsertRange(0, new le5ColumnsProvider().getColumns());

            _lis.InsertRange(0, new p90ColumnsProvider().getColumns());
            _lis.InsertRange(0, new p91ColumnsProvider().getColumns());

            _lis.InsertRange(0, new p56ColumnsProvider().getColumns());
            _lis.InsertRange(0, new o22ColumnsProvider().getColumns());
            _lis.InsertRange(0, new p11ColumnsProvider().getColumns());
            _lis.InsertRange(0, new b05ColumnsProvider().getColumns());
            _lis.InsertRange(0, new p84ColumnsProvider().getColumns());
            _lis.InsertRange(0, new fpColumnsProvider().getColumns());

            string strLastEntity = "";
            string strLastEntityAlias = "";
            foreach (var c in _lis)
            {
                if (c.Entity == strLastEntity)
                {
                    c.EntityAlias = strLastEntityAlias;
                }
                else
                {
                    c.EntityAlias = _ep.ByTable(c.Entity).AliasSingular;
                }
                strLastEntity = c.Entity;
                strLastEntityAlias = c.EntityAlias;
            }




        }


        public List<BO.TheGridColumn> getDefaultPallete(bool bolComboColumns, BO.baseQuery mq, BL.Factory f)
        {

            List<BO.TheGridColumn> ret = new List<BO.TheGridColumn>();
            IEnumerable<BO.TheGridColumn> qry = null;
            if (bolComboColumns)
            {
                qry = _lis.Where(p => p.Prefix == mq.PrefixDb && (p.DefaultColumnFlag == BO.TheGridDefColFlag.GridAndCombo || p.DefaultColumnFlag == BO.TheGridDefColFlag.ComboOnly));

            }
            else
            {
                qry = _lis.Where(p => p.Prefix == mq.PrefixDb && (p.DefaultColumnFlag == BO.TheGridDefColFlag.GridAndCombo || p.DefaultColumnFlag == BO.TheGridDefColFlag.GridOnly));
            }

            foreach (BO.TheGridColumn c in qry)
            {
                ret.Add(Clone2NewInstance(c));
            }

            List<BO.EntityRelation> rels = _ep.getApplicableRelations(mq.Prefix, f);   //všechny dostupné relace pro entitu mq.prefix

            if (rels.Count() == 0)
            {
                return ret; //divný stav - je třeba později řešit!!!
            }
            switch (mq.Prefix)
            {
                case "j02":



                    break;



                case "j61":

                    ret.Add(InhaleColumn4Relation("j61_j02", "j02User", "fullname_desc", rels, bolComboColumns));

                    break;
                case "p28":


                    break;
                case "p31":
                case "app":
                    ret.Add(InhaleColumn4Relation("p31_j02", "j02User", "fullname_desc", rels, bolComboColumns));
                    ret.Add(InhaleColumn4Relation("p31_p41_p28", "p28Contact", "p28Name", rels, bolComboColumns));
                    ret.Add(InhaleColumn4Relation("p31_p41", "p41Project", "p41Name", rels, bolComboColumns));
                    ret.Add(InhaleColumn4Relation("p31_p32", "p32Activity", "p32Name", rels, bolComboColumns));
                    break;
                case "p36":
                    ret.Add(InhaleColumn4Relation("p36_j02", "j02User", "fullname_desc", rels, bolComboColumns));
                    ret.Add(InhaleColumn4Relation("p36_j11", "j11Team", "j11Name", rels, bolComboColumns));
                    break;

                case "p51":
                    ret.Add(InhaleColumn4Relation("p51_j27", "j27Currency", "j27Code", rels, bolComboColumns));
                    break;
                case "p53":
                    ret.Add(InhaleColumn4Relation("p53_j27", "j27Currency", "j27Code", rels, bolComboColumns));
                    ret.Add(InhaleColumn4Relation("p53_x15", "x15VatRateType", "x15Name", rels, bolComboColumns));
                    break;
                case "p32":
                    ret.Add(InhaleColumn4Relation("p32_p34", "p34ActivityGroup", "p34Name", rels, bolComboColumns));
                    ret.Add(InhaleColumn4Relation("p32_p95", "p95InvoiceRow", "p95Name", rels, bolComboColumns));
                    ret.Add(InhaleColumn4Relation("p32_p38", "p38ActivityTag", "p38Name", rels, bolComboColumns));
                    break;
                case "b07":
                    ret.Add(InhaleColumn4Relation("b07_p28", "p28Contact", "p28Name", rels, bolComboColumns));
                    ret.Add(InhaleColumn4Relation("b07_p41", "p41Project", "p41Name", rels, bolComboColumns));
                    break;
                case "p90":
                    ret.Add(InhaleColumn4Relation("p90_j27", "j27Currency", "j27Code", rels, bolComboColumns));
                    ret.Add(InhaleColumn4Relation("p90_p28", "p28Contact", "p28Name", rels, bolComboColumns));
                    break;
                case "p92":
                    //ret.Add(InhaleColumn4Relation("p92_j27", "j27Currency", "j27Code", rels, bolComboColumns));
                    //ret.Add(InhaleColumn4Relation("p92_p93", "p93InvoiceHeader", "p93Name", rels, bolComboColumns));
                    //ret.Add(InhaleColumn4Relation("p92_x15", "x15VatRateType", "x15Name", rels, bolComboColumns));
                    //ret.Add(InhaleColumn4Relation("p92_x38", "x38CodeLogic", "x38Name", rels, bolComboColumns));
                    break;

                case "x31":


                    break;
                case "o40":
                    ret.Add(InhaleColumn4Relation("o40_j02", "j02User", "fullname_desc", rels, bolComboColumns));
                    break;
                case "m62":
                    ret.Add(InhaleColumn4Relation("m62_j27slave", "j27Currency", "j27Code", rels, bolComboColumns));
                    break;
                case "o51":
                    ret.Add(InhaleColumn4Relation("o51_o53", "o53TagGroup", "o53Name", rels, bolComboColumns));
                    break;


            }

            if (!f.CurrentUser.IsRatesAccess && ret.Any(p => p.IHRC == true))
            {
                ret = ret.Where(p => p.IHRC == false).ToList();
            }
            if (!f.CurrentUser.IsVysledovkyAccess && ret.Any(p => p.VYSL == true))
            {
                ret = ret.Where(p => p.VYSL == false).ToList();
            }

            return ret;


        }
        public List<BO.TheGridColumn> AllColumns()
        {

            return _lis;


        }
        private BO.TheGridColumn InhaleColumn4Relation(string strRelName, string strFieldEntity, string strFieldName, List<BO.EntityRelation> applicable_rels, bool bolComboColumns)
        {


            BO.TheGridColumn c0 = ByUniqueName("a__" + strFieldEntity + "__" + strFieldName);
            BO.TheGridColumn c = Clone2NewInstance(c0);

            BO.EntityRelation rel = applicable_rels.Where(p => p.RelName == strRelName).First();
            c.RelName = strRelName;
            c.RelSql = rel.SqlFrom;
            if (rel.RelNameDependOn != null)
            {
                c.RelSqlDependOn = applicable_rels.Where(p => p.RelName == rel.RelNameDependOn).First().SqlFrom;    //relace závisí na jiné relaci
            }

            if (c.NotShowRelInHeader == true)
            {
                return c;   //nezobrazovat u sloupce název relace
            }

            if (bolComboColumns)
            {
                c.Header = rel.AliasSingular;
            }
            else
            {
                c.Header = c.Header + " [" + rel.AliasSingular + "]";

            }


            return c;
        }
        public BO.TheGridColumn ByUniqueName(string strUniqueName)
        {
            if (_lis.Where(p => p.UniqueName == strUniqueName).Count() > 0)
            {
                return _lis.Where(p => p.UniqueName == strUniqueName).First();
            }
            else
            {
                return null;
            }
        }
        private BO.TheGridColumn Clone2NewInstance(BO.TheGridColumn c)
        {
            return new BO.TheGridColumn()
            {
                Entity = c.Entity,
                EntityAlias = c.EntityAlias,
                Field = c.Field,
                FieldType = c.FieldType,
                FixedWidth = c.FixedWidth,
                Header = c.Header,
                SqlSyntax = c.SqlSyntax,
                IsFilterable = c.IsFilterable,
                IsShowTotals = c.IsShowTotals,
                IsTimestamp = c.IsTimestamp,
                RelName = c.RelName,
                RelSql = c.RelSql,
                RelSqlDependOn = c.RelSqlDependOn,
                RelSqlInCol = c.RelSqlInCol,
                NotShowRelInHeader = c.NotShowRelInHeader,
                TranslateLang1 = c.TranslateLang1,
                TranslateLang2 = c.TranslateLang2,
                TranslateLang3 = c.TranslateLang3,
                IsNotUseP31TOTALS = c.IsNotUseP31TOTALS,
                IHRC = c.IHRC,
                VYSL = c.VYSL,
                SqlExplicitGroupBy = c.SqlExplicitGroupBy,
                Tooltip = c.Tooltip,
                IsHours=c.IsHours
            };

        }




        public List<BO.TheGridColumn> ParseTheGridColumns(string strPrimaryPrefix, string strJ72Columns, BL.Factory f)
        {
            //v strJ72Columns je čárkou oddělený seznam sloupců z pole j72Columns: název relace+__+entita+__+field
            //v lisFFs se předávají další sloupce

            List<BO.TheGridColumn> lisFFs = null;
            List<BO.EntityRelation> applicable_rels = _ep.getApplicableRelations(strPrimaryPrefix, f);



            if (strJ72Columns.Contains("Free"))
            {
                lisFFs = new BL.ffColumnsProvider(f, strPrimaryPrefix).getColumns();
            }
            else
            {
                return Handle_ParseTheGridColumns_WithoutFFs(strPrimaryPrefix, strJ72Columns, f, applicable_rels);  //na vstupu nejsou uživatelsky definované sloupce
            }

            List<string> sels = BO.Code.Bas.ConvertString2List(strJ72Columns, ",");

            Remove_Unwanted_Columns(ref strJ72Columns, ref sels, f);
            bool bolAllowRates = f.CurrentUser.IsRatesAccess;
            bool bolAllowVysledovka = f.CurrentUser.IsVysledovkyAccess;

            List<BO.TheGridColumn> ret = new List<BO.TheGridColumn>();

            string[] arr;
            for (int i = 0; i < sels.Count(); i++)
            {
                arr = sels[i].Split("__");
                BO.TheGridColumn colSource = null;
                if (arr.Length > 2)
                {
                    try
                    {
                        if (sels[i].Contains("Free"))
                        {
                            colSource = lisFFs.First(p => p.Entity == arr[1] && p.Field == arr[2]);
                        }
                        else
                        {
                            colSource = _lis.First(p => p.Entity == arr[1] && p.Field == arr[2]);
                        }
                    }
                    catch
                    {

                        BO.Code.File.LogError($"Nelze najít sloupec: Entity: {arr[1]}, Field: {arr[2]}, strPrimaryPrefix: {strPrimaryPrefix}", "singleton", "ParseTheGridColumns");

                    }
                }

                if (colSource != null)
                {
                    BO.TheGridColumn c = CreateNewInstanceColumn(colSource, arr, f.CurrentUser.j02LangIndex, applicable_rels);
                    if (c != null)
                    {
                        if ((i == sels.Count - 1) && (c.FieldType == "num" || c.FieldType == "num0" || c.FieldType == "num3"))
                        {
                            c.CssClass = "tdn_lastcol";
                        }
                        if (c.IHRC && c.VYSL)
                        {
                            if (bolAllowRates && bolAllowVysledovka)
                            {
                                ret.Add(c); //sloupec, který vyžaduje přístup k sazbám i k výsledovkám
                            }
                        }
                        else
                        {
                            if (c.IHRC || c.VYSL)
                            {
                                if (c.IHRC && bolAllowRates)
                                {
                                    ret.Add(c); //sloupec fakturačních cen a honorářů
                                }
                                if (c.VYSL && bolAllowVysledovka)
                                {
                                    ret.Add(c); //sloupec výsledovky a nákladových a honorářů
                                }
                            }
                            else
                            {
                                ret.Add(c); //sloupec bez omezení
                            }
                        }


                        //if (bolAllowRates || (!bolAllowRates && !c.IHRC))
                        //{
                        //    ret.Add(c);
                        //}

                    }

                }

            }

            ret.Last().IsLastColInGrid = true;



            return ret;

        }

        private void Remove_Unwanted_Columns(ref string strJ72Columns, ref List<string> sels, BL.Factory f)
        {
            if (f.CurrentUser.j04GridColumnsExclude != null)
            {
                //z přehledu otestovat zakázané sloupce                
                var exc = f.CurrentUser.j04GridColumnsExclude.Split(",");
                for (int i = 0; i < exc.Length; i++)
                {
                    if (sels.Contains(exc[i]))
                    {
                        sels.Remove(exc[i]);
                    }
                }
                strJ72Columns = string.Join(",", sels);
            }


        }

        private List<BO.TheGridColumn> Handle_ParseTheGridColumns_WithoutFFs(string strPrimaryPrefix, string strJ72Columns, BL.Factory f, List<BO.EntityRelation> applicable_rels)
        {
            //rychlejší práce
            List<string> sels = BO.Code.Bas.ConvertString2List(strJ72Columns, ",");
            List<BO.TheGridColumn> ret = new List<BO.TheGridColumn>();

            Remove_Unwanted_Columns(ref strJ72Columns, ref sels, f);
            bool bolAllowRates = f.CurrentUser.IsRatesAccess;
            bool bolAllowVysledovka = f.CurrentUser.IsVysledovkyAccess;

            string[] arr;

            for (int i = 0; i < sels.Count; i++)
            {
                if (sels[i] == "undefined")
                {
                    continue;
                }

                arr = sels[i].Split("__");
                try
                {
                    BO.TheGridColumn c = CreateNewInstanceColumn(_lis.First(p => p.Entity == arr[1] && p.Field == arr[2]), arr, f.CurrentUser.j02LangIndex, applicable_rels);
                    if (c != null)
                    {
                        if ((i == sels.Count - 1) && (c.FieldType == "num" || c.FieldType == "num0" || c.FieldType == "num3"))
                        {
                            c.CssClass = "tdn_lastcol";
                        }
                        if (c.IHRC && c.VYSL)
                        {
                            if (bolAllowRates && bolAllowVysledovka)
                            {
                                ret.Add(c); //sloupec, který vyžaduje přístup k sazbám i k výsledovkám
                            }
                        }
                        else
                        {
                            if (c.IHRC || c.VYSL)
                            {
                                if (c.IHRC && bolAllowRates)
                                {
                                    ret.Add(c); //sloupec fakturačních cen a honorářů
                                }
                                if (c.VYSL && bolAllowVysledovka)
                                {
                                    ret.Add(c); //sloupec výsledovky a nákladových a honorářů
                                }
                            }
                            else
                            {
                                ret.Add(c); //sloupec bez omezení
                            }
                        }



                    }
                }
                catch
                {
                    BO.Code.File.LogError($"Nelze najít: Entity: {arr[1]}, Field: {arr[2]}", "singleton", "Handle_ParseTheGridColumns_WithoutFFs");

                }


            }
            if (ret.Count() > 0)
            {
                ret.Last().IsLastColInGrid = true;
            }


            return ret;
        }

        private BO.TheGridColumn CreateNewInstanceColumn(BO.TheGridColumn colSource, string[] arr, int intLangIndex, List<BO.EntityRelation> applicable_rels)
        {
            BO.TheGridColumn c = Clone2NewInstance(colSource);
            switch (intLangIndex)
            {
                case 1:
                    c.Header = c.TranslateLang1;
                    break;
                case 2:
                    c.Header = c.TranslateLang2;
                    break;
                case 3:
                    c.Header = c.TranslateLang3;
                    break;
                default:
                    c.Header = c.Header;
                    break;
            }
            if (arr[0] == "a")
            {
                c.RelName = null;   //jednoduchý sloupec bez relace
            }
            else
            {
                c.RelName = arr[0]; //název relace v sql dotazu

                var qryRel = applicable_rels.Where(p => p.RelName == c.RelName);
                if (qryRel.Count() == 0)
                {
                    //volá se relace, která neexistuje!!!! - zásadní chyba
                    BO.Code.File.LogError("qryRel.Count() == 0, c.RelName: " + c.RelName, null, "CreateNewInstanceColumn");
                    return null;
                }
                BO.EntityRelation rel = applicable_rels.First(p => p.RelName == c.RelName);
                c.RelSql = rel.SqlFrom;    //sql klauzule relace    
                if (c.NotShowRelInHeader == false)
                {
                    c.Header = c.Header + " [" + rel.AliasSingular + "]";   //zobrazovat název entity v záhlaví sloupce                           
                }


                if (rel.RelNameDependOn != null)
                {
                    c.RelSqlDependOn = applicable_rels.Where(p => p.RelName == rel.RelNameDependOn).First().SqlFrom;    //relace závisí na jiné relaci
                }
            }



            return c;
        }

        public List<BO.TheGridColumnFilter> ParseAdhocFilterFromString(string strJ72Filter, IEnumerable<BO.TheGridColumn> explicit_cols)
        {
            var ret = new List<BO.TheGridColumnFilter>();
            if (String.IsNullOrEmpty(strJ72Filter) == true) return ret;


            List<string> lis = BO.Code.Bas.ConvertString2List(strJ72Filter, "$$$");
            foreach (var s in lis)
            {
                List<string> arr = BO.Code.Bas.ConvertString2List(s, "###");
                if (explicit_cols.Where(p => p.UniqueName == arr[0]).Count() > 0)
                {
                    var c = new BO.TheGridColumnFilter() { field = arr[0], oper = arr[1], value = arr[2] };
                    c.BoundColumn = explicit_cols.Where(p => p.UniqueName == arr[0]).First();

                    ParseFilterValue(ref c);
                    ret.Add(c);
                }


            }
            return ret;
        }

        private void ParseFilterValue(ref BO.TheGridColumnFilter col)
        {

            {
                if (col.value.Contains("|"))
                {
                    var a = col.value.Split("|");
                    col.c1value = a[0];
                    col.c2value = a[1];
                }
                else
                {
                    col.c1value = col.value;
                    col.c2value = "";
                }
                switch (col.oper)
                {
                    case "1":
                        {
                            col.value_alias = "Je prázdné";
                            break;
                        }

                    case "2":
                        {
                            col.value_alias = "Není prázdné";
                            break;
                        }

                    case "3":  // obsahuje
                        {
                            col.value_alias = col.c1value;
                            break;
                        }

                    case "5":  // začíná na
                        {

                            col.value_alias = "[*=] " + col.c1value;
                            break;
                        }

                    case "6":  // je rovno
                        {
                            col.value_alias = "[=] " + col.c1value;
                            break;
                        }

                    case "7":  // není rovno
                        {
                            col.value_alias = "[<>] " + col.c1value;
                            break;
                        }

                    case "8":
                        {
                            col.value_alias = "ANO";
                            break;
                        }

                    case "9":
                        {
                            col.value_alias = "NE";
                            break;
                        }

                    case "10": // je větší než nula
                        {
                            col.value_alias = "větší než 0";
                            break;
                        }

                    case "11":
                        {
                            col.value_alias = "0 nebo prázdné";
                            break;
                        }

                    case "4":  // interval
                        {

                            if (col.BoundColumn.FieldType == "date" | col.BoundColumn.FieldType == "datetime")
                            {
                                col.value_alias = col.c1value + " - " + col.c2value;   // datum
                            }
                            else
                            {
                                col.value_alias = col.c1value + " - " + col.c2value;
                            }    // číslo

                            break;
                        }
                }





            }


        }


    }
}
