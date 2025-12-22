namespace BO.Integrace
{
    public class InputInvoiceRow
    {
        public string Oddil { get; set; }
        public double BezDPH { get; set; }
        public double DPHSazba { get; set; }
        public int x15ID { get; set; }  //1: 0%, 2: snížená sazba, 3: základní sazba, 4: speciální sazba
        public double DPH { get; set; }
        public double VcDPH { get; set; }
        public string j27Code { get; set;}
        public int Poradi { get; set; }
        public int RowPID { get; set; }

        public int p31ID { get; set; }
        public int p95ID { get; set; }
        public double BezDPH_Domestic { get; set; }
        public double DPH_Domestic { get; set; }
        public double VcDPH_Domestic { get; set; }        
        public string p31Code { get; set; }

        public string PredkontaceIS { get; set; }
        public string CinnostIS { get; set; }
    }
}
