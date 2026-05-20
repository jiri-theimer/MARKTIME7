
namespace BO
{
    public class p78UpominkaSdruzena:BaseBO
    {
        public int p28ID { get; set; }
        public int j02ID_Owner { get; set; }
        public string p78Code { get; set; }
        public string p78Name { get; set; }
        public string p78TextA { get; set; }
        public string p78TextB { get; set; }
        public DateTime p78Date { get; set; }

        public string p78VariableSymbol { get; set; }
        public string p78Client { get; set; }
        public string p78Client_RegID { get; set; }
        public string p78Client_VatID { get; set; }
        public string p78Client_ICDPH_SK { get; set; }
        public string p78ClientAddress1_Street { get; set; }
        public string p78ClientAddress1_City { get; set; }
        public string p78ClientAddress1_ZIP { get; set; }
        public string p78ClientAddress1_Country { get; set; }
        public string p78ClientAddress1_Before { get; set; }

        public double p78Amount_Debt { get; set; }
        public double p78Amount_Debt_KratKurz { get; set; }
        public int j27ID { get; set; }
        
        public string p28Name { get; }
        public string Owner { get; }
    }
}
