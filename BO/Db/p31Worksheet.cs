

namespace BO
{
    public enum p31HoursEntryFlagENUM
    {
        Hodiny = 1,
        Minuty = 2,
        NeniCas = 0
    }



    public class p31Worksheet : BaseBO
    {
        public int p31ExcludeBillingFlag { get; set; }  //1: vyloučeno z vyúčtování
        public bool isdeleted { get; }  //true: záznam se nachází v archivu p31worksheet_del
        public int p41ID { get; set; }
        public string p41Name { get; }
        public string p41NameShort { get; }
        public string p41Code { get; }
        public BO.p41BillingFlagEnum p41BillingFlag { get; }
        public string p42Name { get; }
        public int p42ID { get; }
        public int p28ID_Client { get; }
        public string ClientName { get; }

        public int j02ID { get; set; }
        public string j02FirstName { get; }
        public string j02LastName { get; }
        public string Person
        {
            get
            {
                return $"{j02LastName} {j02FirstName}";
            }
        }

        public int p32ID { get; set; }
        public int p28ID_Supplier { get; set; }
        public int p28ID_ContactPerson { get; set; }    //Kontaktní osoba úkonu


        public int p56ID { get; set; }
        public string p56Name { get; }
        public string p56Code { get; }
        public double Nevyuctovano_Hodiny { get; }
        public double Nevyuctovano_Castka { get; }
        public double Schvaleno_Hodiny_Pozdeji { get; }
        public string p32Name { get; }
        public bool p32IsBillable { get; }
        public string p32Color { get; }
       
        public int p32AbsenceFlag { get; }
        public int p32AbsenceBreakFlag { get; }
        public int p34ID { get; }
        public BO.p33IdENUM p33ID { get; }
        public string p34Name { get; }
        public p34IncomeStatementFlagENUM p34IncomeStatementFlag { get; set; }
        public int p95ID { get; }
        public string p95Name { get; }
        public int o23ID { get; set; }
        public int p54ID { get; set; }   //stupeň přesčasu
        public int p40ID_Source { get; set; }   //zdroj opakované odměny, která vygenerovala úkon
        public int p40ID_FixPrice { get; set; }   //vazba na paušální odměnu        
        public string p40Name { get; }  //vazba na paušální odměnu
        public int p91ID { get; set; }
        public string p91Code { get; }
        public bool p91IsDraft { get; }

        public int j02ID_Owner { get; set; }
       
        public p70IdENUM p70ID { get; set; }
        public string p70Name { get; }

        public p71IdENUM p71ID { get; set; }
        public string p71Name { get; }

        public p72IdENUM p72ID_AfterApprove { get; set; }
        public string approve_p72Name { get; }
        public p72IdENUM p72ID_AfterTrimming { get; set; }
        
        public DateTime p31Date { get; set; }

        public DateTime? p31DateTimeFrom_Orig { get; set; }
        public DateTime? p31DateTimeUntil_Orig { get; set; }
        public string p31Text { get; set; }
        public string p31TextInternal { get; set; }



        public int p31BitStream { get; set; }        //Bitstream pro nastavení funkcí [Rozšížení]
        public string p31Code { get; set; }
        public int p31Ordinary { get; set; }
        public double p31Value_Orig { get; set; }
        public double p31Value_Trimmed { get; set; }
        public double p31Value_Approved_Billing { get; set; }
        public double p31Value_Approved_Internal { get; set; }
        public double p31Value_Invoiced { get; set; }

        public int p31Minutes_Orig { get; set; }
        public int p31Minutes_Trimmed { get; set; }
        public int p31Minutes_Approved_Billing { get; set; }
        public int p31Minutes_Approved_Internal { get; set; }
        public int p31Minutes_Invoiced { get; set; }

        public double p31Hours_Orig { get; set; }
        public double p31Hours_Trimmed { get; set; }
        public double p31Hours_Approved_Billing { get; set; }
        public double p31Hours_Approved_Internal { get; set; }
        public double p31Hours_Invoiced { get; set; }

        public string p31HHMM_Orig { get; }
        public string p31HHMM_Trimmed { get; }
        public string p31HHMM_Approved_Billing { get; }
        public string p31HHMM_Approved_Internal { get; }
        public string p31HHMM_Invoiced { get; }

        public int p51ID_BillingRate { get; }
        public int p51ID_CostRate { get; }
        public double p31Rate_Billing_Orig { get; set; }
        public double p31Rate_Internal_Orig { get; set; }
        public double p31Amount_Internal { get; set; }
        public double p31Rate_Billing_Approved { get; set; }
        public double p31Rate_Internal_Approved { get; set; }
        public double p31Rate_Billing_Invoiced { get; set; }

        public double p31Amount_WithoutVat_Orig { get; set; }
        public double p31Amount_WithVat_Orig { get; set; }
        public double p31Amount_Vat_Orig { get; set; }
        public double p31VatRate_Orig { get; set; }
        public int j27ID_Billing_Orig { get; set; }
        public int j27ID_Internal { get; set; }

        public double p31Amount_WithoutVat_Approved { get; set; }
        public double p31Amount_WithVat_Approved { get; set; }
        public double p31Amount_Vat_Approved { get; set; }
        public double p31VatRate_Approved { get; set; }

        public double p31Amount_WithoutVat_Invoiced { get; set; }
        public double p31Amount_WithVat_Invoiced { get; set; }
        public double p31VatRate_Invoiced { get; set; }
        public double p31Amount_Vat_Invoiced { get; set; }
        public double p31ExchangeRate_Invoice { get; set; }
        public int j27ID_Billing_Invoiced { get; set; }

        public double p31Amount_WithoutVat_Invoiced_Domestic { get; set; }
        public double p31Amount_WithVat_Invoiced_Domestic { get; set; }
        public double p31Amount_Vat_Invoiced_Domestic { get; set; }
        public double p31ExchangeRate_Domestic { get; set; }
        public int j27ID_Billing_Invoiced_Domestic { get; set; }

        public double p31Amount_WithoutVat_FixedCurrency { get; set; }


        public string p31ExternalCode { get; set; }

        public int j02ID_ApprovedBy { get; set; }


        public string p31Value_Orig_Entried { get; set; }

        public p31HoursEntryFlagENUM p31HoursEntryFlag { get; set; }
        public DateTime? p31Approved_When { get; set; }


        public string j27Code_Billing_Orig { get; set; }

        public double p31Calc_Pieces { get; set; }
        public double p31Calc_PieceAmount { get; set; }
        public int p35ID { get; set; }
        public int j19ID { get; set; }
        public int p31MasterID { get; set; }
        public int p31ApprovingLevel { get; set; }

        public double p31Value_FixPrice { get; set; }
        public bool p31IsInvoiceManual { get; set; }
        public string TagsInlineHtml { get; }
        public double p31MarginHidden { get; set; }   // nové pole verze 6
        public double p31MarginTransparent { get; set; }  // nové pole verze 6
        //public double BillingRateOrig { get; set; }
        public string p31PostRecipient { get; set; }
        public string p31PostCode { get; set; }
        public int p31PostFlag { get; set; }
        public DateTime? p31TimerTimestamp { get; set; }  // čas posledního zapnutí stopek
        public double p31Value_Off { get; set; }
        
        public int p32ManualFeeFlag { get; }
        public int b05ID_Last { get; set; }
        public int p49ID { get; set; }  //překlopený plán
        public string TimeFrom
        {
            get
            {
                if (this.p31DateTimeFrom_Orig == null)
                {
                    return null;
                }
                else
                {
                    return BO.Code.Bas.ObjectDateTime2String(this.p31DateTimeFrom_Orig, "HH:mm");
                }


            }
        }
        public string TimeUntil
        {
            get
            {
                if (this.p31DateTimeUntil_Orig == null)
                    return null;
                else
                    return BO.Code.Bas.ObjectDateTime2String(this.p31DateTimeUntil_Orig, "HH:mm");

            }
        }





        public bool IsRecommendedHHMM()
        {
            if (this.p33ID == p33IdENUM.Cas)
            {
                if (this.p31Value_Orig_Entried != "")
                {
                    if (this.p31Value_Orig_Entried != null && this.p31Value_Orig_Entried.IndexOf(":") > 0)
                        return true; // původně zadaná hodnota obsahuje rovnou hodnota v HH:MM
                }
                if (this.p31Value_Orig.ToString().Length > 5)
                    return true; // desetinné číslo s velkým počtem desetinných míst
            }

            return false;
        }
        public bool IsRecommendedHHMM_Invoiced()
        {
            if (this.p33ID == p33IdENUM.Cas)
            {
                if (this.p31Hours_Invoiced.ToString().Length > 5)
                    return true; // desetinné číslo s velkým počtem desetinných míst
            }

            return false;
        }
        public bool IsRecommendedHHMM_Approved()
        {
            if (this.p33ID == p33IdENUM.Cas)
            {
                if (this.p31Hours_Approved_Billing.ToString().Length > 5)
                    return true; // desetinné číslo s velkým počtem desetinných míst
            }

            return false;
        }
        public bool IsRecommendedHHMM_Trimmed()
        {
            if (this.p33ID == p33IdENUM.Cas)
            {
                if (this.p31Value_Trimmed.ToString().Length > 5)
                    return true; // desetinné číslo s velkým počtem desetinných míst
            }

            return false;
        }

        public string tabquery()
        {
            if (this.p33ID == BO.p33IdENUM.Cas) return "time";
            if (this.p33ID == BO.p33IdENUM.PenizeVcDPHRozpisu || this.p33ID == BO.p33IdENUM.PenizeBezDPH)
            {
                if (this.p34IncomeStatementFlag == p34IncomeStatementFlagENUM.Vydaj)
                {
                    return "expense";
                }
                else
                {
                    return "fee";
                }
            }
            return "kusovnik";
        }

        public string p41NamePlusCode
        {
            get
            {
                return $"{this.p41Name} ({this.p41Code})";
            }
        }
        public string Project
        {
            get
            {
                if (this.p28ID_Client == 0)
                {
                    if (this.p41NameShort != null)
                    {
                        return this.p41NameShort;
                    }
                    else
                    {
                        return this.p41Name;
                    }
                }
                else
                {
                    if (this.p41NameShort != null)
                    {
                        return $"{this.ClientName} - {this.p41NameShort} ({this.p41Code})";
                        
                    }
                    else
                    {
                        return $"{this.ClientName} - {this.p41Name} ({this.p41Code})";
                        
                    }
                }

            }
        }

    }
}
