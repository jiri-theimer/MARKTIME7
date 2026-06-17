namespace BO.Integrace
{
    public class InputInvoice
    {
        public int p92TypeFlag { get; set; }
        public int pid { get; set; }
        public int p91ID { get; set; }
        public List<InputInvoiceRow> InvoiceRows { get; set; }
        public string Project { get; set; }
        public string p41Code { get; set; }
        public string p91Guid { get; set; }
        public string j27Code_Invoice { get; set; }
        public string j27Code_Domestic { get; set; }
        public int j27ID_Domestic { get; set; }
        public int j27ID { get; set; }
        public int p92ID { get; set; }
        public int p28ID { get; set; }
        public string p28CountryCode { get; set; }
        public int p41ID_First { get; set; }
        public int j19ID { get; set; }
        
        public int j02ID_Owner { get; set; }
        public int p28ID_ContactPerson { get; set; }
        public int p91ID_CreditNoteBind { get; set; }

        public int b02ID { get; set; }
        public int p98ID { get; set; }
        public int p63ID { get; set; }
        public int p80ID { get; set; }
        
        public string p91Code { get; set; }
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
        public string p91ClientPerson { get; set; }
        public string p91ClientPerson_Salutation { get; set; }
        public string p91Client_RegID { get; set; }
        public string p91Client_VatID { get; set; }
        public string p91ClientAddress1_Street { get; set; }
        public string p91ClientAddress1_City { get; set; }
        public string p91ClientAddress1_ZIP { get; set; }
        public string p91ClientAddress1_Country { get; set; }
        public string p91ClientAddress2 { get; set; }

       
        public string p91Client_ICDPH_SK { get; set; }
        

       

        public string j27Code { get; set; }
        

        public int p93ID { get; set; }
        public string p93RegID { get; set; }
        public string p93Company { get; set; }
        public string p93Street { get; set; }
        public string p93City { get; set; }
        public string p93Zip { get; set; }
        public string p93Country { get; set; }
        public string p93CountryCode { get; set; }
        public string p93VatID { get; set; }
        public string p93Contact { get; set; }
        public string p93Referent { get; set; }
        public string p93ICDPH_SK { get; set; }
        public string p93Email { get; set; }

       public string p86Account { get; set; }
        public string p86Code { get; set; }
        public string p86BankName { get; set; }
        public string p86IBAN { get; set; }
        public string p86SWIFT { get; set; }


        public string PredkontaceIS { get; set; }
        public string KlasifikaceDphIS { get; set; }

        public int j18ID { get; set; }
        public string j18Code { get; set; }


        
        public string Implementace { get; set; }
        public string ZchPartner { get; set; }

    }
}
