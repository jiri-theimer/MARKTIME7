using System;

namespace BO
{
    public enum m62RateTypeENUM
    {
        InvoiceRate = 1,
        FixedRate = 2
    }
    public class m62ExchangeRate:BaseBO
    {
        public int x01ID { get; set; }
        public m62RateTypeENUM m62RateType { get; set; } = m62RateTypeENUM.InvoiceRate;
        public int j27ID_Master { get; set; }
        public int j27ID_Slave { get; set; }
        public DateTime m62Date { get; set; }
        public double m62Rate { get; set; }
        public int m62Units { get; set; }

        public string RateType
        {
            get
            {
                if (m62RateType == m62RateTypeENUM.InvoiceRate)
                    return "Fakturační kurz";
                else
                    return "Fixní kurz";
            }
        }        
        public string j27Code_Master { get; }                
        public string j27Code_Slave { get; }
       
        public string Veta
        {
            get
            {
                return $"{m62Units.ToString()} {this.j27Code_Slave} = {m62Rate.ToString()} {this.j27Code_Master}";
                
            }
        }
    }
}
