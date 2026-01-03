

namespace BL
{
    public abstract class ColumnsProviderBase
    {
        private List<BO.TheGridColumn> _lis;
        
        public BO.TheGridColumn oc;
        public string CurrentFieldGroup { get; set; }
        public BO.TheGridDefColFlag gdc1 = BO.TheGridDefColFlag.GridAndCombo;
        public BO.TheGridDefColFlag gdc0 = BO.TheGridDefColFlag._none;
        public BO.TheGridDefColFlag gdc2 = BO.TheGridDefColFlag.GridOnly;
        public string EntityName { get; set; }

        public ColumnsProviderBase()
        {
            _lis = new List<BO.TheGridColumn>();
        }

        public List<BO.TheGridColumn> getColumns()
        {
            return _lis;
        }

        public BO.TheGridColumn AF(string strField, string strHeader, string strSqlSyntax = null, string strFieldType = "string", bool bolIsShowTotals = false)
        {
            
            _lis.Add(new BO.TheGridColumn() { Field = strField,
                Entity = this.EntityName,
                Header = strHeader,
                DefaultColumnFlag = BO.TheGridDefColFlag._none,
                SqlSyntax = strSqlSyntax,
                FieldType = strFieldType,
                IsShowTotals = bolIsShowTotals,
                NotShowRelInHeader = false,
                FixedWidth = SetDefaultColWidth(strFieldType),
                TranslateLang1 = strHeader, TranslateLang2 = strHeader,
                TranslateLang3 = strHeader,
                DesignerGroup=this.CurrentFieldGroup
            });
            
            return _lis[_lis.Count - 1];
        }

        public BO.TheGridColumn AFBOOL(string strField, string strHeader)
        {
            return AF(strField, strHeader, null, "bool");
        }
        public BO.TheGridColumn AFNUM0(string strField, string strHeader)
        {
            return AF(strField, strHeader, null, "num0", false);
        }
       
        public BO.TheGridColumn AFDATE(string strField, string strHeader, string strSqlSyntax = null)
        {
            return AF(strField, strHeader, strSqlSyntax, "date");
           
        }

        public BO.TheGridColumn AFNUM_OCAS(string strField, string strHeader, string strSqlSyntax = null, bool bolIsShowTotals = false,string strTooltip=null)
        {
            BO.TheGridColumn c = AF(strField, strHeader, strSqlSyntax, "num",bolIsShowTotals);
            c.RelSqlInCol = "LEFT OUTER JOIN dbo.view_p31_ocas p31_ocas ON a.p31ID=p31_ocas.p31ID";
            c.Tooltip = strTooltip;
            return c;
        }

        private int SetDefaultColWidth(string strFieldType)
        {
            switch (strFieldType)
            {
                case "date":
                    return 90;
                case "datetime":
                    return 120;
                case "num":                
                case "num4":
                case "num5":
                case "num3":
                    return 100;
                case "num0":
                case "num1":
                    return 75;
                case "int":
                    return 65;
                case "filesize":
                    return 70;
                case "bool":
                    return 75;
                default:
                    return 0;
            }

        }


        private BO.TheGridColumn AF_TIMESTAMP(string strField, string strHeader, string strSqlSyntax, string strFieldType)
        {
            BO.TheGridColumn c = AF(strField, strHeader, strSqlSyntax, strFieldType, false);
            c.IsTimestamp = true;
            return c;
            

        }

        public void AppendTimestamp(bool include_validity = true)
        {
            this.CurrentFieldGroup = "Časové razítko záznamu";
            
            string prefix = BO.Code.Entity.GetPrefixDb(this.EntityName.Substring(0, 3));
            AF_TIMESTAMP($"DateInsert_{this.EntityName}", "Založeno", $"a.{prefix}DateInsert", "datetime");
            
            oc=AF_TIMESTAMP($"UserInsert_{this.EntityName}", "Založil", $"a.{prefix}UserInsert", "string");oc.SqlExplicitGroupBy = $"a.{prefix}UserInsert";
            AF_TIMESTAMP("DateUpdate_" + this.EntityName, "Aktualizace", "a." + prefix + "DateUpdate", "datetime");
            oc=AF_TIMESTAMP($"UserUpdate_{this.EntityName}", "Aktualizoval", $"a.{prefix}UserUpdate", "string");oc.SqlExplicitGroupBy = $"a.{prefix}UserUpdate";
            
            if (include_validity == true)
            {
                AF_TIMESTAMP($"ValidFrom_{this.EntityName}", "Platné od", $"a.{prefix}ValidFrom", "datetime");
                AF_TIMESTAMP($"ValidUntil_{this.EntityName}", "Platné do", $"a.{prefix}ValidUntil", "datetime");

                AF_TIMESTAMP($"IsValid_{this.EntityName}", "Časově platné", string.Format("convert(bit,case when GETDATE() between a.{0}ValidFrom AND a.{0}ValidUntil then 1 else 0 end)", prefix), "bool");
            }

            this.CurrentFieldGroup = null;
        }


        public void AppendProjectColumns(string prefix)
        {
            oc = AF("NazevProjektu", "Název projektu", "a.p41Name");oc.DefaultColumnFlag = gdc1;oc.NotShowRelInHeader = true; oc.SqlExplicitGroupBy = "a.p41Name";
            oc=AF("p41NameShort", "Zkrácený název"); oc.SqlExplicitGroupBy = "a.p41NameShort";
            //oc = AF("NazevNeboZkraceny", "Optimální název", "case when a.p41ParentID IS NOT NULL then a.p41TreePath else ISNULL(a.p41NameShort,a.p41Name) end");oc.NotShowRelInHeader = true;
            oc = AF("NazevNeboZkraceny", "Optimální název", "ISNULL(a.p41NameShort,a.p41TreePath)"); oc.NotShowRelInHeader = true;
            oc = AF("p41Code", "Kód"); oc.FixedWidth = 100; oc.DefaultColumnFlag = gdc1;
            oc = AF("TypProjektu", "Typ","p41_p42y.p42Name");oc.RelSqlInCol = "INNER JOIN p42ProjectType p41_p42y ON a.p42ID=p41_p42y.p42ID";oc.FixedWidth = 120; oc.SqlExplicitGroupBy = "a.p42ID";

            oc = AF("KlientProjektu", "Klient projektu", "p41_p28clienty.p28Name"); oc.RelSqlInCol = "LEFT OUTER JOIN p28Contact p41_p28clienty ON a.p28ID_Client=p41_p28clienty.p28ID";oc.NotShowRelInHeader = true; oc.SqlExplicitGroupBy = "a.p28ID_Client";

            oc = AF("KlientPlusProjekt", "Klient + Projekt", "ISNULL(p41_p28clienty.p28Name+' - ','')+a.p41Name"); oc.RelSqlInCol = "LEFT OUTER JOIN p28Contact p41_p28clienty ON a.p28ID_Client=p41_p28clienty.p28ID"; oc.NotShowRelInHeader = true; oc.SqlExplicitGroupBy = "a.p28ID_Client";

            oc = AF("StrediskoProjektu", "Středisko", "p41_j18y.j18Name"); oc.RelSqlInCol = "LEFT OUTER JOIN j18CostUnit p41_j18y ON a.j18ID=p41_j18y.j18ID"; oc.SqlExplicitGroupBy = "a.j18ID";
            oc = AF("StromUroven", "Strom úroveň", "p41_p07y.p07Name"); oc.RelSqlInCol = "INNER JOIN p07ProjectLevel p41_p07y ON a.p07ID=p41_p07y.p07ID"; oc.SqlExplicitGroupBy = "a.p07ID"; oc.FixedWidth = 120;

            oc = AF("KlastrAktivit", "Klastr aktivit", "p41_p61y.p61Name"); oc.RelSqlInCol = "LEFT OUTER JOIN p61ActivityCluster p41_p61y ON a.p61ID=p41_p61y.p61ID"; oc.SqlExplicitGroupBy = "a.p61ID";
            oc = AF("WorkflowStav", "Workflow stav", "p41_b02y.b02Name"); oc.RelSqlInCol = "LEFT OUTER JOIN b02WorkflowStatus p41_b02y ON a.b02ID=p41_b02y.b02ID"; oc.SqlExplicitGroupBy = "a.b02ID";

            
            oc = AF("TagsHtml", "Štítky", "p41_o54x.o54InlineHtml"); oc.RelSqlInCol = "LEFT OUTER JOIN o54TagBindingInline p41_o54x ON a.p41ID=p41_o54x.o54RecordPid AND p41_o54x.o54RecordEntity='p41'";oc.IsNotUseP31TOTALS = true;oc.SqlExplicitGroupBy = "p41_o54x.o54InlineText";
            //oc = AF("TagsText", "Štítky (pouze text)", "p41_o54x.o54InlineText"); oc.RelSqlInCol = "LEFT OUTER JOIN o54TagBindingInline p41_o54x ON a.p41ID=p41_o54x.o54RecordPid AND p41_o54x.o54RecordEntity='p41'"; oc.IsNotUseP31TOTALS = true; oc.SqlExplicitGroupBy = "p41_o54x.o54InlineText";

            oc = AF("VlastnikProjektu", "Vlastník záznamu", "p41_j02owner.j02Name"); oc.RelSqlInCol = "LEFT OUTER JOIN j02User p41_j02owner ON a.j02ID_Owner=p41_j02owner.j02ID";
            oc = AF("RowColor", "Barva", "convert(char(1),a.p41RowColorFlag)");

            if (prefix == "p41" || prefix == "le5" || prefix=="le4" || prefix=="le3" || prefix=="le2")
            {
                //oc = AF("UrovenProjektu", "Vertikální úroveň", $"{prefix}_p42_p07.p07Name"); oc.RelSqlInCol = $"INNER JOIN p42ProjectType {prefix}_p42 ON a.p42ID={prefix}_p42.p42ID INNER JOIN p07ProjectLevel {prefix}_p42_p07 ON {prefix}_p42.p07ID={prefix}_p42_p07.p07ID";
                oc = AF("Nadrizeny", "Nadřízená úroveň", $"{prefix}parent.p41Name"); oc.RelSqlInCol = $"LEFT OUTER JOIN p41Project {prefix}parent On a.p41ParentID={prefix}parent.p41ID";
            }

            

            oc = AF("WorksheetOperFlag", "Další omezení vykazování", "case a.p41WorksheetOperFlag when 3 then 'Vykazovat přes úkol' when 4 then 'Hodiny přes úkol' end"); oc.SqlExplicitGroupBy = "a.p41WorksheetOperFlag";


            this.CurrentFieldGroup = "Fakturační nastavení";
            
            oc = AF("TypFaktury", "Typ faktury", "p41_p92.p92Name"); oc.RelSqlInCol = "LEFT OUTER JOIN p92InvoiceType p41_p92 ON a.p92ID=p41_p92.p92ID"; oc.SqlExplicitGroupBy = "a.p92ID";
            oc = AF("Odberatel", "Odběratel faktury", $"{prefix}_odberatel.p28Name"); oc.RelSqlInCol = $"LEFT OUTER JOIN p28Contact {prefix}_odberatel On a.p28ID_Billing={prefix}_odberatel.p28ID"; oc.SqlExplicitGroupBy = "a.p28ID_Billing";
            oc = AF("PrirazenyCenik", "Fakturační ceník", $"{prefix}p51billing.p51Name"); oc.RelSqlInCol = $"LEFT OUTER JOIN p51PriceList {prefix}p51billing ON a.p51ID_Billing={prefix}p51billing.p51ID"; oc.SqlExplicitGroupBy = "a.p51ID_Billing";
            //oc = AF("SkutecnyCenik", "Skutečný fakturační ceník", "dbo.get_billing_pricelist_name(a.p41ID,a.p28ID_Client)");
            oc = AF("FakturacniJazyk", "Fakturační jazyk", "case when a.p41BillingLangIndex>0 then '#'+convert(varchar(10),a.p41BillingLangIndex) end"); oc.SqlExplicitGroupBy = "a.p41BillingLangIndex";

            //oc = AF("p41BillingMemo", "Fakturační poznámka", "p41_fapo.Text200"); oc.RelSqlInCol = "LEFT OUTER JOIN view_p41_fakturacni_poznamka p41_fapo ON a.p41ID=p41_fapo.p41ID"; oc.FixedWidth = 300;
            oc = AF("p41BillingMemo", "Fakturační poznámka", "a.p41BillingMemo200"); oc.FixedWidth = 300;
            oc = AF("p41Round2Minutes", "Zaokr.hodin na míru", "case when a.p41Round2Minutes=0 then NULL else a.p41Round2Minutes end", "num0"); oc.SqlExplicitGroupBy = "a.p41Round2Minutes";

            oc = AF("p41AccountingIds", "Předkontace"); oc.SqlExplicitGroupBy = "a.p41AccountingIds"; oc.FixedWidth = 100;



            this.CurrentFieldGroup = "Rozpočet";
            string strP41VykaSQL = "LEFT OUTER JOIN dbo.view_p41_vykony_rozpocet p41vykony ON a.p41ID=p41vykony.p41ID";
           
            
            oc=AF("p41PlanFrom", "Plánované zahájení", "a.p41PlanFrom", "date");oc.SqlExplicitGroupBy = "a.p41PlanFrom";
            AF("p41PlanUntil", "Plánované dokončení", "a.p41PlanUntil", "date"); oc.SqlExplicitGroupBy = "a.p41PlanUntil";
            oc =AF("p41Plan_Hours", "Plán hodin", "a.p41Plan_Hours", "num", true);

            oc = AF("PlanMinusVykazaneHodiny", "Hodiny - Plán hodin", "isnull(p41vykony.Hodiny,0)-a.p41Plan_Hours", "num", true); oc.RelSqlInCol = strP41VykaSQL;            
            oc = AF("VykazaneHodinyDelenoPlan", "Hodiny/Plán hodin %", "case when isnull(a.p41Plan_Hours,0)<>0 then 100*p41vykony.Hodiny/a.p41Plan_Hours end", "num", true); oc.RelSqlInCol = strP41VykaSQL;
            

            oc=AF("p41Plan_Hours_Billable", "Plán Fa hodin", "a.p41Plan_Hours_Billable", "num", true);
            oc=AF("p41Plan_Hours_Nonbillable", "Plán Nefa hodin", "a.p41Plan_Hours_Nonbillable", "num", true);
            oc = AF("p41Plan_Expenses", "Plán výdajů", "a.p41Plan_Expenses", "num", true);
            oc = AF("PlanMinusVykazaneVydaje", "Výdaje - Plán výdajů", "isnull(p41vykony.Vydaje,0)-a.p41Plan_Expenses", "num", true); oc.RelSqlInCol = strP41VykaSQL;oc.VYSL = true;
            oc = AF("VykazaneVydajeDelenoPlan", "Výdaje/Plán výdajů %", "case when isnull(a.p41Plan_Expenses,0)<>0 then 100*p41vykony.Vydaje/a.p41Plan_Expenses end", "num", true); oc.RelSqlInCol = strP41VykaSQL;oc.VYSL = true;

            oc=AF("p41Plan_Revenue", "Plán fakturace", "a.p41Plan_Revenue", "num", true);oc.IHRC = true;
            oc = AF("p41Plan_Internal_Fee", "Plán nákl.honoráře", "a.p41Plan_Internal_Fee", "num", true);oc.VYSL = true;
            oc = AF("PlanCenaMinusNakladovyHonorar", "Nákl.hon. - Plán nákl.hon.", "isnull(p41vykony.Honorar_Naklad,0)-a.p41Plan_Internal_Fee", "num", true); oc.RelSqlInCol = strP41VykaSQL;oc.VYSL = true;
        }
    }
}
