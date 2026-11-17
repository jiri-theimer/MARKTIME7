namespace BO
{
    //Rozpočet projektu: mřížka aktivita × měsíc nad položkami finančního plánu (p49FinancialPlan).

    //skutečnost: peněžní úkony projektu sečtené po aktivitě, měně a měsíci
    public class p49BudgetActual
    {
        public int p32ID { get; set; }
        public string p32Name { get; set; }
        public int p34ID { get; set; }
        public string p34Name { get; set; }
        public int p34Ordinary { get; set; }
        public int Strana { get; set; }     //1: výdaj, 2: příjem
        public int j27ID { get; set; }
        public string j27Code { get; set; }
        public int Rok { get; set; }
        public int Mesic { get; set; }
        public double Amount { get; set; }
    }

    //aktivita, na kterou lze rozpočtovat (sešity mimo časové)
    public class p49BudgetActivity
    {
        public int p32ID { get; set; }
        public string p32Name { get; set; }
        public int p34ID { get; set; }
        public string p34Name { get; set; }
        public int p34Ordinary { get; set; }
        public int p32Ordinary { get; set; }
        public int Strana { get; set; }
    }

    //přehled rozpočtů: jeden řádek za projekt a měnu
    public class p49BudgetProject
    {
        public int p41ID { get; set; }
        public string Project { get; set; }
        public string p41Code { get; set; }
        public string Client { get; set; }
        public string j27Code { get; set; }
        public double PlanVydaje { get; set; }
        public double PlanPrijmy { get; set; }
        public double SkutVydaje { get; set; }
        public double SkutPrijmy { get; set; }
    }
}
