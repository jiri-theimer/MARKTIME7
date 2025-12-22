

namespace BL
{
    public class p28ColumnsProvider : ColumnsProviderBase
    {
        public p28ColumnsProvider()
        {
            this.EntityName = "p28Contact";

            this.CurrentFieldGroup = "Root";
            oc = AF("p28Name", "Název", null, "string"); oc.NotShowRelInHeader = false; oc.DefaultColumnFlag = gdc1;oc.FixedWidth = 200; oc.SqlExplicitGroupBy = "a.p28ID";
            //oc = AF("TypKontaktu", "Typ kontaktu", "p29.p29Name"); oc.RelSqlInCol = "INNER JOIN p29ContactType p29x ON a.p29ID=p29x.p29ID"; oc.NotShowRelInHeader = true; oc.DefaultColumnFlag = gdc2;

            oc = AF("p28Code", "Kód"); oc.FixedWidth = 100;
            //AF("ClientPid", "PID", "a.p28ID", "int");

            oc=AF("p28ShortName", "Zkrácený název"); oc.SqlExplicitGroupBy = "a.p28ID";

            oc =AF("p28CompanyName", "Společnost");oc.FixedWidth = 200; oc.SqlExplicitGroupBy = "a.p28ID";
            oc = AFBOOL("JeFyzickaOsoba", "FO");oc.SqlSyntax = "convert(bit,case when a.p28IsCompany=1 then 0 else 1 end)";oc.NotShowRelInHeader = true;oc.Tooltip = "Fyzická osoba"; oc.SqlExplicitGroupBy = "a.p28IsCompany";
            oc = AFBOOL("JePravnickaOsoba", "PO"); oc.SqlSyntax = "a.p28IsCompany"; oc.NotShowRelInHeader = true;oc.Tooltip = "Právnická osoba";oc.SqlExplicitGroupBy = "a.p28IsCompany";
            AF("p28LastName", "Příjmení");
            AF("p28FirstName", "Jméno");
            AF("p28TitleBeforeName", "Titul Před");
            AF("p28TitleAfterName", "Titul za");

            oc = AF("TypKontaktu", "Typ kontaktu", "p28_p29y.p29Name"); oc.RelSqlInCol = "LEFT OUTER JOIN p29ContactType p28_p29y ON a.p29ID=p28_p29y.p29ID"; oc.SqlExplicitGroupBy = "a.p29ID";


            oc = AF("TagsHtml", "Štítky", "p28_o54x.o54InlineHtml"); oc.RelSqlInCol = "LEFT OUTER JOIN o54TagBindingInline p28_o54x ON a.p28ID=p28_o54x.o54RecordPid AND p28_o54x.o54RecordEntity='p28'"; oc.IsNotUseP31TOTALS = true; oc.SqlExplicitGroupBy = "p28_o54x.o54InlineText";
            //oc = AF("TagsText", "Štítky (text)", "p28_o54x.o54InlineText"); oc.RelSqlInCol = "LEFT OUTER JOIN o54TagBindingInline p28_o54x ON a.p28ID=p28_o54x.o54RecordPid AND p28_o54x.o54RecordEntity='p28'";
            oc = AF("VlastnikKontaktu", "Vlastník záznamu", "p28_j02owner.j02Name"); oc.RelSqlInCol = "LEFT OUTER JOIN j02User p28_j02owner ON a.j02ID_Owner=p28_j02owner.j02ID";
            oc = AF("RowColor", "Barva", "convert(char(1),a.p28RowColorFlag)");
            oc.FixedWidth = 50;

            oc = AF("WorkflowStav", "Workflow stav", "p28_b02y.b02Name"); oc.RelSqlInCol = "LEFT OUTER JOIN b02WorkflowStatus p28_b02y ON a.b02ID=p28_b02y.b02ID";oc.SqlExplicitGroupBy = "a.b02ID";


            oc = AF("PocetProjektu", "Počet otevřených projektů", "op.PocetOtevrenychProjektu", "num0", true); oc.RelSqlInCol = "LEFT OUTER JOIN view_p28_projects op ON a.p28ID=op.p28ID";
            oc=AF("KontaktniOsoby", "Kontaktní osoby", "dbo.p28_persons_inline(a.p28ID)"); oc.IsNotUseP31TOTALS = true;
            oc = AF("KontaktniMaster", "Zařazení kontaktní osoby", "dbo.p28_person_first_master(a.p28ID)"); oc.IsNotUseP31TOTALS = true;
            oc = AF("ZarazeniDoSkupin", "Zařazení do skupin", "dbo.p28_groups_inline(a.p28ID)");

            oc=AF("p28CountryCode", "ISO kód státu", "a.p28CountryCode");oc.SqlExplicitGroupBy = "a.p28CountryCode";
            oc =AF("p28BankAccount", "Bankovní účet"); 
            AF("p28BankCode", "Kód banky");
            AF("p28JobTitle", "Pozice na vizitce");
            AF("p28Salutation", "Oslovení");
            oc=AF("p28ExternalCode", "Externí kód"); 


            this.CurrentFieldGroup = "Fakturační nastavení";
            oc = AF("p28RegID", "IČ");oc.DefaultColumnFlag = gdc2; 
            oc = AF("p28VatID", "DIČ");oc.DefaultColumnFlag = gdc1; 
            oc=AF("p28ICDPH_SK", "IČ DPH (SK)"); 
            oc = AF("PrirazenyCenik", "Přiřazený fakturační ceník", "clientp51billing.p51Name"); oc.RelSqlInCol = "LEFT OUTER JOIN p51PriceList clientp51billing ON a.p51ID_Billing=clientp51billing.p51ID"; oc.SqlExplicitGroupBy = "a.p51ID_Billing";
            oc=AF("FakturacniJazyk", "Fakturační jazyk", "case when a.p28BillingLangIndex>0 then '#'+convert(nvarchar(20),a.p28BillingLangIndex) end");oc.SqlExplicitGroupBy = "a.p28BillingLangIndex";
            oc=AF("p28VatCodePohoda", "Členění DPH");oc.SqlExplicitGroupBy = "a.p28VatCodePohoda";

            oc =AF("p28Round2Minutes", "Zaokr.hodin na míru", "case when a.p28Round2Minutes=0 then NULL else a.p28Round2Minutes end", "num0");oc.SqlExplicitGroupBy = "a.p28Round2Minutes";
            oc=AF("p28InvoiceMaturityDays", "Splatnost faktury", "case when a.p28InvoiceMaturityDays=0 then NULL else a.p28InvoiceMaturityDays end", "num0");oc.SqlExplicitGroupBy = "a.p28InvoiceMaturityDays";
            oc = AF("RezijniPrirazka", "Režijní přirážka", "p28_p63x.p63Name"); oc.RelSqlInCol = "LEFT OUTER JOIN p63Overhead p28_p63x ON a.p63ID=p28_p63x.p63ID";oc.SqlExplicitGroupBy = "a.p63ID";

            //oc = AF("p28BillingMemo", "Fakturační poznámka","p28_fapo.Text200"); oc.RelSqlInCol = "LEFT OUTER JOIN view_p28_fakturacni_poznamka p28_fapo ON a.p28ID=p28_fapo.p28ID"; oc.FixedWidth = 300;
            oc = AF("p28BillingMemo", "Fakturační poznámka", "a.p28BillingMemo200"); oc.FixedWidth = 300;


            this.CurrentFieldGroup = "Adresa #1 (fakturační)";
            oc = AF("Adresa1", "Adresa #1", "isnull(a.p28City1+', ','')+isnull('<code>'+a.p28Street1+'</code>','')+isnull(', '+a.p28PostCode1,'')+isnull(' <var>'+a.p28Country1+'</var>','')", "string"); oc.DefaultColumnFlag = gdc2;oc.FixedWidth = 300;oc.IsNotUseP31TOTALS = true; //oc.NotShowRelInHeader = true;
            oc=AF("p28Street1", "Ulice #1");oc.IsNotUseP31TOTALS = true;
            AF("p28City1", "Město #1");
            oc=AF("p28PostCode1", "PSČ #1"); oc.SqlExplicitGroupBy = "a.p28PostCode1";
            oc=AF("p28Country1", "Stát #1");oc.SqlExplicitGroupBy = "a.p28Country1";
            oc = AF("p28BeforeAddress1", "Doplnění adresy"); oc.SqlExplicitGroupBy = "a.p28BeforeAddress1";
            

            this.CurrentFieldGroup = "Adresa #2";
            oc = AF("Adresa2", "Adresa #2", "isnull(a.p28City2+', ','')+isnull('<code>'+a.p28Street2+'</code>','')+isnull(', '+a.p28PostCode2,'')+isnull(' <var>'+a.p28Country2+'</var>','')", "string"); oc.IsNotUseP31TOTALS = true;
            oc=AF("p28Street2", "Ulice #2");
            AF("p28City2", "Město #2");
            AF("p28PostCode2", "PSČ #2");
            AF("p28Country2", "Stát #2");

            this.CurrentFieldGroup = "Kontaktní média";
            oc=AF("Medias", "Kontaktní média", "dbo.p28_medias_inline(a.p28ID,0)"); oc.IsNotUseP31TOTALS = true;
            oc=AF("InvoiceEmails", "Fakturační e-mail", "dbo.p28_invoice_email_inline(a.p28ID)"); oc.IsNotUseP31TOTALS = true;
            oc=AF("Emails", "E-mail", "dbo.p28_medias_inline(a.p28ID,2)"); oc.IsNotUseP31TOTALS = true;
            oc=AF("Tels", "TEL", "dbo.p28_medias_inline(a.p28ID,1)"); oc.IsNotUseP31TOTALS = true;
            oc=AF("Urls", "URL", "dbo.p28_medias_inline(a.p28ID,3)"); oc.IsNotUseP31TOTALS = true;


            this.CurrentFieldGroup = "Stromová struktura";
            oc = AF("ParentContact", "Nadřízený kontakt", "parent.ParentName"); oc.RelSqlInCol = "LEFT OUTER JOIN (select p28ID as ParentPID,isnull(p28TreePath,p28Name) as ParentName FROM p28Contact) parent ON a.p28ParentID=parent.ParentPID";
            AF("p28TreePath", "Stromový název", "a.p28TreePath");
            AF("p28TreeLevel", "Tree Level", "a.p28TreeLevel", "num0");
            AF("HasChilds", "Má podřízené", "convert(bit,case when a.p28TreeNext>a.p28TreePrev then 1 else 0 end)", "bool");
            AF("ChildContactsInline", "Podřízené kontakty", "dbo.p28_get_childs_inline_print_version6(a.p28ID)");

            AppendTimestamp();

            

        }
    }
}
