
namespace BL
{
    public class p91ColumnsProvider : ColumnsProviderBase
    {
        public p91ColumnsProvider()
        {
            this.EntityName = "p91Invoice";


            this.CurrentFieldGroup = "Root";
            oc = AF("p91Code", "Číslo", null, "string"); oc.DefaultColumnFlag = gdc1; oc.NotShowRelInHeader = true; oc.FixedWidth = 110;
            oc = AF("p91CodeNumeric", "Variabilní symbol", null, "string"); oc.NotShowRelInHeader = true; oc.FixedWidth = 110;
            oc = AF("j27Code", "Měna", "p91_j27x.j27Code"); oc.RelSqlInCol = "LEFT OUTER JOIN j27Currency p91_j27x ON a.j27ID=p91_j27x.j27ID"; oc.DefaultColumnFlag = gdc1; oc.FixedWidth = 60; oc.SqlExplicitGroupBy = "a.j27ID";

            oc = AF("AktualniStav", "Aktuální stav", "b02x.b02Name"); oc.DefaultColumnFlag = gdc1; oc.SqlExplicitGroupBy = "a.b02ID";
            AF("p91Text1", "Text faktury");
            AF("p91Text2", "Technický text");
            oc = AF("TypFaktury", "Typ faktury", "typf.p92Name"); oc.RelSqlInCol = "LEFT OUTER JOIN p92InvoiceType typf ON a.p92ID=typf.p92ID"; oc.SqlExplicitGroupBy = "a.p92ID";
            oc = AF("VystavovatelFaktury", "Vystavovatel faktury", "p93x.p93Name"); oc.RelSqlInCol = "LEFT OUTER JOIN p92InvoiceType p92xx ON a.p92ID=p92xx.p92ID LEFT OUTER JOIN p93InvoiceHeader p93x ON p92xx.p93ID=p93x.p93ID"; oc.SqlExplicitGroupBy = "p92xx.p93ID";
            oc=AF("p91VatCodePohoda", "Členění DPH"); oc.SqlExplicitGroupBy = "a.p91VatCodePohoda";
            AF("ZapojeneOsoby", "Zapojení uživatelé", "dbo.j02_invoiced_persons_inline(a.p91ID)");

            oc = AFBOOL("p91IsDraft", "Draft"); oc.SqlExplicitGroupBy = "a.p91IsDraft";
            oc = AF("TagsHtml", "Štítky", "p91_o54x.o54InlineHtml"); oc.RelSqlInCol = "LEFT OUTER JOIN o54TagBindingInline p91_o54x ON a.p91ID=p91_o54x.o54RecordPid AND p91_o54x.o54RecordEntity='p91'";
            oc = AF("VlastnikVyuctovani", "Vlastník záznamu", "p91_j02owner.j02Name"); oc.RelSqlInCol = "LEFT OUTER JOIN j02User p91_j02owner ON a.j02ID_Owner=p91_j02owner.j02ID";

            oc = AF("RowColor", "Barva", "convert(char(1),a.p91RowColorFlag)");
            oc.FixedWidth = 50;
            AF("ZamekA", "Zámek A", "case when a.p91LockFlag & 2 = 2 then 1 else 0 end", "bool");
            AF("ZamekB", "Zámek B", "case when a.p91LockFlag & 4 = 4 then 1 else 0 end", "bool");
            AF("ZamekC", "Zámek C", "case when a.p91LockFlag & 8 = 8 then 1 else 0 end", "bool");

            oc = AF("WorkflowSablona", "Workflow šablona", "b01x.b01Name"); oc.RelSqlInCol = "LEFT OUTER JOIN b01WorkflowTemplate b01x ON b02x.b01ID=b01x.b01ID";
            oc = AFBOOL("p91PortalFlag", "Portál");oc.SqlSyntax = "CONVERT(bit,a.p91PortalFlag)"; oc.SqlExplicitGroupBy = "a.p91PortalFlag";

            this.CurrentFieldGroup = "Datum";
            oc = AFDATE("p91Date", "Vystaveno"); oc.DefaultColumnFlag = gdc2; oc.SqlExplicitGroupBy = "a.p91Date";
            oc = AFDATE("p91DateSupply", "Datum plnění"); oc.DefaultColumnFlag = gdc1; oc.SqlExplicitGroupBy = "a.p91DateSupply";
            oc = AF("p91DateSupplyYear", "Rok DUZP", "convert(varchar(4),a.p91DateSupply,126)"); oc.SqlExplicitGroupBy = "convert(varchar(4),a.p91DateSupply,126)"; oc.FixedWidth = 80;
            oc = AF("p91DateSupplyMesic", "Měsíc DUZP", "convert(varchar(7),a.p91DateSupply,126)"); oc.SqlExplicitGroupBy = "convert(varchar(7),a.p91DateSupply,126)"; oc.FixedWidth = 80;
            oc = AFDATE("p91DateMaturity", "Splatnost", "a.p91DateMaturity"); oc.DefaultColumnFlag = gdc2; oc.SqlExplicitGroupBy = "a.p91DateMaturity";
            AF("DnuPoSplatnosti", "Dnů do splatnosti", "case When a.p91Amount_Debt=0 Then null Else datediff(day, a.p91DateMaturity, dbo.get_today()) End", "num0");
            AFDATE("p91DateBilled", "Datum úhrady", "a.p91DateBilled");
            AFDATE("p91DateExchange", "Datum měn.kurzu");

            this.CurrentFieldGroup = "Částka";
            oc = AF("p91Amount_WithoutVat", "Bez dph", null, "num", true); oc.DefaultColumnFlag = gdc1;
            AF("BezDphKratKurz", "Bez dph x Kurz", "case When a.j27ID=a.j27ID_Domestic Then p91Amount_WithoutVat Else p91Amount_WithoutVat*p91ExchangeRate End", "num", true);
            AF("p91Amount_Debt", "Dluh", null, "num", true);
            AF("DluhKratKurz", "Dluh x Kurz", "case When a.j27ID=a.j27ID_Domestic Then p91Amount_Debt Else p91Amount_Debt*p91ExchangeRate End", "num", true);
            oc = AF("p91Amount_TotalDue", "Celkem", null, "num", true); oc.DefaultColumnFlag = gdc1;
            AF("CelkemKratKurz", "Celkem x Kurz", "case When a.j27ID = a.j27ID_Domestic Then p91Amount_TotalDue Else p91Amount_TotalDue*p91ExchangeRate End", "num", true);

            AF("p91Amount_Billed", "Uhrazeno", null, "num", true);
            AF("p91Amount_Vat", "Celkem dph", null, "num", true);
            AF("p91Amount_WithVat", "Vč.dph", null, "num", true);
            AF("p91RoundFitAmount", "Haléřové zaokrouhlení", null, "num", true);
            AF("p91Amount_WithoutVat_None", "Základ v nulové DPH", null, "num", true);
            AF("p91Amount_WithoutVat_Standard", "Základ ve standardní sazbě", null, "num", true);
            AF("p91Amount_WithoutVat_Low", "Základ ve snížené sazbě", null, "num", true);
            AF("p91Amount_WithoutVat_Special", "Základ ve speciální sazbě", null, "num", true);
            AF("p91Amount_Vat_Standard", "DPH ve standardní sazbě", null, "num", true);
            AF("p91Amount_Vat_Low", "DPH ve snížené sazbě", null, "num", true);
            AF("p91Amount_Vat_Special", "DPH ve speciální sazbě", null, "num", true);
            AF("p91VatRate_Standard", "DPH sazba standardní", null, "num", true);
            AF("p91VatRate_Low", "DPH sazba snížená", null, "num", true);
            AF("p91VatRate_Special", "DPH sazba speciální", null, "num", true);



            AF("p91ProformaBilledAmount", "Uhrazené zálohy", null, "num");
            AF("p91ExchangeRate", "Měnový kurz", null, "num3");



            this.CurrentFieldGroup = "Klient vyúčtování";
            oc = AF("p91Client", "Název klienta"); oc.DefaultColumnFlag = gdc1; oc.SqlExplicitGroupBy = "a.p91Client";
            oc = AF("p91Client_RegID", "IČ klienta"); oc.FixedWidth = 100;
            oc = AF("p91Client_VatID", "DIČ klienta"); oc.FixedWidth = 100;
            oc = AF("p91Client_ICDPH_SK", "IČ DPH (SK)"); oc.FixedWidth = 100;
            AF("p91ClientAddress1_Street", "Ulice klienta");
            AF("p91ClientAddress1_City", "Město klienta");
            oc = AF("p91ClientAddress1_ZIP", "PSČ klienta"); oc.FixedWidth = 70;
            AF("p91ClientAddress1_Country", "Stát klienta");
            AF("p91ClientAddress1_Before", "Doplnění adresy");
            

            this.CurrentFieldGroup = "Vystavovatel/Dodavatel";
            oc = AF("p91Supplier", "Název vystavovatele"); oc.DefaultColumnFlag = gdc1; oc.SqlExplicitGroupBy = "a.p91Supplier";
            oc = AF("p91Supplier_RegID", "IČ vystavovatele"); oc.FixedWidth = 100;
            oc = AF("p91Supplier_VatID", "DIČ vystavovatele"); oc.FixedWidth = 100;
            oc = AF("p91Supplier_ICDPH_SK", "IČ DPH vystavovatele"); oc.FixedWidth = 100;
            oc = AF("p91Supplier_Street", "Ulice vystavovatele");
            oc = AF("p91Supplier_City", "Město vystavovatele");
            oc = AF("p91Supplier_ZIP", "PSČ vystavovatele"); oc.FixedWidth = 70;

            this.CurrentFieldGroup = "Elektronicky odesláno";
            oc = AF("VomKdyOdeslano", "Čas odeslání", "vom.Kdy_Odeslano", "datetime"); oc.RelSqlInCol = "LEFT OUTER JOIN view_p91_sendbyemail vom On a.p91ID = vom.p91ID";
            oc = AF("VomStav", "Stav odeslání", "vom.AktualniStav"); oc.RelSqlInCol = "LEFT OUTER JOIN view_p91_sendbyemail vom On a.p91ID = vom.p91ID"; oc.SqlExplicitGroupBy = "vom.AktualniStav";
            oc = AF("VomKomu", "Komu odesláno", "vom.Komu"); oc.RelSqlInCol = "LEFT OUTER JOIN view_p91_sendbyemail vom On a.p91ID = vom.p91ID";
            oc = AF("VomDateInsert", "Vloženo do fronty", "vom.Kdy_Zahajeno", "datetime"); oc.RelSqlInCol = "LEFT OUTER JOIN view_p91_sendbyemail vom On a.p91ID = vom.p91ID";

            oc = AF("BindClient", "Klient vyúčtování", "p28client.p28Name"); oc.RelSqlInCol = "LEFT OUTER JOIN p28Contact p28client ON a.p28ID=p28client.p28ID"; oc.SqlExplicitGroupBy = "a.p28ID";
            oc = AF("ProjectClient", "Klient projektu", "projectclient.j02Name"); oc.RelSqlInCol = "LEFT OUTER JOIN p28Contact projectclient ON p41.p28ID_Client=projectclient.p28ID"; oc.SqlExplicitGroupBy = "p41.p28ID_Client";


            AppendTimestamp();


            this.EntityName = "p91_vyuctovano";
            oc = AF("Hodiny", "Vyúčtované hodiny", "a.Hodiny", "num", true); oc.NotShowRelInHeader = true; oc.IsHours = true;
            oc = AF("Hodiny_Odpis", "Hodiny ODP", "a.Hodiny_Odpis", "num", true); oc.NotShowRelInHeader = true; oc.IsHours = true;
            oc = AF("Hodiny_Pausal", "Hodiny PAU", "a.Hodiny_Pausal", "num", true); oc.NotShowRelInHeader = true; oc.IsHours = true;
            oc = AF("Honorar", "Vyúčtovaný honorář", "a.Honorar", "num", true); oc.NotShowRelInHeader = true;
            oc = AF("Honorar_Fakturacni_CZK", "Honorář CZK", "a.Honorar_Fakturacni_CZK", "num", true);
            oc = AF("Honorar_Fakturacni_EUR", "Honorář EUR", "a.Honorar_Fakturacni_EUR", "num", true);
            oc = AF("Vydaje", "Výdaje", "a.Vydaje", "num", true); oc.NotShowRelInHeader = true;
            oc = AF("Vydaje_CZK", "Výdaje CZK", "a.Vydaje_CZK", "num", true);
            oc = AF("Vydaje_EUR", "Výdaje EUR", "a.Vydaje_EUR", "num", true);
            oc = AF("Pausaly", "Pevné odměny", "a.Pausaly", "num", true); oc.NotShowRelInHeader = true;
            oc = AF("Pausaly_CZK", "Pevné odměny CZK", "a.Pausaly_CZK", "num", true);
            oc = AF("Pausaly_EUR", "Pevné odměny EUR", "a.Pausaly_EUR", "num", true);


            this.EntityName = "p91_vykazano";
            AF("PrvniUkon_Kdy", "První úkon kdy", "a.PrvniUkon_Kdy", "date");
            AF("PosledniUkon_Kdy", "Poslední úkon kdy", "a.PosledniUkon_Kdy", "date");
            oc = AF("Hodiny", "Vykázané hodiny", "a.Hodiny", "num"); oc.NotShowRelInHeader = true; oc.IsHours = true;
            oc = AF("Hodiny_Fa", "Vykázané Fa hodiny", "a.Hodiny_Fa", "num", true); oc.NotShowRelInHeader = true; oc.IsHours = true;
            oc = AF("Hodiny_NeFa", "Vykázané Nefa hodiny", "a.Hodiny_NeFa", "num", true); oc.NotShowRelInHeader = true; oc.IsHours = true;
            oc = AF("Honorar_Fakturacni", "Vykázaný honorář", "a.Honorar_Fakturacni", "num", true); oc.NotShowRelInHeader = true;
            oc = AF("Honorar_Fakturacni_CZK", "Honorář CZK", "a.Honorar_Fakturacni_CZK", "num", true);
            oc = AF("Honorar_Fakturacni_EUR", "Honorář EUR", "a.Honorar_Fakturacni_EUR", "num", true);
            oc = AF("Honorar_Nakladovy", "Nákladový honorář", "a.Honorar_Nakladovy", "num", true); oc.VYSL = true;
            oc = AF("Vydaje", "Vykázané výdaje", "a.Vydaje", "num", true); oc.NotShowRelInHeader = true;
            oc = AF("Vydaje_CZK", "Výdaje CZK", "a.Vydaje_CZK", "num", true);
            oc = AF("Vydaje_EUR", "Výdaje EUR", "a.Vydaje_EUR", "num", true);
            oc = AF("Pausaly", "Vykázané pevné odměny", "a.Pausaly", "num", true); oc.NotShowRelInHeader = true;
            oc = AF("Pausaly_CZK", "Pevné odměny CZK", "a.Pausaly_CZK", "num", true);
            oc = AF("Pausaly_EUR", "Pevné odměny EUR", "a.Pausaly_EUR", "num", true);

        }
    }
}
