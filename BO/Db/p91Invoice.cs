using System;

namespace BO
{
    public enum p91PortalFlagEnum
    {
        None=0,
        Published=1
    }
    public class p91Invoice:BaseBO
    {
        public int j27ID { get; set; }
        public int p92ID { get; set; }
        public int p28ID { get; set; }
        public int p41ID_First { get; set; }
        public int j19ID { get; set; }
        
        public int j02ID_Owner { get; set; }
        //public int p28ID_ContactPerson { get; set; }
        public int p91ID_CreditNoteBind { get; set; }
        
        public int b02ID { get; set; }
        public int p98ID { get; set; }
        public int p63ID { get; set; }
        public int p80ID { get; set; }

        public string p91Code { get; set; }
        public Guid p91Guid { get; set; }
        public bool p91IsDraft { get; set; }
        public DateTime p91Date { get; set; }
        public DateTime? p91DateBilled { get; set; }
        public DateTime p91DateMaturity { get; set; }
        public DateTime p91DateSupply { get; set; }
        public DateTime? p91DateExchange { get; set; }

        public double p91ExchangeRate { get; set; }
        public DateTime? p91Datep31_From { get; set; }
        public DateTime? p91Datep31_Until { get; set; }



        public double p91Amount_WithoutVat { get; set; }

        public double p91Amount_Vat { get; set; }
        public double p91Amount_Billed { get; set; }
        public double p91Amount_WithVat { get; set; }
        public double p91Amount_Debt { get; set; }
        public double p91ProformaAmount { get; set; }
        public double p91ProformaBilledAmount { get; set; }
        public double p91Amount_WithoutVat_None { get; set; }

        public double p91VatRate_Low { get; set; }
        public double p91Amount_WithVat_Low { get; set; }
        public double p91Amount_WithoutVat_Low { get; set; }
        public double p91Amount_Vat_Low { get; set; }

        public double p91VatRate_Standard { get; set; }
        public double p91Amount_WithVat_Standard { get; set; }
        public double p91Amount_WithoutVat_Standard { get; set; }
        public double p91Amount_Vat_Standard { get; set; }

        public double p91VatRate_Special { get; set; }
        public double p91Amount_WithVat_Special { get; set; }
        public double p91Amount_WithoutVat_Special { get; set; }
        public double p91Amount_Vat_Special { get; set; }

        public double p91Amount_TotalDue { get; set; }
        public double p91RoundFitAmount { get; set; }
        public double p91FixedVatRate { get; set; }
        public int x15ID { get; set; }

        public string p91Text1 { get; set; }
        public string p91Text2 { get; set; }

        public string p91Client { get; set; }        
        public string p91Client_RegID { get; set; }
        public string p91Client_VatID { get; set; }
        public string p91ClientAddress1_Street { get; set; }
        public string p91ClientAddress1_City { get; set; }
        public string p91ClientAddress1_ZIP { get; set; }
        public string p91ClientAddress1_Country { get; set; }
        public string p91ClientAddress1_Before { get; set; }
        public string p91ClientAddress2 { get; set; }

        public string p91Supplier { get; set; }
        public string p91Supplier_RegID { get; set; }
        public string p91Supplier_VatID { get; set; }
        public string p91Supplier_Street { get; set; }
        public string p91Supplier_City { get; set; }
        public string p91Supplier_ZIP { get; set; }
        public string p91Supplier_Country { get; set; }
        public string p91Supplier_Registration { get; set; }
        public string p91Supplier_ICDPH_SK { get; set; }
        public p91PortalFlagEnum p91PortalFlag { get; set; }
        public int p91LockFlag { get; set; }  // bitová sekvence: 2 - zámek ceny faktury, 4 - zámek úkonů s nulovou cenou, 8 - úprava karty faktury pouze pro admina
        public int p91BitStream { get; set; }
        public string p91Client_ICDPH_SK { get; set; }
        
        public string p91QrCode { get; set; }
        
        public int b01ID { get; }
        public int p84ID_Last { get; set; }
        public string p91VatCodePohoda { get; set; }

        public string j27Code { get; }
        
        public string b02Name { get; }
     
        public int p93ID { get; }
        public int p83ID { get; }
       public int x38ID { get; }

        public string p28Name { get; }
       public string p28CountryCode { get; }
        public string p28CompanyName { get; }
        
        public string CodeAndAmount
        {
            get
            {
                return this.p91Code + " [" + BO.Code.Bas.ObjectDate2String(this.p91DateSupply) + "] " + BO.Code.Bas.GN(this.p91Amount_WithoutVat) + " " + this.j27Code;
            }
        }

        public string p92Name { get; }
        
        public BO.p92TypeFlagENUM p92TypeFlag { get; }
        
        
        
        public string Owner { get; }
        
        public string p41Name { get; }
        
       
        public double WithoutVat_Krat_Kurz
        {
            get
            {
                return this.p91Amount_WithoutVat * this.p91ExchangeRate;
            }
        }
        public double Debt_Krat_Kurz
        {
            get
            {
                return this.p91Amount_Debt * this.p91ExchangeRate;
            }
        }
        public double p91Amount_TotalDue_Krat_Kurz
        {
            get
            {
                return this.p91Amount_TotalDue * this.p91ExchangeRate;
            }
        }

        public string PrimaryAddress
        {
            get
            {
                string s = this.p91ClientAddress1_Street;
                if (s == null)
                    s = this.p91ClientAddress1_City;
                else
                    s += ", " + this.p91ClientAddress1_City;
                if (this.p91ClientAddress1_ZIP != null)
                    s += ", " + this.p91ClientAddress1_ZIP;
                if (this.p91ClientAddress1_Country != null)
                    s += ", " + this.p91ClientAddress1_Country;
                return s;
            }
        }
    }
}
