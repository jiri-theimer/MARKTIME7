

namespace BO.Integrace
{
    public class InputZaloha
    {
        public int pid { get; set; }
        public int p90ID { get; set; }
        public int j27ID { get; set; }
        public int j27ID_Domestic { get; set; }
        public string j27Code_Domestic { get; set; }
        public int p89ID { get; set; }
        public int p28ID { get; set; }
        public int j19ID { get; set; }
        public int j02ID_Owner { get; set; }
        public int x15ID { get; set; }
        public string p90Code { get; set; }
        public bool p90IsDraft { get; set; }
        public DateTime p90Date { get; set; }
        public DateTime? p90DateBilled { get; set; }
        public DateTime? p90DateMaturity { get; set; }
        public double p90Amount_WithoutVat { get; set; }
        public double p90Amount_Vat { get; set; }
        public double p90Amount_Billed { get; set; }
        public double p90VatRate { get; set; }
        public double p90Amount { get; set; }
        public double p90Amount_Debt { get; set; }
        public string p90Text1 { get; set; }
        public string p90Text2 { get; set; }

        public Guid p90Guid { get; set; }


        public string j27Code { get; set; }


        public string p28Name { get; set; }
        public string p28RegID { get; set; }
        public string p28VatID { get; set; }
        public string p28ICDPH_SK { get; set; }
        public string p28Street1 { get; set; }
        public string p28City1 { get; set; }
        public string p28PostCode1 { get; set; }
        public string p28Country1 { get; set; }


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
    }
}
