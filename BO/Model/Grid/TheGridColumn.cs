using System;
using System.Collections.Generic;
using System.Text;

namespace BO
{
    public enum TheGridDefColFlag
    {
        _none = 0,
        GridAndCombo = 1,
        GridOnly = 2,
        ComboOnly = 3
    }
    public class TheGridColumn
    {
        private string _CssClass;
        private string _Entity;
        private string _Field;
        private string _Prefix;
        public bool IsTimestamp;
        
        public string FieldType { get; set; }   //string, bool, int, num, date, datetime
        public string Header { get; set; }
        public string TranslateLang1 { get; set; }
        public string TranslateLang2 { get; set; }
        public string TranslateLang3 { get; set; }
        public string SqlSyntax { get; set; }
        public bool IsNotUseP31TOTALS { get; set; } //zakázat pole v nástroji SOUČTY
        public string SqlExplicitGroupBy { get; set; }
        public TheGridDefColFlag DefaultColumnFlag { get; set; }
        public string DesignerGroup { get; set; }
        public bool IsSortable { get; set; } = true;
        public bool IsFilterable { get; set; } = true;
        public bool IsFulltext { get; set; }
        public bool IsShowTotals { get; set; }
        public bool IsHours { get; set; }
        public int FixedWidth { get; set; }
        public string EntityAlias { get; set; }
        public bool NotShowRelInHeader { get; set; }
        public string Tooltip { get; set; }
        public string Rezerva { get; set; }
        
       
        public string RelName { get; set; } //název relace ve from klauzuly - naplní se v getSelectedPallete
        public string RelSql { get; set; }  //sql relace from klauzule - naplní se v getSelectedPallete
        public string RelSqlDependOn { get; set; } //sql relace na které je závislá RelSql
        public string RelSqlInCol { get; set; } //sql relace definované přímo ve sloupci
        
        public bool IHRC { get; set; }    //pokud uživatel nemá přístup k sazbám a honorářům
        public bool VYSL { get; set; }    //pokud uživatel nemá přístup k výsledovkám a k nákladovým sazbám
        public string VisibleWithinEntityOnly { get; set; } //aplikovatelné pouze pro kategorie

        public bool IsLastColInGrid { get; set; }   //poslední sloupec v gridu

        public string Entity
        {
            get
            {
                return _Entity;
            }
            set
            {
                _Entity = value;

                _Prefix = _Entity.Substring(0, 3);
            }
        }
        public string Field
        {
            get
            {
                return _Field;
            }
            set
            {
                _Field = value;
               
            }
        }
       

        public string CssClass
        {
            get
            {
                if (_CssClass == null)
                {
                    if (FieldType == "num" || FieldType == "num0" || FieldType=="num3" || FieldType == "num1" || FieldType == "num4" || FieldType=="num5" || FieldType=="filesize") _CssClass = "tdn";
                    if (FieldType == "bool") _CssClass = "tdb";
                }                
                return _CssClass;
            }
            set
            {
                _CssClass = value;
            }
        }
        public string HtmlTableType
        {
            get
            {
                switch (this.FieldType)
                {
                    case "num0":
                    case "int":
                        return "I";
                    case "num":
                    case "num2":
                    case "num4":                        
                        return "N";
                    
                    case "bool":
                        return "B";
                    case "date":
                        return "D";
                    case "datetimesec":
                    case "datetime":
                        return "DT";

                    default:
                        return "S";

                }
            }
        }
        public string UniqueName
        {
            get
            {
                if (RelName == null)
                {
                    return $"a__{_Entity}__{_Field}";
                    //return "a__" + _Entity + "__" + _Field;
                }
                else
                {
                    return $"{RelName}__{_Entity}__{_Field}";
                    //return RelName + "__" + _Entity + "__" + _Field;
                }
            }
        }
        


        public string getColumnWidthPixels(bool bolTdWidthAuto)
        {
            if (this.FixedWidth == 0)
            {
                //není explicitně definována šířka sloupce
                if (bolTdWidthAuto)
                {
                    return "auto";
                                                         
                }
                else
                {
                    return "200px";
                }
                
            }
            else
            {
                return $"{this.FixedWidth}px";
            }
        }
        

        public string NormalizedTypeName
        {
            get
            {
                if (FieldType == "num0" || FieldType == "num" || FieldType=="num3" || FieldType=="num4" || FieldType=="num5" || FieldType=="int") return "num";
                if (FieldType == "date" || FieldType == "datetime" || FieldType== "datetimesec") return "date";
                return this.FieldType;
            }
            
        }
        
        public string Prefix
        {
            get
            {
                return _Prefix;
            }
        }

        public string getFinalSqlSyntax_SELECT()
        {                        
            if (this.SqlSyntax == null)
            {
                if (this.RelName == null)
                {
                    return $"a.{this.Field} AS {this.UniqueName}";    //pole z primární tabulky strPrimaryTablePrefix
                    //return "a." + this.Field + " AS " + this.UniqueName;    //pole z primární tabulky strPrimaryTablePrefix
                }
                else
                {
                    return $"{this.RelName}.{this.Field} AS {this.UniqueName}"; //v RelName je uložený název relace GRIDU
                    //return this.RelName + "." + this.Field + " AS " + this.UniqueName;   //v RelName je uložený název relace GRIDU
                }               
            }
            else
            {                
                if (this.RelName == null)
                {
                    return $"{this.SqlSyntax} AS {this.UniqueName}";
                    //return this.SqlSyntax + " AS " + this.UniqueName;
                }
                else
                {
                    this.SqlSyntax = this.SqlSyntax.Replace("_relname_", this.RelName);
                    if (this.SqlSyntax.Contains("a.") || this.SqlSyntax.Contains("relname."))
                    {
                        return this.SqlSyntax.Replace("a.", this.RelName + ".").Replace("relname.", this.RelName+".") + " AS " + this.UniqueName;
                    }
                    else
                    {
                        return $"{this.SqlSyntax} AS {this.UniqueName}";
                        //return this.SqlSyntax + " AS " + this.UniqueName;
                    }
                    
                }
                                        
            }
            
        }
        public string getFinalSqlSyntax_WHERE()
        {
            if (this.SqlSyntax == null)
            {
                if (this.RelName == null)
                {
                    return $"a.{this.Field}";    //pole z primární tabulky strPrimaryTablePrefix
                }
                else
                {
                    return $"{this.RelName}.{this.Field}";                    
                }                    
            }
            else
            {                
                if (this.RelName == null)
                {
                    return this.SqlSyntax;
                }
                else
                {                    
                    return this.SqlSyntax.Replace("a.", this.RelName + ".").Replace("relname.", this.RelName).Replace("_relname_", this.RelName);
                }                                    
            }

        }
        public string getFinalSqlSyntax_ORDERBY()
        {
            return getFinalSqlSyntax_WHERE();  //SQL pro ORDERBY je stejná jako WHERE
        }


        public string getFinalSqlSyntax_SUM()
        {
            
            if (!this.IsShowTotals) return $"NULL AS {this.UniqueName}";
            if (this.SqlSyntax == null)
            {
                if (this.RelName == null)
                {
                    return $"SUM(a.{this.Field}) AS {this.UniqueName}";    //pole z primární tabulky strPrimaryTablePrefix
                }
                else
                {
                    return $"SUM({this.RelName}.{this.Field}) AS {this.UniqueName}";
                    //return "SUM("+this.RelName + "." + this.Field + ") AS " +this.UniqueName;
                }
            }
            else
            {                
                if (this.RelName == null)
                {
                    return $"SUM({this.SqlSyntax}) AS {this.UniqueName}";
                }
                else
                {
                    return $"SUM({this.SqlSyntax.Replace("a.", this.RelName + ".").Replace("relname.", this.RelName).Replace("_relname_", this.RelName)}) AS {this.UniqueName}";
                    //return "SUM(" + this.SqlSyntax.Replace("a.",this.RelName+".").Replace("relname.", this.RelName).Replace("_relname_", this.RelName) + ") AS " +this.UniqueName;
                }
                    
            }

        }

        //public string getFinalSqlSyntax_PivotSum()
        //{

        //    if (!this.IsShowTotals) return "NULL";
        //    if (this.SqlSyntax == null)
        //    {
        //        if (this.RelName == null)
        //        {
        //            return $"SUM(a.{this.Field})";    //pole z primární tabulky strPrimaryTablePrefix
        //        }
        //        else
        //        {
        //            return $"SUM({this.RelName}.{this.Field})";                    
        //        }
        //    }
        //    else
        //    {
        //        if (this.RelName == null)
        //        {
        //            return $"SUM({this.SqlSyntax})";
        //        }
        //        else
        //        {
        //            return $"SUM({this.SqlSyntax.Replace("a.", this.RelName + ".").Replace("relname.", this.RelName).Replace("_relname_", this.RelName)})";                    
        //        }

        //    }

        //}

        public string getSymbol()
        {
            switch (this.FieldType)
            {
                case "num":
                    return "0.0";
                case "num0":                    
                    return "000";
                case "int":
                    return "000";
                case "date":
                    return "&#128197;";
                case "datetime":
                    return "&#128197;";
                case "bool":
                    return "&#10004;";
                default:
                    return "ab";
            }
        }

        public string getImage()
        {
            switch (this.FieldType)
            {
                case "num":
                    return "type_number.png";
                case "num0":
                    return "type_number.png";
                case "int":
                    return "type_number.png";
                case "date":
                    return "type_date.png";
                case "datetime":
                    return "type_datetime.png";
                case "bool":
                    return "type_checkbox.png";
                default:
                    return "type_text.png";
            }
        }

        public string getFinalSqlSyntax_GROUPBY_SELECT()
        {
            string ops = "MIN";
            if (this.IsShowTotals) ops = "SUM";
            string bitops = null;
            if (this.FieldType == "bool") bitops = "+0";    //bit sloupec je třeba sčítat s nulou, protože sqlserver nepodporuje MIN/MAX pro bit sloupce!!

            if (this.SqlSyntax == null)
            {
                if (this.RelName == null)
                {
                    return $"{ops}(a.{this.Field}{bitops}) AS {this.UniqueName}";
                    
                    
                }
                else
                {
                    return $"{ops}({this.RelName}.{this.Field}{bitops}) AS {this.UniqueName}";
                    

                }
            }
            else
            {
                if (this.RelName == null)
                {
                    return $"{ops}({this.SqlSyntax}{bitops}) AS {this.UniqueName}";
                                    

                }
                else
                {
                    this.SqlSyntax = this.SqlSyntax.Replace("_relname_", this.RelName);
                    
                    if (this.SqlSyntax.Contains("a.") || this.SqlSyntax.Contains("relname."))
                    {
                        return $"{ops}({this.SqlSyntax.Replace("a.", this.RelName + ".").Replace("relname.", this.RelName + ".")}{bitops}) AS {this.UniqueName}";
                        
                    }
                    else
                    {
                        return $"{ops}({this.SqlSyntax}{bitops}) AS {this.UniqueName}";
                        
                    }

                }

            }

          

        }

        public string getFinalSqlSyntax_GROUPBY()
        {
            string thesql = this.SqlExplicitGroupBy;
            if (thesql == null)
            {
                thesql = this.SqlSyntax;
            }
            
            if (thesql == null)
            {
                string strGroupByField = $"{this.Prefix}ID";
                
                if (this.RelName == null)
                {
                    return $"a.{strGroupByField}";
                    

                }
                else
                {
                    return $"{this.RelName}.{strGroupByField}";                    

                }
            }
            else
            {
                if (this.RelName == null)
                {
                    return thesql;

                }
                else
                {
                    thesql = thesql.Replace("_relname_", this.RelName);

                    if (thesql.Contains("a.") || thesql.Contains("relname."))
                    {
                        return thesql.Replace("a.", this.RelName + ".").Replace("relname.", this.RelName + ".");
                    }
                    else
                    {
                        return thesql;
                    }

                }

            }

        }

        public string getFinalSqlSyntax_GROUPBY_ORDERBY()
        {
            string bitops = null;
            if (this.FieldType == "bool") bitops = "+0";    //bit sloupec je třeba sčítat s nulou, protože sqlserver nepodporuje MIN/MAX pro bit sloupce!!

            if (this.SqlSyntax == null)
            {
                if (this.RelName == null)
                {
                    return $"MIN(a.{this.Field}{bitops})";
                    

                }
                else
                {
                    return $"MIN({this.RelName}.{this.Field}{bitops})";                    

                }
            }
            else
            {
                if (this.RelName == null)
                {
                    return $"MIN({this.SqlSyntax}{bitops})";                    

                }
                else
                {
                    this.SqlSyntax = this.SqlSyntax.Replace("_relname_", this.RelName);

                    if (this.SqlSyntax.Contains("a.") || this.SqlSyntax.Contains("relname."))
                    {
                        return $"MIN({this.SqlSyntax.Replace("a.", this.RelName + ".").Replace("relname.", this.RelName + ".")}{bitops})";
                        //return "MIN(" + this.SqlSyntax.Replace("a.", this.RelName + ".").Replace("relname.", this.RelName + ".") + ")";
                    }
                    else
                    {
                        return $"MIN({this.SqlSyntax}{bitops})";
                       
                    }

                }

            }

        }


    }
}
