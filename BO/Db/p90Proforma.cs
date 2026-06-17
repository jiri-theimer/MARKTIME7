using System;

namespace BO
{
    public class p90Proforma:BaseBO
    {
        public int j27ID { get; set; }
        public int p89ID { get; set; }
        public int p28ID { get; set; }
        public int j19ID { get; set; }
        public int j02ID_Owner { get; set; }
        
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
        public string p90TextDPP { get; set; }
        public string TagsInlineHtml { get; set; }
        public int p90BitStream { get; set; }
        
        public Guid p90Guid { get; set; }
        public int x38ID { get; }
        public int x31ID { get; }
        public int x31ID_Payment { get; }
        public string j27Code { get; }
        

        public string p28Name { get; }
        public string p28RegID { get; }
        public string p28VatID { get; }


        public string p89Name { get; }
        

        public string Owner { get; }
       
        public string CodeWithClient
        {
            get
            {
                return this.p90Code + " - " + this.p28Name;
            }
        }

        public string FullName
        {
            get
            {
                return this.p89Name + ": " + this.p90Code + " (" + this.p28Name + ") " + BO.Code.Bas.GN(this.p90Amount) + " " + this.j27Code;
            }
        }
    }
}
