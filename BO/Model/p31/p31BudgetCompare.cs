

namespace BO
{
    public class p31BudgetCompare
    {
        public string prefix { get; set; }
        public int pid { get; set; }
        public int j27ID_Max { get; set; }
        public int j27ID_Min { get; set; }
        public double Hodiny { get; set; }
        public double HodinyFa { get; set; }
        public double HodinyNefa { get; set; }
        public double Honorar { get; set; }
        public double Honorar_Naklad { get; set; }
        
        public double Vydaje { get; set; }
        public double VydajeFa { get; set; }
        public double VydajeNefa { get; set; }
        public double Odmeny { get; set; }

        public double Kusovnik { get; set; }   //počet úkonů vykázaných jako kusovník
        public double Kusovnik_Naklad { get; set; }
        public double Kusovnik_Honorar { get; set; }

        public double Vyuctovano_Hodiny { get; set; }
        public double Vyuctovano_Honorar { get; set; }
        public double Vyuctovano_BezDph { get; set; }
        public double Vyuctovano_Vydaje { get; set; }
        public double Vyuctovano_Odmeny { get; set; }
        public int Vyuctovano_Kusovnik { get; set; }   //počet úkonů vyúčtoaných jako kusovník
        public double Vyuctovano_Vydaje_Vykazane { get; set; }    //výdaje ve vyúčtování
        public double Vyuctovano_Hodiny_Vykazane { get; set; }    //hodin vykázané ve vyúčtování
        public double Vyuctovano_Honorar_Naklad { get; set; }
        public double Vyuctovano_Kusovnik_Naklad { get; set; }
        public double Vyuctovano_Hodiny_6 { get; set; }
        public double Vyuctovano_Hodiny_2 { get; set; }
        public double Vyuctovano_Hodiny_3 { get; set; }
        
    }
}
