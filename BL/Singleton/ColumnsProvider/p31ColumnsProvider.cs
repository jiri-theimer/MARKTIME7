

namespace BL
{
    public class p31ColumnsProvider:ColumnsProviderBase
    {        
        public p31ColumnsProvider()
        {
            this.EntityName = "p31Worksheet";

            this.CurrentFieldGroup = "Root";//-----------Root---------------------
            oc =AF("p31Text", "Text");oc.DefaultColumnFlag = gdc1;oc.FixedWidth = 500;oc.IsNotUseP31TOTALS = true;
            oc=AF("OdradkovanyText", "Odřádkovaný text", "REPLACE(a.p31Text,CHAR(10),'<br>')"); oc.IsNotUseP31TOTALS = true;
            //oc =AF("EditovatelnyText", "Upravit text úkonu", "CASE WHEN a.p71ID IS NULL THEN CASE WHEN CHARINDEX(CHAR(10),a.p31Text)>0 OR LEN(a.p31Text)>170 THEN '<textarea onchange=''_p31text_edi(this,'+convert(varchar(10),a.p31ID)+')'' style=''width:349px;height:100px;overflow:auto;''>'+ISNULL(a.p31Text,'')+'</textarea>' ELSE '<textarea onchange=''_p31text_edi(this,'+convert(varchar(10),a.p31ID)+')'' style=''width:349px;overflow:auto;''>'+ISNULL(a.p31Text,'')+'</textarea>' END ELSE a.p31Text END");
            oc.FixedWidth = 350;oc.IsSortable = false; oc.IsNotUseP31TOTALS = true;

            oc = AF("p31TextInternal", "Interní text"); oc.FixedWidth = 250; oc.IsNotUseP31TOTALS = true;

            AF("p31Code", "Kód dokladu");
            oc = AF("j27Code_Billing_Orig", "Měna", "j27billing_orig.j27Code", "string"); oc.IHRC = true; oc.RelSqlInCol = "LEFT OUTER JOIN j27Currency j27billing_orig ON a.j27ID_Billing_Orig=j27billing_orig.j27ID";oc.FixedWidth = 50;oc.SqlExplicitGroupBy = "a.j27ID_Billing_Orig";

            oc = AF("KontaktniOsoba", "Kontaktní osoba", "p28cp.p28Name", "string"); oc.IHRC = true; oc.RelSqlInCol = "LEFT OUTER JOIN p28Contact p28cp ON a.p28ID_ContactPerson=p28cp.p28ID"; oc.SqlExplicitGroupBy = "a.p28ID_ContactPerson";


            //oc = AF("Dokument", "Vazba na dokument", "p31_o23x.o23Name"); oc.RelSqlInCol = "LEFT OUTER JOIN o23Doc p31_o23x ON a.o23ID=p31_o23x.o23ID";
            oc = AF("TagsHtml", "Štítky", "p31_o54x.o54InlineHtml"); oc.RelSqlInCol = "LEFT OUTER JOIN o54TagBindingInline p31_o54x ON a.p31ID=p31_o54x.o54RecordPid AND p31_o54x.o54RecordEntity='p31'";
            //oc = AF("TagsText", "Štítky (text)", "p31_o54x.o54InlineText"); oc.RelSqlInCol = "LEFT OUTER JOIN o54TagBindingInline p31_o54x ON a.p28ID=p31_o54x.o54RecordPid AND p31_o54x.o54RecordEntity='p31'";
            oc = AF("RowColor", "Barva", "convert(char(1),a.p31RowColorFlag)");
            oc.FixedWidth = 50;

            AF("p31ExternalCode", "Externí kód");
            oc = AF("DodavatelUkonu", "Dodavatel", "p28dodavatel.p28Name", "string"); oc.IHRC = true; oc.RelSqlInCol = "LEFT OUTER JOIN p28Contact p28dodavatel ON a.p28ID_Supplier=p28dodavatel.p28ID"; oc.SqlExplicitGroupBy = "a.p28ID_Supplier";


            //AF("p31RecordSourceFlag_Alias", "Zdrojová aplikace", "case a.p31RecordSourceFlag when 1 then 'Mobil' else 'MT' end");
            AF("p31TimerTimestamp", "Čas zapnutí stopek", null, "datetime");
            oc = AF("FP", "FP", "CASE WHEN a.p49ID IS NOT NULL THEN 1 ELSE 0 END", "bool");oc.Tooltip = "Vazba na finančí plán";

            
            this.CurrentFieldGroup = "Datum a čas úkonu";//-----------Datum a čas úkonu---------------------
            oc =AFDATE("p31Date", "Datum","a.p31Date");oc.DefaultColumnFlag = gdc1;oc.SqlExplicitGroupBy = "a.p31Date";
            
            oc=AF("UkonYear", "Rok", "convert(varchar(4),a.p31Date,126)");oc.SqlExplicitGroupBy = "convert(varchar(4),a.p31Date,126)";oc.FixedWidth = 80;
            oc=AF("UkonMesic", "Měsíc", "convert(varchar(7),a.p31Date,126)");oc.SqlExplicitGroupBy = "convert(varchar(7),a.p31Date,126)";oc.FixedWidth = 80;
            AF("UkonTyden", "Týden", "convert(varchar(4),year(a.p31Date))+'-'+convert(varchar(10),DATEPART(week,a.p31Date))");
            oc=AF("p31DateTimeFrom_Orig", "Čas od", null, "time");oc.FixedWidth = 50;
            oc=AF("p31DateTimeUntil_Orig", "Čas do", null, "time"); oc.FixedWidth = 50;



            this.CurrentFieldGroup = "Vykázáno";//-----------Vykázáno---------------------
            
            oc=AF("p31Hours_Orig", "Hodiny", null, "num", true); oc.DefaultColumnFlag = gdc1;oc.IsHours = true;
            oc = AF("p31HHMM_Orig", "HH:MM"); oc.FixedWidth = 50;
            oc=AFNUM_OCAS("Vykazano_Hodiny_Fa", "Hodiny Fa", "p31_ocas.Vykazano_Hodiny_Fa", true); oc.IsHours = true;
            oc=AFNUM_OCAS("Vykazano_Hodiny_NeFa", "Hodiny NeFa", "p31_ocas.Vykazano_Hodiny_NeFa", true); oc.IsHours = true;
            oc=AFNUM_OCAS("Vykazano_Hodiny_Vyloucene", "Hodiny vyloučené z vyúčt.", "p31_ocas.Vykazano_Hodiny_Vyloucene", true); oc.IsHours = true;



            AF("Kusovnik", "Kusovník", "case when p34x.p33ID=3 then a.p31Value_Orig end", "num", true);
            AF("p31Value_Orig", "Hodnota", null, "num");

            oc = AF("p31Rate_Billing_Orig", "Fakturační sazba", "a.p31Rate_Billing_Orig", "num");oc.IHRC = true;oc.DefaultColumnFlag = gdc1;oc.SqlExplicitGroupBy = "a.p31Rate_Billing_Orig";
            oc = AF("p51Name_BillingRate", "Fakturační ceník", "p51billingrate.p51Name", "string"); oc.IHRC = true; oc.RelSqlInCol = "LEFT OUTER JOIN p51PriceList p51billingrate ON a.p51ID_BillingRate=p51billingrate.p51ID";oc.SqlExplicitGroupBy = "a.p51ID_BillingRate";
            oc = AF("p31Amount_WithoutVat_Orig", "Bez DPH", null, "num", true); oc.IHRC = true;oc.DefaultColumnFlag = gdc1;
            oc = AF("p31Amount_WithVat_Orig", "Vč. DPH", null, "num", true); oc.IHRC = true;
            oc = AF("p31Amount_Vat_Orig", "Částka DPH", null, "num", true); oc.IHRC = true;

            oc = AF("Vykazano_Odmena", "Pevná odměna", "case when p34x.p33ID IN (2,5) and p34x.p34IncomeStatementFlag=2 then a.p31Amount_WithoutVat_Orig end", "num", true); oc.IHRC = true;
            oc = AF("Vykazano_Vydaj", "Výdaj", "case when p34x.p33ID IN (2,5) and p34x.p34IncomeStatementFlag=1 then a.p31Amount_WithoutVat_Orig end", "num", true); oc.IHRC = true;
            
            oc = AF("p54", "Hladina sazby", "p54.p54Name"); oc.RelSqlInCol = "LEFT OUTER JOIN p54OvertimeLevel p54 On a.p54ID=p54.p54ID"; oc.IHRC = true;oc.SqlExplicitGroupBy = "a.p54ID";

            oc = AF("p40", "Vazba na paušál", "p40.p40Name"); oc.RelSqlInCol = "LEFT OUTER JOIN p40WorkSheet_Recurrence p40 On a.p40ID_FixPrice=p40.p40ID"; oc.IHRC = true;oc.SqlExplicitGroupBy = "a.p40ID_FixPrice";

            oc = AF("trimm_p72Name", "Status korekce", "p72trimm.p72Name");oc.RelSqlInCol = "LEFT OUTER JOIN p72PreBillingStatus p72trimm On a.p72ID_AfterTrimming=p72trimm.p72ID";oc.SqlExplicitGroupBy = "a.p72ID_AfterTrimming";

            oc = AFNUM_OCAS("VykazanoHodinyFaPoKorekci", "Hodiny Fa po korekci", "case when a.p72ID_AfterTrimming is null then p31_ocas.Vykazano_Hodiny_Fa else a.p31Hours_Trimmed end", true); oc.IsHours = true;
            oc = AF("Fakturacni_Honorar_Po_Korekci", "Fakturační honorář po korekci", "case when a.p72ID_AfterTrimming is not null then a.p31Hours_Trimmed*a.p31Rate_Billing_Orig else a.p31Hours_Orig*a.p31Rate_Billing_Orig end", "num", true); oc.IHRC = true;
            oc = AF("p31Amount_WithoutVat_AfterTrimming", "Bez DPH po korekci", "a.p31Amount_WithoutVat_AfterTrimming","num", true); oc.IHRC = true;
            oc = AF("p31Value_Off", "Off billing hodnota", null, "num", true); oc.IHRC = true;

            this.CurrentFieldGroup = "Rozpracováno";//-----------Rozpracováno---------------------
            oc = AFNUM_OCAS("WIP_Hodiny", "Rozpr.hodiny", "p31_ocas.WIP_Hodiny",  true);
            oc = AFNUM_OCAS("WIP_Vydaje", "Rozpr.výdaj", "p31_ocas.WIP_Vydaje", true);
            oc = AFNUM_OCAS("WIP_BezDph", "Rozpr.bez DPH", "p31_ocas.WIP_BezDph", true); oc.IHRC = true;
            oc = AFNUM_OCAS("WIP_BezDph_EUR", "Rozpr.bez DPH EUR", "p31_ocas.WIP_BezDph_EUR", true); oc.IHRC = true;
            oc = AFNUM_OCAS("WIP_Honorar", "Rozpr.Honorář", "p31_ocas.WIP_Honorar", true); oc.IHRC = true;
            oc = AFNUM_OCAS("WIP_Vydaje_EUR", "Rozpr.výdaje EUR", "p31_ocas.WIP_Vydaje_EUR", true); oc.IHRC = true;
            oc = AFNUM_OCAS("WIP_Pausaly", "Rozpr.pevná odměna", "p31_ocas.WIP_Pausaly", true); oc.IHRC = true;
            oc = AFNUM_OCAS("WIP_Pausaly_EUR", "Rozpr.pevná odměna EUR", "p31_ocas.WIP_Pausaly_EUR", true); oc.IHRC = true;

            this.CurrentFieldGroup = "Nevyúčtováno";//-----------Nevyúčtováno---------------------
            oc = AFNUM_OCAS("Nevyfakturovano_BezDph", "Nevyúčtováno bez DPH", "p31_ocas.Nevyfakturovano_BezDph", true); oc.IHRC = true;
            oc = AFNUM_OCAS("Nevyfakturovano_Hodiny", "Nevyúčtováné hodiny", "p31_ocas.Nevyfakturovano_Hodiny", true); oc.IHRC = true;
            oc = AFNUM_OCAS("Nevyfakturovano_Vydaje", "Nevyúčtováný výdaj", "p31_ocas.Nevyfakturovano_Vydaje", true); oc.IHRC = true;
            oc = AFNUM_OCAS("Nevyfakturovano_Pausaly", "Nevyúčtováná pevná odměna", "p31_ocas.Nevyfakturovano_Pausaly", true); oc.IHRC = true;
            oc = AFNUM_OCAS("Nevyfakturovano_Schvalene_Hodiny", "Schválené hodiny - čeká na vyúčtování", "p31_ocas.Nevyfakturovano_Schvalene_Hodiny",  true); oc.IHRC = true;
            oc = AFNUM_OCAS("Nevyfakturovano_Schvalene_Hodiny_Pausal", "Schválené hodiny PAU - čeká na vyúčtování", "p31_ocas.Nevyfakturovano_Schvalene_Hodiny_Pausal",  true); oc.IHRC = true;
            oc = AFNUM_OCAS("Nevyfakturovano_Schvalene_Hodiny_Odpis", "Schválené hodiny ODPIS - čeká na vyúčtování", "p31_ocas.Nevyfakturovano_Schvalene_Hodiny_Odpis", true); oc.IHRC = true;
            oc = AFNUM_OCAS("Nevyfakturovano_Schvaleno_BezDph", "Schváleno bez DPH - čeká na vyúčtování", "p31_ocas.Nevyfakturovano_Schvaleno_BezDph", true); oc.IHRC = true;


            this.CurrentFieldGroup = "Vyúčtováno";//-----------Vyúčtováno---------------------
            oc = AF("StatusVyuctovani", "Status V", "p70.p70Name", "string"); oc.IHRC = true; oc.RelSqlInCol = "LEFT OUTER JOIN p70BillingStatus p70 ON a.p70ID=p70.p70ID";oc.SqlExplicitGroupBy = "a.p70ID";
            oc = AF("p31Hours_Invoiced", "Vyúčtované hodiny", null, "num", true); oc.IsHours = true;
            oc = AF( "p31HHMM_Invoiced", "Vyúčtováno HH:mm", null, "num");
            oc = AF( "p31Rate_Billing_Invoiced", "Vyúčtovaná hodinová sazba", null, "num"); oc.IHRC = true;
            oc = AF("p31Rate_Billing_Invoiced_Krat_Kurz", "Vyúčt.sazba x kurz", "a.p31Rate_Billing_Invoiced*a.p31ExchangeRate_Invoice", "num"); oc.IHRC = true;
            oc = AF( "p70Name", "Fakturační status", "p70.p70Name"); oc.RelSqlInCol = "LEFT OUTER JOIN p70BillingStatus p70 On a.p70ID=p70.p70ID"; oc.IHRC = true;oc.SqlExplicitGroupBy = "a.p70ID";
            oc = AF( "p70Name_BillingLang1", "Fakturační status L1", "p70.p70Name_BillingLang1"); oc.RelSqlInCol = "LEFT OUTER JOIN p70BillingStatus p70 On a.p70ID=p70.p70ID"; oc.IHRC = true;
            oc = AF( "p70Name_BillingLang2", "Fakturační status L2", "p70.p70Name_BillingLang2"); oc.RelSqlInCol = "LEFT OUTER JOIN p70BillingStatus p70 On a.p70ID=p70.p70ID"; oc.IHRC = true;
            oc = AF( "p31Amount_WithoutVat_Invoiced", "Vyúčtováno bez DPH", null, "num", true); oc.IHRC = true;
            oc = AF( "p31Amount_WithVat_Invoiced", "Vyúčtováno vč. DPH", null, "num", true); oc.IHRC = true;
            oc = AF( "p31VatRate_Invoiced", "Vyúčtovaná DPH sazba", null, "num"); oc.IHRC = true;
            oc = AF( "p31Amount_WithoutVat_Invoiced_Domestic", "Vyúčtováno bez DPH x Kurz", null, "num", true); oc.IHRC = true;
            oc = AF( "j27Code_Billing_Invoice", "Měna vyúčtování", "j27billing_invoice.j27Code", "string"); oc.IHRC = true; oc.RelSqlInCol = "LEFT OUTER JOIN j27Currency j27billing_invoice ON a.j27ID_Billing_Invoiced=j27billing_invoice.j27ID";oc.FixedWidth = 70;oc.SqlExplicitGroupBy = "a.j27ID_Billing_Invoiced";
            oc = AF("p31ExchangeRate_Invoice", "Měnový kurz", "a.p31ExchangeRate_Invoice", "num3");
            oc = AFNUM_OCAS( "Vyfakturovano_Hodiny_Fakturovat", "Vyúčt.hodiny [Fakturovat]", "p31_ocas.Vyfakturovano_Hodiny_Fakturovat",  true); oc.IHRC = true; oc.IsHours = true;
            oc = AFNUM_OCAS( "Vyfakturovano_Hodiny_Pausal", "Odepsané hodiny PAU", "p31_ocas.Vyfakturovano_Hodiny_Pausal", true); oc.IHRC = true; oc.IsHours = true;
            oc = AFNUM_OCAS( "Vyfakturovano_Hodiny_Odpis", "Odepsané hodiny ODP", "p31_ocas.Vyfakturovano_Hodiny_Odpis", true); oc.IHRC = true; oc.IsHours = true;
            oc = AFNUM_OCAS("Vyfakturovano_OdpisCelkem", "Celkový odpis hodin", "isnull(p31_ocas.Vyfakturovano_Hodiny_Odpis,0)+isnull(p31_ocas.Vyfakturovano_Hodiny_Pausal,0)", true); oc.IHRC = true; oc.IsHours = true;
            oc = AFNUM_OCAS( "Vyfakturovano_Honorar", "Vyúčt.honorář", "p31_ocas.Vyfakturovano_Honorar", true); oc.IHRC = true;
            oc = AFNUM_OCAS("Vyfakturovano_Honorar_Domestic", "Vyúčt.honorář x kurz", "p31_ocas.Vyfakturovano_Honorar_Domestic", true); oc.IHRC = true;
            oc = AFNUM_OCAS("Vyfakturovano_Vydaje", "Vyúčt.výdaj", "p31_ocas.Vyfakturovano_Vydaje", true); oc.IHRC = true;
            oc = AFNUM_OCAS("Vyfakturovano_Vydaj_Domestic", "Vyúčt.výdaj x kurz", "p31_ocas.Vyfakturovano_Vydaj_Domestic", true); oc.IHRC = true;
            oc = AFNUM_OCAS("Vyfakturovano_Pausaly", "Vyúčt.pevná odměna", "p31_ocas.Vyfakturovano_Pausaly", true); oc.IHRC = true;
            oc = AFNUM_OCAS("Vyfakturovano_Pausal_Domestic", "Vyúčt.pevná odměna x kurz", "p31_ocas.Vyfakturovano_Pausal_Domestic", true); oc.IHRC = true;
            oc = AFNUM_OCAS("Vyuctovano_Kusy", "Vyúčt.kusovník", "case when p34x.p33ID=3 AND a.p91ID IS NOT NULL then a.p31Value_Invoiced end", false); oc.IHRC = true;
            

            this.CurrentFieldGroup = "Schvalování";//-----------Schvalování---------------------
            oc = AF("StatusSchvalovani", "Status S", "p72.p72Name", "string"); oc.IHRC = true; oc.RelSqlInCol = "LEFT OUTER JOIN p72PreBillingStatus p72 ON a.p72ID_AfterApprove=p72.p72ID"; oc.SqlExplicitGroupBy = "a.p72ID_AfterApprove";
            oc = AF("p31Hours_Approved_Billing", "Schválené hodiny", "case when p34x.p33ID=1 and a.p72ID_AfterApprove=4 then a.p31Hours_Approved_Billing end", "num", true); oc.IsHours = true;
            oc = AF("p31Rate_Billing_Approved", "Schválená sazba", "case when p34x.p33ID IN (1,3) and a.p72ID_AfterApprove=4 then a.p31Rate_Billing_Approved end", "num",false);oc.SqlExplicitGroupBy = "a.p31Rate_Billing_Approved";
            oc = AF("p31Amount_WithoutVat_Approved", "Schváleno bez DPH", "case when a.p72ID_AfterApprove=4 then a.p31Amount_WithoutVat_Approved end", "num", true);
            oc = AF("p31Hours_Approved_Internal", "Interně schválené hodiny", null, "num", true); oc.IsHours = true;
            oc = AF("ISH_NaklSazba", "ISH x Nákl.sazba", "a.p31Hours_Approved_Internal*a.p31Rate_Internal_Approved", "num", true); oc.VYSL = true;
            oc = AF("ISH_FaktSazba", "ISH x Fakt.sazba", "a.p31Hours_Approved_Internal*a.p31Rate_Billing_Approved", "num", true);
            oc = AF("p31Value_Approved_Billing", "Schválené kusy", "case when p34x.p33ID=3 and a.p72ID_AfterApprove=4 then a.p31Value_Approved_Billing end", "num", true);
            oc = AFNUM0("p31ApprovingLevel", "Úroveň schvalování");
            oc = AF("p31Approved_When", "Schváleno kdy", null, "datetime");
            oc = AF("Schvalovatel", "Schvalovatel", "schvalovatel.j02Name", "string"); oc.RelSqlInCol = "LEFT OUTER JOIN j02User schvalovatel ON a.j02ID_ApprovedBy=schvalovatel.j02ID"; oc.SqlExplicitGroupBy = "a.j02ID_ApprovedBy";
            oc = AF("p31VatRate_Approved", "Sazba DPH po schvalování", null, "num", false);
            oc = AF("SchvaleneHodinyVPausalu", "Hodiny v paušálu", "case when p34x.p33ID=1 and a.p72ID_AfterApprove=6 then case when isnull(a.p31Value_FixPrice,0)<>0 then a.p31Value_FixPrice else p31Hours_Orig end end", "num", true);
            oc = AF("KorekceHodinPausal", "Korekce hodin v paušálu", "case when p34x.p33ID=1 then a.p31Value_FixPrice end", "num", true);
            oc = AF("KorekceSchvalHodin", "Rozdíl hodin", "case when a.p71ID=1 AND p34x.p33ID=1 and a.p31Hours_Approved_Billing<>p31Hours_Orig then a.p31Hours_Approved_Billing-p31Hours_Orig end", "num",true);
            oc = AF("KorekceSchvalBezDph", "Rozdíl Bez dph", "case when a.p71ID=1 AND a.p31Amount_WithoutVat_Approved<>a.p31Amount_WithoutVat_Orig then a.p31Amount_WithoutVat_Approved-a.p31Amount_WithoutVat_Orig end", "num", true);

            //oc = AF("SchvalovaniText", "Upravit text úkonu", "CASE WHEN CHARINDEX(CHAR(10),a.p31Text)>0 OR LEN(a.p31Text)>170 THEN '<textarea onchange=''p31text_temp(this,'+convert(varchar(10),a.p31ID)+')'' style=''width:349px;height:100px;overflow:auto;''>'+ISNULL(a.p31Text,'')+'</textarea>' ELSE '<textarea onchange=''p31text_temp(this,'+convert(varchar(10),a.p31ID)+')'' style=''width:349px;overflow:auto;''>'+ISNULL(a.p31Text,'')+'</textarea>' END");
            //oc = AF("SchvalovaniText", "Upravit text úkonu", "'<textarea onchange=''p31text_temp(this,'+convert(varchar(10),a.p31ID)+')'' rows=''3'' style=''width:499px;overflow:auto;''>'+ISNULL(a.p31Text,'')+'</textarea>'");

            //oc = AF("Schvalit", "Schválit", "'<div class=''rowvalhover''>xx<a class=''reczoom'' data-rel=''/p28/Info?pid=17&hover_by_reczoom=1'' data-width=''1400px'' data-height=''70'' >Schválit...</a></div>'");
            oc.IsSortable = false;

            this.CurrentFieldGroup = "Nákladová cena";//-----------Nákladová cena---------------------
            oc = AF("p31Rate_Internal_Orig", "Nákladová sazba", "a.p31Rate_Internal_Orig", "num"); oc.IHRC = true;oc.SqlExplicitGroupBy = "a.p31Rate_Internal_Orig";oc.VYSL = true;
            oc = AF("p31Amount_Internal", "Nákladový honorář", "a.p31Amount_Internal", "num", true); oc.IHRC = true; oc.VYSL = true;
            oc = AF("p51Name_CostRate", "Nákladový ceník", "p51costrate.p51Name", "string"); oc.IHRC = true; oc.RelSqlInCol = "LEFT OUTER JOIN p51PriceList p51costrate ON a.p51ID_CostRate=p51costrate.p51ID"; oc.VYSL = true;
            oc = AF("p31Rate_Overhead", "Režijní sazba", null, "num"); oc.IHRC = true; oc.VYSL = true; oc.SqlExplicitGroupBy = "a.p31Rate_Overhead";
            oc = AF("p31Amount_Overhead", "Režijní honorář", null, "num", true); oc.IHRC = true; oc.VYSL = true;
            oc = AF("HonorarSimulacniSazbaProjektu", "Sim.sazba projektu x Hodiny", "case when p34x.p33ID=1 then p41x.p41Plan_Internal_Rate*a.p31Hours_Orig end", "num", true); oc.IHRC = true; oc.VYSL = true;
            oc = AFNUM_OCAS("HonorarSimulacniSazbaUzivatele", "Sim.sazba uživatele x Hodiny", "p31_ocas.HonorarSimulacniSazbaUzivatele",true,"Simulační sazba uživatele x Hodiny"); oc.IHRC = true; oc.VYSL = true;
            

            this.CurrentFieldGroup = "Přepočet podle fixního kurzu";//-----------Přepočet podle fixního kurzu---------------------
            oc = AF("p31ExchangeRate_Fixed", "Fixní kurz", null, "num"); oc.IHRC = true;
            oc = AF("p31Amount_WithoutVat_FixedCurrency", "Vykázáno bez DPH FK", "a.p31ExchangeRate_Fixed*a.p31Amount_WithoutVat_Orig", "num", true); oc.IHRC = true;
            oc = AFNUM_OCAS("WIP_BezDph_FK", "Rozpracováno bez DPH FK", "a.p31ExchangeRate_Fixed*p31_ocas.WIP_BezDph", true); oc.IHRC = true;
            oc = AFNUM_OCAS("Nevyfakturovano_BezDph_FK", "Nevyúčtováno bez DPH FK", "a.p31ExchangeRate_Fixed*p31_ocas.Nevyfakturovano_BezDph", true); oc.IHRC = true;
            

            this.CurrentFieldGroup = "Výsledovka z vykázaných hodnot";//-----------Výsledovka z vykázaných hodnot---------------------
            oc = AFNUM_OCAS("Naklad1", "Náklad1", "p31_ocas.Naklad1", true,"Nákladová cena hodin+peněžní výdaje"); oc.IHRC = true; oc.VYSL = true;
            oc = AFNUM_OCAS("Naklad2", "Náklad2", "p31_ocas.Naklad2", true,"Nákladová cena hodin z režijní sazby+peněžní výdaje"); oc.IHRC = true; oc.VYSL = true;
            oc = AFNUM_OCAS("Naklad3", "Náklad3", "p31_ocas.Naklad3", true,"Nákladová cena podle simulační sazby projektu + peněžní výdaje"); oc.IHRC = true; oc.VYSL = true;
            oc = AFNUM_OCAS("Naklad4", "Náklad4", "p31_ocas.Naklad4", true,"Nákladová cena podle simulační sazby uživatele + peněžní výdaje"); oc.IHRC = true; oc.VYSL = true;
            oc = AFNUM_OCAS("Vynos1", "Výnos1", "p31_ocas.Vykazano_Vynos1", true, "Fa peněžní výdaje + Honorář z Fa hodin + Fa pevné odměny"); oc.IHRC = true; oc.VYSL = true;
            oc = AFNUM_OCAS("Vynos2", "Výnos2", "p31_ocas.Vykazano_Vynos2", true,"Honorář z Fa hodin + Fa pevné odměny"); oc.IHRC = true; oc.VYSL = true;
            oc = AFNUM_OCAS("Vynos3", "Výnos3", "p31_ocas.Vykazano_Vynos3", true,"Pouze Fa pevné odměny"); oc.IHRC = true; oc.VYSL = true;
            oc = AFNUM_OCAS("Vynos1MinusNaklad1", "V1-N1", "ISNULL(p31_ocas.Vykazano_Vynos1,0)-ISNULL(p31_ocas.Naklad1,0)", true); oc.IHRC = true; oc.VYSL = true;
            oc = AFNUM_OCAS("Vynos1MinusNaklad2", "V1-N2", "ISNULL(p31_ocas.Vykazano_Vynos1,0)-ISNULL(p31_ocas.Naklad2,0)", true); oc.IHRC = true; oc.VYSL = true;
            oc = AFNUM_OCAS("Vynos1MinusNaklad3", "V1-N3", "ISNULL(p31_ocas.Vykazano_Vynos1,0)-ISNULL(p31_ocas.Naklad3,0)", true); oc.IHRC = true; oc.VYSL = true;
            oc = AFNUM_OCAS("Vynos1MinusNaklad4", "V1-N4", "ISNULL(p31_ocas.Vykazano_Vynos1,0)-ISNULL(p31_ocas.Naklad4,0)", true); oc.IHRC = true; oc.VYSL = true;
            oc = AFNUM_OCAS("Vynos2MinusNaklad1", "V2-N1", "ISNULL(p31_ocas.Vykazano_Vynos2,0)-ISNULL(p31_ocas.Naklad1,0)", true); oc.IHRC = true; oc.VYSL = true;
            oc = AFNUM_OCAS("Vynos3MinusNaklad1", "V3-N1", "ISNULL(p31_ocas.Vykazano_Vynos3,0)-ISNULL(p31_ocas.Naklad1,0)", true); oc.IHRC = true; oc.VYSL = true;
            oc = AFNUM_OCAS("Vynos3MinusNaklad1", "V3-N3", "ISNULL(p31_ocas.Vykazano_Vynos3,0)-ISNULL(p31_ocas.Naklad3,0)", true); oc.IHRC = true; oc.VYSL = true;
            oc = AFNUM_OCAS("Vynos3MinusNaklad1", "V3-N4", "ISNULL(p31_ocas.Vykazano_Vynos3,0)-ISNULL(p31_ocas.Naklad4,0)", true); oc.IHRC = true; oc.VYSL = true;


            this.CurrentFieldGroup = "Výsledovka z vyúčtovaných hodnot";//-----------Výsledovka z vyúčtovaných hodnot---------------------
            oc = AFNUM_OCAS("Vyfakturovano_Puvodni_Naklad_Domestic", "Vykázaný náklad x Kurz", "p31_ocas.Vyfakturovano_Puvodni_Naklad_Domestic", true); oc.IHRC = true; oc.VYSL = true;
            oc = AFNUM_OCAS("Vyfakturovano_Vynos", "Vyúčtovaný výnos", "p31_ocas.Vyfakturovano_Vynos", true); oc.IHRC = true; oc.VYSL = true;
            oc = AFNUM_OCAS("Vyfakturovano_Vynos_Domestic", "Vyúčtovaný výnos x Kurz", "p31_ocas.Vyfakturovano_Vynos_Domestic", true); oc.IHRC = true; oc.VYSL = true;
            oc = AFNUM_OCAS("Vyfakturovano_Zisk", "Zisk po vyúčtování", "p31_ocas.Vyfakturovano_Zisk", true); oc.IHRC = true; oc.VYSL = true;
            oc = AFNUM_OCAS("Vyfakturovano_Zisk_Rezije", "Režijní zisk po vyúčtování", "p31_ocas.Vyfakturovano_Zisk_Rezije", true); oc.IHRC = true; oc.VYSL = true;

            this.CurrentFieldGroup = "Efektivní sazba z vyúčtovaných paušálů";
            oc = AF("p31AKDS_FPR_SAZBA", "Efektivní sazba", null, "num"); oc.IHRC = true; oc.VYSL = true;
            oc = AF("p31AKDS_FPR_PODIL", "Podíl % z PO", "a.p31AKDS_FPR_PODIL*100", "num",true); oc.IHRC = true; oc.VYSL = true;
            oc = AF("p31AKDS_FPR_BODY", "Podíl z PO", null, "num",true); oc.IHRC = true; oc.VYSL = true;
            oc = AF("p31AKDS_FPR_VZOR", "Vzorová sazba", null, "num"); oc.IHRC = true; oc.VYSL = true;

            this.CurrentFieldGroup = "Expense marže";//-----------Expense marže---------------------
            AF("p31MarginHidden", "Skrytá marže", null, "num").IHRC = true; oc.VYSL = true;
            AF("p31MarginTransparent", "Přiznaná marže%", null, "num").IHRC = true; oc.VYSL = true;
            AF("ExpenseAfterMarginHidden", "Výdaj po skryté marži", "a.p31Amount_WithoutVat_Orig+(a.p31Amount_WithoutVat_Orig*a.p31MarginHidden/100)", "num", true).IHRC = true; oc.VYSL = true;
            AF("ExpenseAfterAllMargins", "Výdaj po obou maržích", "dbo.p31_get_expense_with_margins(a.p31Amount_WithoutVat_Orig,a.p31MarginHidden,a.p31MarginTransparent)", "num", true).IHRC = true;
            AF("Odmena_Minus_Vydaj_Minus_HonorarR", "Odměna - Výdaj s marží - Režijní honorář", "(case when p34x.p33ID IN (2,5) and p34x.p34IncomeStatementFlag=2 then a.p31Amount_WithoutVat_Orig else 0 end) - (case when p34x.p33ID IN (2,5) and p34x.p34IncomeStatementFlag=1 then dbo.p31_get_expense_with_margins(a.p31Amount_WithoutVat_Orig,a.p31MarginHidden,a.p31MarginTransparent) else 0 end) - (case when p34x.p33ID IN (1,3) then a.p31Hours_Orig*a.p31Rate_Overhead else 0 end)", "num", true).IHRC = true; oc.VYSL = true;

            AppendTimestamp(true);
        }
    }
}
